using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WaitingRoomsController : ControllerBase
    {
        private readonly IWaitingRoomRepository _waitingRoomRepository;

        public WaitingRoomsController(IWaitingRoomRepository waitingRoomRepository)
        {
            _waitingRoomRepository = waitingRoomRepository;
        }

        [HttpGet]
        [Route("")]
        public Task<IEnumerable<WaitingRoom>> Get()
        {
            return _waitingRoomRepository.GetAsync();
        }

        [HttpPost]
        [Route("Add")]
        public Task<WaitingRoom> Add(WaitingRoom waitingRoom)
        {
            return _waitingRoomRepository.CreateAsync(waitingRoom);
        }
    }
}
