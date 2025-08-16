namespace SogdianMerchant
{
    // GameState.cs
    public class GameState
    {
        public double PlayerGold { get; set; } = 500.0;
        public double ComputerGold { get; set; } = 500.0;
        public int RoundNumber { get; set; } = 1;
        public const int TotalGuards = 6;
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
        public string GuideInput { get; set; } = "None";
        public string MarketInput { get; set; } = "Do Nothing";
        public int PlayerGuards { get; set; }
        public int ComputerGuards { get; set; }
        public string PlayerGuide { get; set; } = "None";
        public string ComputerGuide { get; set; } = "None";
        public string PlayerMarket { get; set; } = "Do Nothing";
        public string ComputerMarket { get; set; } = "Do Nothing";
        public bool PlayerPicksGuardsFirst { get; set; }
        public bool PlayerPicksGuideFirst { get; set; }
        public bool PlayerPicksMarketFirst { get; set; }
        public string CurrentMessage { get; set; } = "";
        public string CurrentPhase { get; set; } = "";
        public bool GameOver { get; set; } = false;

        public string[] AvailableMarkets => new[] { "Bukhara Market", "Jankent Market", "Karachi Market" }
            .Where(m => !UnavailableMarkets.Contains(m)).ToArray();
    }
}
