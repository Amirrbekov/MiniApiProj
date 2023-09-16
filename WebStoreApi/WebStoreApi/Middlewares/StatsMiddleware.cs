using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebStoreApi.Middlewares;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class StatsMiddleware
{
    private readonly RequestDelegate _next;

    public StatsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext httpContext)
    {
        // handle the request (before executing the controller)
        DateTime requestTime = DateTime.Now;

        var result = _next(httpContext);

        // handle the response (after executing the controller)
        DateTime responseTime = DateTime.Now;
        TimeSpan processDuration = responseTime - requestTime;

        Console.WriteLine("[Inline Middleware] Process Duration" + processDuration.TotalMilliseconds + "ms");

        return result;
    }
}
