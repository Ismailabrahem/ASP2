using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ColorHandler;

public class DeleteColorByIdTests
{
    private readonly ILogger<DeleteColorById> _logger;

    public DeleteColorByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<DeleteColorById>();
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
    public async Task Run_ValidId_ReturnsOkResult()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteColorByIdFunction = new DeleteColorById(_logger, context);

        var color = new Color
        {
            Id = "test-id",
            ColorName = "Test Color"
        };

        context.Colors.Add(color);
        await context.SaveChangesAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteColorByIdFunction.Run(request, "test-id");

        // Assert
        Assert.IsType<OkResult>(result);
        var deletedColor = await context.Colors.FindAsync("test-id");
        Assert.Null(deletedColor);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteColorByIdFunction = new DeleteColorById(_logger, context);

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteColorByIdFunction.Run(request, "non-existent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteColorByIdFunction = new DeleteColorById(_logger, context);

        var color = new Color
        {
            Id = "test-id",
            ColorName = "Test Color"
        };

        context.Colors.Add(color);
        await context.SaveChangesAsync();

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteColorByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
