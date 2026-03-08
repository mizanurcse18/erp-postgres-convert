using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Core.AppContexts;
using Manager.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace API.Core
{
    public class RestrictStaticFilesMiddleware
    {
        private readonly RequestDelegate _next;

        public RestrictStaticFilesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //public async Task InvokeAsync(HttpContext context)
        //{
        //    // Check if the request is for a static file (you can customize this check)

        //    if (context.Request.Path.ToString().Contains("/upload/attachments"))
        //    {
        //        IManager manager = new ManagerBase();
        //        var user = AppContexts.User.UserID;
        //        bool isValid = manager.GetValidatePath(context.Request.Path);
        //        string userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        string ipAddress = context.Connection.LocalIpAddress?.ToString();
        //        VerifyToken verifyToken = new VerifyToken();
        //        var token = verifyToken.VerifyTokenHash(context);
        //        if (!isValid)
        //        {
        //            if (!context.User.Identity.IsAuthenticated)
        //            {
        //                await context.ChallengeAsync();
        //                return;
        //            }
        //        }
        //    }

        //    await _next(context);
        //}



        public async Task InvokeAsync(HttpContext context)
        {
            //if (context.Request.Path.ToString().Contains("/upload/attachments"))
            
            if ((context.Request.Path.ToString().Contains("/upload/attachments")) && !context.Request.Path.ToString().Contains("MicroSite"))
            {
                IManager manager = new ManagerBase();
                var user = AppContexts.User.UserID;
                int employeeID = AppContexts.User.EmployeeID != null  ? (int)AppContexts.User.EmployeeID : 0;


                if (context.Request.Headers.ContainsKey("X-Link-Click") ||
                context.Request.Query.ContainsKey("linkClick"))
                    {
                        await _next(context);
                        return;
                    }

                if (user > 0)
                {
                    bool hasListPermission = false;
                    string encodedPath = context.Request.Path.ToString();
                    string path = HttpUtility.UrlDecode(encodedPath);
                    bool strAllListPermission = path.StartsWith("/ALL");                    
                    if (strAllListPermission)
                    {
                        path = path.Substring("/ALL".Length);
                        VerifyToken verifyToken = new VerifyToken();
                        var token = verifyToken.VerifyTokenHash(context);
                        var blackList = manager.GetBlackListToken(token, user);
                        if(blackList.Result.Count == 0)
                        {
                            hasListPermission = true;
                        }
                    }

                    bool isValid = hasListPermission || manager.GetValidatePath(employeeID, path);

                    if (!isValid && !context.User.Identity.IsAuthenticated)
                    {
                        await context.ChallengeAsync();
                    }


                    var data = new
                    {
                        Result = new
                        {
                            IsValid = isValid,
                            Path = path
                        }
                    };

                    var jsonResponse = JsonConvert.SerializeObject(data);
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync(jsonResponse);
                }
                else
                {
                    await context.ChallengeAsync();
                    return;
                }
            }
            
            await _next(context);
        }





    }
}
