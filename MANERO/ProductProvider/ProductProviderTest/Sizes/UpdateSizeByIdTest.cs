using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using ProductProvider.Functions.SizeHandler;
using System.Text;

public class UpdateSizeByIdTests
{
    private readonly ILogger<UpdateSizeById> _logger;

    public UpdateSizeByIdTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UpdateSizeById>();
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
    public async Task Run_ValidSize_ReturnsOkObjectResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateSizeByIdFunction = new UpdateSizeById(_logger, context);

        var size = new Size
        {
            Id = "test-id",
            SizeName = "Test Size"
        };

        context.Sizes.Add(size);
        await context.SaveChangesAsync();

        var updatedSize = new Size
        {
            Id = "test-id",
            SizeName = "Updated Test Size"
        };

        var json = JsonConvert.SerializeObject(updatedSize);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateSizeByIdFunction.Run(request, "test-id");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSize = Assert.IsType<Size>(okResult.Value);

        Assert.Equal(updatedSize.Id, returnedSize.Id);
        Assert.Equal(updatedSize.SizeName, returnedSize.SizeName);
    }

    [Fact]
    public async Task Run_InvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateSizeByIdFunction = new UpdateSizeById(_logger, context);

        var updatedSize = new Size
        {
            Id = "invalid-id",
            SizeName = "Updated Test Size"
        };

        var json = JsonConvert.SerializeObject(updatedSize);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateSizeByIdFunction.Run(request, "invalid-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Run_InvalidJson_ReturnsBadRequestResult()
    {
        // Arrange
        var context = CreateNewContext();
        var updateSizeByIdFunction = new UpdateSizeById(_logger, context);

        var invalidJson = "{ invalid json }";
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        request.ContentLength = invalidJson.Length;
        request.ContentType = "application/json";

        // Act
        var result = await updateSizeByIdFunction.Run(request, "test-id");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid JSON format.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_DatabaseError_ReturnsStatusCode500()
    {
        // Arrange
        var context = CreateNewContext();
        var updateSizeByIdFunction = new UpdateSizeById(_logger, context);

        var size = new Size
        {
            Id = "test-id",
            SizeName = "Test Size"
        };

        context.Sizes.Add(size);
        await context.SaveChangesAsync();

        var updatedSize = new Size
        {
            Id = "test-id",
            SizeName = "Updated Test Size"
        };

        var json = JsonConvert.SerializeObject(updatedSize);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        request.ContentType = "application/json";

        // Simulate database error by disposing the context
        await context.DisposeAsync();

        // Act
        var result = await updateSizeByIdFunction.Run(request, "test-id");

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
