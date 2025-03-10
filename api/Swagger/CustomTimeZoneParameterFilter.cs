using System.Reflection.Metadata.Ecma335;
using api.Dtos;
using Microsoft.Identity.Client.AppConfig;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace api.Swagger
{
    public class CustomTimeZoneParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parameters = context.MethodInfo.GetParameters();
        bool hasTimeZoneInfo = parameters.Any(p => p.ParameterType == typeof(TimeZoneRequest));

        if (hasTimeZoneInfo)
        {
            var parametersToRemove = operation.Parameters
                .Where(p => p.Name.Contains(nameof(TimeZoneRequest.TimeZone)))
                .ToList();  
            foreach (var parameter in parametersToRemove)
            {
                operation.Parameters.Remove(parameter);
            }

        }
    }
}

}