using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ColorHandler;

public class GetAllColorsTests
{
    private readonly ILogger<GetAllColors> _logger;

    public GetAllColorsTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<GetAllColors>();
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
    public async Task Run_ReturnsAllColors()
    {
        // Arrange
        var context = CreateNewContext();
        var getAllColorsFunction = new GetAllColors(_logger, context);

        var colors = new List<Color>
        {
            new Color { Id = "test-id-1", ColorName = "Color 1" },
            new Color { Id = "test-id-2", ColorName = "Color 2" }
        };

        context.Colors.AddRange(colors);
        await context.SaveChangesAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getAllColorsFunction.Run(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedColors = Assert.IsType<List<Color>>(okResult.Value);

        Assert.Equal(2, returnedColors.Count);
        Assert.Equal(colors[0].Id, returnedColors[0].Id);
        Assert.Equal(colors[1].Id, returnedColors[1].Id);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var getAllColorsFunction = new GetAllColors(_logger, context);

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        var request = new DefaultHttpContext().Request;

        // Act
        var result = await getAllColorsFunction.Run(request);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
