using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient _client;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        //static IConfiguration conf = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());
        private Timer _timer;
        static IConfiguration conf = (new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build());
        public static int repeatTimeInMS = Convert.ToInt32(conf["ApplicationSetting:RepeatTimeInMS"]);
        public static string UddoktaSyncTime = Convert.ToString(conf["ApplicationSetting:UddoktaSyncTime"]);
        public static int ApiCallTimoutInMIn = Convert.ToInt16(conf["ApplicationSetting:ApiCallTimoutInMIn"]);
        public static string DCFMService = Convert.ToString(conf["ApplicationSetting:DCFMService"]);
        public static string UddoktaSyncAPI = Convert.ToString(conf["ApplicationSetting:UddoktaSyncAPI"]);
        static string Token = "NagadUddoktaSyncWithDateTime";
        //string UddoktaSyncApi = $"{DCFMService}{UddoktaSyncAPI}?FromDate={DateTime.Now.AddDays(-1):yyyy-MM-dd}&ToDate={DateTime.Now:yyyy-MM-dd}&Token={Token}";
        public Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting the worker service...");

            HttpClientHandler clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            _client = new HttpClient(clientHandler);

            SetDailyTimer(cancellationToken);

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Dispose();
            _timer?.Change(Timeout.Infinite, 0);
            _logger.LogInformation("The service has been stopped..");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation("Executing the worker service...");
            try
            {
                //var tasks = new List<Task>
                //{
                //    // Add tasks with different delay times
                //    CallApiAsync(UddoktaSyncApi, 90000, stoppingToken) // 1 minute
                //};
                //tasks.Add(CallApiAsync("https://api3.com", 120000, stoppingToken)); // 2 minutes
                //var timer = new Timer(CallApiAsyncWrapper, stoppingToken, GetNextOccurrence(DateTime.Now), TimeSpan.FromDays(1));
                

                while (!stoppingToken.IsCancellationRequested)
                {
                    //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    //await Task.WhenAll(tasks);
                }
            }
            finally
            {
                _logger.LogCritical("Exiting application...");
                _hostApplicationLifetime.StopApplication();
            }
        }


        private void SetDailyTimer(CancellationToken stoppingToken)
        {
            var nextOccurrence = GetNextOccurrence(DateTime.Now);
            //var nextOccurrence = TimeSpan.FromMinutes(5);
            _timer = new Timer(CallApiAsyncWrapper, stoppingToken, nextOccurrence, TimeSpan.FromDays(1));
            //_timer = new Timer(CallApiAsyncWrapper, stoppingToken, nextOccurrence, TimeSpan.FromMinutes(5));

            _logger.LogInformation($"Next API call scheduled at: {DateTime.Now + nextOccurrence}");
        }

        private void CallApiAsyncWrapper(object state)
        {
            var cancellationToken = (CancellationToken)state;
            string fromDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            string toDate = DateTime.Now.ToString("yyyy-MM-dd");
            string UddoktaSyncApi = $"{DCFMService}{UddoktaSyncAPI}?FromDate={fromDate}&ToDate={toDate}&Token={Token}";


            CallApiAsync(UddoktaSyncApi, cancellationToken);

            // Log next scheduled time after each API call
            var nextOccurrence = TimeSpan.FromDays(1);
            _logger.LogInformation($"API call completed. Next API call scheduled at: {DateTime.Now + nextOccurrence}");
        }

        private async Task CallApiAsync(string url, int delayInMS, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Calling API: {url}");
                    var response = await _client.GetAsync(url, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    _logger.LogInformation($"API call successful: {url}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error calling API: {url}");
                }
                await Task.Delay(delayInMS, cancellationToken);
            }
        }
        private async void CallApiAsync(string url, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(ApiCallTimoutInMIn)); // 5 minute timeout

            try
            {
                _logger.LogInformation($"Calling API: {url}");
                var response = await _client.GetAsync(url, cts.Token);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation($"API call successful: {url}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling API: {url}");
            }
        }

        private TimeSpan GetNextOccurrence(DateTime now)
        {
            var timeParts = UddoktaSyncTime.Split(' ');
            var time = timeParts[0].Split(':');
            var hour = int.Parse(time[0]);
            var minute = int.Parse(time[1]);

            if (timeParts[1].ToUpper() == "PM" && hour != 12)
            {
                hour += 12;
            }
            else if (timeParts[1].ToUpper() == "AM" && hour == 12)
            {
                hour = 0;
            }

            var nextOccurrence = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            if (nextOccurrence < now)
            {
                nextOccurrence = nextOccurrence.AddDays(1);
            }
            return nextOccurrence - now;
        }
    }
}