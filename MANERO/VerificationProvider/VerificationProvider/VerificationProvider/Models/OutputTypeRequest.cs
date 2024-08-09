
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace VerificationProvider.Models;

public class OutputTypeRequest
{
    [ServiceBusOutput("email_provider", Connection = "ServiceBus")]
    public string OutputEvent { get; set; } = null!;
    public HttpResponseData HttpResponse { get; set; } = null!;

}
