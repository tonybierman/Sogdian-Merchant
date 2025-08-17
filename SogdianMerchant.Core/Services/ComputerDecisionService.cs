// ComputerDecisionService.cs (Modified)
namespace SogdianMerchant.Core.Services
{
    public class ComputerDecisionService : IComputerDecisionService
    {
        private readonly ICalculationService _calculationService;

        public ComputerDecisionService(ICalculationService calculationService)
        {
            _calculationService = calculationService;
        }

        public int ChooseGuards(int availableGuards, bool noviceAvailable, bool veteranAvailable, double camelQuality)
        {
            int bestGuards = 0;
            double bestScore = -1.0;

            double bestTravelRate = 1.0;
            if (veteranAvailable) bestTravelRate = 1.3;
            else if (noviceAvailable) bestTravelRate = 1.1;

            for (int guards = 1; guards <= availableGuards; guards++)
            {
                var (market, score, _) = _calculationService.ChooseBestMarket(500.0, 0.5, guards, bestTravelRate, Array.Empty<string>(), camelQuality);
                if (score > bestScore && market != "Do Nothing")
                {
                    bestScore = score;
                    bestGuards = guards;
                }
            }

            return bestGuards > 0 ? bestGuards : 1;
        }

        public string ChooseGuide(bool noviceAvailable, bool veteranAvailable, int guards, double camelQuality)
        {
            string bestGuide = "None";
            double bestScore = -1.0;

            var guideOptions = new List<(double Rate, string Name)> { (1.0, "None") };
            if (noviceAvailable) guideOptions.Add((1.1, "Novice"));
            if (veteranAvailable) guideOptions.Add((1.3, "Veteran"));

            foreach (var guide in guideOptions)
            {
                var (market, score, _) = _calculationService.ChooseBestMarket(500.0, 0.5, guards, guide.Rate, Array.Empty<string>(), camelQuality);
                if (score > bestScore && market != "Do Nothing")
                {
                    bestScore = score;
                    bestGuide = guide.Name;
                }
            }

            return bestGuide;
        }

        public string ChooseMarket(int guards, string guide, string[] unavailableMarkets, double camelQuality)
        {
            double travelRate = _calculationService.GetTravelRate(guide);
            var (market, _, _) = _calculationService.ChooseBestMarket(500.0, 0.5, guards, travelRate, unavailableMarkets, camelQuality);
            return market;
        }
    }
}