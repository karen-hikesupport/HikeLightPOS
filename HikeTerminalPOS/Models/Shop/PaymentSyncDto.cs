using System;
using System.Collections.Generic;

namespace HikePOS.Models.Shop
{
    public class PaymentSyncDto
    {
        public int registerClosureId { get; set; }
        public List<int> paymentOptionIds { get; set; }
    }




    public class RegisterclosuresTally
    {
        public int registerCloserId { get; set; }
        public int paymentOptionId { get; set; }
        public int paymentOptionType { get; set; }
        public string paymentOptionName { get; set; }
        public double actualTotal { get; set; }
        public double registeredTotal { get; set; }
        public List<object> registerClosureTallyDenominations { get; set; }
        public bool isActive { get; set; }
        public int id { get; set; }
    }

    public class RegisterCashInOut
    {
        public int registerClosureId { get; set; }
        public int paymentOptionId { get; set; }
        public int userId { get; set; }
        public int registerCashType { get; set; }
        public string registerCashTypeName { get; set; }
        public string userBy { get; set; }
        public double amount { get; set; }
        public string note { get; set; }
        public int id { get; set; }
    }

    public class TaxList
    {
        public int taxId { get; set; }
        public string taxName { get; set; }
        public double taxRate { get; set; }
        public double taxAmount { get; set; }
        public double taxSaleAmount { get; set; }
        public object subTaxes { get; set; }
    }

    public class Result
    {
        public string refNumber { get; set; }
        public int registerId { get; set; }
        public DateTime startDateTime { get; set; }
        public object endDateTime { get; set; }
        public string registerName { get; set; }
        public string outletRegisterName { get; set; }
        public string outletName { get; set; }
        public int startBy { get; set; }
        public object closeBy { get; set; }
        public string startByUser { get; set; }
        public object closeByUser { get; set; }
        public double totalSales { get; set; }
        public double totalCompletedSales { get; set; }
        public double totalOnAccountSales { get; set; }
        public double totalParkedSales { get; set; }
        public double totalLayBySales { get; set; }
        public double difference { get; set; }
        public double totalDiscounts { get; set; }
        public double totalTax { get; set; }
        public double totalTip { get; set; }
        public double totalPayments { get; set; }
        public double totalRefunds { get; set; }
        public object notes { get; set; }
        public object merchant_receipt { get; set; }
        public object transactionDetail { get; set; }
        public int thirdPartySyncStatus { get; set; }
        public List<RegisterclosuresTally> registerclosuresTallys { get; set; }
        public List<RegisterCashInOut> registerCashInOuts { get; set; }
        public List<TaxList> taxList { get; set; }
        public object registerClosureTallyDenominations { get; set; }
        public object registerClosureTransactionDetailsDto { get; set; }
        public bool isActive { get; set; }
        public int id { get; set; }
    }

    public class IpadPaymentSyncResponse 
    {
        public Result result { get; set; }
        public object targetUrl { get; set; }
        public bool success { get; set; }
        public object error { get; set; }
        public bool unAuthorizedRequest { get; set; }
        public bool __abp { get; set; }
    }
}
