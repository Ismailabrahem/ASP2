using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.ColorHandler;

public class GetColorById
{
    private readonly ILogger<GetColorById> _logger;
    private readonly DataContext _context;

    public GetColorById(ILogger<GetColorById> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("GetColorById")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "colors/{id}")] HttpRequest req, string id)
    {
        try
        {
            var item = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
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