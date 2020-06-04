namespace Api.Persistence
{
    public enum GameType
    {
        Unknown = 0,
        TicTacToe = 1,
        Memory = 2,
        SnakeAndLadders = 3, // Goose game
        FindSameAndTapIt = 4, // if
        FindStorytellerCard = 5, // storyteller gives a hint, then others should find it // https://unsplash.com/s/photos/random
        CardBattle,
        Scrabble
    }
}
