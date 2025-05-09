using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.TenantManagement;

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
            try
            {
                EnsureTenantsTable(connStr);
                _logger.LogInformation($"Tenants table processed (tenant DB): {connStr}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{connStr}] Error processing tenant DB");
            }
        }
    }

    public void BuildMasterDatabase()
    {
        try
        {
            EnsureTenantsTable(_masterConnectionString);
            _logger.LogInformation("Tenants table processed (master DB)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing master DB");
        }
    }

    private List<string> GetTenantConnectionStrings()
    {
        var result = new List<string>();

        using (var connection = new SqlConnection(_masterConnectionString))
        {
            connection.Open();
            var cmd = new SqlCommand("SELECT ConnectionString FROM dbo.Tenants", connection);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var connectionString = reader["ConnectionString"]?.ToString();
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        result.Add(connectionString);
                    }
                }
            }
        }

        return result;
    }

    private void EnsureTenantsTable(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            var cmdCheck = new SqlCommand(@"
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'Tenants'", connection);

            int tableCount = (int)cmdCheck.ExecuteScalar();

            if (tableCount == 0)
            {
                var cmdCreate = new SqlCommand(@"
                CREATE TABLE [dbo].[Tenants] (
                    [ID] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [ConnectionString] NVARCHAR(MAX) NULL,
                    [Name] NVARCHAR(MAX) NULL,
                    [AuthenticationHeader] NVARCHAR(MAX) NULL,
                    [AccountID] NVARCHAR(MAX) NULL,
                    [GSConnectionString] NVARCHAR(MAX) NULL,
                    [ReportWriterURL] NVARCHAR(MAX) NULL,
                    [EmployeeURL] NVARCHAR(MAX) NULL,
                    [VendorURL] NVARCHAR(MAX) NULL,
                    [InternalAuditURL] NVARCHAR(MAX) NULL,
                    [BadgePhotoType] NVARCHAR(50) NULL,
                    [PortalName] NVARCHAR(MAX) NULL DEFAULT ('AssureHire'),
                    [ScreeningPartnerName] NVARCHAR(MAX) NULL DEFAULT ('AssureHire'),
                    [IsMultiPortalEnabled] BIT NULL DEFAULT 0,
                    [IsNewPortalOnly] BIT NULL DEFAULT 0
                )", connection);

                cmdCreate.ExecuteNonQuery();

                _logger.LogInformation("Tenants table created.");
            }
            else
            {
                var expectedColumns = new Dictionary<string, string>
                {
                    ["ConnectionString"] = "NVARCHAR(MAX)",
                    ["Name"] = "NVARCHAR(MAX)",
                    ["AuthenticationHeader"] = "NVARCHAR(MAX)",
                    ["AccountID"] = "NVARCHAR(MAX)",
                    ["GSConnectionString"] = "NVARCHAR(MAX)",
                    ["ReportWriterURL"] = "NVARCHAR(MAX)",
                    ["EmployeeURL"] = "NVARCHAR(MAX)",
                    ["VendorURL"] = "NVARCHAR(MAX)",
                    ["InternalAuditURL"] = "NVARCHAR(MAX)",
                    ["BadgePhotoType"] = "NVARCHAR(50)",
                    ["PortalName"] = "NVARCHAR(MAX)",
                    ["ScreeningPartnerName"] = "NVARCHAR(MAX)",
                    ["IsMultiPortalEnabled"] = "BIT",
                    ["IsNewPortalOnly"] = "BIT"
                };

                foreach (var kvp in expectedColumns)
                {
                    var columnName = kvp.Key;

                    var cmdColumnCheck = new SqlCommand(@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Tenants' AND COLUMN_NAME = @ColumnName", connection);
                    cmdColumnCheck.Parameters.AddWithValue("@ColumnName", columnName);

                    int colExists = (int)cmdColumnCheck.ExecuteScalar();

                    if (colExists == 0)
                    {
                        var alterCmd = new SqlCommand(
                            $"ALTER TABLE [dbo].[Tenants] ADD [{columnName}] {kvp.Value} NULL", connection);
                        alterCmd.ExecuteNonQuery();

                        _logger.LogInformation($"Column added: {columnName} ({kvp.Value})");
                    }
                }
            }

            // === 기본 데이터 삽입 추가 ===
            var cmdCountRows = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Tenants]", connection);
            int rowCount = (int)cmdCountRows.ExecuteScalar();

            if (rowCount == 0)
            {
                var cmdInsertDefaults = new SqlCommand(@"
                INSERT INTO [dbo].[Tenants] (ConnectionString, Name, AuthenticationHeader)
                VALUES
                    ('Server=.;Database=Tenant1Db;Trusted_Connection=True;', 'Tenant1', 'AuthHeader1'),
                    ('Server=.;Database=Tenant2Db;Trusted_Connection=True;', 'Tenant2', 'AuthHeader2')
            ", connection);

                int inserted = cmdInsertDefaults.ExecuteNonQuery();
                _logger.LogInformation($"Tenants 기본 데이터 {inserted}건 삽입 완료");
            }
            // ===
        }
    }

    public static void Run(IServiceProvider services, bool forMaster)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<TenantsTableBuilder>>();
            var config = services.GetRequiredService<IConfiguration>();
            var masterConnectionString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(masterConnectionString))
            {
                throw new InvalidOperationException("DefaultConnection is not configured in appsettings.json.");
            }

            var builder = new TenantsTableBuilder(masterConnectionString, logger);

            if (forMaster)
            {
                builder.BuildMasterDatabase();
            }
            else
            {
                builder.BuildTenantDatabases();
            }
        }
        catch (Exception ex)
        {
            var fallbackLogger = services.GetService<ILogger<TenantsTableBuilder>>();
            fallbackLogger?.LogError(ex, "Error while processing Tenants table.");
        }
    }
}
