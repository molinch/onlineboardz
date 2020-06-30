using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Domain;
using Api.Domain.Queries;
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

        public GamesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("search")]
        public Task<IEnumerable<Game>> Get([FromQuery]IEnumerable<GameType> types, [FromQuery] IEnumerable<GameStatus> statuses)
        {
            return _mediator.Send(new ListPlayableGamesQuery(types, statuses));
        }

        [HttpGet("mine")]
        public Task<IEnumerable<Player.Game>> GetMine()
        {
            return _mediator.Send(new ListMyGamesQuery());
        }

        [HttpGet("{gameId}")]
        public Task<Game> Get(string gameId)
        {
            return _mediator.Send(new GetGameQuery(gameId));
        }
    }
}
