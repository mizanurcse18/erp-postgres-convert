using AutoMapper;
using Core.Extensions;
using DAL.Core;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Security.DAL;
using Security.Manager.Implementations;
using Security.Manager.Interfaces;
using System;
using System.Linq;
using System.Reflection;

namespace Security.Manager
{
    public static class Extensions
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddServices();
            var types = Assembly.GetExecutingAssembly().GetTypes()
               .Where(type => type.BaseType == typeof(ManagerBase));

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                if (interfaces.Length.IsNotZero())
                {
                    services.AddTransient(interfaces[1], type);
                }
            }
            services.AddTransient(typeof(IComboManager), typeof(ComboManager));
            services.AddTransient(typeof(ISearchManager), typeof(SearchManager));
            services.AddTransient(typeof(IModelAdapter), typeof(ModelAdapter));
            
            // Configure AutoMapper v12
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                var profile = new AutoMapperProfile(Assembly.GetExecutingAssembly());
                cfg.AddProfile(profile);
            });
            
            // Create and register IMapper instance
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            
            // Initialize static helper for backward compatibility
            AutoMapExtensions.Initialize(mapper);
        }
    }
}
