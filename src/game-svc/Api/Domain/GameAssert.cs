using Api.Extensions;
using Api.Persistence;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Api.Domain
{
    public class GameAssert
    {
        private readonly PlayerIdentity _playerIdentity;
        private readonly IGameRepository _repository;
        private readonly IOptions<GameOptions> _gameOptions;

        public GameAssert(PlayerIdentity playerIdentity, IGameRepository repository, IOptions<GameOptions> gameOptions)
        {
            _playerIdentity = playerIdentity;
            _repository = repository;
            _gameOptions = gameOptions;
        }

        public async Task NotTooManyOpenGamesAsync()
        {
            var numberOfGames = await _repository.GetNumberOfGamesAsync(_playerIdentity.Id);
            if (numberOfGames >= _gameOptions.Value.MaxNumberOfGamesPerUser)
            {
                throw new ValidationException($"Maximum number of games reached: you already have {numberOfGames} games started while maximum is {_gameOptions.Value.MaxNumberOfGamesPerUser}");
            }
        }
    }
}
