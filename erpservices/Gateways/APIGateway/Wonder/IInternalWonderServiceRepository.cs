using Ocelot.Responses;
using Ocelot.Values;
using System.Collections.Generic;

namespace WonderOcelot
{
    public interface IInternalWonderServiceRepository
    {      
        List<ServiceRegistration> Get();
        List<ServiceRegistration> Get(string serviceName);
        bool Exists(string serviceName, string host, int port);
        List<Service> GetService(string serviceName);        
        Response Add(ServiceRegistration service);
        Response AddOrReplace(List<ServiceRegistration> services);
        Response Remove(string serviceId);        
    }
}
