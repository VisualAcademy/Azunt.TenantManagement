using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Azunt.TenantManagement
{
    /// <summary>
    /// Provides methods to ensure the Tenants table exists in both master and tenant databases.
    /// This includes creating the table if it does not exist and adding any missing columns 
    /// to keep the schema consistent across databases.
    /// </summary>
    public class TenantsTableBuilder
    {
        private readonly string _masterConnectionString;
        private readonly ILogger<TenantsTableBuilder> _logger;

        public TenantsTableBuilder(string masterConnectionString, ILogger<TenantsTableBuilder> logger)
        {
            _masterConnectionString = masterConnectionString;
            _logger = logger;
        }

        public void BuildTenantDatabases()
        {
            var tenantConnectionStrings = GetTenantConnectionStrings();

            foreach (var connStr in tenantConnectionStrings)
            {
                var connTag = SafeConnDescriptor(connStr);
                try
                {
                    EnsureTenantsTable(connStr);
                    _logger.LogInformation("Tenants table processed (tenant DB): {Conn}", connTag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Conn} Error processing tenant DB", connTag);
                }
            }
        }

        public void BuildMasterDatabase()
        {
            var connTag = SafeConnDescriptor(_masterConnectionString);
            try
            {
                EnsureTenantsTable(_masterConnectionString);
                _logger.LogInformation("Tenants table processed (master DB): {Conn}", connTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Conn} Error processing master DB", connTag);
            }
        }

        private List<string> GetTenantConnectionStrings()
        {
            var result = new List<string>();

            using var connection = new SqlConnection(_masterConnectionString);
            connection.Open();

            using var cmd = new SqlCommand(
                "SELECT ConnectionString FROM [dbo].[Tenants]",
                connection
            );

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var connectionString = reader["ConnectionString"]?.ToString();
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    result.Add(connectionString);
                }
            }

            return result;
        }

        private void EnsureTenantsTable(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            // 1) Table existence (schema-qualified)
            using var cmdCheck = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Tenants'", connection);

            var tableCount = (int)cmdCheck.ExecuteScalar();

            // 2) Create if missing
            if (tableCount == 0)
            {
                var createSql = @"
CREATE TABLE [dbo].[Tenants](
    [ID] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ConnectionString]        NVARCHAR(MAX) NULL,
    [Name]                    NVARCHAR(MAX) NULL,
    [AuthenticationHeader]    NVARCHAR(MAX) NULL,
    [AccountID]               NVARCHAR(MAX) NULL,
    [GSConnectionString]      NVARCHAR(MAX) NULL,
    [ReportWriterURL]         NVARCHAR(MAX) NULL,
    [EmployeeURL]             NVARCHAR(MAX) NULL,
    [VendorURL]               NVARCHAR(MAX) NULL,
    [InternalAuditURL]        NVARCHAR(MAX) NULL,
    [BadgePhotoType]          NVARCHAR(50)  NULL,
    [PortalName]              NVARCHAR(MAX) NULL CONSTRAINT DF_Tenants_PortalName DEFAULT(N'Azunt'),
    [ScreeningPartnerName]    NVARCHAR(MAX) NULL CONSTRAINT DF_Tenants_SPN DEFAULT(N'Azunt'),
    [IsMultiPortalEnabled]    BIT NOT NULL  CONSTRAINT DF_Tenants_IsMultiPortalEnabled DEFAULT(0),
    [IsNewPortalOnly]         BIT NOT NULL  CONSTRAINT DF_Tenants_IsNewPortalOnly DEFAULT(0)
);";
                ExecuteNonQuery(connection, createSql);
                _logger.LogInformation("Tenants table created.");
            }

            // 3) Back-fill missing columns
            var expectedColumns = new Dictionary<string, string>
            {
                ["ConnectionString"] = "NVARCHAR(MAX) NULL",
                ["Name"] = "NVARCHAR(MAX) NULL",
                ["AuthenticationHeader"] = "NVARCHAR(MAX) NULL",
                ["AccountID"] = "NVARCHAR(MAX) NULL",
                ["GSConnectionString"] = "NVARCHAR(MAX) NULL",
                ["ReportWriterURL"] = "NVARCHAR(MAX) NULL",
                ["EmployeeURL"] = "NVARCHAR(MAX) NULL",
                ["VendorURL"] = "NVARCHAR(MAX) NULL",
                ["InternalAuditURL"] = "NVARCHAR(MAX) NULL",
                ["BadgePhotoType"] = "NVARCHAR(50) NULL",

                ["PortalName"] = "NVARCHAR(MAX) NULL CONSTRAINT DF_Tenants_PortalName DEFAULT(N'Azunt')",
                ["ScreeningPartnerName"] = "NVARCHAR(MAX) NULL CONSTRAINT DF_Tenants_SPN DEFAULT(N'Azunt')",

                ["IsMultiPortalEnabled"] = "BIT NOT NULL CONSTRAINT DF_Tenants_IsMultiPortalEnabled DEFAULT(0) WITH VALUES",
                ["IsNewPortalOnly"] = "BIT NOT NULL CONSTRAINT DF_Tenants_IsNewPortalOnly DEFAULT(0) WITH VALUES",
            };

            foreach (var (columnName, definition) in expectedColumns)
            {
                using var cmdColumnCheck = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Tenants' AND COLUMN_NAME = @ColumnName", connection);
                cmdColumnCheck.Parameters.AddWithValue("@ColumnName", columnName);

                var colExists = (int)cmdColumnCheck.ExecuteScalar();
                if (colExists == 0)
                {
                    var sqlToAdd = $"ALTER TABLE [dbo].[Tenants] ADD [{columnName}] {definition}";
                    if (definition.Contains("NOT NULL", StringComparison.OrdinalIgnoreCase) &&
                        definition.Contains("DEFAULT", StringComparison.OrdinalIgnoreCase) &&
                        !definition.Contains("WITH VALUES", StringComparison.OrdinalIgnoreCase))
                    {
                        sqlToAdd += " WITH VALUES";
                    }

                    ExecuteNonQuery(connection, sqlToAdd);
                    _logger.LogInformation("Column added: {Column} ({Def})", columnName, definition);
                }
            }
        }

        private static void ExecuteNonQuery(SqlConnection connection, string sql)
        {
            using var cmd = new SqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns a safe descriptor string for a connection without exposing sensitive details.
        /// Shows server and database names if possible, otherwise a short hash identifier.
        /// </summary>
        private static string SafeConnDescriptor(string connStr)
        {
            try
            {
                var b = new SqlConnectionStringBuilder(connStr);

                b.Remove("Password");
                b.Remove("Pwd");
                b.Remove("User ID");
                b.Remove("UID");
                b.Remove("UserID");

                var server = b.DataSource;
                var db = b.InitialCatalog;

                if (!string.IsNullOrWhiteSpace(server) || !string.IsNullOrWhiteSpace(db))
                {
                    return $"Server={server ?? "(unknown)"}; Database={db ?? "(unknown)"}";
                }
            }
            catch
            {
                // ignore and fall back to hash
            }

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(connStr ?? string.Empty));
            var hash6 = Convert.ToHexString(bytes).Substring(0, 6);
            return $"Conn#{hash6}";
        }

        public static void Run(IServiceProvider services, bool forMaster)
        {
            try
            {
                var logger = services.GetRequiredService<ILogger<TenantsTableBuilder>>();
                var config = services.GetRequiredService<IConfiguration>();
                var masterConnectionString = config.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(masterConnectionString))
                    throw new InvalidOperationException("DefaultConnection is not configured in appsettings.json.");

                var builder = new TenantsTableBuilder(masterConnectionString, logger);

                if (forMaster)
                    builder.BuildMasterDatabase();
                else
                    builder.BuildTenantDatabases();
            }
            catch (Exception ex)
            {
                var fallbackLogger = services.GetService<ILogger<TenantsTableBuilder>>();
                fallbackLogger?.LogError(ex, "Error while processing Tenants table.");
            }
        }
    }
}
