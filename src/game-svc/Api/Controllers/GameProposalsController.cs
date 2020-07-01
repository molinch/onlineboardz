using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Api.Domain.Commands;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public GameProposalsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Adds the current player to the specifig waiting room/game
        /// </summary>
        [HttpPatch("join")]
        public async Task<TransferObjects.Game> Update([Required][FromBody] AddPlayerToSpecificGameCommand command)
        {
            var game = await _mediator.Send(command);
            return _mapper.Map<TransferObjects.Game>(game);
        }

        /// <summary>
        /// Adds the current player to any waiting room/game that matches
        /// </summary>
        [HttpPatch("joinAny")]
        public async Task<TransferObjects.Game> Update([Required][FromBody] JoinAnyGameCommand command)
        {
            var game = await _mediator.Send(command);
            return _mapper.Map<TransferObjects.Game>(game);
        }
    }
}
