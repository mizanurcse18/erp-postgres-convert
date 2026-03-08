using Core.AppContexts;
using Core.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Core.Authentication
{
    public static class JWTAuth
    {
        public static void AddJWTAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var ipAddress = context.Principal.FindFirst("IPAddress").Value;
                        

                        if (context.Principal.Identity.Name.IsNullOrEmpty() ||
                            ipAddress.IsNullOrEmpty() ||
                            !AppContexts.IsValidUser(ipAddress))
                        {
                            // return unauthorized if user no longer exists
                            context.Fail("Unauthorized");
                        }

                        var newUser = new UserPrincipal
                        {
                            LogedID = context.Principal.FindFirst("LogedID").Value.IsNullOrEmpty() ? 0 : context.Principal.FindFirst("LogedID").Value.ToInt(),
                            UserID = context.Principal.FindFirst("UserID").Value.ToInt(),
                            UserName = context.Principal.Identity.Name,
                            IsAdmin = context.Principal.FindFirst("IsAdmin").Value.ToBoolean(),
                            ApplicationID = context.Principal.FindFirst("ApplicationID").Value.ToInt(),
                            CompanyID = context.Principal.FindFirst("CompanyID").Value,
                            CompanyName = context.Principal.FindFirst("CompanyName").Value,
                            LogInDateTime = context.Principal.FindFirst("LogInDateTime").Value.ToDate(),
                            IPAddress = ipAddress,
                            PersonID = context.Principal.FindFirst("PersonID").Value.IsNullOrEmpty() ? 0 : context.Principal.FindFirst("PersonID").Value.ToInt(),
                            EmployeeID = context.Principal.FindFirst("EmployeeID").Value.IsNullOrEmpty() ? 0 : context.Principal.FindFirst("EmployeeID").Value.ToInt(),
                            EmployeeCode = context.Principal.FindFirst("EmployeeCode").Value.IsNullOrEmpty() ? "" : context.Principal.FindFirst("EmployeeCode").Value,
                            FullName = context.Principal.FindFirst("FullName").Value.IsNullOrEmpty() ? "" : context.Principal.FindFirst("FullName").Value,
                            DivisionID = context.Principal.FindFirst("DivisionID").Value.IsNullOrEmpty() ? 0 : context.Principal.FindFirst("DivisionID").Value.ToInt(),
                            DepartmentID = context.Principal.FindFirst("DepartmentID").Value.IsNullOrEmpty() ? 0 : context.Principal.FindFirst("DepartmentID").Value.ToInt(),
                            DepartmentName = context.Principal.FindFirst("DepartmentName").Value.IsNullOrEmpty() ? "" : context.Principal.FindFirst("DepartmentName").Value,
                            DesignationName = context.Principal.FindFirst("DesignationName").Value.IsNullOrEmpty() ? "" : context.Principal.FindFirst("DesignationName").Value,
                            DivisionName = context.Principal.FindFirst("DivisionName").Value.IsNullOrEmpty() ? "" : context.Principal.FindFirst("DivisionName").Value,
                            CompanyShortCode = context.Principal.FindFirst("CompanyShortCode").Value.IsNullOrEmpty() ? "" : context.Principal.FindFirst("CompanyShortCode").Value,
                            WorkMobile = context.Principal.FindFirst("WorkMobile").Value.IsNullOrEmpty() ? "" : context.Principal.FindFirst("WorkMobile").Value,
                            Email = context.Principal.FindFirst("Email").Value.IsNullOrEmpty() ? "" : context.Principal.FindFirst("Email").Value,
                            Role = context.Principal.FindFirstValue("Role").IsNullOrEmpty() ? "": context.Principal.FindFirstValue("Role")
                        };

                        AppContexts.SetUserInfo(newUser);
                        return Task.CompletedTask;
                    }
                    
                };
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }
    }
}
