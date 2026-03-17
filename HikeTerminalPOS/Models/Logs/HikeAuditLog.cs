using System;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models.Log
{
    public partial class HikeAuditLog : IRealmObject
    {

        public string Id { get; set; }
        public long? ImpersonatorUserId { get; set; }
        public string Exception { get; set; }
        public string BrowserInfo { get; set; }
        public string ClientName { get; set; }
        public string ClientIpAddress { get; set; }
        public int ExecutionDuration { get; set; }
        public long? UserId { get; set; }
        public string MethodName { get; set; }
        public string ServiceName { get; set; }
        public int? ImpersonatorTenantId { get; set; }
        public int? TenantId { get; set; }
        public string Parameters { get; set; }
        public string CustomData { get; set; }
        public string Title { get; set; }
        public DateTimeOffset ExecutionTime { get; set; }
        public string InvoiceTempId { get; set; }
        public static HikeAuditLog CreateFromAuditInfo(AuditInfo auditInfo)
        {
            var exceptionMessage = auditInfo.Exception != null ? auditInfo.Exception.ToString() : null;
            return new HikeAuditLog
            {
                Id = auditInfo.Id,
                TenantId = auditInfo.TenantId,
                UserId = auditInfo.UserId,
                ServiceName = auditInfo.ServiceName,//.TruncateWithPostfix(MaxServiceNameLength),
                MethodName = auditInfo.MethodName,//.TruncateWithPostfix(MaxMethodNameLength),
                Parameters = auditInfo.Parameters,
                ExecutionTime = auditInfo.ExecutionTime,
                ExecutionDuration = auditInfo.ExecutionDuration,
                ClientIpAddress = auditInfo.ClientIpAddress,//.TruncateWithPostfix(MaxClientIpAddressLength),
                ClientName = auditInfo.ClientName,//.TruncateWithPostfix(MaxClientNameLength),
                BrowserInfo = auditInfo.BrowserInfo,//.TruncateWithPostfix(MaxBrowserInfoLength),
                Exception = exceptionMessage,//.TruncateWithPostfix(MaxExceptionLength),
                ImpersonatorUserId = auditInfo.ImpersonatorUserId,
                ImpersonatorTenantId = auditInfo.ImpersonatorTenantId,
                CustomData = auditInfo.CustomData,//.TruncateWithPostfix(MaxCustomDataLength)
                Title = auditInfo.Title
            };
        }
    }

}

    