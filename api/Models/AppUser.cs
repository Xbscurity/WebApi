﻿using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    public class AppUser : IdentityUser
    {
        public bool IsBanned { get; set; }
    }
}
