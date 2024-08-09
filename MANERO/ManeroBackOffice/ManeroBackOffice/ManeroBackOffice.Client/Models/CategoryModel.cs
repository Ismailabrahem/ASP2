using System.ComponentModel.DataAnnotations;

namespace ManeroBackOffice.Models
{
    public class CategoryModel
    {
        [Required]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CategoryName { get; set; } = null!;

        public List<string> SubCategories { get; set; } = new List<string>();
    }
}
