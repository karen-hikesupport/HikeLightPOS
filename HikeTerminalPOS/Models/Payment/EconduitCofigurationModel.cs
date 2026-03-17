using System;
namespace HikePOS.Models.Payment
{
    public class EconduitCofigurationDto
    {
        public string ReferenceID { get; set; }
        public string TermicalSerialNumber { get; set; }
        public string APIKey { get; set; }
        public string ApiPassword { get; set; }
        public string TerminalID { get; set; }
        public bool IsPaired { get; set; } = false;

    }


    public class TDConfigurationDTO
    {
        public string RegisterID { get; set; }
        public string MerchantId { get; set; }

    }


}
