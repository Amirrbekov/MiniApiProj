using Microsoft.AspNetCore.Mvc.Filters;

namespace WebStoreApi.Filters;

public class DebugFilterAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        DateTime reference = new DateTime(2023, 1, 1);
        TimeSpan time = DateTime.Now - reference;
        Console.WriteLine("[DebugFilterAttribute] OnActionExecuting - Time: " + time.TotalMilliseconds + "ms");
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {
        DateTime reference = new DateTime(2023, 1, 1);
        TimeSpan time = DateTime.Now - reference;
        Console.WriteLine("[DebugFilterAttribute] OnActionExecuted - Time: " + time.TotalMilliseconds + "ms");
    }
}
