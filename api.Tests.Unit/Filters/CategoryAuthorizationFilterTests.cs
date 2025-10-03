using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

// ... other using statements for your project
public class FinancialTransactionAuthorizationTests
{
    public static ActionExecutingContext CreateActionExecutingContext(
        int categoryId,
        ClaimsPrincipal user,
        IServiceProvider serviceProvider)
    {
        var httpContext = new DefaultHttpContext { User = user };
        httpContext.RequestServices = serviceProvider;

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());

        var actionArguments = new Dictionary<string, object>
    {
        { "id", categoryId } // The filter looks for an argument named "id"
    };

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments,
            null! /* Controller */
        );
    }
}