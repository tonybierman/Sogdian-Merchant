namespace SogdianMerchant.Services
{
    // RandomGenerator.cs
    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _rand = new Random();

        public int Next(int maxValue) => _rand.Next(maxValue);
        public double NextDouble() => _rand.NextDouble();
    }
}
