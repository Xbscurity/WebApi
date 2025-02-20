using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace api.Helpers
{
    public class ApiResponse
    {
        public Error Error { get; set; }
        public static ApiResponse<T> Success<T>(T data) => new() { Data = data};
        public static ApiResponse<T> NotFound<T>(string message = "Entity Not Found", object errorData = null) =>
            new() { Error = new Error {Message = message, Data = errorData, Code = "NOT_FOUND"}};
    }
    public class ApiResponse<T> : ApiResponse
    {

        public T Data { get; set; }

    }
}