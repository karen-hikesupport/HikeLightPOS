using System;
using System.Globalization;
using HikePOS.Enums;
using System.Text.Json;
using Realms;
using Newtonsoft.Json;

namespace HikePOS.Models
{
	
	public class GeneralRuleDto : BaseNotify
	{
		public bool EnableDiscount { get; set; }
		public bool EnableTips { get; set; }
		public bool EnableAppointment { get; set; }
		public bool EnableInventory { get; set; }
		public bool CheckStockValidation { get; set; }
        public bool AllowSellingOutofStock { get; set; }

		public bool RoundUptoFiveCent { get; set; }
        public string RoundUptoCent { get; set; }
		public bool TaxInclusive { get; set; }
		public bool ApplyTaxAfterDiscount { get; set; }
		public bool SendOutOfStockEmail { get; set; }
		public bool SendSyncIssueLogEmail { get; set; }
		public bool DirectPrint { get; set; }

		public bool EnableLoyalty { get; set; }
		public decimal EarnAmountOnPurchgeByLoyalty { get; set; }

		public int LoyaltyPointsValue { get; set; }

		public bool ParkingPaidOrder { get; set; }

		public bool EnableGiftCard { get; set; }

		public bool ActivateOnAccount { get; set; }
		public bool ActivateLayBy { get; set; }

		public bool SwitchUserAfterEachSale { get; set; }


		public bool DisplayLineItemDiscountOnReceipt { get; set; }

        public bool HideProductTypeIfNoProductInIt { get; set; }
        public bool ShowStockCountOnEnterSale { get; set; }


        public bool ShowOtherOutletInventoryOnProductDetailsScreenInPOS { get; set; }
        public bool ToAllowSwitchBetweenUser { get; set; }
        public bool ToAllowUserToLockScreen { get; set; }
        public bool IsAblyAsRealTime { get; set; }
        public bool ExchangeInvoice { get; set; }

		//Ticket #11252 Start : Product images are not showing on POS. By Nikhil 
		public bool IsEnableS3ForStorage { get; set; }
		//Ticket #11252 End. By Nikhil

		//Ticket #13213 : Display individually as separate line items not working.. By Nikhil 
		public bool DisplayMutipleQuantitiesOfSameProduct { get; set; }
		//Ticket #13213 End. By Nikhil

		//Ticket #9609 Start: Sales are not showing in Sale History issue. By Nikhil.
		public bool UseEnglishCalendarWhenNoEnglishCalendar { get; set; }
		//Ticket #9609 End:By Nikhil.

		//Added by rupesh for showing outlet filter option in salehistory

		public bool CanRefundOnDifferentOutlet { get; set; }

		//Ticket #20096 customer profile visible as per one or multiple outlets. By rupesh

		public bool EnableQuoteSale { get; set; }

		public bool RequireDeliveryAddressTocustomer { get; set; }

        [JsonProperty("purgeDataCurrentDate")]
        private string PurgeDataCurrentDateString { get; set; }

        [JsonIgnore]
        public DateTime? PurgeDataCurrentDate
        {
			
            get {
                if (PurgeDataCurrentDateString == null || string.IsNullOrEmpty(PurgeDataCurrentDateString))
				{
					return null;
				}
                  return DateTime.Parse(PurgeDataCurrentDateString, new CultureInfo("en-US"));
			}
        }

        public bool EnableCustomerMultiOutlet { get; set; }

		public bool AutoSuggestPaymentByTag { get; set; }
		public bool AllowCustomerOnAccountInOfflineMode { get; set; }

		//Ticket #22607 iOS - Enable Shipping Option on POS Screen
		public bool AllowShippingTaxOnPOS { get; set; }


		//Ticket #20096 End:By rupesh.

		//Ticket:#30959 iPad - New feature request :: Rule for discount offers when there are more than one offers applicable.by rupesh
		public bool ConflictOffer { get; set; }
		public ConflictOfferType ConflictOfferHighest { get; set; }
		//Ticket:#30959 .by rupesh


		//#32357 iPad :: Feature request :: Show Item Count on POS Screen
		public bool ShowTotalQuantityOfItemsInBasket { get; set; }
		//#32357 iPad :: Feature request :: Show Item Count on POS Screen

		//#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
		public bool ApplySurchargeOnTaxInclusiveTotal { get; set; }
		//#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total

		//#33583 iPad :: Feature request :: Create a parked order that has been paid
		public string ParkPaidOrderRule { get; set; }
		//#33583 iPad :: Feature request :: Create a parked order that has been paid

