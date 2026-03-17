using System;
namespace HikePOS.Models.Payment
{
    public class PaymentConfigurationModel
    {
        public string AccessToken { get; set; }
        public string RefreshUrl { get; set; }
        public Enums.PaymentOptionType Type { get; set; }
        public string MerchantId { get; set; }
        public string TerminalId { get; set; }
        
    }

}
