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
            var game = new TicTacToeGameBuilder()
                .Game(b => b
                    .TicTacToe
                    .WithPlayerEinstein
                    .WithPlayerEiffel
                    .RandomId)
                .Build();
            var savedGame = await _gameRepository.CreateGameAsync(game);

            // Act
            var updatedGamed = await _ticTacToeRepository.SetTicTacToeStepAsync(savedGame.ID!, 2, true);

            updatedGamed.Steps.Should().BeEquivalentTo(new bool?[]
            {
                null,
                null,
                true,
                null,
                null,
                null,
                null,
                null,
                null
            }, options => options.WithStrictOrdering());
        }
    }
}