		//#38721 iPad: Feature request - Changes to Payment Status and Fulfilment Status
		public bool ExcludeOnAccountSalesFromTheFulfillment { get; set; }
		//#38721 iPad: Feature request - Changes to Payment Status and Fulfilment Status

		//Start #40634 iPad  :: Feature request - About How to Handle Transaction Date of Parked Sales
		public bool HandleDateOfParkedSales { get; set; }
		//#40634 iPad: End by nutan

        //Start #68487 Lay-by completion date option: Same as parked sale by Pratik
        public bool HandleDateOfLayBySales { get; set; }
        //#68487 iPad: End by Pratik

		//Ticket start:#36542 Move Out of Stock to end in Point of Sale Products Listings.by rupesh
		public bool ActivateSmartSearchOnPOS { get; set; }
		//Ticket end:#36542 .by rupesh

		//Ticket start:#42157 Feature Request - iPad: show % value for invoice level Discount in print receipt.by rupesh
		public bool ShowInvoiceLevelDiscountInPercentage { get; set; }
		//Ticket end:#42157 .by rupesh

		//Start #45386 iPad: Please ignore birthday "year" when add new customer
		public bool DoNotAskForTheYearInTheBirthDateOfTheCustomers { get; set; }
		//#45386 iPad: End by nutan

		//Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
		public bool DisplayProductsBasedOnTheOrderAddedInCart { get; set; }
        //End:#45375 .by rupesh

        //Ticket Start:#45367 iPad: FR - Group Products by Category on POS.by rupesh
        bool _showGroupProductsByCategory;
        public bool ShowGroupProductsByCategory
        {
            get { return _showGroupProductsByCategory; }
            set { _showGroupProductsByCategory = value; SetPropertyChanged(nameof(ShowGroupProductsByCategory)); }
        }
		//Ticket End:#45367.by rupesh

        //Ticket start:#45654 iPad: FR - Bypassing the Who is Selling screen.by rupesh
        public bool EachUserHasAUniquePinCode { get; set; }
        //Ticket end:#45654 .by rupesh

        //Tcket start:#66015 iOS: Tax removed from product does not work correctly on Cart.by rupesh
        public bool RetailPriceUpdateWhenTaxChangeCart { get; set; }
        //Tcket end:#66015 .by rupesh

        //Start #81159 Show product name if no image available By Pratik
        public bool DisplayFullProductNameOnPOS { get; set; }
        //End #81159 By Pratik

         public bool displayFullCategoryNameOnPOS { get; set; } //Start #93286 by Pratik

        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        public bool ServedByLineItem { get; set; }
        //end #84287 .by Pratik

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public bool AllowPartialFulfilled { get; set; }
        //end #84293 .by Pratik

       //Start #90944 iOS:FR Gift cards expiry date by Pratik
        public bool EnableGiftcardExpiration { get; set; }
        public string GiftcardValidity { get; set; }

        public bool EnableGiftCardSelling { get; set; }

        [JsonIgnore]
        public GiftcardValidityDto GiftcardExpiration => (EnableGiftcardExpiration && !string.IsNullOrEmpty(GiftcardValidity)) ? JsonConvert.DeserializeObject<GiftcardValidityDto>(GiftcardValidity)  : new GiftcardValidityDto();
        //end #90944 .by Pratik

        //Start #90946 iOS:FR  Item Serial Number Tracking by Pratik
        public bool EnableSerialNumberTracking { get; set; }
        //End #90946 by Pratik

        public bool OnAccountPONumberForAccouting { get; set; } //Start #91991 By Pratik

        public bool EnableTenantEmail { get; set; } //Start #92768 By Pratik

        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
        public bool EnableBackOrder { get; set; }
        //Ticket end:#92764.by rupesh

        //#94565
        public bool EnableRestaurantTableOrder { get; set; }

        public bool EnableOnGoingSaleRefundAndExchange { get; set; }
        //#94565

        public string ConnectedAccountingAddOn { get; set; } //#94418

        public bool AllowOnAccountPaymentHikePay { get; set; }

        

