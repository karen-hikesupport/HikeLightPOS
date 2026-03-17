using System;
namespace HikePOS.Models.Payment
{
    public class PaypalConfigurationDto
    {
        public string AccessToken { get; set; } 
        public string RefreshUrl { get; set; }
    }
}
