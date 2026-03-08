using API.Core.Logging;
using Core.AppContexts;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace API.Core
{
    public class ErrorHandling
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public ErrorHandling(RequestDelegate next, ILogger logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var orgException = exception.GetOriginalException();
            var errorLog = WriteLog(context, exception);
            var err = new { Error = true, errorLog.ErrorId, StatusCode = StatusCodes.Status400BadRequest, orgException.Message };
            var result = JsonConvert.SerializeObject(err);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return context.Response.WriteAsync(result);
        }

        private ErrorLog WriteLog(HttpContext filterContext, Exception exception)
        {
            var errorId = DateTime.Now.ToString("hhmmssfff").ToInt();
            var errorLog = new ErrorLog
            {
                ErrorId = errorId,
                ErrorCode = exception.HResult,
                Url = filterContext.Request.Path.Value,
                ErrorType = exception.GetType().Name,
                ErrorMessage = $"Error: {exception.Message}, StackTrace: {exception.StackTrace}",
                ErrorBy = AppContexts.User.UserName,
                ErrorDate = DateTime.Now,
                ErrorIP = AppContexts.GetIPAddress()
            };

            logger.LogError(errorLog);
            return errorLog;
        }
    }
}
