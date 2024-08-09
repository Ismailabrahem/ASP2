using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;

namespace ProductProvider.Functions.CategoryHandler
{
    public class UpdateCategoryById
    {
        private readonly ILogger<UpdateCategoryById> _logger;
        private readonly DataContext _context;

        public UpdateCategoryById(ILogger<UpdateCategoryById> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("UpdateCategoryById")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "categories/{id}")] HttpRequest req, string id)
        {
            _logger.LogInformation("UpdateCategoryById function started.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Request body read successfully: {Body}", requestBody);

                var updatedItem = JsonConvert.DeserializeObject<Category>(requestBody);
                if (updatedItem == null)
                {
                    _logger.LogWarning("Failed to deserialize request body to Category.");
                    return new BadRequestObjectResult("Invalid category data.");
                }

                var existingItem = await _context.Categories.FindAsync(id);
                if (existingItem == null)
                {
                    return new NotFoundResult();
                }

                existingItem.CategoryName = updatedItem.CategoryName;

                _context.Categories.Update(existingItem);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Category updated successfully: {Category}", existingItem);

                return new OkObjectResult(existingItem);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing request body.");
                return new BadRequestObjectResult("Invalid JSON format.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the category.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
