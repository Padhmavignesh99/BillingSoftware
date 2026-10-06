using BillingSoftware.API.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Data
{
    /// <summary>
    /// Configures the correct EF Core database provider based on <see cref="DataSettings.ActiveProvider"/>.
    /// Add a new case here when adding additional providers in the future.
    /// </summary>
    public static class DbContextConfigurator
    {
        /// <summary>
        /// Applies the correct UseMySql / UseSqlServer / UseNpgsql call to <paramref name="options"/>
        /// based on the <paramref name="provider"/> value read from DataSettings.ActiveProvider.
        /// </summary>
        /// <param name="options">DbContextOptionsBuilder to configure.</param>
        /// <param name="provider">
        /// Provider key — must match a key in ConnectionStrings.
        /// Accepted: "MySql" | "MSSql" | "PostgreSQL" | "MySql_Cloud" | "MSSql_Cloud" | "PostgreSQL_Cloud"
        /// </param>
        /// <param name="connectionString">The resolved connection string for the provider.</param>
        public static void Configure(DbContextOptionsBuilder options, string provider, string connectionString)
        {
            switch (provider)
            {
                // ─── MySQL / MariaDB (local or cloud) ─────────────────────────
                case "MySql":
                case "MySql_Cloud":
                    options.UseMySql(
                        connectionString,
                        ServerVersion.AutoDetect(connectionString),
                        mySqlOptions =>
                        {
                            mySqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorNumbersToAdd: null);
                        });
                    break;

                // ─── Microsoft SQL Server (local or Azure SQL) ────────────────
                case "MSSql":
                case "MSSql_Cloud":
                    options.UseSqlServer(
                        connectionString,
                        sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorNumbersToAdd: null);
                        });
                    break;

                // ─── PostgreSQL (local or Azure Database for PostgreSQL) ───────
                case "PostgreSQL":
                case "PostgreSQL_Cloud":
                    options.UseNpgsql(
                        connectionString,
                        npgsqlOptions =>
                        {
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorCodesToAdd: null);
                        });
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown database provider '{provider}'. " +
                        $"Valid values: MySql, MSSql, PostgreSQL, MySql_Cloud, MSSql_Cloud, PostgreSQL_Cloud. " +
                        $"Check DataSettings.ActiveProvider in appsettings.json.");
            }
        }

        /// <summary>
        /// Returns true when the active provider is PostgreSQL (local or cloud).
        /// Useful for provider-specific SQL syntax decisions in OnModelCreating.
        /// </summary>
        public static bool IsPostgreSQL(string provider)
            => provider is "PostgreSQL" or "PostgreSQL_Cloud";

        /// <summary>
        /// Returns true when the active provider is MySQL or MariaDB (local or cloud).
        /// </summary>
        public static bool IsMySQL(string provider)
            => provider is "MySql" or "MySql_Cloud";

        /// <summary>
        /// Returns true when the active provider is Microsoft SQL Server (local or Azure SQL).
        /// </summary>
        public static bool IsMSSql(string provider)
            => provider is "MSSql" or "MSSql_Cloud";
    }
}
