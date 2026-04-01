using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Fusillade;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Model;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using HikePOS.Services.Payment;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;


namespace HikePOS.ViewModels
{
    public class PaymentViewModel : BaseViewModel
    {
        #region Services object

        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();

        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;

        ApiService<IGiftCardApi> giftCardApiService = new ApiService<IGiftCardApi>();
        GiftCardServices giftCardService;

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        public SaleServices saleService;

        ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
        public OutletServices outletService;

        ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
        public ProductServices productService; //= new ProductServices(productApiService);

        ApiService<ITaxApi> taxApiService = new ApiService<ITaxApi>();
        TaxServices taxServices;

        ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        PaymentServices paymentService;

        ApiService<ISubmitLogApi> logApiService = new ApiService<ISubmitLogApi>();
        SubmitLogServices logService;

        INadaTapToPay nadaTapToPay;
        INadaPayTerminalAppService HikeTerminalPay;

        #endregion

        #region Pages

        PromptPopupPage promptPopupPage;
        ApproveAdminPage ApproveAdminPage;
        #endregion

        #region Poperties

        bool IsBackToPOS = false; //Start #92959 By Pratik
        Dictionary<string, string> logRequestDetails = new Dictionary<string, string>();

        private string _customerEMailId;
        public string CustomerEMailId
        {
            get
            {
                return _customerEMailId;
            }
            set
            {
                _customerEMailId = value;
                SetPropertyChanged(nameof(CustomerEMailId));
            }
        }

          
        private int _paymentDetailListHeight;
        public int PaymentDetailListHeight
        {
            get
            {
                return _paymentDetailListHeight;
            }
            set
            {
                _paymentDetailListHeight = value;
                SetPropertyChanged(nameof(PaymentDetailListHeight));
            }
        }

        private int _paymentTagsListHeight;
        public int PaymentTagsListHeight
        {
            get
            {
                return _paymentTagsListHeight;
            }
            set
            {
                _paymentTagsListHeight = value;
                SetPropertyChanged(nameof(PaymentTagsListHeight));
            }
        }

        private string refundAmount;
        public string RefundAmountText
        {
            get
            {
                return refundAmount;
            }
            set
            {
                refundAmount = value;
                IsRefundAmountText = !string.IsNullOrEmpty(value);
                SetPropertyChanged(nameof(RefundAmountText));
            }
        }

        private bool _isEmailWithPayLink;
        public bool IsEmailWithPayLink
        {
            get
            {
                return _isEmailWithPayLink;
            }
            set
            {
                _isEmailWithPayLink = value;
                SetPropertyChanged(nameof(IsEmailWithPayLink));
            }
        }

        private bool _isEmailWithPayLinkVisible;
        public bool IsEmailWithPayLinkVisible
        {
            get
            {
                return _isEmailWithPayLinkVisible;
            }
            set
            {
                _isEmailWithPayLinkVisible = value;
                SetPropertyChanged(nameof(IsEmailWithPayLinkVisible));
            }
        }

        private bool _ispaymentListvisible;
        public bool IspaymentListvisible
        {
            get
            {
                return _ispaymentListvisible;
            }
            set
            {
                _ispaymentListvisible = value;
                SetPropertyChanged(nameof(IspaymentListvisible));
            }
        }

        private bool _ispaymentTagsListvisible;
        public bool IspaymentTagsListvisible
        {
            get
            {
                return _ispaymentTagsListvisible;
            }
            set
            {
                _ispaymentTagsListvisible = value;
                SetPropertyChanged(nameof(IspaymentTagsListvisible));
            }
        }

        private bool isRefundAmountText;
        public bool IsRefundAmountText
        {
            get
            {
                return isRefundAmountText;
            }
            set
            {
                isRefundAmountText = value;
                SetPropertyChanged(nameof(IsRefundAmountText));
            }
        }

        private int _IsOpenBackground;
        public int IsOpenBackground { get { return _IsOpenBackground; } set { _IsOpenBackground = value; SetPropertyChanged(nameof(IsOpenBackground)); } }

        private int _IsOpenMainBackground;
        public int IsOpenMainBackground { get { return _IsOpenMainBackground; } set { _IsOpenMainBackground = value; SetPropertyChanged(nameof(IsOpenMainBackground)); } }

        private Color _paymentButtonTextColor;
        public Color PaymentButtonTextColor { get { return _paymentButtonTextColor; } set { _paymentButtonTextColor = value; SetPropertyChanged(nameof(PaymentButtonTextColor)); } }

        private ObservableCollection<InvoiceLineItemDto> _invoiceLineItems;
        public ObservableCollection<InvoiceLineItemDto> InvoiceLineItems
        {
            get
            {
                return _invoiceLineItems;
            }
            set
            {
                _invoiceLineItems = value;
                SetPropertyChanged(nameof(InvoiceLineItems));
            }
        }

        private string _TaxLabelName;
        public string TaxLabelName { get { return _TaxLabelName; } set { _TaxLabelName = value; SetPropertyChanged(nameof(TaxLabelName)); } }

