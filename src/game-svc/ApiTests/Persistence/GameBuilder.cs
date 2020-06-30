using Api.Domain;
using Api.Persistence;
using AutoMapper;
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
            AcceptedAt = DateTime.UtcNow,
            PlayOrder = 0
        };
        public static readonly Game.Player Eiffel = new Game.Player()
        {
            ID = PlayerData.Eiffel.ID,
            Name = PlayerData.Eiffel.Name,
            AcceptedAt = DateTime.UtcNow,
            PlayOrder = 1
        };
        private readonly IMapper _mapper;

        public GameBuilder()
        {
            _mapper = new MapperConfiguration(c =>
                c.CreateMap<Game.Player, Game.Player>()).CreateMapper();
        }

        private GameBuilder SetType(GameType gameType) { _game.GameType = gameType; return this; }
        private GameBuilder SetStatus(GameStatus status) { _game.Status = status; return this; }
        private GameBuilder AddPlayer(Game.Player player, int? playOrder = null) {
            if (playOrder != null)
            {
                player = _mapper.Map<Game.Player, Game.Player>(player); // make a copy since we alter it
                player.PlayOrder = playOrder.Value;
            }
            _game.Players.Add(player);
            return this;
        }

        public GameBuilder TicTacToe => SetType(GameType.TicTacToe);
        public GameBuilder GooseGame => SetType(GameType.GooseGame);
        public GameBuilder CardBattle => SetType(GameType.CardBattle);

        public GameBuilder InGame => SetStatus(GameStatus.InGame);
        public GameBuilder TimedOut => SetStatus(GameStatus.TimedOut);
        public GameBuilder Finished => SetStatus(GameStatus.Finished);

        public GameBuilder FirstPlayerEinstein => AddPlayer(Einstein);
        public GameBuilder SecondPlayerEiffel => AddPlayer(Eiffel);

        public GameBuilder WithFirstPlayerEinstein(Action<Game.Player> withPlayer)
        {
            AddPlayer(Einstein);
            withPlayer(_game.Players.Last());
            return this;
        }

        public GameBuilder WithSecondPlayerEiffel(Action<Game.Player> withPlayer)
        {
            AddPlayer(Eiffel);
            withPlayer(_game.Players.Last());
            return this;
        }

        public GameBuilder Open { get { _game.IsOpen = true; return this; } }

        public GameBuilder RandomId
        {
            get
            {
                _game.ID = ObjectId.GenerateNewId().ToString();
                return this;
            }
        }

        public GameBuilder MinMaxPlayers(int min, int max)
        {
            _game.MinPlayers = min;
            _game.MaxPlayers = max;
            return this;
        }

        public GameBuilder StartedEndedAt(DateTime? startedAt, DateTime? endedAt)
        {
            _game.StartedAt = startedAt;
            _game.EndedAt = endedAt;
            return this;
        }

        public GameBuilder MaxDuration(int maxDuration)
        {
            _game.MaxDuration = maxDuration;
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
                MaxDuration = _game.MaxDuration,
                Players = _game.Players.ToList(),
                PlayersCount = _game.Players.Count,
                IsOpen = _game.IsOpen,
                StartedAt = _game.StartedAt,
                EndedAt = _game.EndedAt,
            };
        }

        public static implicit operator Game(GameBuilder builder)
        {
            return builder.Build();
        }
    }
}
