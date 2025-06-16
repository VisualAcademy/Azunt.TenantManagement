using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;

namespace Azunt.TenantManagement;

/// <summary>
/// Tenants 테이블에 기본 데이터를 삽입하는 시더 클래스입니다.
/// </summary>
public static class TenantSeeder
{
    /// <summary>
    /// 기본 테넌트 데이터를 삽입합니다.
    /// 이미 동일한 이름의 테넌트가 존재하면 삽입하지 않습니다.
    /// </summary>
    /// <param name="connectionString">마스터 데이터베이스 연결 문자열</param>
    /// <param name="logger">로깅 인스턴스</param>
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
            logger.LogInformation("기본 테넌트 'Azunt'가 이미 존재합니다.");
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
        logger.LogInformation($"기본 테넌트 'Azunt'가 {inserted}건 삽입되었습니다.");
    }
}
