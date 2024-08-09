using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductProvider.Contexts;
using static System.Net.WebRequestMethods;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<DataContext>(x => x.UseCosmos("AccountEndpoint = https://products-cosmos.documents.azure.com:443/;AccountKey=MZWNSRJKt3GJ150JyaqfL4AVepjJVlY1oT3V1LUXxnmxvTanxCyfsNVJTYCZGPdw366cyqqd6MPOACDbC2zrVA==;", "Manero"));

    })
    .Build();

host.Run();
