using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.CategoryHandler;

public class GetCategoryByIdTests
{
    private readonly ILogger<GetCategoryById> _logger;

    public GetCategoryByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<GetCategoryById>();
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
    public async Task Run_ValidId_ReturnsOkObjectResult()
    {
        // Arrange
        var context = CreateNewContext();
        var getCategoryByIdFunction = new GetCategoryById(_logger, context);

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

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getCategoryByIdFunction.Run(request, "test-id");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<Category>(okResult.Value);

        Assert.Equal(category.Id, returnedCategory.Id);
        Assert.Equal(category.CategoryName, returnedCategory.CategoryName);
        Assert.Single(returnedCategory.SubCategories);
        Assert.Equal(category.SubCategories[0].Id, returnedCategory.SubCategories[0].Id);
        Assert.Equal(category.SubCategories[0].CategoryName, returnedCategory.SubCategories[0].CategoryName);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var getCategoryByIdFunction = new GetCategoryById(_logger, context);

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getCategoryByIdFunction.Run(request, "non-existent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var getCategoryByIdFunction = new GetCategoryById(_logger, context);

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

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getCategoryByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
