namespace SogdianMerchant.Core.Services
{
    public interface IRandomGenerator
    {
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
        double NextDouble();
        double NextDouble(double min, double max);
    }
}
