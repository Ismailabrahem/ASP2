using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.SizeHandler;

public class DeleteSizeByIdTests
{
    private readonly ILogger<DeleteSizeById> _logger;

    public DeleteSizeByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<DeleteSizeById>();
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
        var deleteSizeByIdFunction = new DeleteSizeById(_logger, context);

        var size = new Size
        {
            Id = "test-id",
            SizeName = "Test Size"
        };

        context.Sizes.Add(size);
        await context.SaveChangesAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteSizeByIdFunction.Run(request, "test-id");

        // Assert
        Assert.IsType<OkResult>(result);
        var deletedSize = await context.Sizes.FindAsync("test-id");
        Assert.Null(deletedSize);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteSizeByIdFunction = new DeleteSizeById(_logger, context);

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteSizeByIdFunction.Run(request, "non-existent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var deleteSizeByIdFunction = new DeleteSizeById(_logger, context);

        var size = new Size
        {
            Id = "test-id",
            SizeName = "Test Size"
        };

        context.Sizes.Add(size);
        await context.SaveChangesAsync();

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await deleteSizeByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
