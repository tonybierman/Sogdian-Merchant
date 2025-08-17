// CalculationService.cs (Modified)
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
                new Market("Bukhara Market", 280 * (1 + _rand.NextDouble(-0.2, 0.2)), 0.1 * (1 + _rand.NextDouble(-0.1, 0.1)), 500.0 * (1 + _rand.NextDouble(-0.2, 0.2))),
                new Market("Jankent Market", 780.0 * (1 + _rand.NextDouble(-0.2, 0.2)), 0.3 * (1 + _rand.NextDouble(-0.1, 0.1)), 1500.0 * (1 + _rand.NextDouble(-0.2, 0.2))),
                new Market("Karachi Market", 2217.0 * (1 + _rand.NextDouble(-0.2, 0.2)), 0.6 * (1 + _rand.NextDouble(-0.1, 0.1)), 3000.0 * (1 + _rand.NextDouble(-0.2, 0.2)))
            };
        }

        public double GetTravelRate(string guide)
        {
            double baseRate = guide == "Novice" ? 1.1 : guide == "Veteran" ? 1.3 : 1.0;
            double modifier = _rand.NextDouble() < 0.2 ? _rand.NextDouble(0.8, 1.2) : 1.0; // 20% chance of randomization
            if (guide != "None" && _rand.NextDouble() < 0.1) modifier *= 0.8; // 10% failure chance
            return baseRate * modifier;
        }

        public double CalculateProfit(string market, int guards, double travelRate, double caravanValue, double camelQuality)
        {
            var markets = GetDynamicMarkets();
            var selectedMarket = markets.FirstOrDefault(m => m.Name == market);
            if (selectedMarket == null) return 0.0;

            double distance = selectedMarket.Distance;
            double baseRisk = selectedMarket.BaseRisk;
            double profit = selectedMarket.Profit * camelQuality * 1.5; // Increased influence
            double guardEffect = _rand.NextDouble(0.05, 0.15);
            double adjustedRisk = Math.Max(0, baseRisk - (guards * guardEffect));
            double effectiveTravelRate = travelRate * camelQuality;
            double travelTime = distance / (10.0 * effectiveTravelRate);
            if (_rand.NextDouble() < 0.05 && camelQuality < 0.9) travelTime *= 2; // Rare poor camel event
            double illiquidityCost = travelTime * (guards * 10.0);

            return profit - (caravanValue * adjustedRisk) - illiquidityCost;
        }

        public (string Market, double Score, string Reasoning) ChooseBestMarket(double caravanValue, double riskTolerance, int guards, double travelRate, string[] unavailableMarkets, double camelQuality)
        {
            string bestMarket = "Do Nothing";
            double bestScore = 0.0;
            string reasoning = "No market selected yet.";

            double illiquidityCostPerDay = guards * 10.0;
            var markets = GetDynamicMarkets();

            foreach (var market in markets)
            {
                string name = market.Name;
                if (unavailableMarkets.Contains(name)) continue;

                double distance = market.Distance;
                double baseRisk = market.BaseRisk;
                double profit = market.Profit * camelQuality * 1.5; // Increased influence

                double guardEffect = _rand.NextDouble(0.05, 0.15);
                double adjustedRisk = Math.Max(0, baseRisk - (guards * guardEffect));
                double effectiveTravelRate = travelRate * camelQuality;
                double travelTime = distance / (10.0 * effectiveTravelRate);
                if (_rand.NextDouble() < 0.05 && camelQuality < 0.9) travelTime *= 2; // Rare poor camel event
                double illiquidityCost = travelTime * illiquidityCostPerDay;

                if (illiquidityCost > profit) continue;

                double expectedValue = profit - (caravanValue * adjustedRisk) - illiquidityCost;
                double score = expectedValue / distance * (1 - adjustedRisk / riskTolerance);

                string marketReasoning = $"Evaluated {name}: Profit = {profit:F2}, Risk = {adjustedRisk:F2}, Risk loss = {(caravanValue * adjustedRisk):F2}, Illiquidity = {illiquidityCost:F2}, Travel = {travelTime:F2} days, Score = {score:F2}";

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