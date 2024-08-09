namespace Manero_WebApp.Models;

public class CartItem
{
    public string ProductId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public decimal Price { get; set; }

    public string Size { get; set; } = null!;
    public string Color { get; set; } = null!;

    public int Quantity { get; set; }

    public string ImageUrl { get; set; } = null!;
}
