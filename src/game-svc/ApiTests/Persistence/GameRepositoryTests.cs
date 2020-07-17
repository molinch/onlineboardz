using Api.Domain;
using Api.Persistence;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ApiTests.Persistence
{
    public class GameRepositoryTests : RepositoryTests
    {
        protected IGameRepository _repository;

        public GameRepositoryTests(): base()
        {
            _repository = new GameRepository(_database);
        }

        [Fact]
        public async Task Should_create_game()
        {
            var builder = new GameBuilder()
                .TicTacToe
                .MinMaxPlayers(2, 4)
                .Open
                .FirstPlayerEinstein;

            var game = builder.Build(); // need to get another one since mongodb will alter it (to add id for example)

            // Act
            var createdGame = await _repository.CreateGameAsync(builder.Build());

            createdGame.Should().BeEquivalentTo(game, options => options.Excluding(e => e.Id));
            createdGame.Id.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Should_get_playable_games()
        {
            var game1 = await _repository.CreateGameAsync(new GameBuilder()
                .GooseGame
                .SecondPlayerEiffel
                .Build());
            var game2 = await _repository.CreateGameAsync(new GameBuilder()
                .CardBattle
                .SecondPlayerEiffel
                .Build());
            var game3 = await _repository.CreateGameAsync(new GameBuilder()
                .TimedOut // should not be picked since it's not waiting for players
                .SecondPlayerEiffel
                .Build());
            var game4 = await _repository.CreateGameAsync(new GameBuilder()
                .CardBattle
                .FirstPlayerEinstein // should not be picked since Einstein is already in the game
                .Build());

            // Act
            var games = await _repository.GetPlayableGamesAsync(
                GameBuilder.Einstein.Id,
                new[] { GameType.CardBattle, GameType.GooseGame },
                new[] { GameStatus.WaitingForPlayers });

            games.Should().BeEquivalentTo(new[] { game1, game2 }, options => options.WithMongoDateTime());
        }

        [Fact]
        public async Task Should_count_be_two_when_already_in_two_games()
        {
            var game1 = new GameBuilder()
                .FirstPlayerEinstein
                .Build();
            await _repository.CreateGameAsync(game1);
            var game2 = new GameBuilder()
                .FirstPlayerEinstein
                .Build();
            await _repository.CreateGameAsync(game2);

            // Act
            var numberOfGames = await _repository.GetNumberOfGamesAsync(GameBuilder.Einstein.Id);

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
        public async Task Should_get_game_by_id()
        {
            var game = new GameBuilder()
                .FirstPlayerEinstein
                .CardBattle
                .Build();
            await _repository.CreateGameAsync(game);
            
            // Act
            var fetchedGame = await _repository.GetAsync<Game>(game.Id!);

            fetchedGame.Should().BeEquivalentTo(game, options => options.WithMongoDateTime());
        }

        [Fact]
        public async Task Should_get_null_when_fetching_unknown_game()
        {
            // Act
            var fetchedGame = await _repository.GetAsync<Game>(ObjectId.GenerateNewId().ToString());

            fetchedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_remove_game()
        {
            var game = new GameBuilder()
                .FirstPlayerEinstein
                .CardBattle
                .Build();
            await _repository.CreateGameAsync(game);

            // Act
            await _repository.RemoveAsync(game.Id!);
            var fetchedGame = await _repository.GetAsync<Game>(game.Id!);

            fetchedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_add_player_when_not_there()
        {
            var game = new GameBuilder()
                .TicTacToe
                .FirstPlayerEinstein
                .Build();
            game = await _repository.CreateGameAsync(game);

            // Act
            var updatedGame = await _repository.AddPlayerToGameIfNotThereAsync(game.Id!, GameBuilder.Eiffel);

            updatedGame.Should().NotBeNull();
            updatedGame!.PlayersCount.Should().Be(2);
            updatedGame.Players.Should().BeEquivalentTo(new[]
            {
                GameBuilder.Einstein,
                GameBuilder.Eiffel
            }, options => options.WithMongoDateTime());
        }

        [Fact]
        public async Task Should_not_add_player_when_already_there()
        {
            var game = new GameBuilder()
                .TicTacToe
                .SecondPlayerEiffel
                .Build();
            game = await _repository.CreateGameAsync(game);

            // Act
            var updatedGame = await _repository.AddPlayerToGameIfNotThereAsync(game.Id!, GameBuilder.Eiffel);

            updatedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_not_add_player_when_game_status_is_not_waiting()
        {
            var game = new GameBuilder()
                .TicTacToe
                .SecondPlayerEiffel
                .Finished
                .Build();
            game = await _repository.CreateGameAsync(game);

            // Act
            var updatedGame = await _repository.AddPlayerToGameIfNotThereAsync(game.Id!, GameBuilder.Einstein);

            updatedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_status_change_to_ingame_when_not_yet_changed()
        {
            var game = new GameBuilder().Build();
            game = await _repository.CreateGameAsync(game);

            // Act
            var updatedGame = await _repository.StartGameAsync(game.Id!, new int[0]);

            updatedGame.Should().NotBeNull();
            updatedGame!.Status.Should().Be(GameStatus.InGame);
            updatedGame.Version.Should().Be(game.Version + 1);
        }

        [Fact]
        public async Task Should_no_change_status_when_it_was_changed_meanwhile()
        {
            var game = new GameBuilder().InGame.Build();
            game = await _repository.CreateGameAsync(game);

            // Act: simulate that we look for a waiting game, while in db it's already ingame
            game = await _repository.StartGameAsync(game.Id!, new int[0]);

            game.Should().BeNull();
        }

        [Fact]
        public async Task Should_players_play_order_change()
        {
            var game = new GameBuilder()
                .SecondPlayerEiffel
                .FirstPlayerEinstein
                .Build();
            game = await _repository.CreateGameAsync(game);

            // Act
            var updatedGame = await _repository.StartGameAsync(game.Id!, new[] { 1, 0 });

            updatedGame.Should().NotBeNull();
            updatedGame!.Players[0].PlayOrder.Should().Be(1);
            updatedGame.Players[1].PlayOrder.Should().Be(0);
            updatedGame.Version.Should().Be(game.Version + 1);
        }

        [Fact]
        public async Task Should_create_player()
        {
            var builder = new PlayerBuilder()
                .Eiffel
                .AddGame(b => b.SecondPlayerEiffel);
            var player = builder.Build(); // need to get another one since mongodb will alter it (to add id for example)

            // Act
            var hasCreated = await _repository.CreatePlayerIfNotExistingAsync(builder);

            hasCreated.Should().BeTrue();
        }

        [Fact]
        public async Task Should_create_player_only_once()
        {
            var builder = new PlayerBuilder()
                .Eiffel
                .AddGame(b => b.SecondPlayerEiffel);
            await _repository.CreatePlayerIfNotExistingAsync(builder);

            // Act
            var hasCreated = await _repository.CreatePlayerIfNotExistingAsync(builder);

            hasCreated.Should().BeFalse();
        }

        [Fact]
        public async Task Should_get_games_where_player_is_in()
        {
            var player1 = new PlayerBuilder()
                .Eiffel
                .AddGame(b => b.SecondPlayerEiffel)
                .Build();
            await _repository.CreatePlayerIfNotExistingAsync(player1);
            var player2 = new PlayerBuilder()
                .Einstein
                .AddGame(b => b.FirstPlayerEinstein)
                .Build();
            await _repository.CreatePlayerIfNotExistingAsync(player2);

            // Act
            var games = await _repository.GetPlayerGamesAsync(PlayerData.Einstein.ID);

            games.Should().BeEquivalentTo(player2.Games, options => options.WithMongoDateTime());
        }

        public async Task Should_get_active_games_only_where_player_is_in()
        {
            var player2 = new PlayerBuilder()
                .Einstein
                .AddGame(b => b.FirstPlayerEinstein.InGame)
                .AddGame(b => b.FirstPlayerEinstein.Finished)
                .Build();
            await _repository.CreatePlayerIfNotExistingAsync(player2);

            // Act
            var games = await _repository.GetPlayerGamesAsync(PlayerData.Einstein.ID, GameStatus.InGame);

            games.Should().BeEquivalentTo(
                player2.Games.Where(g => g.Status == GameStatus.InGame),
                options => options.WithMongoDateTime());
        }

        [Fact]
        public async Task Should_not_add_player_to_game_if_already_in()
        {
            var game = new GameBuilder()
                .FirstPlayerEinstein
                .CardBattle
                .Build();
            await _repository.CreateGameAsync(game);

            // Act
            var fetchedGame = await _repository.AddPlayerToGameAsync(GameType.CardBattle, null, null, GameBuilder.Einstein);

            fetchedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_not_add_player_to_game_when_game_max_is_greather()
        {
            var game = new GameBuilder()
                .FirstPlayerEinstein
                .CardBattle
                .MinMaxPlayers(2, 4)
                .Build();
            await _repository.CreateGameAsync(game);

            // Act
            var fetchedGame = await _repository.AddPlayerToGameAsync(GameType.CardBattle, 3, null, GameBuilder.Eiffel);

            fetchedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_not_add_player_to_game_when_game_duration_is_longer()
        {
            var game = new GameBuilder()
                .FirstPlayerEinstein
                .CardBattle
                .MaxDuration(10)
                .Build();
            await _repository.CreateGameAsync(game);

            // Act
            var fetchedGame = await _repository.AddPlayerToGameAsync(GameType.CardBattle, null, 5, GameBuilder.Eiffel);

            fetchedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_not_add_player_to_game_when_game_type_is_different()
        {
            var game = new GameBuilder()
                .FirstPlayerEinstein
                .TicTacToe
                .Build();
            await _repository.CreateGameAsync(game);

            // Act
            var fetchedGame = await _repository.AddPlayerToGameAsync(GameType.CardBattle, null, null, GameBuilder.Eiffel);

            fetchedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_not_add_player_to_game_when_status_is_not_waiting()
        {
            var game = new GameBuilder()
                .FirstPlayerEinstein
                .CardBattle
                .Finished
                .Build();
            await _repository.CreateGameAsync(game);

            // Act
            var fetchedGame = await _repository.AddPlayerToGameAsync(GameType.CardBattle, null, null, GameBuilder.Eiffel);

            fetchedGame.Should().BeNull();
        }

        [Fact]
        public async Task Should_add_player_to_game()
        {
            var game = new GameBuilder()
                .FirstPlayerEinstein
                .CardBattle
                .Build();
            await _repository.CreateGameAsync(game);

            // Act
            var updatedGame = await _repository.AddPlayerToGameAsync(GameType.CardBattle, null, null, GameBuilder.Eiffel);

            updatedGame.Should().NotBeNull();
            updatedGame!.PlayersCount.Should().Be(2);
            updatedGame.Players.Should().BeEquivalentTo(new[]
            {
                GameBuilder.Einstein,
                GameBuilder.Eiffel
            }, options => options.WithMongoDateTime());
            updatedGame.Version.Should().Be(game.Version + 1);
        }

        [Fact]
        public async Task Should_throw_when_adding_game_to_player_for_unknown_player()
        {
            var player = new PlayerBuilder()
                    .Einstein
                    .Build();
            var game = Player.Game.From(player.Id, new GameBuilder().FirstPlayerEinstein.RandomId);

            // Act
            await Assert.ThrowsAsync<UpdateException>(() => _repository.AddOrUpdatePlayerGameAsync(player.Id, game));
        }

        [Fact]
        public async Task Should_add_game_to_player_when_game_is_not_there()
        {
            var builder = new PlayerBuilder().Einstein;
            var player = builder.Build();
            await _repository.CreatePlayerIfNotExistingAsync(player);
            var game = Player.Game.From(player.Id, new GameBuilder().FirstPlayerEinstein.CardBattle.RandomId);

            // Act
            await _repository.AddOrUpdatePlayerGameAsync(player.Id, game);
            var games = await _repository.GetPlayerGamesAsync(player.Id);

            games.Should().ContainEquivalentOf(game, options => options.WithMongoDateTime());
        }

        [Fact]
        public async Task Should_update_player_game_player_when_already_there()
        {
            var gameBuilder = new GameBuilder()
                .FirstPlayerEinstein
                .TicTacToe
                .RandomId;
            var playerBuilder = new PlayerBuilder()
                    .Einstein
                    .AddGame(gameBuilder);
            var player = playerBuilder.Build();
            await _repository.CreatePlayerIfNotExistingAsync(player);
            gameBuilder.TimedOut.StartedEndedAt(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3));
            var updatedGame = Player.Game.From(player.Id, gameBuilder);

            // Act
            await _repository.AddOrUpdatePlayerGameAsync(player.Id, updatedGame);
            var games = await _repository.GetPlayerGamesAsync(player.Id);

            games.Single().Should().BeEquivalentTo(updatedGame, options => options.WithMongoDateTime());
        }
    }
}
