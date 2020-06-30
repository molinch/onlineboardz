using ApiTests.Persistence;
using FluentAssertions;
using Xunit;

namespace ApiTests.Domain
{
    public class TicTacToeTests
    {
        [Fact]
        public void Should_get_empty_cells()
        {
            var tictactoe = new TicTacToeBuilder()
                .TickCell(0)
                .TickCell(1)
                .Build();

            // Act
            var emptyCells = tictactoe.EmptyCellsCount;

            emptyCells.Should().Be(7);
        }

        [Fact]
        public void Should_determine_first_player()
        {
            var tictactoe = new TicTacToeBuilder()
                .Game(g => g.FirstPlayerEinstein.SecondPlayerEiffel.InGame)
                .Build();

            // Act
            var player = tictactoe.NextPlayer;

            player.ID.Should().Be(GameBuilder.Einstein.ID);
        }

        [Fact]
        public void Should_determine_next_player()
        {
            var tictactoe = new TicTacToeBuilder()
                .Game(g => g.FirstPlayerEinstein.SecondPlayerEiffel.InGame)
                .TickCell(0)
                .TickCell(1)
                .TickCell(2)
                .Build();

            // Act
            var player = tictactoe.NextPlayer;

            player.ID.Should().Be(GameBuilder.Eiffel.ID);
        }

        [Theory]
        [InlineData(0, 1, 2)]
        [InlineData(3, 4, 5)]
        [InlineData(6, 7, 8)]
        [InlineData(0, 3, 6)]
        [InlineData(1, 4, 7)]
        [InlineData(2, 5, 8)]
        [InlineData(0, 4, 8)]
        [InlineData(2, 4, 6)]
        public void Should_be_won(int a, int b, int c)
        {
            var tictactoe = new TicTacToeBuilder()
                .Game(g => g.FirstPlayerEinstein.SecondPlayerEiffel)
                .TickCell(a)
                .TickOtherCell(a, b, c)
                .TickCell(b)
                .TickOtherCell(a, b, c)
                .TickCell(c)
                .TickOtherCell(a, b, c)
                .Build();

            // Act
            var won = tictactoe.HasWon();

            won.Should().BeTrue();
        }

        [Theory]
        [InlineData(0, 2, 3)]
        [InlineData(3, 6, 8)]
        public void Should_not_be_won(int a, int b, int c)
        {
            var tictactoe = new TicTacToeBuilder()
                .Game(g => g.FirstPlayerEinstein.SecondPlayerEiffel)
                .TickCell(a)
                .TickOtherCell(a, b, c)
                .TickCell(b)
                .TickOtherCell(a, b, c)
                .TickCell(c)
                .TickOtherCell(a, b, c)
                .Build();

            // Act
            var won = tictactoe.HasWon();

            won.Should().BeFalse();
        }

        [Fact]
        public void Should_not_be_won_when_nothing_set()
        {
            var tictactoe = new TicTacToeBuilder()
                .Game(g => g.FirstPlayerEinstein.SecondPlayerEiffel)
                .Build();

            // Act
            var won = tictactoe.HasWon();

            won.Should().BeFalse();
        }
    }
}
