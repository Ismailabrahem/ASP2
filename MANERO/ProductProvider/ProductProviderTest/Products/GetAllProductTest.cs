using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ProductHandler;

public class GetAllProductsTests
{
    private readonly ILogger<GetAllProducts> _logger;

    public GetAllProductsTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<GetAllProducts>();
    }

    private DataContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique database for each test
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task Run_ReturnsAllProducts()
    {
        // Arrange
        var context = CreateNewContext();
        var getAllProductsFunction = new GetAllProducts(_logger, context);

        // Seed the in-memory database with test data
        var products = new List<Product>
        {
            new Product
            {
                Id = "test-id-1",
                BatchNumber = "batch-001",
                Title = "Test Product 1",
                ShortDescription = "Short description 1",
                LongDescription = "Long description 1",
                Categories = new List<string> { "Category1" },
                Color = "Red",
                Size = "L",
                Price = 19.99m,
                ImageUrl = "http://example.com/image1.png"
            },
            new Product
            {
                Id = "test-id-2",
                BatchNumber = "batch-002",
                Title = "Test Product 2",
                ShortDescription = "Short description 2",
                LongDescription = "Long description 2",
                Categories = new List<string> { "Category2" },
                Color = "Blue",
                Size = "M",
                Price = 29.99m,
                ImageUrl = "http://example.com/image2.png"
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getAllProductsFunction.Run(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProducts = Assert.IsType<List<Product>>(okResult.Value);

        Assert.Equal(2, returnedProducts.Count);
        Assert.Equal(products[0].Id, returnedProducts[0].Id);
        Assert.Equal(products[1].Id, returnedProducts[1].Id);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var getAllProductsFunction = new GetAllProducts(_logger, context);

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getAllProductsFunction.Run(request);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
