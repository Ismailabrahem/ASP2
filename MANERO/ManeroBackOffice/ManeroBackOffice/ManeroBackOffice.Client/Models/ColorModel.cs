using System.ComponentModel.DataAnnotations;

namespace ManeroBackOffice.Models;

public class ColorModel
{
    [Required]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string ColorName { get; set; } = null!;
}