        private InvoiceDto _Invoice;
        public InvoiceDto Invoice
        {
            get { return _Invoice; }
            set
            {
                _Invoice = value;
                SetPropertyChanged(nameof(Invoice));

                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                if (Invoice != null)
                {
                    Invoice.AmountChanged += ((sender, e) =>
                    {
                        foreach (var item in PaymentOptionList.Where(a => a.Surcharge != null && a.PaymentOptionType != PaymentOptionType.Cash))
                        {
                            if (Invoice.Status == InvoiceStatus.Refunded || (Invoice.Status == InvoiceStatus.Exchange && Invoice.TotalPay < 0))
                            {
                                if (item.CanApplySurchageOnRefund)
                                {
                                    var surcharge = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == item.Id && a.InvoicePaymentDetails != null && a.InvoicePaymentDetails.Count > 0)
                                    .Sum(a => a.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount)
                                    .Sum(a => Convert.ToDecimal(a.Value ?? "0").ToPositive())) + Invoice.InvoicePayments.Where(a => a.PaymentOptionId == item.Id && a.InvoicePaymentDetails != null && a.InvoicePaymentDetails.Count > 0)
                                       .Sum(a => a.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount)
                                       .Sum(a => Convert.ToDecimal(a.Value ?? "0")));

                                    var totaltender = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == item.Id)
                                    .Sum(a => a.Amount) + Invoice.InvoicePayments.Where(a => a.PaymentOptionId == item.Id)
                                    .Sum(a => a.Amount) - surcharge;
                                    if (totaltender.ToPositive() > 0)
                                    {
                                        var percentage = Math.Round(((surcharge * 100) / totaltender), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                                        if (e.ToPositive() <= totaltender)
                                            item.DisplaySurcharge = Math.Round(((e * percentage) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                                        else
                                            item.DisplaySurcharge = Math.Round(((totaltender * percentage) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                                    }
                                    else
                                        item.DisplaySurcharge = null;

                                }
                                else
                                {
                                    item.DisplaySurcharge = null;
                                }

                            }
                            else
                            {
                                if (e.ToPositive() <= (Invoice.NetAmount - Invoice.TotalPaid))
                                    item.DisplaySurcharge = Math.Round(((e * item.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                                else
                                    item.DisplaySurcharge = Math.Round((((Invoice.NetAmount - Invoice.TotalPaid) * item.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                            }
                            item.DisplayName = getCardName(item.Name);
                            item.DisplaySubName = item.DisplaySurcharge.HasValue ? (item.DisplaySurcharge.Value.ToPositive() > 0 ? "Extra " + item.DisplaySurcharge.Value.ToString("C") : "") : "";
                        }
                        //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
                        foreach (var item in PaymentOptionList.Where(a => a.PaymentOptionType == PaymentOptionType.Cash))
                        {
                            var amount = CashPaymentRoundingAmount();
                            item.DisplaySubName = Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent ? amount.ToString("C") : "";

                        }
                        //Ticket end:#91262 .by rupesh
                    });

                }
                //end ticket #73190 
            }
        }

        private InvoiceDto _InvoiceBackOrder;
        public InvoiceDto InvoiceBackOrder { get { return _InvoiceBackOrder; } set { _InvoiceBackOrder = value; SetPropertyChanged(nameof(InvoiceBackOrder)); } }

        private CustomerViewModel _CustomerModel;
        public CustomerViewModel CustomerModel
        {
            get
            {
                return _CustomerModel;
            }
            set
            {
                _CustomerModel = value;
                SetPropertyChanged(nameof(CustomerModel));
            }
        }

        private ImageSource _BarcodeImage;
        public ImageSource BarcodeImage { get { return _BarcodeImage; } set { _BarcodeImage = value; SetPropertyChanged(nameof(BarcodeImage)); } }

        private ImageSource _QRCodeImage;
        public ImageSource QRCodeImage { get { return _QRCodeImage; } set { _QRCodeImage = value; SetPropertyChanged(nameof(QRCodeImage)); } }

        private OutletDto_POS _currentOutlet;
        public OutletDto_POS CurrentOutlet { get { return _currentOutlet; } set { _currentOutlet = value; SetPropertyChanged(nameof(CurrentOutlet)); } }

        private RegisterDto _currentRegister;
        public RegisterDto CurrentRegister { get { return _currentRegister; } set { _currentRegister = value; SetPropertyChanged(nameof(CurrentRegister)); } }

        private string _customerPrintNo;
        public string CustomerPrintNo { get { return _customerPrintNo; } set { _customerPrintNo = value; SetPropertyChanged(nameof(CustomerPrintNo)); } }

        private decimal _loyaltyRedeemed;
        public decimal LoyaltyRedeemed { get { return _loyaltyRedeemed; } set { _loyaltyRedeemed = value; SetPropertyChanged(nameof(LoyaltyRedeemed)); } }

        private decimal _closingbalance;
        public decimal Closingbalance { get { return _closingbalance; } set { _closingbalance = value; SetPropertyChanged(nameof(Closingbalance)); } }

        private ObservableCollection<OfferDto> _offers;
        public ObservableCollection<OfferDto> offers { get { return _offers; } set { _offers = value; SetPropertyChanged(nameof(offers)); } }

        private bool _IsAddPaymentActive = true;
        public bool IsAddPaymentActive { get { return _IsAddPaymentActive; } set { _IsAddPaymentActive = value; SetPropertyChanged(nameof(IsAddPaymentActive)); } }

        private bool _IsSuccessPaymentActive;
        public bool IsSuccessPaymentActive
        {
            get { return _IsSuccessPaymentActive; }
            set
            {
                _IsSuccessPaymentActive = value;
                SetPropertyChanged(nameof(IsSuccessPaymentActive));
                CheckOutViewModel.IsSaleSucceess = IsSuccessPaymentActive;
                IsDeliveryPrint = false;
                if (IsSuccessPaymentActive)
                {
                    var result = (Invoice.CustomerName == "Cash sale" || Invoice.CustomerName == "Walk in" || Invoice.Status == InvoiceStatus.Voided || Invoice.Status == InvoiceStatus.Quote);
                    IsDeliveryPrint = !result;
                }
                if (IsSuccessPaymentActive)
                    IsEmailWithPayLinkVisible = Invoice != null && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.AllowOnAccountPaymentHikePay && Invoice.Status == InvoiceStatus.OnAccount;
                else
                    IsEmailWithPayLinkVisible = false;
            }
        }

        private bool _isToPrintDeliveryDocket;
        public bool IsToPrintDeliveryDocket { get { return _isToPrintDeliveryDocket; } set { _isToPrintDeliveryDocket = value; SetPropertyChanged(nameof(IsToPrintDeliveryDocket)); } }

        private bool _isToPrintInvoiceReceipt;
        public bool IsToPrintInvoiceReceipt { get { return _isToPrintInvoiceReceipt; } set { _isToPrintInvoiceReceipt = value; SetPropertyChanged(nameof(IsToPrintInvoiceReceipt)); } }

        private bool _isDeliveryPrint;
        public bool IsDeliveryPrint
        {
            get { return _isDeliveryPrint; }
            set
            {
                _isDeliveryPrint = value;
                SetPropertyChanged(nameof(IsDeliveryPrint));
            }
        }

        private bool _EnableSendEmail;
        public bool EnableSendEmail
        {
            get { return _EnableSendEmail; }
            set
            {
                if (Invoice != null && Invoice.Id > 0)
                    _EnableSendEmail = true;
                else
                    _EnableSendEmail = false;
                SetPropertyChanged(nameof(EnableSendEmail));
            }
        }

        private ObservableCollection<PaymentOptionDto> _PaymentOptionList;
        public ObservableCollection<PaymentOptionDto> PaymentOptionList
        {
            get { return _PaymentOptionList; }
            set
            {
                _PaymentOptionList = value;
                SetPropertyChanged(nameof(PaymentOptionList));
            }
        }

        private ReceiptTemplateDto _currentReceiptTemplate;
        public ReceiptTemplateDto CurrentReceiptTemplate
        {
            get
            {
                return _currentReceiptTemplate;
            }
            set
            {
                _currentReceiptTemplate = value;
                SetPropertyChanged(nameof(CurrentReceiptTemplate));
            }
        }

        private Color _onAccountButtonTextColor;
        public Color OnAccountButtonTextColor { get { return _onAccountButtonTextColor; } set { _onAccountButtonTextColor = value; SetPropertyChanged(nameof(OnAccountButtonTextColor)); } }

        private int _PaymentListHeight = 180;
        public int PaymentListHeight { get { return _PaymentListHeight; } set { _PaymentListHeight = value; SetPropertyChanged(nameof(PaymentListHeight)); } }

        public IEnumerable<PaymentOptionDto> AllPaymentOptionList { get; set; }

        private string _onAccountText = "On Account";
        public string OnAccountText { get { return _onAccountText; } set { _onAccountText = value; SetPropertyChanged(nameof(OnAccountText)); } }

        private string payment_GiftCardNumber;
        public string Payment_GiftCardNumber { get { return payment_GiftCardNumber; } set { payment_GiftCardNumber = value; SetPropertyChanged(nameof(Payment_GiftCardNumber)); } }

        private bool _IsPayment_GiftCardVisible;
        public bool IsPayment_GiftCardVisible { get { return _IsPayment_GiftCardVisible; } set { _IsPayment_GiftCardVisible = value; SetPropertyChanged(nameof(IsPayment_GiftCardVisible)); } }

        private bool _IsPayment_OnAccountEnable;
        public bool IsPayment_OnAccountEnable { get { return _IsPayment_OnAccountEnable; } set { _IsPayment_OnAccountEnable = value; SetPropertyChanged(nameof(IsPayment_OnAccountEnable)); } }

        private bool _IsPayment_OnAccountVisible;
        public bool IsPayment_OnAccountVisible { get { return _IsPayment_OnAccountVisible; } set { _IsPayment_OnAccountVisible = value; SetPropertyChanged(nameof(IsPayment_OnAccountVisible)); } }

        private bool _IsPayment_LaybyEnable;
        public bool IsPayment_LaybyEnable { get { return _IsPayment_LaybyEnable; } set { _IsPayment_LaybyEnable = value; SetPropertyChanged(nameof(IsPayment_LaybyEnable)); } }

        private bool _IsPayment_LaybyVisible;
        public bool IsPayment_LaybyVisible { get { return _IsPayment_LaybyVisible; } set { _IsPayment_LaybyVisible = value; SetPropertyChanged(nameof(IsPayment_LaybyVisible)); } }

        private bool _IsPayment_PartialyPaid;
        public bool IsPayment_PartialyPaid
        {
            get
            {
                return Settings.IsRestaurantPOS ? false : _IsPayment_PartialyPaid; //#94565
            }
            set
            {
                //Ticket #9511 Start: Partial Refund issue. By Nikhil.
                if (Invoice?.Status == InvoiceStatus.Refunded)
                    value = false;
                //Ticket #9511 End:By Nikhil. 
                _IsPayment_PartialyPaid = value;
                SetPropertyChanged(nameof(IsPayment_PartialyPaid));
            }
        }

        private bool _IsPrinterAvailable;
        public bool IsPrinterAvailable { get { return _IsPrinterAvailable; } set { _IsPrinterAvailable = value; SetPropertyChanged(nameof(IsPrinterAvailable)); } }

        private bool _IsCloudPrintActive;
        public bool IsCloudPrintActive { get { return _IsCloudPrintActive; } set { _IsCloudPrintActive = value; SetPropertyChanged(nameof(IsCloudPrintActive)); } }

        private GeneralRuleDto _shopGeneralRule = Settings.StoreGeneralRule;
        public GeneralRuleDto ShopGeneralRule { get { return _shopGeneralRule; } set { _shopGeneralRule = value; SetPropertyChanged(nameof(ShopGeneralRule)); } }

        private ShopDto _GeneralShopDto;
        public ShopDto GeneralShopDto { get { return _GeneralShopDto; } set { _GeneralShopDto = value; SetPropertyChanged(nameof(GeneralShopDto)); } }

        private string _giftCardBalance;
        public string GiftCardBalance { get { return _giftCardBalance; } set { _giftCardBalance = value; SetPropertyChanged(nameof(GiftCardBalance)); } }

        private bool _IsGiftCardAnable;
        public bool IsGiftCardAnable { get { return _IsGiftCardAnable; } set { _IsGiftCardAnable = value; SetPropertyChanged(nameof(IsGiftCardAnable)); } }

        private SubscriptionDto _subscription;
        public SubscriptionDto Subscription { get { return _subscription; } set { _subscription = value; SetPropertyChanged(nameof(Subscription)); } }

        private UserListDto _currentUser;
        public UserListDto CurrentUser { get { return _currentUser; } set { _currentUser = value; SetPropertyChanged(nameof(CurrentUser)); } }

        private string _receiptInfo;
        public string ReceiptInfo { get { return _receiptInfo; } set { _receiptInfo = value; SetPropertyChanged(nameof(ReceiptInfo)); } }

        private bool _isParkSale;
        public bool IsParkSale { get { return _isParkSale; } set { _isParkSale = value; SetPropertyChanged(nameof(IsParkSale)); } }

        #region property related to windcave button view display

        private bool _isWindcaveButtonViewDisplay;
        public bool IsWindcaveButtonViewDisplay
        {
            get
            {
                return _isWindcaveButtonViewDisplay;
            }
            set
            {
                _isWindcaveButtonViewDisplay = value;
                SetPropertyChanged(nameof(IsWindcaveButtonViewDisplay));
            }
        }

        private bool _isFirstWindcaveButtonVisible;
        public bool IsFirstWindcaveButtonVisible
        {
            get
            {
                return _isFirstWindcaveButtonVisible;
            }
            set
            {
                _isFirstWindcaveButtonVisible = value;
                SetPropertyChanged(nameof(IsFirstWindcaveButtonVisible));
            }
        }

        private bool _isSecondWindcaveButtonVisible;
        public bool IsSecondWindcaveButtonVisible
        {
            get
            {
                return _isSecondWindcaveButtonVisible;
            }
            set
            {
                _isSecondWindcaveButtonVisible = value;
                SetPropertyChanged(nameof(IsSecondWindcaveButtonVisible));
            }
        }

        private string _firstButtonText;
        public string FirstButtonText { get { return _firstButtonText; } set { _firstButtonText = value; SetPropertyChanged(nameof(FirstButtonText)); } }

        private string _secondButtonText;
        public string SecondButtonText { get { return _secondButtonText; } set { _secondButtonText = value; SetPropertyChanged(nameof(SecondButtonText)); } }

        private string _windcavePopupText = string.Empty;
        public string WindcavePopupText { get { return _windcavePopupText; } set { _windcavePopupText = value; SetPropertyChanged(nameof(WindcavePopupText)); } }


        public ICommand WindcaveButtonSelectedOption { get; }


        public ICommand RemovePaymentCommand { get; }

        #endregion

        private bool _isQuoteSale = Settings.IsQuoteSale;
        public bool IsQuoteSale
        {
            get
            {
                return _isQuoteSale;
            }
            set
            {
                _isQuoteSale = value;
                SetPropertyChanged(nameof(IsQuoteSale));
            }
        }

        static bool _isItemCountVisible { get; set; } = Settings.StoreGeneralRule.ShowTotalQuantityOfItemsInBasket;
        public bool IsItemCountVisible { get { return _isItemCountVisible; } set { _isItemCountVisible = value; SetPropertyChanged(nameof(IsItemCountVisible)); } }

        public List<string> AssemblyeConduitTempReceiptList;


        public List<string> VantivCloudReceiptList;

        private ObservableCollection<ProductTagDto> _paymentTags;
        public ObservableCollection<ProductTagDto> PaymentTags { get { return _paymentTags; } set { _paymentTags = value; SetPropertyChanged(nameof(PaymentTags)); } }

        // Start #90942 iOS:FR Cheque number for sale by pratik
        public string ReferenceID { get; set; }
        // End #90942 by pratik

        public string POReferenceID { get; set; } //Start #91991 By Pratik

        private bool _IsToPrintGiftReceipt;
        public bool IsToPrintGiftReceipt { get { return _IsToPrintGiftReceipt; } set { _IsToPrintGiftReceipt = value; SetPropertyChanged(nameof(IsToPrintGiftReceipt)); } }

        private string _customerName;
        public string CustomerName
        {
            get
            {
                return _customerName;
            }
            set
            {
                _customerName = value;
                SetPropertyChanged(nameof(CustomerName));
            }
        }

        PaymentOptionDto SelectedPaymentOption = null;
        private List<string> tempassempblyReceipt = null;
        private List<string> tempVantivCloudReceipt = null;

        private double saleItemListHeight;


        public double SaleItemListHeight
        {
            get { return saleItemListHeight; }
            set
            {
                saleItemListHeight = value;
                SetPropertyChanged(nameof(SaleItemListHeight));
            }
        }

        private double giftPrintPrintItemListHeight;
        public double GiftPrintPrintItemListHeight
        {
            get { return giftPrintPrintItemListHeight; }
            set
            {
                giftPrintPrintItemListHeight = value;
                SetPropertyChanged(nameof(GiftPrintPrintItemListHeight));
            }
        }

        private string _GiftCardAfterPayButtonText;
        public string GiftCardAfterPayButtonText
        {
            get
            {
                return _GiftCardAfterPayButtonText;
            }
            set
            {
                _GiftCardAfterPayButtonText = value;
                SetPropertyChanged(nameof(GiftCardAfterPayButtonText));
            }
        }

        private string _GiftcardLabelText;
        public string GiftcardLabelText
        {
            get
            {
                return _GiftcardLabelText;
            }
            set
            {
                _GiftcardLabelText = value;
                SetPropertyChanged(nameof(GiftcardLabelText));
            }
        }

        private string _GiftCardUserEntryHint;
        public string GiftCardUserEntryHint
        {
            get
            {
                return _GiftCardUserEntryHint;
            }
            set
            {
                _GiftCardUserEntryHint = value;
                SetPropertyChanged(nameof(GiftCardUserEntryHint));
            }
        }

        private bool _IsGiftCardAfterPayButtonEnabled;
        public bool IsGiftCardAfterPayButtonEnabled
        {
            get
            {
                return _IsGiftCardAfterPayButtonEnabled;
            }
            set
            {
                _IsGiftCardAfterPayButtonEnabled = value;
                SetPropertyChanged(nameof(IsGiftCardAfterPayButtonEnabled));
            }
        }

        private bool _IsLblTipEnabled;
        public bool IsLblTipEnabled
        {
            get
            {
                return _IsLblTipEnabled;
            }
            set
            {
                _IsLblTipEnabled = value;
                SetPropertyChanged(nameof(IsLblTipEnabled));
            }
        }

        private bool _IsStrTenderAmountEntryEnabled;
        public bool IsStrTenderAmountEntryEnabled
        {
            get
            {
                return _IsStrTenderAmountEntryEnabled;
            }
            set
            {
                _IsStrTenderAmountEntryEnabled = value;
                SetPropertyChanged(nameof(IsStrTenderAmountEntryEnabled));
            }
        }

        private bool _IsBackorderDepositEntryEnabled;
        public bool IsBackorderDepositEntryEnabled
        {
            get
            {
                return _IsBackorderDepositEntryEnabled;
            }
            set
            {
                _IsBackorderDepositEntryEnabled = value;
                SetPropertyChanged(nameof(IsBackorderDepositEntryEnabled));
            }
        }

        private bool _IsGiftCardUserEntryEnabled;
        public bool IsGiftCardUserEntryEnabled
        {
            get
            {
                return _IsGiftCardUserEntryEnabled;
            }
            set
            {
                _IsGiftCardUserEntryEnabled = value;
                SetPropertyChanged(nameof(IsGiftCardUserEntryEnabled));
            }
        }

        private bool _IsTxtEmailEnabled;
        public bool IsTxtEmailEnabled
        {
            get
            {
                return _IsTxtEmailEnabled;
            }
            set
            {
                _IsTxtEmailEnabled = value;
                SetPropertyChanged(nameof(IsTxtEmailEnabled));
            }
        }

        private bool _IsBtnEditCustomerVisible;
        public bool IsBtnEditCustomerVisible
        {
            get
            {
                return _IsBtnEditCustomerVisible;
            }
            set
            {
                _IsBtnEditCustomerVisible = value;
                SetPropertyChanged(nameof(IsBtnEditCustomerVisible));
            }
        }

        private bool _IsBtnRemoveCustomerVisible;
        public bool IsBtnRemoveCustomerVisible
        {
            get
            {
                return _IsBtnRemoveCustomerVisible;
            }
            set
            {
                _IsBtnRemoveCustomerVisible = value;
                SetPropertyChanged(nameof(IsBtnRemoveCustomerVisible));
            }
        }

        private bool _IsBtnDeliveryCustomerVisible;
        public bool IsBtnDeliveryCustomerVisible
        {
            get
            {
                return _IsBtnDeliveryCustomerVisible;
            }
            set
            {
                _IsBtnDeliveryCustomerVisible = value;
                SetPropertyChanged(nameof(IsBtnDeliveryCustomerVisible));
            }
        }

        private bool _IsQuickCashVisible;
        public bool IsQuickCashVisible
        {
            get
            {
                return _IsQuickCashVisible;
            }
            set
            {
                _IsQuickCashVisible = value;
                SetPropertyChanged(nameof(IsQuickCashVisible));
            }
        }


        #endregion

        #region Command
        public ICommand ParkCommand { get; }
        public ICommand ToPayCommand { get; }
        public ICommand EmailCommand { get; }
        public ICommand PaymentToolMenuCommand { get; }
        public ICommand QuickCashOptionCommand { get; }

        public ICommand AddNewSaleHandleCommand => new Command(addNewSaleHandle_Clicked);
        public ICommand BackPageCommand => new Command(BackHandle_Clicked);
        public ICommand TenderAmountHandleCommand => new Command<Entry>(tenderAmountHandle_Unfocused);
        public ICommand selectPaymentHandle_Command => new Command<PaymentOptionDto>(selectPaymentHandle_Clicked);
        public ICommand OnAccountChargeHandleCommand => new Command(onAccountChargeHandle_Clicked);
        public ICommand LayByHandleCommand => new Command(layByHandle_Clicked);
        public ICommand BackgroundHandleCommand => new Command(backgroundHandle_Tapped);
        public ICommand GiftCardHandleCommand => new Command<Entry>(giftCardHandle_TextChanged);
        public ICommand BackorderDepositHandleCommand => new Command<Entry>(backorderDepositHandle_TextChanged);
        public ICommand SelectPaymentTagHandleCommand => new Command<ProductTagDto>(selectPaymentTagHandle_Clicked);
        public ICommand GiftCardHandleFocusedCommand => new Command(giftCardHandle_Focused);
        public ICommand GiftCardHandleUnFocusedCommand => new Command(giftCardHandle_Unfocused);
        #endregion


        #region Page Load
        public PaymentViewModel()
        {
            AssemblyeConduitTempReceiptList = new List<string>();
            VantivCloudReceiptList = new List<string>();
            AssemblyeConduitTempReceiptList.Clear();

            OnAccountButtonTextColor = Colors.White;
            PaymentButtonTextColor = Colors.Red;
            RemovePaymentCommand = new Command<InvoicePaymentDto>(RemovePayment);

            customerService = new CustomerServices(customerApiService);
            giftCardService = new GiftCardServices(giftCardApiService);
            saleService = new SaleServices(saleApiService);
            outletService = new OutletServices(outletApiService);
            productService = new ProductServices(productApiService);
            taxServices = new TaxServices(taxApiService);
            logService = new SubmitLogServices(logApiService);
            paymentService = new PaymentServices(paymentApiService);
            nadaTapToPay = DependencyService.Get<INadaTapToPay>();
            //HikeTerminalPay = DependencyService.Get<INadaPayTerminalRemoteAppService>();

            ParkCommand = new Command(async () =>
            {
                await ParkSale();
            });
            ToPayCommand = new Command(() =>
            {
                Invoice.StrTenderAmount = Invoice.TotalPay.ToString("C");
            });
            PaymentToolMenuCommand = new Command<string>(async (obj) =>
            {
                await PaymentToolMenu(obj);
            });

            EmailCommand = new Command(EmailSend);
            QuickCashOptionCommand = new Command<decimal>(QuickCashOption);

            if (CustomerModel == null)
            {
                CustomerModel = new CustomerViewModel(customerService, saleService);
                CustomerModel.CustomerChanged += async (object sender, CustomerDto_POS customer) =>
                {
                    await Task.Run(async () =>
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            try
                            {
                                if (Invoice == null)
                                {
                                    return;
                                }
                                Invoice = await InvoiceCalculations.CustomerOnSelectAsync(customer, Invoice, offers, productService, taxServices);

                                updatePaymentList();

                                if (!(customer.Id == 0 && string.IsNullOrEmpty(customer.TempId)) && Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer && !Settings.IsQuoteSale)
                                {
                                    try
                                    {

                                        PropertyInfo myPropInfo;
                                        bool StorePermissionResult = false;

                                        myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeCustomerDeliverAddressFeature");


                                        bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out StorePermissionResult);

                                        if (!StorePermissionResult)
                                        {

                                            return;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine(ex.Message);
                                    }
                                    var result = await App.Alert.ShowAlert(LanguageExtension.Localize("DeliveryAddressAlertTitle"), LanguageExtension.Localize("DeliveryAddressAlertMessage"), LanguageExtension.Localize("YesButtonText"), LanguageExtension.Localize("CancelButtonText"));
                                    if (result)
                                    {
                                        CustomerModel.DeliveryCustomerCommand.Execute(Invoice.CustomerDetail);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.Track();
                            }
                        });
                    });
                };

                CustomerModel.DeliveryAddressChanged += (object sender, CustomerAddressDto e) =>
                {
                    try
                    {
                        if (Invoice == null)
                        {
                            return;
                        }
                        Invoice.DeliveryAddressId = e.Id;
                        Invoice.DeliveryAddress = e;
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                    }

                };
            }
        }


      
        #endregion

        #region Methods
        async void RemovePayment(InvoicePaymentDto paymentdto)
        {

            Debug.WriteLine("Before remove: " + Newtonsoft.Json.JsonConvert.SerializeObject(Invoice).ToString());


            if (paymentdto.PaymentOptionType == PaymentOptionType.Credit)
                paymentdto.IsDeletePaymentActive = true;

            if (!paymentdto.IsDeletePaymentActive || IsSuccessPaymentActive)
                return;


            var decline = await App.Alert.ShowAlert("ARE YOU SURE?", LanguageExtension.Localize("RemovePaymentInCheckOut"), "Yes", "No");
            if (!decline)
                return;

            Invoice.InvoicePayments.Remove(paymentdto);

            Invoice.TotalPaid = Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);
            Invoice.TotalTender = Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);
            Invoice.ChangeAmount = 0;

            Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);

            var count = Invoice.InvoicePayments.Count();

            if (count > 0)
                IspaymentListvisible = true;
            else
                IspaymentListvisible = false;

            if (Invoice.NetAmount > Invoice.TotalPay)
            {
                IsPayment_PartialyPaid = true;
                if (count < 3)
                {
                    PaymentDetailListHeight = PaymentDetailListHeight - 40;
                }
            }
            else
            {
                IsPayment_PartialyPaid = false;
                PaymentDetailListHeight = 0;
            }
            Debug.WriteLine("After remove: " + Newtonsoft.Json.JsonConvert.SerializeObject(Invoice).ToString());
        }

        private async void paypalpage_PaymentSuccessed(Message<PaypalPaymentResult> paypalresponse)
        {
            if (paypalresponse == null)
            {
                SendLogsToServer(SelectedPaymentOption, null, paypalresponse);
                return;
            }

            SendLogsToServer(SelectedPaymentOption, null, paypalresponse);
            if (paypalresponse.Type == MessageType.Success)
            {
                if (SelectedPaymentOption != null && SelectedPaymentOption.PaymentOptionType == PaymentOptionType.PayPal)
                {
                    ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();
                    InvoicePaymentDetails.Add(new InvoicePaymentDetailDto
                    {
                        Key = InvoicePaymentKey.PaypalResponse,
                        Value = paypalresponse.Result.ToJson()
                    });


                    var paypal_result = await addIntegratedPaymentToSell(SelectedPaymentOption, paypalresponse.Result.TransactionNumber, InvoicePaymentDetails);
                    if (paypal_result)
                    {
                        Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                        if (Invoice.TotalPay == 0)
                        {
                            IsAddPaymentActive = !paypal_result;
                            IsSuccessPaymentActive = paypal_result;
                        }
                        else
                        {
                            if (Invoice.NetAmount > Invoice.TotalPay)
                            {
                                IsPayment_PartialyPaid = true;
                            }
                            else
                            {
                                IsPayment_PartialyPaid = false;
                            }
                        }
                    }

                    if (IsSuccessPaymentActive && Settings.GetCachePrinters != null && Settings.GetCachePrinters.Any(x => x.IsAutoPrintReceipt))
                    {
                        WeakReferenceMessenger.Default.Send(new Messenger.PaypalAutoPrintMessenger(true));
                    }
                }
            }

            if (paypalresponse.Type == MessageType.Info)
            {
                App.Instance.Hud.DisplayToast(paypalresponse.Text, Duration: new TimeSpan(1000));
            }
            else if (paypalresponse.Type == MessageType.Failed)
            {
                App.Instance.Hud.DisplayToast(paypalresponse.Text, Colors.Red, Colors.White);
            }
            else
            {
                App.Instance.Hud.DisplayToast(paypalresponse.Text, Colors.Green, Colors.White);
            }
        }

        async void EmailSend()
        {
            try
            {
                if (!string.IsNullOrEmpty(CustomerEMailId) && !string.IsNullOrEmpty(CustomerEMailId.Trim()))
                {
                    if (CustomerEMailId.IsValidEmail())
                    {
                        var result = await SendEmail(CustomerEMailId);
                        if (result)
                        {
                            CustomerEMailId = "";
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("EmailSendWaiting"), Colors.Green, Colors.White);
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"));
                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_InValidMessage"));
                    }
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_EmptyMessage"));
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async void updatePaymentList(bool isPartillyAfterPayRefund = false)
        {
            //Start Ticket #72507 iPad:- Ability to Change Sequence of POS Screen Payment Types By: Pratik
            if (App.Instance.Issync)
            {
                App.Instance.Issync = false;
                if (PaymentOptionList != null)
                {
                    PaymentOptionList.Clear();
                    PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
                }
                else
                {
                    PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
                }
            }
            //End Ticket #72507 Pratik
            List<PaymentOptionDto> tmppayment = new List<PaymentOptionDto>();

            // using (new Busy(this, false))
            //{
            bool isdata = PaymentOptionList != null && PaymentOptionList.Count > 0;
            bool isHavingGiftCardPermission = Settings.GrantedPermissionNames != null && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EnableGiftCard && Settings.Subscription != null && Settings.Subscription.Edition != null && Settings.Subscription.Edition.PlanType != null && Settings.Subscription.Edition.PlanType != PlanType.StartUp;
            try
            {
                // Changes by Jigar  ticket no 8270
                bool isFullDiscount;
                if (!Invoice.DiscountIsAsPercentage)
                {
                    if (Invoice.TaxInclusive)
                    {
                        isFullDiscount = Invoice.SubTotal == 0 ? true : false;
                    }
                    else
                    {
                        isFullDiscount = Math.Round(Invoice.DiscountValue, 2, MidpointRounding.AwayFromZero) == Math.Round(Invoice.SubTotal, 2, MidpointRounding.AwayFromZero) ? true : false;
                    }
                }
                else
                {
                    if (Invoice.TaxInclusive)
                    {
                        isFullDiscount = Invoice.SubTotal == 0 ? true : false;
                    }
                    else if (Math.Round(Invoice.DiscountValue, 2, MidpointRounding.AwayFromZero) == 0)
                    {
                        isFullDiscount = Math.Round(Invoice.DiscountValue, 2, MidpointRounding.AwayFromZero) == Math.Round(Invoice.SubTotal, 2, MidpointRounding.AwayFromZero) ? true : false;
                    }
                    else
                    {
                        isFullDiscount = Invoice.DiscountValue == 100 ? true : Invoice.InvoiceLineItems.Any(x => x.DiscountValue == 100) ? true : false;
                    }
                }
                //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                if (Invoice.Status == InvoiceStatus.BackOrder && Invoice.TenderAmount == 0 && Invoice.TotalPaid > 0)
                    isFullDiscount = true;
                if (Settings.IsBackorderSaleSelected)
                {
                    Invoice.Status = InvoiceStatus.BackOrder;
                    Invoice.IsEditBackOrderFromSaleHistory = true;
                }
                //Ticket end:#92764.by rupesh
                if (Settings.IsQuoteSale || (Invoice.TenderAmount == 0 && !Invoice.InvoiceLineItems.Any(x => x.BackOrderQty > 0) && isFullDiscount))
                {
                    if (Invoice.Status != InvoiceStatus.Exchange && Invoice.Status != InvoiceStatus.Refunded)
                        Invoice.Status = Settings.IsQuoteSale ? InvoiceStatus.Quote : InvoiceStatus.Completed;//Ticket:#89498

                    IsAddPaymentActive = false;
                    IsSuccessPaymentActive = true;
                    CustomerDto_POS customerresult = null;
                    if (Invoice.CustomerId != null)
                        customerresult = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                    if (customerresult != null)
                    {
                        CustomerEMailId = customerresult.Email;
                        CustomerName = customerresult.FullName;
                    }
                    else
                    {
                        CustomerEMailId = "";
                        CustomerName = "";
                    }
                    if (string.IsNullOrEmpty(CustomerEMailId))
                    {
                        if (Invoice.CustomerDetail != null)
                        {
                            CustomerEMailId = Invoice.CustomerDetail.Email;
                            CustomerName = Invoice.CustomerDetail.FullName;
                        }
                    }
                    if (Settings.IsQuoteSale || isFullDiscount)
                    {
                        if (Settings.IsQuoteSale)
                            SelectedPaymentOption = new PaymentOptionDto();

                        bool result = await calculateInvoicePayment(PaymentOptionType.Cash);
                        if (result)
                        {
                            if (Invoice.ReferenceInvoiceId != 0 && Invoice.Status == InvoiceStatus.Exchange)
                            {
                                var UpdateRefunded = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                                if (UpdateRefunded != null)
                                {
                                    foreach (var itemRF in Invoice.InvoiceLineItems)
                                    {
                                        foreach (var item in UpdateRefunded.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount))
                                        {
                                            if (item.InvoiceItemType == itemRF.InvoiceItemType && item.InvoiceItemValue == itemRF.InvoiceItemValue && itemRF.Quantity < 0)
                                            {
                                                item.RefundedQuantity += itemRF.Quantity.ToPositive();
                                            }
                                        }
                                    }
                                    await saleService.UpdateLocalInvoice(UpdateRefunded);
                                }
                            }

                        }
                    }
                }
                else if (AllPaymentOptionList != null && AllPaymentOptionList.Count() > 0)
                {
                    if (isdata)
                    {
                        var temppay = PaymentOptionList.Where(x => x.IsConfigered && !x.IsDeleted && x.IsActive && (x.PaymentOptionType == PaymentOptionType.Credit || x.PaymentOptionType == PaymentOptionType.Loyalty)).Copy();
                        foreach (var giftcard in temppay)
                        {
                            PaymentOptionList.RemoveAt(PaymentOptionList.IndexOf(PaymentOptionList.First(a => a.Id == giftcard.Id)));
                        }
                        var tempgiftpay = PaymentOptionList.FirstOrDefault(x => x.IsConfigered && !x.IsDeleted && x.IsActive && x.PaymentOptionType == PaymentOptionType.Credit);
                        if (tempgiftpay != null && !isHavingGiftCardPermission)
                            PaymentOptionList.RemoveAt(PaymentOptionList.IndexOf(PaymentOptionList.First(a => a.Id == tempgiftpay.Id)));
                    }
                    AllPaymentOptionList.All(x => { x.DisplayName = x.Name; return true; });


                    tmppayment = AllPaymentOptionList.Where(x => x.IsConfigered && !x.IsDeleted && x.IsActive && x.PaymentOptionType != PaymentOptionType.Credit && x.PaymentOptionType != PaymentOptionType.Ecommerce && x.PaymentOptionType != PaymentOptionType.GiftCard && x.PaymentOptionType != PaymentOptionType.Layby && x.PaymentOptionType != PaymentOptionType.Loyalty
                    && x.PaymentOptionType != PaymentOptionType.BarclayCard && x.PaymentOptionType != PaymentOptionType.Fiska
                    && x.PaymentOptionType != PaymentOptionType.PaypalExpress && x.PaymentOptionType != PaymentOptionType.StripeCheckout
                    && x.PaymentOptionType != PaymentOptionType.AuthorizeNet && x.PaymentOptionType != PaymentOptionType.ZipPayEcomm
                    && x.PaymentOptionType != PaymentOptionType.OnAccount).ToList();

                    //Start Ticket #72507 iPad:- Ability to Change Sequence of POS Screen Payment Types By: Pratik
                    tmppayment = tmppayment.Where(x => x.RegisterPaymentOptions.Count() == 0 || x.IsDefault || x.RegisterPaymentOptions.Any(y => y.RegisterId == Settings.CurrentRegister.Id)).OrderBy(a => a.Sequence).ToList(); //Start #94427 Disable cash option on a single register By Pratik
                                                                                                                                                                                                                                  //tmppayment = tmppayment.Where(x => x.RegisterPaymentOptions.Count() == 0 || x.RegisterPaymentOptions.Any(y => y.RegisterId == Settings.CurrentRegister.Id)).ToList();
                                                                                                                                                                                                                                  //End Ticket #72507 By: PratikBy: Pratik

                    if (isHavingGiftCardPermission)
                    {
                        foreach (var giftcard in AllPaymentOptionList.Where(x => x.IsConfigered && !x.IsDeleted && x.IsActive && x.PaymentOptionType == PaymentOptionType.GiftCard))
                        {
                            if (giftcard != null)
                            {
                                tmppayment.Add(giftcard);
                            }
                        }
                    }

                    bool isCustomeUpdated = false;
                    //Ticket start:#21749 On account sale in offline mode in iPad and Android.by rupesh
                    //START Ticket #74753 STORE CREDIT By Pratik
                    if (Invoice != null && Invoice.CustomerId != null && (Invoice.CustomerId > 1 || Invoice.CustomerTempId != null) && Invoice.CustomerName.ToLower() != "select customer" && Invoice.CustomerName.ToLower() != "Cash sale" && Invoice.CustomerName.ToLower() != "Walk in")
                    //End Ticket #74753 by Pratik
                    {
                        CustomerDto_POS customerresult;
                        if (Invoice.CustomerId > 0)
                            customerresult = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                        else
                            customerresult = customerService.GetLocalCustomerByTempId(Invoice.CustomerTempId);

                        if (customerresult != null)
                        {
                            CustomerModel.SelectedCustomer = customerresult;
                            CustomerEMailId = customerresult.Email;
                            CustomerName = customerresult.FullName;
                            Invoice.CustomerDetail = CustomerModel.SelectedCustomer;
                            isCustomeUpdated = true;
                        }
                        else
                        {
                            CustomerEMailId = "";
                            CustomerName = "";
                        }
                    }
                    else
                    {
                        CustomerEMailId = "";
                        CustomerName = "";
                    }

                    bool isHavingLayByPermission = Settings.GrantedPermissionNames != null && (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.LayBy"));

                    if (isHavingLayByPermission)
                    {
                        IsPayment_LaybyVisible = Invoice?.InvoiceFloorTable == null ? true : false; //#94565 
                        if (Invoice.NetAmount > 0 && Settings.StoreGeneralRule.ActivateLayBy && isCustomeUpdated && Invoice.Status != InvoiceStatus.Exchange)
                            IsPayment_LaybyEnable = (Invoice?.InvoiceLineItems != null && Invoice.InvoiceLineItems.Any(a => a.InvoiceItemType == InvoiceItemType.GiftCard)) ? false : true; //start #94424 FR Need to delete Gift cards from discount offer By Pratik
                        else
                            IsPayment_LaybyEnable = false;
                    }
                    else
                        IsPayment_LaybyVisible = false;


                    if (Invoice != null && Invoice.Status == InvoiceStatus.OnAccount)
                    {
                        IsPayment_LaybyVisible = false;
                    }

                    bool isHavingAccountPayPermission = Settings.GrantedPermissionNames != null && (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.OnAccount"));
                    if (!isHavingAccountPayPermission)
                    {
                        IsPayment_OnAccountVisible = false;
                    }
                    else
                    {
                        IsPayment_OnAccountVisible = Invoice?.InvoiceFloorTable == null ? true : false; //#94565 
                        if (isCustomeUpdated)
                        {
                            IsPayment_OnAccountEnable = true;

                            if (Invoice.NetAmount > 0 && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.ActivateOnAccount)
                            {
                                CustomerModel.SelectedCustomer.OutStandingBalance = CustomerModel.SelectedCustomer.OutStandingBalance ?? 0;
                                CustomerModel.SelectedCustomer.CreditLimit = CustomerModel.SelectedCustomer.CreditLimit ?? 0;
                                var val = CustomerModel.SelectedCustomer.CreditLimit - CustomerModel.SelectedCustomer.OutStandingBalance;
                                if (val == null)
                                {
                                    val = 0;
                                }
                                OnAccountText = "On Account" + " (" + val.Value.ToString("C") + ")";
                            }
                            else
                            {
                                IsPayment_OnAccountEnable = false;
                            }
                        }
                        else
                        {
                            IsPayment_OnAccountEnable = false;
                            OnAccountText = "On Account";
                        }

                        //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time.by rupesh
                        if (Invoice.IsEditBackOrderFromSaleHistory)
                        {
                            IsPayment_OnAccountVisible = false;
                            IsPayment_LaybyVisible = false;
                        }
                        //Ticket end:#84289 .by rupesh
                    }

                    if (isCustomeUpdated)
                    {
                        //START Ticket #74753 STORE CREDIT By Pratik
                        //START Ticket #77850 iOS: Not worked user permissioniOS: Not worked user permission By Pratik
                        if ((CustomerModel.SelectedCustomer.CreditBalance >= 0 && Invoice.Status != InvoiceStatus.Refunded) || (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.RefundSaleWithStoreCredit") && Invoice.Status == InvoiceStatus.Refunded) || (Invoice.Status == InvoiceStatus.Exchange && Invoice.TenderAmount < 0))
                        //End Ticket #74753 #77850 by Pratik
                        {
                            foreach (var creditBalance in AllPaymentOptionList.Where(x => x.IsConfigered && !x.IsDeleted && x.IsActive && x.PaymentOptionType == PaymentOptionType.Credit))
                            {
                                if (creditBalance != null)
                                {
                                    CustomerModel.SelectedCustomer.CreditBalance = CustomerModel.SelectedCustomer.CreditBalance ?? 0;
                                    //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                                    creditBalance.DisplayName = "Store credit";
                                    creditBalance.DisplaySubName = CustomerModel.SelectedCustomer.CreditBalance.Value.ToString("C");
                                    //End ticket #73190
                                    tmppayment.Add(creditBalance);
                                }
                            }
                        }

                        //bool isHavingLoyaltyPayPermission = Settings.GrantedPermissionNames != null && (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.Loyalty")) && Settings.Subscription != null && Settings.Subscription.Edition != null && Settings.Subscription.Edition.PlanType != null && Settings.Subscription.Edition.PlanType != PlanType.StartUp;
                        //Ticket start:#91263 iOS:FR Redeeming Points.by rupesh
                        //bool isHavingLoyaltyPayPermission = Settings.Subscription != null && Settings.Subscription.Edition != null && Settings.Subscription.Edition.PlanType != null && Settings.Subscription.Edition.PlanType != PlanType.StartUp;
                        bool isHavingLoyaltyPayPermission = Settings.GrantedPermissionNames != null && (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.AllowRedeemPoints"));
                        //Ticket end:#91263 .by rupesh

                        //For loyality button visiblity
                        //START Ticket #74753 STORE CREDIT By Pratik
                        if (isHavingLoyaltyPayPermission && CustomerModel.SelectedCustomer.AllowLoyalty)
                        //End Ticket #74753 by Pratik
                        {

                            foreach (var loyalty in AllPaymentOptionList.Where(x => x.IsConfigered && !x.IsDeleted && x.IsActive && x.PaymentOptionType == PaymentOptionType.Loyalty))
                            {
                                if (loyalty != null)
                                {
                                    var val = Math.Round((CustomerModel.SelectedCustomer.CurrentLoyaltyBalance / Settings.StoreGeneralRule.LoyaltyPointsValue), 2);
                                    //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                                    loyalty.DisplayName = getCardName(loyalty.Name);
                                    loyalty.DisplaySubName = val.ToString("C");
                                    //End ticket #73190
                                    tmppayment.Add(loyalty);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.SaleLogger("Exception Msg while updatePaymentList - " + ex.StackTrace);
                PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
                ex.Track();
                CustomerEMailId = "";
                CustomerName = "";
            }

            //Start #94427 Disable cash option on a single register By Pratik
            if (tmppayment != null && Settings.CurrentRegister != null && !Settings.CurrentRegister.IsActiveCashPayment)
            {
                PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
                tmppayment = tmppayment.Where(a => a.PaymentOptionType != PaymentOptionType.Cash).ToList();
            }
            //End #94427 By Pratik

            if (tmppayment != null && tmppayment.Count > 0)
            {

                RefundAmountText = string.Empty;
                if ((Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange) && Invoice.InvoiceRefundPayments != null)
                {
                    var payments = Invoice.InvoiceRefundPayments;

                    if (payments != null)
                    {
                        var tempString = string.Empty;
                        foreach (var item in Invoice.InvoiceRefundPayments)
                        {

                            string amount = string.Format("{0:C}", item.Amount.ToString("C"));
                            var tempstring1 = item.PaymentOptionName + " " + amount;
                            tempString = tempString + " " + tempstring1;

                        }
                        RefundAmountText = "This sale was originally paid with " + tempString + ".";
                    }
                }

                bool isOnlyAfterPay = Invoice.Status == InvoiceStatus.Refunded
                    && (Invoice.InvoiceRefundPayments?.FirstOrDefault(x => x.PaymentOptionType == PaymentOptionType.Afterpay) != null);
                //Ticket #8290 End:By Nikhil. 

                for (int i = 0; i < tmppayment.Count; i++)
                {
                    var payment = tmppayment[i];
                    if (isOnlyAfterPay)
                    {
                        if (payment.PaymentOptionType != PaymentOptionType.Afterpay &&
                            !isPartillyAfterPayRefund)
                            payment.IsEnable = false;
                        else
                        {
                            if (isPartillyAfterPayRefund)
                            {
                                payment.IsEnable = true;
                            }
                        }
                    }
                    else
                    {
                        payment.IsEnable = true;
                    }

                    //tempPaymentOptionList.Add(payment);

                    //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                    if (Invoice != null && payment.Surcharge != null && payment.PaymentOptionType != PaymentOptionType.Cash)
                    {
                        if (Invoice.Status == InvoiceStatus.Refunded || (Invoice.Status == InvoiceStatus.Exchange && Invoice.TotalPay < 0))
                        {
                            if (payment.CanApplySurchageOnRefund)
                            {
                                var surcharge = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == payment.Id && a.InvoicePaymentDetails != null && a.InvoicePaymentDetails.Count > 0)
                               .Sum(a => a.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount)
                               .Sum(a => Convert.ToDecimal(a.Value ?? "0").ToPositive())) + Invoice.InvoicePayments.Where(a => a.PaymentOptionId == payment.Id && a.InvoicePaymentDetails != null && a.InvoicePaymentDetails.Count > 0)
                               .Sum(a => a.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount)
                               .Sum(a => Convert.ToDecimal(a.Value ?? "0")));

                                var totaltender = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == payment.Id)
                                .Sum(a => a.Amount) + Invoice.InvoicePayments.Where(a => a.PaymentOptionId == payment.Id)
                                .Sum(a => a.Amount) - surcharge;
                                if (totaltender.ToPositive() > 0)
                                {
                                    var percentage = Math.Round(((surcharge * 100) / totaltender), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                                    if (Invoice.TenderAmount.ToPositive() <= totaltender.ToPositive())
                                        payment.DisplaySurcharge = Math.Round(((Invoice.TenderAmount * percentage) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                                    else
                                        payment.DisplaySurcharge = Math.Round(((totaltender * percentage) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                                }
                                else
                                    payment.DisplaySurcharge = null;
                            }
                            else
                            {
                                payment.DisplaySurcharge = null;
                            }
                        }
                        else
                        {
                            if (Invoice.TenderAmount.ToPositive() <= (Invoice.NetAmount - Invoice.TotalPaid))
                                payment.DisplaySurcharge = Math.Round(((Invoice.TenderAmount * payment.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                            else
                                payment.DisplaySurcharge = Math.Round((((Invoice.NetAmount - Invoice.TotalPaid) * payment.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        }
                        payment.DisplayName = getCardName(payment.Name);
                        payment.DisplaySubName = payment.DisplaySurcharge.HasValue ? (payment.DisplaySurcharge.Value.ToPositive() > 0 ? "Extra " + payment.DisplaySurcharge.Value.ToString("C") : "") : "";
                    }
                    //End ticket #73190

                    //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
                    if (payment.PaymentOptionType == PaymentOptionType.Cash)
                    {
                        var amount = CashPaymentRoundingAmount();
                        payment.DisplaySubName = Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent ? amount.ToString("C") : "";

                    }
                    //Ticket end:#91262.by rupesh

                    if (isdata)
                    {
                        var first = PaymentOptionList.FirstOrDefault(a => a.Id == payment.Id);
                        if (first != null)
                            first = payment;
                        else
                            PaymentOptionList.Add(payment);
                    }
                    else
                    {
                        PaymentOptionList.Add(payment);
                    }

                    if (payment.PaymentOptionType == PaymentOptionType.AssemblyPayment)
                    {
                        if (Invoice.TenderAmount > 0)
                        {
                            var temp = new PaymentOptionDto();

                            temp.Name = payment.Name;
                            temp.PaymentOptionName = payment.PaymentOptionName;
                            temp.PaymentOptionType = PaymentOptionType.AssemblyPayment;
                            temp.IsActive = payment.IsActive;
                            temp.IsMoto = true;
                            temp.IsConfigered = payment.IsConfigered;
                            temp.DisplayName = "[MOTO]";
                            temp.ConfigurationDetails = payment.ConfigurationDetails;
                            temp.IsConfigered = payment.IsConfigered;
                            temp.IsActive = payment.IsActive;
                            temp.IsConfigered = payment.IsConfigered;
                            temp.CanBeConfigered = payment.CanBeConfigered;
                            temp.IsCustomerAccount = payment.IsCustomerAccount;
                            temp.RegisterPaymentOptions = payment.RegisterPaymentOptions;
                            temp.Id = payment.Id;

                            if (isdata)
                            {
                                var first = PaymentOptionList.FirstOrDefault(a => a.Id == temp.Id && temp.IsMoto == true);
                                if (first != null)
                                    first = temp;
                                else
                                    PaymentOptionList.Add(temp);
                            }
                            else
                            {
                                PaymentOptionList.Add(temp);
                            }
                        }
                    }
                }

                if (PaymentOptionList != null && PaymentOptionList.Count > 0)
                {
                    var reminder = (PaymentOptionList.Count() % 3) > 0 ? 1 : 0;

                    int divide = PaymentOptionList.Count() / 3;

                    //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                    PaymentListHeight = (((divide + reminder) * 80) + ((divide + reminder - 1) * 5));
                    //End ticket #73190
                }
                else
                    PaymentListHeight = 0;
            }
            else
            {
                PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
            }

            //};
        }

        //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
        string getCardName(string name)
        {
            if (name.Length > 17)
                return name.Substring(0, 14) + "...";

            return name;
        }
        //End ticket #73190

        public void selectPaymentOptionByType(PaymentOptionType paymentType)
        {
            try
            {
                PaymentOptionDto paymentOption = new PaymentOptionDto();
                if (AllPaymentOptionList != null)
                {
                    paymentOption = AllPaymentOptionList.FirstOrDefault(x => x.PaymentOptionType == paymentType && (x.RegisterPaymentOptions.Any(a => a.RegisterId == Settings.CurrentRegister?.Id) || !x.RegisterPaymentOptions.Any()));
                }
                if (paymentOption != null)
                {
                    selectPaymentOption(paymentOption);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SendLogsToServer(PaymentOptionDto paymentOption, Dictionary<string, string> requestMap, object paymentResponse = null)
        {
            Extensions.SendLogsToServer(logService, paymentOption, requestMap, "", paymentResponse);
        }
        #endregion

        #region Payment/Print related methods
        public async void selectPaymentOption(PaymentOptionDto paymentOption)
        {
            if (paymentOption == null)
            {
                return;
            }

            logRequestDetails.Clear();

            if (paymentOption != null && (paymentOption.PaymentOptionType == PaymentOptionType.Credit || paymentOption.PaymentOptionType == PaymentOptionType.Layby || paymentOption.PaymentOptionType == PaymentOptionType.Loyalty || paymentOption.PaymentOptionType == PaymentOptionType.OnAccount) && (Invoice.CustomerId == null || Invoice.CustomerId < 2) && Invoice.CustomerTempId == null)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("PayButtonCutomerValidation"));
                return;
            }

            if (isClicked)
                return;

            //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time.by rupesh
            if (Invoice.IsEditBackOrderFromSaleHistory)
            {
                var backorderOutstanding = Invoice.NetAmount - Invoice.TotalPaid;
                if (Invoice.TenderAmount > backorderOutstanding)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("BackorderDepositeshouldnotbemorethatbackorderoutstanding"));
                    return;
                }
            }
            //Ticket end:#84289 .by rupesh

            isClicked = true;
            SelectedPaymentOption = paymentOption;
            //IsLoad = true;
            using (new Busy(this, false))
            {
                try
                {
                    if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet && Invoice.InvoiceHistories != null
                    && Invoice.InvoiceHistories.Count > 0 && Invoice.InvoiceHistories.Last(x => x.Status != InvoiceStatus.EmailSent).Status == InvoiceStatus.Quote)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                        return;
                    }

                    bool result = false;
                    if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.GiftCard)
                    {
                        result = await addGiftCardPayment(paymentOption, Payment_GiftCardNumber);
                    }
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.Loyalty)
                    {
                        result = await LoyaltyPayment(paymentOption);
                    }
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.Credit)
                    {
                        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                        {
                            var customerresult = customerService.GetLocalCustomerById(CustomerModel.SelectedCustomer.Id);
                            if (customerresult != null)
                            {
                                CustomerModel.SelectedCustomer = customerresult;//.result;
                            }

                            if (Invoice.TenderAmount > CustomerModel.SelectedCustomer.CreditBalance)
                            {
                                if (CustomerModel.SelectedCustomer.CreditBalance != null && CustomerModel.SelectedCustomer.CreditBalance != 0)
                                {
                                    Invoice.TenderAmount = CustomerModel.SelectedCustomer.CreditBalance.Value;
                                    CustomerModel.SelectedCustomer.CreditBalance = 0;
                                    Invoice.CustomerDetail.CreditBalance = CustomerModel.SelectedCustomer.CreditBalance;
                                    customerService.UpdateLocalCustomer(CustomerModel.SelectedCustomer);

                                    result = await addPaymentToSell(paymentOption, "");

                                    bool CreditPayPermission = Settings.GrantedPermissionNames != null && (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.Credit"));

                                    if (CreditPayPermission && (CustomerModel.SelectedCustomer.CreditBalance > 0 || Invoice.Status == InvoiceStatus.Refunded))
                                    {

                                        var item = PaymentOptionList.FirstOrDefault(i => i.PaymentOptionType == PaymentOptionType.Credit);
                                        if (item != null)
                                        {
                                            CustomerModel.SelectedCustomer.CreditBalance = CustomerModel.SelectedCustomer.CreditBalance ?? 0;
                                            if (item != null)
                                            {
                                                item.Name = "Store credit" + " (" + CustomerModel.SelectedCustomer.CreditBalance + ")";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CreditBalanceValidationMessage"));
                                    return;
                                }
                            }
                            else
                            {
                                CustomerModel.SelectedCustomer.CreditBalance -= Invoice.TenderAmount;
                                Invoice.CustomerDetail.CreditBalance = CustomerModel.SelectedCustomer.CreditBalance;
                                customerService.UpdateLocalCustomer(CustomerModel.SelectedCustomer);

                                result = await addPaymentToSell(paymentOption, "");
                            }
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                            return;
                        }
                    }
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.OnAccount)
                    {
                        var answer = await App.Alert.ShowAlert(LanguageExtension.Localize("SellAccountAlertTitle"), String.Format(LanguageExtension.Localize("SellAccountAlertBody"), Invoice.TenderAmount.ToString("C")), "Yes", "Cancel");
                        if (answer)
                        {
                            //Ticket start:#21749 On account sale in offline mode in iPad and Android. by rupesh
                            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet || Settings.StoreGeneralRule.AllowCustomerOnAccountInOfflineMode)
                            {
                                CustomerDto_POS LocalCustomer;
                                if (CustomerModel.SelectedCustomer.Id > 0)
                                    LocalCustomer = customerService.GetLocalCustomerById(CustomerModel.SelectedCustomer.Id);
                                else
                                    LocalCustomer = customerService.GetLocalCustomerByTempId(CustomerModel.SelectedCustomer.TempId);

                                if (LocalCustomer != null)
                                {
                                    CustomerModel.SelectedCustomer.OutStandingBalance = LocalCustomer.OutStandingBalance ?? 0;
                                }
                                else
                                {
                                    CustomerModel.SelectedCustomer.OutStandingBalance = 0;
                                }

                                decimal? AccountBalance = CustomerModel.SelectedCustomer.CreditLimit - CustomerModel.SelectedCustomer.OutStandingBalance;

                                if (AccountBalance < Invoice.TotalPay)
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CreditLimitValidationMessage"));
                                    return;
                                }

                                // //Start #91991 By Pratik
                                POReferenceID = string.Empty;
                                if (Settings.StoreGeneralRule.OnAccountPONumberForAccouting && paymentOption.PaymentOptionType == PaymentOptionType.OnAccount)
                                {
                                    POReferenceID = saleService.GetInvoiceDetailsValueExist(Invoice.Id);
                                    if (string.IsNullOrEmpty(POReferenceID))
                                        POReferenceID = Invoice.InvoiceDetail?.Value;
                                    if (string.IsNullOrEmpty(POReferenceID))
                                    {
                                        SelectedPaymentOption = paymentOption;
                                        Pages.ChequeNumberPopupPage chequeNumberPopup = new Pages.ChequeNumberPopupPage("PO Number for On-Account Sale");
                                        chequeNumberPopup.AddReferenceNumber += AddReferenceNumber;
                                        await NavigationService.PushModalAsync(chequeNumberPopup);
                                        return;
                                    }
                                }
                                // //End #91991 By Pratik

                                //Start Ticket #63876,#85166 iOS: FR : On Account calculation on print receipt by Pratik
                                // CustomerModel.SelectedCustomer.OutStandingBalance += Invoice.TenderAmount;
                                var result1 = Invoice.CustomFields != null ? JsonConvert.DeserializeObject<CustomFieldsResponce>(Invoice.CustomFields) : null;
                                if (result1?.invoiceOutstanding != null)
                                {
                                    if (Invoice.InvoiceHistories != null
                                        && Invoice.InvoiceHistories.Count >= 1
                                        && Invoice.InvoiceHistories.Last(x => x.Status != InvoiceStatus.EmailSent).Status == InvoiceStatus.OnAccount
                                        && result1.invoiceOutstanding.currentSale.HasValue)
                                    {
                                        CustomerModel.SelectedCustomer.OutStandingBalance -= result1.invoiceOutstanding.currentSale;
                                        CustomerModel.SelectedCustomer.OutStandingBalance += Invoice.TenderAmount;
                                    }
                                    else
                                        CustomerModel.SelectedCustomer.OutStandingBalance -= Invoice.TotalPaid;
                                }
                                else
                                    CustomerModel.SelectedCustomer.OutStandingBalance += Invoice.TenderAmount;
                                //End Ticket #63876,#85166 by Pratik 

                                customerService.UpdateLocalCustomer(CustomerModel.SelectedCustomer);

                                if (Invoice.Status == InvoiceStatus.Exchange)
                                {
                                    //#97493
                                    Invoice.IsExchangeSale = true;
                                    var UpdateRefunded = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                                    foreach (var itemRF in Invoice.InvoiceLineItems)
                                    {
                                        itemRF.Id = 0;
                                        if (UpdateRefunded != null)
                                        {
                                            foreach (var item in UpdateRefunded.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount))
                                            {

                                                if (item.InvoiceItemType == itemRF.InvoiceItemType &&
                                                item.InvoiceItemValue == itemRF.InvoiceItemValue && itemRF.Quantity < 0)
                                                {
                                                    item.RefundedQuantity += itemRF.Quantity.ToPositive();
                                                }
                                            }
                                            await saleService.UpdateLocalInvoice(UpdateRefunded);
                                        }
                                    }
                                    //#97493
                                }

                                Invoice.Status = InvoiceStatus.OnAccount;

                                IsAddPaymentActive = false;
                                IsSuccessPaymentActive = true;
                                await sale_onaccountSale();
                                result = true;
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                return;
                            }
                        }
                    }
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.Layby)
                    {
                        var answer = await App.Alert.ShowAlert(LanguageExtension.Localize("SellLaybyAlertTitle"), LanguageExtension.Localize("SellLaybyAlertBody") + Invoice.TenderAmount.ToString("C"), "Yes", "Cancel");
                        if (answer)
                        {
                            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                            {
                                result = await sale_LaybySale();
                                IsSuccessPaymentActive = true;
                                IsAddPaymentActive = false;
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                return;
                            }
                        }
                    }
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.HikePayTapToPay)
                    {
                        if (!App.Instance.IsInternetConnected)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"));
                            return;
                        }

                        if (string.IsNullOrEmpty(paymentOption.ConfigurationDetails))
                        {
                            App.Instance.Hud.DisplayToast(paymentOption.Name + " is not configured!Please configure from Web and sync data from Setting");
                            return;
                        }

                        string saleCommand = string.Empty;
                        if (Invoice.Status == InvoiceStatus.Refunded)
                        {
                            saleCommand = "Refund";
                        }
                        else if (Invoice.Status == InvoiceStatus.Exchange)
                        {
                            if (Invoice.TenderAmount < 0)
                            {
                                saleCommand = "Refund";
                            }
                            else
                            {
                                saleCommand = "Sale";
                                if (Invoice.TenderAmount > (Invoice.TotalPay))
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("AmountValidationForIntegratedPaymentTypeMessage"));
                                    return;
                                }
                            }
                        }
                        else
                        {
                            saleCommand = "Sale";
                            if (Invoice.TenderAmount > (Invoice.TotalPay))
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("AmountValidationForIntegratedPaymentTypeMessage"));
                                return;
                            }
                        }

                        try
                        {
                            if (paymentOption.ConfigurationDetails != null)
                            {

                                // Ensure service instance
                                nadaTapToPay ??= DependencyService.Get<INadaTapToPay>();
                                // Deserialize config
                                var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
                                var newStoreId = config.StoreId;
                                if (string.IsNullOrEmpty(Settings.HikePayStoreId))
                                    Settings.HikePayStoreId = newStoreId;
                                var currentStoreId = Settings.HikePayStoreId;

                                // Determine if we need to reauthorize
                                bool storeChanged = currentStoreId != newStoreId;
                                // Always authorize if store changed or SDK not initialized
                                if (storeChanged || !nadaTapToPay.IsInitialized)
                                {
                                    nadaTapToPay.AuthorizeSdk(newStoreId);
                                }
                                // If still not initialized → try manual initialization
                                if (!nadaTapToPay.IsInitialized)
                                {
                                    var intializeResponse = await nadaTapToPay.InitializeSdkManually();
                                    if (!nadaTapToPay.IsInitialized)
                                    {
                                        App.Instance.Hud.DisplayToast(intializeResponse.SaleToPOIResponse.TransactionResponse.Response.ErrorMessage, Colors.Red, Colors.White);
                                        return;
                                    }
                                }
                                // If store changed → clear previous session & save new store
                                if (storeChanged)
                                {
                                    nadaTapToPay.ClearSession();
                                    Settings.HikePayStoreId = newStoreId;
                                }

                                logRequestDetails.TryAdd("NadaPayConfigurationDto", paymentOption.ConfigurationDetails);
                                logRequestDetails.TryAdd("IsRefund", (Invoice.Status == InvoiceStatus.Refunded).ToString());
                                logRequestDetails.TryAdd("Amount", Invoice.TenderAmount.ToString());
                                logRequestDetails.TryAdd("InvoiceNumber", Invoice.Number);
                                var lastReference = Guid.NewGuid().ToString();
                                logRequestDetails.TryAdd("Reference", lastReference);
                                Extensions.SendLogsToServer(logService, paymentOption, logRequestDetails);
                                //await nadaTapToPay.DeviceConfigure(Settings.AccessToken);
                                var amount = (Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0));
                                string tempobject = "Sale: " + paymentOption.ConfigurationDetails + " tempInvoiceID: " + Invoice.InvoiceTempId + " Amount: " + amount.ToString() + "Last reference: " + lastReference + " Invoice.Note: " + Invoice.Note;
                                NadaTapToPayDto saleResult;
                                if (amount > 0)
                                {
                                    SaveInLocalbeforePayment(tempobject, false);
                                    saleResult = await nadaTapToPay.Sale(amount, Invoice.Currency, lastReference);
                                    IsBusy = false;
                                }
                                else
                                {


                                    NadaPayTransactionDto nadaTapToPayDtoResponse = null;
                                    decimal authorizedAmount = 0;
                                    decimal requestedAmount = amount.ToPositive();

                                    var tapToPayRefunds = Invoice.InvoiceRefundPayments?
                                        .Where(x => x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay)
                                        ?? Enumerable.Empty<InvoicePaymentDto>();

                                    if (tapToPayRefunds.Count() > 1)
                                    {
                                        var refundSlider = new AdyenRefundPaymentPage();
                                        refundSlider.ViewModel.InvoiceRefundPayments = new ObservableCollection<RefundPaymentDto>();
                                        foreach (var refundPayment in tapToPayRefunds)
                                        {
                                            NadaPayTransactionDto nadaTapToPayDtoReturnResponse = null;
                                            decimal refundedamt = 0;
                                            // decimal payamt = 0;
                                            var refundDetail = refundPayment.InvoicePaymentDetails
                                            .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                                            try
                                            {
                                                nadaTapToPayDtoReturnResponse = JsonConvert.DeserializeObject<NadaPayTransactionDto>(refundDetail?.Value);
                                                // if (nadaTapToPayDtoResponse == null && (int)refundPayment.PaymentOptionType == 49)
                                                // {
                                                //     var hikePayDto = JsonConvert.DeserializeObject<HikePayFromCloud>(refundDetail?.Value);
                                                //     nadaTapToPayDtoReturnResponse = HikePayFromCloud.MapToNada(hikePayDto);
                                                // }
                                                // payamt = nadaTapToPayDtoReturnResponse?
                                                //     .SaleToPOIResponse?.TransactionResponse?.PaymentResult?
                                                //     .AmountsResp?.AuthorizedAmount ?? 0;
                                            }
                                            catch
                                            {
                                                App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                                return;
                                            }

                                            var payments = Invoice.InvoicePayments
                                                .Where(x => x.PaymentOptionId == refundPayment.PaymentOptionId);

                                            foreach (var payment in payments)
                                            {
                                                var paymentDetail = payment.InvoicePaymentDetails
                                                    .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                                                try
                                                {
                                                    var dto = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);
                                                    // if (dto.SaleToPOIResponse == null && (int)payment.PaymentOptionType == 49)
                                                    // {
                                                    //     var hikePayDto = JsonConvert.DeserializeObject<HikePayFromCloud>(paymentDetail?.Value);
                                                    //     dto = HikePayFromCloud.MapToNada(hikePayDto);
                                                    // }
                                                    if (!string.IsNullOrEmpty(dto?.SaleTransactionId) &&
                                                    dto.SaleTransactionId.Contains(nadaTapToPayDtoReturnResponse.SaleTransactionId))
                                                    {
                                                        var refunded = dto.Amount;
                                                        refundedamt += refunded;
                                                    }
                                                }
                                                catch
                                                {
                                                    App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                                    return;
                                                }

                                            }

                                            refundSlider.ViewModel.InvoiceRefundPayments.Add(new RefundPaymentDto
                                            {
                                                Amount = refundPayment.Amount,
                                                ReturnAmount = refundedamt,
                                                TenderedAmount = (refundPayment.Amount - refundedamt).ToString($"N{Settings.StoreDecimalDigit}"),
                                                PrintPaymentOptionName = nadaTapToPayDtoReturnResponse?.MaskedPan,
                                                Id = refundPayment.Id,
                                            });
                                        }

                                        refundSlider.Selected_InvoiceRefundPayments += async (sender, e) =>
                                        {
                                            if (e != null)
                                            {
                                                var refundPayment = e as RefundPaymentDto;
                                                var Selected_RefundPayment = tapToPayRefunds.FirstOrDefault(a => a.Id == refundPayment.Id);
                                                result = await HandlePartialRefund(paymentOption, Selected_RefundPayment, null, refundPayment, true);
                                                refundSlider.ViewModel.IsBusy = false;
                                                if (result)
                                                {
                                                    refundSlider?.Close();
                                                }
                                                await PaymentSuccessCheck(result, paymentOption);
                                            }
                                        };

                                        await NavigationService.PushModalAsync(refundSlider);
                                        return;
                                    }

                                    foreach (var refundPayment in tapToPayRefunds)
                                    {
                                        // -----------------------------
                                        // 1) Get base authorized amount
                                        // -----------------------------
                                        var paymentDetail = refundPayment.InvoicePaymentDetails
                                            .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                                        try
                                        {
                                            nadaTapToPayDtoResponse = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);
                                            // if (nadaTapToPayDtoResponse.SaleToPOIResponse == null && (int)refundPayment.PaymentOptionType == 49)
                                            // {
                                            //     var hikePayDto = JsonConvert.DeserializeObject<HikePayFromCloud>(paymentDetail?.Value);
                                            //     nadaTapToPayDtoResponse = HikePayFromCloud.MapToNada(hikePayDto);
                                            // }
                                            lastReference = nadaTapToPayDtoResponse?.SaleTransactionId;
                                            tempobject = "Sale: " + paymentOption.ConfigurationDetails + " tempInvoiceID: " + Invoice.InvoiceTempId + " Amount: " + amount.ToString() + "Last reference: " + lastReference + " Invoice.Note: " + Invoice.Note;
                                            authorizedAmount = nadaTapToPayDtoResponse.Amount;
                                        }
                                        catch (Exception ex)
                                        {
                                            App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                            return;
                                        }

                                        // -----------------------------
                                        // 2) Subtract amounts from related payments
                                        // -----------------------------
                                        var payments = Invoice.InvoicePayments
                                            .Where(x => x.PaymentOptionId == refundPayment.PaymentOptionId);

                                        foreach (var payment in payments)
                                        {
                                            paymentDetail = payment.InvoicePaymentDetails
                                                .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                                            try
                                            {
                                                var dto = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);
                                                // if (dto.SaleToPOIResponse == null && (int)payment.PaymentOptionType == 49)
                                                // {
                                                //     var hikePayDto = JsonConvert.DeserializeObject<HikePayFromCloud>(paymentDetail?.Value);
                                                //     dto = HikePayFromCloud.MapToNada(hikePayDto);
                                                // }
                                                if (!string.IsNullOrEmpty(dto?.SaleTransactionId) &&
                                                 dto.SaleTransactionId.Contains(nadaTapToPayDtoResponse.SaleTransactionId))
                                                {
                                                    var refunded = dto.Amount;

                                                    authorizedAmount -= refunded;
                                                }
                                            }
                                            catch
                                            {
                                                App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                                return;
                                            }

                                        }

                                        // stop the refund loop as soon as we have a valid amount
                                        if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                                            break;
                                    }

                                    if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                                    {
                                        // Save before proceeding
                                        SaveInLocalbeforePayment(tempobject, true);
                                        // Exact match refund
                                        var isPartial = nadaTapToPayDtoResponse?.Amount > requestedAmount ? true : false;
                                        var poiTransactionId = nadaTapToPayDtoResponse?.PoiTransactionId;
                                        var poiTransactionTimeStamp = nadaTapToPayDtoResponse.PoiTransactionTimeStamp.Value;
                                        var merchantReference = nadaTapToPayDtoResponse?.SaleTransactionId;
                                        var surchargeAmount = nadaTapToPayDtoResponse?.SurchargeAmount ?? 0;
                                        ApiService<IHikePayService> hikePayApiService = new ApiService<IHikePayService>();
                                        var hikePayService = new HikePayService(hikePayApiService);
                                        var hikePaySplitProfile = hikePayService.GetLocalHikePaySplitProfile();
                                        var rule = hikePaySplitProfile?.Rules?
                                            .FirstOrDefault(x => x.PaymentMethod == nadaTapToPayDtoResponse?.PaymentMethod)
                                            ?? hikePaySplitProfile?.Rules?
                                                .FirstOrDefault(x => x.PaymentMethod.ToLower() == "any");
                                        if (rule == null)
                                        {
                                            App.Instance.Hud.DisplayToast("Please contact to HikePay support team", Colors.Red, Colors.White);
                                            return ;
                                        }
                                        var fixedCommission = rule.SplitLogic?.Commission?.FixedAmount ?? 0;
                                        var variableCommissionPercentage = rule.SplitLogic?.Commission?.VariablePercentage ?? 0;
                                        var paymentFee = rule.SplitLogic?.PaymentFee ?? 0;
                                        saleResult = await nadaTapToPay.Refund(isPartial,
                                            requestedAmount, Invoice.Currency, Guid.NewGuid().ToString(), merchantReference, poiTransactionId, poiTransactionTimeStamp, config.BalanceAccountId, surchargeAmount, fixedCommission, variableCommissionPercentage, paymentFee);
                                    }
                                    else
                                    {
                                        App.Instance.Hud.DisplayToast(
                                            $"Refund amount for {paymentOption.Name} should be equal or less than the original sale amount",
                                            Colors.Red, Colors.White
                                        );
                                        return;
                                    }
                                }
                                if (saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.Result != null && saleResult.SaleToPOIResponse.TransactionResponse.Response.Result.ToLower().Contains("success"))
                                {

                                    var merchantReceipt = string.Empty;
                                    var customerReceipt = string.Empty;
                                    foreach (var doc in saleResult?.SaleToPOIResponse?.TransactionResponse?.PaymentReceipt ?? new List<PaymentReceipt>())
                                    {
                                        var receipt = CommonMethods.HikePayReceiptBuilder.BuildReceipt(doc);
                                        bool isCustomerReceipt = doc.DocumentQualifier == "CustomerReceipt";
                                        if (isCustomerReceipt)
                                        {
                                            customerReceipt = receipt;
                                            if (config.IsPrintCustomerReceipt)
                                                VantivCloudReceiptList.Add(receipt);
                                        }
                                        else
                                        {
                                            merchantReceipt = receipt;
                                            if (config.IsPrintMerchantReceipt)
                                                VantivCloudReceiptList.Add(receipt);
                                        }
                                    }
                                    var transactionResult = JsonConvert.SerializeObject(saleResult, new JsonSerializerSettings
                                    {
                                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                                    });
                                    var nadaPayTransactionDto = new NadaPayTransactionDto
                                    {
                                        PoiTransactionId = saleResult?.SaleToPOIResponse?.TransactionResponse?.POIData?.POITransactionID?.TransactionID,
                                        PoiTransactionTimeStamp = saleResult?.SaleToPOIResponse?.TransactionResponse?.POIData?.POITransactionID?.TimeStamp,
                                        SaleTransactionId = saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.MerchantReference,
                                        SaleTransactionTimeStamp = saleResult?.SaleToPOIResponse?.TransactionResponse?.SaleData?.SaleTransactionID?.TimeStamp,
                                        PspReference = saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.PSPReference,
                                        Amount = saleResult?.SaleToPOIResponse?.PaymentResponse != null ?
                                        saleResult?.SaleToPOIResponse?.TransactionResponse?.PaymentResult?
                                        .AmountsResp?.AuthorizedAmount ?? 0 : saleResult?.SaleToPOIResponse?.TransactionResponse?.ReversedAmount ?? 0,
                                        MaskedPan = saleResult?.SaleToPOIResponse?.PaymentResponse?.PaymentResult?.PaymentInstrumentData?.CardData?.MaskedPan ?? "Card",
                                        SurchargeAmount = saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.SurchargeAmount ?? 0,
                                        PaymentMethod = saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.PaymentMethod
                                    };

                                    ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>
                                    {
                                        new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePayResponse, Value = transactionResult },
                                        new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePaySaleResponseData, Value = JsonConvert.SerializeObject(nadaPayTransactionDto,new JsonSerializerSettings
                                        {
                                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                                        }) },
                                        new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayMerchantPrint, Value = merchantReceipt },
                                        new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayCustomerPrint, Value = customerReceipt }
                                    };
                                    // if (Invoice.Status == InvoiceStatus.Refunded)
                                    // {
                                    //     InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { Key = InvoicePaymentKey.ApprovedByUser, Value = Settings.TyroTapToPayRefundPasscodeApprovedBy });
                                    // }
                                    result = await addIntegratedPaymentToSell(SelectedPaymentOption, transactionResult, InvoicePaymentDetails);
                                    if (result)
                                    {
                                        Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                                        if (Invoice.TotalPay == 0)
                                        {
                                            IsAddPaymentActive = !result;
                                            IsSuccessPaymentActive = result;
                                        }
                                        else
                                        {
                                            if (Invoice.NetAmount > Invoice.TotalPay)
                                            {
                                                IsPayment_PartialyPaid = true;
                                            }
                                            else
                                            {
                                                IsPayment_PartialyPaid = false;
                                            }
                                        }
                                    }
                                    Logger.SyncLogger("----Hikepay Status---\n" + "Success" + "\n----HikepayTapToPay response---\n" + transactionResult);
                                }
                                else
                                {
                                    result = false;
                                    if (string.IsNullOrEmpty(saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.ErrorCondition))
                                        App.Instance.Hud.DisplayToast("Transaction Failed", Colors.Red, Colors.White);
                                    else
                                        App.Instance.Hud.DisplayToast(saleResult.SaleToPOIResponse.TransactionResponse.Response.ErrorMessage, Colors.Red, Colors.White);

                                    var transactionResult = "Empty";
                                    if (saleResult != null)
                                        transactionResult = JsonConvert.SerializeObject(saleResult);
                                    Logger.SyncLogger("----Hikepay Status---\n" + "Failed" + "\n----HikepayTapToPay response---\n" + transactionResult);
                                }
                            }
                            else
                            {
                                result = false;
                                App.Instance.Hud.DisplayToast("Hikepay transaction is not allowed");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.SyncLogger("----Hikepay Status---\n" + "Failed" + "\n---- HikepayTapToPay response---\n" + ex.Message);
                        }

                    }
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.HikePay)
                    {
                        if (!App.Instance.IsInternetConnected)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"));
                            return;
                        }

                        if (string.IsNullOrEmpty(paymentOption.ConfigurationDetails))
                        {
                            App.Instance.Hud.DisplayToast(paymentOption.Name + " is not configured!Please configure from Web and sync data from Setting");
                            return;
                        }

                        string saleCommand = string.Empty;
                        if (Invoice.Status == InvoiceStatus.Refunded)
                        {
                            saleCommand = "Refund";
                        }
                        else if (Invoice.Status == InvoiceStatus.Exchange)
                        {
                            if (Invoice.TenderAmount < 0)
                            {
                                saleCommand = "Refund";
                            }
                            else
                            {
                                saleCommand = "Sale";
                                if (Invoice.TenderAmount > (Invoice.TotalPay))
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("AmountValidationForIntegratedPaymentTypeMessage"));
                                    return;
                                }
                            }
                        }
                        else
                        {
                            saleCommand = "Sale";
                            if (Invoice.TenderAmount > (Invoice.TotalPay))
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("AmountValidationForIntegratedPaymentTypeMessage"));
                                return;
                            }
                        }

