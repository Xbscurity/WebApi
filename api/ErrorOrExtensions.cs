using api.Constants;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace api
{
    /// <summary>
    /// Provides extension methods for converting <c>ErrorOr</c> results
    /// into ASP.NET Core <see cref="ActionResult"/> responses.
    /// </summary>
    public static class ErrorOrExtensions
    {
        /// <summary>
        /// Converts an <c>ErrorOr</c> result into an HTTP <see cref="ActionResult"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the successful result value.
        /// </typeparam>
        /// <param name="result">
        /// The result to convert.
        /// </param>
        /// <param name="controller">
        /// The controller used to create HTTP responses.
        /// </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <see cref="OkObjectResult"/> when the operation succeeds.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// A formatted error response when the operation fails.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public static ActionResult ToActionResult<T>(this ErrorOr<T> result, ControllerBase controller)
        {
            return result.Match(
            value => controller.Ok(value),
            errors => CreateErrorResult(controller, errors));
        }

        /// <summary>
        /// Converts an <c>ErrorOr</c> result into an HTTP
        /// <see cref="NoContentResult"/> response when successful.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the successful result value.
        /// </typeparam>
        /// <param name="errorOr">
        /// The result to convert.
        /// </param>
        /// <param name="controller">
        /// The controller used to create HTTP responses.
        /// </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <see cref="NoContentResult"/> when the operation succeeds.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// A formatted error response when the operation fails.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public static ActionResult ToNoContentResult<T>(this ErrorOr<T> errorOr, ControllerBase controller)
        {
            return errorOr.Match(
                _ => controller.NoContent(),
                errors => CreateErrorResult(controller, errors));
        }

        /// <summary>
        /// Creates a standardized HTTP error response
        /// from a collection of <see cref="Error"/> instances.
        /// </summary>
        /// <param name="controller">
        /// The controller used to create the response.
        /// </param>
        /// <param name="errors">
        /// The collection of errors to include in the response.
        /// </param>
        /// <returns>
        /// An <see cref="ObjectResult"/> containing RFC 9110 compliant
        /// <see cref="ProblemDetails"/> data.
        /// </returns>
        private static ActionResult CreateErrorResult(ControllerBase controller, List<Error> errors)
        {
            if (errors.All(e => e.Type == ErrorType.Validation))
            {
                var groupedErrors = errors
            .GroupBy(e => e.Metadata?.GetValueOrDefault(ErrorMetadataKeys.Field)?.ToString() ?? "general")
            .ToDictionary(
                g => g.Key,
                g => g.Select(e =>
                {
                    var errorResponse = new Dictionary<string, object?>
                    {
                        ["code"] = e.Code,
                        ["description"] = e.Description,
                    };

                    if (e.Metadata is not null)
                    {
                        foreach (var kvp in e.Metadata)
                        {
                            if (kvp.Key == ErrorMetadataKeys.Field)
                            {
                                continue;
                            }

                            errorResponse[kvp.Key] = kvp.Value;
                        }
                    }

                    return errorResponse;
                }));

                var validationProblemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Instance = controller.HttpContext.Request.Path,
                    Extensions = { ["errors"] = groupedErrors },
                };

                return new ObjectResult(validationProblemDetails)
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                };
            }

            var firstError = errors[0];
            var statusCode = GetStatusCode(firstError.Type);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(firstError.Type),
                Type = GetRfcType(firstError.Type),
                Detail = firstError.Description,
                Instance = controller.HttpContext.Request.Path,
            };

            if (firstError.Metadata is not null)
            {
                foreach (var kv in firstError.Metadata)
                {
                    problemDetails.Extensions[kv.Key] = kv.Value;
                }
            }

            problemDetails.Extensions["errorCode"] = firstError.Code;
            problemDetails.Extensions["traceId"] = controller.HttpContext.TraceIdentifier;

            if (errors.Count > 1)
            {
                problemDetails.Extensions["errors"] = errors.Select(e => new { e.Code, e.Description });
            }

            return new ObjectResult(problemDetails) { StatusCode = statusCode };
        }

        /// <summary>
        /// Maps an <see cref="ErrorType"/> to an HTTP status code.
        /// </summary>
        /// <param name="type">
        /// The error type to map.
        /// </param>
        /// <returns>
        /// The corresponding HTTP status code.
        /// </returns>
        private static int GetStatusCode(ErrorType type) => type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        /// <summary>
        /// Gets a human-readable title for the specified <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="type">
        /// The error type.
        /// </param>
        /// <returns>
        /// A short descriptive title for the error response.
        /// </returns>
        private static string GetTitle(ErrorType type) => type switch
        {
            ErrorType.NotFound => "Not Found",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.Conflict => "Conflict",
            ErrorType.Unexpected => "An error occurred while processing your request",
            _ => "An error occurred"
        };

        /// <summary>
        /// Gets the RFC 9110 reference URL associated with the specified
        /// <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="type">
        /// The error type.
        /// </param>
        /// <returns>
        /// A URL referencing the relevant RFC 9110 specification section.
        /// </returns>
        private static string GetRfcType(ErrorType type) => type switch
        {
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            ErrorType.Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };
    }
}
