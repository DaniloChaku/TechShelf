namespace TechShelf.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendPlainTextEmailAsync(
        string toEmail,
        string subject,
        string message,
        CancellationToken token = default);
}
