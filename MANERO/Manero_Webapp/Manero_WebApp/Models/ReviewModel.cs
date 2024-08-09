namespace Manero_WebApp.Models;

public class ReviewModel
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Comment { get; set; } = null!;

    public int Rating { get; set; }

    public DateTime Date { get; set; }

    public string ProductBatchNumber { get; set; } = null!;
}
