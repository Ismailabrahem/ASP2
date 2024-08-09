using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.SizeHandler;

public class GetSizeById
{
    private readonly ILogger<GetSizeById> _logger;
    private readonly DataContext _context;

    public GetSizeById(ILogger<GetSizeById> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("GetSizeById")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sizes/{id}")] HttpRequest req, string id)
    {
        try
        {
            var item = await _context.Sizes.FindAsync(id);
            if (item == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
