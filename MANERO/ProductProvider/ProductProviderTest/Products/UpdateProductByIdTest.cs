using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ProductHandler;
using System.Text;
using System.Text.Json;

public class UpdateProductByIdTests
{
    private readonly ILogger<UpdateProductById> _logger;

    public UpdateProductByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UpdateProductById>();
    }

    private DataContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique database for each test
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task Run_ValidProduct_ReturnsOkObjectResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateProductByIdFunction = new UpdateProductById(_logger, context);

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

        var updatedProduct = new Product
        {
            Id = "test-id",
            BatchNumber = "batch-002",
            Title = "Updated Test Product",
            ShortDescription = "Updated short description",
            LongDescription = "Updated long description",
            Categories = new List<string> { "Category1", "Category2" },
            Color = "Blue",
            Size = "M",
            Price = 29.99m,
            ImageUrl = "http://example.com/updated_image.png"
        };

        var json = JsonSerializer.Serialize(updatedProduct);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateProductByIdFunction.Run(request, "test-id");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);

        Assert.Equal(updatedProduct.Id, returnedProduct.Id);
        Assert.Equal(updatedProduct.Title, returnedProduct.Title);
        // Additional assertions for other properties
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateProductByIdFunction = new UpdateProductById(_logger, context);

        var updatedProduct = new Product
        {
            Id = "invalid-id",
            BatchNumber = "batch-002",
            Title = "Updated Test Product",
            ShortDescription = "Updated short description",
            LongDescription = "Updated long description",
            Categories = new List<string> { "Category1", "Category2" },
            Color = "Blue",
            Size = "M",
            Price = 29.99m,
            ImageUrl = "http://example.com/updated_image.png"
        };

        var json = JsonSerializer.Serialize(updatedProduct);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateProductByIdFunction.Run(request, "invalid-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_InvalidProduct_ReturnsBadRequestResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateProductByIdFunction = new UpdateProductById(_logger, context);

        var invalidJson = "{ invalid json }";
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        request.ContentLength = invalidJson.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateProductByIdFunction.Run(request, "test-id");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var updateProductByIdFunction = new UpdateProductById(_logger, context);

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

        var updatedProduct = new Product
        {
            Id = "test-id",
            BatchNumber = "batch-002",
            Title = "Updated Test Product",
            ShortDescription = "Updated short description",
            LongDescription = "Updated long description",
            Categories = new List<string> { "Category1", "Category2" },
            Color = "Blue",
            Size = "M",
            Price = 29.99m,
            ImageUrl = "http://example.com/updated_image.png"
        };

        var json = JsonSerializer.Serialize(updatedProduct);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        // Act
        var result = await updateProductByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
