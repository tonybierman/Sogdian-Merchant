using Moq;
using SogdianMerchant.Core;
using SogdianMerchant.Core.Services;
using System.Reflection;
using Xunit;

namespace SogdianMerchant.Tests
{
    public class CalculationServiceTests
    {
        private readonly Mock<IRandomGenerator> _mockRand;
        private readonly CalculationService _service;

        public CalculationServiceTests()
        {
            _mockRand = new Mock<IRandomGenerator>();
            _mockRand.Setup(r => r.NextDouble()).Returns(0.5);
            _mockRand.Setup(r => r.NextDouble(It.IsAny<double>(), It.IsAny<double>()))
                .Returns<double, double>((min, max) => min + (max - min) * 0.5);
            _service = new CalculationService(_mockRand.Object);
        }

        [Fact]
        public void ChooseBestMarket_SelectsMarketWithHighestScore()
        {
            double caravanValue = 1000;
            double riskTolerance = 0.5;
            int guards = 5;
            string guide = GameState.GuideVeteran;
            string[] unavailable = new string[0];
            double camelQuality = 1.0;

            var (market, score, reasoning) = _service.ChooseBestMarket(caravanValue, riskTolerance, guards, guide, unavailable, camelQuality);

            var getMarketsMethod = typeof(CalculationService).GetMethod("GetDynamicMarkets", BindingFlags.NonPublic | BindingFlags.Instance);
            var markets = (Market[])getMarketsMethod.Invoke(_service, null);

            double bestComputedScore = 0.0;
            string bestComputedMarket = GameState.DoNothingMarket;

            double illiquidityCostPerDay = guards * GameState.IlliquidityCostPerDayPerGuard;
            double expectedTravelRate = _service.GetExpectedTravelRate(guide);
            double guideRiskFactor = expectedTravelRate >= GameState.VeteranTravelThreshold ? GameState.GuideRiskFactorVeteran :
                                     expectedTravelRate >= GameState.NoviceTravelThreshold ? GameState.GuideRiskFactorNovice : GameState.GuideRiskFactorNone;
            double guideCost = _service.GetGuideCost(guide);

            foreach (var m in markets)
            {
                if (unavailable.Contains(m.Name)) continue;

                double baseRisk = m.BaseRisk * guideRiskFactor;
                baseRisk += (1 - camelQuality) * GameState.CamelRiskPenalty;
                double profit = m.Profit * camelQuality * GameState.ProfitCamelMultiplier;
                double guardEffect = _mockRand.Object.NextDouble(GameState.GuardEffectMin, GameState.GuardEffectMax);
                double adjustedRisk = Math.Max(0, baseRisk - (guards * guardEffect));
                double effectiveTravelRate = expectedTravelRate * camelQuality;
                double travelTime = m.Distance / (GameState.BaseTravelSpeed * effectiveTravelRate);
                double illiquidityCost = travelTime * illiquidityCostPerDay;

                if (illiquidityCost + guideCost > profit) continue;

                double expectedValue = (1 - adjustedRisk) * profit + adjustedRisk * (-caravanValue) - illiquidityCost - guideCost;
                double computedScore = expectedValue / m.Distance * (1 - adjustedRisk / riskTolerance);

                if (computedScore > bestComputedScore)
                {
                    bestComputedScore = computedScore;
                    bestComputedMarket = m.Name;
                }
            }

            Assert.Equal(bestComputedMarket, market);
            Assert.Equal(bestComputedScore, score, 6);
        }

        [Fact]
        public void ChooseBestMarket_WhenAllUnavailable_ReturnsDoNothing()
        {
            double caravanValue = 1000;
            double riskTolerance = 0.5;
            int guards = 5;
            string guide = GameState.GuideVeteran;
            string[] unavailable = new[] { GameState.BaghdadMarket, GameState.KashgarMarket, GameState.KarachiMarket };
            double camelQuality = 1.0;

            var (market, score, reasoning) = _service.ChooseBestMarket(caravanValue, riskTolerance, guards, guide, unavailable, camelQuality);

            Assert.Equal(GameState.DoNothingMarket, market);
            Assert.Equal(0.0, score);
            Assert.Contains("No market selected", reasoning);
        }
    }
}
