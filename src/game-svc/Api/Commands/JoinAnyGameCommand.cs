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
            private readonly IHubContext<GameHub> _gameHub;

            public AddPlayerToAnyGameCommandHander(PlayerIdentity playerIdentity, IGameRepository repository,
                IHubContext<GameHub> gameHub, GameAssert gameAssert)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
                _gameHub = gameHub;
                _gameAssert = gameAssert;
            }

            public async Task<Game> Handle(JoinAnyGameCommand request, CancellationToken cancellationToken)
            {
                if (!GameTypeMetadata.All.TryGetValue(request.GameType, out var metadata))
                {
                    throw new ValidationException("Invalid game type");
                }

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
                        new Game.GameMetadata.Player()
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

                if (waitingRoom.Metadata.MaxPlayers == waitingRoom.Metadata.Players.Count)
                {
                    var game = await _repository.SetGameStatusAsync(waitingRoom.ID, waitingRoom.Status, GameStatus.InGame);
                    if (game != null)
                    {
                        await _gameHub.Clients.Users(waitingRoom.Metadata.Players.Select(p => p.ID))
                            .SendAsync("GameStarted", game);
                        waitingRoom = game;
                    }
                }
                else
                {
                    await _gameHub.Clients.Users(waitingRoom.Metadata.Players.Select(p => p.ID))
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

                var game = new Game()
                {
                    Status = GameStatus.WaitingForPlayers,
                    Metadata = new Game.GameMetadata()
                    {
                        GameType = request.GameType, // add validation: check that value is part of enum values
                        MinPlayers = metadata.MinPlayers,
                        MaxPlayers = request.MaxPlayers ?? metadata.MaxPlayers,
                        IsOpen = request.IsOpen,
                        PlayersCount = 1,
                        Players = new List<Game.GameMetadata.Player>()
                        {
                            new Game.GameMetadata.Player()
                            {
                                ID = _playerIdentity.Id,
                                Name = _playerIdentity.Name,
                                AcceptedAt = DateTime.UtcNow
                            }
                        }
                    }
                };
                return await _repository.CreateAsync(game);
            }
        }
    }
}