                        try
                        {
                            if (paymentOption.ConfigurationDetails != null)
                            {
                                var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
                                // Ensure service instance
                                HikeTerminalPay ??= DependencyService.Get<INadaPayTerminalLocalAppService>();
                                // Deserialize config                            

                                logRequestDetails.TryAdd("NadaPayConfigurationDto", paymentOption.ConfigurationDetails);
                                logRequestDetails.TryAdd("IsRefund", (Invoice.Status == InvoiceStatus.Refunded).ToString());
                                logRequestDetails.TryAdd("Amount", Invoice.TenderAmount.ToString());
                                logRequestDetails.TryAdd("InvoiceNumber", Invoice.Number);
                                var lastReference = Guid.NewGuid().ToString();
                                logRequestDetails.TryAdd("Reference", lastReference);
                                Extensions.SendLogsToServer(logService, paymentOption, logRequestDetails);
                                var amount = (Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0));
                                string tempobject = "Sale: " + paymentOption.ConfigurationDetails + " tempInvoiceID: " + Invoice.InvoiceTempId + " Amount: " + amount.ToString() + "Last reference: " + lastReference + " Invoice.Note: " + Invoice.Note;
                                HikePayTerminalResponse saleResult = null;
                                if (amount > 0)
                                {
                                    // SaveInLocalbeforePayment(tempobject, false);
                                    Invoice.CurrentPaymentObject = "NadaPay Sale :  " + tempobject;
                                   _= saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.PaymentProcessing);
                                    saleResult = await HikeTerminalPay.CreateNadaPayTerminalSale(amount, Invoice.Currency, lastReference, paymentOption, Invoice.InvoiceTempId);
                                    IsBusy = false;
                                }
                                else
                                {
                                    NadaPayTransactionDto hikePayTerminalResponse = null;
                                    decimal authorizedAmount = 0;
                                    decimal requestedAmount = amount.ToPositive();

                                    var hikePayTerminalRefunds = Invoice.InvoiceRefundPayments?
                                        .Where(x => x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay)
                                        ?? Enumerable.Empty<InvoicePaymentDto>();

                                    if (hikePayTerminalRefunds.Count() > 1)
                                    {
                                        var refundSlider = new AdyenRefundPaymentPage();
                                        refundSlider.ViewModel.InvoiceRefundPayments = new ObservableCollection<RefundPaymentDto>();
                                        foreach (var refundPayment in hikePayTerminalRefunds)
                                        {
                                            NadaPayTransactionDto nadaTapToPayDtoReturnResponse = null;
                                            decimal refundedamt = 0;
                                            var refundDetail = refundPayment.InvoicePaymentDetails
                                            .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                                            try
                                            {
                                                nadaTapToPayDtoReturnResponse = JsonConvert.DeserializeObject<NadaPayTransactionDto>(refundDetail?.Value);
                                            }
                                            catch
                                            {
                                                App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                                return;
                                            }

                                            var payments = Invoice.InvoicePayments
                                                .Where(x => x.PaymentOptionId == refundPayment.PaymentOptionId);

                                            foreach (var payment in payments)
                                            {
                                                var paymentDetail = payment.InvoicePaymentDetails
                                                    .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                                                try
                                                {
                                                    var dto = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);
                                                    if (!string.IsNullOrEmpty(dto?.SaleTransactionId) &&
                                                    dto.SaleTransactionId.Contains(nadaTapToPayDtoReturnResponse.SaleTransactionId))
                                                    {
                                                        var refunded = dto.Amount;
                                                        refundedamt += refunded;
                                                    }
                                                }
                                                catch
                                                {
                                                    App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                                    return;
                                                }

                                            }

                                            refundSlider.ViewModel.InvoiceRefundPayments.Add(new RefundPaymentDto
                                            {
                                                Amount = refundPayment.Amount,
                                                ReturnAmount = refundedamt,
                                                TenderedAmount = (refundPayment.Amount - refundedamt).ToString($"N{Settings.StoreDecimalDigit}"),
                                                PrintPaymentOptionName = nadaTapToPayDtoReturnResponse?.MaskedPan,
                                                Id = refundPayment.Id,
                                            });
                                        }

                                        refundSlider.Selected_InvoiceRefundPayments += async (sender, e) =>
                                        {
                                            if (e != null)
                                            {
                                                var refundPayment = e as RefundPaymentDto;
                                                var Selected_RefundPayment = hikePayTerminalRefunds.FirstOrDefault(a => a.Id == refundPayment.Id);
                                                result = await HandlePartialRefund(paymentOption, Selected_RefundPayment, null, refundPayment, true);
                                                refundSlider.ViewModel.IsBusy = false;
                                                if (result)
                                                {
                                                    refundSlider?.Close();
                                                }
                                                await PaymentSuccessCheck(result, paymentOption);
                                            }
                                        };

                                        await NavigationService.PushModalAsync(refundSlider);
                                        return;
                                    }

