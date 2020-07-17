using Api.Domain;
using Api.Persistence;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace ApiTests.Persistence
{
    public class TicTacToeRepositoryTests : RepositoryTests
    {
        protected IGameRepository _gameRepository;
        protected ITicTacToeRepository _ticTacToeRepository;

        public TicTacToeRepositoryTests(): base()
        {
            _gameRepository = new GameRepository(_database);
            _ticTacToeRepository = new TicTacToeRepository(_database);
        }

        [Fact]
        public async Task Should_set_step()
        {
            var game = new TicTacToeBuilder()
                .Game(b => b
                    .TicTacToe
                    .FirstPlayerEinstein
                    .SecondPlayerEiffel
                    .RandomId
                    .InGame)
                .TickCell(2)
                .Build();
            var savedGame = await _gameRepository.CreateGameAsync(game);

            // Act
            await _ticTacToeRepository.SetTicTacToeStepAsync(game, 5);
            var updatedGame = await _gameRepository.GetAsync<TicTacToe>(game.Id!);

            updatedGame.Should().NotBeNull();
            updatedGame!.Cells.Should().BeEquivalentTo(new TicTacToe.CellData?[]
            {
                null,
                null,
                new TicTacToe.CellData { Number = 0 },
                null,
                null,
                new TicTacToe.CellData { Number = 1 },
                null,
                null,
                null
            }, options => options.WithStrictOrdering());
            updatedGame.NextPlayer.Id.Should().Be(GameBuilder.Einstein.Id);
            updatedGame.Status.Should().Be(GameStatus.InGame);
            updatedGame.Version.Should().Be(game.Version + 1);
            updatedGame.EndedAt.Should().BeNull();
        }

        [Fact]
        public async Task Should_not_set_step_when_not_ingame()
        {
            var game = new TicTacToeBuilder()
                .Game(b => b
                    .TicTacToe
                    .FirstPlayerEinstein
                    .SecondPlayerEiffel
                    .Finished
                    .RandomId)
                .Build();
            var savedGame = await _gameRepository.CreateGameAsync(game);

            // Act
            await Assert.ThrowsAsync<UpdateException>(() => _ticTacToeRepository.SetTicTacToeStepAsync(savedGame, 1));
        }

        [Fact]
        public async Task Should_not_set_step_when_already_set()
        {
            var game = new TicTacToeBuilder()
                .Game(b => b
                    .TicTacToe
                    .FirstPlayerEinstein
                    .SecondPlayerEiffel
                    .RandomId
                    .InGame)
                .TickCell(2)
                .Build();
            var savedGame = await _gameRepository.CreateGameAsync(game);

            // Act
            await Assert.ThrowsAsync<UpdateException>(() => _ticTacToeRepository.SetTicTacToeStepAsync(savedGame, 2));
        }

        [Fact]
        public async Task Should_set_step_when_won()
        {
            var game = new TicTacToeBuilder()
                .Game(b => b
                    .TicTacToe
                    .FirstPlayerEinstein
                    .SecondPlayerEiffel
                    .RandomId
                    .InGame)
                .Build();
            var savedGame = await _gameRepository.CreateGameAsync(game);
            savedGame.Status = GameStatus.Finished;
            savedGame.Players[0].Status = PlayerGameStatus.Won;
            savedGame.Players[1].Status = PlayerGameStatus.Lost;

            // Act
            await _ticTacToeRepository.SetTicTacToeStepAsync(savedGame, 2); // with cell 2 we have the row [0, 1, 2]
            var updatedGame = await _gameRepository.GetAsync<TicTacToe>(game.Id!);

            updatedGame.Should().NotBeNull();
            updatedGame!.Status.Should().Be(GameStatus.Finished);
            updatedGame.Version.Should().Be(game.Version + 1);
            updatedGame.EndedAt.Should().NotBeNull();
            updatedGame.Players[0].Status.Should().Be(PlayerGameStatus.Won);
            updatedGame.Players[1].Status.Should().Be(PlayerGameStatus.Lost);
        }
    }
}
