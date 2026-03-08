using Core.AppContexts;
using DAL.Core;
using DAL.Core.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HRMS.DAL
{
    public static class Extensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddDbContext<HRMSDbContext>(options =>
            {
                options.UseNpgsql(AppContexts.GetConnectionString(ConnectionName.Default)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
            
            DbRegisterHelper.RegisterForDbContext(typeof(HRMSDbContext), services);

            services.AddDbContext<DbUtility>(options =>
            {
                options.UseNpgsql(AppContexts.GetConnectionString(ConnectionName.SecurityContext)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            DbRegisterHelper.RegisterForDbContext(typeof(DbUtility), services);

        }
    }
}