        public GeneralRuleDB ToModel()
        {
            GeneralRuleDB generalRuleDB = new GeneralRuleDB
            {
                EnableDiscount = EnableDiscount,
                EnableTips = EnableTips,
                EnableAppointment = EnableAppointment,
                EnableInventory = EnableInventory,
                CheckStockValidation = CheckStockValidation,
                AllowSellingOutofStock = AllowSellingOutofStock,
                RoundUptoFiveCent = RoundUptoFiveCent,
                RoundUptoCent = RoundUptoCent,
                TaxInclusive = TaxInclusive,
                ApplyTaxAfterDiscount = ApplyTaxAfterDiscount,
                SendOutOfStockEmail = SendOutOfStockEmail,
                SendSyncIssueLogEmail = SendSyncIssueLogEmail,
                DirectPrint = DirectPrint,
                EnableLoyalty = EnableLoyalty,
                EarnAmountOnPurchgeByLoyalty = EarnAmountOnPurchgeByLoyalty,
                LoyaltyPointsValue = LoyaltyPointsValue,
                ParkingPaidOrder = ParkingPaidOrder,
                EnableGiftCard = EnableGiftCard,
                ActivateOnAccount = ActivateOnAccount,
                ActivateLayBy = ActivateLayBy,
                SwitchUserAfterEachSale = SwitchUserAfterEachSale,
                DisplayLineItemDiscountOnReceipt = DisplayLineItemDiscountOnReceipt,
                HideProductTypeIfNoProductInIt = HideProductTypeIfNoProductInIt,
                ShowStockCountOnEnterSale = ShowStockCountOnEnterSale,
                ShowOtherOutletInventoryOnProductDetailsScreenInPOS = ShowOtherOutletInventoryOnProductDetailsScreenInPOS,
                ToAllowSwitchBetweenUser = ToAllowSwitchBetweenUser,
                ToAllowUserToLockScreen = ToAllowUserToLockScreen,
                IsAblyAsRealTime = IsAblyAsRealTime,
                ExchangeInvoice = ExchangeInvoice,
                IsEnableS3ForStorage = IsEnableS3ForStorage,
                DisplayMutipleQuantitiesOfSameProduct = DisplayMutipleQuantitiesOfSameProduct,
                UseEnglishCalendarWhenNoEnglishCalendar = UseEnglishCalendarWhenNoEnglishCalendar,
                CanRefundOnDifferentOutlet = CanRefundOnDifferentOutlet,
                EnableQuoteSale = EnableQuoteSale,
                RequireDeliveryAddressTocustomer = RequireDeliveryAddressTocustomer,
                EnableCustomerMultiOutlet = EnableCustomerMultiOutlet,
                AutoSuggestPaymentByTag = AutoSuggestPaymentByTag,
                AllowCustomerOnAccountInOfflineMode = AllowCustomerOnAccountInOfflineMode,
                AllowShippingTaxOnPOS = AllowShippingTaxOnPOS,
                ConflictOffer = ConflictOffer,
                ConflictOfferHighest = (int)ConflictOfferHighest,
                ShowTotalQuantityOfItemsInBasket = ShowTotalQuantityOfItemsInBasket,
                ApplySurchargeOnTaxInclusiveTotal = ApplySurchargeOnTaxInclusiveTotal,
                ParkPaidOrderRule = ParkPaidOrderRule,
                ExcludeOnAccountSalesFromTheFulfillment = ExcludeOnAccountSalesFromTheFulfillment,
                HandleDateOfParkedSales = HandleDateOfParkedSales,
                ActivateSmartSearchOnPOS = ActivateSmartSearchOnPOS,
                ShowInvoiceLevelDiscountInPercentage = ShowInvoiceLevelDiscountInPercentage,
                DoNotAskForTheYearInTheBirthDateOfTheCustomers = DoNotAskForTheYearInTheBirthDateOfTheCustomers,
                DisplayProductsBasedOnTheOrderAddedInCart = DisplayProductsBasedOnTheOrderAddedInCart,
                ShowGroupProductsByCategory = ShowGroupProductsByCategory,
                EachUserHasAUniquePinCode = EachUserHasAUniquePinCode,
                RetailPriceUpdateWhenTaxChangeCart = RetailPriceUpdateWhenTaxChangeCart,
                //Start #81159 Show product name if no image available By Pratik
                DisplayFullProductNameOnPOS = DisplayFullProductNameOnPOS,
                //End #81159 By Pratik
                displayFullCategoryNameOnPOS = displayFullCategoryNameOnPOS, //Start #93286 by Pratik
                //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
                ServedByLineItem = ServedByLineItem,
                //end #84287 .by Pratik
                //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                AllowPartialFulfilled = AllowPartialFulfilled,
                //end #84293 .by Pratik
                //Start #90944 iOS:FR Gift cards expiry date by Pratik
                EnableGiftcardExpiration = EnableGiftcardExpiration,
                GiftcardValidity = GiftcardValidity,
                EnableGiftCardSelling = EnableGiftCardSelling,
                //end #90944 .by Pratik
                //Start #90946 iOS:FR  Item Serial Number Tracking by Pratik
                EnableSerialNumberTracking = EnableSerialNumberTracking,
                //End #90946 by Pratik
                OnAccountPONumberForAccouting = OnAccountPONumberForAccouting, //Start #91991 By Pratik
                EnableTenantEmail = EnableTenantEmail, //Start #92768 By Pratik
                EnableBackOrder = EnableBackOrder, // start #92764 by rupesh
                EnableRestaurantTableOrder = EnableRestaurantTableOrder,//#94565
                EnableOnGoingSaleRefundAndExchange = EnableOnGoingSaleRefundAndExchange,//#94565   
                ConnectedAccountingAddOn = ConnectedAccountingAddOn, //#94418   
                AllowOnAccountPaymentHikePay = AllowOnAccountPaymentHikePay     

            };
            return generalRuleDB;
        }

