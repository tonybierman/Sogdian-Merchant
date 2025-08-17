// GameService.cs (Modified)
namespace SogdianMerchant.Core.Services
{
    public class GameService : IGameService
    {
        public GameState State { get; } = new GameState();
        private readonly IRandomGenerator _rand;
        private readonly IComputerDecisionService _computerDecisionService;
        private readonly ICalculationService _calculationService;

        public GameService(IRandomGenerator rand, IComputerDecisionService computerDecisionService, ICalculationService calculationService)
        {
            _rand = rand;
            _computerDecisionService = computerDecisionService;
            _calculationService = calculationService;
        }

        public void StartRound()
        {
            State.CurrentMessage = $"Round {State.RoundNumber} begins!\n";
            State.PlayerCamelQuality = _rand.NextDouble() * 0.4 + 0.8;
            State.ComputerCamelQuality = _rand.NextDouble() * 0.4 + 0.8;
            State.AvailableGuards = GameState.TotalGuards;
            State.NoviceAvailable = true;
            State.VeteranAvailable = true;
            State.UnavailableMarkets = Array.Empty<string>();

            if (_rand.NextDouble() < 0.05) State.AvailableGuards = 0; // Resource scarcity
            if (_rand.NextDouble() < 0.05)
            {
                State.NoviceAvailable = false;
                State.VeteranAvailable = false;
            }

            // Random market unavailability
            if (_rand.NextDouble() < 0.2)
            {
                var randomMarket = new[] { "Bukhara Market", "Jankent Market", "Karachi Market" }[_rand.Next(3)];
                State.UnavailableMarkets = new[] { randomMarket };
            }

            StartGuardsPhase();
        }

        public void StartGuardsPhase()
        {
            State.CurrentPhase = "guards";
            State.PlayerPicksGuardsFirst = _rand.Next(2) == 0;
            State.CurrentMessage += State.PlayerPicksGuardsFirst ? "You pick guards first.\n" : "Computer picks guards first.\n";

            if (State.PlayerPicksGuardsFirst)
            {
                State.ChoosingGuards = true;
            }
            else
            {
                State.ComputerGuards = _computerDecisionService.ChooseGuards(State.AvailableGuards, State.NoviceAvailable, State.VeteranAvailable, State.ComputerCamelQuality);
                State.AvailableGuards -= State.ComputerGuards;
                State.CurrentMessage += $"Computer chose {State.ComputerGuards} guards, leaving {State.AvailableGuards} for you.\n";
                State.ChoosingGuards = true;
            }
        }

        public void SubmitGuards()
        {
            State.PlayerGuards = Math.Clamp(State.GuardInput, 1, State.AvailableGuards);
            State.AvailableGuards -= State.PlayerGuards;
            if (State.PlayerPicksGuardsFirst)
            {
                State.ComputerGuards = _computerDecisionService.ChooseGuards(State.AvailableGuards, State.NoviceAvailable, State.VeteranAvailable, State.ComputerCamelQuality);
                State.AvailableGuards -= State.ComputerGuards;
                State.CurrentMessage += $"You chose {State.PlayerGuards} guards. Computer then chose {State.ComputerGuards} guards.\n";
            }
            else
            {
                State.CurrentMessage += $"You chose {State.PlayerGuards} guards.\n";
            }

            State.ChoosingGuards = false;
            StartGuidePhase();
        }

        public void StartGuidePhase()
        {
            State.CurrentPhase = "guide";
            State.PlayerPicksGuideFirst = _rand.Next(2) == 0;
            State.CurrentMessage += State.PlayerPicksGuideFirst ? "You pick guide first.\n" : "Computer picks guide first.\n";

            if (State.PlayerPicksGuideFirst)
            {
                State.ChoosingGuide = true;
            }
            else
            {
                State.ComputerGuide = _computerDecisionService.ChooseGuide(State.NoviceAvailable, State.VeteranAvailable, State.ComputerGuards, State.ComputerCamelQuality);
                if (State.ComputerGuide == "Novice") State.NoviceAvailable = false;
                if (State.ComputerGuide == "Veteran") State.VeteranAvailable = false;
                State.CurrentMessage += $"Computer chose {State.ComputerGuide} guide.\n";
                State.ChoosingGuide = true;
            }
        }

        public void SubmitGuide()
        {
            State.PlayerGuide = State.GuideInput;
            if (State.PlayerGuide == "Novice" && State.NoviceAvailable) State.NoviceAvailable = false;
            else if (State.PlayerGuide == "Novice") State.PlayerGuide = "None";
            if (State.PlayerGuide == "Veteran" && State.VeteranAvailable) State.VeteranAvailable = false;
            else if (State.PlayerGuide == "Veteran") State.PlayerGuide = "None";

            if (State.PlayerPicksGuideFirst)
            {
                State.ComputerGuide = _computerDecisionService.ChooseGuide(State.NoviceAvailable, State.VeteranAvailable, State.ComputerGuards, State.ComputerCamelQuality);
                if (State.ComputerGuide == "Novice") State.NoviceAvailable = false;
                if (State.ComputerGuide == "Veteran") State.VeteranAvailable = false;
                State.CurrentMessage += $"You chose {State.PlayerGuide} guide. Computer then chose {State.ComputerGuide} guide.\n";
            }
            else
            {
                State.CurrentMessage += $"You chose {State.PlayerGuide} guide.\n";
            }

            State.ChoosingGuide = false;
            StartMarketPhase();
        }

