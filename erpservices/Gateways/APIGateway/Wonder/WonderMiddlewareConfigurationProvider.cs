using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Middleware;
using Ocelot.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WonderOcelot
{
    public static class WonderMiddlewareConfigurationProvider
    {
        public static OcelotMiddlewareConfigurationDelegate Get = async builder =>
        {
            var internalServiceRepo = builder.ApplicationServices.GetService<IInternalWonderServiceRepository>();
            var fileConfigRepo = builder.ApplicationServices.GetService<IServiceFileRepository>();
            var internalConfigCreator = builder.ApplicationServices.GetService<IInternalConfigurationCreator>();
            var internalConfigRepo = builder.ApplicationServices.GetService<IInternalConfigurationRepository>();
            await SetFileConfigInWonder(builder, internalServiceRepo, fileConfigRepo, internalConfigCreator, internalConfigRepo);
        };

        private static async Task SetFileConfigInWonder(IApplicationBuilder builder,
            IInternalWonderServiceRepository internalServiceRepo,
            IServiceFileRepository fileConfigRepo,
            IInternalConfigurationCreator internalConfigCreator,
            IInternalConfigurationRepository internalConfigRepo)
        {
            // get the config from wonder.
            var wonderServices = await fileConfigRepo.Get();

            if (IsError(wonderServices))
            {
                ThrowToStopOcelotStarting(wonderServices);
            }
            else if (ConfigNotStoredInWonder(wonderServices))
            {
                //there was no config in wonder set the file in config in wonder
                await fileConfigRepo.Set(new List<ServiceRegistration>());
            }
            else
            {
                var fileConfiguration = new FileConfiguration();
                foreach (var service in wonderServices.Data)
                {
                    if (fileConfiguration.ReRoutes.Find(rr => rr.ServiceName == service.Name) != null) continue;
                    fileConfiguration.ReRoutes.Add(ServiceUtil.CreateFileReRoute(service));
                }

                // add the internal services to the internal repo                               
                internalServiceRepo.AddOrReplace(wonderServices.Data);

                // create the internal config from wonder data
                var internalConfig = await internalConfigCreator.Create(fileConfiguration);
                if (IsError(internalConfig))
                {
                    ThrowToStopOcelotStarting(internalConfig);
                }
                else
                {
                    var interConfig = internalConfigRepo.Get();
                    interConfig.Data.ReRoutes.Clear();
                    interConfig.Data.ReRoutes.AddRange(internalConfig.Data.ReRoutes);
                    // add the internal config to the internal repo
                    var response = internalConfigRepo.AddOrReplace(interConfig.Data);

                    if (IsError(response))
                    {
                        ThrowToStopOcelotStarting(response);
                    }
                }
                if (IsError(internalConfig))
                {
                    ThrowToStopOcelotStarting(internalConfig);
                }
            }
        }

        private static void ThrowToStopOcelotStarting(Response config)
        {
            throw new Exception($"Unable to start Ocelot, errors are: {string.Join(",", config.Errors.Select(x => x.ToString()))}");
        }

        private static bool IsError(Response response)
        {
            return response is null || response.IsError;
        }

        private static bool ConfigNotStoredInWonder(Response<List<ServiceRegistration>> fileConfigFromWonder)
        {
            return fileConfigFromWonder.Data is null;
        }
    }
}
