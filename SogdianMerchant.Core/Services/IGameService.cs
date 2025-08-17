namespace SogdianMerchant.Core.Services
{
    public interface IGameService
    {
        GameState State { get; }

        void ContinueToNextRound();
        void EndRound();
        void RestartGame();
        void StartGuardsPhase();
        void StartGuidePhase();
        void StartMarketPhase();
        void StartRound();
        void SubmitGuards();
        void SubmitGuide();
        void SubmitMarket();
    }
}