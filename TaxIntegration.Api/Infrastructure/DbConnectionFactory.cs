using Npgsql;

namespace TaxIntegration.Api.Infrastructure;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured");
    }
    
    public NpgsqlConnection Create() => new(_connectionString);
}