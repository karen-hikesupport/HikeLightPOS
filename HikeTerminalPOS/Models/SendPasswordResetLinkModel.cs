using System;
namespace HikePOS.Models
{
    public class SendPasswordResetLinkModel
    {
        public string TenancyName { get; set; }
        public string EmailAddress { get; set; }
    }
}
