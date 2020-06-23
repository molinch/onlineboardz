using Api.Domain;
using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Commands
{
    public class JoinAnyGameCommand : IRequest<Game>
    {
        [JsonConstructor]
        public JoinAnyGameCommand(GameType gameType, int? maxPlayers, int? duration, bool isOpen)
        {
            GameType = gameType;
            MaxPlayers = maxPlayers;
            Duration = duration;
            IsOpen = isOpen;
        }

        public GameType GameType { get; }
        public int? MaxPlayers { get; }
        public int? Duration { get; }
        public bool IsOpen { get; }

        public class AddPlayerToAnyGameCommandHander : IRequestHandler<JoinAnyGameCommand, Game>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _repository;
            private readonly GameAssert _gameAssert;
            private readonly IUniqueRandomRangeCreator _uniqueRandomRangeCreator;
            private readonly IGameFactory _gameFactory;
            private readonly IHubContext<GameHub> _gameHub;

            public AddPlayerToAnyGameCommandHander(PlayerIdentity playerIdentity, IGameRepository repository,
                IHubContext<GameHub> gameHub, GameAssert gameAssert, IUniqueRandomRangeCreator uniqueRandomRangeCreator,
                IGameFactory gameFactory)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
                _gameHub = gameHub;
                _gameAssert = gameAssert;
                _uniqueRandomRangeCreator = uniqueRandomRangeCreator;
                _gameFactory = gameFactory;
            }

            public async Task<Game> Handle(JoinAnyGameCommand request, CancellationToken cancellationToken)
            {
                if (!GameTypeMetadata.All.TryGetValue(request.GameType, out var metadata))
                {
                    throw new ValidationException("Invalid game type");
                }

                await _repository.CreatePlayerIfNotThereAsync(new Player()
                {
                    ID = _playerIdentity.Id,
                    Name = _playerIdentity.Name,
                });

                await _gameAssert.NotTooManyOpenGamesAsync();

                Game? waitingRoom;
                if (!request.IsOpen)
                {
                    waitingRoom = await CreateGameAsync(request, metadata);
                }
                else
                {
                    waitingRoom = await _repository.AddPlayerToGameAsync(
                        request.GameType,
                        request.MaxPlayers,
                        request.Duration,
                        new Game.Player()
                        {
                            ID = _playerIdentity.Id,
                            Name = _playerIdentity.Name,
                            AcceptedAt = DateTime.UtcNow
                        });

                    if (waitingRoom == null)
                    {
                        waitingRoom = await CreateGameAsync(request, metadata);
                    }
                }

                if (waitingRoom.ID == null)
                {
                    throw new Exception("Since the game has been created an ID must be set");
                }

                await _repository.AddOrUpdatePlayerGameAsync(_playerIdentity.Id, Player.Game.From(_playerIdentity.Id, waitingRoom));

                if (waitingRoom.MaxPlayers == waitingRoom.Players.Count)
                {
                    var playerOrders = _uniqueRandomRangeCreator.CreateArrayWithAllNumbersFromRange(waitingRoom.Players.Count);
                    var game = await _repository.StartGameAsync(waitingRoom.ID!, playerOrders);

                    if (game != null)
                    {
                        foreach (var player in game.Players)
                        {
                            await _repository.AddOrUpdatePlayerGameAsync(player.ID, Player.Game.From(player.ID, game));
                        }

                        await _gameHub.Clients.Users(waitingRoom.Players.Select(p => p.ID))
                            .SendAsync("GameStarted", game);
                        waitingRoom = game;
                    }
                }
                else
                {
                    await _gameHub.Clients.Users(waitingRoom.Players.Select(p => p.ID))
                        .SendAsync("PlayerAdded", waitingRoom);
                }

                return waitingRoom;
            }

            private async Task<Game> CreateGameAsync(JoinAnyGameCommand request, GameTypeMetadata metadata)
            {
                if (request.MaxPlayers > metadata.MaxPlayers)
                {
                    throw new ValidationException("Players cannot be greather than max number of players");
                }

                var game = _gameFactory.Create(request.GameType);
                game.Status = GameStatus.WaitingForPlayers;
                game.GameType = request.GameType; // add validation: check that value is part of enum values
                game.MinPlayers = metadata.MinPlayers;
                game.MaxPlayers = request.MaxPlayers ?? metadata.MaxPlayers;
                game.MaxDuration = request.Duration ?? metadata.DefaultDuration;
                game.IsOpen = request.IsOpen;
                game.PlayersCount = 1;
                game.Players = new List<Game.Player>()
                {
                    new Game.Player()
                    {
                        ID = _playerIdentity.Id,
                        Name = _playerIdentity.Name,
                        AcceptedAt = DateTime.UtcNow
                    }
                };
                return await _repository.CreateGameAsync(game);
            }
        }
    }
}
