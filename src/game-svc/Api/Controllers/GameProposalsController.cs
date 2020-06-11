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
    public class GameProposalsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GameProposalsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a waiting room/game that includes the current player
        /// </summary>
        [HttpPost]
        public Task<Game> Create([Required]CreateGameProposalCommand command)
        {
            return _mediator.Send(command);
        }

        /// <summary>
        /// Adds the current player to the specifig waiting room/game
        /// </summary>
        [HttpPatch("join")]
        public Task<Game> Update([Required]AddPlayerToSpecificGameCommand command)
        {
            return _mediator.Send(command);
        }

        /// <summary>
        /// Adds the current player to any waiting room/game that matches
        /// </summary>
        [HttpPatch("joinAny")]
        public Task<Game> Update([Required][FromBody] AddPlayerToAnyGameCommand command)
        {
            return _mediator.Send(command);
        }
    }
}
