using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.CategoryHandler;

public class GetAllCategoriesTests
{
    private readonly ILogger<GetAllCategories> _logger;

    public GetAllCategoriesTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<GetAllCategories>();
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
    public async Task Run_ReturnsAllCategories()
    {
        // Arrange
        var context = CreateNewContext();
        var getAllCategoriesFunction = new GetAllCategories(_logger, context);

        // Clear the database to ensure a clean state
        context.Categories.RemoveRange(context.Categories);
        await context.SaveChangesAsync();

        // Seed the in-memory database with test data
        var category = new Category
        {
            Id = "test-id-1",
            CategoryName = "Test Category 1",
            SubCategories = new List<Category>
            {
                new Category { Id = "sub-id-1", CategoryName = "Sub Category 1" }
            }
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        // Log the state of the database after seeding
        _logger.LogInformation("Database seeded with {Count} categories.", await context.Categories.CountAsync());

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getAllCategoriesFunction.Run(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategories = Assert.IsType<List<Category>>(okResult.Value);

        // Since we expect SubCategories to be treated as separate entries, ensure there's more than one entry
        Assert.Equal(2, returnedCategories.Count);

        // Validate main category and its subcategory
        var mainCategory = returnedCategories.First(c => c.Id == "test-id-1");
        Assert.Equal("Test Category 1", mainCategory.CategoryName);
        Assert.Single(mainCategory.SubCategories);
        Assert.Equal("sub-id-1", mainCategory.SubCategories[0].Id);
        Assert.Equal("Sub Category 1", mainCategory.SubCategories[0].CategoryName);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var getAllCategoriesFunction = new GetAllCategories(_logger, context);

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getAllCategoriesFunction.Run(request);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
