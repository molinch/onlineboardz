using System.Security.Policy;

namespace Api.Persistence
{
    public class TicTacToe: Game
    {
        public const byte CellCount = 9;

        public string? NextPlayerId { get; set; }
        public CellData?[] Cells { get; set; } = new CellData?[CellCount];

        public class CellData
        {
            public bool Step { get; set; }
            public int Number { get; set; }
        }
    }
}
