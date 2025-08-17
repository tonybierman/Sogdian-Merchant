using SogdianMerchant.Core.Services;

namespace SogdianMerchant.Cli.Services
{
    public class OptimalDecisionService
    {
        private readonly ICalculationService _calculationService;
        private readonly IComputerDecisionService _computerDecisionService;

        public OptimalDecisionService(ICalculationService calculationService, IComputerDecisionService computerDecisionService)
        {
            _calculationService = calculationService;
            _computerDecisionService = computerDecisionService;
        }

        public int ChooseGuards(bool playerPicksGuardsFirst, int availableGuards, bool noviceAvailable, bool veteranAvailable, double playerCamelQuality, double computerCamelQuality, bool playerPicksGuideFirst, bool playerPicksMarketFirst)
        {
            int bestGuards = 1;
            double bestDiff = double.MinValue;

            for (int g = 1; g <= availableGuards; g++)
            {
                int playerGuards = g;
                int computerGuards = playerPicksGuardsFirst ? _computerDecisionService.ChooseGuards(availableGuards - g, noviceAvailable, veteranAvailable, computerCamelQuality) : (6 - availableGuards);
                double diff = GetBestDiffFromGuide(playerGuards, computerGuards, noviceAvailable, veteranAvailable, playerPicksGuideFirst, playerCamelQuality, computerCamelQuality, playerPicksMarketFirst);
                if (diff > bestDiff)
                {
                    bestDiff = diff;
                    bestGuards = g;
                }
            }
            return bestGuards;
        }

        public string ChooseGuide(bool playerPicksGuideFirst, int playerGuards, int computerGuards, bool noviceAvailable, bool veteranAvailable, double playerCamelQuality, double computerCamelQuality, bool playerPicksMarketFirst, string? computerGuide = null)
        {
            string bestGuide = "None";
            double bestDiff = double.MinValue;
            List<string> possible = new() { "None" };
            if (noviceAvailable) possible.Add("Novice");
            if (veteranAvailable) possible.Add("Veteran");
            foreach (var playerGuide in possible)
            {
                string currentComputerGuide;
                if (playerPicksGuideFirst)
                {
                    bool tempNovice = noviceAvailable;
                    bool tempVeteran = veteranAvailable;
                    if (playerGuide == "Novice") tempNovice = false;
                    if (playerGuide == "Veteran") tempVeteran = false;
                    currentComputerGuide = _computerDecisionService.ChooseGuide(tempNovice, tempVeteran, computerGuards, computerCamelQuality);
                }
                else
                {
                    currentComputerGuide = computerGuide ?? throw new ArgumentNullException(nameof(computerGuide));
                }
                double diff = GetBestDiffFromMarket(playerGuards, playerGuide, computerGuards, currentComputerGuide, playerPicksMarketFirst, playerCamelQuality, computerCamelQuality);
                if (diff > bestDiff)
                {
                    bestDiff = diff;
                    bestGuide = playerGuide;
                }
            }
            return bestGuide;
        }

        public string ChooseMarket(bool playerPicksMarketFirst, int playerGuards, int computerGuards, string playerGuide, string computerGuide, string[] unavailableMarkets, double playerCamelQuality, double computerCamelQuality, string? computerMarket = null)
        {
            string bestMarket = "Do Nothing";
            double bestDiff = double.MinValue;
            string[] allMarkets = { "Bukhara Market", "Jankent Market", "Karachi Market" };
            List<string> possible = allMarkets.Where(m => !unavailableMarkets.Contains(m)).ToList();
            possible.Add("Do Nothing");
            foreach (var playerMarket in possible)
            {
                string currentComputerMarket;
                if (playerPicksMarketFirst)
                {
                    string[] tempUnavailable = unavailableMarkets;
                    if (playerMarket != "Do Nothing") tempUnavailable = tempUnavailable.Append(playerMarket).ToArray();
                    currentComputerMarket = _computerDecisionService.ChooseMarket(computerGuards, computerGuide, tempUnavailable, computerCamelQuality);
                }
                else
                {
                    currentComputerMarket = computerMarket ?? throw new ArgumentNullException(nameof(computerMarket));
                }
                double playerProfit = _calculationService.CalculateProfit(playerMarket, playerGuards, _calculationService.GetTravelRate(playerGuide), 500.0, playerCamelQuality);
                double computerProfit = _calculationService.CalculateProfit(currentComputerMarket, computerGuards, _calculationService.GetTravelRate(computerGuide), 500.0, computerCamelQuality);
                double diff = playerProfit - computerProfit;
                if (diff > bestDiff)
                {
                    bestDiff = diff;
                    bestMarket = playerMarket;
                }
            }
            return bestMarket;
        }

