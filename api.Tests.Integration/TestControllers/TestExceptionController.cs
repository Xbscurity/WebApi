using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Tests.Integration.TestControllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("test/testexception")]
    public class TestExceptionController : ControllerBase
    {
        [HttpGet("throw")]
        public IActionResult ThrowException()
        {
            throw new InvalidOperationException("This is a test exception");
        }
    }
}