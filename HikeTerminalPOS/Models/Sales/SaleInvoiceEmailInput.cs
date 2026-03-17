using System;
using System.Collections.ObjectModel;
using HikePOS.Enums;
using Realms;

namespace HikePOS.Model
{
    public class SaleInvoiceEmailInput
    {
        public int? InvoiceId { get; set; }
        public int RegisterId { get; set; }
        public string InvoiceNumber { get; set; }
        public string SyncReference { get; set; }
        public int? CustomerId { get; set; }
        public string Email { get; set; }
        public string CustomerName { get; set; }
        public EmailTemplateType EmailTemplateType { get; set; }
        public bool EmailForGiftCard { get; set; }
        //Ticket start:#92767 iOS:FR Log when invoices, quotes and reports are sent by email.by rupesh
         public InvoiceFrom InvoiceFrom { get; set; }
        //Ticket end:#92767.
        public bool IsEmailPaymentLinkActive { get; set; }
        public string CountryCode { get; set; }

        public SaleInvoiceEmailInputDB ToModel()
        {
            SaleInvoiceEmailInputDB saleInvoiceEmailInputDB = new SaleInvoiceEmailInputDB
            {
                InvoiceId = InvoiceId,
                RegisterId = RegisterId,
                InvoiceNumber = InvoiceNumber,
                SyncReference = SyncReference,
                CustomerId = CustomerId,
                Email = Email,
                CustomerName = CustomerName,
                EmailTemplateType = (int)EmailTemplateType,
                EmailForGiftCard = EmailForGiftCard,
                InvoiceFrom = (int)InvoiceFrom,
                IsEmailPaymentLinkActive = IsEmailPaymentLinkActive,
                CountryCode = CountryCode

            };
            return saleInvoiceEmailInputDB;
        }
        public static SaleInvoiceEmailInput FromModel(SaleInvoiceEmailInputDB invoiceHistoryDB)
        {
            SaleInvoiceEmailInput invoiceHistoryDto = new SaleInvoiceEmailInput
            {
                InvoiceId = invoiceHistoryDB.InvoiceId,
                RegisterId = invoiceHistoryDB.RegisterId,
                InvoiceNumber = invoiceHistoryDB.InvoiceNumber,
                SyncReference = invoiceHistoryDB.SyncReference,
                CustomerId = invoiceHistoryDB.CustomerId,
                Email = invoiceHistoryDB.Email,
                CustomerName = invoiceHistoryDB.CustomerName,
                EmailTemplateType = (EmailTemplateType)invoiceHistoryDB.EmailTemplateType,
                EmailForGiftCard = invoiceHistoryDB.EmailForGiftCard,
                InvoiceFrom = (InvoiceFrom)invoiceHistoryDB.InvoiceFrom,
                IsEmailPaymentLinkActive = invoiceHistoryDB.IsEmailPaymentLinkActive,
                CountryCode = invoiceHistoryDB.CountryCode

            };
            return invoiceHistoryDto;

        }

    }
    public enum EmailTemplateType
    {
        Loyalty = 1,
        CustomerReceipt = 2,
        PurchaseOrder = 3,
        NewUser = 4,
        ARStatement = 5,
        InventoryTransfer = 6,
        NewCustomer = 7,
        CustomerQuoteReceipt = 8,
        MajorActivityLog = 10,
        EcommerceForgotPassword = 11,
        EcommerceSubscibed = 12,
        RegisterReport = 13, //Start #92768 By Pratik
        CustomerReceiptWithPaymentLink = 14,
        HikePayBulkPayment = 15,
    }
    public partial class SaleInvoiceEmailInputDB : IRealmObject
    {
        public int? InvoiceId { get; set; }
        public int RegisterId { get; set; }
        public string InvoiceNumber { get; set; }
        public string SyncReference { get; set; }
        public int? CustomerId { get; set; }
        public string Email { get; set; }
        public string CustomerName { get; set; }
        public int EmailTemplateType { get; set; }
        public bool EmailForGiftCard { get; set; }
        public int InvoiceFrom { get; set; }
        public bool IsEmailPaymentLinkActive { get; set; }
        public string CountryCode { get; set; }

    }
}