using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            bool hasTimeZoneInfo = parameters.Any(p => p.ParameterType == typeof(TimeZoneInfo));
            if (hasTimeZoneInfo)
            {
                var timeZoneFieldsToRemove = new HashSet<string>
        {
            "Id", "HasIanaId", "DisplayName", "StandardName", "DaylightName", "BaseUtcOffset", "SupportsDaylightSavingTime"
        };
                operation.Parameters = operation.Parameters
                    .Where(p => !timeZoneFieldsToRemove.Contains(p.Name))
                    .ToList();

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "timezone",
                    In = ParameterLocation.Query,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Example = new OpenApiString("Eastern Standard Time")
                    },
                    Description = "Enter the time zone identifier (e.g., 'UTC' or 'Pacific Standard Time')"
                });
            }
        }
    }
}