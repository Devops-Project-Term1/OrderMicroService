using System.Text;
using System.Text.Json;

namespace OrderService.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StockService> _logger;

    public StockService(HttpClient httpClient, ILogger<StockService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ReduceStockAsync(int productId, int quantity, string reason)
    {
        try
        {
            var requestBody = new
            {
                quantity = -quantity, // Negative to reduce
                reason = reason
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"/stock/{productId}/adjust", content);
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to reduce stock for product {ProductId}: {Error}", productId, errorContent);
                return false;
            }
            
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Successfully reduced stock for product {ProductId} by {Quantity}", productId, quantity);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reducing stock for product {ProductId}", productId);
            return false;
        }
    }

    public async Task<StockDto?> GetStockByProductIdAsync(int productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/stock/{productId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
                
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var stock = JsonSerializer.Deserialize<StockDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return stock;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching stock for product {ProductId}", productId);
            return null;
        }
    }
}

public class StockDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
}
