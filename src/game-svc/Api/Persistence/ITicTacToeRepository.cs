using System.Threading.Tasks;

namespace Api.Persistence
{
    public interface ITicTacToeRepository
    {
        Task<TicTacToe> SetTicTacToeStepAsync(string? nextPlayerId, string gameId, int stepIndex, bool value, int stepNumber, GameStatus nextStatus);
    }
}