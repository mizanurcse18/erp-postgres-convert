using Ocelot.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WonderOcelot
{
    public interface IServiceFileRepository
    {
        Task<Response<List<ServiceRegistration>>> Get();
        Task<Response> Set(List<ServiceRegistration> services);
    }
}
