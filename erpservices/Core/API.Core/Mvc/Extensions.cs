using Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;

namespace API.Core.Mvc
{
    public static class Extensions
    {

        public static void AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(new ModelValidationAttribute());
                options.EnableEndpointRouting = false;
                options.MaxModelBindingCollectionSize = int.MaxValue;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            var culture = appSettings.Culture ?? "en-US";
            services.Configure<RequestLocalizationOptions>(option =>
            {
                var supporedCultures = new List<CultureInfo>
                {
                    new CultureInfo(culture)
                };
                option.DefaultRequestCulture = new RequestCulture(culture);
                // Formatting numbers, dates, etc.
                option.SupportedCultures = supporedCultures;
                // UI strings that we have localized.
                option.SupportedUICultures = supporedCultures;
            });
            SetServerDateFormat(culture);

            //services.AddControllers();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(appSettings.Service.Version,
                    new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Title = appSettings.Service.Name,
                        Description = appSettings.Service.Description,
                        Version = appSettings.Service.Version
                    });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                {
                    new OpenApiSecurityScheme
                    {
                    Reference = new OpenApiReference
                        {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,

                    },
                    new List<string>()
                    }
                });
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //options.IncludeXmlComments(xmlPath);
            });
        }

        public static void SetServerDateFormat(string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var dateTimeFormat = DateTimeFormatInfo.GetInstance(cultureInfo);
            var format = dateTimeFormat.ShortDatePattern;
            var separator = dateTimeFormat.DateSeparator;
            var formatArray = format.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            format = string.Empty;

            foreach (var str in formatArray)
            {
                switch (str)
                {
                    case "d":
                        format += "dd" + separator;
                        break;
                    case "M":
                        format += "MM" + separator;
                        break;
                    default:
                        format += str + separator;
                        break;
                }
            }

            format = format.Substring(0, format.Length - 1);
            Util.SysDateFormat = format;
            Util.SysDateTimeFormat = $"{Util.SysDateFormat} hh:mm:ss.fff";
        }
    }
}
