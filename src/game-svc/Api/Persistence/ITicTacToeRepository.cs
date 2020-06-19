using System.Threading.Tasks;

namespace Api.Persistence
{
    public interface ITicTacToeRepository
    {
        Task<TicTacToeGame> SetTicTacToeStepAsync(string gameId, int stepIndex, bool value);
    }
}