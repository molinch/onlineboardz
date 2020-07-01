using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Domain;
using Api.Domain.Queries;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GamesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet("search")]
        public async Task<IEnumerable<TransferObjects.Game>> Get([FromQuery]IEnumerable<GameType> types, [FromQuery] IEnumerable<GameStatus> statuses)
        {
            var games = await _mediator.Send(new ListPlayableGamesQuery(types, statuses));
            return _mapper.Map<IEnumerable<Game>, IEnumerable<TransferObjects.Game>>(games);
        }

        [HttpGet("mine")]
        public async Task<IEnumerable<TransferObjects.Player.Game>> GetMyGames()
        {
            var games = await _mediator.Send(new ListMyGamesQuery(null));
            return _mapper.Map<IEnumerable<Player.Game>, IEnumerable<TransferObjects.Player.Game>>(games);
        }

        [HttpGet("mine/active")]
        public async Task<IEnumerable<TransferObjects.Player.Game>> GetMyActiveGames()
        {
            var games = await _mediator.Send(new ListMyGamesQuery(GameStatus.InGame));
            return _mapper.Map<IEnumerable<Player.Game>, IEnumerable<TransferObjects.Player.Game>>(games);
        }

        [HttpGet("{gameId}")]
        public async Task<TransferObjects.Game> Get(string gameId)
        {
            var game = await _mediator.Send(new GetGameQuery(gameId));
            return _mapper.Map<TransferObjects.Game>(game);
        }
    }
}
