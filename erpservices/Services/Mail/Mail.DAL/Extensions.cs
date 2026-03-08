
using Core.AppContexts;
using DAL.Core;
using DAL.Core.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mail.DAL
{
    public static class Extensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddDbContext<MailDbContext>(options =>
            {
                options.UseNpgsql(AppContexts.GetConnectionString(ConnectionName.Default)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            DbRegisterHelper.RegisterForDbContext(typeof(MailDbContext), services);

            services.AddDbContext<DbUtility>(options =>
            {
                options.UseNpgsql(AppContexts.GetConnectionString(ConnectionName.SecurityContext)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            DbRegisterHelper.RegisterForDbContext(typeof(DbUtility), services);

        }
    }
}
