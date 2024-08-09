using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Models;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider)
{
    private readonly ILogger<GenerateVerificationCode> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [Function(nameof(GenerateVerificationCode))]
    [ServiceBusOutput("email_provider", Connection = "ServiceBus")]
    public async Task<string> Run([ServiceBusTrigger("verification_provider", Connection = "ServiceBus")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var request = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
            if (request != null)
            {
                var existingCode = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == request.Email);
            }
                
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : Run :: {ex.Message}");
        }
        return null!;
    }
}
