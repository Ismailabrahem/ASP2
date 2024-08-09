using Manero_WebApp.Models;


namespace Manero_WebApp.Services;

public class ProductService(HttpClient httpClient, ILogger<ProductService> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<ProductService> _logger = logger;

    public ProductService() : this(new HttpClient(), new LoggerFactory().CreateLogger<ProductService>())
    {
    }
    public async Task<List<ProductEntity>> GetAllProductsAsync()
    {
        var products = await _httpClient.GetFromJsonAsync<List<ProductEntity>>("http://localhost:7181/api/products/all");
        return products ?? new List<ProductEntity>();
    }

    public virtual async Task<ProductEntity?> GetAllProductByIdAsync(string productId)
    {
        var response = await _httpClient.GetAsync($" http://localhost:7181/api/products/{productId}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to fetch product with id {productId}. Status Code: {response.StatusCode}");
            return null;
        }

        var productJson = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"Product JSON: {productJson}");

        var product = await response.Content.ReadFromJsonAsync<ProductEntity>();
        if (product == null)
        {
            _logger.LogError($"Product with id {productId} not found.");
        }

        return product;
    }

    public async Task<List<ProductEntity>> GetProductsByShortDescriptionAndBatchAsync(string shortDescription, string batchNumber)
    {
        var products = await GetAllProductsAsync();
        var filteredProducts = products
            .Where(f => f.ShortDescription == shortDescription && f.BatchNumber == batchNumber)
            .ToList();
        return filteredProducts;
    }



}
