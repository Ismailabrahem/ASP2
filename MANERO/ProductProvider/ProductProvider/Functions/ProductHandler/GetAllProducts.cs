using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.ProductHandler
{
    public class GetAllProducts
    {
        private readonly ILogger<GetAllProducts> _logger;
        private readonly DataContext _context;

        public GetAllProducts(ILogger<GetAllProducts> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("GetAllProducts")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/all")] HttpRequest req)
        {
            try
            {
                var items = await _context.Products.ToListAsync();
                return new OkObjectResult(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the products.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
