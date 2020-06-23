using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Api.Persistence
{
    public class GameFactory : IGameFactory
    {
        private readonly Dictionary<GameType, Func<Game>> _constructorByGameType;

        public GameFactory()
        {
            var gameType = typeof(Game);
            var childrenGameTypes = typeof(GameFactory).Assembly.GetTypes().Where(t => t != gameType && gameType.IsAssignableFrom(t));
            _constructorByGameType = childrenGameTypes
                .ToDictionary(t =>
                    (GameType)Enum.Parse(typeof(GameType), t.Name),
                    t => {
                        var constructorInfo = t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Array.Empty<Type>(), null);
                        if (constructorInfo == null) throw new Exception($"Missing public parameterless constructor for {t.Name}");
                        return (Func<Game>)Expression.Lambda(Expression.New(constructorInfo)).Compile();
                    });
        }

        public Game Create(GameType type)
        {
            return _constructorByGameType[type]();
        }
    }
}
