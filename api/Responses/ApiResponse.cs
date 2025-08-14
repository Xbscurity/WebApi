using api.Constants;

namespace api.Responses
{
    public class ApiResponse
    {
        public ApiError? Error { get; set; }

        public static ApiResponse<T> Success<T>(T data, Pagination pagination = null) =>
            new()
            {
                Data = data,
                Pagination = pagination,
            };

        public static ApiResponse<T> NotFound<T>(string message = "Entity Not Found", object errorData = null) =>
            new()
            {
                Error = new ApiError
                {
                    Message = message,
                    Data = errorData,
                    Code = ErrorCodes.NotFound,
                },
            };

        public static ApiResponse<T> BadRequest<T>(string message = "Invalid Request", object errorData = null) =>
            new()
            {
                Error = new ApiError
                {
                    Message = message,
                    Data = errorData,
                    Code = ErrorCodes.BadRequest,
                },
            };

        public static ApiResponse<T> Unauthorized<T>(string message = "Unauthorized access", object errorData = null) =>
    new()
    {
        Error = new ApiError
        {
            Message = message,
            Data = errorData,
            Code = ErrorCodes.Unauthorized,
        },
    };

        public static ApiResponse<T> Forbidden<T>(string message = "Access denied", object errorData = null) =>
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

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        public Pagination? Pagination { get; set; }
    }
}