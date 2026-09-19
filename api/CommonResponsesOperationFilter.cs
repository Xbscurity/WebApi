using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace api
{
    /// <summary>
    /// Documents standard <see cref="ProblemDetails"/> responses for API operations.
    /// </summary>
    /// <remarks>
    /// Responses are added based on the endpoint's parameters and authorization metadata.
    /// </remarks>
    public class CommonResponsesOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies common response definitions to the specified OpenAPI operation.
        /// </summary>
        /// <param name="operation">
        /// The OpenAPI operation to modify.
        /// </param>
        /// <param name="context">
        /// The context containing API description and endpoint metadata.
        /// </param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses ??= new OpenApiResponses();

            var schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);

            operation.Responses.TryAdd(
                StatusCodes.Status429TooManyRequests.ToString(),
                CreateProblemDetailsResponse("Rate limit exceeded. Please try again later.", schema));

            var hasQuery = context.ApiDescription.ParameterDescriptions
                .Any(p => p.Source == BindingSource.Query);

            var hasBody = context.ApiDescription.ParameterDescriptions
                .Any(p => p.Source == BindingSource.Body);

            if (hasBody || hasQuery)
            {
                operation.Responses.TryAdd(
                    StatusCodes.Status400BadRequest.ToString(),
                    CreateProblemDetailsResponse("The request is invalid or failed validation.", schema));
            }

            var hasAuthorize = context.ApiDescription.ActionDescriptor.EndpointMetadata
                .Any(em => em is AuthorizeAttribute);

            var hasAllowAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata
                .Any(em => em is AllowAnonymousAttribute);

            if (hasAuthorize && !hasAllowAnonymous)
            {
                operation.Responses.TryAdd(
                    StatusCodes.Status401Unauthorized.ToString(),
                    CreateProblemDetailsResponse("Valid JWT token is missing or expired.", schema));

                operation.Responses.TryAdd(
                    StatusCodes.Status403Forbidden.ToString(),
                    CreateProblemDetailsResponse("You do not have permission to access this resource.", schema));
            }
        }

        private static OpenApiResponse CreateProblemDetailsResponse(string description, IOpenApiSchema schema) =>
        new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = schema,
                },
            },
        };
    }
}
