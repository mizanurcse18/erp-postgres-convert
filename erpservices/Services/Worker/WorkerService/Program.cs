using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace WorkerService
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var location = Assembly.GetEntryAssembly()?.Location;
            string logFolder = $"{Path.GetDirectoryName(location)}\\Logs";
            string logfilePath = $@"{logFolder}\log-{DateTime.Today.ToString(Util.DateFormat)}.log";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(logfilePath)
                .CreateLogger();
            try
            {
                Log.Information("Starting up the service");
                CreateHostBuilder(args).Build().Run();
                return;
            }
            catch (Exception ex)
            {
               // Log.Fatal($@"{Directory.GetCurrentDirectory()}");
                Log.Fatal($@"{AppContext.BaseDirectory}");
                Log.Fatal(ex, "There was a problem starting the service");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            
                //.ConfigureAppConfiguration((hostingContext, config) =>
                //{
                //    var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //    config.SetBasePath(exePath);
                //    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                //})            
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();                                                                                                                                                         
                })
                .UseWindowsService()
                .UseSerilog();
        

    }
}
