// Most likely there's a better way to do that, than listing all images here...
import tictactoeLogo from './TicTacToe/logo.svg';
import memoryLogo from './Memory/logo.jpg';
import notfound from './notfound.png';

const GameType =
{
    Unknown: 0,
    TicTacToe: 1,
    Memory: 2,
    GooseGame: 3,
    FindSameAndTapIt: 4,
    FindStorytellerCard: 5,
    CardBattle: 6,
    Scrabble: 7,
};
Object.freeze(GameType);

const GameTypeInfos = [
    { type: GameType.Unknown, name: "Unknown", logo: notfound },
    { type: GameType.TicTacToe, name: "TicTacToe", logo: tictactoeLogo },
    { type: GameType.Memory, name: "Memory", logo: memoryLogo },
    { type: GameType.GooseGame, name: "GooseGame", logo: notfound },
    { type: GameType.FindSameAndTapIt, name: "FindSameAndTapIt", logo: notfound },
    { type: GameType.FindStorytellerCard, name: "FindStorytellerCard", logo: notfound },
    { type: GameType.CardBattle, name: "CardBattle", logo: notfound },
    { type: GameType.Scrabble, name: "Scrabble", logo: notfound },
];
Object.freeze(GameTypeInfos);

const GameTypeInfo = {
    ById: id => GameTypeInfos[id],
    ByName: name => GameTypeInfos.find(g => g.name === name),
};
Object.freeze(GameTypeInfo);

export { GameType, GameTypeInfo };