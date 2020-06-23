using Api.Persistence;
using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ApiTests.Persistence
{
    public class TicTacToeRepositoryTests : RepositoryTests
    {
        private readonly GameRepository _gameRepository;
        private readonly TicTacToeRepository _ticTacToeRepository;

        public TicTacToeRepositoryTests()
        {
            _gameRepository = new GameRepository(
                new DB(
                    _mongoClientSettings,
                    _dbName
                ));
            _ticTacToeRepository = new TicTacToeRepository(
                new DB(
                    _mongoClientSettings,
                    _dbName
                ));
        }

        [Fact]
        public async Task Should_set_step()
        {
            var game = new TicTacToeBuilder()
                .Game(b => b
                    .TicTacToe
                    .WithPlayerEinstein
                    .WithPlayerEiffel
                    .RandomId)
                .Build();
            var savedGame = await _gameRepository.CreateGameAsync(game);
            var nextPlayerId = Guid.NewGuid().ToString();

            // Act
            var updatedGame = await _ticTacToeRepository.SetTicTacToeStepAsync(nextPlayerId, savedGame.ID!, 2, true, 3, GameStatus.InGame);

            updatedGame.Cells.Should().BeEquivalentTo(new TicTacToe.CellData?[]
            {
                null,
                null,
                new TicTacToe.CellData { Step = true, Number = 3 },
                null,
                null,
                null,
                null,
                null,
                null
            }, options => options.WithStrictOrdering());
            updatedGame.NextPlayerId.Should().Be(nextPlayerId);
            updatedGame.Status.Should().Be(GameStatus.InGame);
        }

        [Fact]
        public async Task Should_not_set_step_when_not_ingame()
        {
            var game = new TicTacToeBuilder()
                .Game(b => b
                    .TicTacToe
                    .WithPlayerEinstein
                    .WithPlayerEiffel
                    .Finished
                    .RandomId)
                .Build();
            var savedGame = await _gameRepository.CreateGameAsync(game);
            var nextPlayerId = Guid.NewGuid().ToString();

            // Act
            await Assert.ThrowsAsync<UpdateException>(() => _ticTacToeRepository.SetTicTacToeStepAsync(nextPlayerId, savedGame.ID!, 2, true, 3, GameStatus.InGame));
        }

        [Fact]
        public async Task Should_not_set_step_when_already_set()
        {
            var game = new TicTacToeBuilder()
                .Game(b => b
                    .TicTacToe
                    .WithPlayerEinstein
                    .WithPlayerEiffel
                    .RandomId)
                .Steps(new TicTacToe.CellData?[]
                {
                    null,
                    null,
                    new TicTacToe.CellData { Step = true, Number = 3 },
                    null,
                    null,
                    null,
                    null,
                    null,
                    null
                })
                .Build();
            var savedGame = await _gameRepository.CreateGameAsync(game);
            var nextPlayerId = Guid.NewGuid().ToString();

            // Act
            await Assert.ThrowsAsync<UpdateException>(() => _ticTacToeRepository.SetTicTacToeStepAsync(nextPlayerId, savedGame.ID!, 2, true, 3, GameStatus.InGame));
        }
    }
}
