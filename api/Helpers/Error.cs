using api.Enums;

namespace api.Helpers
{
    public class Error
    {
        public ErrorCodes Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}