using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Linq;

namespace Api.Domain
{
    public class TicTacToe: Game
    {
        public const byte CellCount = 9;
        public CellData?[] Cells { get; set; } = new CellData?[CellCount];

        [BsonIgnore]
        public int TickedCellsCount => CellCount - EmptyCellsCount;

        [BsonIgnore]
        public int EmptyCellsCount => Cells.Count(c => c == null);

        [BsonIgnore]
        public bool AllCellsTicked => EmptyCellsCount == 0;

        [BsonIgnore]
        public Player NextPlayer
        {
            get
            {
                if (Status != GameStatus.InGame) throw new InvalidOperationException("The game cannot have a next player when it's not in game");
                var nextPlayOrder = (CellCount - EmptyCellsCount) % 2;
                return Players.First(p => p.PlayOrder == nextPlayOrder);
            }
        }

        public class CellData
        {
            public int Number { get; set; }
        }

        private static readonly int[][] WinningCells = new[]
        {
            new[] { 0, 1, 2 },
            new[] { 3, 4, 5 },
            new[] { 6, 7, 8 },
            new[] { 0, 3, 6 },
            new[] { 1, 4, 7 },
            new[] { 2, 5, 8 },
            new[] { 0, 4, 8 },
            new[] { 2, 4, 6 },
        };

        public bool HasWon()
        {
            var cells = Cells!;
            foreach (var line in WinningCells)
            {
                var a = line[0];
                var b = line[1];
                var c = line[2];

                if (cells[a] != null
                    && cells[a]?.Number % 2 == cells[b]?.Number % 2
                    && cells[a]?.Number % 2 == cells[c]?.Number % 2)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
