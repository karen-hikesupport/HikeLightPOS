using System;
namespace HikePOS.Models.Log
{
    public class AuditLog //: IMayHaveTenant
    {
        public AuditLog(){
            
        }

        public static int MaxServiceNameLength;
        public static int MaxMethodNameLength;
        public static int MaxParametersLength;
        public static int MaxClientIpAddressLength;
        public static int MaxClientNameLength;
        public static int MaxBrowserInfoLength;
        public static int MaxExceptionLength;
        public static int MaxCustomDataLength;

        public string Id { get; set; }
        public long? ImpersonatorUserId { get; set; }
        public string Exception { get; set; }
        public string BrowserInfo { get; set; }
        public string ClientName { get; set; }
        public string ClientIpAddress { get; set; }
        public int ExecutionDuration { get; set; }
        public DateTime ExecutionTime { get; set; }
        public long? UserId { get; set; }
        public string MethodName { get; set; }
        public string ServiceName { get; set; }
        public int? ImpersonatorTenantId { get; set; }
        public int? TenantId { get; set; }
        public string Parameters { get; set; }
        public string CustomData { get; set; }
        public string Title { get; set; }

        //public static AuditLog CreateFromAuditInfo(AuditInfo auditInfo);
    }
}