using SogdianMerchant.Cli.Services;
using SogdianMerchant.Core.Services;

namespace SogdianMerchant.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            int playerWins = 0;
            int computerWins = 0;
            int ties = 0;
            const int numGames = 10000;
            Random random = new Random();
            IRandomGenerator randGen = new RandomGenerator();
            ICalculationService calc = new CalculationService(randGen);
            IComputerDecisionService compDec = new ComputerDecisionService(calc);
            OptimalDecisionService optDec = new OptimalDecisionService(calc, compDec);
            Stats playerStats = new Stats();
            long totalRounds = 0;
            for (int game = 0; game < numGames; game++)
            {
                double playerGold = 500.0;
                double computerGold = 500.0;
                int rounds = 0;
                while (playerGold < 5000.0 && computerGold < 5000.0)
                {
                    double playerCamelQuality = random.NextDouble() * 0.4 + 0.8;
                    double computerCamelQuality = random.NextDouble() * 0.4 + 0.8;
                    bool playerPicksGuardsFirst = random.Next(2) == 0;
                    bool playerPicksGuideFirst = random.Next(2) == 0;
                    bool playerPicksMarketFirst = random.Next(2) == 0;
                    int availableGuards = 6;
                    bool noviceAvailable = true;
                    bool veteranAvailable = true;
                    string[] unavailableMarkets = Array.Empty<string>();
                    int playerGuards;
                    int computerGuards;
                    if (playerPicksGuardsFirst)
                    {
                        playerGuards = optDec.ChooseGuards(playerPicksGuardsFirst, availableGuards, noviceAvailable, veteranAvailable, playerCamelQuality, computerCamelQuality, playerPicksGuideFirst, playerPicksMarketFirst);
                        computerGuards = compDec.ChooseGuards(availableGuards - playerGuards, noviceAvailable, veteranAvailable, computerCamelQuality);
                    }
                    else
                    {
                        computerGuards = compDec.ChooseGuards(availableGuards, noviceAvailable, veteranAvailable, computerCamelQuality);
                        availableGuards -= computerGuards;
                        playerGuards = optDec.ChooseGuards(playerPicksGuardsFirst, availableGuards, noviceAvailable, veteranAvailable, playerCamelQuality, computerCamelQuality, playerPicksGuideFirst, playerPicksMarketFirst);
                    }
                    string playerGuide;
                    string computerGuide;
                    if (playerPicksGuideFirst)
                    {
                        playerGuide = optDec.ChooseGuide(playerPicksGuideFirst, playerGuards, computerGuards, noviceAvailable, veteranAvailable, playerCamelQuality, computerCamelQuality, playerPicksMarketFirst);
                        if (playerGuide == "Novice") noviceAvailable = false;
                        if (playerGuide == "Veteran") veteranAvailable = false;
                        computerGuide = compDec.ChooseGuide(noviceAvailable, veteranAvailable, computerGuards, computerCamelQuality);
                    }
                    else
                    {
                        computerGuide = compDec.ChooseGuide(noviceAvailable, veteranAvailable, computerGuards, computerCamelQuality);
                        if (computerGuide == "Novice") noviceAvailable = false;
                        if (computerGuide == "Veteran") veteranAvailable = false;
                        playerGuide = optDec.ChooseGuide(playerPicksGuideFirst, playerGuards, computerGuards, noviceAvailable, veteranAvailable, playerCamelQuality, computerCamelQuality, playerPicksMarketFirst, computerGuide);
                    }
                    string playerMarket;
                    string computerMarket;
                    if (playerPicksMarketFirst)
                    {
                        playerMarket = optDec.ChooseMarket(playerPicksMarketFirst, playerGuards, computerGuards, playerGuide, computerGuide, unavailableMarkets, playerCamelQuality, computerCamelQuality);
                        if (playerMarket != "Do Nothing") unavailableMarkets = unavailableMarkets.Append(playerMarket).ToArray();
                        computerMarket = compDec.ChooseMarket(computerGuards, computerGuide, unavailableMarkets, computerCamelQuality);
                    }
                    else
                    {
                        computerMarket = compDec.ChooseMarket(computerGuards, computerGuide, unavailableMarkets, computerCamelQuality);
                        if (computerMarket != "Do Nothing") unavailableMarkets = unavailableMarkets.Append(computerMarket).ToArray();
                        playerMarket = optDec.ChooseMarket(playerPicksMarketFirst, playerGuards, computerGuards, playerGuide, computerGuide, unavailableMarkets, playerCamelQuality, computerCamelQuality, computerMarket);
                    }
                    // Collect stats for player (optimal)
                    if (playerPicksGuardsFirst)
                    {
                        playerStats.GuardsWhenFirst.Add(playerGuards);
                    }
                    else
                    {
                        playerStats.GuardsWhenSecond.Add(playerGuards);
                    }
                    if (playerPicksGuideFirst)
                    {
                        playerStats.GuidesWhenFirst[playerGuide]++;
                    }
                    else
                    {
                        playerStats.GuidesWhenSecond[playerGuide]++;
                    }
                    if (playerPicksMarketFirst)
                    {
                        playerStats.MarketsWhenFirst[playerMarket]++;
                    }
                    else
                    {
                        playerStats.MarketsWhenSecond[playerMarket]++;
                    }
                    double playerProfit = calc.CalculateProfit(playerMarket, playerGuards, playerGuide, 500.0, playerCamelQuality);
                    double computerProfit = calc.CalculateProfit(computerMarket, computerGuards, computerGuide, 500.0, computerCamelQuality);
                    if (playerMarket != "Do Nothing")
                    {
                        if (playerMarket == "Baghdad Market") playerStats.BaghdadProfits.Add(playerProfit);
                        else if (playerMarket == "Kashgar Market") playerStats.KashgarProfits.Add(playerProfit);
                        else if (playerMarket == "Karachi Market") playerStats.KarachiProfits.Add(playerProfit);
                    }
                    if (computerMarket != "Do Nothing")
                    {
                        if (computerMarket == "Baghdad Market") playerStats.BaghdadProfits.Add(computerProfit);
                        else if (computerMarket == "Kashgar Market") playerStats.KashgarProfits.Add(computerProfit);
                        else if (computerMarket == "Karachi Market") playerStats.KarachiProfits.Add(computerProfit);
                    }
                    playerGold += playerProfit;
                    computerGold += computerProfit;
                    rounds++;
                }
                totalRounds += rounds;
                if (playerGold >= 5000.0 && computerGold < 5000.0) playerWins++;
                else if (computerGold >= 5000.0 && playerGold < 5000.0) computerWins++;
                else ties++;
            }
            Console.WriteLine($"After {numGames} games:");
            Console.WriteLine($"Player (optimal) wins: {playerWins}");
            Console.WriteLine($"Computer (heuristic) wins: {computerWins}");
            Console.WriteLine($"Ties: {ties}");
            double meanRounds = (double)totalRounds / numGames;
            Console.WriteLine($"Mean rounds to victory: {meanRounds:F2}");

            // Statistical summary for optimal player choices
            Console.WriteLine("\nOptimal Player Statistical Summary:");

            double totalFirstGuards = playerStats.GuardsWhenFirst.Count;
            double totalSecondGuards = playerStats.GuardsWhenSecond.Count;
            double avgGuardsFirst = playerStats.GuardsWhenFirst.Any() ? playerStats.GuardsWhenFirst.Average() : 0;
            double avgGuardsSecond = playerStats.GuardsWhenSecond.Any() ? playerStats.GuardsWhenSecond.Average() : 0;
            Console.WriteLine($"Guards when picking first (count: {totalFirstGuards}): average {avgGuardsFirst:F2}");
            Console.WriteLine($"Guards when picking second (count: {totalSecondGuards}): average {avgGuardsSecond:F2}");

            double totalGuidesFirst = playerStats.GuidesWhenFirst.Values.Sum();
            Console.WriteLine("Guides when picking first:");
            foreach (var kv in playerStats.GuidesWhenFirst)
            {
                double perc = totalGuidesFirst > 0 ? (kv.Value / totalGuidesFirst * 100) : 0;
                Console.WriteLine($"{kv.Key}: {perc:F2}%");
            }

            double totalGuidesSecond = playerStats.GuidesWhenSecond.Values.Sum();
            Console.WriteLine("Guides when picking second:");
            foreach (var kv in playerStats.GuidesWhenSecond)
            {
                double perc = totalGuidesSecond > 0 ? (kv.Value / totalGuidesSecond * 100) : 0;
                Console.WriteLine($"{kv.Key}: {perc:F2}%");
            }

            double totalMarketsFirst = playerStats.MarketsWhenFirst.Values.Sum();
            Console.WriteLine("Markets when picking first:");
            foreach (var kv in playerStats.MarketsWhenFirst)
            {
                double perc = totalMarketsFirst > 0 ? (kv.Value / totalMarketsFirst * 100) : 0;
                Console.WriteLine($"{kv.Key}: {perc:F2}%");
            }

            double totalMarketsSecond = playerStats.MarketsWhenSecond.Values.Sum();
            Console.WriteLine("Markets when picking second:");
            foreach (var kv in playerStats.MarketsWhenSecond)
            {
                double perc = totalMarketsSecond > 0 ? (kv.Value / totalMarketsSecond * 100) : 0;
                Console.WriteLine($"{kv.Key}: {perc:F2}%");
            }

            // Profit statistics
            PrintProfitStats("Baghdad Market", playerStats.BaghdadProfits);
            PrintProfitStats("Kashgar Market", playerStats.KashgarProfits);
            PrintProfitStats("Karachi Market", playerStats.KarachiProfits);
        }

        private static void PrintProfitStats(string marketName, List<double> profits)
        {
            Console.WriteLine($"\nProfit Statistics for {marketName}:");
            if (profits.Any())
            {
                double mean = profits.Average();
                double variance = profits.Select(p => Math.Pow(p - mean, 2)).Average();
                double stdDev = Math.Sqrt(variance);
                Console.WriteLine($"Mean: {mean:F2}, Std Dev: {stdDev:F2}");
                int count = profits.Count;
                int within1 = profits.Count(p => Math.Abs(p - mean) <= stdDev);
                int between1and2 = profits.Count(p => Math.Abs(p - mean) > stdDev && Math.Abs(p - mean) <= 2 * stdDev);
                int between2and3 = profits.Count(p => Math.Abs(p - mean) > 2 * stdDev && Math.Abs(p - mean) <= 3 * stdDev);
                int beyond3 = profits.Count(p => Math.Abs(p - mean) > 3 * stdDev);
                Console.WriteLine($"<= 1 SD: {(within1 / (double)count * 100):F2}%");
                Console.WriteLine($">1 and <=2 SD: {(between1and2 / (double)count * 100):F2}%");
                Console.WriteLine($">2 and <=3 SD: {(between2and3 / (double)count * 100):F2}%");
                Console.WriteLine($">3 SD: {(beyond3 / (double)count * 100):F2}%");
            }
            else
            {
                Console.WriteLine("No data");
            }
        }
    }
}