        public static GeneralRuleDto FromModel(GeneralRuleDB generalRuleDB)
        {
            if (generalRuleDB == null)
                return null;
            GeneralRuleDto generalRuleDto = new GeneralRuleDto
            {
                EnableDiscount = generalRuleDB.EnableDiscount,
                EnableTips = generalRuleDB.EnableTips,
                EnableAppointment = generalRuleDB.EnableAppointment,
                EnableInventory = generalRuleDB.EnableInventory,
                CheckStockValidation = generalRuleDB.CheckStockValidation,
                AllowSellingOutofStock = generalRuleDB.AllowSellingOutofStock,
                RoundUptoFiveCent = generalRuleDB.RoundUptoFiveCent,
                RoundUptoCent = generalRuleDB.RoundUptoCent,
                TaxInclusive = generalRuleDB.TaxInclusive,
                ApplyTaxAfterDiscount = generalRuleDB.ApplyTaxAfterDiscount,
                SendOutOfStockEmail = generalRuleDB.SendOutOfStockEmail,
                SendSyncIssueLogEmail = generalRuleDB.SendSyncIssueLogEmail,
                DirectPrint = generalRuleDB.DirectPrint,
                EnableLoyalty = generalRuleDB.EnableLoyalty,
                EarnAmountOnPurchgeByLoyalty = generalRuleDB.EarnAmountOnPurchgeByLoyalty,
                LoyaltyPointsValue = generalRuleDB.LoyaltyPointsValue,
                ParkingPaidOrder = generalRuleDB.ParkingPaidOrder,
                EnableGiftCard = generalRuleDB.EnableGiftCard,
                ActivateOnAccount = generalRuleDB.ActivateOnAccount,
                ActivateLayBy = generalRuleDB.ActivateLayBy,
                SwitchUserAfterEachSale = generalRuleDB.SwitchUserAfterEachSale,
                DisplayLineItemDiscountOnReceipt = generalRuleDB.DisplayLineItemDiscountOnReceipt,
                HideProductTypeIfNoProductInIt = generalRuleDB.HideProductTypeIfNoProductInIt,
                ShowStockCountOnEnterSale = generalRuleDB.ShowStockCountOnEnterSale,
                ShowOtherOutletInventoryOnProductDetailsScreenInPOS = generalRuleDB.ShowOtherOutletInventoryOnProductDetailsScreenInPOS,
                ToAllowSwitchBetweenUser = generalRuleDB.ToAllowSwitchBetweenUser,
                ToAllowUserToLockScreen = generalRuleDB.ToAllowUserToLockScreen,
                IsAblyAsRealTime = generalRuleDB.IsAblyAsRealTime,
                ExchangeInvoice = generalRuleDB.ExchangeInvoice,
                IsEnableS3ForStorage = generalRuleDB.IsEnableS3ForStorage,
                DisplayMutipleQuantitiesOfSameProduct = generalRuleDB.DisplayMutipleQuantitiesOfSameProduct,
                UseEnglishCalendarWhenNoEnglishCalendar = generalRuleDB.UseEnglishCalendarWhenNoEnglishCalendar,
                CanRefundOnDifferentOutlet = generalRuleDB.CanRefundOnDifferentOutlet,
                EnableQuoteSale = generalRuleDB.EnableQuoteSale,
                RequireDeliveryAddressTocustomer = generalRuleDB.RequireDeliveryAddressTocustomer,
                EnableCustomerMultiOutlet = generalRuleDB.EnableCustomerMultiOutlet,
                AutoSuggestPaymentByTag = generalRuleDB.AutoSuggestPaymentByTag,
                AllowCustomerOnAccountInOfflineMode = generalRuleDB.AllowCustomerOnAccountInOfflineMode,
                AllowShippingTaxOnPOS = generalRuleDB.AllowShippingTaxOnPOS,
                ConflictOffer = generalRuleDB.ConflictOffer,
                ConflictOfferHighest = (ConflictOfferType)generalRuleDB.ConflictOfferHighest,
                ShowTotalQuantityOfItemsInBasket = generalRuleDB.ShowTotalQuantityOfItemsInBasket,
                ApplySurchargeOnTaxInclusiveTotal = generalRuleDB.ApplySurchargeOnTaxInclusiveTotal,
                ParkPaidOrderRule = generalRuleDB.ParkPaidOrderRule,
                ExcludeOnAccountSalesFromTheFulfillment = generalRuleDB.ExcludeOnAccountSalesFromTheFulfillment,
                HandleDateOfParkedSales = generalRuleDB.HandleDateOfParkedSales,
                ActivateSmartSearchOnPOS = generalRuleDB.ActivateSmartSearchOnPOS,
                ShowInvoiceLevelDiscountInPercentage = generalRuleDB.ShowInvoiceLevelDiscountInPercentage,
                DoNotAskForTheYearInTheBirthDateOfTheCustomers = generalRuleDB.DoNotAskForTheYearInTheBirthDateOfTheCustomers,
                DisplayProductsBasedOnTheOrderAddedInCart = generalRuleDB.DisplayProductsBasedOnTheOrderAddedInCart,
                ShowGroupProductsByCategory = generalRuleDB.ShowGroupProductsByCategory,
                EachUserHasAUniquePinCode = generalRuleDB.EachUserHasAUniquePinCode,
                RetailPriceUpdateWhenTaxChangeCart = generalRuleDB.RetailPriceUpdateWhenTaxChangeCart,
                //Start #81159 Show product name if no image available By Pratik
                DisplayFullProductNameOnPOS = generalRuleDB.DisplayFullProductNameOnPOS,
                //End #81159 By Pratik
                displayFullCategoryNameOnPOS = generalRuleDB.displayFullCategoryNameOnPOS, //Start #93286 by Pratik
                //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
                ServedByLineItem = generalRuleDB.ServedByLineItem,
                //end #84287 .by Pratik
                //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                AllowPartialFulfilled = generalRuleDB.AllowPartialFulfilled,
                //end #84293 .by Pratik
                //Start #90944 iOS:FR Gift cards expiry date by Pratik
                EnableGiftcardExpiration = generalRuleDB.EnableGiftcardExpiration,
                GiftcardValidity = generalRuleDB.GiftcardValidity,
                EnableGiftCardSelling = generalRuleDB.EnableGiftCardSelling,
                //end #90944 .by Pratik
                //Start #90946 iOS:FR  Item Serial Number Tracking by Pratik
                EnableSerialNumberTracking = generalRuleDB.EnableSerialNumberTracking,
                //End #90946 by Pratik
                OnAccountPONumberForAccouting = generalRuleDB.OnAccountPONumberForAccouting, //Start #91991 By Pratik
                EnableTenantEmail = generalRuleDB.EnableTenantEmail, //Start #92768 By Pratik
                EnableBackOrder = generalRuleDB.EnableBackOrder,// start #92764 by rupesh
                EnableRestaurantTableOrder = generalRuleDB.EnableRestaurantTableOrder,//#94565
                EnableOnGoingSaleRefundAndExchange = generalRuleDB.EnableOnGoingSaleRefundAndExchange,//#94565  
                ConnectedAccountingAddOn = generalRuleDB.ConnectedAccountingAddOn, //#94418
                AllowOnAccountPaymentHikePay = generalRuleDB.AllowOnAccountPaymentHikePay
            };
            return generalRuleDto;

        }
    }

