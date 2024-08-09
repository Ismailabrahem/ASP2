using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.SizeHandler;

public class GetAllSizes
{
    private readonly ILogger<GetAllSizes> _logger;
    private readonly DataContext _context;

    public GetAllSizes(ILogger<GetAllSizes> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("GetAllSizes")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sizes/all")] HttpRequest req)
    {
        try
        {
            var items = await _context.Sizes.ToListAsync();
            return new OkObjectResult(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new NotFoundResult();
        }
    }
}
