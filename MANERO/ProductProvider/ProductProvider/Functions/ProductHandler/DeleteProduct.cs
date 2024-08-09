using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;

namespace ProductProvider.Functions.ProductHandler
{
    public class DeleteProductById
    {
        private readonly ILogger<DeleteProductById> _logger;
        private readonly DataContext _context;

        public DeleteProductById(ILogger<DeleteProductById> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("DeleteProductById")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id}")] HttpRequest req, string id)
        {
            try
            {
                var item = await _context.Products.FindAsync(id);
                if (item == null)
                {
                    return new NotFoundResult();
                }

                _context.Products.Remove(item);
                await _context.SaveChangesAsync();

                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the product.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
