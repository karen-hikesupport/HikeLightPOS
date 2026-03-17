using System;

namespace HikePOS.Models.Payment
{
    public class AssemblyPaymentConfigurationDto : BaseModel
    {
        public string PosId { get; set; }

        //#23024 iOS - Westpac Acquirer
        public string AcquirerCode { get; set; }
        public string AcquirerName { get; set; }
        //#23024 iOS - Westpac Acquirer

        public string EftposAddress { get; set; }
        public string EncKey { get; set; }
        public string HmacKey { get; set; }
        //Ticket #9484 Start: Westpac payment issue. By Nikhil.
        public bool ReceiptFromEFTPOS { get; set; } = false;

        //Ticket #9484 End:By Nikhil.

        public bool IsTestMode { get; set; } = false;  //#35436 iOS- mx51 Suggested changes







    }
}
