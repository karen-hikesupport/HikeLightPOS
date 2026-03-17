using System;
namespace HikePOS.Models.Payment
{
    public class AfterpayConfiguration
    {
        public string Token { get; set; }
        public Int64 ExpiresIn { get; set; }
        public Int64 DeviceId { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Secret { get; set; }
        public string MerchantID { get; set; }
        public DateTime TokenExpireOn { get; set; }
        public bool IsPaired { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorId { get; set; }
        public string Message { get; set; }
        public bool HttpStatusCode { get; set; }


        //public string Token { get; set; }
        //public Int64 ExpiresIn { get; set; }
        //public Int64 DeviceId { get; set; }
        //public string Key { get; set; }
        //public string Name { get; set; }
        //public string Secret { get; set; }
        //public string MerchantID { get; set; }
        //public DateTime TokenExpireOn { get; set; }
        //public bool IsPaired { get; set; }

    }
}
