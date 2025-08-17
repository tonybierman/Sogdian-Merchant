namespace SogdianMerchant.Core.Services
{
    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _random = new Random();

        public int Next(int maxValue) => _random.Next(maxValue);

        public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

        public double NextDouble() => _random.NextDouble();

        public double NextDouble(double min, double max) => _random.NextDouble() * (max - min) + min;
    }
}
