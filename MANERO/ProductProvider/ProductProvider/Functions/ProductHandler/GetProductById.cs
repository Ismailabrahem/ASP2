using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.ProductHandler;

public class GetProductById
{
    private readonly ILogger<GetProductById> _logger;
    private readonly DataContext _context;

    public GetProductById(ILogger<GetProductById> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("GetProductById")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id}")] HttpRequest req, string id)
    {
        try
        {
            var item = await _context.Products.FindAsync(id);
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
