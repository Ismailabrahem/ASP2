using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ColorHandler;
using System.Text;

public class UpdateColorByIdTests
{
    private readonly ILogger<UpdateColorById> _logger;

    public UpdateColorByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UpdateColorById>();
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
    public async Task Run_ValidColor_ReturnsOkObjectResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateColorByIdFunction = new UpdateColorById(_logger, context);

        var color = new Color
        {
            Id = "test-id",
            ColorName = "Test Color"
        };

        context.Colors.Add(color);
        await context.SaveChangesAsync();

        var updatedColor = new Color
        {
            Id = "test-id",
            ColorName = "Updated Test Color"
        };

        var json = JsonConvert.SerializeObject(updatedColor);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateColorByIdFunction.Run(request, "test-id");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedColor = Assert.IsType<Color>(okResult.Value);

        Assert.Equal(updatedColor.Id, returnedColor.Id);
        Assert.Equal(updatedColor.ColorName, returnedColor.ColorName);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateColorByIdFunction = new UpdateColorById(_logger, context);

        var updatedColor = new Color
        {
            Id = "invalid-id",
            ColorName = "Updated Test Color"
        };

        var json = JsonConvert.SerializeObject(updatedColor);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateColorByIdFunction.Run(request, "invalid-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_InvalidJson_ReturnsBadRequestResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateColorByIdFunction = new UpdateColorById(_logger, context);

        var invalidJson = "{ invalid json }";
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        request.ContentLength = invalidJson.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateColorByIdFunction.Run(request, "test-id");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid JSON format.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var updateColorByIdFunction = new UpdateColorById(_logger, context);

        var color = new Color
        {
            Id = "test-id",
            ColorName = "Test Color"
        };

        context.Colors.Add(color);
        await context.SaveChangesAsync();

        var updatedColor = new Color
        {
            Id = "test-id",
            ColorName = "Updated Test Color"
        };

        var json = JsonConvert.SerializeObject(updatedColor);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        // Act
        var result = await updateColorByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
