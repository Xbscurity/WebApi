
namespace api.Helpers
{
    public class ApiResponse
    {
        public Error? Error { get; set; }

        public static ApiResponse<T> Success<T>(T data, Pagination pagination = null) =>
            new()
            {
                Data = data,
                Pagination = pagination,
            };

        public static ApiResponse<T> NotFound<T>(string message = "Entity Not Found", object errorData = null) =>
            new()
            {
                Error = new Error
                {
                    Message = message,
                    Data = errorData,
                    Code = "NOT_FOUND",
                },
            };

        public static ApiResponse<T> BadRequest<T>(string message = "Invalid Request", object errorData = null) =>
            new()
            {
                Error = new Error
                {
                    Message = message,
                    Data = errorData,
                    Code = "BAD_REQUEST",
                },
            };

        public static ApiResponse<T> Unauthorized<T>(string message = "Unauthorized access", object errorData = null) =>
    new()
    {
        Error = new Error
        {
            Code = "UNAUTHORIZED",
            Message = message,
            Data = errorData,
        },
    };

        public static ApiResponse<T> Forbidden<T>(string message = "Access denied", object errorData = null) =>
    new()
    {
        Error = new Error
        {
            Code = "FORBIDDEN",
            Message = message,
            Data = errorData,
        },
    };
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        public Pagination? Pagination { get; set; }
    }
}