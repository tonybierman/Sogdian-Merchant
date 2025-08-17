namespace SogdianMerchant.Core.Services
{
    public interface IComputerDecisionService
    {
        int ChooseGuards(int availableGuards, bool noviceAvailable, bool veteranAvailable, double camelQuality);
        string ChooseGuide(bool noviceAvailable, bool veteranAvailable, int guards, double camelQuality);
        string ChooseMarket(int guards, string guide, string[] unavailableMarkets, double camelQuality);
    }
}