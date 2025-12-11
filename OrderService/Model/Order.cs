namespace OrderService.Models;

public class Order
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    // The User/Service ID from the JWT token
    public string UserId { get; set; } = string.Empty; 
}