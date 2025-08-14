namespace api.Dtos.Account
{
    public class UserProfileDto
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

    }
}
