using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using TechShelf.Application.Exceptions;
using TechShelf.Application.Interfaces.Services;

namespace TechShelf.Infrastructure.Services.SendGrid;

public class SendGridService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridService> _logger;

    public SendGridService(ISendGridClient sendGridClient, 
        IOptions<SendGridOptions> options, 
        ILogger<SendGridService> logger)
    {
        _sendGridClient = sendGridClient;
        _options = options.Value;
        _logger = logger;
    }
    public async Task SendPlainTextEmailAsync(
        string toEmail,
        string subject,
        string message,
        CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _logger.LogInformation("Sending email to {ToEmail} with subject {Subject}", toEmail, subject);
        var from = new EmailAddress(_options.FromEmail, "TechShelf");
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, message, null);

        if (_options.IsSandboxEnabled)
        {
            msg.SetSandBoxMode(true);
        }

        var response = await _sendGridClient.SendEmailAsync(msg, token);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Body.ReadAsStringAsync(token);
            _logger.LogError("Failed to send email. Status: {StatusCode}, Response: {Response}",
                response.StatusCode, responseBody);
            var errorMessage = $"Failed to send email to {toEmail}. Status Code: {response.StatusCode}";
            throw new EmailSendingException(errorMessage);
        }

        _logger.LogInformation("Successfully sent email to {ToEmail}", toEmail);
    }
}
