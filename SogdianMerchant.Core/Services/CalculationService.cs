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

        private Market[] GetDynamicMarkets()
        {
            return new[]
            {
                new Market("Baghdad Market", 2100.0 * (1 + _rand.NextDouble(-0.2, 0.2)), 0.1 * (1 + _rand.NextDouble(-0.1, 0.1)), 500.0 * (1 + _rand.NextDouble(-0.2, 0.2))),
                new Market("Kashgar Market", 1000.0 * (1 + _rand.NextDouble(-0.2, 0.2)), 0.3 * (1 + _rand.NextDouble(-0.1, 0.1)), 1500.0 * (1 + _rand.NextDouble(-0.2, 0.2))),
                new Market("Karachi Market", 2217.0 * (1 + _rand.NextDouble(-0.2, 0.2)), 0.6 * (1 + _rand.NextDouble(-0.1, 0.1)), 3000.0 * (1 + _rand.NextDouble(-0.2, 0.2)))
            };
        }

        public double GetTravelRate(string guide)
        {
            double baseRate = guide == "Novice" ? 1.1 : guide == "Veteran" ? 1.3 : 1.0;
            double modifier = _rand.NextDouble() < 0.2 ? _rand.NextDouble(0.8, 1.2) : 1.0;
            if (guide != "None" && _rand.NextDouble() < 0.1) modifier *= 0.8;
            return baseRate * modifier;
        }

        public double GetExpectedTravelRate(string guide)
        {
            return guide == "Novice" ? 1.1 : guide == "Veteran" ? 1.3 : 1.0;
        }

        public double GetGuideCost(string guide)
        {
            return guide == "Novice" ? 50.0 : guide == "Veteran" ? 100.0 : 0.0;
        }

        public double CalculateProfit(string market, int guards, string guide, double caravanValue, double camelQuality)
        {
            var markets = GetDynamicMarkets();
            var selectedMarket = markets.FirstOrDefault(m => m.Name == market);
            if (selectedMarket == null) return 0.0;

            double distance = selectedMarket.Distance;
            double baseRisk = selectedMarket.BaseRisk;
            double profit = selectedMarket.Profit * camelQuality * 1.5;

            double travelRate = GetTravelRate(guide);
            double guideRiskFactor = travelRate >= 1.3 ? 1.0 : travelRate >= 1.1 ? 1.1 : 1.2;
            baseRisk *= guideRiskFactor;
            baseRisk += (1 - camelQuality) * 0.3;

            double guardEffect = _rand.NextDouble(0.15, 0.25);
            double adjustedRisk = Math.Max(0, baseRisk - (guards * guardEffect));
            double effectiveTravelRate = travelRate * camelQuality;
            double travelTime = distance / (10.0 * effectiveTravelRate);
            if (_rand.NextDouble() < 0.05 && camelQuality < 0.9) travelTime *= 2;
            double illiquidityCost = travelTime * (guards * 5.0);
            double guideCost = GetGuideCost(guide);

            if (_rand.NextDouble() < adjustedRisk)
            {
                return -caravanValue - illiquidityCost - guideCost;
            }
            return profit - illiquidityCost - guideCost;
        }

        public (string Market, double Score, string Reasoning) ChooseBestMarket(double caravanValue, double riskTolerance, int guards, string guide, string[] unavailableMarkets, double camelQuality)
        {
            string bestMarket = "Do Nothing";
            double bestScore = 0.0;
            string reasoning = "No market selected yet.";

            double illiquidityCostPerDay = guards * 5.0;
            var markets = GetDynamicMarkets();

            double expectedTravelRate = GetExpectedTravelRate(guide);
            double guideRiskFactor = expectedTravelRate >= 1.3 ? 1.0 : expectedTravelRate >= 1.1 ? 1.1 : 1.2;
            double guideCost = GetGuideCost(guide);

            foreach (var market in markets)
            {
                string name = market.Name;
                if (unavailableMarkets.Contains(name)) continue;

                double distance = market.Distance;
                double baseRisk = market.BaseRisk * guideRiskFactor;
                baseRisk += (1 - camelQuality) * 0.3;
                double profit = market.Profit * camelQuality * 1.5;

                double guardEffect = _rand.NextDouble(0.15, 0.25);
                double adjustedRisk = Math.Max(0, baseRisk - (guards * guardEffect));
                double effectiveTravelRate = expectedTravelRate * camelQuality;
                double travelTime = distance / (10.0 * effectiveTravelRate);
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

            if (bestMarket == "Do Nothing")
            {
                reasoning = "No market selected: All markets unavailable or unprofitable.";
            }

            return (bestMarket, bestScore, reasoning);
        }
    }
}