        public void StartMarketPhase()
        {
            State.CurrentPhase = "market";
            State.PlayerPicksMarketFirst = _rand.Next(2) == 0;
            State.CurrentMessage += State.PlayerPicksMarketFirst ? "You pick market first.\n" : "Computer picks market first.\n";

            if (State.PlayerPicksMarketFirst)
            {
                State.ChoosingMarket = true;
            }
            else
            {
                State.ComputerMarket = _computerDecisionService.ChooseMarket(State.ComputerGuards, State.ComputerGuide, State.UnavailableMarkets, State.ComputerCamelQuality);
                if (State.ComputerMarket != "Do Nothing")
                {
                    State.UnavailableMarkets = State.UnavailableMarkets.Append(State.ComputerMarket).ToArray();
                    State.CurrentMessage += $"Computer chose {State.ComputerMarket}.\n";
                }
                else
                {
                    State.CurrentMessage += "Computer chose to Do Nothing.\n";
                }
                State.ChoosingMarket = true;
            }
        }

        public void SubmitMarket()
        {
            State.PlayerMarket = State.AvailableMarkets.Contains(State.MarketInput) ? State.MarketInput : "Do Nothing";
            if (State.PlayerMarket != "Do Nothing")
            {
                State.UnavailableMarkets = State.UnavailableMarkets.Append(State.PlayerMarket).ToArray();
            }

            if (State.PlayerPicksMarketFirst)
            {
                State.ComputerMarket = _computerDecisionService.ChooseMarket(State.ComputerGuards, State.ComputerGuide, State.UnavailableMarkets, State.ComputerCamelQuality);
                if (State.ComputerMarket != "Do Nothing")
                {
                    State.UnavailableMarkets = State.UnavailableMarkets.Append(State.ComputerMarket).ToArray();
                    State.CurrentMessage += $"You chose {State.PlayerMarket}. Computer then chose {State.ComputerMarket}.\n";
                }
                else
                {
                    State.CurrentMessage += $"You chose {State.PlayerMarket}. Computer chose to Do Nothing.\n";
                }
            }
            else
            {
                State.CurrentMessage += $"You chose {State.PlayerMarket}.\n";
            }

            State.ChoosingMarket = false;
            State.CurrentPhase = "end";
            EndRound();
        }

        public void EndRound()
        {
            double riskTolerance = _rand.NextDouble(0.3, 0.7); // Random risk tolerance
            double playerTravelRate = _calculationService.GetTravelRate(State.PlayerGuide);
            double computerTravelRate = _calculationService.GetTravelRate(State.ComputerGuide);

            double playerProfit = _calculationService.CalculateProfit(State.PlayerMarket, State.PlayerGuards, playerTravelRate, 500.0, State.PlayerCamelQuality);
            double computerProfit = _calculationService.CalculateProfit(State.ComputerMarket, State.ComputerGuards, computerTravelRate, 500.0, State.ComputerCamelQuality);

            State.PlayerGold += playerProfit;
            State.ComputerGold += computerProfit;

            var playerResult = _calculationService.ChooseBestMarket(500.0, riskTolerance, State.PlayerGuards, playerTravelRate, State.UnavailableMarkets, State.PlayerCamelQuality);
            var computerResult = _calculationService.ChooseBestMarket(500.0, riskTolerance, State.ComputerGuards, computerTravelRate, State.UnavailableMarkets, State.ComputerCamelQuality);

            State.CurrentMessage += "\nRound Summary:\n";
            State.CurrentMessage += $"You sent a caravan with {State.PlayerGuards} guards and a {State.PlayerGuide} guide to {State.PlayerMarket}.\n";
            State.CurrentMessage += State.PlayerMarket != "Do Nothing" ? $"Your caravan earned {playerProfit:F2} gold.\n" : "You stayed home and earned no profit.\n";
            State.CurrentMessage += $"\nThe computer sent a caravan with {State.ComputerGuards} guards and a {State.ComputerGuide} guide to {State.ComputerMarket}.\n";
            State.CurrentMessage += State.ComputerMarket != "Do Nothing" ? $"The computer's caravan earned {computerProfit:F2} gold.\n" : "The computer stayed home and earned no profit.\n";

            State.RoundNumber++;

            if (State.PlayerGold >= 5000.0 || State.ComputerGold >= 5000.0)
            {
                State.GameOver = true;
                State.CurrentMessage += "\nGame Over!\n";
            }
        }

        public void ContinueToNextRound()
        {
            if (!State.GameOver)
            {
                StartRound();
            }
        }

        public void RestartGame()
        {
            State.PlayerGold = 500.0;
            State.ComputerGold = 500.0;
            State.RoundNumber = 1;
            State.GameOver = false;
            StartRound();
        }
    }
}