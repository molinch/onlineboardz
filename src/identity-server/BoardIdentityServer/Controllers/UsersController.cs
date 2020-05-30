using BoardIdentityServer.Persistence;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BoardIdentityServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly UserDbContext _users;

        public UsersController(UserDbContext users)
        {
            _users = users;
        }

        [HttpGet]
        [Route("Me")]
        public Task<User> Me()
        {
            var subjectId = HttpContext.User.GetSubjectId();
            var userId = Guid.Parse(subjectId);
            return _users.Users.Where(u => u.ExternalId == userId).FirstOrDefaultAsync();
        }
    }
}
