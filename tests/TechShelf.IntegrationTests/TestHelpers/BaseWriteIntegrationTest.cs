namespace TechShelf.IntegrationTests.TestHelpers;

public abstract class BaseWriteIntegrationTest : IAsyncLifetime
{
    protected TestWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        Factory = new TestWebApplicationFactory();
        await Factory.InitializeAsync();
        Client = Factory.CreateClient();
    }

    public virtual async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        Client.Dispose();
    }
}
