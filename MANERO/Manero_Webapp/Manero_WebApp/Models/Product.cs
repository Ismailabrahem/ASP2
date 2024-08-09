namespace Manero_WebApp.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public List<string> Sizes { get; set; } = null!;

    public List<string> Color { get; set; } = null!;

    public int Rating { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }
}
