﻿using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public class GameRepository : IGameRepository
    {
        private readonly DB _database;

        public GameRepository(DB database)
        {
            _database = database;
        }

        public async Task<IEnumerable<Player.Game>> GetPlayerGamesAsync(string playerId)
        {
            return await _database.Queryable<Player>()
                .Where(p => p.ID == playerId)
                .SelectMany(p => p.Games)
                .OrderByDescending(p => p.AcceptedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task<Player?> CreatePlayerIfNotThereAsync(Player player)
        {
            var modifiedOn = DateTime.UtcNow;
            var result = await _database.Collection<Player>()
                .UpdateOneAsync(
                    p => p.ID == player.ID,
                    Builders<Player>.Update
                        .SetOnInsert(p => p.ID, player.ID)
                        .SetOnInsert(p => p.ModifiedOn, modifiedOn)
                        .SetOnInsert(p => p.Name, player.Name)
                        .SetOnInsert(p => p.SchemaVersion, player.SchemaVersion)
                        .SetOnInsert(p => p.Games, (IEnumerable<Player.Game>)player.Games ?? Array.Empty<Player.Game>()),
                    new UpdateOptions { IsUpsert = true });

            if (result.MatchedCount == 0)
            {
                if (result.UpsertedId == null) throw new InsertException();
                player.ModifiedOn = modifiedOn;
                return player;
            }
            else // already existing
            {
                return null;
            }
        }

        public async Task AddOrUpdatePlayerGameAsync(Player player, Player.Game game)
        {
            // First try to add it, if not there yet
            var updateResult = await _database.Update<Player>()
                .Match(p => p.ID == player.ID && !p.Games.Any(g => g.ID == game.ID))
                .Modify(p => p.Push(g => g.Games, game))
                .ExecuteAsync();

            if (updateResult.MatchedCount == 0) // game is already part of Games array, so we need to update it
            {
                var update = _database.Update<Player>()
                  .Match(p => p.ID == player.ID)
                  .WithArrayFilter($"{{ 'x._id' : '{game.ID}' }}")
                  .Modify($"{{ $set : {{ 'Games.$[x].Status' : {(int)game.Status} }} }}");

                if (game.StartedAt.HasValue)
                {
                    update = update.Modify($"{{ $set : {{ 'Games.$[x].StartedAt' : '{game.StartedAt.Value.ToIso()}' }} }}");
                }

                if (game.EndedAt.HasValue)
                {
                    update = update.Modify($"{{ $set : {{ 'Games.$[x].EndedAt' : '{game.EndedAt.Value.ToIso()}' }} }}");
                }

                updateResult = await update
                  .ExecuteAsync();

                if (updateResult.MatchedCount == 0)
                {
                    throw new UpdateException();
                }
            }
        }

        public async Task<IEnumerable<Game>> GetPlayableGamesAsync(string playerId, IEnumerable<GameType> gameTypes, IEnumerable<GameStatus> statuses)
        {
            return await _database.Queryable<Game>()
                .Where(g => g.Players.Any(p => p.ID != playerId))
                .Where(g => gameTypes.Contains(g.GameType) && statuses.Contains(g.Status))
                .Take(50)
                .ToListAsync();
        }

        public async Task<Game?> GetAsync(string id)
        {
            return await _database.Find<Game>().OneAsync(id);
        }

        public Task<int> GetNumberOfGamesAsync(string playerId)
        {
            return _database.Queryable<Game>()
                .Where(g => g.Players.Any(p => p.ID == playerId))
                .Where(g => g.Status == GameStatus.WaitingForPlayers || g.Status == GameStatus.InGame)
                .CountAsync();
        }

        public async Task<TGame> CreateGameAsync<TGame>(TGame game)
            where TGame : Game
        {
            await _database.SaveAsync(game);
            return game;
        }

        public async Task<Game?> AddPlayerIfNotThereAsync(string id, Game.Player player)
        {
            var game = await _database.UpdateAndGet<Game>()
                .Match(g => g.ID == id && !g.Players.Any(p => p.ID == player.ID) && g.Status == GameStatus.WaitingForPlayers)
                .Modify(g => g.Push(g => g.Players, player))
                .Modify(g => g.Inc(g => g.PlayersCount, 1))
                .ExecuteAsync();
            return game;
        }

        public async Task<Game?> AddPlayerToGameAsync(GameType gameType, int? maxPlayers, int? duration, Game.Player player)
        {
            var match = _database.UpdateAndGet<Game>()
                .Match(g =>
                    g.GameType == gameType &&
                    !g.Players.Any(p => p.ID == player.ID) &&
                    g.Status == GameStatus.WaitingForPlayers);

            if (maxPlayers.HasValue)
            {
                match = match.Match(g => g.MaxPlayers <= maxPlayers);
            }

            if (duration.HasValue)
            {
                match = match.Match(g => g.MaxDuration <= duration);
            }

            var game = await match
                .Modify(g => g.Push(g => g.Players, player))
                .Modify(g => g.Inc(g => g.PlayersCount, 1))
                .ExecuteAsync();

            return game;
        }

        public async Task<Game?> StartGameAsync(string id, IReadOnlyList<int> playerOrders)
        {
            var update = _database.UpdateAndGet<Game>()
                .Match(g => g.ID == id && g.Status == GameStatus.WaitingForPlayers)
                .Modify(g => g.Status, GameStatus.InGame)
                .Modify(g => g.StartedAt, DateTime.UtcNow);

            for (var i = 0; i < playerOrders.Count; i++)
            {
                var playOrder = playerOrders[i];
                update = update.Modify($"{{ $set : {{ 'Players.{i}.PlayOrder' : {playOrder} }} }}");
            }

            return await update
                .ExecuteAsync();
        }

        public async Task RemoveAsync(string id) =>
            await _database.DeleteAsync<Game>(id);
    }
}
