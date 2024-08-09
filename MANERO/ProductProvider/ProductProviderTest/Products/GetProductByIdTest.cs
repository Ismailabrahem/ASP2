using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ProductHandler;

public class GetProductByIdTests
{
    private readonly ILogger<GetProductById> _logger;

    public GetProductByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<GetProductById>();
    }

    [Fact]
    public async Task Run_ValidId_ReturnsOkObjectResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "ProductProvider")
            .Options;

        var context = new DataContext(options);
        var getProductByIdFunction = new GetProductById(_logger, context);

        // Clear the database to ensure a clean state
        context.Products.RemoveRange(context.Products);
        await context.SaveChangesAsync();

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
        var result = await getProductByIdFunction.Run(request, "test-id");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);

        Assert.Equal(product.Id, returnedProduct.Id);
        Assert.Equal(product.Title, returnedProduct.Title);
        // Additional assertions for other properties
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "ProductProvider")
            .Options;

        var context = new DataContext(options);
        var getProductByIdFunction = new GetProductById(_logger, context);

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getProductByIdFunction.Run(request, "non-existent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "ProductProviderWithError")
            .Options;

        var context = new DataContext(options);
        var getProductByIdFunction = new GetProductById(_logger, context);

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
        var result = await getProductByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
