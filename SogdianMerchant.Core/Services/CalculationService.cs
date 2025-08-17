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

        public double CalculateProfit(string market, int guards, string guide, double caravanValue, double camelQuality)
        {
            var markets = GetDynamicMarkets();
            var selectedMarket = markets.FirstOrDefault(m => m.Name == market);
            if (selectedMarket == null) return 0.0;

            double distance = selectedMarket.Distance;
            double baseRisk = selectedMarket.BaseRisk;
            double profit = selectedMarket.Profit * camelQuality * GameState.ProfitCamelMultiplier;

            double travelRate = GetTravelRate(guide);
            double guideRiskFactor = travelRate >= GameState.VeteranTravelThreshold ? GameState.GuideRiskFactorVeteran : travelRate >= GameState.NoviceTravelThreshold ? GameState.GuideRiskFactorNovice : GameState.GuideRiskFactorNone;
            baseRisk *= guideRiskFactor;
            baseRisk += (1 - camelQuality) * GameState.CamelRiskPenalty;

            double guardEffect = _rand.NextDouble(GameState.GuardEffectMin, GameState.GuardEffectMax);
            double adjustedRisk = Math.Max(0, baseRisk - (guards * guardEffect));
            double effectiveTravelRate = travelRate * camelQuality;
            double travelTime = distance / (GameState.BaseTravelSpeed * effectiveTravelRate);
            if (_rand.NextDouble() < GameState.TravelDelayProb && camelQuality < GameState.CamelDelayThreshold) travelTime *= GameState.TravelDelayMultiplier;
            double illiquidityCost = travelTime * (guards * GameState.IlliquidityCostPerDayPerGuard);
            double guideCost = GetGuideCost(guide);

            if (_rand.NextDouble() < adjustedRisk)
            {
                return -caravanValue - illiquidityCost - guideCost;
            }
            return profit - illiquidityCost - guideCost;
        }

        public (string Market, double Score, string Reasoning) ChooseBestMarket(double caravanValue, double riskTolerance, int guards, string guide, string[] unavailableMarkets, double camelQuality)
        {
            string bestMarket = GameState.DoNothingMarket;
            double bestScore = 0.0;
            string reasoning = "No market selected yet.";

            double illiquidityCostPerDay = guards * GameState.IlliquidityCostPerDayPerGuard;
            var markets = GetDynamicMarkets();

            double expectedTravelRate = GetExpectedTravelRate(guide);
            double guideRiskFactor = expectedTravelRate >= GameState.VeteranTravelThreshold ? GameState.GuideRiskFactorVeteran : expectedTravelRate >= GameState.NoviceTravelThreshold ? GameState.GuideRiskFactorNovice : GameState.GuideRiskFactorNone;
            double guideCost = GetGuideCost(guide);

            foreach (var market in markets)
            {
                string name = market.Name;
                if (unavailableMarkets.Contains(name)) continue;

                double distance = market.Distance;
                double baseRisk = market.BaseRisk * guideRiskFactor;
                baseRisk += (1 - camelQuality) * GameState.CamelRiskPenalty;
                double profit = market.Profit * camelQuality * GameState.ProfitCamelMultiplier;

                double guardEffect = _rand.NextDouble(GameState.GuardEffectMin, GameState.GuardEffectMax);
                double adjustedRisk = Math.Max(0, baseRisk - (guards * guardEffect));
                double effectiveTravelRate = expectedTravelRate * camelQuality;
                double travelTime = distance / (GameState.BaseTravelSpeed * effectiveTravelRate);
                double illiquidityCost = travelTime * illiquidityCostPerDay;

                if (illiquidityCost + guideCost > profit) continue;

                double expectedValue = (1 - adjustedRisk) * profit + adjustedRisk * (-caravanValue) - illiquidityCost - guideCost;
                double score = expectedValue / distance * (1 - adjustedRisk / riskTolerance);

                string marketReasoning = $"Evaluated {name}: Profit = {profit:F2}, Risk = {adjustedRisk:F2}, Illiquidity = {illiquidityCost:F2}, Guide Cost = {guideCost:F2}, Travel = {travelTime:F2} days, Score = {score:F2}";

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMarket = name;
                    reasoning = marketReasoning + $"\nSelected {name} for highest score.";
                }
            }

            if (bestMarket == GameState.DoNothingMarket)
            {
                reasoning = "No market selected: All markets unavailable or unprofitable.";
            }

            return (bestMarket, bestScore, reasoning);
        }
    }
}