                                    foreach (var refundPayment in hikePayTerminalRefunds)
                                    {
                                        // -----------------------------
                                        // 1) Get base authorized amount
                                        // -----------------------------
                                        var paymentDetail = refundPayment.InvoicePaymentDetails
                                            .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                                        try
                                        {
                                            hikePayTerminalResponse = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);
                                            lastReference = hikePayTerminalResponse?.SaleTransactionId;
                                            tempobject = "Sale: " + paymentOption.ConfigurationDetails + " tempInvoiceID: " + Invoice.InvoiceTempId + " Amount: " + amount.ToString() + "Last reference: " + lastReference + " Invoice.Note: " + Invoice.Note;
                                            authorizedAmount = hikePayTerminalResponse?.Amount ?? 0;
                                        }
                                        catch (Exception ex)
                                        {
                                            App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                            return;
                                        }

                                        var payments = Invoice.InvoicePayments
                                            .Where(x => x.PaymentOptionId == refundPayment.PaymentOptionId);

                                        foreach (var payment in payments)
                                        {
                                            paymentDetail = payment.InvoicePaymentDetails
                                                .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                                            try
                                            {
                                                var dto = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);
                                                if (!string.IsNullOrEmpty(dto?.SaleTransactionId) &&
                                                 dto.SaleTransactionId.Contains(hikePayTerminalResponse.SaleTransactionId))
                                                {
                                                    var refunded = dto.Amount;

                                                    authorizedAmount -= refunded;
                                                }
                                            }
                                            catch
                                            {
                                                App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                                return;
                                            }

                                        }

