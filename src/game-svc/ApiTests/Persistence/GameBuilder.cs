using Api.Persistence;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiTests.Persistence
{
    public class GameBuilder
    {
        private Game _game = new Game()
        {
            Players = new List<Game.Player>()
        };

        public static readonly Game.Player Einstein = new Game.Player()
        {
            ID = PlayerData.Einstein.ID,
            Name = PlayerData.Einstein.Name,
            AcceptedAt = DateTime.UtcNow
        };
        public static readonly Game.Player Eiffel = new Game.Player()
        {
            ID = PlayerData.Eiffel.ID,
            Name = PlayerData.Eiffel.Name,
            AcceptedAt = DateTime.UtcNow
        };

        private GameBuilder SetType(GameType gameType) { _game.GameType = gameType; return this; }
        private GameBuilder SetStatus(GameStatus status) { _game.Status = status; return this; }
        private GameBuilder AddPlayer(Game.Player player) { _game.Players.Add(player); return this; }

        public GameBuilder TicTacToe => SetType(GameType.TicTacToe);
        public GameBuilder GooseGame => SetType(GameType.GooseGame);
        public GameBuilder CardBattle => SetType(GameType.CardBattle);

        public GameBuilder InGame => SetStatus(GameStatus.InGame);
        public GameBuilder TimedOut => SetStatus(GameStatus.TimedOut);
        public GameBuilder Finished => SetStatus(GameStatus.Finished);

        public GameBuilder WithPlayerEinstein => AddPlayer(Einstein);
        public GameBuilder WithPlayerEiffel => AddPlayer(Eiffel);

        public GameBuilder Open { get { _game.IsOpen = true; return this; } }

        public GameBuilder MinMaxPlayers(int min, int max)
        {
            _game.MinPlayers = min;
            _game.MaxPlayers = max;
            return this;
        }

        public Game Build()
        {
            return new Game()
            {
                ID = _game.ID,
                GameType = _game.GameType,
                Status = _game.Status,
                MinPlayers = _game.MinPlayers,
                MaxPlayers = _game.MaxPlayers,
                Players = _game.Players.ToList(),
                PlayersCount = _game.Players.Count,
                IsOpen = _game.IsOpen
            };
        }

        public static implicit operator Game(GameBuilder builder)
        {
            return builder.Build();
        }
    }
}
