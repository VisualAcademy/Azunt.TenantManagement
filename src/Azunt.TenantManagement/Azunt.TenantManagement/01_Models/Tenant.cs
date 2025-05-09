using System;

namespace Azunt.TenantManagement
{
    /// <summary>
    /// 테넌트 정보를 나타내는 모델 클래스입니다.
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// 테넌트 고유 ID (Primary Key)
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 테넌트 이름
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 데이터베이스 연결 문자열
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 인증 헤더
        /// </summary>
        public string AuthenticationHeader { get; set; } = string.Empty;

        /// <summary>
        /// 계정 ID
        /// </summary>
        public string AccountID { get; set; } = string.Empty;

        /// <summary>
        /// 글로벌 검색(Global Search) DB 연결 문자열
        /// </summary>
        public string GSConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 뱃지 사진 유형
        /// </summary>
        public string BadgePhotoType { get; set; } = string.Empty;

        /// <summary>
        /// 리포트 작성기(Report Writer) URL
        /// </summary>
        public string ReportWriterURL { get; set; } = string.Empty;

        /// <summary>
        /// 포털 이름 (기본값: VisualAcademy)
        /// </summary>
        public string PortalName { get; set; } = "VisualAcademy";

        /// <summary>
        /// 스크리닝 파트너 이름 (기본값: VisualAcademy)
        /// </summary>
        public string ScreeningPartnerName { get; set; } = "VisualAcademy";

        /// <summary>
        /// VisualAcademy와 Kodee 포털을 함께 사용할지 여부
        /// </summary>
        public bool IsMultiPortalEnabled { get; set; } = false;

        /// <summary>
        /// 직원(Employee Licensing) URL
        /// </summary>
        public string EmployeeURL { get; set; } = string.Empty;

        /// <summary>
        /// 벤더(Vendor Licensing) URL
        /// </summary>
        public string VendorURL { get; set; } = string.Empty;

        /// <summary>
        /// 내부 감사(Internal Audit) URL
        /// </summary>
        public string InternalAuditURL { get; set; } = string.Empty;

        /// <summary>
        /// 새로운 포털(New Portal)만 사용할지 여부
        /// </summary>
        public bool IsNewPortalOnly { get; set; } = false;
    }
}
