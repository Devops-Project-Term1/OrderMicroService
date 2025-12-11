using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Data;
using OrderService.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class ControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"ConnectionStrings:DefaultConnection", ""},
                }!);
            });
            
            builder.ConfigureServices(services =>
            {
                // Remove EntityFrameworkCore services registered with PostgreSQL
                var descriptors = services.Where(d =>
                    d.ServiceType.Name.Contains("DbContext") ||
                    d.ServiceType.Name.Contains("EntityFramework")).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Add the DbContext using the In-Memory provider for testing
                services.AddDbContext<OrderDbContext>(options =>
                    options.UseInMemoryDatabase("TestOrdersDbForController"),
                    ServiceLifetime.Scoped);
            });
        });
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/orders");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_ReturnsCreatedAndCreatesOrder()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var order = new OrderDto { ProductId = "TestItem", Quantity = 1, TotalPrice = 10.00m };
        var content = new StringContent(
            JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/orders", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var createdOrder = JsonSerializer.Deserialize<Order>(responseString, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(createdOrder);
        Assert.Equal("TestItem", createdOrder.ProductId);
    }
}

// Simple DTO for the test payload
public class OrderDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}