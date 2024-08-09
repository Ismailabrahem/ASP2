using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ProductHandler;

public class DeleteProductByIdTests
{
    private readonly ILogger<DeleteProductById> _logger;

    public DeleteProductByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<DeleteProductById>();
    }

    private DataContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique database for each test
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task Run_ValidId_ReturnsOkResult()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteProductByIdFunction = new DeleteProductById(_logger, context);

        var product = new Product
        {
            Id = "test-id",
            BatchNumber = "batch-001",
            Title = "Test Product",
            ShortDescription = "Short description",
            LongDescription = "Long description",
            Categories = new List<string> { "Category1" },
            Color = "Red",
            Size = "L",
            Price = 19.99m,
            ImageUrl = "http://example.com/image.png"
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteProductByIdFunction.Run(request, "test-id");

        // Assert
        Assert.IsType<OkResult>(result);
        var deletedProduct = await context.Products.FindAsync("test-id");
        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteProductByIdFunction = new DeleteProductById(_logger, context);

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteProductByIdFunction.Run(request, "non-existent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteProductByIdFunction = new DeleteProductById(_logger, context);

        var product = new Product
        {
            Id = "test-id",
            BatchNumber = "batch-001",
            Title = "Test Product",
            ShortDescription = "Short description",
            LongDescription = "Long description",
            Categories = new List<string> { "Category1" },
            Color = "Red",
            Size = "L",
            Price = 19.99m,
            ImageUrl = "http://example.com/image.png"
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteProductByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
