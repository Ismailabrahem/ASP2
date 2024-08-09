using System.ComponentModel.DataAnnotations.Schema;

namespace Manero_WebApp.Models;

public class ProductEntity
{
    public string Id { get; set; } = null!;
    public string BatchNumber { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public List<string> Categories { get; set; } = new();
    public string Color { get; set; } = null!;
    public string Size { get; set; } = null!;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = null!;

    [NotMapped]
    public List<string> Colors { get; set; } = new();

    [NotMapped]
    public List<string> Sizes { get; set; } = new();

}
