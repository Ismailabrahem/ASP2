using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.SizeHandler;

public class GetAllSizesTests
{
    private readonly ILogger<GetAllSizes> _logger;

    public GetAllSizesTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<GetAllSizes>();
    }

    private DataContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;

        var context = new DataContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task Run_ReturnsAllSizes()
    {
        // Arrange
        var context = CreateNewContext();
        var getAllSizesFunction = new GetAllSizes(_logger, context);

        var sizes = new List<Size>
        {
            new Size { Id = "test-id-1", SizeName = "Size 1" },
            new Size { Id = "test-id-2", SizeName = "Size 2" }
        };

        context.Sizes.AddRange(sizes);
        await context.SaveChangesAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getAllSizesFunction.Run(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSizes = Assert.IsType<List<Size>>(okResult.Value);

        Assert.Equal(2, returnedSizes.Count);
        Assert.Equal(sizes[0].Id, returnedSizes[0].Id);
        Assert.Equal(sizes[1].Id, returnedSizes[1].Id);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var getAllSizesFunction = new GetAllSizes(_logger, context);

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getAllSizesFunction.Run(request);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
