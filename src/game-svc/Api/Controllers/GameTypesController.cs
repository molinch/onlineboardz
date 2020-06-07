using Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GameTypesController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<GameTypeMetadata> Get()
        {
            // returns the type of game that are enabled, and in which order they should be proposed in the UI
            return GameTypeMetadata.All;
        }
    }
}
