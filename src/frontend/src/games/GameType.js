const GameType =
{
    Unknown: 0,
    TicTacToe: 1,
    Memory: 2,
    SnakeAndLadders: 3,
    FindSameAndTapIt: 4,
    FindStorytellerCard: 5,
    CardBattle: 6,
    Scrabble: 7
};
Object.freeze(GameType);

const GameTypeI18n = Object.keys(GameType).map(k => k);
Object.freeze(GameTypeI18n);

export { GameType, GameTypeI18n };