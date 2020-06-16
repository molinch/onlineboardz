using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public interface IGameRepository
    {
        Task<Game?> AddPlayerIfNotThereAsync(string id, Game.GameMetadata.Player player);
        Task<Game?> AddPlayerToGameAsync(GameType gameType, int? maxPlayers, int? duration, Game.GameMetadata.Player player);
        Task<Game> CreateAsync(Game room);
        Task<IEnumerable<Game>> GetAsync(string playerId, IEnumerable<GameType> gameTypes, IEnumerable<GameStatus> statuses);
        Task<Game?> GetAsync(string id);
        Task<int> GetNumberOfGamesAsync(string playerId);
        Task RemoveAsync(string id);
        Task<Game?> SetGameStatusAsync(string id, GameStatus oldStatus, GameStatus newStatus);
    }
}