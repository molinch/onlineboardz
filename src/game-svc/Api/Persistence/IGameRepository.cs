using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public interface IGameRepository
    {
        Task<Game> AddPlayerIfNotThereAsync(string id, Game.GameMetadata.Player player);
        Task<Game> CreateAsync(Game room);
        Task<IEnumerable<Game>> GetAsync();
        Task<Game> GetAsync(string id);
        Task<bool> IsAlreadyInGamesAsync(string playerId);
        Task RemoveAsync(string id);
        Task<Game> SetGameStatus(string id, GameStatus oldStatus, GameStatus newStatus);
    }
}