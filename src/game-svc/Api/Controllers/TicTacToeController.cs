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
    public class TicTacToeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public TicTacToeController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPatch]
        public async Task<TransferObjects.TicTacToe> Update([Required][FromBody] AddTicTacToeStepCommand command)
        {
            var game = await _mediator.Send(command);
            return _mapper.Map<TransferObjects.TicTacToe>(game);
        }
    }
}
