
using API.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace External.API
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
            //services.ConfigureServices();        
        }
    }
}
