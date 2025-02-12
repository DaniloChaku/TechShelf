namespace TechShelf.Infrastructure.Data.Outbox;

public class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int OutboxProcessorFrequencyMilliseconds { get; set; }
}
