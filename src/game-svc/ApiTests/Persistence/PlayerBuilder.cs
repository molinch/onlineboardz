using Api.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiTests.Persistence
{
    public class PlayerData
    {
        public static readonly PlayerData Einstein = new PlayerData("Albert Einstein");
        public static readonly PlayerData Eiffel = new PlayerData("Gustave Eiffel");

        public PlayerData(string name)
        {
            Name = name;
        }

        public string ID { get; } = Guid.NewGuid().ToString();
        public string Name { get; }
    }

    public class PlayerBuilder
    {
        private Player _player = new Player()
        {
            Games = new List<Player.Game>()
        };

        private PlayerBuilder FromPlayerData(PlayerData player)
        {
            _player.ID = player.ID;
            _player.Name = player.Name;
            return this;
        }

        public Player.Game ToPlayerGame(Game game)
        {
            return new Player.Game()
            {
                ID = game.ID!,
                Status = game.Status,
                GameType = game.GameType,
                IsOpen = game.IsOpen,
                AcceptedAt = game.Players.Where(p => p.ID == _player.ID).Single().AcceptedAt,
                StartedAt = game.StartedAt,
                EndedAt = game.EndedAt,
            };
        }

        public PlayerBuilder Einstein => FromPlayerData(PlayerData.Einstein);
        public PlayerBuilder Eiffel => FromPlayerData(PlayerData.Eiffel);

        public PlayerBuilder AddGame(Func<GameBuilder,GameBuilder> withBuilder)
        {
            return AddGame(withBuilder(new GameBuilder()));
        }

        public PlayerBuilder AddGame(GameBuilder builder)
        {
            return AddGame(builder.Build());
        }

        public PlayerBuilder AddGame(Game game)
        {
            _player.Games.Add(ToPlayerGame(game));
            return this;
        }

        public Player Build()
        {
            return new Player()
            {
                ID = _player.ID,
                Name = _player.Name,
                Games = _player.Games.ToList()
            };
        }

        public static implicit operator Player(PlayerBuilder builder)
        {
            return builder.Build();
        }
    }
}
