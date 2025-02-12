using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Helpers
{
    public class ApiResponse
    {
        public Error Error { get; set; }
        public static ApiResponse<T> Success<T>(T data) => new() { Data = data };
        public static ApiResponse<T> NoContent<T>() => new(){};
        public static ApiResponse<T> NotFound<T>(string message = "Entity Not Found", object errorData = null) =>
            new() { Error = new Error { Code = "NOT_FOUND", Message = message, Data = errorData } };
    }
    public class ApiResponse<T> 
    {
         public Error Error { get; set; }
        public T Data { get; set; }
    }
}