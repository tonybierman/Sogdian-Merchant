// GameState.cs
namespace SogdianMerchant.Core
{
    public class GameState
    {
        public double PlayerGold { get; set; } = StartingGold;
        public double ComputerGold { get; set; } = StartingGold;
        public int RoundNumber { get; set; } = InitialRoundNumber;
        public const int TotalGuards = 6;
        public const int MinGuards = 1;
        public bool NoviceAvailable { get; set; }
        public bool VeteranAvailable { get; set; }
        public string[] UnavailableMarkets { get; set; } = Array.Empty<string>();
        public double PlayerCamelQuality { get; set; }
        public double ComputerCamelQuality { get; set; }
        public int AvailableGuards { get; set; }
        public bool ChoosingGuards { get; set; }
        public bool ChoosingGuide { get; set; }
        public bool ChoosingMarket { get; set; }
        public int GuardInput { get; set; }
        public string GuideInput { get; set; } = GuideNone;
        public string MarketInput { get; set; } = DoNothingMarket;
        public int PlayerGuards { get; set; }
        public int ComputerGuards { get; set; }
        public string PlayerGuide { get; set; } = GuideNone;
        public string ComputerGuide { get; set; } = GuideNone;
        public string PlayerMarket { get; set; } = DoNothingMarket;
        public string ComputerMarket { get; set; } = DoNothingMarket;
        public bool PlayerPicksGuardsFirst { get; set; }
        public bool PlayerPicksGuideFirst { get; set; }
        public bool PlayerPicksMarketFirst { get; set; }
        public string CurrentMessage { get; set; } = "";
        public string CurrentPhase { get; set; } = "";
        public bool GameOver { get; set; } = false;

        public string[] AvailableMarkets => AllMarkets
            .Where(m => !UnavailableMarkets.Contains(m)).ToArray();

        public const double StartingGold = 500.0;
        public const double WinningGold = 5000.0;
        public const double DefaultCaravanValue = 500.0;
        public const double ComputerRiskTolerance = 0.8;
        public const double MinRiskTolerance = 0.3;
        public const double MaxRiskTolerance = 0.7;
        public const double MinCamelQuality = 0.8;
        public const double CamelQualityVariation = 0.4;
        public const double GuardScarcityProbability = 0.05;
        public const double GuideUnavailabilityProbability = 0.05;
        public const double MarketUnavailabilityProbability = 0.2;
        public const string GuideNone = "None";
        public const string GuideNovice = "Novice";
        public const string GuideVeteran = "Veteran";
        public const string DoNothingMarket = "Do Nothing";
        public const string BaghdadMarket = "Baghdad Market";
        public const string KashgarMarket = "Kashgar Market";
        public const string KarachiMarket = "Karachi Market";
        public static readonly string[] AllMarkets = new[] { BaghdadMarket, KashgarMarket, KarachiMarket };
        public const double BaseDistanceBaghdad = 2100.0;
        public const double BaseRiskBaghdad = 0.1;
        public const double BaseProfitBaghdad = 500.0;
        public const double BaseDistanceKashgar = 1000.0;
        public const double BaseRiskKashgar = 0.3;
        public const double BaseProfitKashgar = 1500.0;
        public const double BaseDistanceKarachi = 2217.0;
        public const double BaseRiskKarachi = 0.6;
        public const double BaseProfitKarachi = 3000.0;
        public const double MarketVariationDistance = 0.2;
        public const double MarketVariationRisk = 0.1;
        public const double MarketVariationProfit = 0.2;
        public const double TravelRateNone = 1.0;
        public const double TravelRateNovice = 1.1;
        public const double TravelRateVeteran = 1.3;
        public const double TravelModifierProb = 0.2;
        public const double TravelModifierMin = 0.8;
        public const double TravelModifierMax = 1.2;
        public const double GuidePenaltyProb = 0.1;
        public const double GuidePenaltyMult = 0.8;
        public const double GuideCostNone = 0.0;
        public const double GuideCostNovice = 50.0;
        public const double GuideCostVeteran = 100.0;
        public const double ProfitCamelMultiplier = 1.5;
        public const double VeteranTravelThreshold = 1.3;
        public const double NoviceTravelThreshold = 1.1;
        public const double GuideRiskFactorVeteran = 1.0;
        public const double GuideRiskFactorNovice = 1.1;
        public const double GuideRiskFactorNone = 1.2;
        public const double CamelRiskPenalty = 0.3;
        public const double GuardEffectMin = 0.15;
        public const double GuardEffectMax = 0.25;
        public const double BaseTravelSpeed = 10.0;
        public const double TravelDelayProb = 0.05;
        public const double CamelDelayThreshold = 0.9;
        public const double TravelDelayMultiplier = 2.0;
        public const double IlliquidityCostPerDayPerGuard = 5.0;
        public const int InitialRoundNumber = 1;
    }
}