using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.CategoryHandler;

public class DeleteCategoryByIdTests
{
    private readonly ILogger<DeleteCategoryById> _logger;

    public DeleteCategoryByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<DeleteCategoryById>();
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
        var deleteCategoryByIdFunction = new DeleteCategoryById(_logger, context);

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

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteCategoryByIdFunction.Run(request, "test-id");

        // Assert
        Assert.IsType<OkResult>(result);
        var deletedCategory = await context.Categories.FindAsync("test-id");
        Assert.Null(deletedCategory);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteCategoryByIdFunction = new DeleteCategoryById(_logger, context);

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteCategoryByIdFunction.Run(request, "non-existent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteCategoryByIdFunction = new DeleteCategoryById(_logger, context);

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

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteCategoryByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
