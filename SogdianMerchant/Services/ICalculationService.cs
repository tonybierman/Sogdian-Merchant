namespace SogdianMerchant.Services
{
    public interface ICalculationService
    {
        double CalculateProfit(string market, int guards, double travelRate, double caravanValue, double camelQuality);
        (string Market, double Score, string Reasoning) ChooseBestMarket(double caravanValue, double riskTolerance, int guards, double travelRate, string[] unavailableMarkets, double camelQuality);
        double GetTravelRate(string guide);
    }
}