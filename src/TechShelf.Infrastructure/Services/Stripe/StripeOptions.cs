namespace TechShelf.Infrastructure.Services.Stripe;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    public string WhSecret { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string ErrorUrl { get; set; } = string.Empty;
}
