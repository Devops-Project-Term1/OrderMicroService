using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Data;
using OrderService.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using Microsoft.EntityFrameworkCore;

public class ControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string TEST_TOKEN = 
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlRlc3QgVXNlciIsImlzcyI6IkF1dGhTZXJ2aWNlIiwiYXVkIjoiT3JkZXJTZXJ2aWNlIiwiaWF0IjoxNTE2MjM5MDIyfQ.KE3xfM61AJZ7p9-sDACr5n5fJ5fgIjnrQw7G9l0bVcE";
        // NOTE: This token must be valid and signed by the same key in appsettings.json!

    public ControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 1. Find and REMOVE the production DbContext options
                //    This removes the PostgreSQL configuration.
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // 2. Add the DbContext using the In-Memory provider for testing
                services.AddDbContext<OrderDbContext>(options =>
                {
                    // Use a unique database name for concurrency safety in tests
                    options.UseInMemoryDatabase("TestOrdersDbForController");
                });
                
                // CRITICAL: Ensure the database is created and seeded (if necessary)
                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var context = scopedServices.GetRequiredService<OrderDbContext>();
                    
                    // Ensure the database is clean and created for the test run
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }
            });
        });
    }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WithoutToken()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/orders");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_ReturnsOkAndCreatesOrder_WithValidToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", TEST_TOKEN);
        
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

        // Check if the UserId was correctly extracted from the token (sub: 1234567890)
        Assert.Equal("1234567890", createdOrder.UserId); 
    }
}

// Simple DTO for the test payload
public class OrderDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}