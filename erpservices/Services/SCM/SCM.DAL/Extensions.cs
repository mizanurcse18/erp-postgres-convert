using Core.AppContexts;
using DAL.Core;
using DAL.Core.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SCM.DAL
{
    public static class Extensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddDbContext<SCMDbContext>(options =>
            {
                options.UseNpgsql(AppContexts.GetConnectionString(ConnectionName.Default)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
            
            DbRegisterHelper.RegisterForDbContext(typeof(SCMDbContext), services);

            services.AddDbContext<DbUtility>(options =>
            {
                options.UseNpgsql(AppContexts.GetConnectionString(ConnectionName.SecurityContext)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            DbRegisterHelper.RegisterForDbContext(typeof(DbUtility), services);

        }
    }
}
