using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace HikePOS.Models
{

    //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.


    public class ShopFeature : FullAuditedPassiveEntityDto
    {
            [JsonProperty("hikePOS.Allow.Change.CloudAddress.Feature")]
            public string HikePOSAllowChangeCloudAddressFeature { get; set; }

            [JsonProperty("hikePOS.AllowBasicFeature")]
            public string HikePOSAllowBasicFeature { get; set; }

            [JsonProperty("hikePOS.AllowMultipleOutetFeature")]
            public string HikePOSAllowMultipleOutetFeature { get; set; }

            [JsonProperty("hikePOS.Free.RegisterFeature")]
            public string HikePOSFreeRegisterFeature { get; set; }

            [JsonProperty("hikePOS.UserAllowedFeature")]
            public string HikePOSUserAllowedFeature { get; set; }

            [JsonProperty("hikePOS.ProductAllowdFeature")]
            public string HikePOSProductAllowdFeature { get; set; }

            [JsonProperty("hikePOS.EnableSerialNumberTracking.Feature")]
            public string HikePOSEnableSerialNumberTrackingFeature { get; set; }

            [JsonProperty("hikePOS.ShowTotalNumberOfItemsOnReceipt.Feature")]
            public string HikeShowTotalNumberOfItemsOnReceiptFeature { get; set; }

            [JsonProperty("hikePOS.ShowSKU.Feature")]
            public string HikeShowSKUFeature { get; set; }

            [JsonProperty("hikePOS.ReportFeature")]
            public string HikePOSReportFeature { get; set; }

            [JsonProperty("hikePOS.GiftCardFeature")]
            public string HikeGiftCardFeature { get; set; }

            [JsonProperty("hikePOS.LoyaltyFeature")]
            public string HikeLoyaltyFeature { get; set; }

            [JsonProperty("hikePOS.AdvanceReportingFeature")]
            public string HikePOSAdvanceReportingFeature { get; set; }

            [JsonProperty("hikePOS.UserPermissionFeature")]
            public string HikePOSUserPermissionFeature { get; set; }

            [JsonProperty("hikePOS.StockTransferFeature")]
            public string HikePOSStockTransferFeature { get; set; }

            [JsonProperty("hikePOS.ECommerceFeature")]
            public string HikePOSECommerceFeature { get; set; }

            [JsonProperty("hikePOS.CustomerGroupPricebook.Feature")]
            public string HikePOSCustomerGroupPricebookFeature { get; set; }

            [JsonProperty("hikePOS.DymoLabelPrint.Feature")]
            public string HikePOSDymoLabelPrintFeature { get; set; }

            [JsonProperty("hikePOS.SkipAveryLableCount.Feature")]
            public string HikePOSSkipAveryLableCountFeature { get; set; }

            [JsonProperty("hikePOS.AllowImportPrintList.Feature")]
            public string HikePOSAllowImportPrintListFeature { get; set; }

            [JsonProperty("hikePOS.AmazonMarketPlace.Feature")]
            public string HikePOSAmazonMarketPlaceFeature { get; set; }

            [JsonProperty("hikePOS.CustomReport.Feature")]
            public string HikePOSCustomReportFeature { get; set; }

            [JsonProperty("hikePOS.CustomFields.Feature")]
            public string HikeCustomFieldsFeature { get; set; }

            [JsonProperty("hikePOS.CustomerSecondaryEmail.Feature")]
            public string HikeCustomerSecondaryEmailFeature { get; set; }

            [JsonProperty("hikePOS.ClockInClockOut.Feature")]
            public string HikeClockInClockOutFeature { get; set; }

            [JsonProperty("hikePOS.InvoiceExchange.Feature")]
            public string HikeInvoiceExchangeFeature { get; set; }

            [JsonProperty("hikePOS.OnAccount.Feature")]
            public string HikeOnAccountFeature { get; set; }

            [JsonProperty("hikePOS.CreditNote.Feature")]
            public string HikeCreditNotFeature { get; set; }

            //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik
            [JsonProperty("hikePOS.CreditNotePrint.Feature")]
            public string HikeCreditNotePrintFeature { get; set; }
            //End Ticket #74631 by pratik

            [JsonProperty("hikePOS.UserLanguageSelection.Feature")]
            public string HikePOSUserLanguageSelectionFeature { get; set; }

            [JsonProperty("hikePOS.ChatFeature")]
            public string HikePOSChatFeature { get; set; }

            [JsonProperty("hikePOS.Allow.InvoiceNote")]
            public string HikePOSAllowInvoiceNote { get; set; }

            [JsonProperty("hikePOS.Allow.CustomPayment")]
            public string HikePOSAllowCustomPayment { get; set; }

            [JsonProperty("hikePOS.Allow.PO")]
            public string HikePOSAllowPO { get; set; }

            [JsonProperty("hikePOS.Allow.StockTake")]
            public string HikePOSAllowStockTake { get; set; }

            [JsonProperty("hikePOS.Allow.Supplier")]
            public string HikePOSAllowSupplier { get; set; }

            [JsonProperty("hikePOS.Allow.Refund")]
            public string HikePOSAllowRefund { get; set; }

            [JsonProperty("hikePOS.Allow.RegisterClosure")]
            public string HikePOSAllowRegisterClosure { get; set; }

            [JsonProperty("hikePOS.Allow.Customer")]
            public string HikePOSAllowCustomer { get; set; }

            [JsonProperty("hikePOS.AddOnUnLeased.Feature")]
            public string HikePOSAddOnUnLeasedFeature { get; set; }

            [JsonProperty("hikePOS.PurchaseStockReturn.Feature")]
            public string HikePOSPurchaseStockReturnFeature { get; set; }

            [JsonProperty("hikePOS.ManageRole.Feature")]
            public string HikePOSManageRoleFeature { get; set; }

            [JsonProperty("hikePOS.MajorActivityReport.Feature")]
            public string HikePOSMajorActivityReportFeature { get; set; }

            [JsonProperty("hikePOS.UnitOfMeasure.Feature")]
            public string HikePOSUnitOfMeasureFeature { get; set; }

            [JsonProperty("hikePOS.CustomerDeliverAddress.Feature")]
            public string HikeCustomerDeliverAddressFeature { get; set; }

            [JsonProperty("hikePOS.QuoteSale.Feature")]
            public string HikeQuoteSaleFeature { get; set; }

            [JsonProperty("hikePOS.RegisterClosure.TransactionBySku.Report.Feature")]
            public string HikePOSRegisterClosureTransactionBySkuReportFeature { get; set; }

            [JsonProperty("hikePOS.StaffTargetReport.Feature")]
            public string HikePOSStaffTargetReportFeature { get; set; }

            [JsonProperty("hikePOS.CustomerMultiOutlet.Feature")]
            public string HikePOSCustomerMultiOutletFeature { get; set; }

            [JsonProperty("hikePOS.ReceiptAdjustFontSize.Feature")]
            public string HikePOSReceiptAdjustFontSizeFeature { get; set; }

            [JsonProperty("hikePOS.AdvanceFilterFeature")]
            public string HikePOSAdvanceFilterFeature { get; set; }

            [JsonProperty("hikePOS.ChatFeature.TenantToTenant")]
            public string HikePOSChatFeatureTenantToTenant { get; set; }

            [JsonProperty("hikePOS.ChatFeature.TenantToHost")]
            public string HikePOSChatFeatureTenantToHost { get; set; }

           [JsonProperty("hikePOS.SortingProductsOnPOS.Feature")]
            public string HikePOSSortingProductsOnPOSFeature { get; set; }

            [JsonProperty("hikePOS.EditWalkInCustomerName.Feature")]
            public string HikePOSEditWalkInCustomerName { get; set; }

           [JsonProperty("hikePOS.DuplicateSale.Feature")]
           public string HikePOSDuplicateSaleFeature { get; set; }


    }
}
