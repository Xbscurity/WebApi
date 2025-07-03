namespace api.Dtos.UserManagement
{
    public class UserDto
    {
        public string Id { get; set; } = default!;

        public string UserName { get; set; } = default!;

        public string Email { get; set; } = default!;

        public bool IsBanned { get; set; }
    }
}