                                        if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                                            break;
                                    }

                                    if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                                    {
                                        SaveInLocalbeforePayment(tempobject, true);


                                        var isPartial = hikePayTerminalResponse?.Amount > requestedAmount ? true : false;
                                        var poiTransactionId = hikePayTerminalResponse?.PoiTransactionId;
                                        var poiTransactionTimeStamp = hikePayTerminalResponse.PoiTransactionTimeStamp ?? DateTime.Now;
                                        var merchantReference = hikePayTerminalResponse?.SaleTransactionId;
                                        var surchargeAmount = hikePayTerminalResponse?.SurchargeAmount ?? 0;
                                        ApiService<IHikePayService> hikePayApiService = new ApiService<IHikePayService>();
                                        var hikePayService = new HikePayService(hikePayApiService);
                                        var hikePaySplitProfile = hikePayService.GetLocalHikePaySplitProfile();
                                        var rule = hikePaySplitProfile?.Rules?
                                            .FirstOrDefault(x => x.PaymentMethod == hikePayTerminalResponse?.PaymentMethod)
                                            ?? hikePaySplitProfile?.Rules?
                                                .FirstOrDefault(x => x.PaymentMethod.ToLower() == "any");
                                        if (rule == null)
                                        {
                                            App.Instance.Hud.DisplayToast("Please contact to HikePay support team", Colors.Red, Colors.White);
                                            return ;
                                        }
                                        var fixedCommission = rule.SplitLogic?.Commission?.FixedAmount ?? 0;
                                        var variableCommissionPercentage = rule.SplitLogic?.Commission?.VariablePercentage ?? 0;
                                        var paymentFee = rule.SplitLogic?.PaymentFee ?? 0;
                                        saleResult = await HikeTerminalPay.CreateNadaPayTerminalRefund(isPartial,
                                            requestedAmount, Invoice.Currency, Guid.NewGuid().ToString(), merchantReference, poiTransactionId, poiTransactionTimeStamp, surchargeAmount, fixedCommission, variableCommissionPercentage, paymentFee, paymentOption, hikePayTerminalResponse);
                                        IsBusy = false;
                                        // saleResult = await nadaTapToPay.Refund(isPartial,
                                        //    requestedAmount,Invoice.Currency, Guid.NewGuid().ToString(), merchantReference, poiTransactionId, poiTransactionTimeStamp, config.BalanceAccountId, surchargeAmount, fixedCommission,variableCommissionPercentage, paymentFee);

                                    }
                                    else
                                    {
                                        App.Instance.Hud.DisplayToast(
                                            $"Refund amount for {paymentOption.Name} should be equal or less than the original sale amount",
                                            Colors.Red, Colors.White
                                        );
                                        return;
                                    }
                                }

                                if (saleResult?.DecodeAdditonalResponse != null && saleResult.PaymentStatusSuccess)
                                {
                                    var merchantReceipt = string.Empty;
                                    var customerReceipt = string.Empty;

                                    if (!string.IsNullOrEmpty(saleResult.CustomerReceipt))
                                    {
                                        customerReceipt = saleResult.CustomerReceipt;
                                        if (config.IsPrintCustomerReceipt)
                                            VantivCloudReceiptList.Add(saleResult.CustomerReceipt);
                                    }
                                    if (!string.IsNullOrEmpty(saleResult.MerchantReceipt))
                                    {
                                        merchantReceipt = saleResult.MerchantReceipt;
                                        if (config.IsPrintMerchantReceipt)
                                            VantivCloudReceiptList.Add(saleResult.MerchantReceipt);
                                    }

                                    var transactionResult = JsonConvert.SerializeObject(saleResult, new JsonSerializerSettings
                                    {
                                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                                    });
                                    var amt = Convert.ToDecimal(saleResult?.DecodeAdditonalResponse?.additionalData.posAuthAmountValue ?? "0") / 100m;
                                    var surchargeamt = Convert.ToDecimal(saleResult?.DecodeAdditonalResponse?.additionalData.surchargeAmount ?? "0") / 100m;
                                    var nadaPayTransactionDto = new NadaPayTransactionDto
                                    {
                                        PoiTransactionId = saleResult?.PoiTransactionId,
                                        PoiTransactionTimeStamp = saleResult?.PoiTransactionTimeStamp,
                                        SaleTransactionId = saleResult?.DecodeAdditonalResponse?.additionalData?.merchantReference,
                                        SaleTransactionTimeStamp = saleResult?.DecodeAdditonalResponse?.additionalData?.iso8601TxDate,
                                        PspReference = saleResult?.DecodeAdditonalResponse?.additionalData?.pspReference,
                                        Amount = amt,
                                        MaskedPan = saleResult?.DecodeAdditonalResponse?.additionalData?.cardSummary ?? "Card",
                                        SurchargeAmount = surchargeamt,
                                        PaymentMethod = saleResult?.DecodeAdditonalResponse?.additionalData?.paymentMethod
                                    };

                                    ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>
                                    {
                                        new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePayResponse, Value = transactionResult },
                                        new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePaySaleResponseData, Value = JsonConvert.SerializeObject(nadaPayTransactionDto,new JsonSerializerSettings
                                        {
                                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                                        }) },
                                        new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayMerchantPrint, Value = merchantReceipt },
                                        new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayCustomerPrint, Value = customerReceipt }
                                    };
                                    result = await addIntegratedPaymentToSell(SelectedPaymentOption, transactionResult, InvoicePaymentDetails);
                                    if (result)
                                    {
                                        Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                                        if (Invoice.TotalPay == 0)
                                        {
                                            IsAddPaymentActive = !result;
                                            IsSuccessPaymentActive = result;
                                        }
                                        else
                                        {
                                            if (Invoice.NetAmount > Invoice.TotalPay)
                                            {
                                                IsPayment_PartialyPaid = true;
                                            }
                                            else
                                            {
                                                IsPayment_PartialyPaid = false;
                                            }
                                        }
                                    }
                                    Logger.SyncLogger("----Hikepay Status---\n" + "Success" + "\n----HikepayTapToPay response---\n" + transactionResult);
                                }
                                else if (saleResult?.RefusalReason != null && !saleResult.PaymentStatusSuccess)
                                {
                                    result = false;
                                    App.Instance.Hud.DisplayToast(saleResult.RefusalReason, Colors.Red, Colors.White);
                                    var transactionResult = "Empty";
                                    if (saleResult != null)
                                        transactionResult = JsonConvert.SerializeObject(saleResult);
                                    Logger.SyncLogger("----HikeTerminalpay Status---\n" + "Failed" + "\n----HikeTerminalpay response---\n" + transactionResult);
                                }
                                else
                                {
                                    result = false;
                                    var transactionResult = "Empty";
                                    if (saleResult != null)
                                        transactionResult = JsonConvert.SerializeObject(saleResult);
                                    Logger.SyncLogger("----HikeTerminalpay Status---\n" + "Failed" + "\n----HikeTerminalpay response---\n" + transactionResult);
                                }
                            }
                            else
                            {
                                result = false;
                                App.Instance.Hud.DisplayToast("Hikepay transaction is not allowed");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.SyncLogger("----HikeTerminalpay Status---\n" + "Failed" + "\n---- HikeTerminalpay response---\n" + ex.Message);
                        }

                    }
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.Cash || paymentOption.PaymentOptionType == Enums.PaymentOptionType.Card)
                    {
                        result = await addPaymentToSell(paymentOption, "");
                    }

                    await PaymentSuccessCheck(result, paymentOption);
                }
                catch (Exception ex)
                {
                    Logger.SaleLogger("Exception Msg - " + ex.Message);
                    ex.Track();
                }
                finally
                {
                    isClicked = false;
                }
            }
            //IsLoad = false;
        }


        private async void SaveInLocalbeforePayment(string paymentObject, bool isRefund)
        {

            if (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
            {
                return;
            }
            string currentPaymentObject = string.Empty;
            if (isRefund)
                currentPaymentObject = "NadaPay Refund : ";
            else
                currentPaymentObject = "NadaPay Sale :  ";


            currentPaymentObject = currentPaymentObject + " : " + paymentObject;
            Invoice.CurrentPaymentObject = currentPaymentObject;
            Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;
            await saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.PaymentProcessing);

        }

        async Task<bool> HandlePartialRefund(PaymentOptionDto paymentOption, InvoicePaymentDto Selected_RefundPayment, object[] extraParameters, RefundPaymentDto refundPaymentDto = null, bool isadyen = false)
        {
            if (isadyen)
            {
                return await HandlePartialRefundCall(paymentOption, Selected_RefundPayment, extraParameters, refundPaymentDto);
            }
            else
            {
                using (new Busy(this, false))
                {
                    return await HandlePartialRefundCall(paymentOption, Selected_RefundPayment, extraParameters, refundPaymentDto);
                }
            }

        }

        async Task<bool> HandlePartialRefundCall(PaymentOptionDto paymentOption, InvoicePaymentDto Selected_RefundPayment, object[] extraParameters, RefundPaymentDto refundPaymentDto = null)
        {
            var result = false;
            try
            {
                logRequestDetails.Clear();
                if (Selected_RefundPayment != null)
                {
                    paymentOption.Name = Selected_RefundPayment.PaymentOptionName;
                    paymentOption.PaymentOptionName = Selected_RefundPayment.PaymentOptionName;
                    paymentOption.PaymentOptionType = Selected_RefundPayment.PaymentOptionType;

                    Invoice.TenderAmount = refundPaymentDto != null ? -Convert.ToDecimal(refundPaymentDto.TenderedAmount) : -Selected_RefundPayment.Amount;
                    if (Selected_RefundPayment.PaymentOptionType == Enums.PaymentOptionType.GiftCard)
                    {
                        result = await addGiftCardPayment(paymentOption, Payment_GiftCardNumber);
                    }
                    else if (Selected_RefundPayment.PaymentOptionType == Enums.PaymentOptionType.Loyalty)
                    {
                        result = await LoyaltyPayment(paymentOption);
                    }
                    else if (Selected_RefundPayment.PaymentOptionType == Enums.PaymentOptionType.Cash || Selected_RefundPayment.PaymentOptionType == Enums.PaymentOptionType.Card)
                    {
                        result = await addPaymentToSell(paymentOption, "");
                    }
                    else if (Selected_RefundPayment.PaymentOptionType == PaymentOptionType.HikePayTapToPay)
                    {
                        decimal amount = Convert.ToDecimal(refundPaymentDto.TenderedAmount);
                        NadaTapToPayDto saleResult;
                        string tempobject = string.Empty;
                        NadaPayTransactionDto nadaTapToPayDtoResponse = null;
                        decimal authorizedAmount = 0;
                        decimal requestedAmount = amount.ToPositive();

                        // -----------------------------
                        // 1) Get base authorized amount
                        // -----------------------------
                        var paymentDetail = Selected_RefundPayment.InvoicePaymentDetails
                            .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                        try
                        {
                            nadaTapToPayDtoResponse = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);
                            // if (nadaTapToPayDtoResponse.SaleToPOIResponse == null && (int)Selected_RefundPayment.PaymentOptionType == 49)
                            // {
                            //     var hikePayDto = JsonConvert.DeserializeObject<HikePayFromCloud>(paymentDetail?.Value);
                            //     nadaTapToPayDtoResponse = HikePayFromCloud.MapToNada(hikePayDto);
                            // }
                            var lastReference = nadaTapToPayDtoResponse
                                    .SaleTransactionId;
                            tempobject = "Sale: " + paymentOption.ConfigurationDetails + " tempInvoiceID: " + Invoice.InvoiceTempId + " Amount: " + amount.ToString() + "Last reference: " + lastReference + " Invoice.Note: " + Invoice.Note;

                            authorizedAmount = nadaTapToPayDtoResponse?
                                .Amount ?? 0;
                        }
                        catch
                        {
                            App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                            return false;
                        }

                        // -----------------------------
                        // 2) Subtract amounts from related payments
                        // -----------------------------
                        var payments = Invoice.InvoicePayments
                            .Where(x => x.PaymentOptionId == Selected_RefundPayment.PaymentOptionId);

                        foreach (var payment in payments)
                        {
                            paymentDetail = payment.InvoicePaymentDetails
                                .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                            try
                            {
                                var dto = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);
                                // if (dto.SaleToPOIResponse == null && (int)payment.PaymentOptionType == 49)
                                // {
                                //     var hikePayDto = JsonConvert.DeserializeObject<HikePayFromCloud>(paymentDetail?.Value);
                                //     dto = HikePayFromCloud.MapToNada(hikePayDto);
                                // }
                                if (!string.IsNullOrEmpty(dto?.SaleTransactionId) &&
                                    dto.SaleTransactionId.Contains(nadaTapToPayDtoResponse.SaleTransactionId))
                                {
                                    var refunded = dto.Amount;
                                    authorizedAmount -= refunded;
                                }
                            }
                            catch
                            {
                                App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                return false;
                            }
                        }
                        var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
                        if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                        {
                            // Save before proceeding
                            SaveInLocalbeforePayment(tempobject, true);
                            // Exact match refund
                            var isPartial = nadaTapToPayDtoResponse?.Amount > requestedAmount ? true : false;
                            var poiTransactionId = nadaTapToPayDtoResponse?.PoiTransactionId;
                            var poiTransactionTimeStamp = nadaTapToPayDtoResponse.PoiTransactionTimeStamp.Value;
                            var merchantReference = nadaTapToPayDtoResponse?.SaleTransactionId;
                            var surchargeAmount = nadaTapToPayDtoResponse?.SurchargeAmount ?? 0;
                            ApiService<IHikePayService> hikePayApiService = new ApiService<IHikePayService>();
                            var hikePayService = new HikePayService(hikePayApiService);
                            var hikePaySplitProfile = hikePayService.GetLocalHikePaySplitProfile();
                            var rule = hikePaySplitProfile?.Rules?
                                .FirstOrDefault(x => x.PaymentMethod == nadaTapToPayDtoResponse?.PaymentMethod)
                                ?? hikePaySplitProfile?.Rules?
                                    .FirstOrDefault(x => x.PaymentMethod.ToLower() == "any");
                            if (rule == null)
                            {
                                App.Instance.Hud.DisplayToast("Please contact to HikePay support team", Colors.Red, Colors.White);
                                return false;
                            }
                            var fixedCommission = rule.SplitLogic?.Commission?.FixedAmount ?? 0;
                            var variableCommissionPercentage = rule.SplitLogic?.Commission?.VariablePercentage ?? 0;
                            var paymentFee = rule.SplitLogic?.PaymentFee ?? 0;
                            saleResult = await nadaTapToPay.Refund(isPartial,
                            requestedAmount, Invoice.Currency, Guid.NewGuid().ToString(), merchantReference, poiTransactionId, poiTransactionTimeStamp, config.BalanceAccountId, surchargeAmount, fixedCommission, variableCommissionPercentage, paymentFee);
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(
                                $"Refund amount for {paymentOption.Name} should be equal or less than the original sale amount",
                                Colors.Red, Colors.White
                            );
                            return false;
                        }
                        if (saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.Result != null && saleResult.SaleToPOIResponse.TransactionResponse.Response.Result.ToLower().Contains("success"))
                        {

                            var merchantReceipt = string.Empty;
                            var customerReceipt = string.Empty;
                            foreach (var doc in saleResult?.SaleToPOIResponse?.TransactionResponse?.PaymentReceipt ?? new List<PaymentReceipt>())
                            {
                                var receipt = CommonMethods.HikePayReceiptBuilder.BuildReceipt(doc);
                                bool isCustomerReceipt = doc.DocumentQualifier == "CustomerReceipt";
                                if (isCustomerReceipt)
                                {
                                    customerReceipt = receipt;
                                    if (config.IsPrintCustomerReceipt)
                                        VantivCloudReceiptList.Add(receipt);
                                }
                                else
                                {
                                    merchantReceipt = receipt;
                                    if (config.IsPrintMerchantReceipt)
                                        VantivCloudReceiptList.Add(receipt);
                                }
                            }
                            var transactionResult = JsonConvert.SerializeObject(saleResult, new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            });
                            var nadaPayTransactionDto = new NadaPayTransactionDto
                            {
                                PoiTransactionId = saleResult?.SaleToPOIResponse?.TransactionResponse?.POIData?.POITransactionID?.TransactionID,
                                PoiTransactionTimeStamp = saleResult?.SaleToPOIResponse?.TransactionResponse?.POIData?.POITransactionID?.TimeStamp,
                                SaleTransactionId = saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.MerchantReference,
                                SaleTransactionTimeStamp = saleResult?.SaleToPOIResponse?.TransactionResponse?.SaleData?.SaleTransactionID?.TimeStamp,
                                PspReference = saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.PSPReference,
                                Amount = saleResult?.SaleToPOIResponse?.PaymentResponse != null ?
                                saleResult?.SaleToPOIResponse?.TransactionResponse?.PaymentResult?
                                .AmountsResp?.AuthorizedAmount ?? 0 : saleResult?.SaleToPOIResponse?.TransactionResponse?.ReversedAmount ?? 0,
                                MaskedPan = saleResult?.SaleToPOIResponse?.PaymentResponse?.PaymentResult?.PaymentInstrumentData?.CardData?.MaskedPan ?? "Card",
                                SurchargeAmount = saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.SurchargeAmount ?? 0,
                                PaymentMethod = saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.PaymentMethod
                            };

                            ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>
                                {
                                    new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePayResponse, Value = transactionResult },
                                    new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePaySaleResponseData, Value = JsonConvert.SerializeObject(nadaPayTransactionDto,new JsonSerializerSettings
                                    {
                                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                                    }) },
                                    new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayMerchantPrint, Value = merchantReceipt },
                                    new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayCustomerPrint, Value = customerReceipt }
                                };
                            result = await addIntegratedPaymentToSell(SelectedPaymentOption, transactionResult, InvoicePaymentDetails);
                            if (result)
                            {
                                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                                if (Invoice.TotalPay == 0)
                                {
                                    IsAddPaymentActive = !result;
                                    IsSuccessPaymentActive = result;
                                }
                                else
                                {
                                    if (Invoice.NetAmount > Invoice.TotalPay)
                                    {
                                        IsPayment_PartialyPaid = true;
                                    }
                                    else
                                    {
                                        IsPayment_PartialyPaid = false;
                                    }
                                }
                            }
                            Logger.SyncLogger("----Hikepay Status---\n" + "Success" + "\n----HikepayTapToPay response---\n" + transactionResult);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.ErrorCondition))
                                App.Instance.Hud.DisplayToast("Transaction Failed", Colors.Red, Colors.White);
                            else
                                App.Instance.Hud.DisplayToast(saleResult.SaleToPOIResponse.TransactionResponse.Response.ErrorMessage, Colors.Red, Colors.White);

                            var transactionResult = "Empty";
                            if (saleResult != null)
                                transactionResult = JsonConvert.SerializeObject(saleResult);
                            Logger.SyncLogger("----Hikepay Status---\n" + "Failed" + "\n----HikepayTapToPay response---\n" + transactionResult);
                            return false;
                        }
                    }
                    else if (Selected_RefundPayment.PaymentOptionType == PaymentOptionType.HikePay)
                    {
                        decimal amount = Convert.ToDecimal(refundPaymentDto.TenderedAmount);
                        HikePayTerminalResponse saleResult;
                        string tempobject = string.Empty;
                        NadaPayTransactionDto nadaTapToPayDtoResponse = null;
                        decimal authorizedAmount = 0;
                        decimal requestedAmount = amount.ToPositive();

                        // -----------------------------
                        // 1) Get base authorized amount
                        // -----------------------------
                        var paymentDetail = Selected_RefundPayment.InvoicePaymentDetails
                            .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                        try
                        {
                            nadaTapToPayDtoResponse = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);

                            var lastReference = nadaTapToPayDtoResponse
                                    .SaleTransactionId;
                            tempobject = "Sale: " + paymentOption.ConfigurationDetails + " tempInvoiceID: " + Invoice.InvoiceTempId + " Amount: " + amount.ToString() + "Last reference: " + lastReference + " Invoice.Note: " + Invoice.Note;

                            authorizedAmount = nadaTapToPayDtoResponse?
                                .Amount ?? 0;
                        }
                        catch
                        {
                            App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                            return false;
                        }

                        // -----------------------------
                        // 2) Subtract amounts from related payments
                        // -----------------------------
                        var payments = Invoice.InvoicePayments
                            .Where(x => x.PaymentOptionId == Selected_RefundPayment.PaymentOptionId);

                        foreach (var payment in payments)
                        {
                            paymentDetail = payment.InvoicePaymentDetails
                                .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                            try
                            {
                                var dto = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);

                                if (!string.IsNullOrEmpty(dto?.SaleTransactionId) &&
                                    dto.SaleTransactionId.Contains(nadaTapToPayDtoResponse.SaleTransactionId))
                                {
                                    var refunded = dto.Amount;
                                    authorizedAmount -= refunded;
                                }
                            }
                            catch
                            {
                                App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                return false;
                            }
                        }
                        var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
                        if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                        {
                            // Save before proceeding
                            SaveInLocalbeforePayment(tempobject, true);
                            // Exact match refund
                            var isPartial = nadaTapToPayDtoResponse?.Amount > requestedAmount ? true : false;
                            var poiTransactionId = nadaTapToPayDtoResponse?.PoiTransactionId;
                            var poiTransactionTimeStamp = nadaTapToPayDtoResponse.PoiTransactionTimeStamp.Value;
                            var merchantReference = nadaTapToPayDtoResponse?.SaleTransactionId;
                            var surchargeAmount = nadaTapToPayDtoResponse?.SurchargeAmount ?? 0;
                            ApiService<IHikePayService> hikePayApiService = new ApiService<IHikePayService>();
                            var hikePayService = new HikePayService(hikePayApiService);
                            var hikePaySplitProfile = hikePayService.GetLocalHikePaySplitProfile();
                            var rule = hikePaySplitProfile?.Rules?
                                .FirstOrDefault(x => x.PaymentMethod == nadaTapToPayDtoResponse?.PaymentMethod)
                                ?? hikePaySplitProfile?.Rules?
                                    .FirstOrDefault(x => x.PaymentMethod.ToLower() == "any");
                            if (rule == null)
                            {
                                App.Instance.Hud.DisplayToast("Please contact to HikePay support team", Colors.Red, Colors.White);
                                return false;
                            }
                            var fixedCommission = rule.SplitLogic?.Commission?.FixedAmount ?? 0;
                            var variableCommissionPercentage = rule.SplitLogic?.Commission?.VariablePercentage ?? 0;
                            var paymentFee = rule.SplitLogic?.PaymentFee ?? 0;
                            saleResult = await HikeTerminalPay.CreateNadaPayTerminalRefund(isPartial,
                            requestedAmount, Invoice.Currency, Guid.NewGuid().ToString(), merchantReference, poiTransactionId, poiTransactionTimeStamp, surchargeAmount, fixedCommission, variableCommissionPercentage, paymentFee, paymentOption, nadaTapToPayDtoResponse);
                            //saleResult = await nadaTapToPay.Refund(isPartial,
                            //requestedAmount,Invoice.Currency, Guid.NewGuid().ToString(), merchantReference, poiTransactionId, poiTransactionTimeStamp,config.BalanceAccountId, surchargeAmount, fixedCommission,variableCommissionPercentage, paymentFee);
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(
                                $"Refund amount for {paymentOption.Name} should be equal or less than the original sale amount",
                                Colors.Red, Colors.White
                            );
                            return false;
                        }
                        if (saleResult?.DecodeAdditonalResponse != null && saleResult.PaymentStatusSuccess)
                        {
                            var merchantReceipt = string.Empty;
                            var customerReceipt = string.Empty;
                            if (!string.IsNullOrEmpty(saleResult.CustomerReceipt))
                            {
                                customerReceipt = saleResult.CustomerReceipt;
                                if (config.IsPrintCustomerReceipt)
                                    VantivCloudReceiptList.Add(saleResult.CustomerReceipt);
                            }
                            if (!string.IsNullOrEmpty(saleResult.MerchantReceipt))
                            {
                                merchantReceipt = saleResult.MerchantReceipt;
                                if (config.IsPrintMerchantReceipt)
                                    VantivCloudReceiptList.Add(saleResult.MerchantReceipt);
                            }

                            var transactionResult = JsonConvert.SerializeObject(saleResult, new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            });

                            // var transactionResult = JsonConvert.SerializeObject(saleResult);
                            var amt = Convert.ToDecimal(saleResult?.DecodeAdditonalResponse?.additionalData.posAuthAmountValue ?? "0") / 100m;
                            var surchargeamt = Convert.ToDecimal(saleResult?.DecodeAdditonalResponse?.additionalData.surchargeAmount ?? "0") / 100m;
                            var nadaPayTransactionDto = new NadaPayTransactionDto
                            {
                                PoiTransactionId = saleResult?.PoiTransactionId,
                                PoiTransactionTimeStamp = saleResult?.PoiTransactionTimeStamp,
                                SaleTransactionId = saleResult?.DecodeAdditonalResponse?.additionalData?.merchantReference,
                                SaleTransactionTimeStamp = saleResult?.DecodeAdditonalResponse?.additionalData?.iso8601TxDate,
                                PspReference = saleResult?.DecodeAdditonalResponse?.additionalData?.pspReference,
                                Amount = amt,
                                MaskedPan = saleResult?.DecodeAdditonalResponse?.additionalData?.cardSummary ?? "Card",
                                SurchargeAmount = surchargeamt,
                                PaymentMethod = saleResult?.DecodeAdditonalResponse?.additionalData?.paymentMethod
                            };

                            ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>
                                {
                                    new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePayResponse, Value = transactionResult },
                                    new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePaySaleResponseData, Value = JsonConvert.SerializeObject(nadaPayTransactionDto,new JsonSerializerSettings
                                    {
                                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                                    }) },
                                    new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayMerchantPrint, Value = merchantReceipt },
                                    new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayCustomerPrint, Value = customerReceipt }
                                };
                            result = await addIntegratedPaymentToSell(SelectedPaymentOption, transactionResult, InvoicePaymentDetails);
                            if (result)
                            {
                                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                                if (Invoice.TotalPay == 0)
                                {
                                    IsAddPaymentActive = !result;
                                    IsSuccessPaymentActive = result;
                                }
                                else
                                {
                                    if (Invoice.NetAmount > Invoice.TotalPay)
                                    {
                                        IsPayment_PartialyPaid = true;
                                    }
                                    else
                                    {
                                        IsPayment_PartialyPaid = false;
                                    }
                                }
                            }
                            Logger.SyncLogger("----Hikepay Status---\n" + "Success" + "\n----HikepayTapToPay response---\n" + transactionResult);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(saleResult?.RefusalReason))
                                App.Instance.Hud.DisplayToast("Transaction Failed", Colors.Red, Colors.White);
                            else
                                App.Instance.Hud.DisplayToast(saleResult?.RefusalReason, Colors.Red, Colors.White);

                            var transactionResult = "Empty";
                            if (saleResult != null)
                                transactionResult = JsonConvert.SerializeObject(saleResult);
                            Logger.SyncLogger("----Hikepay Status---\n" + "Failed" + "\n----HikepayTapToPay response---\n" + transactionResult);
                            return false;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Logger.SaleLogger("HandlePartialRefund Exception Msg - " + e.Message);
                Debug.WriteLine("Exception in HandlePartialRefund :" + e.Message + " : " + e.StackTrace);
            }
            return result;
        }

        async Task<bool> HandleClearantPartialRefund(PaymentOptionDto paymentOption, InvoicePaymentDto Selected_RefundPayment, object[] extraParameters)
        {
            var result = false;
            try
            {
                logRequestDetails.Clear();
                using (new Busy(this, false))
                {
                    if (Selected_RefundPayment != null)
                    {

                        paymentOption.Name = Selected_RefundPayment.PaymentOptionName;
                        paymentOption.PaymentOptionName = Selected_RefundPayment.PaymentOptionName;
                        paymentOption.PaymentOptionType = Selected_RefundPayment.PaymentOptionType;

                        Invoice.TenderAmount = -Selected_RefundPayment.Amount;
                        if (Selected_RefundPayment.PaymentOptionType == Enums.PaymentOptionType.GiftCard)
                        {
                            result = await addGiftCardPayment(paymentOption, Payment_GiftCardNumber);
                        }
                        else if (Selected_RefundPayment.PaymentOptionType == Enums.PaymentOptionType.Loyalty)
                        {
                            result = await LoyaltyPayment(paymentOption);
                        }
                      
                    }
                }
            }
            catch (Exception e)
            {
                Logger.SaleLogger("HandleClearantPartialRefund Exception Msg - " + e.Message);
                Debug.WriteLine("Exception in HandlePartialRefund :" + e.Message + " : " + e.StackTrace);
            }
            return result;
        }

        async Task<bool> PaymentSuccessCheck(bool result, PaymentOptionDto paymentOption)
        {
            if (result)
            {

                var totalPaidAmmount = Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);
                Invoice.TotalPay = Invoice.NetAmount - totalPaidAmmount;
                Invoice = InvoiceCalculations.GetLoyalty(Invoice, Invoice.CustomerDetail);
                if (Convert.ToDouble(Invoice.TotalPay) < 0.005 && Convert.ToDouble(Invoice.TotalPay) > 0.00)
                {
                    Invoice.TotalPay = 0.00m;
                }

                if (Invoice.TotalPay <= 0.005m && Invoice.TotalPay >= -0.005m && Invoice.Status == InvoiceStatus.Completed)
                {
                    Invoice.TotalPay = 0.00m;
                }

                if (Invoice.TotalPay >= -0.010m && Invoice.Status == InvoiceStatus.Refunded)
                {
                    IsAddPaymentActive = !result;
                    IsSuccessPaymentActive = result;
                    Invoice.TotalPay = 0.00m;
                    await Task.Delay(10);
                }

                if (Invoice.TotalPay == 0)
                {
                    IsAddPaymentActive = !result;
                    IsSuccessPaymentActive = result;
                    await Task.Delay(10);
                }
                else
                {
                    if (Invoice.NetAmount > Invoice.TotalPay)
                    {
                        if (paymentOption.PaymentOptionType == PaymentOptionType.OnAccount || paymentOption.PaymentOptionType == PaymentOptionType.Credit || paymentOption.PaymentOptionType == PaymentOptionType.Loyalty)
                        {
                            updatePaymentList();
                        }
                        IsPayment_PartialyPaid = true;
                    }
                    else
                    {
                        IsPayment_PartialyPaid = false;
                    }
                }
                Debug.Write(Invoice.TenderAmount.ToString());

                if (Invoice.ReferenceInvoiceId != 0 && Invoice.Status == InvoiceStatus.Completed && Invoice.InvoiceHistories?.Count > 1 && Invoice.InvoiceHistories[Invoice.InvoiceHistories.Count - 2].Status == InvoiceStatus.Quote)
                {
                    var UpdatedInvoice = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                    UpdatedInvoice.FinancialStatus = FinancialStatus.Closed;
                    UpdatedInvoice.ReferenceNote = "Converted sale " + Invoice.Number;
                    await saleService.UpdateLocalInvoice(UpdatedInvoice);
                }
            }
            return result;
        }

        async Task<bool> LoyaltyPayment(PaymentOptionDto paymentOption)
        {
            bool result = false;
            //Ticket start:#91263 iOS:FR Redeeming Points.by rupesh
            string approverName = null;
            bool isHavingRequirePINRedeemPoints = Settings.GrantedPermissionNames != null && (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.RequirePINRedeemPoints"));
            if (isHavingRequirePINRedeemPoints)
            {
                approverName = await ApproveFromAdmin();
                if (approverName == null)
                    return result;
            }
            //Ticket end:#91263 .by rupesh
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                var customerresult = customerService.GetLocalCustomerById(CustomerModel.SelectedCustomer.Id);
                if (customerresult != null)//&& customerresult.result!=null)
                {
                    CustomerModel.SelectedCustomer = customerresult;//.result;
                }
                var Balance = Math.Round((CustomerModel.SelectedCustomer.CurrentLoyaltyBalance / Settings.StoreGeneralRule.LoyaltyPointsValue), 2);
                Invoice.CustomerCurrentLoyaltyPoints = CustomerModel.SelectedCustomer.CurrentLoyaltyBalance > 0 ? CustomerModel.SelectedCustomer.CurrentLoyaltyBalance : 0;
                Invoice.LoyaltyPoints = Balance;
                Invoice.LoyaltyPointsValue = Balance / Settings.StoreGeneralRule.LoyaltyPointsValue;
                if (Invoice.TenderAmount >= Balance)
                {
                    if (Balance != 0)
                    {
                        Invoice.TenderAmount = Balance;
                        CustomerModel.SelectedCustomer.CurrentLoyaltyBalance = 0;
                        Invoice.CustomerDetail.CurrentLoyaltyBalance = CustomerModel.SelectedCustomer.CurrentLoyaltyBalance;
                        customerService.UpdateLocalCustomer(CustomerModel.SelectedCustomer);

                        result = await addPaymentToSell(paymentOption, approverName);

                        bool isHavingLoyaltyPayPermission = Settings.Subscription != null && Settings.Subscription.Edition != null && Settings.Subscription.Edition.PlanType != null && Settings.Subscription.Edition.PlanType != PlanType.StartUp;
                        if (isHavingLoyaltyPayPermission && CustomerModel.SelectedCustomer.CurrentLoyaltyBalance > 0)
                        {
                            var item = PaymentOptionList.FirstOrDefault(i => i.PaymentOptionType == PaymentOptionType.Loyalty);
                            if (item != null)
                            {
                                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                                var val = Math.Round((CustomerModel.SelectedCustomer.CurrentLoyaltyBalance / Settings.StoreGeneralRule.LoyaltyPointsValue), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                                if (val < 0)
                                {
                                    item.DisplayName = getCardName(item.Name);
                                    item.DisplaySubName = val.ToString("C");
                                }
                                else
                                {
                                    item.DisplayName = getCardName(item.Name);
                                    item.DisplaySubName = val.ToString("C");
                                }
                                //Endd ticket #73190
                            }
                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("LoyaltyLimitValidationMessage"));
                        return result;
                    }
                }
                else
                {
                    CustomerModel.SelectedCustomer.CurrentLoyaltyBalance -= Math.Round(Invoice.TenderAmount * Settings.StoreGeneralRule.LoyaltyPointsValue, 2);
                    Invoice.CustomerDetail.CurrentLoyaltyBalance = CustomerModel.SelectedCustomer.CurrentLoyaltyBalance;
                    customerService.UpdateLocalCustomer(CustomerModel.SelectedCustomer);

                    result = await addPaymentToSell(paymentOption, approverName);
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return result;
            }
            return result;
        }

        //Ticket start:#91263 iOS:FR Redeeming Points.by rupesh
        async Task<string> ApproveFromAdmin()
        {
            var tcs = new TaskCompletionSource<string>();
            if (ApproveAdminPage == null)
            {
                ApproveAdminPage = new ApproveAdminPage();
                ApproveAdminPage.ViewModel.IsDescriptionNotShown = true;
                ApproveAdminPage.ViewModel.Users = new ObservableCollection<UserListDto>(ApproveAdminPage.ViewModel.Users);

            }
            ApproveAdminPage.SelectedUser -= ApproveAdminPage.SelectedUser;
            ApproveAdminPage.ClosedPopUp -= ApproveAdminPage.ClosedPopUp;
            ApproveAdminPage.SelectedUser += async (object sender, UserListDto e) =>
            {
                await ApproveAdminPage.Close();
                tcs.SetResult(e.DisplayName);
            };
            ApproveAdminPage.ClosedPopUp += (object sender, bool e) =>
            {
                tcs.SetResult(null);
            };

            await _navigationService.GetCurrentPage.Navigation.PushModalAsync(ApproveAdminPage);
            return await tcs.Task;
        }
        //Ticket end:#91263 .by rupesh

        void QuickCashOption(decimal selectedQuickCashOption)
        {
            Invoice.StrTenderAmount = selectedQuickCashOption.ToString();
        }

        public async Task<bool> addIntegratedPaymentToSell(PaymentOptionDto paymentType, string transactionResult, ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails, string eConduitORAfterPayRefID = null)
        {
            try
            {
                string tempPrintPaymetName = paymentType.Name;

                if (Extensions.IseConduitPayment(paymentType.PaymentOptionType))
                {
                    paymentType.Name = paymentType.Name + "(" + eConduitORAfterPayRefID + ")";
                    tempPrintPaymetName = paymentType.Name + "(" + eConduitORAfterPayRefID + ")";

                }
                else if (paymentType.PaymentOptionType == Enums.PaymentOptionType.Afterpay || paymentType.PaymentOptionType == Enums.PaymentOptionType.Zip
                    || paymentType.PaymentOptionType == Enums.PaymentOptionType.Linkly)
                {
                    tempPrintPaymetName = paymentType.Name + "(#" + eConduitORAfterPayRefID + ")";
                }

                string temppaymetGuid = Guid.NewGuid().ToString();
                InvoicePaymentDto payment = new InvoicePaymentDto()
                {
                    PaymentOptionId = paymentType.Id,
                    SyncPaymentReference = temppaymetGuid,
                    PaymentOptionName = paymentType.Name,
                    PaymentOptionType = paymentType.PaymentOptionType,
                    PrintPaymentOptionName = tempPrintPaymetName
                };

                var outstanding = Invoice.NetAmount.ToPositive() - Invoice.TotalPaid.ToPositive();

                if (Settings.StoreCulture == "ja-jp")
                    Invoice.NetAmount = Math.Round(Invoice.NetAmount, 0, MidpointRounding.AwayFromZero);
                else
                    //Start Ticket #75526 Myob rounding issue By Pratik
                    Invoice.NetAmount = Math.Round(Invoice.NetAmount, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                //End Ticket #75526 By Pratik

                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                var amount = Invoice.TenderAmount;
                if ((Invoice.Status == InvoiceStatus.Refunded || (Invoice.Status == InvoiceStatus.Exchange && Invoice.TotalPay <= 0)))
                {
                    if (paymentType.CanApplySurchageOnRefund)
                    {
                        var surcharge = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == payment.PaymentOptionId && a.InvoicePaymentDetails != null && a.InvoicePaymentDetails.Count > 0)
                                    .Sum(a => a.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount)
                                    .Sum(a => Convert.ToDecimal(a.Value ?? "0").ToPositive()));

                        var totaltender = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == payment.PaymentOptionId)
                        .Sum(a => a.Amount) - surcharge;
                        if (totaltender.ToPositive() > 0)
                        {
                            var percentage = Math.Round(((surcharge * 100) / totaltender), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);

                            if (Invoice.TenderAmount.ToPositive() <= totaltender)
                                surcharge = Math.Round(((Invoice.TenderAmount * percentage) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        }
                        if (surcharge > 0)
                            surcharge = surcharge.ToNegative();
                        if (surcharge.ToPositive() > 0)
                            InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { InvoicePaymentID = payment.Id, InvoiceId = payment.InvoiceId, Key = InvoicePaymentKey.SurchargeAmount, Value = surcharge.ToString("0.####") });
                        amount = amount + surcharge;
                        Invoice.TotalPaymentSurcharge += surcharge;
                        Invoice.TotalTipSurcharge += surcharge;
                        Invoice.NetAmount += surcharge;
                        IsLblTipEnabled = true;
                    }
                }
                //Ticket start:#75743 Exchanges not working at Great World outlet.by rupesh
                else if (paymentType.Surcharge.HasValue && ((Invoice.Status != InvoiceStatus.Refunded && Invoice.Status != InvoiceStatus.Exchange) || (Invoice.Status == InvoiceStatus.Exchange && Invoice.TotalPay > 0)))
                {
                    //Ticket end:#75743 .by rupesh
                    decimal surcharge = 0;
                    if (outstanding > amount)
                        surcharge = Math.Round(((amount * paymentType.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                    else
                        surcharge = Math.Round(((outstanding * paymentType.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);

                    InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { InvoicePaymentID = payment.Id, InvoiceId = payment.InvoiceId, Key = InvoicePaymentKey.SurchargeAmount, Value = surcharge.ToString("0.####") });
                    amount = amount + surcharge;
                    Invoice.TotalPaymentSurcharge += surcharge;
                    Invoice.TotalTipSurcharge += surcharge;
                    Invoice.NetAmount += surcharge;
                    IsLblTipEnabled = true;

                }
                //End ticket #73190

                if (Invoice.TenderAmount.ToPositive() >= outstanding)
                {
                    payment.Amount = Invoice.NetAmount - Invoice.TotalPaid;
                }
                else
                {
                    //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                    payment.Amount = Invoice.TenderAmount + (paymentType.DisplaySurcharge ?? 0);
                    //End ticket #73190  By Rupesh
                }

                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                payment.TenderedAmount = Invoice.TenderAmount + (paymentType.DisplaySurcharge ?? 0);
                //End ticket #73190  By Rupesh
                payment.IsPaid = true;
                payment.RegisterId = Settings.CurrentRegister.Id;
                payment.RegisterName = Settings.CurrentRegister.Name;
                payment.OutletId = Settings.SelectedOutletId;
                payment.OutletName = Settings.SelectedOutletName;
                payment.ServedBy = Settings.CurrentUser.FullName;

                if (paymentType.PaymentOptionType == PaymentOptionType.TyroTapToPay &&
                    Invoice.Status != InvoiceStatus.Refunded)
                {
                    var transactionOutCome = JsonConvert.DeserializeObject<TyroTapToPayPaymentResponse>(transactionResult);
                    double surcharge = 0;
                    double.TryParse(transactionOutCome.surcharge, out surcharge);
                    var surchargeValue = (decimal)(surcharge / 100.0);
                    Invoice.TotalPaymentSurcharge += surchargeValue;
                    Invoice.TotalTipSurcharge += surchargeValue;
                    Invoice.NetAmount += surchargeValue;
                    Invoice.SubTotal += surchargeValue;
                    payment.Amount += surchargeValue;
                    IsLblTipEnabled = true;
                }
                else if (paymentType.PaymentOptionType == PaymentOptionType.HikePayTapToPay &&
                                   Invoice.Status != InvoiceStatus.Refunded)
                {
                    var transactionOutCome = JsonConvert.DeserializeObject<NadaTapToPayDto>(transactionResult);
                    if (transactionOutCome == null && paymentType.PaymentOptionType == PaymentOptionType.HikePay)
                    {
                        var hikePayDto = JsonConvert.DeserializeObject<HikePayFromCloud>(transactionResult);
                        transactionOutCome = HikePayFromCloud.MapToNada(hikePayDto);
                    }
                    decimal surcharge = transactionOutCome?.SaleToPOIResponse?.TransactionResponse?.Response?.SurchargeAmount ?? 0;
                    var surchargeValue = surcharge;
                    Invoice.TotalPaymentSurcharge += surchargeValue;
                    Invoice.TotalTipSurcharge += surchargeValue;
                    Invoice.NetAmount += surchargeValue;
                    Invoice.SubTotal += surchargeValue;
                    payment.Amount += surchargeValue;
                    IsLblTipEnabled = true;
                }
                else if (paymentType.PaymentOptionType == PaymentOptionType.HikePay &&
                                   Invoice.Status != InvoiceStatus.Refunded)
                {
                    var transactionOutCome = JsonConvert.DeserializeObject<HikePayTerminalResponse>(transactionResult);
                    decimal surcharge = Convert.ToDecimal(transactionOutCome?.DecodeAdditonalResponse?.additionalData.surchargeAmount ?? "0") / 100m;
                    var surchargeValue = surcharge;
                    InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { InvoicePaymentID = payment.Id, InvoiceId = payment.InvoiceId, Key = InvoicePaymentKey.SurchargeAmount, Value = surcharge.ToString("0.####") });
                    Invoice.TotalPaymentSurcharge += surchargeValue;
                    Invoice.TotalTipSurcharge += surchargeValue;
                    Invoice.NetAmount += surchargeValue;
                    Invoice.SubTotal += surchargeValue;
                    payment.Amount += surchargeValue;
                    IsLblTipEnabled = true;
                }
                payment.ActionType = Invoice.Status == InvoiceStatus.Refunded
                    ? ActionType.Refund
                    : ActionType.Sell;

                payment.PaymentDate = Extensions.moment();
                payment.PaymentFrom = DeviceInfo.Platform == DevicePlatform.iOS
                    ? InvoiceFrom.iPad
                    : InvoiceFrom.Android;

                payment.InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();

                if (
                    payment.PaymentOptionType == PaymentOptionType.PayPal ||
                    payment.PaymentOptionType == PaymentOptionType.PayPalhere ||
                    payment.PaymentOptionType == PaymentOptionType.Tyro ||
                    payment.PaymentOptionType == PaymentOptionType.iZettle ||
                    payment.PaymentOptionType == PaymentOptionType.Mint ||
                    payment.PaymentOptionType == PaymentOptionType.VantivIpad ||
                    payment.PaymentOptionType == PaymentOptionType.VantivCloud ||
                    payment.PaymentOptionType == PaymentOptionType.AssemblyPayment ||
                    payment.PaymentOptionType == PaymentOptionType.Afterpay ||
                    payment.PaymentOptionType == PaymentOptionType.Zip ||
                    payment.PaymentOptionType == PaymentOptionType.TD ||
                    payment.PaymentOptionType == PaymentOptionType.Elavon ||
                    payment.PaymentOptionType == PaymentOptionType.Square ||
                    Extensions.IseConduitPayment(payment.PaymentOptionType) ||
                    Extensions.IseLnklyPayment(payment.PaymentOptionType) ||
                    payment.PaymentOptionType == PaymentOptionType.Clearent ||
                    payment.PaymentOptionType == PaymentOptionType.Windcave ||
                    payment.PaymentOptionType == PaymentOptionType.Castle ||
                    payment.PaymentOptionType == PaymentOptionType.Clover ||
                    payment.PaymentOptionType == PaymentOptionType.TyroTapToPay ||
                    payment.PaymentOptionType == PaymentOptionType.HikePay ||
                    payment.PaymentOptionType == PaymentOptionType.HikePayTapToPay
                )
                {
                    payment.InvoicePaymentDetails = InvoicePaymentDetails;

                    // Optional: store transaction detail if needed
                    // payment.InvoicePaymentDetails.Add(new InvoicePaymentDetailDto
                    // {
                    //     Key = paymentKey,
                    //     Value = transactionResult,
                    // });
                }

                if (Invoice.InvoicePayments == null)
                {
                    Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                }

                if (paymentType.PaymentOptionType == PaymentOptionType.iZettle && (Invoice.Status != InvoiceStatus.Refunded && Invoice.Status != InvoiceStatus.Exchange))
                {
                    var transactionOutCome = JsonConvert.DeserializeObject<iZettlePaymentResult>(transactionResult);
                    var tip = Convert.ToDecimal(transactionOutCome.GratuityAmount / 100.0);
                    Invoice.TipValue += tip;
                    Invoice.TipIsAsPercentage = false;
                    Invoice.TotalTip += tip;
                    Invoice.NetAmount += tip;
                    Invoice.SubTotal += tip;
                    payment.Amount += tip;
                    payment.TenderedAmount += tip;
                }

                if (Invoice.Status == InvoiceStatus.Refunded)
                    payment.ActionType = ActionType.Refund;
                else
                    payment.ActionType = ActionType.Sell;

                payment.PaymentDate = Extensions.moment();
                payment.PaymentFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                payment.InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();

                if (payment.PaymentOptionType == PaymentOptionType.PayPal || payment.PaymentOptionType == PaymentOptionType.PayPalhere
                    || payment.PaymentOptionType == PaymentOptionType.Tyro || payment.PaymentOptionType == PaymentOptionType.iZettle
                    || payment.PaymentOptionType == PaymentOptionType.Mint || payment.PaymentOptionType == PaymentOptionType.VantivIpad
                    || payment.PaymentOptionType == PaymentOptionType.VantivCloud || payment.PaymentOptionType == PaymentOptionType.AssemblyPayment
                    || payment.PaymentOptionType == PaymentOptionType.Afterpay
                    || payment.PaymentOptionType == PaymentOptionType.Zip
                    || payment.PaymentOptionType == PaymentOptionType.TD
                    || payment.PaymentOptionType == PaymentOptionType.Elavon
                    || payment.PaymentOptionType == PaymentOptionType.Square
                    || Extensions.IseConduitPayment(payment.PaymentOptionType)
                    || Extensions.IseLnklyPayment(payment.PaymentOptionType)
                    || payment.PaymentOptionType == PaymentOptionType.Clearent
                    || payment.PaymentOptionType == PaymentOptionType.Windcave
                    || payment.PaymentOptionType == PaymentOptionType.Castle
                    || payment.PaymentOptionType == PaymentOptionType.Clover
                    || payment.PaymentOptionType == PaymentOptionType.TyroTapToPay
                    || payment.PaymentOptionType == PaymentOptionType.HikePay
                    || payment.PaymentOptionType == PaymentOptionType.HikePayTapToPay)
                {
                    payment.InvoicePaymentDetails = InvoicePaymentDetails;
                }
                if (Invoice.InvoicePayments == null)
                {
                    Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                }




                Invoice.InvoicePayments.Add(payment);

                var Result = await calculateInvoicePayment(payment.PaymentOptionType);

                Invoice.TenderAmount = Invoice.NetAmount - Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);

                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                Invoice.AmountChanged?.Invoke(this, Invoice.TenderAmount);
                //End ticket #73190 By Pratik

                if (Result && Invoice.ReferenceInvoiceId != 0 && (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange))
                {
                    var UpdateRefunded = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                    if (UpdateRefunded != null)
                    {

                        foreach (var itemRF in Invoice.InvoiceLineItems)
                        {
                            foreach (var item in UpdateRefunded.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount))
                            {
                                if (item.InvoiceItemType == itemRF.InvoiceItemType && item.InvoiceItemValue == itemRF.InvoiceItemValue && itemRF.Quantity < 0)
                                {
                                    item.RefundedQuantity += itemRF.Quantity.ToPositive();
                                }
                            }
                        }
                        UpdateRefunded.InvoiceHistories = Invoice.InvoiceHistories;
                        await saleService.UpdateLocalInvoice(UpdateRefunded);
                    }
                }
                SetPaymentListVisiblity();
                return Result;
            }
            catch (Exception ex)
            {
                Logger.SaleLogger("addIntegratedPaymentToSell Exception Msg - " + ex.Message);
                ex.Track();
                return false;
            }
        }

        public async Task<bool> addPaymentToSell(PaymentOptionDto paymentType, string giftcardNumber, GiftCardDto giftCard = null)
        {
            try
            {
                InvoicePaymentDto payment = new InvoicePaymentDto();

                if (Invoice.InvoiceLineItems.Sum(x => x.BackOrderQty) > 0)
                {
                    if (Invoice.BackorderDeposite > 0 && Invoice.TenderAmount < Invoice.BackorderDeposite)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("YouCanNotPayWithLessThanBackOrderDeposite"));
                        return false;
                    }

                    var backorderOutstanding = Invoice.BackOrdertotal - Invoice.BackOrdertotalPaid;

                    if (Invoice.BackorderDeposite > backorderOutstanding)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("BackorderDepositeshouldnotbemorethatbackorderoutstanding"));
                        return false;
                    }

                    if (Invoice.TenderAmount >= Invoice.BackorderDeposite)
                    {
                        Invoice.TenderAmount = Invoice.TenderAmount - Invoice.BackorderDeposite;
                        payment.BackorderPayment = Invoice.BackorderDeposite;
                        Invoice.BackorderDeposite = 0;
                    }
                }
                else
                {
                    payment.BackorderPayment = 0;
                }

                string temppaymetGuid = Guid.NewGuid().ToString();

                Debug.WriteLine("temppaymetGuid : " + temppaymetGuid);

                payment.SyncPaymentReference = temppaymetGuid;
                Debug.WriteLine("SyncPaymentReference : " + payment.SyncPaymentReference);
                payment.PaymentOptionId = paymentType.Id;

                payment.PaymentOptionName = paymentType.Name;
                payment.PaymentOptionType = paymentType.PaymentOptionType;
                var outstanding = (Invoice.NetAmount.ToPositive() - Invoice.TotalPaid.ToPositive());
                var amount = Invoice.TenderAmount;
                //Start Ticket #75526 Myob rounding issue By Pratik
                Invoice.NetAmount = Math.Round(Invoice.NetAmount, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                //End Ticket #75526 By Pratik

                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                payment.TenderedAmount = Invoice.TenderAmount;
                payment.InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();

                //Start Ticket #90942 iOS:FR Cheque number for sale By: Pratik
                if (!string.IsNullOrEmpty(ReferenceID) && paymentType.PaymentOptionType == PaymentOptionType.Card)
                {
                    payment.InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { Key = InvoicePaymentKey.CheckIDAtCheckout, Value = ReferenceID.Trim() });
                }
                ReferenceID = null;
                //End Ticket #90942 By: Pratik

                if ((Invoice.Status == InvoiceStatus.Refunded || (Invoice.Status == InvoiceStatus.Exchange && Invoice.TotalPay <= 0))
                    && payment.PaymentOptionType != PaymentOptionType.Cash)
                {
                    if (paymentType.CanApplySurchageOnRefund)
                    {
                        var surcharge = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == payment.PaymentOptionId && a.InvoicePaymentDetails != null && a.InvoicePaymentDetails.Count > 0)
                                    .Sum(a => a.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount)
                                    .Sum(a => Convert.ToDecimal(a.Value ?? "0").ToPositive()));

                        var totaltender = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == payment.PaymentOptionId)
                        .Sum(a => a.Amount) - surcharge;
                        if (totaltender.ToPositive() > 0)
                        {
                            var percentage = Math.Round(((surcharge * 100) / totaltender), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);

                            if (Invoice.TenderAmount.ToPositive() <= totaltender)
                                surcharge = Math.Round(((Invoice.TenderAmount * percentage) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        }
                        if (surcharge > 0)
                            surcharge = surcharge.ToNegative();
                        if (surcharge.ToPositive() > 0)
                            payment.InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { InvoicePaymentID = payment.Id, InvoiceId = payment.InvoiceId, Key = InvoicePaymentKey.SurchargeAmount, Value = surcharge.ToString("0.####") });
                        amount = amount + surcharge;
                        Invoice.TotalPaymentSurcharge += surcharge;
                        Invoice.TotalTipSurcharge += surcharge;
                        Invoice.NetAmount += surcharge;
                        payment.TenderedAmount = Invoice.TenderAmount + surcharge;
                        IsLblTipEnabled = true;

                    }
                }
                //Ticket start:#75743 Exchanges not working at Great World outlet.by rupesh
                else if (paymentType.Surcharge.HasValue && payment.PaymentOptionType != PaymentOptionType.Cash && ((Invoice.Status != InvoiceStatus.Refunded && Invoice.Status != InvoiceStatus.Exchange) || (Invoice.Status == InvoiceStatus.Exchange && Invoice.TotalPay > 0)))
                {
                    //Ticket end:#75743 .by rupesh
                    decimal surcharge = 0;
                    if (outstanding > amount)
                        surcharge = Math.Round(((amount * paymentType.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                    else
                        surcharge = Math.Round(((outstanding * paymentType.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);

                    payment.InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { InvoicePaymentID = payment.Id, InvoiceId = payment.InvoiceId, Key = InvoicePaymentKey.SurchargeAmount, Value = surcharge.ToString("0.####") });
                    amount = amount + surcharge;
                    Invoice.TotalPaymentSurcharge += surcharge;
                    Invoice.TotalTipSurcharge += surcharge;
                    Invoice.NetAmount += surcharge;
                    payment.TenderedAmount = Invoice.TenderAmount + surcharge;
                    IsLblTipEnabled = true;

                }
                //End ticket #73190

                if (Invoice.TenderAmount.ToPositive() >= outstanding)
                {
                    amount = (Invoice.NetAmount - Invoice.TotalPaid);
                }
                else if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent && paymentType.PaymentOptionType == PaymentOptionType.Cash)
                {
                    if (Settings.StoreGeneralRule.RoundUptoCent == "1")
                    {
                        if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUptoTencentOnRefund())
                        {
                            amount = outstanding;
                        }
                        else if (amount == outstanding.RoundUptoTencent())
                        {
                            amount = outstanding;
                        }
                    }
                    else if (Settings.StoreGeneralRule.RoundUptoCent == "2")
                    {
                        if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUptoZerocent())
                        {
                            amount = outstanding;
                        }
                        else if (amount == outstanding.RoundUptoZerocent())
                        {
                            amount = outstanding;
                        }
                    }
                    //Ticket start:#73187 iPad: Round off totals.by Rupesh
                    else if (Settings.StoreGeneralRule.RoundUptoCent == "3")
                    {
                        if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUptoFiftycent())
                        {
                            amount = outstanding;
                        }
                        else if (amount == outstanding.RoundUptoFiftycent())
                        {
                            amount = outstanding;
                        }
                    }
                    //Ticket end:#73187 .by Rupesh

                    //Ticket start:#84296 iOS - Feature:- Allow round-off totals to nearest 10 and 100. by rupesh
                    else if (Settings.StoreGeneralRule.RoundUptoCent == "4")
                    {
                        if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUpto10cent())
                        {
                            amount = outstanding;
                        }
                        else if (amount == outstanding.RoundUpto10cent())
                        {
                            amount = outstanding;
                        }
                    }
                    else if (Settings.StoreGeneralRule.RoundUptoCent == "5")
                    {
                        if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUpto100cent())
                        {
                            amount = outstanding;
                        }
                        else if (amount == outstanding.RoundUpto100cent())
                        {
                            amount = outstanding;
                        }
                    }
                    //Ticket end:#84296.by rupesh

                    else
                    {
                        if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUptoFivecentOnRefund())
                        {
                            amount = outstanding;
                        }
                        else if (amount == outstanding.RoundUptoFivecent())
                        {
                            amount = outstanding;
                        }
                    }
                }

                payment.RoundingAmount = 0;

                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent && paymentType.PaymentOptionType == PaymentOptionType.Cash)
                {
                    if (Settings.StoreGeneralRule.RoundUptoCent == "1")
                    {
                        payment.RoundingAmount = (amount.RoundUptoTencent() - amount);
                    }
                    else if (Settings.StoreGeneralRule.RoundUptoCent == "2")
                    {
                        payment.RoundingAmount = (amount.RoundUptoZerocent() - amount);
                    }
                    //Ticket start:#73187 iPad: Round off totals.by Rupesh
                    else if (Settings.StoreGeneralRule.RoundUptoCent == "3")
                    {
                        payment.RoundingAmount = (amount.RoundUptoFiftycent() - amount);
                    }
                    //Ticket end:#73187 .by Rupesh

                    //Ticket start:#84296 iOS - Feature:- Allow round-off totals to nearest 10 and 100. by rupesh
                    else if (Settings.StoreGeneralRule.RoundUptoCent == "4")
                    {
                        payment.RoundingAmount = (amount.RoundUpto10cent() - amount);
                    }
                    else if (Settings.StoreGeneralRule.RoundUptoCent == "5")
                    {
                        payment.RoundingAmount = (amount.RoundUpto100cent() - amount);
                    }
                    //Ticket end:#84296.by rupesh

                    else
                    {
                        payment.RoundingAmount = (amount.RoundUptoFivecent() - amount);
                    }
                }

                payment.Amount = amount + payment.RoundingAmount;
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                payment.TenderedAmount = Invoice.TenderAmount + payment.RoundingAmount + (paymentType.DisplaySurcharge ?? 0); //payment.roundingAmount;
                //End ticket #73190  By Rupesh

                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent && paymentType.PaymentOptionType == PaymentOptionType.Cash)
                {
                    if (Invoice.Status == InvoiceStatus.Refunded)
                    {
                        payment.TenderedAmount = Invoice.TenderAmount - payment.RoundingAmount;
                    }
                }

                payment.IsPaid = true;

                var currentRegister = Settings.CurrentRegister;
                if (currentRegister != null)
                {
                    payment.OutletId = currentRegister.OutletID;
                    payment.OutletName = currentRegister.OutletName;
                    payment.RegisterId = currentRegister.Id;
                    payment.RegisterName = currentRegister.Name;
                    if (currentRegister.Registerclosure != null)
                    {
                        payment.RegisterClosureId = currentRegister.Registerclosure.Id;
                    }
                }
                payment.ServedBy = Settings.CurrentUser.FullName;

                if (Invoice.Status == InvoiceStatus.Refunded)
                    payment.ActionType = ActionType.Refund;
                else
                    payment.ActionType = ActionType.Sell;

                payment.PaymentDate = Extensions.moment();
                payment.PaymentFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                // payment.InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();
                //End ticket #73190 
                //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
                if (payment.PaymentOptionType == PaymentOptionType.GiftCard)
                {
                    var giftCardSpentAmount = Invoice.InvoicePayments.Where(a => a.PaymentOptionType == PaymentOptionType.GiftCard && a.InvoicePaymentDetails.Any(a => a.Value == giftcardNumber)).Sum(a => a.Amount);
                    var giftBalance = giftCard.Amount - giftCard.SpentAmount - giftCardSpentAmount;
                    payment.InvoicePaymentDetails.Add(
                        new InvoicePaymentDetailDto
                        {
                            Key = InvoicePaymentKey.GiftCardNumber,
                            Value = giftcardNumber
                        });
                    payment.InvoicePaymentDetails.Add(
                    new InvoicePaymentDetailDto
                    {
                        Key = InvoicePaymentKey.GiftCardOpeningBalance,
                        Value = giftBalance.ToString("G29")
                    });
                    payment.InvoicePaymentDetails.Add(
                    new InvoicePaymentDetailDto
                    {
                        Key = InvoicePaymentKey.GiftCardClosingBalance,
                        Value = (giftBalance - payment.TenderedAmount).ToString("G29")
                    });

                    GiftCardBalance = LanguageExtension.Localize("BalanceLabelText") + (giftBalance - payment.TenderedAmount).ToString("C");
                }
                //Ticket end:#94420.by rupesh
                else if (payment.PaymentOptionType == PaymentOptionType.Credit)
                {
                    payment.InvoicePaymentDetails.Add(
                        new InvoicePaymentDetailDto
                        {
                            Key = InvoicePaymentKey.OpeningCreditBalance,
                            Value = (Invoice.CustomerDetail.CreditBalance + payment.Amount).ToString()
                        });
                    payment.InvoicePaymentDetails.Add(
                    new InvoicePaymentDetailDto
                    {
                        Key = InvoicePaymentKey.ClosingCreditBalance,
                        Value = (Invoice.CustomerDetail.CreditBalance).ToString()
                    });

                }
                if (Invoice.InvoicePayments == null)
                {
                    Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                }

                if (payment.PaymentOptionType == PaymentOptionType.Loyalty)
                {
                    var Loyaltyamt = Invoice.CustomerCurrentLoyaltyPoints;
                    var bal = Loyaltyamt / Settings.StoreGeneralRule.LoyaltyPointsValue;
                    if (payment.TenderedAmount > bal)
                        payment.LoyaltyPoints = bal * Settings.StoreGeneralRule.LoyaltyPointsValue;
                    else
                        payment.LoyaltyPoints = payment.TenderedAmount * Settings.StoreGeneralRule.LoyaltyPointsValue;

                    //Ticket start:#91263 iOS:FR Redeeming Points.by rupesh
                    if (giftcardNumber != null)
                        payment.InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { Key = InvoicePaymentKey.ApprovedByUser, Value = giftcardNumber });
                    //Ticket end:#91263 .by rupesh

                }

                Invoice.InvoicePayments.Add(payment);

                foreach (var item in Invoice.InvoicePayments.Where(a => a.BackorderPayment > 0))
                {
                    Invoice.BackorderPayments.Add(item);
                }

                var Result = await calculateInvoicePayment(payment.PaymentOptionType);

                var temps = Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);

                Invoice.TenderAmount = Invoice.NetAmount - Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);

                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                Invoice.AmountChanged?.Invoke(this, Invoice.TenderAmount);
                //End ticket #73190 By Pratik

                if (Convert.ToDouble(Invoice.TenderAmount) < 0.005 && Convert.ToDouble(Invoice.TenderAmount) > 0.00)
                {
                    Invoice.TenderAmount = 0.00m;

                }

                if (Result && Invoice.ReferenceInvoiceId != 0 && (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange))
                {
                    var UpdateRefunded = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                    if (UpdateRefunded != null)
                    {

                        foreach (var itemRF in Invoice.InvoiceLineItems)
                        {
                            foreach (var item in UpdateRefunded.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount))
                            {
                                if (item.InvoiceItemType == itemRF.InvoiceItemType && item.InvoiceItemValue == itemRF.InvoiceItemValue && itemRF.Quantity < 0)
                                {
                                    item.RefundedQuantity += itemRF.Quantity.ToPositive();
                                }
                            }
                            UpdateRefunded.InvoiceHistories = Invoice.InvoiceHistories;
                        }
                        await saleService.UpdateLocalInvoice(UpdateRefunded);
                    }
                }

                if (DeviceInfo.Idiom != DeviceIdiom.Phone) //#97675
                    SetPaymentListVisiblity(); //#97675

                return Result;
            }
            catch (Exception ex)
            {
                Logger.SaleLogger("addPaymentToSell Exception Msg - " + ex.Message);
                ex.Track();
                return false;
            }
        }

        async Task<bool> calculateInvoicePayment(PaymentOptionType PaymentType)
        {
            try
            {
                /*if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent && PaymentType == PaymentOptionType.Cash)
                {
                    if (Invoice.Status != InvoiceStatus.Refunded)
                    {
                        //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
                       // var tempRoundAmount = (Invoice.InvoicePayments.Sum(x => x.RoundingAmount) - Invoice.RoundingAmount);
                       // Invoice.RoundingAmount = Invoice.InvoicePayments.Sum(x => x.RoundingAmount);
                       //Invoice.NetAmount += tempRoundAmount;
                       //Ticket End:#91262 by rupesh
                    }
                    else
                    {
                        //Start #85573 iOS: while have rounding value with surchage and shipping at that time refund not working By Pratik
                        //decimal tempRoundAmount =0;
                        //if (Invoice.RoundingAmount == 0)
                        //    tempRoundAmount = (Invoice.InvoicePayments.Sum(x => x.RoundingAmount) - Invoice.RoundingAmount);
                        //else
                        //    tempRoundAmount = Invoice.RoundingAmount;
                        ////Invoice.RoundingAmount = Invoice.InvoicePayments.Sum(x => x.RoundingAmount);
                        
                        //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
                        //Invoice.RoundingAmount = Invoice.InvoicePayments.Sum(x => x.RoundingAmount);
                        //Invoice.NetAmount += Invoice.RoundingAmount;
                        //Ticket End:#91262 by rupesh
                        
                        //End #85573 By Pratik
                    }
                }*/

                var outstanding = Invoice.NetAmount.ToPositive() - Invoice.TotalPaid.ToPositive();
                if (Invoice.TenderAmount.ToPositive() > outstanding)
                {
                    Invoice.TotalTender = Invoice.TotalPaid + Invoice.TenderAmount;
                }
                else
                {
                    Invoice.TotalTender = Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);
                }
                var iscomplete = Settings.IsQuoteSale;
                //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
                decimal roundamt = 0;
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent && PaymentType == PaymentOptionType.Cash)
                    roundamt = NetAmountRoundingAmount();
                //Ticket End:#91262 by rupesh
                //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time. by rupesh
                if (Invoice.IsEditBackOrderFromSaleHistory)
                {
                    iscomplete = false;
                }
                //Ticket end:#84289 . by rupesh
                else if ((Invoice.TotalTender.ToPositive() + roundamt.ToPositive()) >= Invoice.NetAmount.ToPositive()) //Ticket start:#91262 by rupesh
                {
                    Invoice.NetAmount = Invoice.NetAmount + roundamt; //Ticket start:#91262 by rupesh
                    //Start Ticket #75526 Myob rounding issue By Pratik
                    var tempNetAmount = Math.Round(Invoice.NetAmount.ToPositive(), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                    //End Ticket #75526 By Pratik
                    Invoice.RoundingAmount = roundamt; //Ticket start:#91262 by rupesh
                    if (Invoice.BackorderDeposite <= 0)
                    {
                        Invoice.ChangeAmount = Invoice.TotalTender.ToPositive() - tempNetAmount;
                    }
                    iscomplete = true;
                }
                else
                {
                    var tempAmount = Invoice.NetAmount.ToPositive() - Invoice.TotalTender.ToPositive();

                    if (tempAmount < 0.005m)
                    {
                        //Start Ticket #75526 Myob rounding issue By Pratik
                        Invoice.NetAmount = Math.Round(Invoice.NetAmount, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        //End Ticket #75526 By Pratik
                        Invoice.ChangeAmount = Invoice.TotalTender.ToPositive() - Invoice.NetAmount.ToPositive();
                        Invoice.RoundingAmount = roundamt; //Ticket start:#91262 by rupesh
                        iscomplete = true;
                    }

                    if (Invoice.Status == InvoiceStatus.Refunded)
                    {
                        if (Invoice.NetAmount >= Invoice.TotalTender)
                        {
                            Invoice.ChangeAmount = Invoice.NetAmount.ToPositive() - Invoice.TotalTender.ToPositive();
                            Invoice.RoundingAmount = roundamt; //Ticket start:#91262 by rupesh
                            iscomplete = true;
                        }
                    }
                }

                if (!Settings.IsQuoteSale)
                {
                    if (Invoice.TotalTender > 0)
                        Invoice.TotalPaid = Invoice.TotalTender - Invoice.ChangeAmount;
                    else
                        Invoice.TotalPaid = Invoice.TotalTender + Invoice.ChangeAmount;
                }
                else
                    Invoice.TotalPaid = 0;

                //Suceess check
                if (PaymentType == PaymentOptionType.HikePayTapToPay || PaymentType == PaymentOptionType.HikePay)
                {
                    var totalPay = Invoice.NetAmount - Invoice.TotalPaid;
                    if (totalPay < 0 && totalPay > (decimal)(-0.01))
                    {
                        totalPay = 0;
                    }
                    totalPay = Math.Round(totalPay, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                    if (Invoice.TotalPay == 0)
                    {
                        IsAddPaymentActive = false;
                        IsSuccessPaymentActive = true;
                    }
                }
                //End


                Invoice.BackOrdertotalPaid = Invoice.InvoicePayments.Sum(x => x.BackorderPayment);

                if (Invoice.RoundingAmount != 0)
                {
                    if (Math.Round(Invoice.RoundingAmount.ToPositive(), 2, MidpointRounding.AwayFromZero) == Invoice.ChangeAmount)
                    {

                        Invoice.ChangeAmount = 0.00m;
                    }

                    if (Invoice.RoundingAmount.ToPositive() == Invoice.OutstandingAmount.ToPositive())
                    {
                        Invoice.ChangeAmount = 0.00m;
                    }
                }

                if (iscomplete)
                {
                    if (Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy || Invoice.Status == InvoiceStatus.BackOrder
                        || Settings.StoreGeneralRule.ParkPaidOrderRule == "1")
                    {
                        if (Invoice.Status == InvoiceStatus.OnAccount || Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
                        {
                            //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                            var surcharge = Invoice.InvoicePayments.Where(a => a.InvoicePaymentDetails != null && a.InvoicePaymentDetails.Count > 0)
                                       .Sum(a => a.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount)
                                       .Sum(a => Convert.ToDecimal(a.Value ?? "0").ToPositive()));
                            

                            if (Invoice.TotalPaid < 0)
                                Invoice.TotalPaymentSurcharge = surcharge.ToNegative();
                            else
                                Invoice.TotalPaymentSurcharge = surcharge.ToPositive();
                            //end ticket #73190
                            return await sale_completed();
                        }

                        if (Settings.StoreGeneralRule.ParkingPaidOrder || Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy)
                        {
                            bool result = false;

                            if (!Settings.IsQuoteSale && SelectedPaymentOption?.PaymentOptionType != PaymentOptionType.VantivIpad)
                            {
                                //Start #103222 PR
                                if (NavigationService?.ModalStack != null && NavigationService.ModalStack.Count == 1)
                                {
                                    await NavigationService.PopModalAsync();
                                }
                                //End #103222 
                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                {
                                    result = Invoice?.InvoiceFloorTable != null ? true : await App.Alert.ShowFulfillmentAlert("Confirmation", LanguageExtension.Localize("HasOrderFulfilled"), "Yes", "No");//#94565
                                });
                            }
                            else
                            {
                                result = true;
                            }

                            if (result)
                            {
                                // Start #73186 iPad  :iPad - Lay-by completion date option: Same as parked sale By Pratik
                                string invoiceStatus = null;
                                if (Invoice.Status == InvoiceStatus.LayBy || Invoice.Status == InvoiceStatus.Parked)
                                {
                                    invoiceStatus = Convert.ToString(Invoice.Status);
                                }
                                else
                                {
                                    invoiceStatus = null;
                                }
                                // End #73186 iPad By Pratik


                                if (Settings.IsQuoteSale)
                                    Invoice.Status = InvoiceStatus.Quote;
                                else
                                    Invoice.Status = InvoiceStatus.Completed;


                                if (Invoice.BackOrdertotal > 0)
                                {
                                    var tempInvoice = Invoice;
                                    InvoiceBackOrder = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService, invoiceStatus);
                                    Invoice = tempInvoice;
                                }
                                else
                                {
                                    Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService, invoiceStatus);
                                }

                                return true;
                            }
                            else
                            {
                                Invoice.Status = InvoiceStatus.Parked;
                                return await sale_completed();
                            }
                        }
                        else
                        {
                            return await sale_completed();
                        }
                    }
                    else
                    {
                        if (!(Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange))
                        {
                            //Ticket start:#81028 
                            if (Settings.IsQuoteSale)
                                Invoice.Status = InvoiceStatus.Quote;
                            else
                                Invoice.Status = InvoiceStatus.Completed;
                            //Ticket end:#81028 
                        }
                        else
                        {
                            //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                            var surcharge = Invoice.InvoicePayments.Where(a => a.InvoicePaymentDetails != null && a.InvoicePaymentDetails.Count > 0)
                                       .Sum(a => a.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount)
                                       .Sum(a => Convert.ToDecimal(a.Value ?? "0").ToPositive()));
                            if (Invoice.TotalPaid < 0)
                                Invoice.TotalPaymentSurcharge = surcharge.ToNegative();
                            else
                                Invoice.TotalPaymentSurcharge = surcharge.ToPositive();
                            //end ticket #73190
                            //TapToPayRelated change Start.
                            if (Invoice.Status == InvoiceStatus.Exchange)
                            {
                                var tapToPayments = Invoice.InvoicePayments.Where(x => x.PaymentOptionType == PaymentOptionType.TyroTapToPay);
                                foreach (var payment in tapToPayments)
                                {
                                    try
                                    {
                                        var transactionOutCome = JsonConvert.DeserializeObject<HikePOS.Models.Payment.TyroTapToPayPaymentResponse>(payment.InvoicePaymentDetails?.FirstOrDefault().Value);
                                        double paymentSurcharge = 0;
                                        double.TryParse(transactionOutCome.surcharge, out paymentSurcharge);
                                        Invoice.TotalPaymentSurcharge += (decimal)(paymentSurcharge / 100.0);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }

                                tapToPayments = Invoice.InvoicePayments.Where(x => x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay);
                                foreach (var payment in tapToPayments)
                                {
                                    try
                                    {
                                        var transactionOutCome = JsonConvert.DeserializeObject<HikePOS.Models.Payment.NadaPayTransactionDto>(payment.InvoicePaymentDetails.FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData).Value);
                                        decimal paymentSurcharge = transactionOutCome.SurchargeAmount;
                                        Invoice.TotalPaymentSurcharge += paymentSurcharge;
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                            //TapToPayRelated change End.

                        }
                        return await sale_completed();

                    }
                }
                else
                {

                    //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time. by rupesh
                    //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                    if (Invoice.Status == InvoiceStatus.BackOrder && Invoice.IsEditBackOrderFromSaleHistory)
                    {
                        Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                        IsSuccessPaymentActive = true;
                        IsAddPaymentActive = false;
                    }
                    //Ticket start:#92764.by rupesh
                    //Ticket start:#84289 . by rupesh

                    if (Invoice.Status == InvoiceStatus.Pending)
                    {
                        Invoice.Status = InvoiceStatus.Parked;
                    }

                    if (Invoice.Status != InvoiceStatus.Refunded)
                        await sale_uploadedTolocaldb();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.SaleLogger("calculateInvoicePayment Exception Msg - " + ex.Message);
                ex.Track();
                return false;
            }
        }

        public async Task<bool> GetGiftCardBalance(string giftcardnumber)
        {
            if (giftcardnumber.Length < 4)
            {
                GiftCardBalance = "";
                IsGiftCardAnable = false;
                return false;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                var result = await giftCardService.GetRemoteGiftCard(Fusillade.Priority.UserInitiated, false, giftcardnumber);
                if (result == null || result.Id == 0)
                {
                    GiftCardBalance = "";
                    IsGiftCardAnable = false;
                    return false;
                }

                var GiftBalance = result.Amount - result.SpentAmount;
                //Start #90944 iOS:FR Gift cards expiry date by Pratik
                if (string.IsNullOrEmpty(result.ExpiryMsg))
                {
                    GiftCardBalance = LanguageExtension.Localize("BalanceLabelText") + (GiftBalance).ToString("C");
                    IsGiftCardAnable = true;
                }
                else
                {
                    GiftCardBalance = result.ExpiryMsg;
                    IsGiftCardAnable = true;
                    return false;
                }
                //End #90944 by Pratik
                return true;
            }
            else
            {
                GiftCardBalance = LanguageExtension.Localize("NoInternetMessage");
                IsGiftCardAnable = false;
                return false;
            }
        }

        public async Task<bool> addGiftCardPayment(PaymentOptionDto paymentType, string giftcardnumber)
        {
            GiftCardBalance = "";
            IsGiftCardAnable = false;
            if (string.IsNullOrEmpty(giftcardnumber))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("GiftCardNumberValidationMessage"));
                return false;
            }
            var result = await giftCardService.GetRemoteGiftCard(Fusillade.Priority.UserInitiated, false, giftcardnumber);
            if (result == null || result.Id == 0)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("GiftCardNotAvailableValidationMessage"));
                return false;
            }

            if (!string.IsNullOrEmpty(result.ExpiryMsg))
            {
                GiftCardBalance = result.ExpiryMsg;
                IsGiftCardAnable = true;
                return false;
            }

            //#97675
            decimal giftCardSpentAmount = 0;
            if (Invoice.InvoicePayments != null && Invoice.InvoicePayments.Count > 0)
                giftCardSpentAmount = Invoice.InvoicePayments.Where(a => a.PaymentOptionType == PaymentOptionType.GiftCard && a.InvoicePaymentDetails.Any(a => a.Value == giftcardnumber)).Sum(a => a.Amount); //#97675
                                                                                                                                                                                                               //#97675-

            var GiftBalance = result.Amount - result.SpentAmount - giftCardSpentAmount;

            if (Invoice.TenderAmount > GiftBalance)
            {
                if (GiftBalance > 0)
                {
                    Invoice.TenderAmount = GiftBalance;
                    Payment_GiftCardNumber = "";
                    //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
                    return await addPaymentToSell(paymentType, giftcardnumber, result);
                    //Ticket end:#94420.by rupesh
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("GiftCardBalanceValidationMessage") + " " + GiftBalance.ToString("C"));
                    GiftCardBalance = LanguageExtension.Localize("BalanceLabelText") + (result.Amount - result.SpentAmount).ToString("C");
                    return false;
                }
            }
            else
            {
                Payment_GiftCardNumber = "";
                //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
                return await addPaymentToSell(paymentType, giftcardnumber, result);
                //Ticket start:#94420.by rupesh
            }
        }

        async Task<bool> sale_completed()
        {
            try
            {
                if (!(Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange))
                {
                    if ((Settings.StoreGeneralRule.ParkingPaidOrder && !Settings.IsQuoteSale) || Invoice.Status == InvoiceStatus.Parked)
                    {
                        Invoice.Status = InvoiceStatus.Parked;

                        var data = Invoice.InvoiceHistories.Where(x => x.Status == InvoiceStatus.OnAccount);
                        if (data != null && data.Count() > 0 && Settings.StoreGeneralRule.ExcludeOnAccountSalesFromTheFulfillment)
                            Invoice.Status = InvoiceStatus.Completed;
                    }
                    else
                    {
                        //Ticket start:#81028 
                        if (Settings.IsQuoteSale)
                            Invoice.Status = InvoiceStatus.Quote;
                        else
                            Invoice.Status = InvoiceStatus.Completed;
                        //Ticket end:#81028 
                    }
                }

                if (Invoice.BackOrdertotal > 0)
                {
                    var tempInvoice = Invoice;
                    InvoiceBackOrder = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                    Invoice = tempInvoice;
                }
                else
                {
                    Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.SaleLogger("sale_completed Exception Msg - " + ex.Message);
                ex.Track();
                return false;
            }
        }

        async Task<bool> sale_onaccountSale()
        {
            Invoice.Status = InvoiceStatus.OnAccount;


            if (Invoice.BackOrdertotal > 0)
            {
                var tempInvoice = Invoice;
                InvoiceBackOrder = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                Invoice = tempInvoice;
            }
            else
            {

                Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
            }
            return true;
        }

        async Task<bool> BackorderSale()
        {
            Invoice.Status = InvoiceStatus.BackOrder;


            if (Invoice.BackOrdertotal > 0)
            {
                var tempInvoice = Invoice;
                InvoiceBackOrder = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                Invoice = tempInvoice;
            }
            else
            {
                Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
            }
            return true;
        }

        async Task<bool> sale_LaybySale()
        {
            Invoice.Status = InvoiceStatus.LayBy;


            if (Invoice.BackOrdertotal > 0)
            {
                var tempInvoice = Invoice;
                InvoiceBackOrder = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                Invoice = tempInvoice;
            }
            else
            {
                Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
            }
            return true;
        }

        async Task<InvoiceDto> sale_uploadedTolocaldb()
        {
            if (string.IsNullOrEmpty(Invoice.Number))
            {
                var Register = Settings.CurrentRegister;
                if (Settings.IsQuoteSale)
                    Invoice.Number = Register.QuotePrefix + (Register.QuoteReceiptNumber + 1) + Register.QuoteSuffix;
                else
                    Invoice.Number = Register.Prefix + (Register.ReceiptNumber + 1) + Register.Suffix;

                Invoice.Barcode = "#" + Invoice.Number;
                Invoice.TransactionDate = Extensions.moment();
                Invoice.FinalizeDate = Invoice.TransactionDate;
                outletService.UpdateLocalRegisterReceiptNumber(Settings.CurrentRegister.Id, Settings.SelectedOutletId);
            }

            if (EnterSalePage.ServedBy == null)
            {
                EnterSalePage.ServedBy = Settings.CurrentUser;
            }
            //Ticket start:#28837 Served by column on sales receipt does not reflex latest.by rupesh
            //Ticket start:#36567 Layby sales not updating served by.by rupesh
            //if (Invoice.ServedBy == 0)
            //{
            Invoice.ServedBy = EnterSalePage.ServedBy.Id;
            Invoice.ServedByName = EnterSalePage.ServedBy.FullName;
            //}
            //Ticket end:#36567 .by rupesh
            //Ticket end:#28837.by rupesh


            if (Invoice.CustomerId == null || (Invoice.CustomerId == 0 && string.IsNullOrEmpty(Invoice.CustomerTempId)) || Invoice.CustomerName == "Select customer")
            {
                Invoice.CustomerId = null;
                Invoice.CustomerName = "Walk in";
                Invoice.CustomerTempId = "";
                Invoice.CustomerDetail = new CustomerDto_POS();
            }
            return await saleService.UpdateLocalInvoice(Invoice);
        }

        async Task ParkSale()
        {
            try
            {
                using (new Busy(this, false))
                {
                    if (Invoice != null && Invoice.InvoiceLineItems != null && Invoice.InvoiceLineItems.Count > 0 && (Invoice.Status == InvoiceStatus.Pending || Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy))
                    {

                        if (InvoiceCalculations.CheckHasBackOrder(Invoice) && (Invoice.CustomerId == null || Invoice.CustomerId == 0))
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("BackOrderCutomerValidation"), Colors.Red, Colors.White);
                            return;
                        }

                        if (promptPopupPage == null)
                        {
                            promptPopupPage = new PromptPopupPage()
                            {
                                Title = LanguageExtension.Localize("ParkedAlertTitle"),
                                Description = LanguageExtension.Localize("ParkedAlertBody"),
                                Placeholder = LanguageExtension.Localize("AddNotePlaceholder"),
                                YesButtonText = LanguageExtension.Localize("SaveButtonText"),
                                NoButtonText = LanguageExtension.Localize("CancelButtonText"),
                                SaveAndPrintButtonText = LanguageExtension.Localize("SaveAndPrintButtonText")
                            };

                            promptPopupPage.SavedAndPrint += async (object sender, string e) =>
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(e))
                                    {
                                        Invoice.Note = e;
                                    }

                                    using (new Busy(this, false))
                                    {
                                        Invoice.Status = InvoiceStatus.Parked;

                                        if (Invoice.BackOrdertotal > 0)
                                        {
                                            var tempInvoice = Invoice;
                                            InvoiceBackOrder = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                                            Invoice = tempInvoice;
                                        }
                                        else
                                        {
                                            Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                                        }

                                        if (Invoice.ReferenceInvoiceId != 0 && Invoice.Status == InvoiceStatus.Parked && Invoice.InvoiceHistories?.LastOrDefault(a => a.Status != InvoiceStatus.EmailSent)?.Status == InvoiceStatus.Quote)
                                        {
                                            var UpdatedInvoice = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                                            UpdatedInvoice.FinancialStatus = FinancialStatus.Closed;
                                            UpdatedInvoice.ReferenceNote = "Converted sale " + Invoice.Number;
                                            await saleService.UpdateLocalInvoice(UpdatedInvoice);
                                        }


                                        Invoice = null;
                                        CheckOutViewModel.IsSaleSucceess = true;
                                        await _navigationService.Navigation.PopAsync();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.Track();
                                }
                            };

                            promptPopupPage.Saved += async (object sender, string e) =>
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(e))
                                    {
                                        Invoice.Note = e;
                                    }

                                    using (new Busy(this, false))
                                    {
                                        Invoice.Status = InvoiceStatus.Parked;

                                        if (Invoice.BackOrdertotal > 0)
                                        {
                                            var tempInvoice = Invoice;
                                            InvoiceBackOrder = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                                            Invoice = tempInvoice;
                                        }
                                        else
                                        {
                                            Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                                        }

                                        if (Invoice.ReferenceInvoiceId != 0 && Invoice.Status == InvoiceStatus.Parked && Invoice.InvoiceHistories?.LastOrDefault(a => a.Status != InvoiceStatus.EmailSent)?.Status == InvoiceStatus.Quote)
                                        {
                                            var UpdatedInvoice = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                                            UpdatedInvoice.FinancialStatus = FinancialStatus.Closed;
                                            UpdatedInvoice.ReferenceNote = "Converted sale " + Invoice.Number;
                                            await saleService.UpdateLocalInvoice(UpdatedInvoice);

                                        }

                                        Invoice = null;
                                        CheckOutViewModel.IsSaleSucceess = true;
                                        await _navigationService.Navigation.PopAsync();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.Track();
                                }
                            };
                        }
                        await NavigationService.PushModalAsync(promptPopupPage);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        async Task PaymentToolMenu(string option)
        {
            try
            {
                switch (option)
                {
                    case "Parked":
                        await ParkSale();
                        break;

                    case "PrintInvoice":
                        if (Settings.GetCachePrinters != null && Settings.GetCachePrinters.Any(x => x.PrimaryReceiptPrint))
                        {
                            WeakReferenceMessenger.Default.Send(new Messenger.ManualPrintMessenger(true));
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("PrinterValidationMessage"));
                        }
                        break;

                    case "OpenCashDrawer":
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task<bool> SendEmail(string email)
        {
            try
            {
                using (new Busy(this, false))
                {
                    if (!string.IsNullOrEmpty(email.Trim()))
                    {
                        if (App.Instance.IsInternetConnected)
                        {
                            if ((Invoice.CustomerId == null || Invoice.CustomerId < 1) && !string.IsNullOrEmpty(Invoice.CustomerTempId))
                            {
                                await Task.Delay(3000);
                            }
                            SaleInvoiceEmailInput objInvoiceEmailDto = new SaleInvoiceEmailInput()
                            {
                                Email = email,
                                SyncReference = Invoice.InvoiceTempId,
                                InvoiceId = Invoice.Id,
                                CustomerId = Invoice.CustomerId,
                                RegisterId = Invoice.RegisterId.Value,
                                InvoiceNumber = Invoice.Number,
                                EmailTemplateType = (Invoice.Status == InvoiceStatus.Quote) ? EmailTemplateType.CustomerQuoteReceipt : (IsEmailWithPayLink && IsEmailWithPayLinkVisible ? EmailTemplateType.CustomerReceiptWithPaymentLink : EmailTemplateType.CustomerReceipt),
                                EmailForGiftCard = IsToPrintGiftReceipt,
                                CustomerName = CustomerName,
                                InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android,
                                IsEmailPaymentLinkActive = IsEmailWithPayLink && IsEmailWithPayLinkVisible,
                                CountryCode = Settings.CountryCode,
                            };
                            var result = await saleService.SendInvoiceEmail(objInvoiceEmailDto);
                            if (result == LanguageExtension.Localize("EmailSuccess"))
                            {
                                if (!string.IsNullOrEmpty(CustomerName))
                                {
                                    Invoice.CustomerName = CustomerName;
                                    await saleService.UpdateLocalInvoice(Invoice);
                                    CustomerName = "";
                                }
                                App.Instance.Hud.DisplayToast(result, Colors.Green, Colors.White);
                                if (Invoice.CustomerId == null)
                                {
                                    var invoice = await saleService.GetRemoteInvoice(Priority.Background, true, Invoice.Id);
                                    if (invoice != null && invoice.CustomerId != null)
                                    {
                                        Invoice.CustomerId = invoice.CustomerId.Value;
                                        CustomerModel.SelectedCustomer.Id = invoice.CustomerId.Value;
                                        CustomerModel.SelectedCustomer.FirstName = invoice.CustomerName;
                                    }
                                }
                                //Ticket start:#92767 iOS:FR Log when invoices, quotes and reports are sent by email.by rupesh
                                var newInvoiceHistory = new InvoiceHistoryDto();
                                newInvoiceHistory.Status = InvoiceStatus.EmailSent;
                                newInvoiceHistory.StatusName = "Email sent";
                                newInvoiceHistory.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                                newInvoiceHistory.ServerdBy = Invoice.ServedByName;
                                newInvoiceHistory.CreationTime = DateTime.UtcNow;
                                Invoice.InvoiceHistories.Add(newInvoiceHistory);
                                await saleService.UpdateLocalInvoice(Invoice);
                                //Ticket end:#92767.by rupesh

                                return true;
                            }
                            App.Instance.Hud.DisplayToast(result, Colors.Red, Colors.White);
                            return false;
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                            return false;
                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_EmptyMessage"));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

   
        public void calculateInvoiceHeight1(string printerType = "")
        {
            if (string.IsNullOrEmpty(printerType))
            {
                var tmp = (ObservableCollection<InvoiceLineItemDto>)(Invoice.InvoiceLineItems);
                int offercount = 0;
                int descriptioncount = 0;

                offercount = tmp.Count(x => !string.IsNullOrEmpty(x.OffersNote));
                descriptioncount = tmp.Count(x => !string.IsNullOrEmpty(x.Description));
                SaleItemListHeight = ((tmp.Count * 80) + (offercount * 40) + (descriptioncount * 40));
            }
            else
            {
                var tmp = (ObservableCollection<InvoiceLineItemDto>)(Invoice.InvoiceLineItems);
                int offercount = 0;
                int descriptioncount = 0;
                double listheight = 0;

                offercount = tmp.Count(x => !string.IsNullOrEmpty(x.OffersNote));
                descriptioncount = tmp.Count(x => !string.IsNullOrEmpty(x.Description));
                foreach (var item in tmp)
                {
                    if (item.ProductTitleWithQuantity.Length > 60)
                        listheight += 15 + item.ProductTitleWithQuantity.Length;
                    else if (item.ProductTitleWithQuantity.Length > 40)
                        listheight += 25 + item.ProductTitleWithQuantity.Length;
                    else
                        listheight += 80;
                }

                SaleItemListHeight = (listheight + (offercount * 60) + (descriptioncount * 60));
            }
        }

        public void calculateInvoiceHeight(string printerType = "")
        {
            var tmp = (ObservableCollection<InvoiceLineItemDto>)(Invoice.InvoiceLineItems);

            int skuCount = 0;
            if (CurrentReceiptTemplate != null && CurrentReceiptTemplate.PrintSKU)
                skuCount = tmp.Count(x => !string.IsNullOrEmpty(x.SKUWithLabel));
            int barcodeCount = 0;
            if (CurrentReceiptTemplate != null && CurrentReceiptTemplate.ShowItemBarCode)
                barcodeCount = tmp.Count(x => !string.IsNullOrEmpty(x.BarcodeWithLabel));
            int offercount = 0;
            offercount = tmp.Count(x => !string.IsNullOrEmpty(x.OffersNote));

            int descriptionHeight = 0;
            tmp.ForEach(x =>
            {
                var desc = x.Description;
                if (!string.IsNullOrEmpty(desc))
                {
                    if (desc.Length > 40)
                        descriptionHeight += ((desc.Length / 40) * 40);
                    else descriptionHeight += 40;

                    var newLineCount = Regex.Matches(desc, "\n").Count;
                    descriptionHeight += (newLineCount * 15);
                }
            });

            double retailAmountHeight = 0;
            int retailAmountCount = 0;
            retailAmountCount = tmp.Count(x => (x.IsTotalReatilAmountVisible && (ShopGeneralRule.DisplayLineItemDiscountOnReceipt)));
            retailAmountHeight = retailAmountCount * 30;

            int serialNumberCount = serialNumberCount = tmp.Count(x => !string.IsNullOrEmpty(x.SerialNumber));

            double lineItemPadding = tmp.Count() * 10;
            double listheight = 0;
            foreach (var item in tmp)
            {
                var title = item.ProductTitleWithQuantity;
                if (printerType.Contains("POP"))
                    listheight += 22 * ((title.Length / 29) + 1) + 11;
                else
                    listheight += 22 * ((title.Length / 50) + 1) + 11;
            }

            double groupHeaderHeight = 0;
            if (Settings.StoreGeneralRule.ShowGroupProductsByCategory)
            {

                var groupInvoiceLineItemsConverter = new GroupInvoiceLineItemsConverter();
                var groupInvoiceLineItems = (ObservableCollection<InvoiceLineiItemGroup>)groupInvoiceLineItemsConverter.Convert(Invoice.InvoiceLineItems, null, null, null);
                groupHeaderHeight = 25 * groupInvoiceLineItems.Count;
            }
            SaleItemListHeight = listheight + (skuCount * 23) + (barcodeCount * 23) + (offercount * 23) + (serialNumberCount * 23) + descriptionHeight
                + retailAmountHeight + lineItemPadding + groupHeaderHeight;
        }

        private async void Fiska_PaymentSuccessed(FiskaPaymentResult fiskaPaymentResult)
        {
            string fiskaPaymentResult1 = "Nothing : ";
            try
            {
                SendLogsToServer(SelectedPaymentOption, null, fiskaPaymentResult);

                if (fiskaPaymentResult == null)
                {
                    return;
                }
                logRequestDetails.Clear();
                fiskaPaymentResult1 = JsonConvert.SerializeObject(fiskaPaymentResult);

                if ((fiskaPaymentResult.ResultCode == ResultCode.FSKResultCodeSuccess &&
                  fiskaPaymentResult.operationStatus == OperationStatus.FSKOperationStatusCompleted)
                  || (fiskaPaymentResult.ResultCode == ResultCode.FSKResultCodeSuccessWithBalance &&
                  fiskaPaymentResult.operationStatus == OperationStatus.FSKOperationStatusCompleted))
                {
                    if (SelectedPaymentOption != null && (SelectedPaymentOption.PaymentOptionType == PaymentOptionType.TD ||
                        SelectedPaymentOption.PaymentOptionType == PaymentOptionType.Elavon))
                    {
                        if (fiskaPaymentResult.ApprovedAmount > 0)
                        {
                            Invoice.TenderAmount = (decimal)fiskaPaymentResult.ApprovedAmount / 100;
                        }

                        ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();
                        InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { Key = InvoicePaymentKey.Fiska, Value = fiskaPaymentResult.ToJson() });

                        var fiska_result = await addIntegratedPaymentToSell(SelectedPaymentOption, fiskaPaymentResult.ReferenceCode, InvoicePaymentDetails);
                        if (fiska_result)
                        {
                            Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                            if (Invoice.TotalPay == 0)
                            {
                                IsAddPaymentActive = !fiska_result;
                                IsSuccessPaymentActive = fiska_result;
                            }
                            else
                            {
                                if (Invoice.NetAmount > Invoice.TotalPay)
                                {
                                    IsPayment_PartialyPaid = true;
                                }
                                else
                                {
                                    IsPayment_PartialyPaid = false;
                                }
                            }
                        }

                    }
                }
                else if (!string.IsNullOrEmpty(fiskaPaymentResult.CustomErrormessage))
                {
                    App.Instance.Hud.DisplayToast(fiskaPaymentResult.CustomErrormessage, Colors.Red, Colors.White);
                    return;
                }
                else if (!string.IsNullOrEmpty(fiskaPaymentResult.TerminalMessage))
                {
                    App.Instance.Hud.DisplayToast(fiskaPaymentResult.TerminalMessage, Colors.Red, Colors.White);
                    return;
                }
                else
                {
                    App.Instance.Hud.DisplayToast("Something went wrong! ", Colors.Red, Colors.White);
                    return;
                }
            }
            catch (Exception ex)
            {
                logRequestDetails.Add("Payment Type", "TD or Elavon");
                logRequestDetails.Add("RegisterId", Settings.CurrentRegister.Id.ToString());
                logRequestDetails.Add("FSKResultCodeSuccess", ResultCode.FSKResultCodeSuccess.ToString());
                logRequestDetails.Add("FSKOperationStatusCompleted", OperationStatus.FSKOperationStatusCompleted.ToString());
                logRequestDetails.Add("Fiska Exception ", ex.ToString());
                logRequestDetails.Add("InvoiceNumber", Invoice.Number);
                SendLogsToServer(SelectedPaymentOption, logRequestDetails);

                Debug.WriteLine("Fiska error :" + ex.ToString());
                Logger.SaleLogger("Fiska_PaymentSuccessed Exception Msg - " + ex.Message);
            }

            if (IsSuccessPaymentActive && Settings.GetCachePrinters != null && Settings.GetCachePrinters.Any(x => x.IsAutoPrintReceipt))
            {
                WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter()));
            }
        }

        private async void Square_PaymentCompleted(SquarePaymentResult paymentResult)
        {
            if (paymentResult == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(paymentResult.ResponseErrorDescription))
            {
                App.Instance.Hud.DisplayToast(paymentResult.ResponseErrorDescription, Colors.Red, Colors.White);
                return;
            }
            if (paymentResult.ResponseStatus == SCCAPIResponseStatus.Ok)
            {
                if (SelectedPaymentOption != null && SelectedPaymentOption.PaymentOptionType == PaymentOptionType.Square)
                {
                    ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();
                    InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { Key = InvoicePaymentKey.SquareReader, Value = paymentResult.ToJson() });

                    var square_result = await addIntegratedPaymentToSell(SelectedPaymentOption, paymentResult.TransactionID, InvoicePaymentDetails);
                    if (square_result)
                    {
                        Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                        if (Invoice.TotalPay == 0)
                        {
                            IsAddPaymentActive = !square_result;
                            IsSuccessPaymentActive = square_result;
                        }
                        else
                        {
                            if (Invoice.NetAmount > Invoice.TotalPay)
                            {
                                IsPayment_PartialyPaid = true;
                            }
                            else
                            {
                                IsPayment_PartialyPaid = false;
                            }
                        }
                    }
                    if (IsSuccessPaymentActive && Settings.GetCachePrinters != null && Settings.GetCachePrinters.Any(x => x.IsAutoPrintReceipt))
                    {
                        WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter()));
                    }

                }
            }
        }

        private async void SquareTerminal_PaymentCompleted(object paymentResult, bool isRefund)
        {
            if (paymentResult == null)
            {
                return;
            }
            SendLogsToServer(SelectedPaymentOption, null, paymentResult);

            ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();

            var transactionId = string.Empty;
            if (isRefund)
            {
                var result = paymentResult as SquareTerminalRefundResponse;

                if (result != null && result.errors == null && result.refund != null)
                {
                    if (!string.IsNullOrEmpty(result.refund.id))
                    {
                        transactionId = result.refund.id;
                        InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { Key = InvoicePaymentKey.SquareTerminalRefund, Value = result.ToJson() });
                    }
                }
            }
            else
            {
                var result = paymentResult as SquareTerminalCheckoutResponse;

                if (result != null && result.errors == null && result.checkout != null)
                {
                    if (result.checkout.payment_ids != null && result.checkout.payment_ids.Length > 0)
                    {

                        transactionId = result.checkout.payment_ids[0];
                        if (transactionId == result.checkout.id)
                            InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { Key = InvoicePaymentKey.SquareCheckoutId, Value = result.checkout.id });
                        else
                            InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { Key = InvoicePaymentKey.SquareTerminalPayment, Value = result.ToJson() });

                    }
                }
            }

            if (!string.IsNullOrEmpty(transactionId))
            {
                var square_result = await addIntegratedPaymentToSell(SelectedPaymentOption, transactionId, InvoicePaymentDetails);

                if (square_result)
                {
                    Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                    if (Invoice.TotalPay == 0)
                    {
                        IsAddPaymentActive = !square_result;
                        IsSuccessPaymentActive = square_result;
                    }
                    else
                    {
                        if (Invoice.NetAmount > Invoice.TotalPay)
                        {
                            IsPayment_PartialyPaid = true;
                        }
                        else
                        {
                            IsPayment_PartialyPaid = false;
                        }
                    }
                }

                if (!isRefund)
                {
                    if (IsSuccessPaymentActive && Settings.GetCachePrinters != null && Settings.GetCachePrinters.Any(x => x.IsAutoPrintReceipt))
                    {
                        WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter()));
                    }
                }

            }
        }


        private async void SaveInLocalbeforePayment(string currentPaymentObject)
        {
            try
            {
                if (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
                {
                    return;
                }

                Invoice.CurrentPaymentObject = SelectedPaymentOption.PaymentOptionName + " : " + currentPaymentObject;
                Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;

                Debug.WriteLine("Local database before payment :" + Newtonsoft.Json.JsonConvert.SerializeObject(Invoice).ToString());
                await saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.PaymentProcessing);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveInLocalbeforePayment : " + ex.ToString());
            }
        }


        public void SetPaymentTagsVisibility()
        {
            try
            {
                var tempPaymentTags = new ObservableCollection<ProductTagDto>();
                IspaymentTagsListvisible = false;
                PaymentTagsListHeight = 0;
                PaymentTags = null;
                if (Invoice.InvoiceLineItems != null && Invoice.InvoiceLineItems.Count() > 0)
                {
                    IspaymentTagsListvisible = true;

                    var totalEffectiveamount = Invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);

                    var discountPercentValue = 0;
                    if (!Invoice.DiscountIsAsPercentage)
                    {
                        var total = ((Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange) ? totalEffectiveamount.ToPositive() : totalEffectiveamount);
                        discountPercentValue = ((int)((Invoice.DiscountValue * 100) / total));
                    }
                    else
                    {
                        discountPercentValue = (int)(Invoice.DiscountValue);
                    }

                    foreach (var item in Invoice.InvoiceLineItems)
                    {
                        var offerDiscountAmount = InvoiceCalculations.GetValuefromPercent(item.EffectiveAmount, item.OfferDiscountPercent);
                        var afterOfferDiscountEffectiveAmount = item.EffectiveAmount - offerDiscountAmount;

                        var finaleffectiveAmount = Invoice.TaxInclusive ? afterOfferDiscountEffectiveAmount : (afterOfferDiscountEffectiveAmount + item.TaxAmount);

                        var discountAmountCalculateEffectiveAmount = finaleffectiveAmount;
                        if (!Invoice.ApplyTaxAfterDiscount)
                            discountAmountCalculateEffectiveAmount = afterOfferDiscountEffectiveAmount;

                        var invoiceLevelDiscount = InvoiceCalculations.GetValuefromPercent(discountAmountCalculateEffectiveAmount, discountPercentValue);
                        var effectiveAmount = finaleffectiveAmount - invoiceLevelDiscount;

                        if (item.InvoiceItemType == InvoiceItemType.Standard || item.InvoiceItemType == InvoiceItemType.UnityOfMeasure || item.InvoiceItemType == InvoiceItemType.CompositeProduct || item.InvoiceItemType == InvoiceItemType.Custom || item.InvoiceItemType == InvoiceItemType.GiftCard)
                        {
                            ObservableCollection<string> tags = null;
                            if (item.Tags != null && item.Tags != "[]")
                            {
                                try
                                {
                                    tags = JsonConvert.DeserializeObject<ObservableCollection<string>>(item.Tags);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                    var productTags = JsonConvert.DeserializeObject<ObservableCollection<ProductTagDto>>(item.Tags);
                                    tags = new ObservableCollection<string>(productTags.Select(x => x.Id).ToList());
                                }
                                foreach (var tag in tags)
                                {
                                    var existingTag = tempPaymentTags.Where(x => tags.Contains(x.Id)).FirstOrDefault();
                                    if (existingTag != null)
                                    {
                                        existingTag.Amount += effectiveAmount;
                                        existingTag.Quantity += (int)item.Quantity;
                                    }
                                    else
                                    {
                                        var localTag = productService.GetLocalProductTag(tag);
                                        var newTag = new ProductTagDto();
                                        newTag.Id = tag;
                                        newTag.Amount = effectiveAmount;
                                        newTag.Quantity = (int)item.Quantity;
                                        newTag.Name = localTag.Name;
                                        newTag.Color = Resources.AppColors.TagColors[tempPaymentTags.Count % 9];
                                        tempPaymentTags.Add(newTag);
                                        PaymentTagsListHeight = PaymentTagsListHeight + 50;
                                    }
                                }
                            }
                            else
                            {
                                var existingTag = tempPaymentTags.Where(x => x.Id == "0").FirstOrDefault();
                                if (existingTag != null)
                                {
                                    existingTag.Amount += effectiveAmount;
                                    existingTag.Quantity += (int)item.Quantity;
                                }
                                else
                                {
                                    var newTag = new ProductTagDto();
                                    newTag.Id = "0";
                                    newTag.Amount = effectiveAmount;
                                    newTag.Quantity = (int)item.Quantity;
                                    newTag.Name = LanguageExtension.Localize("Untagged");
                                    newTag.Color = Resources.AppColors.TagColors[tempPaymentTags.Count % 9];
                                    tempPaymentTags.Add(newTag);
                                    PaymentTagsListHeight = PaymentTagsListHeight + 50;
                                }
                            }
                        }
                        else if (item.InvoiceItemType == InvoiceItemType.Composite)
                        {
                            foreach (var sitem in item.InvoiceLineSubItems)
                            {
                                var product = productService.GetLocalProduct(sitem.Id);
                                if (product?.ProductTags != null && product.ProductTags.Any() && item.Tags != "[]")
                                {
                                    var tags = product.ProductTags.Select(a => a.ToString()).ToList();
                                    foreach (var tag in tags)
                                    {
                                        var existingTag = tempPaymentTags.Where(x => tags.Contains(x.Id)).FirstOrDefault();
                                        if (PaymentTags != null && existingTag != null)
                                        {
                                            existingTag.Amount += effectiveAmount;
                                            existingTag.Quantity += (int)item.Quantity;
                                        }
                                        else
                                        {
                                            var localTag = productService.GetLocalProductTag(tag);
                                            var newTag = new ProductTagDto();
                                            newTag.Id = tag;
                                            newTag.Amount = effectiveAmount;
                                            newTag.Quantity = (int)item.Quantity;
                                            newTag.Name = localTag.Name;
                                            newTag.Color = Resources.AppColors.TagColors[tempPaymentTags.Count % 9];
                                            tempPaymentTags.Add(newTag);
                                            PaymentTagsListHeight = PaymentTagsListHeight + 50;
                                        }
                                    }
                                }
                                else
                                {
                                    var existingTag = tempPaymentTags.Where(x => x.Id == "0").FirstOrDefault();
                                    if (PaymentTags != null && existingTag != null)
                                    {
                                        existingTag.Amount += effectiveAmount;
                                        existingTag.Quantity += (int)item.Quantity;
                                    }
                                    else
                                    {
                                        var newTag = new ProductTagDto();
                                        newTag.Id = "0";
                                        newTag.Amount = effectiveAmount;
                                        newTag.Quantity = (int)item.Quantity;
                                        newTag.Name = LanguageExtension.Localize("Untagged");
                                        newTag.Color = Resources.AppColors.TagColors[tempPaymentTags.Count % 9];
                                        tempPaymentTags.Add(newTag);
                                        PaymentTagsListHeight = PaymentTagsListHeight + 50;
                                    }
                                }
                            }
                        }
                    }
                    if (PaymentTagsListHeight >= 100)
                        PaymentTagsListHeight = 100;
                }
                else
                {
                    IspaymentTagsListvisible = false;
                }
                foreach (var tag in tempPaymentTags)
                {
                    tag.Name = tag.Name + " " + tag.Quantity + " ITEM(S)";

                }
                PaymentTags = tempPaymentTags;
                if (PaymentTags != null && PaymentTags.Count > 1)
                {
                    PaymentTagsListHeight = 100;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion

        public static class AssemblyOrEconduitReceiptData
        {
            public static List<string> AssemblyReceipt = null;
            public static string InvoiceID = null;
        }

        #region Command Click
        async private void BackHandle_Clicked()
        {
            if (IsBusy)
                return;

            if (IsSuccessPaymentActive)
                VantivCloudReceiptList?.Clear();
            await GoToBack();
        }

        async private void addNewSaleHandle_Clicked()
        {
            VantivCloudReceiptList?.Clear();
            await GoToBack();
        }
        private async Task GoToBack()
        {
            try
            {
                //await MainThread.InvokeOnMainThreadAsync(async () =>
                //{
                if (NavigationService != null && NavigationService.NavigationStack != null && NavigationService.NavigationStack.Count > 0)
                {
                    IsBackToPOS = true; //Start #92959 By Pratik
                    await NavigationService.PopAsync();
                    // await Shell.Current.GoToAsync("..",true);
                }
                //});
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }
        private void tenderAmountHandle_Unfocused(Entry sender)
        {
            try
            {
                var val = ((Entry)sender).Text;
                decimal result = 0;
                if (decimal.TryParse(val, out result))
                {

                    if (Invoice?.Status == InvoiceStatus.Refunded
                        && result > 0)
                    {
                        result = result.ToNegative();
                        Invoice.StrTenderAmount = result.ToString();
                    }

                    Invoice.TenderAmount = result;
                }
                else
                {
                    Invoice.TenderAmount = 0;
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ValidTenderAmount"));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TenderAmountHandle_Unfocused" + ex.ToString());
            }

        }

        //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
        private bool IsPaymentClicked;
        private async void selectPaymentHandle_Clicked(PaymentOptionDto paymentOption)
        {
            if (IsPaymentClicked)
                return;
            IsPaymentClicked = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? 2000 : 1000).Wait();
                IsPaymentClicked = false;
            });
            try
            {

                // Start #90942 iOS:FR Cheque number for sale by pratik 
                if (paymentOption.IsRequireCheckIDAtCheckout && string.IsNullOrEmpty(ReferenceID))
                {
                    SelectedPaymentOption = paymentOption;
                    await NavigationService.PushModalAsync(new Pages.ChequeNumberPopupPage());
                    return;
                }
                // End #90942 by pratik

                if ((paymentOption.DisplaySurcharge.HasValue && paymentOption.DisplaySurcharge > 0 && paymentOption.IsDisplaySurchargeWarningOnSale)
                    || (paymentOption.IsDisplaySurchargeWarningOnSale && paymentOption.CanApplySurchageOnRefund && (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)))
                {
                    decimal netvalue = 0;
                    decimal surcharval = 0;
                    if (Invoice.Status == InvoiceStatus.Refunded || (Invoice.Status == InvoiceStatus.Exchange && Invoice.TotalPay < 0))
                    {
                        var surcharge = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == paymentOption.Id && a.InvoicePaymentDetails != null && a.InvoicePaymentDetails.Count > 0)
                                .Sum(a => a.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount)
                                .Sum(a => Convert.ToDecimal(a.Value ?? "0").ToPositive()));

                        var totaltender = Invoice.InvoiceRefundPayments.Where(a => a.PaymentOptionId == paymentOption.Id)
                        .Sum(a => a.Amount) - surcharge;
                        if (totaltender.ToPositive() > 0)
                        {
                            var percentage = Math.Round(((surcharge * 100) / totaltender), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);

                            if (Invoice.TenderAmount.ToPositive() <= totaltender)
                                surcharval = Math.Round(((Invoice.TenderAmount * percentage) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                            else
                                surcharval = Math.Round(((totaltender * percentage) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        }
                        if (Invoice.TenderAmount.ToPositive() <= (Invoice.NetAmount.ToPositive() - Invoice.TotalPaid.ToPositive()))
                        {
                            netvalue = Math.Round(Invoice.TenderAmount, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            netvalue = Math.Round((Invoice.NetAmount - Invoice.TotalPaid), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        }
                        if (surcharval > 0)
                            surcharval = surcharval.ToNegative();
                    }
                    else
                    {
                        if (Invoice.TenderAmount.ToPositive() <= (Invoice.NetAmount - Invoice.TotalPaid))
                        {
                            surcharval = Math.Round(((Invoice.TenderAmount * paymentOption.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                            netvalue = Math.Round(Invoice.TenderAmount, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            surcharval = Math.Round((((Invoice.NetAmount - Invoice.TotalPaid) * paymentOption.Surcharge.Value) / 100), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                            netvalue = Math.Round((Invoice.NetAmount - Invoice.TotalPaid), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        }
                    }
                    SelectedPaymentOption = paymentOption;
                    // Start #90942 iOS:FR Cheque number for sale by pratik 
                    var navpage = new Pages.SurchargePopupPage(netvalue, surcharval);
                    navpage.CancelEvent += (object sender, bool e) =>
                    {
                        ReferenceID = null;
                    };
                    await NavigationService.PushModalAsync(navpage);
                    // End #90942 by pratik
                    return;
                }

                SelectPaymentHandle(paymentOption);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //Start #91991 By Pratik
        private async void AddReferenceNumber(object sender, string e)
        {
            POReferenceID = e;
            IsPaymentClicked = false;
            if (!string.IsNullOrEmpty(POReferenceID) && !string.IsNullOrWhiteSpace(POReferenceID) && Invoice.InvoiceDetail?.Value == null)
            {
                Invoice.InvoiceDetail = new OnAccountPONumberRequest() { Key = 0, Value = POReferenceID };
            }
            var result1 = Invoice.CustomFields != null ? JsonConvert.DeserializeObject<CustomFieldsResponce>(Invoice.CustomFields) : null;
            if (result1?.invoiceOutstanding != null)
            {
                if (Invoice.InvoiceHistories != null
                    && Invoice.InvoiceHistories.Count >= 1
                    && Invoice.InvoiceHistories.Last(x => x.Status != InvoiceStatus.EmailSent).Status == InvoiceStatus.OnAccount
                    && result1.invoiceOutstanding.currentSale.HasValue)
                {
                    CustomerModel.SelectedCustomer.OutStandingBalance -= result1.invoiceOutstanding.currentSale;
                    CustomerModel.SelectedCustomer.OutStandingBalance += Invoice.TenderAmount;
                }
                else
                    CustomerModel.SelectedCustomer.OutStandingBalance -= Invoice.TotalPaid;
            }
            else
                CustomerModel.SelectedCustomer.OutStandingBalance += Invoice.TenderAmount;

            customerService.UpdateLocalCustomer(CustomerModel.SelectedCustomer);

            if (Invoice.Status == InvoiceStatus.Exchange)
            {
                //#97493 
                Invoice.IsExchangeSale = true;
                var UpdateRefunded = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                foreach (var itemRF in Invoice.InvoiceLineItems)
                {
                    itemRF.Id = 0;
                    if (UpdateRefunded != null)
                    {
                        foreach (var item in UpdateRefunded.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount))
                        {
                            if (item.InvoiceItemType == itemRF.InvoiceItemType &&
                            item.InvoiceItemValue == itemRF.InvoiceItemValue && itemRF.Quantity < 0)
                            {
                                item.RefundedQuantity += itemRF.Quantity.ToPositive();
                            }
                        }
                        await saleService.UpdateLocalInvoice(UpdateRefunded);
                    }
                }
                //#97493 
            }
            Invoice.Status = InvoiceStatus.OnAccount;
            IsAddPaymentActive = false;
            IsSuccessPaymentActive = true;
            await sale_onaccountSale();
            await PaymentSuccessCheck(true, SelectedPaymentOption);
        }
        //End #91991 By Pratik

        void SelectPaymentHandle(PaymentOptionDto paymentOption)
        {
            //Ticket start:#61832 iPad:Create text file for invoice log.by rupesh
            Logger.SaleLogger("-------------------------------Request Start--------------------------------");
            Logger.SaleLogger("Payment Type - " + paymentOption.PaymentOptionType.ToString());
            Logger.SaleLogger(Newtonsoft.Json.JsonConvert.SerializeObject(Invoice, Newtonsoft.Json.Formatting.Indented));
            //Ticket start:#61832.by rupesh

            if (paymentOption.PaymentOptionType == PaymentOptionType.Credit ||
                paymentOption.PaymentOptionType == PaymentOptionType.GiftCard)
            {
                try
                {

                    PropertyInfo myPropInfo;
                    bool result = false;
                    if (paymentOption.PaymentOptionType == PaymentOptionType.Credit)
                        myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeCreditNotFeature");
                    else
                        myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeGiftCardFeature");


                    bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out result);

                    if (!result)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreFeatureNotAvailable"), Colors.Red, Colors.White);

                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                if (Invoice.InvoiceLineItems.Any(x => x.InvoiceItemType == InvoiceItemType.GiftCard))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return;
                }

            }

            var hasbackorder = InvoiceCalculations.CheckHasBackOrder(Invoice);
            if (hasbackorder && (Invoice.CustomerId == null
                || Invoice.CustomerId == 0))
            {

                if (Invoice.CustomerDetail != null && (!string.IsNullOrEmpty(Invoice.CustomerDetail.TempId)))
                {
                    Invoice.CustomerTempId = Invoice.CustomerDetail.TempId;
                }

                if (string.IsNullOrEmpty(Invoice.CustomerTempId))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("BackOrderCutomerValidation"), Colors.Red, Colors.White);
                    return;
                }
            }

            if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.GiftCard)
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    GiftCardAfterPayButtonText = " " + LanguageExtension.Localize("ChargeText") + " ";

                    GiftcardLabelText = LanguageExtension.Localize("GiftCardText");
                    GiftCardUserEntryHint = "Enter gift card number here";
                    IsPayment_GiftCardVisible = true;

                    WeakReferenceMessenger.Default.Unregister<Messenger.BarcodeMessenger>(this);
                    if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.BarcodeMessenger>(this))
                    {
                        WeakReferenceMessenger.Default.Register<Messenger.BarcodeMessenger>(this, (sender, arg) =>
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Payment_GiftCardNumber = arg.Value;
                            });
                        });
                    }
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    IsPayment_GiftCardVisible = false;
                }
            }
            else if ((paymentOption.PaymentOptionType == Enums.PaymentOptionType.Afterpay || paymentOption.PaymentOptionType == Enums.PaymentOptionType.Zip)
                && Invoice.Status != InvoiceStatus.Refunded
                && Invoice.TenderAmount > 0)
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        if (Invoice.Status != InvoiceStatus.Refunded)
                        {
                            if (Invoice.Status == InvoiceStatus.Exchange)
                            {
                                if (Invoice.TotalPay > 0)
                                {
                                    App.Instance.Hud.DisplayToast("Afterpay cannot be used to pay difference when exchanged product is of a higher value.");
                                    return;
                                }
                            }
                            else
                            {
                                var outstanding = Invoice.NetAmount.ToPositive() - Invoice.TotalPaid.ToPositive();

                                if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.Zip)
                                {
                                    GiftcardLabelText = "Zip pay";
                                    GiftCardUserEntryHint = "Enter Barcode";
                                    GiftCardAfterPayButtonText = " Confirm & Continue ";
                                    IsPayment_GiftCardVisible = true;
                                    IsGiftCardAfterPayButtonEnabled = false;
                                }
                                else
                                {

                                    if (Math.Round(outstanding, 2, MidpointRounding.AwayFromZero) != Invoice.TenderAmount && Math.Round(Invoice.NetAmount, 2, MidpointRounding.AwayFromZero) != Invoice.TenderAmount)
                                    {
                                        App.Instance.Hud.DisplayToast("Afterpay recommends that the Afterpay component of the sale is requested as the last split tender type.");
                                        return;
                                    }

                                    GiftcardLabelText = "After pay";
                                    GiftCardUserEntryHint = "Enter Barcode";
                                    GiftCardAfterPayButtonText = " Confirm & Continue ";
                                    IsPayment_GiftCardVisible = true;
                                    IsGiftCardAfterPayButtonEnabled = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.SaleLogger("Exception Msg - " + ex.Message);
                    }
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    IsPayment_GiftCardVisible = false;
                }
            }
            else
            {
                var temppaymentOption = paymentOption.Copy();
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                temppaymentOption.DisplayName = paymentOption.DisplayName;
                temppaymentOption.DisplaySubName = paymentOption.DisplaySubName;
                temppaymentOption.DisplaySurcharge = paymentOption.DisplaySurcharge;
                //End ticket #73190  By Rupesh
                if (temppaymentOption.DisplayName == "[MOTO]")
                {
                    temppaymentOption.Name = "Simple Payments Integration";
                    temppaymentOption.IsMoto = true;
                    temppaymentOption.DisplayName = "Simple Payments Integration";
                }
                selectPaymentOption(temppaymentOption);
            }
            SetPaymentListVisiblity();
        }
        //End ticket #73190 by pratik

        private bool isClicked;
        private void AllowOne(Action a)
        {
            if (!isClicked)
            {
                try
                {
                    isClicked = true;
                    a();
                }
                finally
                {
                    isClicked = false;
                }
            }
        }

        public void SetPaymentListVisiblity()
        {
            try
            {
                IspaymentListvisible = false;
                if (Invoice.InvoicePayments != null && Invoice.InvoicePayments.Count() > 0)
                {
                    IspaymentListvisible = true;
                    PaymentDetailListHeight = 0;
                    foreach (var item in Invoice.InvoicePayments)
                    {
                        if (item.PaymentOptionType == PaymentOptionType.Cash
                            || item.PaymentOptionType == PaymentOptionType.Loyalty
                            || item.PaymentOptionType == PaymentOptionType.GiftCard
                            || item.PaymentOptionType == PaymentOptionType.Credit
                            || item.PaymentOptionType == PaymentOptionType.Card
                            || item.PaymentOptionType == PaymentOptionType.Clearent)
                        {
                            item.IsDeletePaymentActive = true;
                        }
                        else
                        {
                            item.IsDeletePaymentActive = false;

                        }

                        PaymentDetailListHeight = PaymentDetailListHeight + 40;
                    }
                    if (PaymentDetailListHeight >= 120)
                        PaymentDetailListHeight = 121;
                }
                else
                {
                    IspaymentListvisible = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void onAccountChargeHandle_Clicked()
        {
            PropertyInfo myPropInfo;
            bool result = false;

            myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeOnAccountFeature");

            bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out result);

            if (!result)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreFeatureNotAvailable"), Colors.Red, Colors.White);

                return;
            }
            selectPaymentOption(new PaymentOptionDto() { PaymentOptionType = Enums.PaymentOptionType.OnAccount });
        }

        private void layByHandle_Clicked()
        {
            selectPaymentOption(new PaymentOptionDto() { PaymentOptionType = Enums.PaymentOptionType.Layby });
        }

        private void backgroundHandle_Tapped()
        {
            if (CustomerModel != null)
                CustomerModel.IsOpenSearchCustomerPopUp = 0;
        }

        async private void giftCardHandle_TextChanged(Entry entry)
        {
            bool result = false;
            string cardNumber = entry.Text;
            if (!string.IsNullOrEmpty(cardNumber))
            {
                if (GiftCardAfterPayButtonText.Trim() != "Confirm & Continue")
                {
                    IsGiftCardAfterPayButtonEnabled = true;
                    result = await GetGiftCardBalance(entry.Text);
                }
                else if (GiftcardLabelText == "Zip pay")
                {
                    if (cardNumber.Count() >= 6 && (cardNumber.Count() <= 18))
                        IsGiftCardAfterPayButtonEnabled = true;
                    else
                        IsGiftCardAfterPayButtonEnabled = false;

                }
                else
                {
                    if (cardNumber.Count() >= 8 && (cardNumber.Count() <= 18))
                        IsGiftCardAfterPayButtonEnabled = true;
                    else
                        IsGiftCardAfterPayButtonEnabled = false;
                }
            }
            else
            {
                IsGiftCardAfterPayButtonEnabled = false;
            }
        }

        void backorderDepositHandle_TextChanged(Entry entry)
        {
            decimal decimalparseresult;
            if (Invoice != null && decimal.TryParse(entry.Text, out decimalparseresult))
                if (string.IsNullOrEmpty(entry.Text))
                {
                    Invoice.StrTenderAmount = Invoice.TotalPay.ToString("C");
                }
                else
                {
                    Invoice.StrTenderAmount = (Invoice.TotalPay + Convert.ToDecimal(entry.Text)).ToString("C");
                }
        }

        void selectPaymentTagHandle_Clicked(ProductTagDto productTag)
        {
            Invoice.StrTenderAmount = productTag.Amount.ToString("C");
        }

        void giftCardHandle_Focused()
        {
        }

        void giftCardHandle_Unfocused()
        {
        }

        #endregion

        //Start #77145 iPAD: Store credit visibility by Pratik
        public override void OnAppearing()
        {
            IsBackToPOS = false; //Start #92959 By Pratik
            if (CustomerModel?.SelectedCustomer?.CreditBalance != null && Invoice?.CustomerDetail?.CreditBalance != null && PaymentOptionList != null && PaymentOptionList.Any(a => a.PaymentOptionType == PaymentOptionType.Credit))
            {
                var credittype = PaymentOptionList.First(a => a.PaymentOptionType == PaymentOptionType.Credit);
                CustomerModel.SelectedCustomer.CreditBalance = Invoice.CustomerDetail.CreditBalance;
                credittype.DisplaySubName = Invoice.CustomerDetail.CreditBalance.Value.ToString("C");
            }
            base.OnAppearing();
        }
        //End #77145 by Pratik

        public void OnAppearingCall()
        {
            Task.Run(async () =>
            {
                if (PaymentOptionList.Count == 0)
                {
                    using (new Busy(this, false))
                    {
                        await Task.Delay(100);
                        loadPaymentDetails();
                    }
                }
                else
                {
                    await Task.Delay(100);
                    loadPaymentDetails();
                }
            });
        }

        async Task loadPaymentDetails()
        {
            try
            {
                dynamic paymentPage;
                if (_navigationService.NavigatedPage is PaymentPagePhone)
                    paymentPage = (PaymentPagePhone)_navigationService.NavigatedPage;
                else
                    paymentPage = (PaymentPage)_navigationService.NavigatedPage;
                bool isTipPermission = true;
                SetPaymentListVisiblity();
                if (Settings.StoreGeneralRule.AutoSuggestPaymentByTag)
                {
                    SetPaymentTagsVisibility();
                }
                updatePaymentList();
                if (Settings.GrantedPermissionNames != null)
                {
                    isTipPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.TogiveTips"));
                    //Start #77581 Edit field should be disable if User is not granted with Permission as per web by Pratik
                    bool isEditPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.Customers.Customer.Edit"));
                    //End #77581  by Pratik
                    if (Invoice != null && Invoice.CustomerId != null && (Invoice.CustomerId > 0 || !string.IsNullOrEmpty(Invoice.CustomerTempId)))
                    {
                        if (isEditPermission)
                            IsBtnEditCustomerVisible = true;
                        else
                            IsBtnEditCustomerVisible = false;
                        IsBtnRemoveCustomerVisible = true;

                        if (Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer && !Settings.IsQuoteSale)
                            IsBtnDeliveryCustomerVisible = CheckDelieveryAddressPermisson();
                        else
                            IsBtnDeliveryCustomerVisible = false;
                    }
                    else
                    {
                        IsBtnEditCustomerVisible = false;
                        IsBtnRemoveCustomerVisible = false;
                        IsBtnDeliveryCustomerVisible = false;
                    }
                }
                else
                {
                    if (Invoice.CustomerId != null && (Invoice.CustomerId > 0 || !string.IsNullOrEmpty(Invoice.CustomerTempId)))
                    {
                        IsBtnEditCustomerVisible = true;
                        IsBtnRemoveCustomerVisible = true;

                        if (Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer && !Settings.IsQuoteSale)
                            IsBtnDeliveryCustomerVisible = CheckDelieveryAddressPermisson();
                        else
                            IsBtnDeliveryCustomerVisible = false;
                    }
                    else
                    {
                        IsBtnEditCustomerVisible = false;
                        IsBtnRemoveCustomerVisible = false;
                        IsBtnDeliveryCustomerVisible = false;
                    }
                }
                //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time.by rupesh
                if (Invoice.IsEditBackOrderFromSaleHistory)
                {
                    IsBtnRemoveCustomerVisible = false;
                }
                //Ticket start:#84289 .by rupesh

                CustomerPrintNo = "Customer # " + Settings.PrintCustomerCurrentNumber;

                IsCloudPrintActive = Settings.IsAllReceiptRegisterActive;

                if (Settings.GetCachePrinters != null)
                {
                    IsPrinterAvailable = Settings.GetCachePrinters.Any(x => x.ActiveDocketPrint || x.PrimaryReceiptPrint);
                }
                else
                {
                    IsPrinterAvailable = false;
                }

                if (Invoice != null && Invoice.Taxgroup != null && Invoice.Taxgroup.Count > 0 && CurrentRegister != null && CurrentRegister.ReceiptTemplate != null)
                {
                    TaxLabelName = CurrentRegister.ReceiptTemplate.TaxLable + "(" + string.Join(",", Invoice.Taxgroup.Select(x => x.TaxName)) + ")";
                }
                else
                {
                    if (CurrentRegister != null && CurrentRegister.ReceiptTemplate != null)
                    {
                        TaxLabelName = CurrentRegister.ReceiptTemplate.TaxLable;
                    }
                    else
                    {
                        TaxLabelName = "";
                    }
                }

                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                if (Invoice != null)
                {
                    Invoice.TotalTipSurcharge = Invoice.TotalTip;
                    if (Invoice.Status == InvoiceStatus.LayBy)
                        Invoice.TotalTipSurcharge += Invoice.TotalPaymentSurcharge;
                }
                //End Ticket #73190 By: Pratik

                if (Invoice != null && Invoice.Status != Enums.InvoiceStatus.Refunded)
                    IsQuickCashVisible = true;
                else
                    IsQuickCashVisible = false;

                var hasbackorder = InvoiceCalculations.CheckHasBackOrder(Invoice);
                if (hasbackorder)
                    Invoice.StrTenderAmount = (Invoice.TotalPay + Invoice.BackorderDeposite).ToString("C");
                else 
                    Invoice.StrTenderAmount = Invoice.TotalPay.ToString("C");



                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.SurchargeApplyMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.SurchargeApplyMessenger>(this, (sender, arg) =>
                    {
                        SelectPaymentHandle(SelectedPaymentOption);
                    });
                }
                //End ticket #73190

                // Start #90942 iOS:FR Cheque number for sale by pratik
                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.ChequeNumberMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.ChequeNumberMessenger>(this, (sender, arg) =>
                    {
                        ReferenceID = arg.Value;
                        IsPaymentClicked = false;
                        selectPaymentHandle_Clicked(SelectedPaymentOption);
                    });
                }
                // End #90942 by pratik


                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.BackgroundInvoiceUpdatedMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.BackgroundInvoiceUpdatedMessenger>(this, (sender, arg) =>
                    {
                        InvoiceDto invoice = arg.Value;
                        if (Invoice != null && invoice.InvoiceTempId != null && Invoice.InvoiceTempId == invoice.InvoiceTempId)
                        {
                            Debug.WriteLine("Before BackgroundInvoiceUpdated... " + Newtonsoft.Json.JsonConvert.SerializeObject(Invoice));

                            Invoice.Id = invoice.Id;
                            EnableSendEmail = true;

                            Invoice.InvoiceHistories = invoice.InvoiceHistories;
                            Debug.WriteLine("after BackgroundInvoiceUpdated... " + Newtonsoft.Json.JsonConvert.SerializeObject(Invoice));
                        }
                        else if (Invoice != null && invoice.InvoiceTempId != null && invoice.Id > 0 && (invoice.Status == InvoiceStatus.Refunded || invoice.Status == InvoiceStatus.Exchange))
                        {
                            Invoice.Id = invoice.Id;
                            EnableSendEmail = true;
                        }
                    });
                }

                CustomerModel.PropertyChanged += CustomerModel_PropertyChanged;

                ShopGeneralRule = Settings.StoreGeneralRule;

                GeneralShopDto = Settings.StoreShopDto;
                Subscription = Settings.Subscription;
                CurrentUser = Settings.CurrentUser;

                if (isTipPermission)
                    IsLblTipEnabled = ShopGeneralRule.EnableTips;

                if (Invoice != null && Invoice.Status == InvoiceStatus.OnAccount)
                {
                    IsBtnEditCustomerVisible = false;
                    IsBtnRemoveCustomerVisible = false;
                }

                if (Invoice != null && Invoice.InvoiceLineItems != null)
                {
                    if (Invoice.InvoiceLineItems.Sum(x => x.BackOrderQty) > 0)
                    {
                        if (Invoice.NetAmount == 0.0m)
                            IsStrTenderAmountEntryEnabled = false;
                        else
                            IsStrTenderAmountEntryEnabled = true;
                    }
                    else
                    {
                        IsStrTenderAmountEntryEnabled = true;
                    }
                }
                IsBackorderDepositEntryEnabled = true;
                IsGiftCardUserEntryEnabled = true;
                IsTxtEmailEnabled = true;
                if (CustomerModel != null && CustomerModel.IsOpenSearchCustomerPopUp == 1)
                    paymentPage._CustomerPopup?.FocusOnEntry();

                IsQuoteSale = Settings.IsQuoteSale;

                IsToPrintGiftReceipt = false;

                //Ticket start:#74633 iOS: Print Delivery Docket and Invoice together (FR) .by pratik
                IsToPrintDeliveryDocket = false;
                IsToPrintInvoiceReceipt = true;
                //Ticket end:#74633 by pratik
                isClicked = false;
                IsPaymentClicked = false;
                await VerifyNadaPayTerminalPendingSale();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error : " + ex.ToString());
            }
        }

        private bool CheckDelieveryAddressPermisson()
        {
            try
            {
                PropertyInfo myPropInfo;
                bool result = false;
                myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeCustomerDeliverAddressFeature");
                bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out result);

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return true;
            }
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            // if (NavigationService.ModalStack.Count > 0)
            // {
            //     return;
            // }
            if (IsBackToPOS) //Start #92959 By Pratik
            {
                IsBackToPOS = false; //Start #92959 By Pratik
                //start #71929 Pratik
                backgroundHandle_Tapped();
                //end #71929 Pratik
                dynamic paymentPage;
                if (_navigationService.NavigatedPage is PaymentPagePhone)
                    paymentPage = (PaymentPagePhone)_navigationService.NavigatedPage;
                else
                    paymentPage = (PaymentPage)_navigationService.NavigatedPage;

                IsPayment_PartialyPaid = false;
                // if (IsSuccessPaymentActive || Invoice == null)
                // {
                //     IsAddPaymentActive = true;
                // }

                WeakReferenceMessenger.Default.Unregister<Messenger.ManualPrintMessenger>(this);
                WeakReferenceMessenger.Default.Unregister<Messenger.OnlyVantivPrintMessenger>(this);
                WeakReferenceMessenger.Default.Unregister<Messenger.IncludeVantivPrintMessenger>(this);
                WeakReferenceMessenger.Default.Unregister<Messenger.AutoPrintMessenger>(this);
                WeakReferenceMessenger.Default.Unregister<Messenger.PaypalAutoPrintMessenger>(this);
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                WeakReferenceMessenger.Default.Unregister<Messenger.SurchargeApplyMessenger>(this);
                //End #73190

                //Start Ticket #90942 iOS:FR Cheque number for sale By: Pratik
                ReferenceID = null;
                WeakReferenceMessenger.Default.Unregister<Messenger.ChequeNumberMessenger>(this);
                //End Ticket #90942 By: Pratik

                WeakReferenceMessenger.Default.Unregister<Messenger.BackgroundInvoiceUpdatedMessenger>(this);
                IsPayment_GiftCardVisible = false;
                Payment_GiftCardNumber = "";

                CustomerModel.PropertyChanged -= CustomerModel_PropertyChanged;
                IsStrTenderAmountEntryEnabled = false;
                IsBackorderDepositEntryEnabled = false;
                IsGiftCardUserEntryEnabled = false;
                IsTxtEmailEnabled = false;
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    paymentPage._CustomerPopup?.UnFocusOnEntry();
                WeakReferenceMessenger.Default.Unregister<Messenger.BarcodeMessenger>(this);
            }
        }

        public void CustomerNameLableChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                bool isEditPermission = true;
                if (Settings.GrantedPermissionNames != null)
                {
                    //Start #77581 Edit field should be disable if User is not granted with Permission as per web by Pratik
                    isEditPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.Customers.Customer.Edit"));
                    //End #77581 by Pratik
                }
                if (Invoice != null && Invoice.CustomerId != null && Invoice.CustomerId > 0)
                {
                    if (isEditPermission)
                        IsBtnEditCustomerVisible = true;
                    else
                        IsBtnEditCustomerVisible = false;
                    IsBtnRemoveCustomerVisible = true;

                    if (Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer && !Settings.IsQuoteSale)
                    {
                        IsBtnDeliveryCustomerVisible = CheckDelieveryAddressPermisson();
                    }
                }
                else
                {
                    IsBtnEditCustomerVisible = false;
                    IsBtnRemoveCustomerVisible = false;
                    IsBtnDeliveryCustomerVisible = false;
                }
            }
        }

        public void CustomerModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            dynamic paymentPage;
            if (_navigationService.NavigatedPage is PaymentPagePhone)
                paymentPage = (PaymentPagePhone)_navigationService.NavigatedPage;
            else
                paymentPage = (PaymentPage)_navigationService.NavigatedPage;

            if (e.PropertyName == "IsOpenSearchCustomerPopUp")
            {
                if (CustomerModel.IsOpenSearchCustomerPopUp == 1)
                {
                    paymentPage._CustomerPopup?.FocusOnEntry();
                }
            }
        }

        //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
        private decimal CashPaymentRoundingAmount()
        {
            var outstanding = (Invoice.NetAmount.ToPositive() - Invoice.TotalPaid.ToPositive());
            var amount = Invoice.TenderAmount;

            if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent)
            {
                if (Settings.StoreGeneralRule.RoundUptoCent == "1")
                {
                    if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUptoTencentOnRefund())
                    {
                        amount = outstanding;
                    }
                    else if (amount == outstanding.RoundUptoTencent())
                    {
                        amount = outstanding;
                    }
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "2")
                {
                    if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUptoZerocent())
                    {
                        amount = outstanding;
                    }
                    else if (amount == outstanding.RoundUptoZerocent())
                    {
                        amount = outstanding;
                    }
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "3")
                {
                    if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUptoFiftycent())
                    {
                        amount = outstanding;
                    }
                    else if (amount == outstanding.RoundUptoFiftycent())
                    {
                        amount = outstanding;
                    }
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "4")
                {
                    if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUpto10cent())
                    {
                        amount = outstanding;
                    }
                    else if (amount == outstanding.RoundUpto10cent())
                    {
                        amount = outstanding;
                    }
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "5")
                {
                    if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUpto100cent())
                    {
                        amount = outstanding;
                    }
                    else if (amount == outstanding.RoundUpto100cent())
                    {
                        amount = outstanding;
                    }
                }
                else
                {
                    if (Invoice.Status == InvoiceStatus.Refunded && amount == outstanding.RoundUptoFivecentOnRefund())
                    {
                        amount = outstanding;
                    }
                    else if (amount == outstanding.RoundUptoFivecent())
                    {
                        amount = outstanding;
                    }
                }
            }

            decimal RoundingAmount = 0;

            if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent)
            {
                if (Settings.StoreGeneralRule.RoundUptoCent == "1")
                {
                    RoundingAmount = (amount.RoundUptoTencent() - amount);
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "2")
                {
                    RoundingAmount = (amount.RoundUptoZerocent() - amount);

                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "3")
                {
                    RoundingAmount = (amount.RoundUptoFiftycent() - amount);
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "4")
                {
                    RoundingAmount = (amount.RoundUpto10cent() - amount);
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "5")
                {
                    RoundingAmount = (amount.RoundUpto100cent() - amount);
                }
                else
                {
                    RoundingAmount = (amount.RoundUptoFivecent() - amount);

                }

            }

            amount = amount + RoundingAmount;
            return amount;

        }

        private decimal NetAmountRoundingAmount()
        {
            var otherpay = Invoice.InvoicePayments == null ? 0 : Invoice.InvoicePayments.Where(a => a.PaymentOptionType != PaymentOptionType.Cash).Sum(a => a.Amount);
            var amount = Invoice.NetAmount - otherpay;
            decimal RoundingAmount = 0;
            if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent)
            {
                if (Settings.StoreGeneralRule.RoundUptoCent == "1")
                {
                    RoundingAmount = (amount.RoundUptoTencent() - amount);
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "2")
                {
                    RoundingAmount = (amount.RoundUptoZerocent() - amount);
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "3")
                {
                    RoundingAmount = (amount.RoundUptoFiftycent() - amount);
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "4")
                {
                    RoundingAmount = (amount.RoundUpto10cent() - amount);
                }
                else if (Settings.StoreGeneralRule.RoundUptoCent == "5")
                {
                    RoundingAmount = (amount.RoundUpto100cent() - amount);
                }
                else
                {
                    RoundingAmount = (amount.RoundUptoFivecent() - amount);
                }
            }
            return RoundingAmount;
        }
        //Ticket end:#91262 .by rupesh
        private async Task VerifyNadaPayTerminalPendingSale()
        {

            if (Settings.HikePayVerify != null && Settings.HikePayVerify.InvoiceSyncReference == Invoice?.InvoiceTempId)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    using (new Busy(this, false))
                    {
                        try
                        {
                            var paymentOption = paymentService.GetLocalPaymentOption(Settings.HikePayVerify.PaymentId);
                            if (paymentOption != null)
                            {

                                var lastReference = Settings.HikePayVerify.SaleReference;
                                var amount = (Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0));
                                HikePayTerminalResponse saleResult = null;
                                if (amount > 0)
                                {
                                    var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
                                    // Ensure service instance
                                    HikeTerminalPay ??=  DependencyService.Get<INadaPayTerminalLocalAppService>();

                                    saleResult = await HikeTerminalPay.VerifyNadaPayTerminalSale(amount, Invoice.Currency, lastReference, paymentOption, Settings.HikePayVerify.ServiceId);

                                    if (saleResult?.DecodeAdditonalResponse != null && saleResult.PaymentStatusSuccess)
                                    {
                                        var merchantReceipt = string.Empty;
                                        var customerReceipt = string.Empty;
                                        if (!string.IsNullOrEmpty(saleResult.CustomerReceipt))
                                        {
                                            customerReceipt = saleResult.CustomerReceipt;
                                            if (config.IsPrintCustomerReceipt)
                                                VantivCloudReceiptList.Add(saleResult.CustomerReceipt);
                                        }
                                        if (!string.IsNullOrEmpty(saleResult.MerchantReceipt))
                                        {
                                            merchantReceipt = saleResult.MerchantReceipt;
                                            if (config.IsPrintMerchantReceipt)
                                                VantivCloudReceiptList.Add(saleResult.MerchantReceipt);
                                        }

                                        var transactionResult = JsonConvert.SerializeObject(saleResult, new JsonSerializerSettings
                                        {
                                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                                        });
                                        var amt = Convert.ToDecimal(saleResult?.DecodeAdditonalResponse?.additionalData.posAuthAmountValue ?? "0") / 100m;
                                        var surchargeamt = Convert.ToDecimal(saleResult?.DecodeAdditonalResponse?.additionalData.surchargeAmount ?? "0") / 100m;
                                        var nadaPayTransactionDto = new NadaPayTransactionDto
                                        {
                                            PoiTransactionId = saleResult?.PoiTransactionId,
                                            PoiTransactionTimeStamp = saleResult?.PoiTransactionTimeStamp,
                                            SaleTransactionId = saleResult?.DecodeAdditonalResponse?.additionalData?.merchantReference,
                                            SaleTransactionTimeStamp = saleResult?.DecodeAdditonalResponse?.additionalData?.iso8601TxDate,
                                            PspReference = saleResult?.DecodeAdditonalResponse?.additionalData?.pspReference,
                                            Amount = amt,
                                            MaskedPan = saleResult?.DecodeAdditonalResponse?.additionalData?.cardSummary ?? "Card",
                                            SurchargeAmount = surchargeamt,
                                            PaymentMethod = saleResult?.DecodeAdditonalResponse?.additionalData?.paymentMethod
                                        };

                                        ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>
                            {
                                new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePayResponse, Value = transactionResult },
                                new InvoicePaymentDetailDto { Key = InvoicePaymentKey.HikePaySaleResponseData, Value = JsonConvert.SerializeObject(nadaPayTransactionDto, new JsonSerializerSettings
                                    {
                                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                                    }) },
                                new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayMerchantPrint, Value = merchantReceipt },
                                new InvoicePaymentDetailDto { Key = InvoicePaymentKey.hikePayCustomerPrint, Value = customerReceipt }
                            };
                            var result = await addIntegratedPaymentToSell(paymentOption, transactionResult, InvoicePaymentDetails);
                            if (result)
                            {
                                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                                if (Invoice.TotalPay == 0)
                                {
                                    IsAddPaymentActive = !result;
                                    IsSuccessPaymentActive = result;
                                }
                                else
                                {
                                    if (Invoice.NetAmount > Invoice.TotalPay)
                                    {
                                        IsPayment_PartialyPaid = true;
                                    }
                                    else
                                    {
                                        IsPayment_PartialyPaid = false;
                                    }
                                }
                            }
                            Logger.SyncLogger("----Hikepay Status---\n" + "Success" + "\n----HikepayTapToPay response---\n" + transactionResult);
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.SyncLogger("----HikeTerminalpay Status---\n" + "Failed" + "\n---- HikeTerminalpay response---\n" + ex.Message);

                        }

                    }
                });
            }
        }
    }
    
}