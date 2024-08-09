using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductProvider.Contexts;
using ProductProvider.Entities;
using System.Text.Json;

namespace ProductProvider.Functions.ProductHandler
{
    public class UpdateProductById
    {
        private readonly ILogger<UpdateProductById> _logger;
        private readonly DataContext _context;

        public UpdateProductById(ILogger<UpdateProductById> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("UpdateProductById")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{id}")] HttpRequest req, string id)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updatedItem = JsonSerializer.Deserialize<Product>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (updatedItem == null || updatedItem.Id != id)
                {
                    return new BadRequestResult();
                }

                var existingItem = await _context.Products.FindAsync(id);
                if (existingItem == null)
                {
                    return new NotFoundResult();
                }

                existingItem.BatchNumber = updatedItem.BatchNumber;
                existingItem.Title = updatedItem.Title;
                existingItem.ShortDescription = updatedItem.ShortDescription;
                existingItem.LongDescription = updatedItem.LongDescription;
                existingItem.Categories = updatedItem.Categories;
                existingItem.Color = updatedItem.Color;
                existingItem.Size = updatedItem.Size;
                existingItem.Price = updatedItem.Price;
                existingItem.ImageUrl = updatedItem.ImageUrl;

                await _context.SaveChangesAsync();

                return new OkObjectResult(existingItem);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing request body.");
                return new BadRequestResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the product.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
