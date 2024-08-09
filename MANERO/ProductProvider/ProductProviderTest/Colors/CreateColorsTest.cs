using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.ColorHandler;
using System.Text;

public class CreateColorTests
{
    private readonly ILogger<CreateColor> _logger;

    public CreateColorTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<CreateColor>();
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
    public async Task Run_ValidColor_ReturnsOkResult()
    {
        // Arrange
        var context = CreateNewContext();
        var createColorFunction = new CreateColor(_logger, context);

        var color = new Color
        {
            Id = "test-id",
            ColorName = "Test Color"
        };

        var json = JsonConvert.SerializeObject(color);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await createColorFunction.Run(request);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Run_InvalidJson_ReturnsBadRequestResult()
    {
        // Arrange
        var context = CreateNewContext();
        var createColorFunction = new CreateColor(_logger, context);

        var invalidJson = "{ invalid json }";
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        request.ContentLength = invalidJson.Length;
        request.ContentType = "application/json";

        // Act
        var result = await createColorFunction.Run(request);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
}
