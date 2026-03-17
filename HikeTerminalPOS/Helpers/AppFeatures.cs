using System;
namespace HikePOS.Helpers
{

    //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
    public static class AppFeatures
    {
        public const string MaxUserCount = "App.MaxUserCount";

        public const string HikeAllowChangeCloudAddressFeature = "HikePOS.Allow.Change.CloudAddress.Feature";
        public const string HikeAllowBasicFeature = "HikePOS.AllowBasicFeature";
        public const string HikeAllowMultipleOutetFeature = "HikePOS.AllowMultipleOutetFeature";
        public const string HikeFreeRegisterFeature = "HikePOS.Free.RegisterFeature";
        public const string HikeProductAllowdFeature = "HikePOS.ProductAllowdFeature";

        public const string HikeEnableSerialNumberTrackingFeature = "HikePOS.EnableSerialNumberTracking.Feature";

        public const string HikeUserAllowedFeature = "HikePOS.UserAllowedFeature";
        public const string HikeReportFeature = "HikePOS.ReportFeature";
        public const string HikeGiftCardFeature = "HikePOS.GiftCardFeature";
        public const string HikeLoyaltyFeature = "HikePOS.LoyaltyFeature";
        public const string HikeAdvanceReportingFeature = "HikePOS.AdvanceReportingFeature";
        public const string HikeUserPermissionFeature = "HikePOS.UserPermissionFeature";
        public const string HikeStockTransferFeature = "HikePOS.StockTransferFeature";
        public const string HikeECommerceFeature = "HikePOS.ECommerceFeature";

        public const string HikeCustomerGroupPricebookFeature = "HikePOS.CustomerGroupPricebook.Feature";
        public const string HikeDymoLabelPrintFeature = "HikePOS.DymoLabelPrint.Feature";
        public const string HikeSkipAveryLableCountFeature = "HikePOS.SkipAveryLableCount.Feature";
        public const string HikeAllowImportPrintListFeature = "HikePOS.AllowImportPrintList.Feature";

        public const string HikeAmazonMarketPlaceFeature = "HikePOS.AmazonMarketPlace.Feature";
        public const string HikeCustomReportFeature = "HikePOS.CustomReport.Feature";

        public const string HikeCustomFieldsFeature = "HikePOS.CustomFields.Feature";

        public const string HikeClockInClockOutFeature = "HikePOS.ClockInClockOut.Feature";
        public const string HikeOnAccountReportFeature = "HikePOS.OnAccountReport.Feature";
        public const string HikeInvoiceExchangeFeature = "HikePOS.InvoiceExchange.Feature";
        public const string HikeOnAccountFeature = "HikePOS.OnAccount.Feature";

        public const string HikeCreditNotFeature = "HikePOS.CreditNote.Feature";

        public const string ChatFeature = "HikePOS.ChatFeature";
        public const string TenantToTenantChatFeature = "HikePOS.ChatFeature.TenantToTenant";
        public const string TenantToHostChatFeature = "HikePOS.ChatFeature.TenantToHost";

        public const string HikeShowTotalNumberOfItemsOnReceiptFeature = "HikePOS.ShowTotalNumberOfItemsOnReceipt.Feature";
        public const string HikeShowSKUFeature = "HikePOS.ShowSKU.Feature";
        public const string HikeUserLanguageSelectionFeature = "HikePOS.UserLanguageSelection.Feature";

        #region Open Advance Feature
        public const string HikeAllowInvoiceNote = "HikePOS.Allow.InvoiceNote";
        public const string HikeAllowCustomPayment = "HikePOS.Allow.CustomPayment";
        public const string HikeAllowPO = "HikePOS.Allow.PO";
        public const string HikeAllowStockTake = "HikePOS.Allow.StockTake";
        public const string HikeAllowSupplier = "HikePOS.Allow.Supplier";
        public const string HikeAllowRefund = "HikePOS.Allow.Refund";
        public const string HikeAllowRegisterClosure = "HikePOS.Allow.RegisterClosure";
        public const string HikeAllowCustomer = "HikePOS.Allow.Customer";
        #endregion

        public const string HikeAddOnUnLeasedFeature = "HikePOS.AddOnUnLeased.Feature";
        public const string HikePurchaseStockReturnFeature = "HikePOS.PurchaseStockReturn.Feature";
        public const string HikeManageRoleFeature = "HikePOS.ManageRole.Feature";
        public const string HikeMajorActivityReportFeature = "HikePOS.MajorActivityReport.Feature";
        public const string HikeUnitOfMeasureFeature = "HikePOS.UnitOfMeasure.Feature";
        public const string HikeCustomerDeliverAddressFeature = "HikePOS.CustomerDeliverAddress.Feature";
        public const string HikeQuoteSaleFeature = "HikePOS.QuoteSale.Feature";

        public const string HikePOSRegisterClosureTransactionBySkuReportFeature = "HikePOS.RegisterClosure.TransactionBySku.Report.Feature";

        public const string HikeCustomerMultiOutletFeature = "HikePOS.CustomerMultiOutlet.Feature";
        public const string HikeReceiptAdjustFontSizeFeature = "HikePOS.ReceiptAdjustFontSize.Feature";

        public const string HikeStaffTargetReportFeature = "HikePOS.StaffTargetReport.Feature";

        public const string HikeAdvanceFilterFeature = "HikePOS.AdvanceFilterFeature";

        public const string HikeCustomerSecondaryEmailFeature = "HikePOS.CustomerSecondaryEmail.Feature";

        //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik
        public const string HikeCreditNotePrintFeature = "HikePOS.CreditNotePrint.Feature";
        //End Ticket #74631 by pratik

        public const string HikeEnableRestaurantTableOrder = "App.General.EnableRestaurantTableOrder";

        public const int AndroidMSecond = 2000;
        public const int IOSMSecond = 1000;
    }
    //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
}