    public partial class GeneralRuleDB : IRealmObject
    {
        public bool EnableDiscount { get; set; }
        public bool EnableTips { get; set; }
        public bool EnableAppointment { get; set; }
        public bool EnableInventory { get; set; }
        public bool CheckStockValidation { get; set; }
        public bool AllowSellingOutofStock { get; set; }
        public bool RoundUptoFiveCent { get; set; }
        public string RoundUptoCent { get; set; }
        public bool TaxInclusive { get; set; }
        public bool ApplyTaxAfterDiscount { get; set; }
        public bool SendOutOfStockEmail { get; set; }
        public bool SendSyncIssueLogEmail { get; set; }
        public bool DirectPrint { get; set; }
        public bool EnableLoyalty { get; set; }
        public decimal EarnAmountOnPurchgeByLoyalty { get; set; }
        public int LoyaltyPointsValue { get; set; }
        public bool ParkingPaidOrder { get; set; }
        public bool EnableGiftCard { get; set; }
        public bool ActivateOnAccount { get; set; }
        public bool ActivateLayBy { get; set; }
        public bool SwitchUserAfterEachSale { get; set; }
        public bool DisplayLineItemDiscountOnReceipt { get; set; }
        public bool HideProductTypeIfNoProductInIt { get; set; }
        public bool ShowStockCountOnEnterSale { get; set; }
        public bool ShowOtherOutletInventoryOnProductDetailsScreenInPOS { get; set; }
        public bool ToAllowSwitchBetweenUser { get; set; }
        public bool ToAllowUserToLockScreen { get; set; }
        public bool IsAblyAsRealTime { get; set; }
        public bool ExchangeInvoice { get; set; }
        public bool IsEnableS3ForStorage { get; set; }
        public bool DisplayMutipleQuantitiesOfSameProduct { get; set; }
        public bool UseEnglishCalendarWhenNoEnglishCalendar { get; set; }
        public bool CanRefundOnDifferentOutlet { get; set; }
        public bool EnableQuoteSale { get; set; }
        public bool RequireDeliveryAddressTocustomer { get; set; }
        public bool EnableCustomerMultiOutlet { get; set; }
        public bool AutoSuggestPaymentByTag { get; set; }
        public bool AllowCustomerOnAccountInOfflineMode { get; set; }
        public bool AllowShippingTaxOnPOS { get; set; }
        public bool ConflictOffer { get; set; }
        public int ConflictOfferHighest { get; set; }
        public bool ShowTotalQuantityOfItemsInBasket { get; set; }
        public bool ApplySurchargeOnTaxInclusiveTotal { get; set; }
        public string ParkPaidOrderRule { get; set; }
        public bool ExcludeOnAccountSalesFromTheFulfillment { get; set; }
        public bool HandleDateOfParkedSales { get; set; }
        public bool ActivateSmartSearchOnPOS { get; set; }
        public bool ShowInvoiceLevelDiscountInPercentage { get; set; }
        public bool DoNotAskForTheYearInTheBirthDateOfTheCustomers { get; set; }
        public bool DisplayProductsBasedOnTheOrderAddedInCart { get; set; }
        public bool ShowGroupProductsByCategory { get; set; }
        public bool EachUserHasAUniquePinCode { get; set; }
        public bool RetailPriceUpdateWhenTaxChangeCart { get; set; }
        //Start #81159 Show product name if no image available By Pratik
        public bool DisplayFullProductNameOnPOS { get; set; }
        //End #81159 By Pratik

