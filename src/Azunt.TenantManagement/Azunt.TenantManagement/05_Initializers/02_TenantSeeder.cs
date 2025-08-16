using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;

namespace Azunt.TenantManagement;

/// <summary>
/// Seeder class for inserting default tenant data into the Tenants table.
/// </summary>
public static class TenantSeeder
{
    /// <summary>
    /// Inserts a default tenant record.
    /// If a tenant with the same name already exists, insertion will be skipped.
    /// </summary>
    /// <param name="connectionString">Master database connection string</param>
    /// <param name="logger">Logging instance</param>
    public static void InsertDefaultTenant(string connectionString, ILogger logger)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var checkCmd = new SqlCommand(@"
            SELECT COUNT(*) FROM [dbo].[Tenants]
            WHERE [Name] = @Name", connection);
        checkCmd.Parameters.AddWithValue("@Name", "Azunt");

        int exists = (int)checkCmd.ExecuteScalar();
        if (exists > 0)
        {
            logger.LogInformation("Default tenant 'Azunt' already exists.");
            return;
        }

        var insertCmd = new SqlCommand(@"
            INSERT INTO [dbo].[Tenants] 
                ([ConnectionString], [Name], [AuthenticationHeader], [AccountID], [GSConnectionString], [BadgePhotoType],
                 [ReportWriterURL], [PortalName], [ScreeningPartnerName], [EmployeeURL], [VendorURL], [InternalAuditURL],
                 [IsMultiPortalEnabled], [IsNewPortalOnly])
            VALUES
                (@ConnectionString, @Name, @AuthenticationHeader, @AccountID, @GSConnectionString, @BadgePhotoType,
                 @ReportWriterURL, @PortalName, @ScreeningPartnerName, @EmployeeURL, @VendorURL, @InternalAuditURL,
                 @IsMultiPortalEnabled, @IsNewPortalOnly)", connection);

        insertCmd.Parameters.AddWithValue("@ConnectionString", "Server=(localdb)\\mssqllocaldb;Database=Azunt;Trusted_Connection=True;");
        insertCmd.Parameters.AddWithValue("@Name", "Azunt");
        insertCmd.Parameters.AddWithValue("@AuthenticationHeader", Guid.NewGuid().ToString());
        insertCmd.Parameters.AddWithValue("@AccountID", "7777777");
        insertCmd.Parameters.AddWithValue("@GSConnectionString", "");
        insertCmd.Parameters.AddWithValue("@BadgePhotoType", "");
        insertCmd.Parameters.AddWithValue("@ReportWriterURL", "");
        insertCmd.Parameters.AddWithValue("@PortalName", "VisualAcademy");
        insertCmd.Parameters.AddWithValue("@ScreeningPartnerName", "VisualAcademy");
        insertCmd.Parameters.AddWithValue("@EmployeeURL", "");
        insertCmd.Parameters.AddWithValue("@VendorURL", "");
        insertCmd.Parameters.AddWithValue("@InternalAuditURL", "");
        insertCmd.Parameters.AddWithValue("@IsMultiPortalEnabled", false);
        insertCmd.Parameters.AddWithValue("@IsNewPortalOnly", false);

        int inserted = insertCmd.ExecuteNonQuery();
        logger.LogInformation($"Default tenant 'Azunt' has been inserted ({inserted} row(s) affected).");
    }
}
