using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WonderOcelot
{
    public static class ServiceUtil
    {
        public static FileReRoute CreateFileReRoute(Service service)
        {
            var routeRule = new FileReRoute();
            var listUpMethod = new List<string> { "OPTIONS", "GET", "POST", "PUT", "DELETE" };
            routeRule.UpstreamPathTemplate = $"/{service.Name}/{{url}}";
            routeRule.UpstreamHttpMethod = listUpMethod;
            routeRule.ServiceName = service.Name;
            routeRule.DownstreamPathTemplate = $"/{{url}}";
            routeRule.DownstreamScheme = "ws";
            routeRule.LoadBalancerOptions = new FileLoadBalancerOptions()
            {
                Type = "LeastConnection"
            };
            return routeRule;
        }
        public static FileReRoute CreateFileReRoute(ServiceRegistration service)
        {
            var routeRule = new FileReRoute();
            var listUpMethod = new List<string> { "OPTIONS", "GET", "POST", "PUT", "DELETE" };
            routeRule.UpstreamPathTemplate = $"/{service.Name}/{{url}}";
            routeRule.UpstreamHttpMethod = listUpMethod;
            routeRule.ServiceName = service.Name;
            routeRule.DownstreamPathTemplate = $"/{{url}}";
            routeRule.DownstreamScheme = "ws";
            routeRule.LoadBalancerOptions = new FileLoadBalancerOptions()
            {
                Type = "LeastConnection"
            };
            return routeRule;
        }
        public static Service CreateService(ServiceRegistration service)
        {
            return new Service(service.Name, new ServiceHostAndPort(service.Host, service.Port), service.ServiceId, null, null);
        }
        public static async Task AddServiceAsync(IServiceProvider provider, ServiceRegistration regService)
        {
            var wonderRepo = (IInternalWonderServiceRepository)provider.GetService(typeof(IInternalWonderServiceRepository));            
            CreateFileReRoute(regService);            
            wonderRepo.Add(regService);
            await UpdateAsync(provider);                        
        }
        public static async Task RemoveServiceAsync(IServiceProvider provider, string serviceId)
        {
            var wonderRepo = (IInternalWonderServiceRepository)provider.GetService(typeof(IInternalWonderServiceRepository));
            wonderRepo.Remove(serviceId);
            await UpdateAsync(provider);            
        }
        private static async Task UpdateAsync(IServiceProvider provider)
        {
            var wonderRepo = (IInternalWonderServiceRepository)provider.GetService(typeof(IInternalWonderServiceRepository));
            var fileRepo = (IServiceFileRepository)provider.GetService(typeof(IServiceFileRepository));
            var internalConfigCreator = (IInternalConfigurationCreator)provider.GetService(typeof(IInternalConfigurationCreator));
            var internalConfigRepo = (IInternalConfigurationRepository)provider.GetService(typeof(IInternalConfigurationRepository));            
            var services = wonderRepo.Get();
            await fileRepo.Set(services);
            var fileConfiguration = new FileConfiguration();
            foreach (var serv in services)
            {
                if (fileConfiguration.ReRoutes.Find(rr => rr.ServiceName == serv.Name) != null) continue;
                fileConfiguration.ReRoutes.Add(CreateFileReRoute(serv));
            }
            var internalConfig = await internalConfigCreator.Create(fileConfiguration);
            var interConfig = internalConfigRepo.Get();
            interConfig.Data.ReRoutes.Clear();
            interConfig.Data.ReRoutes.AddRange(internalConfig.Data.ReRoutes);
            internalConfigRepo.AddOrReplace(interConfig.Data);
        }
    }
}
