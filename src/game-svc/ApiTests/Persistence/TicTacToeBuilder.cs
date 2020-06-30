using Api.Domain;
using AutoMapper;
using System;
using System.Linq;

namespace ApiTests.Persistence
{
    public class TicTacToeBuilder
    {
        private readonly IMapper _mapper;
        public GameBuilder _gameBuilder = new GameBuilder();
        private TicTacToe.CellData?[] _cells = new TicTacToe.CellData?[TicTacToe.CellCount];
        private int _stepNumber = 0;

        public TicTacToeBuilder()
        {
            _mapper = new MapperConfiguration(c =>
                c.CreateMap<Game, TicTacToe>()).CreateMapper();
        }

        public TicTacToeBuilder Game(Func<GameBuilder, GameBuilder> withGameBuilder)
        {
            withGameBuilder(_gameBuilder);
            return this;
        }

        public TicTacToeBuilder TickCell(int cellIndex)
        {
            _cells[cellIndex] = new TicTacToe.CellData() { Number = _stepNumber++ };
            return this;
        }

        public TicTacToeBuilder TickOtherCell(params int[] butNotTheseCellIndexes)
        {
            var random = new Random();
            int cellIndex;
            do
            {
                cellIndex = random.Next(0, 9); // first value is inclusive, second is exclusive
            } while (butNotTheseCellIndexes.Contains(cellIndex));

            _cells[cellIndex] = new TicTacToe.CellData() { Number = _stepNumber++ };
            return this;
        }

        public TicTacToeBuilder Cells(TicTacToe.CellData?[] steps)
        {
            if (steps.Length != TicTacToe.CellCount) throw new Exception($"Cell count should be {TicTacToe.CellCount}");
            _cells = steps;
            return this;
        }

        public TicTacToe Build()
        {
            var game = _gameBuilder.Build();
            var tictactoe = _mapper.Map<Game, TicTacToe>(game);
            tictactoe.Cells = _cells;
            return tictactoe;
        }

        public static implicit operator TicTacToe(TicTacToeBuilder builder)
        {
            return builder.Build();
        }
    }
}
