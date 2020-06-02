using Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Api.SignalR
{
    [Authorize]
    public class GameHub : Hub
    {
    }
}
