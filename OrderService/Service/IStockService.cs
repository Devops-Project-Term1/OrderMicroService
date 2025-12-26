namespace OrderService.Services;

public interface IStockService
{
    Task<bool> ReduceStockAsync(int productId, int quantity, string reason);
    Task<StockDto?> GetStockByProductIdAsync(int productId);
}
