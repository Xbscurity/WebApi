using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace api.Dtos.Account
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password {  get; set; }  
    }
}
