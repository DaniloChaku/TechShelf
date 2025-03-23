using Testcontainers.PostgreSql;

namespace TechShelf.IntegrationTests.Infrastructure;

public class PostgresContainerTestBase : IAsyncLifetime
{
    protected readonly PostgreSqlContainer PostgresContainer;
    protected string ConnectionString => PostgresContainer.GetConnectionString();

    protected PostgresContainerTestBase()
    {
        PostgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public virtual async Task InitializeAsync()
    {
        await PostgresContainer.StartAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await PostgresContainer.StopAsync();
        await PostgresContainer.DisposeAsync();
    }
}
