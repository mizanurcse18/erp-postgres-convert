using Ocelot.ServiceDiscovery;

namespace WonderOcelot
{
    public static class WonderProviderFactory
    {
        public static ServiceDiscoveryFinderDelegate Get = (provider, config, service) =>
        {
            if (config.Type?.ToLower() == "wonder")
            {
                var internalServiceRepo = (IInternalWonderServiceRepository)provider.GetService(typeof(IInternalWonderServiceRepository));
                return new Wonder(internalServiceRepo, service.ServiceName);
            }
            return null;
        };
    }
}
