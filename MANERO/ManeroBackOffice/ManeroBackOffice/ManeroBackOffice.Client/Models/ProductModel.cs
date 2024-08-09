using System.ComponentModel.DataAnnotations;

namespace ManeroBackOffice.Models
{
    public class ProductModel
    {
        [Required]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string BatchNumber { get; set; } = null!;

        [Required]
        public string Title { get; set; } = null!;

        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }

        [Required]
        public List<string> Categories { get; set; } = new List<string>();

        [Required]
        public string Color { get; set; } = null!;

        [Required]
        public string Size { get; set; } = null!;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public decimal Price { get; set; }

        [Required]
        [Url]
        public string ImageUrl { get; set; } = null!;
    }
}
