using MasterBilling.API.Configuration;
using MasterBilling.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MasterBilling.API.Data
{
    /// <summary>
    /// Allows EF Core tools (dotnet ef migrations add / update) to create
    /// ApplicationDbContext at design time without running the full host.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var dataSettings = config.GetSection("DataSettings").Get<DataSettings>()
                ?? new DataSettings { ActiveProvider = "MSSql" };

            var connectionString = config.GetConnectionString(dataSettings.ActiveProvider)
                ?? throw new InvalidOperationException(
                    $"No connection string for provider '{dataSettings.ActiveProvider}'.");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            DbContextConfigurator.Configure(optionsBuilder, dataSettings.ActiveProvider, connectionString);

            // Provide a dummy IConfiguration for design-time
            return new ApplicationDbContext(optionsBuilder.Options, config);
        }
    }
}
