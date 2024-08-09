using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.SizeHandler;

public class GetSizeByIdTests
{
    private readonly ILogger<GetSizeById> _logger;

    public GetSizeByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<GetSizeById>();
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
    public async Task Run_ValidId_ReturnsOkObjectResult()
    {
        // Arrange
        var context = CreateNewContext();
        var getSizeByIdFunction = new GetSizeById(_logger, context);

        var size = new Size
        {
            Id = "test-id",
            SizeName = "Test Size"
        };

        context.Sizes.Add(size);
        await context.SaveChangesAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getSizeByIdFunction.Run(request, "test-id");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSize = Assert.IsType<Size>(okResult.Value);

        Assert.Equal(size.Id, returnedSize.Id);
        Assert.Equal(size.SizeName, returnedSize.SizeName);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var getSizeByIdFunction = new GetSizeById(_logger, context);

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getSizeByIdFunction.Run(request, "non-existent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var getSizeByIdFunction = new GetSizeById(_logger, context);

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
        var result = await getSizeByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
