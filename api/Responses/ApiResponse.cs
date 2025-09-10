using api.Constants;

namespace api.Responses
{
    /// <summary>
    /// Represents a standardized API response.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Gets or sets error details if the request failed; otherwise <c>null</c>.
        /// </summary>
        public ApiError? Error { get; set; }

        /// <summary>
        /// Creates a successful response with the specified data.
        /// </summary>
        /// <typeparam name="T">The type of the data returned.</typeparam>
        /// <param name="data">The payload of the response.</param>
        /// <param name="pagination">Optional pagination details if the response is paged.</param>
        /// <returns>A new <see cref="ApiResponse{T}"/> representing a successful response.</returns>
        public static ApiResponse<T> Success<T>(T data, Pagination? pagination = null) =>
            new()
            {
                Data = data,
                Pagination = pagination,
            };

        /// <summary>
        /// Creates a response indicating that the requested resource was not found.
        /// </summary>
        /// <typeparam name="T">The expected data type of the response.</typeparam>
        /// <param name="message">Optional error message (default: "Entity Not Found").</param>
        /// <param name="errorData">Optional additional error details.</param>
        /// <returns>A new <see cref="ApiResponse{T}"/> representing a not found response.</returns>
        public static ApiResponse<T> NotFound<T>(string message = "Entity Not Found", object? errorData = null) =>
            new()
            {
                Error = new ApiError
                {
                    Message = message,
                    Data = errorData,
                    Code = ErrorCodes.NotFound,
                },
            };

        /// <summary>
        /// Creates a response indicating a bad request.
        /// </summary>
        /// <typeparam name="T">The expected data type of the response.</typeparam>
        /// <param name="message">Optional error message (default: "Invalid Request").</param>
        /// <param name="errorData">Optional additional error details.</param>
        /// <returns>A new <see cref="ApiResponse{T}"/> representing a bad request response.</returns>
        public static ApiResponse<T> BadRequest<T>(string message = "Invalid Request", object? errorData = null) =>
            new()
            {
                Error = new ApiError
                {
                    Message = message,
                    Data = errorData,
                    Code = ErrorCodes.BadRequest,
                },
            };

        /// <summary>
        /// Creates a response indicating unauthorized access.
        /// </summary>
        /// <typeparam name="T">The expected data type of the response.</typeparam>
        /// <param name="message">Optional error message (default: "Unauthorized access").</param>
        /// <param name="errorData">Optional additional error details.</param>
        /// <returns>A new <see cref="ApiResponse{T}"/> representing an unauthorized response.</returns>
        public static ApiResponse<T> Unauthorized<T>(string message = "Unauthorized access", object? errorData = null) =>
            new()
            {
                Error = new ApiError
                {
                    Message = message,
                    Data = errorData,
                    Code = ErrorCodes.Unauthorized,
                },
            };

        /// <summary>
        /// Creates a response indicating forbidden access.
        /// </summary>
        /// <typeparam name="T">The expected data type of the response.</typeparam>
        /// <param name="message">Optional error message (default: "Access denied").</param>
        /// <param name="errorData">Optional additional error details.</param>
        /// <returns>A new <see cref="ApiResponse{T}"/> representing a forbidden response.</returns>
        public static ApiResponse<T> Forbidden<T>(string message = "Access denied", object? errorData = null) =>
            new()
            {
                Error = new ApiError
                {
                    Message = message,
                    Data = errorData,
                    Code = ErrorCodes.Forbidden,
                },
            };
    }

    /// <summary>
    /// Represents a standardized API response that contains typed data.
    /// </summary>
    /// <typeparam name="T">The type of the data included in the response.</typeparam>
    public class ApiResponse<T> : ApiResponse
    {
        /// <summary>
        /// Gets or sets the payload of the response. Available when the request is successful.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets pagination details if the response contains a paged collection.
        /// </summary>
        public Pagination? Pagination { get; set; }
    }
}