        public bool displayFullCategoryNameOnPOS { get; set; } //Start #93286 by Pratik
        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        public bool ServedByLineItem { get; set; }
        //end #84287 .by Pratik
        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public bool AllowPartialFulfilled { get; set; }
        //end #84293 .by Pratik

        //Start #90944 iOS:FR Gift cards expiry date by Pratik
        public bool EnableGiftcardExpiration { get; set; }
        public string GiftcardValidity { get; set; }
        public bool EnableGiftCardSelling { get; set; }

        //end #90944 .by Pratik

        //Start #90946 iOS:FR  Item Serial Number Tracking by Pratik
        public bool EnableSerialNumberTracking { get; set; }
        //End #90946 by Pratik

        public bool OnAccountPONumberForAccouting { get; set; } //Start #91991 By Pratik

        public bool EnableTenantEmail { get; set; } //Start #92768 By Pratik

        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
        public bool EnableBackOrder { get; set; }
        //Ticket end:#92764.by rupesh
        //#94565
        public bool EnableRestaurantTableOrder { get; set; }
        public bool EnableOnGoingSaleRefundAndExchange { get; set; }
        //#94565

        public string ConnectedAccountingAddOn { get; set; } //#94418

        public bool AllowOnAccountPaymentHikePay { get; set; }

    }

    //Start #90944 iOS:FR Gift cards expiry date by Pratik
    public class GiftcardValidityDto
    {
        public int validityPeriod { get; set; }
        public string validityTime { get; set; }
    }
    //end #90944 .by Pratik
}
