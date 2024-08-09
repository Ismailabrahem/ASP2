using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductProvider.Entities;

public class Color
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ColorName { get; set; } = null!;
}
