using Microsoft.AspNetCore.Authorization;

public class CategoryAccessRequirement : IAuthorizationRequirement
{
    public bool AllowGlobal { get; }

    public CategoryAccessRequirement(bool allowGlobal = false)
    {
        AllowGlobal = allowGlobal;
    }
}
