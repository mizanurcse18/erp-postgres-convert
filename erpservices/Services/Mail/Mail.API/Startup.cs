using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Core;
using Microsoft.AspNetCore.Builder;

using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mail.Manager;

namespace Mail.API
{
    public class Startup : StartupBase
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
            //Configuration = configuration;
        }
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.ConfigureServices();
        }
    }
}
