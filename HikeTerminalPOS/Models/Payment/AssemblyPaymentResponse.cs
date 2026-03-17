using System;
namespace HikePOS.Models.Payment
{
    public class AssemblyPaymentResponse
    {
        public bool Success { get; set; }
        public string RequestId { get; set; }
        public string PosRefId { get; set; }
        public string SchemeName { get; set; }
        public string SchemeAppName { get; set; }
        public string RRN { get; set; }
        public int PurchaseAmount { get; set; }
        public int TipAmount { get; set; }
        public int CashoutAmount { get; set; }
        public int BankNonCashAmount { get; set; }
        public int BankCashAmount { get; set; }
        public string CustomerReceipt { get; set; }
        public string MerchantReceipt { get; set; }
        public string ResponseText { get; set; }
        public string ResponseCode { get; set; }
        public string TerminalReferenceId { get; set; }
        public string CardEntry { get; set; }
        public string AccountType { get; set; }
        public string AuthCode { get; set; }
        public string BankDate { get; set; }
        public string BankTime { get; set; }
        public string MaskedPan { get; set; }
        public string TerminalId { get; set; }
        public bool MerchantReceiptPrinted { get; set; }
        public bool CustomerReceiptPrinted { get; set; }
        public DateTime? SettlementDate { get; set; }
        public bool IsAllowPrintOnEFTPOS { get; set; }
        public bool IsRequiredMerchantCopyToPrint { get; set; }
    }
}
