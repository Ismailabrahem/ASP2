using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Models;

namespace VerificationProvider.Functions;

public class ValidateCode(ILogger<ValidateCode> logger, DataContext context)
{
    private readonly ILogger<ValidateCode> _logger = logger;
    private readonly DataContext _context = context;

    [Function("ValidateCode")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {

        try
        {   
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var ir = JsonConvert.DeserializeObject<ValidateRequest>(body);

            if (ir != null && !string.IsNullOrEmpty(ir.Code))
            {
                var result = await _context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == ir.Email && x.Code == ir.Code);
                if (result != null)
                {

                    _context.Remove(result);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return new OkResult();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error saving changes: {ex.Message}");
                        return new StatusCodeResult(500); // return a 500 error
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ValidateCode ::" + ex.Message);
        }
        return new UnauthorizedResult();
    }
}
  