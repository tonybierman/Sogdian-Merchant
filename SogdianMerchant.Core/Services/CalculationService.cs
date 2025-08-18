// CalculationService.cs (Improved)
namespace SogdianMerchant.Core.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly IRandomGenerator _rand;

        public CalculationService(IRandomGenerator rand)
        {
            _rand = rand;
        }

        private double NextGaussian(double mean, double stdDev)
        {
            double u1;
            do
            {
                u1 = _rand.NextDouble();
            } while (u1 == 0.0);
            double u2 = _rand.NextDouble();
            double normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * normal;
        }

        private Market[] GetDynamicMarkets()
        {
            double profitStd = GameState.MarketVariationProfit / Math.Sqrt(3.0);

            return new[]
            {
                new Market(GameState.BaghdadMarket,
                    GameState.BaseDistanceBaghdad * (1 + _rand.NextDouble(-GameState.MarketVariationDistance, GameState.MarketVariationDistance)),
                    GameState.BaseRiskBaghdad * (1 + _rand.NextDouble(-GameState.MarketVariationRisk, GameState.MarketVariationRisk)),
                    Math.Max(0.0, GameState.BaseProfitBaghdad * NextGaussian(1.0, profitStd))),
                new Market(GameState.KashgarMarket,
                    GameState.BaseDistanceKashgar * (1 + _rand.NextDouble(-GameState.MarketVariationDistance, GameState.MarketVariationDistance)),
                    GameState.BaseRiskKashgar * (1 + _rand.NextDouble(-GameState.MarketVariationRisk, GameState.MarketVariationRisk)),
                    Math.Max(0.0, GameState.BaseProfitKashgar * NextGaussian(1.0, profitStd))),
                new Market(GameState.KarachiMarket,
                    GameState.BaseDistanceKarachi * (1 + _rand.NextDouble(-GameState.MarketVariationDistance, GameState.MarketVariationDistance)),
                    GameState.BaseRiskKarachi * (1 + _rand.NextDouble(-GameState.MarketVariationRisk, GameState.MarketVariationRisk)),
                    Math.Max(0.0, GameState.BaseProfitKarachi * NextGaussian(1.0, profitStd)))
            };
        }

        public double GetTravelRate(string guide)
        {
            double baseRate = guide == GameState.GuideNovice ? GameState.TravelRateNovice : guide == GameState.GuideVeteran ? GameState.TravelRateVeteran : GameState.TravelRateNone;
            double modifier = _rand.NextDouble() < GameState.TravelModifierProb ? _rand.NextDouble(GameState.TravelModifierMin, GameState.TravelModifierMax) : 1.0;
            if (guide != GameState.GuideNone && _rand.NextDouble() < GameState.GuidePenaltyProb) modifier *= GameState.GuidePenaltyMult;
            return baseRate * modifier;
        }

        public double GetExpectedTravelRate(string guide)
        {
            return guide == GameState.GuideNovice ? GameState.TravelRateNovice : guide == GameState.GuideVeteran ? GameState.TravelRateVeteran : GameState.TravelRateNone;
        }

        public double GetGuideCost(string guide)
        {
            return guide == GameState.GuideNovice ? GameState.GuideCostNovice : guide == GameState.GuideVeteran ? GameState.GuideCostVeteran : GameState.GuideCostNone;
        }

        // TODO: Inform user about reason for negative profits
        public double CalculateProfit(string market, int guards, string guide, double caravanValue, double camelQuality)
        {
            // Retrieve dynamic markets data
            var markets = GetDynamicMarkets();

            // Find the selected market by name
            var selectedMarket = markets.FirstOrDefault(m => m.Name == market);

            // If market not found, return 0 profit (neutral, not negative)
            if (selectedMarket == null) return 0.0;

            // Get distance for travel time calculation
            double distance = selectedMarket.Distance;

            // Get base risk level for the market
            double baseRisk = selectedMarket.BaseRisk;

            // Calculate base profit adjusted by camel quality and multiplier
            double profit = selectedMarket.Profit * camelQuality * GameState.ProfitCamelMultiplier; // This could be low if camelQuality is poor, leading to potential negative net profit
                                                                                                    
            // Get travel rate based on guide
            double travelRate = GetTravelRate(guide);

            // Determine guide risk factor based on travel rate thresholds
            double guideRiskFactor = travelRate >= GameState.VeteranTravelThreshold ? GameState.GuideRiskFactorVeteran : travelRate >= GameState.NoviceTravelThreshold ? GameState.GuideRiskFactorNovice : GameState.GuideRiskFactorNone;
            
            // Adjust base risk by guide factor
            baseRisk *= guideRiskFactor; // Higher factor increases risk, raising chance of loss event

            // Add risk penalty for poor camel quality
            baseRisk += (1 - camelQuality) * GameState.CamelRiskPenalty; // Low camelQuality increases risk, raising chance of loss event
                                                                         
            // Random guard effectiveness per guard
            double guardEffect = _rand.NextDouble(GameState.GuardEffectMin, GameState.GuardEffectMax);

            // Reduce risk by guards' effect, clamp to 0 minimum
            double adjustedRisk = Math.Max(0, baseRisk - (guards * guardEffect)); // Few guards or low effect may leave high adjustedRisk, increasing loss chance
                                                                                  
            // Effective travel rate adjusted by camel quality
            double effectiveTravelRate = travelRate * camelQuality; // Low camelQuality slows travel, increasing time and costs
                                                                    
            // Calculate travel time
            double travelTime = distance / (GameState.BaseTravelSpeed * effectiveTravelRate); // Longer time increases illiquidityCost
                                                                                              
            // Random delay if poor camels
            if (_rand.NextDouble() < GameState.TravelDelayProb && camelQuality < GameState.CamelDelayThreshold) travelTime *= GameState.TravelDelayMultiplier; // Delay multiplies time, raising illiquidityCost
                                                                                                                                                               
            // Cost due to travel time and guards
            double illiquidityCost = travelTime * (guards * GameState.IlliquidityCostPerDayPerGuard); // High guards or long travelTime increases this cost, reducing net profit
                                                                                                      
            // Get fixed guide cost
            double guideCost = GetGuideCost(guide); // Adds to total costs, reducing net profit
                                                    
            // Check for loss event based on adjusted risk
            if (_rand.NextDouble() < adjustedRisk)
            {
                // Loss: full caravan value plus costs (always negative if caravanValue > 0)
                return -caravanValue - illiquidityCost - guideCost;
            }

            // Success: profit minus costs (negative if profit < illiquidityCost + guideCost)
            return profit - illiquidityCost - guideCost;
        }

        public (string Market, double Score, string Reasoning) ChooseBestMarket(double caravanValue, double riskTolerance, int guards, string guide, string[] unavailableMarkets, double camelQuality)
        {
            // Default to no action market if no better found
            string bestMarket = GameState.DoNothingMarket; // Neutral choice, implies 0 profit
                                                           
            // Initialize best score at 0; negative scores won't be selected
            double bestScore = 0.0;

            // Initial reasoning
            string reasoning = "No market selected yet.";

            // Precompute daily illiquidity cost based on guards
            double illiquidityCostPerDay = guards * GameState.IlliquidityCostPerDayPerGuard; // More guards increase daily costs, contributing to potential negative net
                                                                                             
            // Retrieve dynamic markets
            var markets = GetDynamicMarkets();

            // Get expected travel rate for guide
            double expectedTravelRate = GetExpectedTravelRate(guide);

            // Determine guide risk factor
            double guideRiskFactor = expectedTravelRate >= GameState.VeteranTravelThreshold ? GameState.GuideRiskFactorVeteran : expectedTravelRate >= GameState.NoviceTravelThreshold ? GameState.GuideRiskFactorNovice : GameState.GuideRiskFactorNone; // Poor guide increases risk factor, raising loss probability
                                                                                                                                                                                                                                                          // Get fixed guide cost
            double guideCost = GetGuideCost(guide); // Adds to total costs, reducing net profit
                                                    
            // Evaluate each market
            foreach (var market in markets)
            {
                // Get market name
                string name = market.Name;

                // Skip unavailable markets
                if (unavailableMarkets.Contains(name)) continue;

                // Get distance
                double distance = market.Distance;

                // Adjust base risk by guide factor
                double baseRisk = market.BaseRisk * guideRiskFactor; // High factor increases risk, raising loss chance
                                                                     
                // Add camel risk penalty
                baseRisk += (1 - camelQuality) * GameState.CamelRiskPenalty; // Low camelQuality increases risk, raising loss chance
                                                                             
                // Calculate adjusted profit
                double profit = market.Profit * camelQuality * GameState.ProfitCamelMultiplier; // Low camelQuality reduces profit, potentially below costs
                                                                                                
                // Random guard effect
                double guardEffect = _rand.NextDouble(GameState.GuardEffectMin, GameState.GuardEffectMax);

                // Adjust risk with guards
                double adjustedRisk = Math.Max(0, baseRisk - (guards * guardEffect)); // Low guards/effect leaves high risk, increasing expected loss
                                                                                      
                // Effective travel rate
                double effectiveTravelRate = expectedTravelRate * camelQuality; // Low camelQuality slows travel, increasing time/costs
                                                                                
                // Calculate travel time
                double travelTime = distance / (GameState.BaseTravelSpeed * effectiveTravelRate); // Longer time increases illiquidityCost
                                                                                                  
                // Total illiquidity cost
                double illiquidityCost = travelTime * illiquidityCostPerDay; // High time/guards raise cost, potentially exceeding profit
                                                                             
                // Skip if certain costs exceed profit (guaranteed negative without risk)
                if (illiquidityCost + guideCost > profit) continue;

                // Expected value: weighted profit/loss minus costs
                double expectedValue = (1 - adjustedRisk) * profit + adjustedRisk * (-caravanValue) - illiquidityCost - guideCost; // Negative if risk high (loss term dominates) or costs > profit
                                                                                                                                   
                // Score adjusts EV by distance and risk vs tolerance
                double score = expectedValue / distance * (1 - adjustedRisk / riskTolerance); // Negative EV or risk > tolerance makes score negative, avoiding selection
                                                                                              
                // Build per-market reasoning
                string marketReasoning = $"Evaluated {name}: Profit = {profit:F2}, Risk = {adjustedRisk:F2}, Illiquidity = {illiquidityCost:F2}, Guide Cost = {guideCost:F2}, Travel = {travelTime:F2} days, Score = {score:F2}";
                
                // Update if better score
                if (score > bestScore)
                {
                    // New best score
                    bestScore = score;

                    // Update market
                    bestMarket = name;

                    // Update reasoning
                    reasoning = marketReasoning + $"\nSelected {name} for highest score.";
                }
            }

            // Check if no market selected
            if (bestMarket == GameState.DoNothingMarket)
            {
                // Final reasoning for no selection
                reasoning = "No market selected: All markets unavailable or unprofitable."; // Implies all had negative/zero scores or skipped (costs > profit)
            }

            // Return results
            return (bestMarket, bestScore, reasoning);
        }
    }
}