namespace TechShelf.Infrastructure.Services.SendGrid;

public class SendGridOptions
{
    public const string SectionName = "SendGrid";

    public string FromEmail { get; set; } = string.Empty;
    public bool IsSandboxEnabled { get; set; }
}
