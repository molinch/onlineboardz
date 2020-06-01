using Api.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ApiTests.Persistence
{
    public class GameRepositoryTests
    {
        private static readonly Game.GameMetadata.Player Einstein = new Game.GameMetadata.Player()
        {
            ID = Guid.NewGuid().ToString(),
            Name = "Albert Einstein",
            AcceptedAt = DateTime.UtcNow
        };
        private static readonly Game.GameMetadata.Player Eiffel = new Game.GameMetadata.Player()
        {
            ID = Guid.NewGuid().ToString(),
            Name = "Gustave Eiffel",
            AcceptedAt = DateTime.UtcNow
        };

        private readonly IConfigurationRoot _configuration;

        public GameRepositoryTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(GameRepositoryTests).Assembly)
                .AddJsonFile("appsettings.json")
                .Build();
        }

        private GameRepository Repository => new GameRepository(
            new MongoDB.Entities.DB(
                MongoClientSettings.FromConnectionString(_configuration.GetValue<string>("MongoConnectionString")),
                _configuration.GetValue<string>("GameDatabaseName")
            ));

        [Fact]
        public async Task Should_create_game()
        {
            var game = new Game()
            {
                Status = GameStatus.WaitingForPlayers,
                Metadata = new Game.GameMetadata()
                {
                    GameType = GameType.TicTacToe,
                    MinPlayers = 2,
                    MaxPlayers = 4,
                    IsOpen = true,
                    PlayersCount = 1,
                    Players = new List<Game.GameMetadata.Player>()
                    {
                        Einstein
                    }
                }
            };
            var gameSentToDb = game.DeepClone(); // need to clone it since mongodb will alter it (to add id for example)

            // Act
            var createdGame = await Repository.CreateAsync(gameSentToDb);

            createdGame.Should().BeEquivalentTo(game, options => options.Excluding(e => e.ID).Excluding(e => e.ModifiedOn));
            createdGame.ID.Should().NotBeNullOrWhiteSpace();
            createdGame.ModifiedOn.Should().BeBefore(DateTime.UtcNow.AddHours(1));
            createdGame.ModifiedOn.Should().BeAfter(DateTime.UtcNow.AddHours(-1));
        }

        [Fact]
        public async Task Should_be_true_when_already_in_game()
        {
            var game = new Game()
            {
                Status = GameStatus.WaitingForPlayers,
                Metadata = new Game.GameMetadata()
                {
                    GameType = GameType.TicTacToe,
                    MinPlayers = 2,
                    MaxPlayers = 4,
                    IsOpen = true,
                    PlayersCount = 1,
                    Players = new List<Game.GameMetadata.Player>()
                    {
                        Einstein
                    }
                }
            };
            game = await Repository.CreateAsync(game);

            // Act
            var isInGame = await Repository.IsAlreadyInGamesAsync(Einstein.ID);

            isInGame.Should().BeTrue();
        }

        [Fact]
        public async Task Should_be_false_when_not_already_in_game()
        {
            var newPlayerId = Guid.NewGuid().ToString();

            // Act
            var isInGame = await Repository.IsAlreadyInGamesAsync(newPlayerId);

            isInGame.Should().BeFalse();
        }

        [Fact]
        public async Task Should_add_player_when_not_there()
        {
            var game = new Game()
            {
                Status = GameStatus.WaitingForPlayers,
                Metadata = new Game.GameMetadata()
                {
                    GameType = GameType.TicTacToe,
                    MinPlayers = 2,
                    MaxPlayers = 4,
                    IsOpen = true,
                    PlayersCount = 1,
                    Players = new List<Game.GameMetadata.Player>()
                    {
                        Einstein
                    }
                }
            };
            game = await Repository.CreateAsync(game);

            // Act
            var updatedGame = await Repository.AddPlayerIfNotThereAsync(game.ID, Eiffel);

            updatedGame.Metadata.PlayersCount.Should().Be(2);
            updatedGame.Metadata.Players.Should().BeEquivalentTo(new[]
            {
                Einstein,
                Eiffel
            }, options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1)) // Mongo slightly changes the datetime
                .WhenTypeIs<DateTime>());
        }

        [Fact]
        public async Task Should_not_add_player_when_already_there()
        {
            var game = new Game()
            {
                Status = GameStatus.WaitingForPlayers,
                Metadata = new Game.GameMetadata()
                {
                    GameType = GameType.TicTacToe,
                    MinPlayers = 2,
                    MaxPlayers = 4,
                    IsOpen = true,
                    PlayersCount = 1,
                    Players = new List<Game.GameMetadata.Player>()
                    {
                        Einstein
                    }
                }
            };
            game = await Repository.CreateAsync(game);
            await Repository.AddPlayerIfNotThereAsync(game.ID, Eiffel);

            // Act
            var updatedGame = await Repository.AddPlayerIfNotThereAsync(game.ID, Eiffel);

            updatedGame.Should().BeNull();
        }
    }
}
