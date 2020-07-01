using AutoMapper;

namespace Api.TransferObjects
{
    public class TransferObjectProfile: Profile
    {
        public TransferObjectProfile()
        {
            CreateMap<Domain.Game.Player, Game.Player>();
            CreateMap<Domain.Game, Game>()
                .Include<Domain.TicTacToe, TicTacToe>();
            CreateMap<Domain.Player.Game, Player.Game>();
            CreateMap<Domain.Player, Player>();
            CreateMap<Domain.TicTacToe.CellData, TicTacToe.CellData>();
            CreateMap<Domain.TicTacToe, TicTacToe>();
        }
    }
}
