using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.SizeHandler;

public class DeleteSizeById
{
    private readonly ILogger<DeleteSizeById> _logger;
    private readonly DataContext _context;

    public DeleteSizeById(ILogger<DeleteSizeById> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("DeleteSizeById")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "sizes/{id}")] HttpRequest req, string id)
    {
        try
        {
            var item = await _context.Sizes.FindAsync(id);
            if (item == null)
            {
                return new NotFoundResult();
            }

            _context.Sizes.Remove(item);
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
