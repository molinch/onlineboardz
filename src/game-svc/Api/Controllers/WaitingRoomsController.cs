using System.Collections.Generic;
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
    public class WaitingRoomsController : ControllerBase
    {
        private readonly IWaitingRoomRepository _repository;
        private readonly IMediator _mediator;

        public WaitingRoomsController(IWaitingRoomRepository repository, IMediator mediator)
        {
            _repository = repository;
            _mediator = mediator;
        }

        [HttpGet]
        public Task<IEnumerable<WaitingRoom>> Get()
        {
            return _repository.GetAsync();
        }

        [HttpPut]
        public Task<WaitingRoom> Create(CreateWaitingRoomCommand command)
        {
            return _mediator.Send(command);
        }
    }
}
