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
    public class GameProposalsController : ControllerBase
    {
        private readonly IGameRepository _repository;
        private readonly IMediator _mediator;

        public GameProposalsController(IGameRepository repository, IMediator mediator)
        {
            _repository = repository;
            _mediator = mediator;
        }

        [HttpGet]
        public Task<IEnumerable<Game>> Get()
        {
            return _repository.GetAsync();
        }

        /// <summary>
        /// Creates a waiting room that includes the current player
        /// </summary>
        [HttpPost]
        public Task<Game> Create([Required]CreateGameProposalCommand command)
        {
            return _mediator.Send(command);
        }

        /// <summary>
        /// Adds the current player to the waiting room
        /// </summary>
        [HttpPatch]
        public Task<Game> Update([Required]AddPlayerToGameProposalCommand command)
        {
            return _mediator.Send(command);
        }
    }
}
