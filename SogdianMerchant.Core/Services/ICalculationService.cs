// CalculationService.cs (Improved)
namespace SogdianMerchant.Core.Services
{
    public interface ICalculationService
    {
        double CalculateProfit(string market, int guards, string guide, double caravanValue, double camelQuality);
        (string Market, double Score, string Reasoning) ChooseBestMarket(double caravanValue, double riskTolerance, int guards, string guide, string[] unavailableMarkets, double camelQuality);
        double GetExpectedTravelRate(string guide);
        double GetGuideCost(string guide);
        double GetTravelRate(string guide);
    }
}