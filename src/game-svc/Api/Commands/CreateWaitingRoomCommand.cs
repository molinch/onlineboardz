using Api.Extensions;
using Api.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Commands
{
    public class CreateWaitingRoomCommand : IRequest<WaitingRoom>
    {
        [JsonConstructor]
        public CreateWaitingRoomCommand(GameType gameType, int minPlayers, int maxPlayers, bool isOpen)
        {
            GameType = gameType;
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
            IsOpen = isOpen;
        }

        public GameType GameType { get; }
        public int MinPlayers { get; }
        public int MaxPlayers { get; }
        public bool IsOpen { get; }

        public class CreateWaitingRoomCommandHandler : IRequestHandler<CreateWaitingRoomCommand, WaitingRoom>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IWaitingRoomRepository _repository;

            public CreateWaitingRoomCommandHandler(PlayerIdentity playerIdentity, IWaitingRoomRepository repository)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
            }

            public async Task<WaitingRoom> Handle(CreateWaitingRoomCommand request, CancellationToken cancellationToken)
            {
                if (await _repository.IsAlreadyInWaitingRoomAsync(_playerIdentity.PlayerId))
                {
                    throw new Exception("You are already in a waiting room");
                }

                var room = new WaitingRoom()
                {
                    GameType = (GameType)request.GameType, // add validation
                    MinPlayers = request.MinPlayers,
                    MaxPlayers = request.MaxPlayers,
                    IsOpen = request.IsOpen,
                    Players = new List<WaitingRoom.Player>()
                    {
                        new WaitingRoom.Player()
                        {
                            Id = _playerIdentity.PlayerId,
                            Name = _playerIdentity.Name,
                            AcceptedAt = DateTime.UtcNow
                        }
                    }
                };
                return await _repository.CreateAsync(room);
            }
        }
    }
}
