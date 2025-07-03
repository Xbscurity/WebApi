using api.Models;

namespace api.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser appUser);
    }
}
