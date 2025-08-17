// RandomGenerator.cs (Modified to use dice rolling library)
using System.Linq;
using Xyaneon.Games.Dice;

namespace SogdianMerchant.Core.Services
{
    public class RandomGenerator : IRandomGenerator
    {
        private static readonly Die<int> ResolutionDie = new Die<int>(Enumerable.Range(0, 20).ToArray());

        public int Next(int maxValue)
        {
            if (maxValue <= 0) return 0; // Handle invalid, though unlikely
            var faces = Enumerable.Range(0, maxValue).ToArray();
            var die = new Die<int>(faces);
            return die.Roll();
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue) return minValue; // Handle invalid
            var faces = Enumerable.Range(minValue, maxValue - minValue).ToArray();
            var die = new Die<int>(faces);
            return die.Roll();
        }

        public double NextDouble()
        {
            return ResolutionDie.Roll() / 19.0;
        }

        public double NextDouble(double min, double max)
        {
            return min + (max - min) * NextDouble();
        }
    }
}