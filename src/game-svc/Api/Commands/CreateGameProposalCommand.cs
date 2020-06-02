using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Commands
{
    public class CreateGameProposalCommand : IRequest<Game>
    {
        [JsonConstructor]
        public CreateGameProposalCommand(GameType gameType, int minPlayers, int maxPlayers, bool isOpen)
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

        public class CreateGameProposalCommandHandler : IRequestHandler<CreateGameProposalCommand, Game>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _repository;
            private readonly IOptions<GameOptions> _gameOptions;

            public CreateGameProposalCommandHandler(PlayerIdentity playerIdentity, IGameRepository repository, IOptions<GameOptions> gameOptions)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
                _gameOptions = gameOptions;
            }

            public async Task<Game> Handle(CreateGameProposalCommand request, CancellationToken cancellationToken)
            {
                if ((await _repository.GetNumberOfGamesAsync(_playerIdentity.Id)) >= _gameOptions.Value.MaxNumberOfGamesPerUser)
                {
                    throw new ValidationException("Maximum number of games reached");
                }

                var game = new Game()
                {
                    Status = GameStatus.WaitingForPlayers,
                    Metadata = new Game.GameMetadata()
                    {
                        GameType = request.GameType, // add validation: check that value is part of enum values
                        MinPlayers = request.MinPlayers,
                        MaxPlayers = request.MaxPlayers,
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
