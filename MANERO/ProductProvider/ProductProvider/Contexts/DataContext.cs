using Microsoft.EntityFrameworkCore;
using ProductProvider.Entities;

namespace ProductProvider.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Size> Sizes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().ToContainer("Products").HasPartitionKey(x => x.BatchNumber);
        modelBuilder.Entity<Category>().ToContainer("Categories").HasPartitionKey(x => x.CategoryName);
        modelBuilder.Entity<Color>().ToContainer("Colors").HasPartitionKey(x => x.ColorName);
        modelBuilder.Entity<Size>().ToContainer("Sizes").HasPartitionKey(x => x.SizeName);
    }
}
