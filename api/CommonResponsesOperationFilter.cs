using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace api
{
    /// <summary>
    /// Adds common HTTP response definitions to Swagger/OpenAPI endpoint documentation.
    /// </summary>
    /// <remarks>
    /// This operation filter automatically appends standard responses
    /// based on endpoint characteristics:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c>400 Bad Request</c> for endpoints that accept request bodies.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>401 Unauthorized</c> for endpoints that require authentication.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>403 Forbidden</c> for authenticated users who do not have
    /// permission to access the resource.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// Authentication-related responses are added only to endpoints decorated with
    /// <see cref="AuthorizeAttribute"/> and not marked with
    /// <see cref="AllowAnonymousAttribute"/>.
    /// </para>
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
