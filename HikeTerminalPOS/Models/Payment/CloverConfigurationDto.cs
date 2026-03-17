using System;
namespace HikePOS.Models.Payment
{
    public class CloverConfigurationDto
    {
        public string ApplicationId { get; set; }
        public string POSName { get; set; }
        public string POSSerial { get; set; }
        public string ConnectionURL { get; set; }
        public string AuthToken { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }
    }
    public class CloverPaymentResponse
    {
        public string Amount { get; set; }
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public string Details { get; set; }

    }

}

