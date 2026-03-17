using System;
namespace HikePOS.Models.Sales
{
    public class SerialNumberDto
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public int InvoiceId { get; set; }
        public bool EnableSerialNumber { get; set; }
    }
}
