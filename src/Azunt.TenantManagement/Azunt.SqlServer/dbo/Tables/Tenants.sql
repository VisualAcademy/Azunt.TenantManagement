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
);

--CREATE TABLE dbo.TenantSettings (
--    TenantID        bigint       NOT NULL,
--    SettingKey      nvarchar(100) NOT NULL,   -- 예: 'EmployeeSummary:ShowHrUpload'
--    Value           nvarchar(max)     NULL,   -- 문자열 저장(불리언/숫자/Json 문자열 등)
--    ValueType       varchar(20)       NULL,   -- 'bool','int','string','json' 등(선택)
--    Environment     varchar(20)       NULL,   -- 'Prod','Staging','Dev' (선택)
--    UpdatedAt       datetime2(3)      NOT NULL CONSTRAINT DF_TenantSettings_UpdatedAt DEFAULT (sysutcdatetime()),
--    UpdatedBy       nvarchar(100)     NULL,   -- 변경자(선택)
--    CONSTRAINT PK_TenantSettings PRIMARY KEY CLUSTERED (TenantID, SettingKey),
--    CONSTRAINT FK_TenantSettings_Tenants FOREIGN KEY (TenantID) REFERENCES dbo.Tenants(ID)
--);
--GO

---- (선택) 자주 조회하는 키 인덱싱
--CREATE NONCLUSTERED INDEX IX_TenantSettings_Key
--ON dbo.TenantSettings (SettingKey, TenantID);
