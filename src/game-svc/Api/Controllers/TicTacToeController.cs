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
    public class TicTacToeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TicTacToeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPatch]
        public Task<Game> Update([Required][FromBody] AddTicTacToeStepCommand command)
        {
            return _mediator.Send(command);
        }
    }
}