        private double GetBestDiffFromGuide(int playerGuards, int computerGuards, bool noviceAvailable, bool veteranAvailable, bool playerPicksGuideFirst, double playerCamelQuality, double computerCamelQuality, bool playerPicksMarketFirst)
        {
            double bestDiff = double.MinValue;
            List<string> possible = new() { "None" };
            if (noviceAvailable) possible.Add("Novice");
            if (veteranAvailable) possible.Add("Veteran");
            if (playerPicksGuideFirst)
            {
                foreach (var playerGuide in possible)
                {
                    bool tempNovice = noviceAvailable;
                    bool tempVeteran = veteranAvailable;
                    if (playerGuide == "Novice") tempNovice = false;
                    if (playerGuide == "Veteran") tempVeteran = false;
                    string computerGuide = _computerDecisionService.ChooseGuide(tempNovice, tempVeteran, computerGuards, computerCamelQuality);
                    double diff = GetBestDiffFromMarket(playerGuards, playerGuide, computerGuards, computerGuide, playerPicksMarketFirst, playerCamelQuality, computerCamelQuality);
                    if (diff > bestDiff) bestDiff = diff;
                }
            }
            else
            {
                string computerGuide = _computerDecisionService.ChooseGuide(noviceAvailable, veteranAvailable, computerGuards, computerCamelQuality);
                bool tempNovice = noviceAvailable;
                bool tempVeteran = veteranAvailable;
                if (computerGuide == "Novice") tempNovice = false;
                if (computerGuide == "Veteran") tempVeteran = false;
                List<string> tempPossible = new() { "None" };
                if (tempNovice) tempPossible.Add("Novice");
                if (tempVeteran) tempPossible.Add("Veteran");
                foreach (var playerGuide in tempPossible)
                {
                    double diff = GetBestDiffFromMarket(playerGuards, playerGuide, computerGuards, computerGuide, playerPicksMarketFirst, playerCamelQuality, computerCamelQuality);
                    if (diff > bestDiff) bestDiff = diff;
                }
            }
            return bestDiff;
        }

        private double GetBestDiffFromMarket(int playerGuards, string playerGuide, int computerGuards, string computerGuide, bool playerPicksMarketFirst, double playerCamelQuality, double computerCamelQuality)
        {
            double bestDiff = double.MinValue;
            string[] allMarkets = { "Bukhara Market", "Jankent Market", "Karachi Market" };
            if (playerPicksMarketFirst)
            {
                List<string> possible = allMarkets.ToList();
                possible.Add("Do Nothing");
                foreach (var playerMarket in possible)
                {
                    string[] tempUnavailable = playerMarket == "Do Nothing" ? Array.Empty<string>() : new[] { playerMarket };
                    string computerMarket = _computerDecisionService.ChooseMarket(computerGuards, computerGuide, tempUnavailable, computerCamelQuality);
                    double playerProfit = _calculationService.CalculateProfit(playerMarket, playerGuards, _calculationService.GetTravelRate(playerGuide), 500.0, playerCamelQuality);
                    double computerProfit = _calculationService.CalculateProfit(computerMarket, computerGuards, _calculationService.GetTravelRate(computerGuide), 500.0, computerCamelQuality);
                    double diff = playerProfit - computerProfit;
                    if (diff > bestDiff) bestDiff = diff;
                }
            }
            else
            {
                string[] unavailable = Array.Empty<string>();
                string computerMarket = _computerDecisionService.ChooseMarket(computerGuards, computerGuide, unavailable, computerCamelQuality);
                if (computerMarket != "Do Nothing") unavailable = unavailable.Append(computerMarket).ToArray();
                List<string> possible = allMarkets.Where(m => !unavailable.Contains(m)).ToList();
                possible.Add("Do Nothing");
                foreach (var playerMarket in possible)
                {
                    double playerProfit = _calculationService.CalculateProfit(playerMarket, playerGuards, _calculationService.GetTravelRate(playerGuide), 500.0, playerCamelQuality);
                    double computerProfit = _calculationService.CalculateProfit(computerMarket, computerGuards, _calculationService.GetTravelRate(computerGuide), 500.0, computerCamelQuality);
                    double diff = playerProfit - computerProfit;
                    if (diff > bestDiff) bestDiff = diff;
                }
            }
            return bestDiff;
        }
    }
}
