using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class CustomCorsMiddleware
{
    private readonly RequestDelegate _next;

    public CustomCorsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000"); // Specify allowed origin
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE"); // Specify allowed HTTP methods
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization"); // Specify allowed headers
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true"); // Allow credentials

        if (context.Request.Method == "OPTIONS")
        {
            context.Response.StatusCode = 200; // Preflight request successful
            return;
        }

        await _next(context);
    }
}

public static class CustomCorsMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomCors(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomCorsMiddleware>();
    }
}
