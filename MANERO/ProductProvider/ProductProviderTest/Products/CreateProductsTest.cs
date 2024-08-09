using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ProductHandler;
using System.Text;

public class CreateProductTests
{
    private readonly ILogger<CreateProduct> _logger;

    public CreateProductTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<CreateProduct>();
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
        var createProductFunction = new CreateProduct(_logger, context);

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

        var json = JsonConvert.SerializeObject(product);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await createProductFunction.Run(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);

        Assert.Equal(product.Id, returnedProduct.Id);
        Assert.Equal(product.Title, returnedProduct.Title);
        // Additional assertions for other properties
    }

    [Fact]
    public async Task Run_InvalidProduct_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var context = CreateNewContext();
        var createProductFunction = new CreateProduct(_logger, context);

        var invalidJson = "{ invalid json }";
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        request.ContentLength = invalidJson.Length;
        request.ContentType = "application/json";

        // Act
        var result = await createProductFunction.Run(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid JSON format.", badRequestResult.Value);
    }
}
