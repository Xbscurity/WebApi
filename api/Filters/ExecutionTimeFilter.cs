using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

public class ExecutionTimeFilter : IActionFilter
{
    private Stopwatch _stopwatch;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _stopwatch = Stopwatch.StartNew();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _stopwatch.Stop();
        var elapsedMs = _stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"[{context.ActionDescriptor.DisplayName}] executed in {elapsedMs} ms");
    }
}

}