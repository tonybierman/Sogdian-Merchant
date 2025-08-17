// ComputerDecisionService.cs (Improved)
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

            var guideOptions = new List<string> { "None" };
            if (noviceAvailable) guideOptions.Add("Novice");
            if (veteranAvailable) guideOptions.Add("Veteran");

            for (int guards = 1; guards <= availableGuards; guards++)
            {
                double innerBestScore = -1.0;
                foreach (var guide in guideOptions)
                {
                    var (market, score, _) = _calculationService.ChooseBestMarket(500.0, 0.8, guards, guide, Array.Empty<string>(), camelQuality);
                    if (score > innerBestScore && market != "Do Nothing")
                    {
                        innerBestScore = score;
                    }
                }
                if (innerBestScore > bestScore)
                {
                    bestScore = innerBestScore;
                    bestGuards = guards;
                }
            }

            return bestGuards > 0 ? bestGuards : 1;
        }

        public string ChooseGuide(bool noviceAvailable, bool veteranAvailable, int guards, double camelQuality)
        {
            string bestGuide = "None";
            double bestScore = -1.0;

            var guideOptions = new List<string> { "None" };
            if (noviceAvailable) guideOptions.Add("Novice");
            if (veteranAvailable) guideOptions.Add("Veteran");

            foreach (var guide in guideOptions)
            {
                var (market, score, _) = _calculationService.ChooseBestMarket(500.0, 0.8, guards, guide, Array.Empty<string>(), camelQuality);
                if (score > bestScore && market != "Do Nothing")
                {
                    bestScore = score;
                    bestGuide = guide;
                }
            }

            return bestGuide;
        }

        public string ChooseMarket(int guards, string guide, string[] unavailableMarkets, double camelQuality)
        {
            var (market, _, _) = _calculationService.ChooseBestMarket(500.0, 0.8, guards, guide, unavailableMarkets, camelQuality);
            return market;
        }
    }
}