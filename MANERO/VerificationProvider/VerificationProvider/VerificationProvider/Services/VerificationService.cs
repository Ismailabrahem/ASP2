﻿using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationService(ILogger<VerificationService> logger, IServiceProvider serviceProvider, ServiceBusClient serviceBusClient )
{
    private readonly ILogger<VerificationService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;

 
    public string GeneratedCode()
    {
        try
        {
            var rnd = new Random();
            var code = rnd.Next(100000, 999999);
            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GeneratedCode :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> SaveVerificationRequest(string email, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();
            var existingRequest = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == email);
            if (existingRequest != null)
            {
                existingRequest.Code = code;
                existingRequest.ExpiryDate = DateTime.Now.AddMinutes(5);
                context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                context.VerificationRequests.Add(new() { Email = email, Code = code });
            }
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GeneratedCode :: {ex.Message}");
        }
        return false!;
    }

    public EmailRequest GenerateEmailRequestEmail(string email, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(code))
            {
                var request = new EmailRequest
                {
                    To = email,
                    Subject = $"Verification code {code}",
                    Body = $@"
                    <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Verification Code</title>
                        </head>
                        <body>
                            <div style='color: #191919; max-width: 500px'>
                                <div style='background-color: #4f85f6; color: white; text-align: center; padding: 20px 0;'>
                                    <h1 style='font-weight: 400;'>Verification Code</h1>
                                </div>
                                <div style='background-color: #f4f4f4; padding: 1rem 2rem;'>
                                    <p>Dear Manero user,</p>
                                    <p>We recieved a request to sign in to your Manero account using e-mail {email}. Please verify your account using this verification code:</p>
                                    <p class='code' style='font-weight: 700; text-align: center; font-size: 48px; letter-spacing: 8px;'>
                                        {code}
                                    </p>
                                    <div class='noreply' style='color: #191919; font-size: 11px;'>
                                        <p>If you did not request this code, it is possible that someone else is trying to access the Manero Account <span style='color: #0041cd;'>{email}</span>. This email can not receive replies. For more information, visit the Manero Help Center.</p>
                                    </div>
                                </div>
                                <div style='color: #191919; text-align: center; font-size: 11px;'>
                                    <p>© Manero, Bandovägen 1, SE-123 45 Stockholm, Sweden</p>
                                </div>
                            </div>
                        </body>
                    </html>
                    ",
                    PlainText = $"Please verify your account using this verification code: {code}. If you did not request this code, it is possible that someone else is trying to access the Silicon Account {email}. This email can't recieve replies. For more information, visit the Manero Help in Bandovägen 1, SE-123 45 Stockholm, Sweden."
                };
                return request;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateEmailRequestEmail :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateServiceBusMessage(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
                return payload;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateServiceBusMessage :: {ex.Message}");
        }
        return null!;
    }

    public async Task SendMessageAsync(string email)
    {
        var code = GeneratedCode();
        if (await SaveVerificationRequest(email, code))
        {
            var emailRequest = GenerateEmailRequestEmail(email, code);
            if (emailRequest != null)
            {
                var payload = GenerateServiceBusMessage(emailRequest);
                if (!string.IsNullOrEmpty(payload))
                {
                    var sender = _serviceBusClient.CreateSender("email_provider");
                    await sender.SendMessageAsync(new ServiceBusMessage(payload)
                    {
                        ContentType = "application/json"
                    });
                }
            }
        }
    }
}
