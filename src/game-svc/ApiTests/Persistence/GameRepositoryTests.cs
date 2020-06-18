using Api.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ApiTests.Persistence
{
    public class GameRepositoryTests : IDisposable
    {
        private static readonly Random _random = new Random();
        private readonly IConfigurationRoot _configuration;
        private readonly string _dbName;
        private readonly MongoClientSettings _mongoClientSettings;
        private readonly GameRepository _repository;

        public GameRepositoryTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(GameRepositoryTests).Assembly)
                .AddJsonFile("appsettings.json")
                .Build();

            // The idea is that each test run gets his own fresh database (dropped during Dispose)
            // This way there is no state shared between the tests
            // Note: seems like Mongo has an inmemory feature that we could leverage too
            _dbName = "GameDbTest-" + _random.Next();
            _mongoClientSettings = MongoClientSettings.FromConnectionString(_configuration.GetValue<string>("MongoConnectionString"));
            _repository = new GameRepository(
                new DB(
                    _mongoClientSettings,
                    _dbName
                ));
        }

        public void Dispose()
        {
            var client = new MongoClient(_mongoClientSettings);
            client.DropDatabase(_dbName);
        }

        [Fact]
        public async Task Should_create_game()
        {
            var builder = new GameBuilder()
                .TicTacToe
                .MinMaxPlayers(2, 4)
                .Open
                .WithPlayerEinstein;

            var game = builder.Build();
            var gameSentToDb = builder.Build(); // need to get another one since mongodb will alter it (to add id for example)

            // Act
            var createdGame = await _repository.CreateAsync(gameSentToDb);

            createdGame.Should().BeEquivalentTo(game, options => options.Excluding(e => e.ID).Excluding(e => e.ModifiedOn));
            createdGame.ID.Should().NotBeNullOrWhiteSpace();
            createdGame.ModifiedOn.Should().BeBefore(DateTime.UtcNow.AddHours(1));
            createdGame.ModifiedOn.Should().BeAfter(DateTime.UtcNow.AddHours(-1));
        }

        [Fact]
        public async Task Should_get_playable_games()
        {
            var game1 = await _repository.CreateAsync(new GameBuilder()
                .GooseGame
                .WithPlayerEiffel
                .Build());
            var game2 = await _repository.CreateAsync(new GameBuilder()
                .CardBattle
                .WithPlayerEiffel
                .Build());
            var game3 = await _repository.CreateAsync(new GameBuilder()
                .TimedOut // should not be picked since it's not waiting for players
                .WithPlayerEiffel
                .Build());
            var game4 = await _repository.CreateAsync(new GameBuilder()
                .CardBattle
                .WithPlayerEinstein // should not be picked since Einstein is already in the game
                .Build());

            // Act
            var games = await _repository.GetPlayableGamesAsync(
                GameBuilder.Einstein.ID,
                new[] { GameType.CardBattle, GameType.GooseGame },
                new[] { GameStatus.WaitingForPlayers });

            games.Should().BeEquivalentTo(new[] { game1, game2 }, options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1)) // Mongo slightly changes the datetime
                .WhenTypeIs<DateTime>());
        }

        [Fact]
        public async Task Should_count_be_two_when_already_in_two_games()
        {
            var game1 = new GameBuilder()
                .WithPlayerEinstein
                .Build();
            await _repository.CreateAsync(game1);
            var game2 = new GameBuilder()
                .WithPlayerEinstein
                .Build();
            await _repository.CreateAsync(game2);

            // Act
            var numberOfGames = await _repository.GetNumberOfGamesAsync(GameBuilder.Einstein.ID);

            numberOfGames.Should().Be(2);
        }

        [Fact]
        public async Task Should_count_be_zero_when_not_in_any_game()
        {
            var newPlayerId = Guid.NewGuid().ToString();

            // Act
            var numberOfGames = await _repository.GetNumberOfGamesAsync(newPlayerId);

            numberOfGames.Should().Be(0);
        }

        [Fact]
        public async Task Should_add_player_when_not_there()
        {
            var game = new GameBuilder()
                .TicTacToe
                .WithPlayerEinstein
                .Build();
            game = await _repository.CreateAsync(game);

            // Act
            var updatedGame = await _repository.AddPlayerIfNotThereAsync(game.ID, GameBuilder.Eiffel);

            updatedGame.Should().NotBeNull();
            updatedGame!.PlayersCount.Should().Be(2);
            updatedGame.Players.Should().BeEquivalentTo(new[]
            {
                GameBuilder.Einstein,
                GameBuilder.Eiffel
            }, options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1)) // Mongo slightly changes the datetime
                .WhenTypeIs<DateTime>());
        }

        [Fact]
        public async Task Should_not_add_player_when_already_there()
        {
            var game = new GameBuilder()
                .TicTacToe
                .WithPlayerEiffel
                .Build();
            game = await _repository.CreateAsync(game);
            await _repository.AddPlayerIfNotThereAsync(game.ID, GameBuilder.Eiffel);

            // Act
            var updatedGame = await _repository.AddPlayerIfNotThereAsync(game.ID, GameBuilder.Eiffel);

            updatedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_status_change_to_ingame_when_not_yet_changed()
        {
            var game = new GameBuilder().Build();
            game = await _repository.CreateAsync(game);

            // Act
            game = await _repository.StartGameAsync(game.ID, new int[0]);

            game.Should().NotBeNull();
            game!.Status.Should().Be(GameStatus.InGame);
        }

        [Fact]
        public async Task Should_no_change_status_when_it_was_changed_meanwhile()
        {
            var game = new GameBuilder().InGame.Build();
            game = await _repository.CreateAsync(game);

            // Act: similate that we look for a waiting game, while in db it's already ingame
            game = await _repository.StartGameAsync(game.ID, new int[0]);

            game.Should().BeNull();
        }

        [Fact]
        public async Task Should_get_games_where_player_is_in()
        {
            var player1 = new PlayerBuilder()
                .Eiffel
                .AddGame(new GameBuilder().WithPlayerEiffel)
                .Build();
            await _repository.CreatePlayerAsync(player1);
            var player2 = new PlayerBuilder()
                .Einstein
                .AddGame(new GameBuilder().WithPlayerEinstein)
                .Build();
            await _repository.CreatePlayerAsync(player2);

            // Act
            var games = await _repository.GetPlayerGamesAsync(PlayerData.Einstein.ID);

            games.Should().BeEquivalentTo(player2.Games, options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1)) // Mongo slightly changes the datetime
                .WhenTypeIs<DateTime>());
        }
    }
}
