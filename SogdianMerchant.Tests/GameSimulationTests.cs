namespace SogdianMerchant.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using SogdianMerchant.Core.Services;
    using System;
    using Xunit;

    public class GameServiceFixture : IDisposable
    {
        public IGameService GameService { get; }

        public GameServiceFixture()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IRandomGenerator, RandomGenerator>();
            services.AddSingleton<ICalculationService, CalculationService>();
            services.AddSingleton<IComputerDecisionService, ComputerDecisionService>();
            services.AddSingleton<IGameService, GameService>();
            var provider = services.BuildServiceProvider();
            GameService = provider.GetRequiredService<IGameService>();
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }

    public class GameSimulationTests : IClassFixture<GameServiceFixture>
    {
        private readonly IGameService _gameService;

        public GameSimulationTests(GameServiceFixture fixture)
        {
            _gameService = fixture.GameService;
        }

        [Fact]
        public void SimulateGameRound()
        {
            _gameService.StartRound();

            // Simulate guards phase
            _gameService.State.ChoosingGuards = true;
            _gameService.State.CurrentPhase = "guards";
            _gameService.State.AvailableGuards = 5;
            _gameService.State.GuardInput = 3;
            _gameService.SubmitGuards();

            // Simulate guide phase
            _gameService.State.ChoosingGuide = true;
            _gameService.State.CurrentPhase = "guide";
            _gameService.State.NoviceAvailable = true;
            _gameService.State.VeteranAvailable = false;
            _gameService.State.GuideInput = "Novice";
            _gameService.SubmitGuide();

            // Simulate market phase
            _gameService.State.ChoosingMarket = true;
            _gameService.State.CurrentPhase = "market";
            //_gameService.State.AvailableMarkets = new[] { "MarketA", "MarketB" };
            _gameService.State.MarketInput = "Bukhara Market";
            _gameService.SubmitMarket();

            // End phase
            _gameService.State.CurrentPhase = "end";
            _gameService.ContinueToNextRound();

            // Assert game progressed
            Assert.Equal(2, _gameService.State.RoundNumber);
            Assert.False(_gameService.State.GameOver);
        }
    }
}