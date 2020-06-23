using Api.Persistence;
using AutoMapper;
using System;

namespace ApiTests.Persistence
{
    public class TicTacToeBuilder
    {
        private readonly IMapper _mapper;
        public GameBuilder _gameBuilder = new GameBuilder();
        private TicTacToe.CellData?[] _steps = new TicTacToe.CellData?[TicTacToe.CellCount];

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

        public TicTacToeBuilder Steps(TicTacToe.CellData?[] steps)
        {
            if (steps.Length != TicTacToe.CellCount) throw new Exception($"Cell count should be {TicTacToe.CellCount}");
            _steps = steps;
            return this;
        }

        public TicTacToe Build()
        {
            var game = _gameBuilder.Build();
            var tictactoe = _mapper.Map<Game, TicTacToe>(game);
            tictactoe.Cells = _steps;
            return tictactoe;
        }

        public static implicit operator TicTacToe(TicTacToeBuilder builder)
        {
            return builder.Build();
        }
    }
}
