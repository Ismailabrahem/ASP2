using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductProvider.Contexts;
using ProductProvider.Entities;

namespace ProductProvider.Functions.ProductHandler;

public class CreateCategory
{
    private readonly ILogger<CreateCategory> _logger;
    private readonly DataContext _context;

    public CreateCategory(ILogger<CreateCategory> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("CreateCategory")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var entity = JsonConvert.DeserializeObject<Category>(body);

            _context.Add(entity);
            await _context.SaveChangesAsync();

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new BadRequestResult();
        }

    }
}
