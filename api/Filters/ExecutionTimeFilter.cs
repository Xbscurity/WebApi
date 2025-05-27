using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace api.Filters
{
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