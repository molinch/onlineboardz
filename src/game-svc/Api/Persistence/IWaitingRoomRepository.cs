using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public interface IWaitingRoomRepository
    {
        Task<WaitingRoom> CreateAsync(WaitingRoom room);
        Task<IEnumerable<WaitingRoom>> GetAsync();
        Task<WaitingRoom> GetAsync(string id);
        Task RemoveAsync(string id);
        Task RemoveAsync(WaitingRoom room);
        Task UpdateAsync(string id, WaitingRoom room);
    }
}