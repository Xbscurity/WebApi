using api.Extensions;
using api.Services.Interfaces;
using System.Security.Claims;

namespace api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User?.GetUserId();

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
    }
}
