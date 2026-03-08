using Core;
using Core.AppContexts;
using Core.Extensions;
using Manager.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Core
{
    public class ApiPathValidationAttribute : ActionFilterAttribute
    {

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            var appSettingsSection = configuration.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();

            var isAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            string fullPath = context.HttpContext.Request.Path.ToString();
            var path = context.HttpContext.Request.Path.ToString().Split("/", 4, StringSplitOptions.RemoveEmptyEntries);

            string pathConcat = fullPath.Contains("api").IsFalse() ? "/" + path[0] + "/" + path[1] : "/" + string.Join("/", path);//"/" + path[0] + "/" + path[1] + "/" + path[2]; //"/" + string.Join("/", path); 
            if (fullPath.Contains("api"))
            {
                var requestData = context.ActionArguments["requestFromBody"];
                if (requestData != null)
                {
                    JObject requestBody = requestData as JObject;
                    var apiVersion = requestBody["APP_VERSION"]?.ToString();
                    if (apiVersion == null)
                    {
                        context.Result = ApiCoreUtility.ResponseAPPVersion();
                    }
                    else if (appSettings.APPVersion.IsNullOrEmpty() || apiVersion != appSettings.APPVersion)
                    {
                        context.Result = ApiCoreUtility.ResponseAPPVersion();
                    }
                }
            }
            if (pathConcat.Contains("/User/SignOut").IsFalse())
                if (AppContexts.User.UserID > 0 && isAnonymous.IsFalse())
                {

                    if (AppContexts.User.IsAdmin.IsFalse())
                    {
                        VerifyToken verifyToken = new VerifyToken();
                        var token = verifyToken.VerifyTokenHash(context.HttpContext);

                        IManager manager = new ManagerBase();
                        var blackList = manager.GetBlackListToken(token, AppContexts.User.UserID);
                        var list = manager.GetApiPath(pathConcat, AppContexts.User.UserID);

                        if ((list.Result.IsNotNull() && list.Result.Count == 0) || blackList.Result.Count > 0)
                        {
                            context.Result = new BadRequestObjectResult(context.ModelState);
                        }
                    }
                }
                else
                {
                    #region Token Hash  
                    if (pathConcat.Contains("ValidateUserWithToken").IsTrue())
                    {
                        IManager manager = new ManagerBase();

                        var requestData = context.ActionArguments["user"];
                        dynamic requestBody = requestData as dynamic;
                        var tokenhash = requestBody?.TokenHash;// requestBody["TokenHash"]?.ToString();

                        if ((tokenhash == null || tokenhash == "") && manager.CheckChangeValidUser(AppContexts.User.UserID) == false)
                        {
                            context.Result = new BadRequestObjectResult(context.ModelState);
                        }
                    }

                    #endregion Token Hash
                }
        }
    }
}
