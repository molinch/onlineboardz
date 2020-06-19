using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public interface IGameRepository
    {
        Task<Game?> AddPlayerIfNotThereAsync(string id, Game.Player player);
        Task<Game?> AddPlayerToGameAsync(GameType gameType, int? maxPlayers, int? duration, Game.Player player);
        Task<TGame> CreateGameAsync<TGame>(TGame game) where TGame : Game;
        Task<Player> CreatePlayerAsync(Player player);
        Task<Game?> GetAsync(string id);
        Task<int> GetNumberOfGamesAsync(string playerId);
        Task<IEnumerable<Game>> GetPlayableGamesAsync(string playerId, IEnumerable<GameType> gameTypes, IEnumerable<GameStatus> statuses);
        Task<IEnumerable<Player.Game>> GetPlayerGamesAsync(string playerId);
        Task RemoveAsync(string id);
        Task<Game?> StartGameAsync(string id, IReadOnlyList<int> playerOrders);
        Task AddOrUpdatePlayerGameAsync(Player player, Player.Game game);
    }
}