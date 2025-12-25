# JWT Authentication & Authorization Guide

This document explains the JWT authentication and authorization implementation in the OrderService.

## Features Implemented

### 1. JWT Authentication Middleware (`JwtAuthenticationMiddleware.cs`)
Custom middleware that:
- Extracts JWT tokens from `Authorization` header
- Validates token signature, expiration, issuer, and audience
- Sets the `HttpContext.User` with claims from the token
- Returns appropriate 401 errors for invalid/expired tokens

### 2. Authorization Handlers (`JwtAuthorizationHandler.cs`)

#### RoleRequirement & JwtAuthorizationHandler
- Validates user roles from JWT claims
- Supports multiple role requirements
- Logs authorization attempts

#### OwnerRequirement & OwnerAuthorizationHandler
- Ensures users can only access their own resources
- Automatically grants access to Admin users
- Customizable for resource-specific ownership validation

### 3. JWT Token Validator Utility (`JwtTokenValidator.cs`)
Provides helper methods to:
- `ValidateToken(token)` - Validate JWT and return ClaimsPrincipal
- `GetUserIdFromToken(token)` - Extract user ID
- `GetUserRolesFromToken(token)` - Extract user roles
- `GetEmailFromToken(token)` - Extract email
- `GetUsernameFromToken(token)` - Extract username
- `IsTokenExpired(token)` - Check if token is expired
- `GetTokenExpiration(token)` - Get expiration datetime
- `GetAllClaims(token)` - Get all claims as dictionary

### 4. Authorization Policies
Three pre-configured policies in `Program.cs`:
- **AdminOnly** - Requires Admin role
- **UserOrAdmin** - Requires User OR Admin role
- **OwnerOnly** - Requires resource ownership or Admin role

## Configuration

### appsettings.json
```json
{
  "JwtSettings": {
    "SecretKey": "ThisIsASecretKeyThatShouldBeVeryLongAndSecure123!",
    "Issuer": "AuthService",
    "Audience": "OrderService"
  }
}
```

**Important**: Change the `SecretKey` in production and store it securely (use Azure Key Vault, AWS Secrets Manager, etc.).

## Usage Examples

### 1. Using Authorization Policies in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication for all endpoints
public class OrdersController : ControllerBase
{
    // Requires User or Admin role
    [HttpGet]
    [Authorize(Policy = "UserOrAdmin")]
    public async Task<IActionResult> GetAll() { }

    // Requires Admin role only
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id) { }
    
    // Requires resource ownership or Admin role
    [HttpPut("{id}")]
    [Authorize(Policy = "OwnerOnly")]
    public async Task<IActionResult> Update(int id) { }
}
```

### 2. Using JWT Token Validator

```csharp
public class OrdersController : ControllerBase
{
    private readonly JwtTokenValidator _jwtTokenValidator;

    public OrdersController(JwtTokenValidator jwtTokenValidator)
    {
        _jwtTokenValidator = jwtTokenValidator;
    }

    [HttpGet("validate")]
    public IActionResult ValidateMyToken()
    {
        var token = Request.Headers["Authorization"]
            .FirstOrDefault()?.Replace("Bearer ", "");
        
        if (_jwtTokenValidator.IsTokenExpired(token))
            return Unauthorized("Token expired");

        var userId = _jwtTokenValidator.GetUserIdFromToken(token);
        var roles = _jwtTokenValidator.GetUserRolesFromToken(token);
        
        return Ok(new { UserId = userId, Roles = roles });
    }
}
```

### 3. Accessing Claims in Controllers

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] Order order)
{
    // Extract user information from claims
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
    var userName = User.FindFirst(ClaimTypes.Name)?.Value;
    
    order.UserId = userId;
    
    return Ok(order);
}
```

### 4. Using Custom Authorization Attribute

```csharp
using OrderService.Attributes;

[HttpGet("admin-only")]
[AuthorizeRoles("Admin")]
public IActionResult AdminOnlyEndpoint() 
{
    return Ok("Admin access granted");
}

[HttpGet("multi-role")]
[AuthorizeRoles("Admin", "Manager", "Supervisor")]
public IActionResult MultiRoleEndpoint() 
{
    return Ok("Access granted");
}
```

## Testing with cURL

### Valid Request with JWT Token
```bash
curl -X GET "https://localhost:5001/api/orders" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### Get Current User Claims
```bash
curl -X GET "https://localhost:5001/api/orders/me/claims" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## JWT Token Format

Your JWT token should include these claims:
```json
{
  "sub": "user123",              // User ID (ClaimTypes.NameIdentifier)
  "email": "user@example.com",   // Email (ClaimTypes.Email)
  "name": "John Doe",            // Username (ClaimTypes.Name)
  "role": ["User", "Admin"],     // Roles (ClaimTypes.Role)
  "iss": "AuthService",          // Issuer
  "aud": "OrderService",         // Audience
  "exp": 1735689600,             // Expiration timestamp
  "iat": 1735603200              // Issued at timestamp
}
```

## Middleware Options

The custom `JwtAuthenticationMiddleware` is available but commented out by default since the built-in JWT authentication is configured. To enable it:

In `Program.cs`, uncomment:
```csharp
app.UseMiddleware<JwtAuthenticationMiddleware>();
```

**Note**: The custom middleware provides additional logging and error handling but is optional.

## Error Responses

| Status Code | Scenario |
|-------------|----------|
| 401 Unauthorized | No token provided, invalid token, expired token, invalid signature |
| 403 Forbidden | Valid token but insufficient permissions (wrong role) |

## Security Best Practices

1. **Secret Key**: Use a strong, random secret key (at least 256 bits)
2. **HTTPS Only**: Always use HTTPS in production
3. **Token Expiration**: Set appropriate token expiration times
4. **Secure Storage**: Store JWT secrets in secure vault services
5. **Clock Skew**: The implementation sets `ClockSkew = TimeSpan.Zero` for strict expiration validation
6. **Algorithm Validation**: Only accepts HS256 algorithm tokens

## Extending Authorization

To add custom authorization logic:

1. Create a new requirement:
```csharp
public class CustomRequirement : IAuthorizationRequirement
{
    public string SomeProperty { get; set; }
}
```

2. Create a handler:
```csharp
public class CustomHandler : AuthorizationHandler<CustomRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomRequirement requirement)
    {
        // Your authorization logic
        if (/* condition met */)
            context.Succeed(requirement);
        
        return Task.CompletedTask;
    }
}
```

3. Register in `Program.cs`:
```csharp
builder.Services.AddScoped<IAuthorizationHandler, CustomHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomPolicy", policy =>
        policy.Requirements.Add(new CustomRequirement()));
});
```

## Troubleshooting

### Token validation fails
- Check that `SecretKey`, `Issuer`, and `Audience` match between token generation and validation
- Verify token hasn't expired
- Ensure token algorithm is HS256

### Authorization denied despite valid token
- Check user has required roles in JWT claims
- Verify role claim type matches `ClaimTypes.Role`
- Check logs for detailed authorization failure reasons

### Claims not found
- Ensure JWT token includes the expected claims
- Verify claim types match standard ClaimTypes constants
