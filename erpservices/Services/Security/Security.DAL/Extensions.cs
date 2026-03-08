using Core.AppContexts;
using DAL.Core;
using DAL.Core.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Security.DAL
{
    public static class Extensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddDbContext<SecurityDbContext>(options =>
            {
                options.UseNpgsql(AppContexts.GetConnectionString(ConnectionName.Default))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
            
            DbRegisterHelper.RegisterForDbContext(typeof(SecurityDbContext), services);
            //, b => b.MigrationsAssembly("Security.DAL")
            services.AddDbContext<DbUtility>(options =>
            {
                options.UseNpgsql(AppContexts.GetConnectionString(ConnectionName.Default)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            DbRegisterHelper.RegisterForDbContext(typeof(DbUtility), services);

        }
    }
}
