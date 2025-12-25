using OrderService.Data;
using OrderService.Models;
using OrderService.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ServiceTests
{
    // Helper method to create a fresh In-Memory DbContext for each test
    private OrderDbContext GetDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;
        return new OrderDbContext(options);
    }

    [Fact]
    public async Task CreateOrderAsync_AddsOrderToDatabase()
    {
        // Arrange
        using var context = GetDbContext("CreateTestDb");
        var service = new OrderService.Services.OrderService(context);
        var newOrder = new Order 
        { 
            ProductId = 123, 
            Quantity = 10, 
            TotalPrice = 50.00m, 
            UserId = "user123" 
        };

        // Act
        var createdOrder = await service.CreateOrderAsync(newOrder);

        // Assert
        Assert.NotEqual(0, createdOrder.Id); // ID should be generated
        var savedOrder = await context.Orders.FindAsync(createdOrder.Id);
        Assert.NotNull(savedOrder);
        Assert.Equal(123, savedOrder.ProductId);
    }

    [Fact]
    public async Task DeleteOrderAsync_RemovesOrderFromDatabase()
    {
        // Arrange
        using var context = GetDbContext("DeleteTestDb");
        var service = new OrderService.Services.OrderService(context);
        
        // Seed an order
        var orderToDelete = new Order { Id = 1, ProductId = 456, Quantity = 1 };
        context.Orders.Add(orderToDelete);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteOrderAsync(1);

        // Assert
        Assert.True(result);
        var deletedOrder = await context.Orders.FindAsync(1);
        Assert.Null(deletedOrder);
    }
}