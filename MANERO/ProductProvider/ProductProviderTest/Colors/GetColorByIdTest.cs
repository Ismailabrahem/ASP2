using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ColorHandler;

public class GetColorByIdTests
{
    private readonly ILogger<GetColorById> _logger;

    public GetColorByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<GetColorById>();
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
        var getColorByIdFunction = new GetColorById(_logger, context);

        var color = new Color
        {
            Id = "test-id",
            ColorName = "Test Color"
        };

        context.Colors.Add(color);
        await context.SaveChangesAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getColorByIdFunction.Run(request, "test-id");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedColor = Assert.IsType<Color>(okResult.Value);

        Assert.Equal(color.Id, returnedColor.Id);
        Assert.Equal(color.ColorName, returnedColor.ColorName);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var getColorByIdFunction = new GetColorById(_logger, context);

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getColorByIdFunction.Run(request, "non-existent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var getColorByIdFunction = new GetColorById(_logger, context);

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
        var result = await getColorByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
