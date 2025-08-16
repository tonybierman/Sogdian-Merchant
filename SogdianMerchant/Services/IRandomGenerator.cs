namespace SogdianMerchant.Services
{
    // IRandomGenerator.cs
    public interface IRandomGenerator
    {
        int Next(int maxValue);
        double NextDouble();
    }
}
