using Api.Domain;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public interface ITicTacToeRepository
    {
        Task SetTicTacToeStepAsync(TicTacToe game, int cellIndex);
    }
}