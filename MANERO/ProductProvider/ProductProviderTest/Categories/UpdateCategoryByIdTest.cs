using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.CategoryHandler;
using System.Text;

public class UpdateCategoryByIdTest
{
    private readonly ILogger<UpdateCategoryById> _logger;

    public UpdateCategoryByIdTest()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UpdateCategoryById>();
    }

    private DataContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique database for each test
            .Options;

        var context = new DataContext(options);
        context.Database.EnsureDeleted(); // Ensure the database is deleted before use
        context.Database.EnsureCreated(); // Ensure the database is created
        return context;
    }

    [Fact]
    public async Task Run_ValidCategory_ReturnsOkObjectResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateCategoryByIdFunction = new UpdateCategoryById(_logger, context);

        var category = new Category
        {
            Id = "test-id",
            CategoryName = "Test Category",
            SubCategories = new List<Category>
            {
                new Category { Id = "sub-id-1", CategoryName = "Sub Category 1" }
            }
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var updatedCategory = new Category
        {
            Id = "test-id",
            CategoryName = "Updated Test Category"
        };

        var json = JsonConvert.SerializeObject(updatedCategory);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateCategoryByIdFunction.Run(request, "test-id");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<Category>(okResult.Value);

        Assert.Equal(updatedCategory.Id, returnedCategory.Id);
        Assert.Equal(updatedCategory.CategoryName, returnedCategory.CategoryName);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateCategoryByIdFunction = new UpdateCategoryById(_logger, context);

        var updatedCategory = new Category
        {
            Id = "invalid-id",
            CategoryName = "Updated Test Category"
        };

        var json = JsonConvert.SerializeObject(updatedCategory);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateCategoryByIdFunction.Run(request, "invalid-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_InvalidJson_ReturnsBadRequestResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateCategoryByIdFunction = new UpdateCategoryById(_logger, context);

        var invalidJson = "{ invalid json }";
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        request.ContentLength = invalidJson.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateCategoryByIdFunction.Run(request, "test-id");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid JSON format.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var updateCategoryByIdFunction = new UpdateCategoryById(_logger, context);

        var category = new Category
        {
            Id = "test-id",
            CategoryName = "Test Category",
            SubCategories = new List<Category>
            {
                new Category { Id = "sub-id-1", CategoryName = "Sub Category 1" }
            }
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var updatedCategory = new Category
        {
            Id = "test-id",
            CategoryName = "Updated Test Category"
        };

        var json = JsonConvert.SerializeObject(updatedCategory);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        // Act
        var result = await updateCategoryByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
