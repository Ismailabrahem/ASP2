using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;

namespace ProductProvider.Functions.ColorHandler
{
    public class UpdateColorById
    {
        private readonly ILogger<UpdateColorById> _logger;
        private readonly DataContext _context;

        public UpdateColorById(ILogger<UpdateColorById> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("UpdateColorById")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "colors/{id}")] HttpRequest req, string id)
        {
            _logger.LogInformation("UpdateColorById function started.");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Request body read successfully: {Body}", requestBody);

                var updatedItem = JsonConvert.DeserializeObject<Color>(requestBody);
                if (updatedItem == null)
                {
                    _logger.LogWarning("Failed to deserialize request body to Color.");
                    return new BadRequestObjectResult("Invalid color data.");
                }

                var existingItem = await _context.Colors.FindAsync(id);
                if (existingItem == null)
                {
                    return new NotFoundResult();
                }

                existingItem.ColorName = updatedItem.ColorName;

                _context.Colors.Update(existingItem);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Color updated successfully: {Color}", existingItem);

                return new OkObjectResult(existingItem);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing request body.");
                return new BadRequestObjectResult("Invalid JSON format.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the color.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
