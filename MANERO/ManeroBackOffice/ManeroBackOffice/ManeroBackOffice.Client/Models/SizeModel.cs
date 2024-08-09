using System.ComponentModel.DataAnnotations;

namespace ManeroBackOffice.Models
{
    public class SizeModel
    {
        [Required]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string SizeName { get; set; } = null!;
    }
}
