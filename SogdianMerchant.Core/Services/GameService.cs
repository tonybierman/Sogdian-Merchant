// GameService.cs (Improved)
namespace SogdianMerchant.Core.Services
{
    public class GameService : IGameService
    {
        public GameState State { get; } = new GameState();
        private readonly IRandomGenerator _rand;
        private readonly IComputerDecisionService _computerDecisionService;
        private readonly ICalculationService _calculationService;
        private readonly IMessageHubService _messenger;

        public GameService(IRandomGenerator rand, IComputerDecisionService computerDecisionService, ICalculationService calculationService, IMessageHubService messenger)
        {
            _rand = rand;
            _computerDecisionService = computerDecisionService;
            _calculationService = calculationService;
            _messenger = messenger;
        }

        public void StartRound()
        {
            _messenger.Clear();
            State.CurrentPhase = "guards";
            State.PlayerCamelQuality = _rand.NextDouble() * GameState.CamelQualityVariation + GameState.MinCamelQuality;
            State.ComputerCamelQuality = _rand.NextDouble() * GameState.CamelQualityVariation + GameState.MinCamelQuality;
            State.AvailableGuards = GameState.TotalGuards;
            State.NoviceAvailable = true;
            State.VeteranAvailable = true;
            State.UnavailableMarkets = Array.Empty<string>();

            _messenger.Publish($"Round {State.RoundNumber} begins!");

            if (_rand.NextDouble() < GameState.GuardScarcityProbability) State.AvailableGuards = 0; // Resource scarcity
            if (_rand.NextDouble() < GameState.GuideUnavailabilityProbability)
            {
                State.NoviceAvailable = false;
                State.VeteranAvailable = false;
            }

            // Random market unavailability
            if (_rand.NextDouble() < GameState.MarketUnavailabilityProbability)
            {
                var randomMarket = GameState.AllMarkets[_rand.Next(GameState.AllMarkets.Length)];
                State.UnavailableMarkets = new[] { randomMarket };
                _messenger.Publish($"{randomMarket} is unavailable this round.");
            }

            StartGuardsPhase();
        }

        public void StartGuardsPhase()
        {
            State.CurrentPhase = "guards";
            State.PlayerPicksGuardsFirst = _rand.Next(2) == 0;
            _messenger.Publish(State.PlayerPicksGuardsFirst ? "You pick guards first." : "Computer picks guards first.");

            if (State.PlayerPicksGuardsFirst)
            {
                State.ChoosingGuards = true;
            }
            else
            {
                State.ComputerGuards = _computerDecisionService.ChooseGuards(State.AvailableGuards, State.NoviceAvailable, State.VeteranAvailable, State.ComputerCamelQuality);
                State.AvailableGuards -= State.ComputerGuards;
                _messenger.Publish($"Computer chose {State.ComputerGuards} guards, leaving {State.AvailableGuards} for you.");
                State.ChoosingGuards = true;
            }
        }

        public void SubmitGuards()
        {
            State.PlayerGuards = Math.Clamp(State.GuardInput, GameState.MinGuards, State.AvailableGuards);
            State.AvailableGuards -= State.PlayerGuards;
            if (State.PlayerPicksGuardsFirst)
            {
                State.ComputerGuards = _computerDecisionService.ChooseGuards(State.AvailableGuards, State.NoviceAvailable, State.VeteranAvailable, State.ComputerCamelQuality);
                State.AvailableGuards -= State.ComputerGuards;
                _messenger.Publish($"You chose {State.PlayerGuards} guards. Computer then chose {State.ComputerGuards} guards.");
            }
            else
            {
                _messenger.Publish($"You chose {State.PlayerGuards} guards.");
            }

            State.ChoosingGuards = false;
            StartGuidePhase();
        }

        public void StartGuidePhase()
        {
            State.CurrentPhase = "guide";
            State.PlayerPicksGuideFirst = _rand.Next(2) == 0;
            _messenger.Publish(State.PlayerPicksGuideFirst ? "You pick guide first." : "Computer picks guide first.");

            if (State.PlayerPicksGuideFirst)
            {
                State.ChoosingGuide = true;
            }
            else
            {
                State.ComputerGuide = _computerDecisionService.ChooseGuide(State.NoviceAvailable, State.VeteranAvailable, State.ComputerGuards, State.ComputerCamelQuality);
                if (State.ComputerGuide == GameState.GuideNovice) State.NoviceAvailable = false;
                if (State.ComputerGuide == GameState.GuideVeteran) State.VeteranAvailable = false;
                _messenger.Publish($"Computer chose {State.ComputerGuide} guide.");
                State.ChoosingGuide = true;
            }
        }

