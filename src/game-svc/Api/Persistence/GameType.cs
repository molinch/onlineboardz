using System.Collections.Generic;
using System.Linq;

namespace Api.Persistence
{
    public enum GameType
    {
        Unknown = 0,
        TicTacToe = 1,
        Memory = 2,
        GooseGame = 3,
        FindSameAndTapIt = 4,
        FindStorytellerCard = 5, // storyteller gives a hint, then others should find it // https://unsplash.com/s/photos/random
        CardBattle,
        Scrabble
    }

    public class GameTypeMetadata
    {
        public static readonly GameTypeMetadata TicTacToe = new GameTypeMetadata(GameType.TicTacToe, 2, 2, 5);
        public static readonly GameTypeMetadata Memory = new GameTypeMetadata(GameType.Memory, 1, 2, 5);
        public static readonly GameTypeMetadata GooseGame = new GameTypeMetadata(GameType.GooseGame, 2, 6, 20);
        public static readonly GameTypeMetadata FindSameAndTapIt = new GameTypeMetadata(GameType.FindSameAndTapIt, 2, 6, 10);
        public static readonly GameTypeMetadata FindStorytellerCard = new GameTypeMetadata(GameType.FindStorytellerCard, 3, 6, 15);
        public static readonly GameTypeMetadata CardBattle = new GameTypeMetadata(GameType.CardBattle, 2, 2, 15);
        public static readonly GameTypeMetadata Scrabble = new GameTypeMetadata(GameType.Scrabble, 2, 2, 20);

        public static readonly IReadOnlyDictionary<GameType, GameTypeMetadata> All = new[]
        {
            TicTacToe,
            Memory,
            GooseGame,
            FindSameAndTapIt,
            FindStorytellerCard,
            CardBattle,
            Scrabble
        }.ToDictionary(g => g.GameType, g => g);

        public GameTypeMetadata(GameType gameType, int minPlayers, int maxPlayers, int defaultDuration)
        {
            GameType = gameType;
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
            DefaultDuration = defaultDuration;
        }

        public GameType GameType { get; }
        public int MinPlayers { get; }
        public int MaxPlayers { get; }
        public int DefaultDuration { get; }
    }
}
