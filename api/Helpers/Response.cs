using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Helpers
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public Error Error { get; set; }

        public static Response<T> SuccessResponse(T data) => new() { Success = true, Data = data };
        public static Response<T> NotFoundResponse(string message = "Entity Not Found",object errorData = null) =>
            new() { Success = false, Error = new Error { Code = "NOT_FOUND", Message = message, Data = errorData } };
    }
}