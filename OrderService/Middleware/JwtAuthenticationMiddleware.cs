using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace OrderService.Middleware;

/// <summary>
/// Custom middleware to validate and authenticate JWT tokens
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractTokenFromHeader(context);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
                    _logger.LogInformation("JWT token validated successfully for user: {UserId}", 
                        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                }
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("JWT token has expired");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token has expired");
                return;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("JWT token has invalid signature");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid token signature");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid token");
                return;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts JWT token from the Authorization header
    /// </summary>
    private string? ExtractTokenFromHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
            return null;

        // Check if it starts with "Bearer "
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }

    /// <summary>
    /// Validates the JWT token and returns ClaimsPrincipal if valid
    /// </summary>
    private ClaimsPrincipal? ValidateToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ClockSkew = TimeSpan.Zero, // Remove default 5 minute clock skew
            NameClaimType = "username",
            RoleClaimType = "role"
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        
        // Additional validation: ensure it's a JWT token
        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token algorithm");
        }

        // Map 'id' claim to NameIdentifier for consistency
        var claimsIdentity = principal.Identity as ClaimsIdentity;
        if (claimsIdentity != null)
        {
            var idClaim = claimsIdentity.FindFirst("id");
            if (idClaim != null && !claimsIdentity.HasClaim(ClaimTypes.NameIdentifier, idClaim.Value))
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, idClaim.Value));
            }
            
            var emailClaim = claimsIdentity.FindFirst("email");
            if (emailClaim != null && !claimsIdentity.HasClaim(ClaimTypes.Email, emailClaim.Value))
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, emailClaim.Value));
            }
        }

        return principal;
    }
}
