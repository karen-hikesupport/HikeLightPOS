using System;
namespace HikePOS.Models.Payment
{
    public class NadaPayConfigurationDto
    {
        public int OutletId { get; set; }
        public string StoreId { get; set; }
        public string BalanceAccountId { get; set; }
        public string StoreReferenceId { get; set; }
        public bool IsPrintCustomerReceipt { get; set; }
        public bool IsPrintMerchantReceipt { get; set; }
        public string SerialNumber { get; set; }
        public string TerminalId { get; set; }

        public int PaymentType { get; set; }
        public string IpAddress { get; set; }
        public DateTime LastActivity { get; set; }
        public List<object> AdditionalPaymentMethods { get; set; }



    }

}

