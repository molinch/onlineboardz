const GameStatus =
{
    WaitingForPlayers: 0,
    InGame: 1,
    TimedOut: 2,
    Finished: 3,
};
Object.freeze(GameStatus);

export default GameStatus;