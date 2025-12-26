using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;
    private readonly IStockService _stockService;
    private readonly IProductService _productService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        OrderDbContext context, 
        IStockService stockService, 
        IProductService productService,
        ILogger<OrderService> logger)
    {
        _context = context;
        _stockService = stockService;
        _productService = productService;
        _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        // Validate product exists
        var product = await _productService.GetProductByIdAsync(order.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {order.ProductId} not found");
        }

        // Check and reduce stock
        var stockReduced = await _stockService.ReduceStockAsync(
            order.ProductId, 
            order.Quantity, 
            $"Order created for product {order.ProductId}");
        
        if (!stockReduced)
        {
            throw new InvalidOperationException($"Insufficient stock for product {order.ProductId}");
        }

        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} created successfully for product {ProductId}", order.Id, order.ProductId);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order, attempting to rollback stock");
            // Rollback stock on failure (add back the quantity)
            await _stockService.ReduceStockAsync(order.ProductId, -order.Quantity, "Rollback: Order creation failed");
            throw;
        }
    }

    public async Task<Order?> UpdateOrderAsync(int id, Order order)
    {
        var existingOrder = await _context.Orders.FindAsync(id);
        if (existingOrder == null) return null;

        existingOrder.ProductId = order.ProductId;
        existingOrder.Quantity = order.Quantity;
        existingOrder.TotalPrice = order.TotalPrice;
        existingOrder.UserId = order.UserId;
        existingOrder.OrderDate = order.OrderDate;

        await _context.SaveChangesAsync();
        return existingOrder;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync()
    {
        return await _productService.GetAllProductsAsync();
    }
}