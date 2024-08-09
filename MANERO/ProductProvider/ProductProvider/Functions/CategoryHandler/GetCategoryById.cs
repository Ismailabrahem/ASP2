using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.CategoryHandler;

public class GetCategoryById
{
    private readonly ILogger<GetCategoryById> _logger;
    private readonly DataContext _context;

    public GetCategoryById(ILogger<GetCategoryById> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("GetCategoryById")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{id}")] HttpRequest req, string id)
    {
        try
        {
            var item = await _context.Categories.FindAsync(id);
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
