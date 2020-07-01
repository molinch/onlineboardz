using System.Threading.Tasks;

namespace Api.SignalR
{
    public interface IGameHubSender
    {
        Task CustomAsync<TIn, TOut>(string method, TIn game)
            where TIn : Domain.Game
            where TOut : TransferObjects.Game;
        Task GameStartedAsync(Domain.Game game);
        Task PlayerAddedAsync(Domain.Game game);
    }
}