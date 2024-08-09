using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.ColorHandler;

public class GetAllColors
{
    private readonly ILogger<GetAllColors> _logger;
    private readonly DataContext _context;

    public GetAllColors(ILogger<GetAllColors> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("GetAllColors")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "colors/all")] HttpRequest req)
    {
        try
        {
            var items = await _context.Colors.ToListAsync();
            return new OkObjectResult(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new NotFoundResult();
        }
    }
}
