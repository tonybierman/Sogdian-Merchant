namespace SogdianMerchant.Core.Services
{
    // CalculationService.cs
    public class CalculationService : ICalculationService
    {
        private static readonly Market[] Markets =
        {
            new("Bukhara Market", 280, 0.1, 500.0),
            new("Jankent Market", 780.0, 0.3, 1500.0),
            new("Karachi Market", 2217.0, 0.6, 3000.0)
        };

        public double GetTravelRate(string guide)
        {
            return guide == "Novice" ? 1.1 : guide == "Veteran" ? 1.3 : 1.0;
        }

        public double CalculateProfit(string market, int guards, double travelRate, double caravanValue, double camelQuality)
        {
            var selectedMarket = Markets.FirstOrDefault(m => m.Name == market);
            if (selectedMarket == null) return 0.0;

            double distance = selectedMarket.Distance;
            double baseRisk = selectedMarket.BaseRisk;
            double profit = selectedMarket.Profit;
            double adjustedRisk = Math.Max(0, baseRisk - (guards * 0.1));
            double effectiveTravelRate = travelRate * camelQuality;
            double travelTime = distance / (10.0 * effectiveTravelRate);
            double illiquidityCost = travelTime * (guards * 10.0);

            return profit - (caravanValue * adjustedRisk) - illiquidityCost;
        }

        public (string Market, double Score, string Reasoning) ChooseBestMarket(double caravanValue, double riskTolerance, int guards, double travelRate, string[] unavailableMarkets, double camelQuality)
        {
            string bestMarket = "Do Nothing";
            double bestScore = 0.0;
            string reasoning = "No market selected yet.";

            double illiquidityCostPerDay = guards * 10.0;

            foreach (var market in Markets)
            {
                string name = market.Name;
                if (unavailableMarkets.Contains(name)) continue;

                double distance = market.Distance;
                double baseRisk = market.BaseRisk;
                double profit = market.Profit;

                double adjustedRisk = Math.Max(0, baseRisk - (guards * 0.1));
                double effectiveTravelRate = travelRate * camelQuality;
                double travelTime = distance / (10.0 * effectiveTravelRate);
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
