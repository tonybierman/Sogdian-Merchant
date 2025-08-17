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
            const int numGames = 1000;
            Random random = new Random();
            ICalculationService calc = new CalculationService();
            IComputerDecisionService compDec = new ComputerDecisionService(calc);
            OptimalDecisionService optDec = new OptimalDecisionService(calc, compDec);
            Stats playerStats = new Stats();
            for (int game = 0; game < numGames; game++)
            {
                double playerGold = 500.0;
                double computerGold = 500.0;
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
                    double playerTravelRate = calc.GetTravelRate(playerGuide);
                    double computerTravelRate = calc.GetTravelRate(computerGuide);
                    double playerProfit = calc.CalculateProfit(playerMarket, playerGuards, playerTravelRate, 500.0, playerCamelQuality);
                    double computerProfit = calc.CalculateProfit(computerMarket, computerGuards, computerTravelRate, 500.0, computerCamelQuality);
                    playerGold += playerProfit;
                    computerGold += computerProfit;
                }
                if (playerGold >= 5000.0 && computerGold < 5000.0) playerWins++;
                else if (computerGold >= 5000.0 && playerGold < 5000.0) computerWins++;
                else ties++;
            }
            Console.WriteLine($"After {numGames} games:");
            Console.WriteLine($"Player (optimal) wins: {playerWins}");
            Console.WriteLine($"Computer (heuristic) wins: {computerWins}");
            Console.WriteLine($"Ties: {ties}");

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
        }
    }
}