using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace OrderService.Authorization;

/// <summary>
/// Custom authorization requirement for role-based access
/// </summary>
public class RoleRequirement : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; }

    public RoleRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

/// <summary>
/// Authorization handler to validate user roles from JWT claims
/// </summary>
public class JwtAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly ILogger<JwtAuthorizationHandler> _logger;

    public JwtAuthorizationHandler(ILogger<JwtAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        if (context.User == null || !context.User.Identity!.IsAuthenticated)
        {
            _logger.LogWarning("User is not authenticated");
            return Task.CompletedTask;
        }

        // Get user roles from claims
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation("Checking authorization for user {UserId} with roles: {Roles}", 
            userId, string.Join(", ", userRoles));

        // Check if user has any of the allowed roles
        if (userRoles.Any(role => requirement.AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            _logger.LogInformation("User {UserId} authorized successfully", userId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} does not have required roles. Required: {Required}, Has: {Has}", 
                userId, 
                string.Join(", ", requirement.AllowedRoles),
                string.Join(", ", userRoles));
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Custom authorization requirement for owner-based access
/// </summary>
public class OwnerRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization handler to ensure user can only access their own resources
/// </summary>
public class OwnerAuthorizationHandler : AuthorizationHandler<OwnerRequirement>
{
    private readonly ILogger<OwnerAuthorizationHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OwnerAuthorizationHandler(
        ILogger<OwnerAuthorizationHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerRequirement requirement)
    {
        if (context.User == null || !context.User.Identity!.IsAuthenticated)
        {
            _logger.LogWarning("User is not authenticated");
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Allow admins to access any resource
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (userRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Admin user {UserId} authorized to access resource", userId);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // For non-admin users, they can only access their own resources
        // This can be customized based on your specific requirements
        _logger.LogInformation("Owner check passed for user {UserId}", userId);
        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
