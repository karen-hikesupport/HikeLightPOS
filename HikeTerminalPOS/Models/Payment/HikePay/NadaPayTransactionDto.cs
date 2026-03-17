using System;
namespace HikePOS.Models.Payment
{
    public class NadaPayTransactionDto
    {
        public string PoiTransactionId { get; set; }
        public DateTime? PoiTransactionTimeStamp { get; set; }
        public string SaleTransactionId { get; set; }
        public DateTime? SaleTransactionTimeStamp { get; set; }
        public string PspReference { get; set; }
        public decimal Amount { get; set; }
        public string MaskedPan { get; set; }
        public decimal SurchargeAmount { get; set; }
        public string PaymentMethod { get; set; }

    }

}

