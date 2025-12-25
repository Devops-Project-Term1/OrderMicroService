using System.Net.Http.Json;
using OrderUI.Models;

namespace OrderUI.Services;

public class OrderService
{
    private readonly HttpClient _httpClient;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        try
        {
            var orders = await _httpClient.GetFromJsonAsync<List<Order>>("api/orders");
            return orders ?? new List<Order>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching orders: {ex.Message}");
            return new List<Order>();
        }
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Order>($"api/orders/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching order {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<Order?> CreateOrderAsync(Order order)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/orders", order);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating order: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateOrderAsync(int id, Order order)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/orders/{id}", order);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating order {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/orders/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting order {id}: {ex.Message}");
            return false;
        }
    }
}
