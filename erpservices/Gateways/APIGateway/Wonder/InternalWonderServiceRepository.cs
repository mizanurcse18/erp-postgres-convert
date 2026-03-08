using Ocelot.Responses;
using Ocelot.Values;
using System.Collections.Generic;

namespace WonderOcelot
{
    public class InternalWonderServiceRepository : IInternalWonderServiceRepository
    {
        private static readonly object LockObject = new object();

        private List<ServiceRegistration> services = new List<ServiceRegistration>();
        private List<Service> octServices = new List<Service>();      

        public List<ServiceRegistration> Get()
        {
            return new List<ServiceRegistration>(services);
        }

        public List<ServiceRegistration> Get(string serviceName)
        {
            return new List<ServiceRegistration>(services.FindAll(s => s.Name == serviceName));
        }

        public bool Exists(string serviceName, string host, int port)
        {
            return services.Find(s => s.Name == serviceName && s.Host == host && s.Port == port) != null;
        }

        public List<Service> GetService(string serviceName)
        {
            return new List<Service>(octServices.FindAll(s => s.Name == serviceName));
        }

        public Response Add(ServiceRegistration service)
        {
            lock (LockObject)
            {
                RemoveIfExists(service.Name, service.Host, service.Port);
                services.Add(service);
                UpdateService();
            }
            return new OkResponse();
        }

        public Response AddOrReplace(List<ServiceRegistration> services)
        {
            lock (LockObject)
            {
                this.services = services;
                UpdateService();
            }
            return new OkResponse();
        }

        private void RemoveIfExists(string serviceName, string host, int port)
        {
            services.RemoveAll(s => s.Name == serviceName && s.Host == host && s.Port == port);
        }

        public Response Remove(string serviceId)
        {
            lock (LockObject)
            {
                services.RemoveAll(s => s.ServiceId == serviceId);
                UpdateService();
            }
            return new OkResponse();
        }       

        private void UpdateService()
        {
            octServices = new List<Service>();
            foreach (var service in services)
            {
                octServices.Add(ServiceUtil.CreateService(service));
            }
        }
    }
}
