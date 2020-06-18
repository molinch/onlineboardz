namespace Api.Persistence
{
    public class TicTacToeGame: Game
    {
        public bool?[] Steps { get; set; } = new bool?[9];
    }
}
