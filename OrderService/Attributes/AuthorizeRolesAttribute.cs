using Microsoft.AspNetCore.Authorization;

namespace OrderService.Attributes;

/// <summary>
/// Custom attribute to specify required roles for authorization
/// </summary>
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}
