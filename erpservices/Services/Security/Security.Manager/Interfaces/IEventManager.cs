using Security.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IEventManager
    {
        public Task<List<Dictionary<string, object>>> GetEventList();
    }
}
