using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;
using OrderService.Utilities;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
#if !DEBUG
[Authorize]
#endif
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly JwtTokenValidator _jwtTokenValidator;

    public OrdersController(IOrderService orderService, JwtTokenValidator jwtTokenValidator)
    {
        _orderService = orderService;
        _jwtTokenValidator = jwtTokenValidator;
    }

    /// <summary>
    /// Get all orders - requires User or Admin role
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "UserOrAdmin")]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Get order by ID - requires authentication
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    /// <summary>
    /// Create a new order - requires User or Admin role
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "UserOrAdmin")]
    public async Task<IActionResult> Create([FromBody] Order order)
    {
        // Extract User ID from the JWT Token Claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userId != null)
        {
            order.UserId = userId;
        }

        var createdOrder = await _orderService.CreateOrderAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
    }

    /// <summary>
    /// Delete order - requires Admin role only
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _orderService.DeleteOrderAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Get current user's claims from JWT token
    /// </summary>
    [HttpGet("me/claims")]
    public IActionResult GetMyClaims()
    {
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        
        if (string.IsNullOrEmpty(token))
            return Unauthorized("No token provided");

        var claims = _jwtTokenValidator.GetAllClaims(token);
        var userId = _jwtTokenValidator.GetUserIdFromToken(token);
        var roles = _jwtTokenValidator.GetUserRolesFromToken(token);
        var expiration = _jwtTokenValidator.GetTokenExpiration(token);

        return Ok(new
        {
            UserId = userId,
            Roles = roles,
            TokenExpiration = expiration,
            AllClaims = claims
        });
    }
}