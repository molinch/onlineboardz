namespace Api.TransferObjects
{
    public class TicTacToe: Game
    {
        public CellData?[] Cells { get; set; } = new CellData?[Domain.TicTacToe.CellCount];

        public class CellData
        {
            public int Number { get; set; }
        }
    }
}
