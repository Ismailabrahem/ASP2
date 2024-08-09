using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ProductHandler;
using System.Text;

public class CreateCategoryTests
{
    private readonly ILogger<CreateCategory> _logger;

    public CreateCategoryTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<CreateCategory>();
    }

    private DataContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique database for each test
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task Run_ValidCategory_ReturnsOkResult()
    {
        // Arrange
        var context = CreateNewContext();
        var createCategoryFunction = new CreateCategory(_logger, context);

        var category = new Category
        {
            Id = "test-id",
            CategoryName = "Test Category",
            SubCategories = new List<Category>
            {
                new Category { Id = "sub-id-1", CategoryName = "Sub Category 1" },
                new Category { Id = "sub-id-2", CategoryName = "Sub Category 2" }
            }
        };

        var json = JsonConvert.SerializeObject(category);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await createCategoryFunction.Run(request);

        // Assert
        Assert.IsType<OkResult>(result);

        var createdCategory = await context.Categories.FindAsync("test-id");
        Assert.NotNull(createdCategory);
        Assert.Equal(category.Id, createdCategory.Id);
        Assert.Equal(category.CategoryName, createdCategory.CategoryName);
        Assert.Equal(category.SubCategories.Count, createdCategory.SubCategories.Count);
    }

    [Fact]
    public async Task Run_InvalidCategory_ReturnsBadRequestResult()
    {
        // Arrange
        var context = CreateNewContext();
        var createCategoryFunction = new CreateCategory(_logger, context);

        var invalidJson = "{ invalid json }";
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        request.ContentLength = invalidJson.Length;
        request.ContentType = "application/json";

        // Act
        var result = await createCategoryFunction.Run(request);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsBadRequestResult()
    {
        // Arrange
        var context = CreateNewContext();
        var createCategoryFunction = new CreateCategory(_logger, context);

        var category = new Category
        {
            Id = "test-id",
            CategoryName = "Test Category",
            SubCategories = new List<Category>
            {
                new Category { Id = "sub-id-1", CategoryName = "Sub Category 1" },
                new Category { Id = "sub-id-2", CategoryName = "Sub Category 2" }
            }
        };

        var json = JsonConvert.SerializeObject(category);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        // Act
        var result = await createCategoryFunction.Run(request);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
}
