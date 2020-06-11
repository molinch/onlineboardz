using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Api.Commands;
using Api.Persistence;
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

        [HttpGet]
        public Task<IEnumerable<Game>> Get([FromQuery]IEnumerable<GameType> types, [FromQuery] IEnumerable<GameStatus> statuses)
        {
            return _mediator.Send(new ListGamesQuery(types, statuses));
        }
    }
}
