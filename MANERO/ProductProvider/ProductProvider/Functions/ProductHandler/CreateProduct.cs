using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;

namespace ProductProvider.Functions.ProductHandler
{
    public class CreateProduct
    {
        private readonly ILogger<CreateProduct> _logger;
        private readonly DataContext _context;

        public CreateProduct(ILogger<CreateProduct> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("CreateProduct")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("CreateProduct function started.");

            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Request body read successfully: {Body}", body);

                var entity = JsonConvert.DeserializeObject<Product>(body);
                if (entity == null)
                {
                    _logger.LogWarning("Failed to deserialize request body to Product.");
                    return new BadRequestObjectResult("Invalid product data.");
                }

                _context.Add(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product saved successfully: {Product}", entity);

                return new OkObjectResult(entity);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing request body.");
                return new BadRequestObjectResult("Invalid JSON format.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the product.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
