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

            var guideOptions = new List<string> { GameState.GuideNone };
            if (noviceAvailable) guideOptions.Add(GameState.GuideNovice);
            if (veteranAvailable) guideOptions.Add(GameState.GuideVeteran);

            for (int guards = GameState.MinGuards; guards <= availableGuards; guards++)
            {
                double innerBestScore = -1.0;
                foreach (var guide in guideOptions)
                {
                    var (market, score, _) = _calculationService.ChooseBestMarket(GameState.DefaultCaravanValue, GameState.ComputerRiskTolerance, guards, guide, Array.Empty<string>(), camelQuality);
                    if (score > innerBestScore && market != GameState.DoNothingMarket)
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

            return bestGuards > 0 ? bestGuards : GameState.MinGuards;
        }

        public string ChooseGuide(bool noviceAvailable, bool veteranAvailable, int guards, double camelQuality)
        {
            string bestGuide = GameState.GuideNone;
            double bestScore = -1.0;

            var guideOptions = new List<string> { GameState.GuideNone };
            if (noviceAvailable) guideOptions.Add(GameState.GuideNovice);
            if (veteranAvailable) guideOptions.Add(GameState.GuideVeteran);

            foreach (var guide in guideOptions)
            {
                var (market, score, _) = _calculationService.ChooseBestMarket(GameState.DefaultCaravanValue, GameState.ComputerRiskTolerance, guards, guide, Array.Empty<string>(), camelQuality);
                if (score > bestScore && market != GameState.DoNothingMarket)
                {
                    bestScore = score;
                    bestGuide = guide;
                }
            }

            return bestGuide;
        }

        public string ChooseMarket(int guards, string guide, string[] unavailableMarkets, double camelQuality)
        {
            var (market, _, _) = _calculationService.ChooseBestMarket(GameState.DefaultCaravanValue, GameState.ComputerRiskTolerance, guards, guide, unavailableMarkets, camelQuality);
            return market;
        }
    }
}