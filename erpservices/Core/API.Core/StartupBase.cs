using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using API.Core.Mvc;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using System.Linq;
using Core.AppContexts;
using Microsoft.AspNetCore.Http;
using API.Core.Logging;
using Microsoft.Extensions.Options;
using API.Core.Authentication;
using Newtonsoft.Json.Serialization;
using API.Core.Hubs;
using Microsoft.AspNetCore.Http.Features;
//using Prometheus;

namespace API.Core
{
    public class StartupBase
    {
        private readonly static string[] Headers;

        public IConfiguration Configuration
        {
            get;
        }

        public IContainer Container
        {
            get;
            private set;
        }

        static StartupBase()
        {
            Headers = new string[] { "X-Operation", "X-Resource", "X-Total-Count" };
        }
        public StartupBase(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddCustomMvc(Configuration);
            services.AddControllers().AddNewtonsoftJson(option => option.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddSingleton<ILogger, FileLogger>();
            ConfigureConnectionStrings();
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //        builder => builder.AllowAnyMethod()
            //            .AllowAnyHeader()
            //            .WithOrigins("http://localhost:5000", "http://localhost:3000")
            //            .AllowCredentials());
            //});
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            services.AddHttpContextAccessor();
            services.AddJWTAuth(Configuration);
            services.AddSignalR();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
           
            AppContexts.Configure(services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>());            

            //ContainerBuilder containerBuilder = new ContainerBuilder();
            //RegistrationExtensions.AsImplementedInterfaces<object>(
            //    RegistrationExtensions.RegisterAssemblyTypes(containerBuilder, new Assembly[] { Assembly.GetEntryAssembly() })
            //    );
            //AutofacRegistration.Populate(containerBuilder, services);
            //this.Container = containerBuilder.Build();
            //return new AutofacServiceProvider(this.Container);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (HostEnvironmentEnvExtensions.IsDevelopment(env))
            //{
            //    DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app);
            //    app.UseSwagger();
            //    app.UseSwaggerUI(c =>
            //    {
            //        c.SwaggerEndpoint("/swagger/v1/swagger.json", " API V1");
            //        c.RoutePrefix = string.Empty;
            //    });
            //}
            //app.UseCors("CorsPolicy");
            //app.Use(async (context, next) =>
            //{
            //    // Remove the "Server" header
            //    context.Response.Headers.Remove("Server");

            //    // Remove the "X-Powered-By" header
            //    context.Response.Headers.Remove("X-Powered-By");

            //    await next();
            //});
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
                c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "My API");

            });
            //app.UseMetricServer();

            // app.UseHttpMetrics();


            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseRouting();
            // global cors policy
            //app.UseCors("AllowAnyOrigin");
            app.UseSession();
            app.UseAuthentication();

            app.UseMiddleware(typeof(ErrorHandling));

            app.UseMvc();
            app.UseMiddleware<RestrictStaticFilesMiddleware>();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapHub<ChatHub>("/hubs/chat");
                //endpoints.MapHub<RemoteAttendanceNotificaitonHub>("/hubs/chat");
                endpoints.MapHub<NotificaitonHub>("/hubs/notification");
                endpoints.MapControllers();
            });
        }        
        private void ConfigureConnectionStrings()
        {
            var connectionList = new Dictionary<string, string>();
            var connections = Configuration.GetSection("ConnectionStrings").GetChildren().AsEnumerable();

            foreach (var connection in connections)
            {
                connectionList.Add(connection.Key, connection.Value);
            }
            AppContexts.ConfigureConnectionStrings(connectionList);
        }

    }
}
