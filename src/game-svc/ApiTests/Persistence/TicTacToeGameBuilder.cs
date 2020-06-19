using Api.Persistence;
using AutoMapper;
using System;

namespace ApiTests.Persistence
{
    public class TicTacToeGameBuilder
    {
        private readonly IMapper _mapper;
        public GameBuilder _gameBuilder = new GameBuilder();

        public TicTacToeGameBuilder()
        {
            _mapper = new MapperConfiguration(c =>
                c.CreateMap<Game, TicTacToeGame>()).CreateMapper();
        }

        public TicTacToeGameBuilder Game(Func<GameBuilder, GameBuilder> withGameBuilder)
        {
            withGameBuilder(_gameBuilder);
            return this;
        }

        public TicTacToeGame Build()
        {
            var game = _gameBuilder.Build();
            return _mapper.Map<Game, TicTacToeGame>(game);
        }

        public static implicit operator TicTacToeGame(TicTacToeGameBuilder builder)
        {
            return builder.Build();
        }
    }
}
