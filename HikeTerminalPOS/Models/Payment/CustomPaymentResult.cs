using System;
namespace HikePOS.Models
{
    public class CustomPaymentResult
    {
        public string Action { get; set; }
        public string Transaction_id { get; set; }
        public string Terminal_transaction_id { get; set; }
        public string Merchant_id { get; set; }
        public string Terminal_serial_number { get; set; }
        public string Terminal_model_no { get; set; }
        public string Entry_method { get; set; }
        public string Card_type { get; set; }
        public string Payment_method { get; set; }
        public string Card_last_4 { get; set; }
        public string Signature_url { get; set; }
        public string Receipt_text { get; set; }
    }
}
