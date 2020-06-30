using Api.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public interface IGameRepository
    {
        Task AddOrUpdatePlayerGameAsync(string playerId, Player.Game game);
        Task<Game?> AddPlayerIfNotThereAsync(string id, Game.Player player);
        Task<Game?> AddPlayerToGameAsync(GameType gameType, int? maxPlayers, int? duration, Game.Player player);
        Task<TGame> CreateGameAsync<TGame>(TGame game) where TGame : Game;
        Task<Player?> CreatePlayerIfNotThereAsync(Player player);
        Task<TGame?> GetAsync<TGame>(string id) where TGame : Game;
        Task<int> GetNumberOfGamesAsync(string playerId);
        Task<IEnumerable<Game>> GetPlayableGamesAsync(string playerId, IEnumerable<GameType> gameTypes, IEnumerable<GameStatus> statuses);
        Task<IEnumerable<Player.Game>> GetPlayerGamesAsync(string playerId);
        Task RemoveAsync(string id);
        Task<Game?> StartGameAsync(string id, IReadOnlyList<int> playerOrders);
    }
}