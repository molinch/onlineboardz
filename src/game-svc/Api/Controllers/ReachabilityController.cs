using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class ReachabilityController : ControllerBase
    {
        [HttpHead]
        public void Head()
        {
        }
    }
}
