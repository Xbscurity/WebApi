using api.Models;

namespace api.Tests.Unit.Factories
{
    public class AppUserFactory
    {
        public static AppUser Create(
            string id = "user-1",
            string userName = "userName1",
            string email = "user@example.com",
            bool isBanned = false,
            DateTimeOffset? createdAt = null)
        {
            return new AppUser
            {
                Id = id,
                UserName = userName,
                Email = email,
                IsBanned = isBanned,
                CreatedAt = createdAt ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
        }
    }
}