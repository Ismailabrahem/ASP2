using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.ColorHandler;

public class DeleteColorById
{
    private readonly ILogger<DeleteColorById> _logger;
    private readonly DataContext _context;

    public DeleteColorById(ILogger<DeleteColorById> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("DeleteColorById")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "colors/{id}")] HttpRequest req, string id)
    {
        try
        {
            var item = await _context.Colors.FindAsync(id);
            if (item == null)
            {
                return new NotFoundResult();
            }

            _context.Colors.Remove(item);
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