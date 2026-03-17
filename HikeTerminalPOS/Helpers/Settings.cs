using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using Newtonsoft.Json;

namespace HikePOS.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static IPreferences AppSettings
        {
            get
            {
                return Preferences.Default;
            }
        }

        #region Setting Constants

        const string SettingsKey = "settings_key";
        static readonly string SettingsDefault = string.Empty;

        const string AccessTokenKey = "accesstoken_key";
        static readonly string AccessTokenDefault = string.Empty;

        const string StoreIdKey = "storeid_key";
        static readonly int StoreIdDefault = 0;

        const string TenantIdKey = "tenantId_key";
        static readonly int TenantIdDefault = 0;


        const string UniqueDeviceIDKey = "UniqueDeviceID_Key";
        static readonly string UniqueDeviceIDDefault = string.Empty;




        const string StoreNameKey = "storename_key";
        static readonly string StoreNameDefault = string.Empty;


        const string TenantNameKey = "tenantname_key";
        static readonly string TenantNameDefault = string.Empty;


        //#37229 iPad: Login session expired
        const string AccessTokenClientIdKey = "AccessToken_Client_Id";
        static readonly string AccessTokenClientIdDefault = ServiceConfiguration.AccessToken_Client_Id;

        const string AccessTokenClientSecretKey = "AccessToken_Client_Secret";
        static readonly string AccessTokenClientSecretDefault = ServiceConfiguration.AccessToken_Client_Secret;

        //#37229 iPad: Login session expired

        const string SelectedOutletNameKey = "selectedoutletname_key";
        const string SelectedOutletKey = "selectedoutlet_key";
        static readonly string SelectedOutletNameDefault = string.Empty;

        const string SelectedOutletIdKey = "selectedoutletid_key";
        static readonly int SelectedOutletIdDefault = 0;

        //const string NoOfPrintKey = "noofprint_key";
        //static readonly int NoOfPrintDefault = 1;


        //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan
        const string GrantedFeatureKey = "GrantedFeature_Key";


        const string UserKey = "User_Key";
        const string GrantedPermissionKey = "GrantedPermission_Key";

        const string CurrentRegisterKey = "CurrentRegister_Key";
        const string StoreGeneralRuleKey = "storegeneralrule_key";
        const string StoreShopdtoKey = "storeshopdto_key";
        const string SubscriptionKey = "Subscription_key";

        const string VantivDetailsKey = "vantivDetailsKey_key";
        const string CloverDetailsKey = "cloverDetailsKey_key";
        const string CastlesDetailsKey = "cloverDetailsKey_key";
        const string TyroTapToPayKey = "tyroTapToPayKey_key";
        const string DisplayAppconfigKey = "DisplayAppconfigKey";

        const string AllCountriesKey = "AllCountries_key";
        const string AllHearAboutUsKey = "AllHearAboutUs_key";



        const string StoreZoneAndFormatDetailKey = "zoneandformatdetail_key";

        const string CachePrintersKey = "cacheprinters_key";
        const string CurrentPrinterKey = "currentprinter_key";

        //const string StoreCultureInfoKey = "StoreCultureInfo_key";

        const string AutoLockKey = "autolock_key";
        const string IsAppLockedKey = "isapplocked_key";

        const string NumberOfPrintKey = "numberOfPrint_key";
        const string PrintDocketKey = "PrintDocket_key";
        const string StartingNumberKey = "StartingNumber_key";
        const string EndingNumberKey = "EndingNumber_key";
        const string CurrentNumberKey = "CurrentNumber_Key";
        const string PrintCustomerNumberKey = "PrintCustomerNumber_Key";

        //const string RenewAccessTokenInputKey = "RenewAccessTokenInputKey_key";


        const string AutoLockActiveKey = "AutoLockActive_key";


        const string PaypalTokenKey = "PaypalIntegrationkey_key";
        static readonly string PaypalTokenDefault = string.Empty;

        const string TyroIntegrationkeyKey = "TyroIntegrationkey_key";
        static readonly string TyroIntegrationkeyDefault = string.Empty;

        const string StoreTimeZoneInfoKey = "StoreTimeZoneInfo_key";
        const string StoreCurrencySymbolKey = "StoreCurrencySymbol_key";
        //Ticket #9042 Start : Customer country field added. By Nikhil.
        const string StoreCountryCodeKey = "StoreCountryCode_key";
        //Ticket #9042 End : By Nikhil.

        const string CountryCodeKey = "CountryCode_key";

        const string RestaurantPOSKey = "RestaurantPOS_key"; //#94565

        const string StoreCurrencyCodeKey = "StoreCurrencyCode_key";
        const string StoreCultureKey = "StoreCulture_key";
        //Ticket start:#26913 iOS - Separator (comma) Not Applied.by rupesh
        const string SymbolForDecimalSeperatorForNonDotKey = "SymbolForDecimalSeperatorForNonDot_key";
        //Ticket end:#26913 .by rupesh

        const string QRCodeSelectedKey = "QRCodeSelected_key";
        const string InfoSelectedKey = "InfoSelected_key";
        const string AllReceiptRegisterActiveKey = "AllReceiptRegisterActive_key";

        const string CurrentUserEmailKey = "currentUserEmail_Key";
        static readonly string CurrentUserEmailDefault = "";


        const string LastLoadedHistoryDateKey = "LastLoadedHistoryDate_Key";

        const string VariantOrderKey = "VariantOrder_Key";
        static readonly int VariantOrderIndexDefault = 1;
        const string CurrentDatabaseNameKey = "currentDatabaseName_Key";
        const string CurrentDatabaseTypeKey = "currentDatabaseType_Key";

        #endregion


        public static string GeneralSettings
        {
            get { return AppSettings.Get(SettingsKey, SettingsDefault); }
            set { AppSettings.Set(SettingsKey, value); }
        }

        public static string AccessToken
        {
            get { return AppSettings.Get(AccessTokenKey, AccessTokenDefault); }
            set { AppSettings.Set(AccessTokenKey, value); }
        }

        public static bool RememberBrowser
        {
            get { return AppSettings.Get("RememberBrowser", false); }
            set { AppSettings.Set("RememberBrowser", value); }
        }

        public static bool IsEnterSaleFirstTimeLoad { get; set; } = false;


        public static string RefreshToken
        {
            get { return AppSettings.Get(nameof(RefreshToken), string.Empty); }
            set { AppSettings.Set(nameof(RefreshToken), value); }
        }


        public static int TenantId
        {
            get { return AppSettings.Get(TenantIdKey, TenantIdDefault); }
            set { AppSettings.Set(TenantIdKey, value); }
        }

        public static string UniqueDeviceID
        {
            get { return AppSettings.Get(UniqueDeviceIDKey, UniqueDeviceIDDefault); }
            set { AppSettings.Set(UniqueDeviceIDKey, value); }
        }

        public static int StoreId
        {
            get { return AppSettings.Get(StoreIdKey, StoreIdDefault); }
            set { AppSettings.Set(StoreIdKey, value); }
        }

        public static string StoreName
        {
            get { return AppSettings.Get(StoreNameKey, StoreNameDefault); }
            set { AppSettings.Set(StoreNameKey, value); }
        }

        public static string TenantName
        {
            get { return AppSettings.Get(TenantNameKey, TenantNameDefault); }
            set { AppSettings.Set(TenantNameKey, value); }
        }

        public static int AppEnvironment
        {
            get { return AppSettings.Get(nameof(AppEnvironment), 0); }
            set { AppSettings.Set(nameof(AppEnvironment), value); }
        }


        //#37229 iPad: Login session expired
        public static string AccessTokenClientId
        {
            get { return AppSettings.Get(AccessTokenClientIdKey, AccessTokenClientIdDefault); }
            set { AppSettings.Set(AccessTokenClientIdKey, value); }
        }


        public static string AccessTokenClientSecretId
        {
            get { return AppSettings.Get(AccessTokenClientSecretKey, AccessTokenClientSecretDefault); }
            set { AppSettings.Set(AccessTokenClientSecretKey, value); }
        }
        //#37229 iPad: Login session expired


        public static int SelectedOutletId
        {
            get { return AppSettings.Get(SelectedOutletIdKey, SelectedOutletIdDefault); }
            set { AppSettings.Set(SelectedOutletIdKey, value); }
        }

        public static string SelectedOutletName
        {
            get { return AppSettings.Get(SelectedOutletNameKey, SelectedOutletNameDefault); }
            set { AppSettings.Set(SelectedOutletNameKey, value); }
        }
        //Ticket start:#38783 iPad: Feature request - Register's Name in Process Sale.by rupesh
        public static OutletDto_POS SelectedOutlet
        {
            get
            {
                OutletDto_POS selectedOutlet = null;
                var serializedCurrentRegister = Preferences.Default.Get<string>(SelectedOutletKey, null);
                if (serializedCurrentRegister != null &&
                    serializedCurrentRegister != "null")
                {
                    selectedOutlet = JsonConvert.DeserializeObject<OutletDto_POS>(serializedCurrentRegister);
                }

                return selectedOutlet;
            }
            set
            {
                Preferences.Default.Set(SelectedOutletKey, JsonConvert.SerializeObject(value));
            }

        }
        //Ticket end:#38783 .by rupesh

        public static RegisterDto CurrentRegister
        {
            get
            {
                RegisterDto currentRegister = null;
                var serializedCurrentRegister = Preferences.Default.Get<string>(CurrentRegisterKey, null);
                if (serializedCurrentRegister != null &&
                    serializedCurrentRegister != "null")
                {
                    currentRegister = JsonConvert.DeserializeObject<RegisterDto>(serializedCurrentRegister);
                }

                return currentRegister;
            }
            set
            {
                Preferences.Default.Set(CurrentRegisterKey, JsonConvert.SerializeObject(value));
            }
        }


        public static UserListDto CurrentUser
        {
            get
            {
                UserListDto currentUser = null;
                var serializedCurrentUser = Preferences.Default.Get<string>(UserKey, null);
                if (serializedCurrentUser != null)
                {
                    currentUser = JsonConvert.DeserializeObject<UserListDto>(serializedCurrentUser);
                }

                return currentUser;
            }
            set
            {
                Preferences.Default.Set(UserKey, JsonConvert.SerializeObject(value));
            }
        }

        public static string CurrentUserEmail
        {
            get { return AppSettings.Get(CurrentUserEmailKey, CurrentUserEmailDefault); }
            set { AppSettings.Set(CurrentUserEmailKey, value); }
        }

        public static bool IsAblyAsRealTime
        {
            get { return AppSettings.Get(nameof(IsAblyAsRealTime), false); }
            set { AppSettings.Set(nameof(IsAblyAsRealTime), value); }
        }

        //#94565
        public static bool IsRestaurantPOS
        {
            get { return AppSettings.Get(nameof(RestaurantPOSKey), StoreGeneralRule.EnableRestaurantTableOrder); } // Change True to false
            set { AppSettings.Set(nameof(RestaurantPOSKey), value); }
        }

        public static int SelectedFloorID
        {
            get { return AppSettings.Get("SelectedFloorID_Key", 0); }
            set { AppSettings.Set("SelectedFloorID_Key", value); }
        }
        //#94565

        public static GeneralRuleDto StoreGeneralRule
        {
            get
            {
                GeneralRuleDto generalrule = null;
                var serializedGeneralrule = Preferences.Default.Get<string>(StoreGeneralRuleKey, null);
                if (serializedGeneralrule != null)
                {
                    generalrule = JsonConvert.DeserializeObject<GeneralRuleDto>(serializedGeneralrule);
                }

                return generalrule;
            }
            set
            {
                Preferences.Default.Set(StoreGeneralRuleKey, JsonConvert.SerializeObject(value));
            }
        }

        public static ShopDto StoreShopDto
        {
            get
            {
                ShopDto generalShopDto = null;
                var serialized = Preferences.Default.Get<string>(StoreShopdtoKey, null);
                if (serialized != null)
                {
                    generalShopDto = JsonConvert.DeserializeObject<ShopDto>(serialized);
                }

                return generalShopDto;
            }
            set
            {
                Preferences.Default.Set(StoreShopdtoKey, JsonConvert.SerializeObject(value));
            }
        }



        public static SubscriptionDto Subscription
        {
            get
            {
                SubscriptionDto subscriptionDto = new SubscriptionDto();
                var serialized = Preferences.Default.Get<string>(SubscriptionKey, null);
                if (serialized != null)
                {
                    subscriptionDto = JsonConvert.DeserializeObject<SubscriptionDto>(serialized);
                }

                return subscriptionDto;
            }
            set
            {
                Preferences.Default.Set(SubscriptionKey, JsonConvert.SerializeObject(value));
            }
        }


        public static ObservableCollection<CountriesDto> AllCountries
        {
            get
            {
                ObservableCollection<CountriesDto> AllCountriesDto = new ObservableCollection<CountriesDto>();
                var serialized = Preferences.Default.Get<string>(AllCountriesKey, null);
                if (serialized != null)
                {
                    AllCountriesDto = JsonConvert.DeserializeObject<ObservableCollection<CountriesDto>>(serialized);
                }

                return AllCountriesDto;
            }
            set
            {
                Preferences.Default.Set(AllCountriesKey, JsonConvert.SerializeObject(value));
            }
        }

        public static ObservableCollection<HearAboutDto> AllHearAbout
        {
            get
            {
                ObservableCollection<HearAboutDto> AllHearAboutUs = new ObservableCollection<HearAboutDto>();
                var serialized = Preferences.Default.Get<string>(AllHearAboutUsKey, null);
                if (serialized != null)
                {
                    AllHearAboutUs = JsonConvert.DeserializeObject<ObservableCollection<HearAboutDto>>(serialized);
                }

                return AllHearAboutUs;
            }
            set
            {
                Preferences.Default.Set(AllHearAboutUsKey, JsonConvert.SerializeObject(value));
            }
        }

        public static ZoneAndFormatDetailDto StoreZoneAndFormatDetail
        {
            get
            {
                ZoneAndFormatDetailDto zoneandformatedetail = null;
                var serializedZoneAndFormatDetail = Preferences.Default.Get<string>(StoreZoneAndFormatDetailKey, null);
                if (serializedZoneAndFormatDetail != null)
                {
                    zoneandformatedetail = JsonConvert.DeserializeObject<ZoneAndFormatDetailDto>(serializedZoneAndFormatDetail);
                }

                return zoneandformatedetail;
            }
            set
            {
                Preferences.Default.Set(StoreZoneAndFormatDetailKey, JsonConvert.SerializeObject(value));
            }
        }


        public static string StoreTimeZoneInfoId
        {
            get { return AppSettings.Get<string>(StoreTimeZoneInfoKey, null); }
            set { AppSettings.Set(StoreTimeZoneInfoKey, value); }
        }

        //Ticket #9042 Start : Customer country field added. By Nikhil.
        public static string StoreCountryCode
        {
            get { return AppSettings.Get<string>(StoreCountryCodeKey, null); }
            set { AppSettings.Set(StoreCountryCodeKey, value); }
        }
        //Ticket #9042 End:By Nikhil. 


        public static string CountryCode
        {
            get { return AppSettings.Get<string>(CountryCodeKey, null); }
            set { AppSettings.Set(CountryCodeKey, value); }
        }

        public static string StoreCurrencySymbol
        {
            get { return AppSettings.Get<string>(StoreCurrencySymbolKey, null); }
            set { AppSettings.Set(StoreCurrencySymbolKey, value); }
        }

        public static int StoreDecimalDigit
        {
            get { return AppSettings.Get<int>(nameof(StoreDecimalDigit), 2); }
            set { AppSettings.Set(nameof(StoreDecimalDigit), value); }
        }

        public static bool IsPrintSKU { get; set; }

        public static string StoreCurrencyCode
        {
            get { return AppSettings.Get<string>(StoreCurrencyCodeKey, null); }
            set { AppSettings.Set(StoreCurrencyCodeKey, value); }
        }

        public static string StoreCulture
        {
            get { return AppSettings.Get<string>(StoreCultureKey, null); }
            set { AppSettings.Set(StoreCultureKey, value); }
        }

        //Ticket start:#26913 iOS - Separator (comma) Not Applied.by rupesh
        public static string SymbolForDecimalSeperatorForNonDot
        {
            get { return AppSettings.Get<string>(SymbolForDecimalSeperatorForNonDotKey, null); }
            set { AppSettings.Set(SymbolForDecimalSeperatorForNonDotKey, value); }
        }
        //Ticket end:#26913 .by rupesh

        #region Printer Section

        public static ObservableCollection<Printer> GetCachePrinters
        {
            get
            {
                List<Printer> CachePrinters = null;
                var serializedCachePrinters = Preferences.Default.Get<string>(CachePrintersKey, null);
                if (serializedCachePrinters != null)
                {
                    CachePrinters = JsonConvert.DeserializeObject<List<Printer>>(serializedCachePrinters);
                }
                if (CachePrinters != null && CachePrinters.Count > 0)
                    return new ObservableCollection<Printer>(CachePrinters);
                else
                    return new ObservableCollection<Printer>();
            }
            set
            {
                Preferences.Default.Set(CachePrintersKey, JsonConvert.SerializeObject(value));
            }
        }

        public static Printer CurrentPrinter
        {
            get
            {
                Printer Current = null;
                var serializedCurrentPrinter = Preferences.Default.Get<string>(CurrentPrinterKey, null);
                if (serializedCurrentPrinter != null)
                {
                    Current = JsonConvert.DeserializeObject<Printer>(serializedCurrentPrinter);
                }

                return Current;
            }
            set
            {
                Preferences.Default.Set(CurrentPrinterKey, JsonConvert.SerializeObject(value));
            }
        }



        public static List<int> IPadConfiguredPaymentID;



        public static double AutoLockDelay
        {
            get { return AppSettings.Get(AutoLockKey, 0.0); }
            set { AppSettings.Set(AutoLockKey, value); }
        }

        public static bool IsAppLocked
        {
            get { return AppSettings.Get(IsAppLockedKey, false); }
            set { AppSettings.Set(IsAppLockedKey, value); }
        }

        public static int NumberOfPrintCopy
        {
            get { return AppSettings.Get(NumberOfPrintKey, 1); }
            set { AppSettings.Set(NumberOfPrintKey, value); }
        }

        public static bool PrintDocketSecondReceipt
        {
            get { return AppSettings.Get(PrintDocketKey, false); }
            set { AppSettings.Set(PrintDocketKey, value); }
        }

        public static int PrintCustomerStartingNumber
        {
            get { return AppSettings.Get(StartingNumberKey, 1); }
            set { AppSettings.Set(StartingNumberKey, value); }
        }

        public static int PrintCustomerEndingNumber
        {
            get { return AppSettings.Get(EndingNumberKey, 100); }
            set { AppSettings.Set(EndingNumberKey, value); }
        }
        public static int PrintCustomerCurrentNumber
        {
            get { return AppSettings.Get(CurrentNumberKey, 1); }
            set { AppSettings.Set(CurrentNumberKey, value); }
        }
        public static bool PrintCustomerNumberReceipt
        {
            get { return AppSettings.Get(PrintCustomerNumberKey, false); }
            set { AppSettings.Set(PrintCustomerNumberKey, value); }
        }

        public static bool IsAutoLockActive
        {
            get { return AppSettings.Get(AutoLockActiveKey, false); }
            set { AppSettings.Set(AutoLockActiveKey, value); }
        }

        public static bool IsQRCodeEnable
        {
            get { return AppSettings.Get(QRCodeSelectedKey, true); }
            set { AppSettings.Set(QRCodeSelectedKey, value); }
        }

        public static bool IsTextPrint
        {
            get { return AppSettings.Get(nameof(IsTextPrint), false); }
            set { AppSettings.Set(nameof(IsTextPrint), value); }
        }

        public static bool IsInfoEnable
        {
            get { return AppSettings.Get(InfoSelectedKey, true); }
            set { AppSettings.Set(InfoSelectedKey, value); }
        }
        public static bool IsAllReceiptRegisterActive
        {
            get { return AppSettings.Get(AllReceiptRegisterActiveKey, false); }
            set { AppSettings.Set(AllReceiptRegisterActiveKey, value); }
        }

        #endregion


        //public static Dictionary<string, object> AccessTokenInput
        //{
        //	get
        //	{
        //		Dictionary<string, object> RenewAccessTokenInput = null;
        //		var serializedRenewAccessTokenInput = Preferences.Default.Get(RenewAccessTokenInputKey);
        //		if (serializedRenewAccessTokenInput != null)
        //		{
        //			RenewAccessTokenInput = JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedRenewAccessTokenInput);
        //		}

        //		return RenewAccessTokenInput;
        //	}
        //	set
        //	{
        //		Preferences.Default.Set(RenewAccessTokenInputKey, JsonConvert.SerializeObject(value));
        //	}
        //}

        //public static string PaypalToken
        //{
        //	get { return AppSettings.Get(PaypalTokenKey, PaypalTokenDefault); }
        //	set { AppSettings.Set(PaypalTokenKey, value); }
        //}

        //public static string TyroIntegrationkey
        //{
        //	get { return AppSettings.Get(TyroIntegrationkeyKey, TyroIntegrationkeyDefault); }
        //	set { AppSettings.Set(TyroIntegrationkeyKey, value); }
        //}





        public static DateTime LastLoadedHistoryDate
        {
            get { return AppSettings.Get(LastLoadedHistoryDateKey, DateTime.UtcNow); }
            set { AppSettings.Set(LastLoadedHistoryDateKey, value); }
        }

        public static ObservableCollection<string> GrantedPermissionNames
        {
            get
            {
                ObservableCollection<string> PermissionNames = new ObservableCollection<string>();
                var serializedData = Preferences.Default.Get<string>(GrantedPermissionKey, null);
                if (serializedData != null)
                {
                    PermissionNames = JsonConvert.DeserializeObject<ObservableCollection<string>>(serializedData);
                }

                return PermissionNames;
            }
            set
            {
                Preferences.Default.Set(GrantedPermissionKey, JsonConvert.SerializeObject(value));
            }
        }

        public static string EFTPOSAddress
        {
            get { return AppSettings.Get(nameof(EFTPOSAddress), string.Empty); }
            set { AppSettings.Set(nameof(EFTPOSAddress), value); }
        }


        //#35436 iOS- mx51 Suggested changes
        public static string SerialNumber
        {
            get { return AppSettings.Get(nameof(SerialNumber), string.Empty); }
            set { AppSettings.Set(nameof(SerialNumber), value); }
        }
        //#35436 iOS- mx51 Suggested changes

        public static string APEncKey
        {
            get { return AppSettings.Get<string>(nameof(APEncKey), null); }
            set { AppSettings.Set(nameof(APEncKey), value); }
        }


        public static string APHmacKey
        {
            get { return AppSettings.Get<string>(nameof(APHmacKey), null); }
            set { AppSettings.Set(nameof(APHmacKey), value); }
        }





        //#23024 iOS - Westpac Acquirer
        public static string AcquirerName
        {
            get { return AppSettings.Get(nameof(AcquirerName), string.Empty); }
            set { AppSettings.Set(nameof(AcquirerName), value); }
        }

        public static string AcquirerCode
        {
            get { return AppSettings.Get(nameof(AcquirerCode), string.Empty); }
            set { AppSettings.Set(nameof(AcquirerCode), value); }
        }

        //#23024 iOS - Westpac Acquirer


        public static VantivConfigurationDto Vantivsettings
        {
            get
            {
                VantivConfigurationDto vantivDetails = null;
                var serializedVantivDetails = Preferences.Default.Get<string>(VantivDetailsKey, null);
                if (serializedVantivDetails != null)
                {
                    vantivDetails = JsonConvert.DeserializeObject<VantivConfigurationDto>(serializedVantivDetails);
                }

                return vantivDetails;
            }
            set
            {
                Preferences.Default.Set(VantivDetailsKey, JsonConvert.SerializeObject(value));
            }
        }

        public static CloverConfigurationDto Cloversettings
        {
            get
            {
                CloverConfigurationDto cloverDetails = null;
                var serializedCloverDetails = Preferences.Default.Get<string>(CloverDetailsKey, null);
                if (serializedCloverDetails != null)
                {
                    cloverDetails = JsonConvert.DeserializeObject<CloverConfigurationDto>(serializedCloverDetails);
                }
                return cloverDetails;
            }
            set
            {
                Preferences.Default.Set(CloverDetailsKey, JsonConvert.SerializeObject(value));
            }
        }

        public static CastlesConfigurationDto Castlessettings
        {
            get
            {
                CastlesConfigurationDto cloverDetails = null;
                var serializedCastlesDetails = Preferences.Default.Get<string>(CastlesDetailsKey, null);
                if (serializedCastlesDetails != null)
                {
                    cloverDetails = JsonConvert.DeserializeObject<CastlesConfigurationDto>(serializedCastlesDetails);
                }
                return cloverDetails;
            }
            set
            {
                Preferences.Default.Set(CastlesDetailsKey, JsonConvert.SerializeObject(value));
            }
        }

        public static PaymentOptionDto TyroTapToPayConfiguration
        {
            get
            {
                PaymentOptionDto tyroTapToPayConfiguration = null;
                var serializedTyroTapToPay = Preferences.Default.Get<string>(TyroTapToPayKey, null);
                if (serializedTyroTapToPay != null)
                {
                    tyroTapToPayConfiguration = JsonConvert.DeserializeObject<PaymentOptionDto>(serializedTyroTapToPay);
                }
                return tyroTapToPayConfiguration;
            }
            set
            {
                Preferences.Default.Set(TyroTapToPayKey, JsonConvert.SerializeObject(value));
            }
        }

        //Ticket Start:#11403 How is variant display order by default setup in Hike App? by Rupesh
        public static int VariantOrderIndex
        {
            get { return AppSettings.Get(VariantOrderKey, VariantOrderIndexDefault); }
            set { AppSettings.Set(VariantOrderKey, value); }
        }
        //Ticket End by Rupesh

        //Ticket start:#22406 Quote sale.by rupesh
        public static bool IsQuoteSale
        {
            get { return AppSettings.Get(nameof(IsQuoteSale), false); }
            set { AppSettings.Set(nameof(IsQuoteSale), value); }

        }
        //Ticket end:#22406 .by rupesh
        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
        public static bool IsBackorderSaleSelected
        {
            get { return AppSettings.Get(nameof(IsBackorderSaleSelected), false); }
            set { AppSettings.Set(nameof(IsBackorderSaleSelected), value); }
        }
        //Ticket end:#92764.by rupesh
        //Ticket start.#24753 iOS - Product Barcode Missing in Receipts.by rupesh
        public static bool IsShowItemBarCode { get; set; }
        //Ticket end.#24753 .by rupesh

        //public static bool IsCustomerAppActive { get; set; } = false;


        public static bool IsCustomerAppActive
        {
            get { return AppSettings.Get(nameof(IsCustomerAppActive), false); }
            set { AppSettings.Set(nameof(IsCustomerAppActive), value); }

        }


        public static string StoreLogo
        {
            get { return AppSettings.Get(nameof(StoreLogo), string.Empty); }
            set { AppSettings.Set(nameof(StoreLogo), value); }

        }


        //#30495 iOS -Change in Register API for display app option
        //public static Dictionary<string, string> DisplayAppConfig;


        public static string CustomerAppConfigFrom
        {
            get { return AppSettings.Get<string>(nameof(CustomerAppConfigFrom), null); }
            set { AppSettings.Set(nameof(CustomerAppConfigFrom), value); }

        }

        public static string CustomerAppPin
        {
            get { return AppSettings.Get<string>(nameof(CustomerAppPin), null); }
            set { AppSettings.Set(nameof(CustomerAppPin), value); }

        }


        //#30495 iOS -Change in Register API for display app option


        public static Tax DefaultTax { get; set; }


        //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan



        public static ShopFeature ShopFeatures
        {
            get
            {
                ShopFeature shopFeatures = new ShopFeature();
                var serializedData = Preferences.Default.Get<string>(GrantedFeatureKey, null);
                if (serializedData != null)
                {
                    shopFeatures = JsonConvert.DeserializeObject<ShopFeature>(serializedData);
                }

                return shopFeatures;
            }
            set
            {
                Preferences.Default.Set(GrantedFeatureKey, JsonConvert.SerializeObject(value));
            }
        }
        //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan
        public static string CurrentDatabaseName
        {
            get { return AppSettings.Get<string>(nameof(CurrentDatabaseNameKey), null); }
            set { AppSettings.Set(nameof(CurrentDatabaseNameKey), value); }

        }

        public static int CurrentDatabaseType
        {
            get { return AppSettings.Get<int>(nameof(CurrentDatabaseTypeKey), 0); }
            set { AppSettings.Set(nameof(CurrentDatabaseTypeKey), value); }

        }

        //TyroTapToPay
        const string TyroTapToPayAccessTokenKey = "TyroTapToPayAccessToken_Key";
        public static string TyroTapToPayAccessToken
        {
            get { return AppSettings.Get(TyroTapToPayAccessTokenKey, AccessTokenDefault); }
            set { AppSettings.Set(TyroTapToPayAccessTokenKey, value); }
        }
        const string TyroTapToPayConnectionSecretKey = "TyroTapToPayConnectionSecret_Key";
        public static string TyroTapToPayConnectionSecret
        {
            get { return AppSettings.Get(TyroTapToPayConnectionSecretKey,string.Empty); }
            set { AppSettings.Set(TyroTapToPayConnectionSecretKey, value); }
        }
        const string TyroTapToPayRefundPasscodeApprovedBy_Key = "TyroTapToPayRefundPasscodeApprovedBy_Key";
        public static string TyroTapToPayRefundPasscodeApprovedBy
        {
            get { return AppSettings.Get(TyroTapToPayRefundPasscodeApprovedBy_Key,string.Empty); }
            set { AppSettings.Set(TyroTapToPayRefundPasscodeApprovedBy_Key, value); }
        }
        const string HikePayStoreKey = "HikePayStoreId_Key";
        public static string HikePayStoreId
        {
            get { return AppSettings.Get(HikePayStoreKey, string.Empty); }
            set { AppSettings.Set(HikePayStoreKey, value); }
        }

        const string HikePayVerifyKey = "HikePayVerify_Key";
        public static HikePayVerify HikePayVerify
        {
            get
            {
                var serializedData = Preferences.Default.Get<string>(HikePayVerifyKey, null);
                if (serializedData != null)
                {
                   return JsonConvert.DeserializeObject<HikePayVerify>(serializedData);
                }

                return null;
            }
            set
            {
                Preferences.Default.Set(HikePayVerifyKey, JsonConvert.SerializeObject(value));
            }
        }
        const string TerminalIdKey = "TerminalId_Key";
        public static string TerminalId
        {
            get { return AppSettings.Get(TerminalIdKey, string.Empty); }
            set { AppSettings.Set(TerminalIdKey, value); }
        }

    }
}