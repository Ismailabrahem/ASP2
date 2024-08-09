using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.CategoryHandler;

public class DeleteCategoryById
{
    private readonly ILogger<DeleteCategoryById> _logger;
    private readonly DataContext _context;

    public DeleteCategoryById(ILogger<DeleteCategoryById> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("DeleteCategoryById")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "categories/{id}")] HttpRequest req, string id)
    {
        try
        {
            var item = await _context.Categories.FindAsync(id);
            if (item == null)
            {
                return new NotFoundResult();
            }

            _context.Categories.Remove(item);
            await _context.SaveChangesAsync();

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
