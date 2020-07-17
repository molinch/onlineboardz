using Api.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Thin;

namespace Api.Persistence
{
    public class GameRepository : IGameRepository
    {
        private readonly IMongoCollection<Game> _games;
        private readonly IMongoCollection<Player> _players;
        private const int MaxItemsReturned = 50;

        public GameRepository(IMongoDatabase database)
        {
            _games = database.Collection<Game>();
            _players = database.Collection<Player>();
        }

        public Task<IEnumerable<Player.Game>> GetPlayerGamesAsync(string playerId)
        {
            return GetPlayerGamesAsync(playerId, null);
        }

        public async Task<IEnumerable<Player.Game>> GetPlayerGamesAsync(string playerId, GameStatus? status)
        {
            var query = _players.AsQueryable()
                .Where(p => p.Id == playerId)
                .SelectMany(p => p.Games);

            if (status != null)
            {
                query = query.Where(g => g.Status == status);
            }

            return await query
                .OrderByDescending(p => p.AcceptedAt)
                .Take(MaxItemsReturned)
                .ToListAsync();
        }

        public async Task<bool> CreatePlayerIfNotExistingAsync(Player player)
        {
            var result = await _players
                .UpdateOneAsync(
                    p => p.Id == player.Id,
                    Builders<Player>.Update
                        .SetOnInsert(p => p.Id, player.Id)
                        .SetOnInsert(p => p.Name, player.Name)
                        .SetOnInsert(p => p.SchemaVersion, player.SchemaVersion)
                        .SetOnInsert(p => p.Games, (IEnumerable<Player.Game>)player.Games ?? Array.Empty<Player.Game>()),
                    new UpdateOptions { IsUpsert = true });

            if (result.MatchedCount == 0)
            {
                if (result.UpsertedId == null) throw new InsertException();
                return true;
            }
            else // already existing
            {
                return false;
            }
        }

        public async Task AddOrUpdatePlayerGameAsync(string playerId, Player.Game game)
        {
            // First try to add it, if not there yet
            var updateResult = await _players.UpdateOne()
                .Filter(b => b.Match(p => p.Id == playerId && !p.Games.Any(g => g.Id == game.Id)))
                .Update(b => b.Modify(p => p.Push(g => g.Games, game)))
                .ExecuteAsync();

            if (updateResult.MatchedCount == 0) // game is already part of Games array, so we need to update it
            {
                var command = _players.UpdateOne()
                .Filter(b => b.Match(p => p.Id == playerId))
                .Options(b => b.WithArrayFilter($"{{ 'x._id' : '{game.Id}' }}"))
                .Update(b => b
                    .Modify($"{{ $set : {{ 'Games.$[x].Status' : {(int)game.Status} }} }}")
                    .Modify($"{{ $set : {{ 'Games.$[x].PlayerGameStatus' : {(int)game.PlayerGameStatus} }} }}"));

                if (game.StartedAt.HasValue)
                {
                    command = command.Update(b => b.Modify($"{{ $set : {{ 'Games.$[x].StartedAt' : '{game.StartedAt.Value.ToIso()}' }} }}"));
                }

                if (game.EndedAt.HasValue)
                {
                    command = command.Update(b => b.Modify($"{{ $set : {{ 'Games.$[x].EndedAt' : '{game.EndedAt.Value.ToIso()}' }} }}"));
                }

                updateResult = await command.ExecuteAsync();

                if (updateResult.MatchedCount == 0)
                {
                    throw new UpdateException();
                }
            }
        }

        public async Task<IEnumerable<Game>> GetPlayableGamesAsync(string playerId, IEnumerable<GameType> gameTypes, IEnumerable<GameStatus> statuses)
        {
            return await _games.AsQueryable()
                .Where(g => !g.Players.Any(p => p.Id == playerId))
                .Where(g => gameTypes.Contains(g.GameType) && statuses.Contains(g.Status))
                .Take(MaxItemsReturned)
                .ToListAsync();
        }

        public async Task<TGame?> GetAsync<TGame>(string id)
            where TGame : Game
        {
            var game = await _games.FindOneAsync(g => g.Id == id);
            if (game != null && !(game is TGame))
            {
                throw new InvalidOperationException($"Cannot cast game to a different type of game: game with '{id}' is a {game.GetType().Name} not a {typeof(TGame).Name}");
            }
            return (TGame?)game;
        }

        public Task<int> GetNumberOfGamesAsync(string playerId)
        {
            return _games.AsQueryable()
                .Where(g => g.Players.Any(p => p.Id == playerId))
                .Where(g => g.Status == GameStatus.WaitingForPlayers || g.Status == GameStatus.InGame)
                .CountAsync();
        }

        public async Task<TGame> CreateGameAsync<TGame>(TGame game)
            where TGame : Game
        {
            await _games.InsertOneAsync(game);
            return game;
        }

        public async Task<Game?> AddPlayerToGameIfNotThereAsync(string id, Game.Player player)
        {
            return await _games.FindOneAndUpdate()
                .Filter(b => b.Match(g => g.Id == id && !g.Players.Any(p => p.Id == player.Id) && g.Status == GameStatus.WaitingForPlayers))
                .Update(b => b
                    .Modify(g => g.Push(g => g.Players, player))
                    .Modify(g => g.Inc(g => g.PlayersCount, 1))
                    .Modify(g => g.Inc(g => g.Version, 1)))
                .ExecuteAsync();
        }

        public async Task<Game?> AddPlayerToGameAsync(GameType gameType, int? maxPlayers, int? duration, Game.Player player)
        {
            var command = _games.FindOneAndUpdate();
            command.Filter(b => b.Match(g =>
                     g.GameType == gameType &&
                     !g.Players.Any(p => p.Id == player.Id) &&
                     g.Status == GameStatus.WaitingForPlayers));

            if (maxPlayers.HasValue)
            {
                command = command.Filter(b => b.Match(g => g.MaxPlayers <= maxPlayers));
            }

            if (duration.HasValue)
            {
                command = command.Filter(b => b.Match(g => g.MaxDuration <= duration));
            }

            var game = await command.Update(b => b
                .Modify(g => g.Push(g => g.Players, player))
                .Modify(g => g.Inc(g => g.PlayersCount, 1))
                .Modify(g => g.Inc(g => g.Version, 1)))
                .ExecuteAsync();

            return game;
        }

        public async Task<Game?> StartGameAsync(string id, IReadOnlyList<int> playerOrders)
        {
            var command = _games.FindOneAndUpdate()
                .Filter(b => b.Match(g => g.Id == id && g.Status == GameStatus.WaitingForPlayers))
                .Update(b => b
                    .Modify(g => g.Status, GameStatus.InGame)
                    .Modify(g => g.StartedAt, DateTime.UtcNow)
                    .Modify(g => g.Inc(g => g.Version, 1)));

            for (var i = 0; i < playerOrders.Count; i++)
            {
                var playOrder = playerOrders[i];
                command = command.Update(b => b.Modify($"{{ $set : {{ 'Players.{i}.PlayOrder' : {playOrder} }} }}"));
            }

            return await command.ExecuteAsync();
        }

        public async Task RemoveAsync(string id) =>
            await _games.DeleteOneAsync(g => g.Id == id);
    }
}
