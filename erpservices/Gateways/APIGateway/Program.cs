using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;
using System;
using System.Threading;
using WonderOcelot;

namespace WonderServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
        public IConfiguration Configuration { get; }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureAppConfiguration((hostingContext, config) =>
                   {
                       config
                           .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                           .AddJsonFile("ocelot.json")
                           .AddEnvironmentVariables();
                   }).ConfigureServices(service =>
                   {
                       service.AddCors(options =>
                       {
                           options.AddPolicy("CorsPolicy", cors =>
                                   cors
                                       //.AllowAnyOrigin()
                                       .AllowAnyMethod()
                                       .AllowAnyHeader()
                                       .SetIsOriginAllowed(x=>true)
                                       .AllowCredentials()

                                       );
                       });
                       service.AddOcelot()
                       .AddWonder();
                       service.AddMvc()
                       .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
                       service.AddHttpClient("MyHttpClient", client =>
                       {
                           client.Timeout = Timeout.InfiniteTimeSpan;
                       });
                   }).Configure(app =>
                   {
                       app.UseCors("CorsPolicy");
                       app.UseWebSockets();
                       app.UseRouting();
                       //app.UseMetricServer();//Prometheus metrics

                       app.UseHttpMetrics();//Prometheus metrics
                       app.UseEndpoints(endpoints =>
                       {
                           endpoints.MapMetrics();
                       });
                       //app.UseCors(builder =>
                       //{
                       //    //builder.WithOrigins("*");
                       //    builder.AllowAnyOrigin();
                       //    builder.AllowAnyHeader();
                       //    builder.AllowAnyMethod();
                       //    builder.AllowCredentials();
                       //});
                       app.UseOcelot().Wait();
                   }).UseNLog();
    }
}