        public void SubmitGuide()
        {
            State.PlayerGuide = State.GuideInput;
            if (State.PlayerGuide == GameState.GuideNovice && State.NoviceAvailable) State.NoviceAvailable = false;
            else if (State.PlayerGuide == GameState.GuideNovice) State.PlayerGuide = GameState.GuideNone;
            if (State.PlayerGuide == GameState.GuideVeteran && State.VeteranAvailable) State.VeteranAvailable = false;
            else if (State.PlayerGuide == GameState.GuideVeteran) State.PlayerGuide = GameState.GuideNone;

            if (State.PlayerPicksGuideFirst)
            {
                State.ComputerGuide = _computerDecisionService.ChooseGuide(State.NoviceAvailable, State.VeteranAvailable, State.ComputerGuards, State.ComputerCamelQuality);
                if (State.ComputerGuide == GameState.GuideNovice) State.NoviceAvailable = false;
                if (State.ComputerGuide == GameState.GuideVeteran) State.VeteranAvailable = false;
                _messenger.Publish($"You chose {State.PlayerGuide} guide. Computer then chose {State.ComputerGuide} guide.");
            }
            else
            {
                _messenger.Publish($"You chose {State.PlayerGuide} guide.");
            }

            State.ChoosingGuide = false;
            StartMarketPhase();
        }

        public void StartMarketPhase()
        {
            State.CurrentPhase = "market";
            State.PlayerPicksMarketFirst = _rand.Next(2) == 0;
            _messenger.Publish(State.PlayerPicksMarketFirst ? "You pick market first." : "Computer picks market first.");

            if (State.PlayerPicksMarketFirst)
            {
                State.ChoosingMarket = true;
            }
            else
            {
                State.ComputerMarket = _computerDecisionService.ChooseMarket(State.ComputerGuards, State.ComputerGuide, State.UnavailableMarkets, State.ComputerCamelQuality);
                if (State.ComputerMarket != GameState.DoNothingMarket)
                {
                    State.UnavailableMarkets = State.UnavailableMarkets.Append(State.ComputerMarket).ToArray();
                    _messenger.Publish($"Computer chose {State.ComputerMarket}.");
                }
                else
                {
                    _messenger.Publish("Computer chose to Do Nothing.");
                }
                State.ChoosingMarket = true;
            }
        }

        public void SubmitMarket()
        {
            State.PlayerMarket = State.AvailableMarkets.Contains(State.MarketInput) ? State.MarketInput : GameState.DoNothingMarket;
            if (State.PlayerMarket != GameState.DoNothingMarket)
            {
                State.UnavailableMarkets = State.UnavailableMarkets.Append(State.PlayerMarket).ToArray();
            }

            if (State.PlayerPicksMarketFirst)
            {
                State.ComputerMarket = _computerDecisionService.ChooseMarket(State.ComputerGuards, State.ComputerGuide, State.UnavailableMarkets, State.ComputerCamelQuality);
                if (State.ComputerMarket != GameState.DoNothingMarket)
                {
                    State.UnavailableMarkets = State.UnavailableMarkets.Append(State.ComputerMarket).ToArray();
                    _messenger.Publish($"You chose {State.PlayerMarket}. Computer then chose {State.ComputerMarket}.");
                }
                else
                {
                    _messenger.Publish($"You chose {State.PlayerMarket}. Computer chose to Do Nothing.");
                }
            }
            else
            {
                _messenger.Publish($"You chose {State.PlayerMarket}.");
            }

            State.ChoosingMarket = false;
            State.CurrentPhase = "end";
            EndRound();
        }

        public void EndRound()
        {
            double riskTolerance = _rand.NextDouble(GameState.MinRiskTolerance, GameState.MaxRiskTolerance);
            double playerProfit = _calculationService.CalculateProfit(State.PlayerMarket, State.PlayerGuards, State.PlayerGuide, GameState.DefaultCaravanValue, State.PlayerCamelQuality);
            double computerProfit = _calculationService.CalculateProfit(State.ComputerMarket, State.ComputerGuards, State.ComputerGuide, GameState.DefaultCaravanValue, State.ComputerCamelQuality);

            State.PlayerGold += playerProfit;
            State.ComputerGold += computerProfit;

            var playerResult = _calculationService.ChooseBestMarket(GameState.DefaultCaravanValue, riskTolerance, State.PlayerGuards, State.PlayerGuide, State.UnavailableMarkets, State.PlayerCamelQuality);
            var computerResult = _calculationService.ChooseBestMarket(GameState.DefaultCaravanValue, riskTolerance, State.ComputerGuards, State.ComputerGuide, State.UnavailableMarkets, State.ComputerCamelQuality);

            _messenger.Publish("Round Summary:");
            _messenger.Publish($"You sent a caravan with {State.PlayerGuards} guards and a {State.PlayerGuide} guide to {State.PlayerMarket}.");
            _messenger.Publish(State.PlayerMarket != GameState.DoNothingMarket ? $"Your caravan earned {playerProfit:F2} gold." : "You stayed home and earned no profit.");
            _messenger.Publish($"The computer sent a caravan with {State.ComputerGuards} guards and a {State.ComputerGuide} guide to {State.ComputerMarket}.");
            _messenger.Publish(State.ComputerMarket != GameState.DoNothingMarket ? $"The computer's caravan earned {computerProfit:F2} gold." : "The computer stayed home and earned no profit.");

            State.RoundNumber++;

            if (State.PlayerGold >= GameState.WinningGold || State.ComputerGold >= GameState.WinningGold)
            {
                State.GameOver = true;
                _messenger.Publish("Game Over!");
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
            State.PlayerGold = GameState.StartingGold;
            State.ComputerGold = GameState.StartingGold;
            State.RoundNumber = GameState.InitialRoundNumber;
            State.GameOver = false;
            StartRound();
        }
    }
}