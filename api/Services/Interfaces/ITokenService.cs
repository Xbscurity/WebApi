using api.Models;
using System.Runtime.CompilerServices;

namespace api.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser appUser);
    }
}
