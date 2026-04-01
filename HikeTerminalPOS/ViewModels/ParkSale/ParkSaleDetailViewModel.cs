using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using System.Threading.Tasks;
using System.Linq;
using HikePOS.Model;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Messaging;
using HikePOS.Interfaces;
using System.Diagnostics;
using System.Text.RegularExpressions;
using HikePOS.Models.Payment;
using HikePOS.Services.Payment;
using Fusillade;
using HikePOS.Resources;
using HikePOS.Models.Sales;
using System.Reflection;
#if IOS  
using UIKit;
#endif

namespace HikePOS.ViewModels
{
    public class ParkSaleDetailViewModel : BaseViewModel
    {
        #region Services object
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        SaleServices saleService;

        ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
        OutletServices outletServices;

        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;

        ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        PaymentServices paymentService;

        ApiService<IProductApi> ProductApiService = new ApiService<IProductApi>();
        ProductServices productService;

        ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
        UserServices userService;

        #endregion

        #region Properties

        private bool _displayRegisterName;
        public bool DisplayRegisterName { get { return _displayRegisterName; } set { _displayRegisterName = value; SetPropertyChanged(nameof(DisplayRegisterName)); } }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        private bool _isFulfilmentVisible;
        public bool IsFulfilmentVisible { get { return _isFulfilmentVisible; } set { _isFulfilmentVisible = value; SetPropertyChanged(nameof(IsFulfilmentVisible)); } }

        private bool _isFulfilmentDisplay;
        public bool IsFulfilmentDisplay { get { return _isFulfilmentDisplay; } set { _isFulfilmentDisplay = value; SetPropertyChanged(nameof(IsFulfilmentDisplay)); } }

        //End #84293 by Pratik

        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        private bool _isServedBy;
        public bool IsServedBy { get { return _isServedBy; } set { _isServedBy = value; SetPropertyChanged(nameof(IsServedBy)); } }
        //end #84287 .by Pratik

      

        public ParkSaleDetailPage parkSaleDetailPage => (ParkSaleDetailPage)NavigationService.ModalStack.FirstOrDefault();
        public bool inPorgress { get; set; } = false;

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public bool EnableInvoiceDueDate => Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.ChangedInvoiceDueDate");
        public bool IsInvoiceDueDate
        {
            get
            {
                if (Invoice != null && Invoice.Status == InvoiceStatus.OnAccount && Invoice.InvoiceDueDate.HasValue)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        private DateTime _invoiceDueDate;
        public DateTime InvoiceDueDate
        {
            get
            {
                return _invoiceDueDate;
            }
            set
            {
                _invoiceDueDate = value;
                SetPropertyChanged(nameof(InvoiceDueDate));
            }
        }

        private DateTime _minInvoiceDueDate;
        public DateTime MinInvoiceDueDate
        {
            get
            {
                return _minInvoiceDueDate;
            }
            set
            {
                _minInvoiceDueDate = value;
                SetPropertyChanged(nameof(MinInvoiceDueDate));
            }
        }
        public DateTime? olddate { get; set; }
        public DateTime? newDate { get; set; }
        //End ticket #76208 by Pratik

        InvoiceDto _Invoice { get; set; }
        public InvoiceDto Invoice
        {
            get
            {
                return _Invoice;
            }
            set
            {
                _Invoice = value;
                SetPropertyChanged(nameof(Invoice));
                SetPropertyChanged(nameof(IsInvoiceReopen));
                SetPropertyChanged(nameof(IsMarkedAsCompleted));
                SetPropertyChanged(nameof(IsDiscard));
                //START ticket #76208 IOS:FR:Terms of payments by Pratik
                InvoiceDueDate = value?.InvoiceDueDate == null ? DateTime.MinValue : value.InvoiceDueDate.Value.ToStoreTime();
                MinInvoiceDueDate = DateTime.Now.Date;
                SetPropertyChanged(nameof(IsInvoiceDueDate));
                SetPropertyChanged(nameof(EnableInvoiceDueDate));
                //End ticket #76208 by Pratik
            }
        }
        public bool IsInvoiceReopen
        {
            get
            {
                if (Invoice != null && Invoice.Status == InvoiceStatus.Quote && Invoice.FinancialStatus != FinancialStatus.Closed)
                {
                    //Start ticket #76209 iOS:FR: User Permission : Add a permission for allowing 'Issue a Quote' By Pratik
                    return Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.Quote");
                    //End ticket #76209 By Pratik
                }

                if (Invoice != null && Invoice.Status == InvoiceStatus.OnGoing) // #94565;
                    return true;

                if (Invoice != null && Invoice.OutletId != Settings.SelectedOutletId)
                    return false;

                if (Invoice != null && Invoice.Status == InvoiceStatus.BackOrder)
                    return true;

                if (Invoice != null && Invoice.Status == InvoiceStatus.Pending && Invoice.NetAmount > Invoice.TotalPaid)
                    return true;

                if (Invoice != null && (Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.OnAccount || Invoice.Status == InvoiceStatus.LayBy) && (Invoice.NetAmount > Invoice.TotalPaid))
                    return true;

                if (Invoice != null && ((Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy) && Invoice.InvoicePayments.Count() == 0))
                    return true;

                return false;
            }
        }

        public bool IsDiscard
        {
            get
            {
                if (Invoice != null)
                {
                    if (Invoice.Status == InvoiceStatus.Quote && Invoice.FinancialStatus != FinancialStatus.Closed)
                    {
                        return true;
                    }

                    if (Invoice.OutletId != Settings.SelectedOutletId)
                        return false;

                    if (Invoice.Status == InvoiceStatus.BackOrder && Invoice.TotalPaid <= 0)
                        return true;

                    var giftCardItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.GiftCard);
                    if (Invoice.InvoiceLineItems.Count() == giftCardItem.Count() && Invoice.Status != InvoiceStatus.Voided)
                        return true;
                }
                return false;
            }
        }

        public bool IsMarkedAsCompleted
        {
            get
            {
                if (Invoice != null && Invoice.Status != InvoiceStatus.BackOrder && Invoice.NetAmount == Invoice.TotalPaid && (Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy || Invoice.Status == InvoiceStatus.Pending) && Invoice.InvoicePayments.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private GeneralRuleDto _shopGeneralRule;
        public GeneralRuleDto ShopGeneralRule { get { return _shopGeneralRule; } set { _shopGeneralRule = value; SetPropertyChanged(nameof(ShopGeneralRule)); } }

        private ShopDto _GeneralShopDto;
        public ShopDto GeneralShopDto { get { return _GeneralShopDto; } set { _GeneralShopDto = value; SetPropertyChanged(nameof(GeneralShopDto)); } }

        private SubscriptionDto _subscription;
        public SubscriptionDto Subscription { get { return _subscription; } set { _subscription = value; SetPropertyChanged(nameof(Subscription)); } }

        private string _TaxLabelName;
        public string TaxLabelName { get { return _TaxLabelName; } set { _TaxLabelName = value; SetPropertyChanged(nameof(TaxLabelName)); } }

        private OutletDto_POS _currentOutlat;
        public OutletDto_POS CurrentOutlet
        {
            get { return _currentOutlat; }
            set
            {
                _currentOutlat = value;
                SetPropertyChanged(nameof(CurrentOutlet));
            }
        }

        private RegisterDto _currentRegister;
        public RegisterDto CurrentRegister
        {
            get { return _currentRegister; }
            set
            {
                _currentRegister = value;
                SetPropertyChanged(nameof(CurrentRegister));
            }
        }

        //Start TICKET #76698 iPAD FR: Next Sale by Pratik
        private bool _PreviousEnable = false;
        public bool PreviousEnable { get { return _PreviousEnable; } set { _PreviousEnable = value; SetPropertyChanged(nameof(PreviousEnable)); } }

        private bool _nextEnable = false;
        public bool NextEnable { get { return _nextEnable; } set { _nextEnable = value; SetPropertyChanged(nameof(NextEnable)); } }

        private ObservableRangeCollection<InvoiceDto> _ParkSales;
        public ObservableRangeCollection<InvoiceDto> ParkSales { get { return _ParkSales; } set { _ParkSales = value; SetPropertyChanged(nameof(ParkSales)); } }
        public int CurrentIndex { get; set; }

        public ICommand PreviousSaleCommand => new Command(PreviousSaleClick);
        public ICommand NextSaleCommand => new Command(NextSaleClick);
        //End TICKET #76698 by Pratik

        private ImageSource _BarcodeImage;
        public ImageSource BarcodeImage { get { return _BarcodeImage; } set { _BarcodeImage = value; SetPropertyChanged(nameof(BarcodeImage)); } }

        //Ticket start:#39934 iPad: Feature request - How to meet with ZATCA requirement.by rupesh
        private ImageSource _QRCodeImage;
        public ImageSource QRCodeImage { get { return _QRCodeImage; } set { _QRCodeImage = value; SetPropertyChanged(nameof(QRCodeImage)); } }
        //Ticket end:#39934 .by rupesh

        private bool _IsOrderDetailVisible;
        public bool IsOrderDetailVisible { get { return _IsOrderDetailVisible; } set { _IsOrderDetailVisible = value; SetPropertyChanged(nameof(IsOrderDetailVisible)); } }


        private bool _IsPaymentSummaryVisible;
        public bool IsPaymentSummaryVisible { get { return _IsPaymentSummaryVisible; } set { _IsPaymentSummaryVisible = value; SetPropertyChanged(nameof(IsPaymentSummaryVisible)); } }

        private bool _IsSaleHistoryVisible;
        public bool IsSaleHistoryVisible { get { return _IsSaleHistoryVisible; } set { _IsSaleHistoryVisible = value; SetPropertyChanged(nameof(IsSaleHistoryVisible)); } }

        private bool _IsRefundSummaryVisible;
        public bool IsRefundSummaryVisible { get { return _IsRefundSummaryVisible; } set { _IsRefundSummaryVisible = value; SetPropertyChanged(nameof(IsRefundSummaryVisible)); } }

        private bool _IsExchangeSummaryVisible;
        public bool IsExchangeSummaryVisible { get { return _IsExchangeSummaryVisible; } set { _IsExchangeSummaryVisible = value; SetPropertyChanged(nameof(IsExchangeSummaryVisible)); } }

        private bool _IsEditVisible;
        public bool IsEditVisible { get { return _IsEditVisible; } set { _IsEditVisible = value; SetPropertyChanged(nameof(IsEditVisible)); } }


        #region Customer properties
        private CustomerViewModel _CustomerModel;
        public CustomerViewModel CustomerModel { get { return _CustomerModel; } set { _CustomerModel = value; SetPropertyChanged(nameof(CustomerModel)); } }
        #endregion

        private int _IsOpenBackground = 0;
        public int IsOpenBackground { get { return _IsOpenBackground; } set { _IsOpenBackground = value; SetPropertyChanged(nameof(IsOpenBackground)); } }

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

        private Color _exchangeButtonTextColor;
        public Color ExchangeButtonTextColor { get { return _exchangeButtonTextColor; } set { _exchangeButtonTextColor = value; SetPropertyChanged(nameof(ExchangeButtonTextColor)); } }

        private bool _removePaymentPermission;
        public bool RemovePaymentPermission { get { return _removePaymentPermission; } set { _removePaymentPermission = value; SetPropertyChanged(nameof(RemovePaymentPermission)); } }

        private bool _isHavingRefund = false;
        public bool IsHavingRefund { get { return _isHavingRefund; } set { _isHavingRefund = value; SetPropertyChanged(nameof(IsHavingRefund)); } }

        private bool _isHavingExchange = false;
        public bool IsHavingExchange { get { return _isHavingExchange; } set { _isHavingExchange = value; SetPropertyChanged(nameof(IsHavingExchange)); } }

        private bool _isInvoiceRefundAndDiscard = false;
        public bool IsInvoiceRefundAndDiscard { get { return _isInvoiceRefundAndDiscard; } set { _isInvoiceRefundAndDiscard = value; SetPropertyChanged(nameof(IsInvoiceRefundAndDiscard)); } }

        private bool _customerNameIsEnabled;
        public bool CustomerNameIsEnabled { get { return _customerNameIsEnabled; } set { _customerNameIsEnabled = value; SetPropertyChanged(nameof(CustomerNameIsEnabled)); } }


        private UserListDto _currentUser;
        public UserListDto CurrentUser { get { return _currentUser; } set { _currentUser = value; SetPropertyChanged(nameof(CurrentUser)); } }

        string _receiptInfo { get; set; }
        public string ReceiptInfo { get { return _receiptInfo; } set { _receiptInfo = value; SetPropertyChanged(nameof(ReceiptInfo)); } }

        private ObservableCollection<RefundSummary> _RefundHistory;
        public ObservableCollection<RefundSummary> RefundHistory { get { return _RefundHistory; } set { _RefundHistory = value; SetPropertyChanged(nameof(RefundHistory)); } }

        private ObservableCollection<RefundSummary> _ExchangeHistory;
        public ObservableCollection<RefundSummary> ExchangeHistory { get { return _ExchangeHistory; } set { _ExchangeHistory = value; SetPropertyChanged(nameof(ExchangeHistory)); } }

        RefundAndVoidPage refundAndVoidPage;

        private ObservableCollection<InvoiceHistoryDto> _InvoiceHistories;
        public ObservableCollection<InvoiceHistoryDto> InvoiceHistories { get { return _InvoiceHistories; } set { _InvoiceHistories = value; SetPropertyChanged(nameof(InvoiceHistories)); } }

        private bool isArabicCulture = false;
        public bool IsArabicCulture
        {
            get
            {
                return isArabicCulture;
            }
            set
            {
                isArabicCulture = value;
                SetPropertyChanged(nameof(IsArabicCulture));
            }
        }

        private bool _IsQuoteDuplicate = false;
        public bool IsQuoteDuplicate { get { return _IsQuoteDuplicate; } set { _IsQuoteDuplicate = value; SetPropertyChanged(nameof(IsQuoteDuplicate)); } }

        private bool _IsQuoteConvertToSale = false;
        public bool IsQuoteConvertToSale { get { return _IsQuoteConvertToSale; } set { _IsQuoteConvertToSale = value; SetPropertyChanged(nameof(IsQuoteConvertToSale)); } }

        private ObservableCollection<string> _SelectedEmails = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedEmails { get { return _SelectedEmails; } set { _SelectedEmails = value; SetPropertyChanged(nameof(SelectedEmails)); } }

        private ObservableCollection<SelectEmail> _SelectEmails = new ObservableCollection<SelectEmail>();
        public ObservableCollection<SelectEmail> SelectEmails { get { return _SelectEmails; } set { _SelectEmails = value; SetPropertyChanged(nameof(SelectEmails)); } }

        public ObservableCollection<SelectEmail> CustomerEmails = new ObservableCollection<SelectEmail>();

        private bool _IsToPrintGiftReceipt = false;
        public bool IsToPrintGiftReceipt 
        { 
            get { return _IsToPrintGiftReceipt; } 
            set 
            { 
                _IsToPrintGiftReceipt = value; 
                SetPropertyChanged(nameof(IsToPrintGiftReceipt));
            } 
        }

        PickAndPackPage pickAndPackPage = null;

        private double deliveryDocketPrintItemListHeight;
        public double DeliveryDocketPrintItemListHeight
        {
            get { return deliveryDocketPrintItemListHeight; }
            set
            {
                deliveryDocketPrintItemListHeight = value;
                SetPropertyChanged(nameof(DeliveryDocketPrintItemListHeight));
            }
        }

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

        private bool _IsSelectedEmailListVisible;
        public bool IsSelectedEmailListVisible
        {
            get
            {
                return _IsSelectedEmailListVisible;
            }
            set
            {
                _IsSelectedEmailListVisible = value;
                SetPropertyChanged(nameof(IsSelectedEmailListVisible));
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
                IsSelectedEmailListVisible = !IsEmailWithPayLink || IsToPrintGiftReceipt;
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


        private bool _IsSelectEmailListVisible;
        public bool IsSelectEmailListVisible
        {
            get
            {
                return _IsSelectEmailListVisible;
            }
            set
            {
                _IsSelectEmailListVisible = value;
                SetPropertyChanged(nameof(IsSelectEmailListVisible));
            }
        }

        private double _SelectedEmailListHeight;
        public double SelectedEmailListHeight
        {
            get
            {
                return _SelectedEmailListHeight;
            }
            set
            {
                _SelectedEmailListHeight = value;
                SetPropertyChanged(nameof(SelectedEmailListHeight));
            }
        }

        private double _SelectEmailListHeight;
        public double SelectEmailListHeight
        {
            get
            {
                return _SelectEmailListHeight;
            }
            set
            {
                _SelectEmailListHeight = value;
                SetPropertyChanged(nameof(SelectEmailListHeight));
            }
        }

        private string _BtnEmailText = "Email";
        public string BtnEmailText
        {
            get
            {
                return _BtnEmailText;
            }
            set
            {
                _BtnEmailText = value;
                SetPropertyChanged(nameof(BtnEmailText));
            }
        }

        private bool _IsFrmEmailVisible;
        public bool IsFrmEmailVisible
        {
            get
            {
                return _IsFrmEmailVisible;
            }
            set
            {
                _IsFrmEmailVisible = value;
                SetPropertyChanged(nameof(IsFrmEmailVisible));
            }
        }

        private bool _isCustomerRemoveVisible;
        public bool IsCustomerRemoveVisible
        {
            get
            {
                return _isCustomerRemoveVisible;
            }
            set
            {
                _isCustomerRemoveVisible = value;
                SetPropertyChanged(nameof(IsCustomerRemoveVisible));
            }
        }

        //#94418
        private bool _isPONumberDisplay;
        public bool IsPONumberDisplay
        {
            get
            {
                return _isPONumberDisplay;
            }
            set
            {
                _isPONumberDisplay = value;
                SetPropertyChanged(nameof(IsPONumberDisplay));
            }
        }
         //#94418

        private bool _emailIsEnabled;
        public bool EmailIsEnabled
        {
            get
            {
                return _emailIsEnabled;
            }
            set
            {
                _emailIsEnabled = value;
                SetPropertyChanged(nameof(EmailIsEnabled));
            }
        }

        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Rupesh
        string _SaleHistoryTipText { get; set; }
        public string SaleHistoryTipText { get { return _SaleHistoryTipText; } set { _SaleHistoryTipText = value; SetPropertyChanged(nameof(SaleHistoryTipText)); } }
        //End Ticket #73190 By: Rupesh

        //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
        private string _eligibilityProofPath;
        public string EligibilityProofPath
        {
            get
            {
                return _eligibilityProofPath;
            }
            set
            {
                _eligibilityProofPath = value;
                SetPropertyChanged(nameof(EligibilityProofPath));
            }
        }
        private string _eligibilityAge;
        public string EligibilityAge
        {
            get
            {
                return _eligibilityAge;
            }
            set
            {
                _eligibilityAge = value;
                SetPropertyChanged(nameof(EligibilityAge));
            }
        }
        private bool _isEligible;
        public bool IsEligible
        {
            get
            {
                return _isEligible;
            }
            set
            {
                _isEligible = value;
                SetPropertyChanged(nameof(IsEligible));
            }
        }
        //Ticket:end:#90938,#94423 .by rupesh

        public EventHandler<InvoiceDto> InvoiceRefund;
        public EventHandler<InvoiceDto> InvoiceExchange;
        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location  by Pratik
        public EventHandler<InvoiceDto> InvoiceStatusChanged;
        //End #84293 by Pratik

        //Ticket start:#91260 iOS:FR:Duplicating invoices.by rupesh
        private bool _isShowInvoiceDuplicate = false;
        public bool IsShowInvoiceDuplicate { get { return _isShowInvoiceDuplicate; } set { _isShowInvoiceDuplicate = value; SetPropertyChanged(nameof(IsShowInvoiceDuplicate)); } }
        private bool _isEnableInvoiceDuplicate = false;
        public bool IsEnableInvoiceDuplicate { get { return _isEnableInvoiceDuplicate; } set { _isEnableInvoiceDuplicate = value; SetPropertyChanged(nameof(IsEnableInvoiceDuplicate)); } }
        //Ticket end:#91260 .by rupesh


        #endregion

        #region Command

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public ICommand CollapseFulfilmentCommand { get; }
        public ICommand PrintFulfilmentCommand { get; }
        //End #84293 by Pratik
        public ICommand CollapseOrderDetailCommand { get; }
        public ICommand CollapsePaymentBreakDownCommand { get; }
        public ICommand CollapseSaleHistoryCommand { get; }
        public ICommand CollapseRefundSummaryCommand { get; }
        public ICommand CollapseExchangeSummaryCommand { get; }

        public ICommand ClickedMenuCommand { get; }
        public ICommand RemovePaymentCommand { get; }
        public ICommand SendErrorsCommand { get; }

        public ICommand SaveNoteCommand { get; }

        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.
        public ICommand SerailNumberUpdateCommand { get; }
        public ICommand EditSerialNumberButtonCommand { get; }
        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.

        public ICommand TaxInvoiceReceiptClickedHandleCommand => new Command(taxInvoiceReceipt_Clicked);
        public ICommand GiftReceiptClickedHandleCommand => new Command(giftReceipt_Clicked);
        public ICommand EmailSelectedHandleCommand => new Command<SelectEmail>(email_Selected);
        public ICommand RemoveEmailClickedHandleCommand => new Command<string>(removeEmail_Clicked);
        public ICommand DuplicateQuoteClickedHandleCommand => new Command(duplicateQuote_Clicked);
        public ICommand ConvertToSaleQuoteClickedHandleCommand => new Command(convertToSaleQuote_Clicked);
        public ICommand RefundHandleClickedCommand => new Command(refundHandle_Clicked);
        public ICommand ExchangeHandleClickedCommand => new Command(exchangeHandle_Clicked);
        public ICommand EmailHandleClickedCommand => new Command(emailHandle_Clicked);
        public ICommand CloseEmailViewCommand => new Command(CloseEmailView_Clicked);
        public ICommand BackgroundHandleTappedCommand => new Command(backgroundHandle_Tapped);
        public ICommand EditNoteHandleClickedCommand => new Command(editNoteHandle_Clicked);
        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
        public ICommand OpenEligibilityProofCommand => new Command(OpenEligibilityProof);
        //Ticket:end:#90938 .by rupesh
        public ICommand DuplicateInvoiceCommand => new Command(DuplicateInvoice);

        #endregion

        #region Life-Cycle
        public ParkSaleDetailViewModel()
        {
            Title = "Invoice detail";
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            CollapseFulfilmentCommand = new Command(() => CollapseItemDetail("Fulfilment"));
            PrintFulfilmentCommand = new Command<InvoiceFulfillmentDto>(async (arg) => await InvoiceFulfillmentClick(arg));
            //End #84293 by Pratik
            CollapseOrderDetailCommand = new Command(() => CollapseItemDetail("OrderDetail"));
            CollapsePaymentBreakDownCommand = new Command(() => CollapseItemDetail("PaymentSummary"));
            CollapseSaleHistoryCommand = new Command(() => CollapseItemDetail("SaleHistory"));
            CollapseRefundSummaryCommand = new Command(() => CollapseItemDetail("RefundSummary"));
            CollapseExchangeSummaryCommand = new Command(() => CollapseItemDetail("ExchangeSummary"));
            SaveNoteCommand = new Command<string>(async (str) => await SaveNote(str));

            ClickedMenuCommand = new Command<string>(async (str) => await HeaderMenu(str));
            saleService = new SaleServices(saleApiService);
            outletServices = new OutletServices(outletApiService);
            customerService = new CustomerServices(customerApiService);
            paymentService = new PaymentServices(paymentApiService);
            userService = new UserServices(userApiService);
            //clearant paymentservice

            productService = new ProductServices(ProductApiService);
            RemovePaymentCommand = new Command<InvoicePaymentDto>(async (obj) => await removePayment(obj));

            SendErrorsCommand = new Command(SendErrors);

            //start #84287 IOS- Feature:- Allow an option to add 'Sold by' user name on line items in the cart By Pratik
            IsServedBy = Settings.StoreGeneralRule.ServedByLineItem;
            //end #84287 .by Pratik

            SerailNumberUpdateCommand = new Command<InvoiceLineItemDto>(async (obj) => await AddUpdateSerialNumber(obj));

            EditSerialNumberButtonCommand = new Command<InvoiceLineItemDto>((obj) => OpenEditSerialNumberText(obj));

            ExchangeButtonTextColor = AppColors.NavigationBarBackgroundColor;
            if (CustomerModel == null)
            {
                CustomerModel = new CustomerViewModel(customerService, saleService, true);
                CustomerModel.CustomerChanged += async (object sender, CustomerDto_POS customer) =>
                {
                    using (new Busy(this, true))
                    {
                        try
                        {
                            bool isDeliveryAddressAlertDisplayed = false;

                            if (Invoice == null || customer == null)
                            {
                                return;
                            }
                            if (customer.Id == 0 && string.IsNullOrEmpty(customer.TempId))
                            {
                                Invoice.CustomerGroupId = null;
                                Invoice.CustomerGroupName = "";
                                Invoice.CustomerGroupDiscount = null;
                                Invoice.CustomerGroupDiscountNote = "";

                                Invoice.CustomerId = 0;
                                Invoice.CustomerTempId = null;
                                Invoice.CustomerName = "";
                                Invoice.CustomerDetail = customer;

                                Invoice.CustomerGroupDiscountNoteInside = "";
                                Invoice.CustomerGroupDiscountNoteInsidePrice = 0;

                                Invoice.CustomerGroupDiscountType = false;
                                Invoice.PriceListCustomerCurrentLoyaltyPoints = 0;
                            }
                            else
                            {
                                Invoice.CustomerId = customer.Id;
                                Invoice.CustomerTempId = customer.TempId;
                                Invoice.CustomerName = (string.IsNullOrEmpty(customer.FirstName) ? "" : (customer.FirstName.ToUppercaseFirstCharacter() + " ")) + (string.IsNullOrEmpty(customer.LastName) ? "" : customer.LastName.ToUppercaseFirstCharacter());
                                Invoice.CustomerDetail = customer;
                                Invoice.CustomerPhone = customer.Phone;
                                if (Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer && !Settings.IsQuoteSale)
                                {
                                    var result = await App.Alert.ShowAlert(LanguageExtension.Localize("DeliveryAddressAlertTitle"), LanguageExtension.Localize("DeliveryAddressAlertMessage"), LanguageExtension.Localize("YesButtonText"), LanguageExtension.Localize("CancelButtonText"));
                                    if (result)
                                    {
                                        CustomerModel.DeliveryCustomerCommand.Execute(Invoice.CustomerDetail);
                                        isDeliveryAddressAlertDisplayed = true;

                                    }
                                }

                            }
                            if (isDeliveryAddressAlertDisplayed)
                            {
                                CustomerModel.DeliveryAddressChanged -= DeliveryAddressChanged;
                                CustomerModel.DeliveryAddressChanged += DeliveryAddressChanged;
                                CustomerModel.DeliveryAddressSelectionClosed -= DeliveryAddressSelectionClosed;
                                CustomerModel.DeliveryAddressSelectionClosed += DeliveryAddressSelectionClosed;

                            }
                            else
                            {
                                Invoice = await InvoiceCalculations.UpdateCustomerParkedSaleOrder(Invoice, saleService, outletServices, productService);
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }
                };
            }
            CountrySpecificCode();
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(() =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        BtnEmailText = "Email";
                        IsFrmEmailVisible = false;
                        IsSelectEmailListVisible = false;
                        IsSelectedEmailListVisible = false;
                        IsEmailWithPayLinkVisible = false;
                        ShopGeneralRule = Settings.StoreGeneralRule;
                        GeneralShopDto = Settings.StoreShopDto;
                        Subscription = Settings.Subscription;
                        CurrentUser = Settings.CurrentUser;


                        CustomerModel.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == "IsOpenSearchCustomerPopUp")
                            {
                                if (CustomerModel.IsOpenSearchCustomerPopUp == 1)
                                {
                                    parkSaleDetailPage?._CustomerPopup.FocusOnEntry();
                                }
                                else
                                {
                                    parkSaleDetailPage?._CustomerPopup.UnFocusOnEntry();
                                }

                            }
                        };
                        EmailIsEnabled = true;

                        //START ticket #76208 IOS:FR:Terms of payments by Pratik
                        olddate = Invoice.InvoiceDueDate;
                        newDate = Invoice.InvoiceDueDate;
                        InvoiceDueDate = Invoice?.InvoiceDueDate == null ? DateTime.MinValue : Invoice.InvoiceDueDate.Value.ToStoreTime();
                        //End ticket #76208 by Pratik
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("error in appearing" + ex.Message.ToString());
                    }
                });
            });
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            Task.Run(() =>
            {
                CollapseItemDetail("Hide");
                if (CustomerModel != null)
                {
                    CustomerModel.IsOpenSearchCustomerPopUp = 0;
                    parkSaleDetailPage?._CustomerPopup.UnFocusOnEntry();
                    EmailIsEnabled = false;
                }
            });
        }

        #endregion

        #region Methods and Command Execution

        public void SetInvoiceDetails()
        {
            //Ticket:start:,#94423 IOS:FR Age varification.by rupesh
            var ageVerificationData = Invoice?.InvoiceLineItems.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails);
            if (ageVerificationData != null && ageVerificationData.Any())
            {
                IsEligible = true;
                EligibilityProofPath = ageVerificationData.Where(x => x.Key == InvoiceItemAgeVerification.ImageUpload).FirstOrDefault()?.Value;
                var dateOfBirth = ageVerificationData.Where(x => x.Key == InvoiceItemAgeVerification.BirthDate).FirstOrDefault()?.Value;
                if (dateOfBirth != null)
                    EligibilityAge = ageVerificationData.Where(x => x.Key == InvoiceItemAgeVerification.AgeLimit).FirstOrDefault()?.Value;
            }
            else
                IsEligible = false;

            //Ticket:end:#90938,#94423 .by rupesh
            if (Settings.GrantedPermissionNames != null)
            {
                RemovePaymentPermission = Settings.GrantedPermissionNames.Count(s => s == "Pages.Tenant.POS.EnterSale.ToRemovePayment") > 0;
                IsHavingRefund = (Settings.GrantedPermissionNames.Count(s => s == "Pages.Tenant.POS.EnterSale.RefundSale") > 0) && Invoice != null && (Invoice.InvoiceFloorTable == null || Settings.StoreGeneralRule.EnableOnGoingSaleRefundAndExchange) && (Invoice.Status == InvoiceStatus.Completed || (Invoice.Status == InvoiceStatus.Exchange && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.ExchangeInvoice)); // #94565;
                IsHavingExchange = (Settings.GrantedPermissionNames.Count(s => s == "Pages.Tenant.POS.EnterSale.ExchangeSale") > 0) && Invoice != null && (Invoice.InvoiceFloorTable == null || Settings.StoreGeneralRule.EnableOnGoingSaleRefundAndExchange) && (Invoice.Status == InvoiceStatus.Completed || (Invoice.Status == InvoiceStatus.Exchange && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.ExchangeInvoice)); // #94565;
                //Ticket start:#77144. by rupesh
                IsInvoiceRefundAndDiscard = (Settings.GrantedPermissionNames.Count(s => s == "Pages.Tenant.POS.EnterSale.RefundSale") > 0) && Invoice != null && (Invoice.Status == InvoiceStatus.BackOrder || Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy || Invoice.Status == InvoiceStatus.OnAccount) && Invoice.TotalPaid > 0;
                //Ticket end:#77144. by rupesh
                CustomerNameIsEnabled = Settings.GrantedPermissionNames.Count(s => s == "Pages.Tenant.Customer.AddUpdateForExistingOrder") > 0;
            }
            if (RemovePaymentPermission && (Settings.CurrentRegister == null ||
                Settings.CurrentRegister.Registerclosure == null ||
                Settings.CurrentRegister.Registerclosure.StartDateTime == null ||
                Invoice == null ||
                Invoice.RegisterClosureId != Settings.CurrentRegister.Registerclosure.Id ||
                Invoice.Status == InvoiceStatus.Refunded))
            {
                RemovePaymentPermission = false;
            }

            //Added by rupesh to avoid removing payment and refund/void for other selected outlet
            if (Invoice != null && Invoice.OutletId != Settings.SelectedOutletId)
            {
                RemovePaymentPermission = false;
                IsInvoiceRefundAndDiscard = false;
            }

            //Start #76072 iPAD: Quote sale should not allow t remove customer from sales history; Only allow to change from existing one.by Pratik
            IsCustomerRemoveVisible = false;
            if (Invoice != null)
            {
                if (Invoice.InvoiceLineItems == null)
                    Invoice.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();

                if (RemovePaymentPermission && Invoice.InvoiceLineItems.Any(x => x.InvoiceItemType == InvoiceItemType.GiftCard) || Invoice.InvoiceLineItems.Any(x => x.RefundedQuantity > 0) || Invoice.Status == InvoiceStatus.Voided || Invoice.Status == InvoiceStatus.Exchange)
                {
                    RemovePaymentPermission = false;
                }

                if (RemovePaymentPermission && Invoice.InvoicePayments.Any(x => x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay))
                {
                    RemovePaymentPermission = false;
                }

                //#95241
                if (Invoice.approvedByUser.HasValue && Invoice.approvedByUser > 0 && string.IsNullOrEmpty(Invoice.approvedByUserName))
                {
                    Invoice.approvedByUserName = userService.GetUserListDtoById(Invoice.approvedByUser.Value)?.FullName;
                }
                if(Invoice.InvoicePayments != null && Invoice.InvoicePayments.Count > 0)
                    DisplayRegisterName = Invoice.InvoicePayments.Count(a => a.RegisterName == Invoice.InvoicePayments.FirstOrDefault().RegisterName) != Invoice.InvoicePayments.Count;
                if (Invoice.InvoiceLineItems.Any(a => a.approvedByUser.HasValue && a.approvedByUser > 0))
                {
                    for (int i = 0; i < Invoice.InvoiceLineItems.Count; i++)
                    {
                        if (Invoice.InvoiceLineItems[i].approvedByUser.HasValue && Invoice.InvoiceLineItems[i].approvedByUser > 0 && string.IsNullOrEmpty(Invoice.InvoiceLineItems[i].approvedByUserName))
                        {
                            Invoice.InvoiceLineItems[i].approvedByUserName = userService.GetUserListDtoById(Invoice.InvoiceLineItems[i].approvedByUser.Value)?.FullName;
                        }
                    }
                }
                //#95241
                
                //#94418
                if (!string.IsNullOrEmpty(Settings.StoreGeneralRule?.ConnectedAccountingAddOn) && !string.IsNullOrEmpty(Invoice.InvoiceDetail?.Value))
                {
                    IsPONumberDisplay = true;
                }
                else
                {
                    IsPONumberDisplay = false;
                }
                //#94418

                //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                if (Settings.StoreGeneralRule.AllowPartialFulfilled)
                {
                    IsFulfilmentDisplay = false;
                    if (Invoice.InvoiceFulfillments != null && Invoice.InvoiceFulfillments.Count > 0)
                    {
                        Invoice.InvoiceFulfillments = new ObservableCollection<InvoiceFulfillmentDto>(Invoice.InvoiceFulfillments.OrderBy(a => a.CreationTime));
                        foreach (var item in Invoice.InvoiceFulfillments)
                        {
                            if (item.InvoiceItemFulfillments != null && item.InvoiceItemFulfillments.Count > 0)
                            {
                                if (!IsFulfilmentDisplay)
                                    IsFulfilmentDisplay = true;
                                item.InvoiceLineItems = new ObservableCollection<InvoiceItemFulfillmentDisplay>(Invoice.InvoiceLineItems.Where(a => item.InvoiceItemFulfillments.Select(a => a.InvoiceLineItemId).ToList().Contains(a.Id)).Select(x => new InvoiceItemFulfillmentDisplay
                                {
                                    Ordered = x.Quantity.ToString("0.##"),
                                    FulfillmentQuantity = item.InvoiceItemFulfillments.First(a => a.InvoiceLineItemId == x.Id).FulfillmentQuantity.ToString("0.##"),
                                    //Ticket start:#90943 iOS:FR Display Barcode or SKU instead of Product Name .by Pratik
                                    ItemName = Settings.CurrentRegister.ReceiptTemplate.ReplaceProductNameWithSKU ? x.SKUWithLabel : (Settings.CurrentRegister.ReceiptTemplate.PrintSKU ? x.ProductTitleWithSku : x.Title),
                                    //Ticket end:#90943 by Pratik 
                                }));
                            }
                        }
                    }
                }
                else
                    IsFulfilmentDisplay = false;
                //End #84293 by Pratik


                if (Invoice.Status == InvoiceStatus.Quote)
                    IsCustomerRemoveVisible = true;
            }
            //End #76072 by Pratik



            if (Invoice != null && (Invoice.Status == InvoiceStatus.Voided ||
                Invoice.Status == InvoiceStatus.LayBy ||
                Invoice.Status == InvoiceStatus.BackOrder ||
                Invoice.Status == InvoiceStatus.Quote ||     //Start #76072 addd Condition .by Pratik
                Invoice.Status == InvoiceStatus.OnAccount))
            {
                IsCustomerRemoveVisible = false;
            }
            else
            {
                IsCustomerRemoveVisible = Settings.GrantedPermissionNames.Count(s => s == "Pages.Tenant.Customer.RemoveForExistingOrder") > 0;
            }

            //Start ticket #76209 iOS:FR: User Permission : Add a permission for allowing 'Issue a Quote' By Pratik
            IsQuoteDuplicate = Invoice != null && Invoice.Status == InvoiceStatus.Quote && Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.Quote");
            IsQuoteConvertToSale = Invoice != null && Invoice.Status == InvoiceStatus.Quote && Invoice.FinancialStatus != FinancialStatus.Closed;
            //End ticket #76209 By Pratik

            //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Rupesh
            if (Invoice != null && (Invoice.TipTaxId == null || Invoice.TipTaxId == 0))
            {
                SaleHistoryTipText = "Surcharge (Inc. Tax)";
            }
            else
            {
                SaleHistoryTipText = "Surcharge (Ex. Tax)";

            }
            //End Ticket #73190 By: Rupesh

            //Ticket start:#91260 iOS:FR:Duplicating invoices.by rupesh
            bool result = false;
            var myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikePOSDuplicateSaleFeature");
            bool tempResult = Boolean.TryParse(Convert.ToString(myPropInfo.GetValue(Settings.ShopFeatures)), out result);
            IsEnableInvoiceDuplicate = Invoice != null && Invoice.OutletId == Settings.SelectedOutletId && result;
            IsShowInvoiceDuplicate = Invoice != null && Invoice.InvoiceFrom != InvoiceFrom.Amazon && Invoice.InvoiceFrom != InvoiceFrom.Import && Invoice.InvoiceFrom != InvoiceFrom.StoreCredit && Invoice.Status != InvoiceStatus.Quote && Invoice.Status != InvoiceStatus.Refunded && Invoice.Status != InvoiceStatus.Exchange && Invoice.FinancialStatus != FinancialStatus.Closed && !Invoice.CanBeModified; // #94565;
            //Ticket end:#91260 .by rupesh

            if (Invoice?.InvoiceHistories != null && Invoice.InvoiceHistories.Count > 0)
            {
                Invoice.InvoiceHistories = new ObservableCollection<InvoiceHistoryDto>(Invoice.InvoiceHistories.OrderBy(a => a.CreationStoreTime));
            }
        }

        async void DeliveryAddressChanged(object s, CustomerAddressDto e)
        {
            DeliveryAddressOnSelect(e);
            Invoice = await InvoiceCalculations.UpdateCustomerParkedSaleOrder(Invoice, saleService, outletServices, productService);

        }

        async void DeliveryAddressSelectionClosed(object s, CustomerAddressDto e)
        {
            Invoice = await InvoiceCalculations.UpdateCustomerParkedSaleOrder(Invoice, saleService, outletServices, productService);
        }

        public void DeliveryAddressOnSelect(CustomerAddressDto deliveryAddress)
        {
            try
            {
                if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RegisterOpenAlertMessage"));
                    return;
                }
                Invoice.DeliveryAddressId = deliveryAddress.Id;
                Invoice.DeliveryAddress = deliveryAddress;
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        void CountrySpecificCode()
        {
            var CurrentCultureName = DependencyService.Get<IMultilingual>().CurrentCultureInfo.Name;
            IsArabicCulture = (CurrentCultureName == "ar-SY");
        }

        public async void UpdateRefundHistoryList()
        {
            string OrginalInvoiceNumber = "";
            if (Invoice == null)
            {
                RefundHistory = null;
                ExchangeHistory = null;
                return;
            }

            if (Invoice != null && string.IsNullOrEmpty(Invoice.ReferenceNote))
            {
                RefundHistory = null;
            }
            else
            {
                try
                {
                    RefundHistory = JsonConvert.DeserializeObject<ObservableCollection<RefundSummary>>(Invoice.ReferenceNote);
                }
                catch (Exception ex)
                {
                    RefundHistory = null;
                    Debug.WriteLine(ex.Message);
                }
            }
            if (Invoice != null && string.IsNullOrEmpty(Invoice.ExchangeReferenceNote))
            {
                ExchangeHistory = null;
            }
            else
            {
                try
                {
                    ExchangeHistory = JsonConvert.DeserializeObject<ObservableCollection<RefundSummary>>(Invoice.ExchangeReferenceNote);
                    OrginalInvoiceNumber = ExchangeHistory.FirstOrDefault(x => x.Title.Contains("Original sale"))?.Title.Replace("Original sale #", "");
                    if (!ExchangeHistory.Any(x => x.InvoiceId > 0))
                        ExchangeHistory = null;
                }
                catch (Exception ex)
                {
                    ExchangeHistory = null;
                    Debug.WriteLine(ex.Message);
                }
            }

            if (Invoice.Status == InvoiceStatus.Quote && Invoice.FinancialStatus == FinancialStatus.Closed && !string.IsNullOrEmpty(Invoice.ReferenceNote))
            {
                try
                {
                    InvoiceHistoryDto invoiceHistoryObj = new InvoiceHistoryDto();
                    invoiceHistoryObj.StatusName = Invoice.ReferenceNote;

                    InvoiceHistories = Invoice.InvoiceHistories;
                    if (InvoiceHistories != null && !InvoiceHistories.Any(a => a.StatusName == Invoice.ReferenceNote))
                        InvoiceHistories.Add(invoiceHistoryObj);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }

            else if (Invoice.ReferenceInvoiceId != null && Invoice.InvoiceHistories != null)
            {
                try
                {
                    var Originalinvoice = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                    InvoiceHistoryDto invoiceHistoryObj = new InvoiceHistoryDto();

                    if (Originalinvoice.Status == InvoiceStatus.Quote)
                        invoiceHistoryObj.StatusName = "Original quotation #" + Originalinvoice.Number;
                    else
                        invoiceHistoryObj.StatusName = "Original sale " + Originalinvoice.Number;
                    invoiceHistoryObj.Status = Originalinvoice.Status;
                    invoiceHistoryObj.InvoiceFrom = Originalinvoice.InvoiceFrom;
                    InvoiceHistories = Invoice.InvoiceHistories;
                    if (InvoiceHistories != null && !InvoiceHistories.Any(a => a.StatusName == invoiceHistoryObj.StatusName))
                        InvoiceHistories.Insert(0, invoiceHistoryObj);
                }
                catch (Exception ex)
                {
                    List<InvoiceStatus> status = new List<InvoiceStatus>() { InvoiceStatus.Pending, InvoiceStatus.Completed, InvoiceStatus.Parked, InvoiceStatus.OnAccount, InvoiceStatus.Refunded, InvoiceStatus.BackOrder, InvoiceStatus.Voided, InvoiceStatus.LayBy, InvoiceStatus.Exchange, InvoiceStatus.Quote };
                    ObservableCollection<InvoiceDto> Originalinvoice = await saleService.GetRemoteInvoices(Fusillade.Priority.UserInitiated, true, Settings.SelectedOutletId, null, null, status, OrginalInvoiceNumber);
                    if (Originalinvoice != null)
                    {
                        InvoiceHistoryDto invoiceHistoryObj = new InvoiceHistoryDto();
                        invoiceHistoryObj.StatusName = "Original sale " + Originalinvoice.Select(x => x.Number).FirstOrDefault();
                        invoiceHistoryObj.Status = Originalinvoice.Select(x => x.Status).FirstOrDefault();
                        invoiceHistoryObj.InvoiceFrom = Originalinvoice.Select(x => x.InvoiceFrom).FirstOrDefault();
                        InvoiceHistories = Invoice.InvoiceHistories;
                        if (InvoiceHistories != null && !InvoiceHistories.Any(a => a.StatusName == invoiceHistoryObj.StatusName))
                            InvoiceHistories.Insert(0, invoiceHistoryObj);
                    }
                    Debug.WriteLine(ex.Message);
                }
            }
            return;
        }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        private async Task InvoiceFulfillmentClick(InvoiceFulfillmentDto arg)
        {
        }
        //End #84293 by Pratik

        public void CollapseItemDetail(string tab)
        {
            try
            {
                switch (tab)
                {
                    case "OrderDetail":
                        IsOrderDetailVisible = !IsOrderDetailVisible;
                        IsPaymentSummaryVisible = false;
                        IsSaleHistoryVisible = false;
                        IsRefundSummaryVisible = false;
                        IsExchangeSummaryVisible = false;
                        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                        IsFulfilmentVisible = false;
                        //End #84293 by Pratik
                        break;
                    case "PaymentSummary":
                        IsOrderDetailVisible = false;
                        IsPaymentSummaryVisible = !IsPaymentSummaryVisible;
                        IsSaleHistoryVisible = false;
                        IsRefundSummaryVisible = false;
                        IsExchangeSummaryVisible = false;
                        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                        IsFulfilmentVisible = false;
                        //End #84293 by Pratik
                        break;
                    case "SaleHistory":
                        //await LoadInvoiceSaleHistories();
                        IsOrderDetailVisible = false;
                        IsPaymentSummaryVisible = false;
                        IsSaleHistoryVisible = !IsSaleHistoryVisible;
                        IsRefundSummaryVisible = false;
                        IsExchangeSummaryVisible = false;
                        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                        IsFulfilmentVisible = false;
                        //End #84293 by Pratik
                        break;
                    case "RefundSummary":
                        //updateRefundHistoryList();
                        //await LoadInvoiceSaleHistories();
                        IsOrderDetailVisible = false;
                        IsPaymentSummaryVisible = false;
                        IsSaleHistoryVisible = false;
                        IsRefundSummaryVisible = !IsRefundSummaryVisible;
                        IsExchangeSummaryVisible = false;
                        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                        IsFulfilmentVisible = false;
                        //End #84293 by Pratik
                        break;
                    case "ExchangeSummary":
                        // updateRefundHistoryList();
                        // await LoadInvoiceSaleHistories();
                        IsOrderDetailVisible = false;
                        IsPaymentSummaryVisible = false;
                        IsSaleHistoryVisible = false;
                        IsRefundSummaryVisible = false;
                        IsExchangeSummaryVisible = !IsExchangeSummaryVisible;
                        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                        IsFulfilmentVisible = false;
                        //End #84293 by Pratik
                        break;
                    //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                    case "Fulfilment":
                        if (!IsFulfilmentVisible)
                        {
                            // updateRefundHistoryList();
                            // await LoadInvoiceSaleHistories();
                            IsOrderDetailVisible = false;
                            IsPaymentSummaryVisible = false;
                            IsSaleHistoryVisible = false;
                            IsRefundSummaryVisible = false;
                            IsExchangeSummaryVisible = false;
                            IsFulfilmentVisible = true;
                        }
                        break;
                    //End #84293 by 
                    default:
                        IsOrderDetailVisible = false;
                        IsPaymentSummaryVisible = false;
                        IsSaleHistoryVisible = false;
                        IsRefundSummaryVisible = false;
                        IsExchangeSummaryVisible = false;
                        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                        IsFulfilmentVisible = false;
                        //End #84293 by Pratik
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void GetTaxDetails()
        {
            try
            {
                using (new Busy(this, true))
                {
                    var texLabel = "Tax";
                    if (Settings.CurrentRegister != null && Settings.CurrentRegister.ReceiptTemplate != null)
                        texLabel = Settings.CurrentRegister.ReceiptTemplate.TaxLable;

                    Invoice.Taxgroup = new ObservableCollection<LineItemTaxDto>();
                    decimal discountPercentValue = 0;
                    if (!Invoice.DiscountIsAsPercentage)
                    {
                        var total = Invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
                        if (total != 0)
                            discountPercentValue = (Invoice.DiscountValue * 100) / total;
                    }
                    else
                    {
                        discountPercentValue = Invoice.DiscountValue;
                    }

                    foreach (var InvoiceLineItems in Invoice.InvoiceLineItems)
                    {

                        var item = InvoiceLineItems.Copy();
                        if ((item.TaxId > 1 || (item.TaxId == 1 && item.TaxRate > 0)) && item.InvoiceItemType != InvoiceItemType.Discount)
                        {
                            var taxamount = item.TaxAmount;
                            var discountOfferTax = item.DiscountOfferTax.HasValue ? item.DiscountOfferTax.Value : 0;

                            if (Invoice.Status == InvoiceStatus.Refunded)
                            {
                                taxamount += Math.Abs(discountOfferTax);
                            }
                            else if (Invoice.Status == InvoiceStatus.Exchange)
                            {
                                if (item.Quantity < 0)
                                {
                                    taxamount += Math.Abs(discountOfferTax);
                                }
                                else
                                {
                                    taxamount += discountOfferTax;
                                }
                            }
                            else
                            {
                                taxamount += discountOfferTax;
                            }

                            if (Invoice.ApplyTaxAfterDiscount)
                            {
                                if (Invoice.Status == InvoiceStatus.Exchange)
                                {
                                    //Start Ticket #73665 iOS: Group Taxes not matched in iOS for Prints from POS Screen and Same print from Sales history by pratik
                                    //if (item.IsExchangedProduct)
                                    if (item.Quantity < 0)
                                        taxamount = (taxamount - InvoiceCalculations.GetValuefromPercent(taxamount, discountPercentValue));
                                    //end Ticket #73665 by pratik

                                }
                                else
                                    taxamount = (taxamount - InvoiceCalculations.GetValuefromPercent(taxamount, discountPercentValue));
                            }

                            var hasTax = Invoice.Taxgroup.FirstOrDefault(x => x.TaxId == item.TaxId);
                            if (hasTax != null)
                            {
                                hasTax.TaxRate += item.TaxRate;
                                hasTax.TaxAmount += taxamount;

                                if (item.LineItemTaxes != null && item.LineItemTaxes.Count > 0 && (hasTax.SubTaxes == null || hasTax.SubTaxes?.Count == 0))
                                {
                                    hasTax.SubTaxes = item.LineItemTaxes;
                                }

                                foreach (var subvalue in item.LineItemTaxes)
                                {
                                    var hasSubTaxTax = hasTax.SubTaxes.FirstOrDefault(x => x.TaxId == subvalue.TaxId);
                                    if (hasSubTaxTax != null)
                                    {
                                        hasSubTaxTax.TaxAmount = hasSubTaxTax.TaxAmount + subvalue.TaxAmount;
                                    }
                                }
                            }
                            else
                            {
                                var tax = new LineItemTaxDto()
                                {
                                    TaxId = item.TaxId,
                                    TaxName = item.TaxName,
                                    TaxRate = item.TaxRate,
                                    TaxAmount = taxamount,
                                    SubTaxes = item.LineItemTaxes,
                                };

                                Invoice.Taxgroup.Add(tax);
                            }
                        }
                    }

                    var taxes = new ObservableCollection<LineItemTaxDto>();
                    // Ticket Start #63111 iOS: Separate line items for Tax Group format need to match with web By: Pratik
                    foreach (var item in Invoice.Taxgroup.OrderBy(a => a.IsGroupTax))
                    // Ticket end #63111 By: Pratik
                    {
                        if (item.SubTaxes.Count > 0)
                        {
                            item.TaxName = texLabel + "(" + item.TaxName + ")";
                            taxes.Add(item);
                            foreach (var item2 in item.SubTaxes)
                            {
                                taxes.Add(item2);
                            }
                        }
                        else
                        {
                            item.TaxName = texLabel + "(" + item.TaxName + ")";
                            taxes.Add(item);
                        }
                    }
                    Invoice.ReceiptTaxList = taxes;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void LoadOutletsDetails()
        {
            try
            {
                using (new Busy(this, true))
                {
                    CurrentOutlet = outletServices.GetLocalOutletById(Settings.SelectedOutletId);
                    CurrentRegister = Settings.CurrentRegister;

                    if (CurrentOutlet == null)
                    {
                        CurrentOutlet = new OutletDto_POS();
                    }

                    LoadCustomer();
                    LoadDeliveryAddress();

                    decimal discountPercentValue = 0;
                    if (!Invoice.DiscountIsAsPercentage)
                    {
                        var total = Invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
                        if (total != 0)
                            discountPercentValue = (Invoice.DiscountValue * 100) / total;
                    }
                    else
                    {
                        discountPercentValue = Invoice.DiscountValue;
                    }

                    //Start Ticket #73665 iOS: Group Taxes not matched in iOS for Prints from POS Screen and Same print from Sales history by pratik
                    //Invoice.Taxgroup = InvoiceCalculations.GetTaxgroup(Invoice, discountPercentValue);
                    //Invoice.Taxgroup = InvoiceCalculations.GetTaxgroupForprint(Invoice, discountPercentValue);
                    //End Ticket #73665 by pratik
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void LoadCustomer()
        {
            try
            {
                var tempSelectedEmails = new ObservableCollection<string>();
                CustomerEmails.Clear();
                if (Invoice != null && Invoice.CustomerId != null && Invoice.CustomerId.Value != 0)
                {
                    var tmpcust = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                    if (tmpcust != null)
                    {
                        Invoice.CustomerDetail = tmpcust;
                        CustomerEMailId = "";
                        if (!string.IsNullOrEmpty(tmpcust.Email))
                        {
                            CustomerEmails.Add(new SelectEmail { Email = tmpcust.Email, IsSelected = true });
                            tempSelectedEmails.Add(tmpcust.Email);

                        }
                        if (!string.IsNullOrEmpty(tmpcust.SecondaryEmail1))
                            CustomerEmails.Add(new SelectEmail { Email = tmpcust.SecondaryEmail1 });
                        if (!string.IsNullOrEmpty(tmpcust.SecondaryEmail2))
                            CustomerEmails.Add(new SelectEmail { Email = tmpcust.SecondaryEmail2 });
                    }
                    else
                    {
                        CustomerEMailId = "";
                    }
                }
                SelectedEmails = tempSelectedEmails;
                if (Invoice.CustomerDetail == null)
                {
                    Invoice.CustomerDetail = new CustomerDto_POS();
                    CustomerEMailId = "";
                }
                SelectedEmailListHeight = SelectedEmails.Count * 60;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void LoadDeliveryAddress()
        {
            try
            {
                if (Invoice != null && Invoice.DeliveryAddressId != null && Invoice.DeliveryAddressId.Value != 0)
                {
                    var tmpDeliveryAddress = customerService.GetLocalDeliveryAddressesById(Invoice.DeliveryAddressId.Value);
                    if (tmpDeliveryAddress != null)
                    {
                        Invoice.DeliveryAddress = tmpDeliveryAddress;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task HeaderMenu(string SelectedMenu)
        {
            if (EventCallRunning)
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? AppFeatures.AndroidMSecond : AppFeatures.IOSMSecond).Wait();
                EventCallRunning = false;
            });
            try
            {
                if (Invoice != null)
                {
                    switch (SelectedMenu)
                    {
                        case "Reopen":
                            var Permissions = Settings.GrantedPermissionNames;
                            if (Permissions == null && Permissions.Any(s => s == "Pages.Tenant.POS.EnterSale"))
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("EnterSaleNoPermissionMessage"));
                                return;
                            }
                            if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ReopenInvoiceAlertMessage"));
                                return;
                            }
                            if (Invoice.Status == InvoiceStatus.BackOrder)
                            {
                                foreach (var item in Invoice.InvoiceLineItems.Where(a => a.InvoiceItemType == InvoiceItemType.Standard || a.InvoiceItemType == InvoiceItemType.UnityOfMeasure))
                                {
                                    if (item.InvoiceItemType == InvoiceItemType.Standard)
                                    {
                                        ProductDto_POS Product = productService.GetLocalProduct(item.InvoiceItemValue);

                                        item.ItemCost = Product.ProductOutlet.CostPrice;
                                        if (Invoice.TaxInclusive)
                                            item.RetailPrice = Product.ProductOutlet.SellingPrice;
                                        else
                                            item.RetailPrice = Product.ProductOutlet.PriceExcludingTax;

                                        using (new Busy(this, true))
                                        {
                                            var hasInStock = await saleService.HasInStock(Fusillade.Priority.Background, Invoice.Id);
                                            if (!hasInStock)
                                            {
                                                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoItemInStockMessage"), Colors.Red, Colors.White);
                                                return;

                                            }
                                        }
                                    }
                                    else if (item.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
                                    {
                                        ProductDto_POS Product = productService.GetLocalProduct(item.InvoiceLineSubItems.FirstOrDefault().ItemId);
                                        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                                        // item.ItemCost = Product.ProductOutlet.CostPrice;
                                        // if (Invoice.TaxInclusive)
                                        //     item.RetailPrice = Product.ProductOutlet.SellingPrice;
                                        // else
                                        //     item.RetailPrice = Product.ProductOutlet.PriceExcludingTax;
                                        //Ticket end:#92764 .by rupesh

                                        var validatedstock = Product.ProductOutlet.OnHandstock - Product.ProductOutlet.Committedstock;

                                        if (Product.TrackInventory && !Product.AllowOutOfStock)
                                        {
                                            if (validatedstock < item.Quantity)
                                            {
                                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoItemInStockMessage"), Colors.Red, Colors.White);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                            //var mainpage = (MainPage)_navigationService.MainPage;
                            if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                                await NavigationService.PopModalAsync();
                            //mainpage.ChangePage("EnterSalePage");
                            Shell.Current.CurrentItem = Shell.Current.Items[0];
                            EnterSalePage.ServedBy = Settings.CurrentUser;
                            ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.CustomerModel.SelectedCustomer = Invoice.CustomerDetail;

                            if (Invoice.Status == InvoiceStatus.Quote)
                            {
                                try
                                {
                                    foreach (var item in Invoice.InvoiceLineItems)
                                    {
                                        if (item.InvoiceItemType != InvoiceItemType.UnityOfMeasure && item.Category != null && !string.IsNullOrEmpty(item.Category) && item.CategoryDtos == null)
                                        {
                                            var categories = JsonConvert.DeserializeObject<List<CategoryDto>>(item.Category);
                                            if (item.Category.Contains("categoryId"))
                                            {
                                                var productCategories = JsonConvert.DeserializeObject<List<ProductCategoryDto>>(item.Category);
                                                foreach (var category in categories)
                                                {
                                                    var productCategory = productCategories.ElementAt(categories.IndexOf(category));
                                                    category.Id = productCategory.CategoryId;
                                                    category.ParentId = productCategory.ParentCategoryId;
                                                }
                                            }
                                            item.CategoryDtos = JsonConvert.SerializeObject(categories);
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.Track();
                                }

                                ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel?.QuoteSelectedTapped(null);
                            }
                            ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.CustomerModel.SelectedCustomer = Invoice.CustomerDetail;
                            _ = Task.Run(() =>
                             {
                                 MainThread.BeginInvokeOnMainThread(() => { ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.ReopenSaleFromHistory(Invoice); });
                             });
                            break;
                        case "MarkAsComplete":
                            Invoice.Status = InvoiceStatus.Completed;
                            Invoice.isSync = false;
                            Invoice.IsCustomerChange = false;
                            WeakReferenceMessenger.Default.Send(new Messenger.SignalRInvoiceMessenger(Invoice));
                            ObservableCollection<InvoiceDto> Invoices = new ObservableCollection<InvoiceDto>() { Invoice };
                            await saleService.UpdateLocalInvoiceThenSendItToServer(Invoices);
                            //await saleService.UpdateLocalInvoiceThenSendItToServer(Invoice);
                            if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                                await NavigationService.PopModalAsync();
                            break;
                        case "Discard":
                            //Ticket start:#77144. by rupesh
                            var result = await App.Alert.ShowAlert("Discard this sale?", "Click 'Yes' to permanently delete this transaction.", "Yes", "Cancel");
                            if (result)
                            {
                                Invoice.Status = InvoiceStatus.Voided;
                                Invoice.isSync = false;
                                Invoice.IsCustomerChange = false;
                                WeakReferenceMessenger.Default.Send(new Messenger.SignalRInvoiceMessenger(Invoice));
                                ObservableCollection<InvoiceDto> tempInvoices = new ObservableCollection<InvoiceDto>() { Invoice };
                                await saleService.UpdateLocalInvoiceThenSendItToServer(tempInvoices);
                                //await saleService.UpdateLocalInvoiceThenSendItToServer(Invoice);
                                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                                    await NavigationService.PopModalAsync();
                            }
                            break;
                        //Ticket end:#77144. by rupesh
                        case "RefundAndDiscard":

                            if (refundAndVoidPage != null)
                                refundAndVoidPage = null;

                            if (refundAndVoidPage == null)
                            {
                                refundAndVoidPage = new RefundAndVoidPage();
                            }
                            refundAndVoidPage.ViewModel.Invoice = Invoice;
                            refundAndVoidPage.ViewModel.LoadPaymentOption();

                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await NavigationService.PushModalAsync(refundAndVoidPage);
                            });

                            break;
                        case "PickAndPack":

                            if (pickAndPackPage != null)
                                pickAndPackPage = null;

                            if (pickAndPackPage == null)
                            {
                                pickAndPackPage = new PickAndPackPage();
                            }

                            pickAndPackPage.ViewModel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Custom && x.InvoiceItemType != InvoiceItemType.Discount));
                            pickAndPackPage.ViewModel.Invoice = Invoice;
                            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                            pickAndPackPage.ViewModel.CopyInvoiceLineItems = pickAndPackPage.ViewModel.InvoiceLineItems.Copy();
                            //End #84293 by Pratik
                            pickAndPackPage.ViewModel.CurrentOutlet = CurrentOutlet;
                            pickAndPackPage.ViewModel.CurrentRegister = CurrentRegister;
                            pickAndPackPage.ViewModel.GeneralShopDto = GeneralShopDto;
                            pickAndPackPage.ViewModel.Subscription = Subscription;
                            pickAndPackPage.ViewModel.ClickedMenuCommand = ClickedMenuCommand;
                            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location  by Pratik
                            pickAndPackPage.ViewModel.InoviceChanged += ViewModel_InoviceChanged;
                            //End #84293 by Pratik
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await NavigationService.PushModalAsync(pickAndPackPage);
                            });

                            break;
                        //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time. by rupesh
                        case "Edit":
                            Permissions = Settings.GrantedPermissionNames;
                            if (Permissions == null && Permissions.Any(s => s == "Pages.Tenant.POS.EnterSale"))
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("EnterSaleNoPermissionMessage"));
                                return;
                            }
                            if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ReopenInvoiceAlertMessage"));
                                return;
                            }

                            //mainpage = (MainPage)_navigationService.MainPage;
                            if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                                await NavigationService.PopModalAsync();
                            //mainpage.ChangePage("EnterSalePage");
                            Shell.Current.CurrentItem = Shell.Current.Items[0];
                            EnterSalePage.ServedBy = Settings.CurrentUser;
                            //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time.by rupesh
                            Invoice.IsEditBackOrderFromSaleHistory = true;
                            //Ticket end:#84289 .by rupesh
                            //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                            var viewModel = ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel;
                            Settings.IsBackorderSaleSelected = viewModel.IsBackorderDisplay;
                            if (Settings.IsBackorderSaleSelected)
                                viewModel.BackorderSelected(null);
                            //Ticket end:#92764.by rupesh
                            viewModel.invoicemodel.CustomerModel.SelectedCustomer = Invoice.CustomerDetail;
                            _ = Task.Run(() =>
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    viewModel.ReopenSaleFromHistory(Invoice);
                                });
                            });

                            break;
                        //Ticket end:#84289 . by rupesh
                        case "Void":
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location  by Pratik
        private async void ViewModel_InoviceChanged(object sender, InvoiceDto e)
        {
            await PrepareNextPreviousSale(e);
            OnAppearing();
            SetInvoiceDetails();

            InvoiceStatusChanged?.Invoke(this, Invoice);
        }
        //End #84293 by Pratik

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public async Task UpdateInvoiceDueDate()
        {
            if (Invoice.InvoiceDueDate != null)
            {
                using (new Busy(this, true))
                {
                    Invoice.InvoiceDueDate = InvoiceDueDate.ToStoreUTCTime();
                    var hasInStock = await saleService.UpdateInvoiceDueDate(Fusillade.Priority.Background, true, Invoice, InvoiceDueDate.ToStoreUTCTime());
                }
            }
        }
        //END ticket #76208 by Pratik

        public async Task AddUpdateSerialNumber(InvoiceLineItemDto invoiceLineItemDto)
        {
            try
            {
                bool isValidSerialNo = CheckForSerialNo(invoiceLineItemDto);

                if (!isValidSerialNo)
                    return;

                if (!string.IsNullOrEmpty(invoiceLineItemDto.SerialNumber))
                {

                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    {
                        using (new Busy(this, true))
                        {
                            SerialNumberDto serialNumberDto = new SerialNumberDto()
                            {
                                Id = invoiceLineItemDto.Id,
                                InvoiceId = Invoice.Id,
                                SerialNumber = invoiceLineItemDto.SerialNumber,
                                EnableSerialNumber = true
                            };

                            var result = await saleService.AddUpdateSerialNumberFromSaleHistory(Fusillade.Priority.UserInitiated, true, serialNumberDto, Invoice);

                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Serial Number saved."), Colors.Green, Colors.White);
                        }
                        ;
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
                else
                {
                    var msg = string.Format(LanguageExtension.Localize("Please enter Serial Number for {0}"), invoiceLineItemDto.Title);
                    App.Instance.Hud.DisplayToast(msg, Colors.Red, Colors.White);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public bool CheckForSerialNo(InvoiceLineItemDto invoiceLineItemDto)
        {
            try
            {
                if (string.IsNullOrEmpty(invoiceLineItemDto.SerialNumber))
                {
                    var msg = string.Format(LanguageExtension.Localize("Please enter Serial Number for {0}"), invoiceLineItemDto.Title);
                    App.Instance.Hud.DisplayToast(msg, Colors.Red, Colors.White);
                    return false;
                }

                var invoiceLineItems = Invoice.InvoiceLineItems;

                var duplicateSerialNo = invoiceLineItems.Where(x => x.SerialNumber == invoiceLineItemDto.SerialNumber);

                if (duplicateSerialNo.Count() > 1)
                {
                    var msg = string.Format(LanguageExtension.Localize("Duplicate serial number: {0}. This serial number item is already added to the cart."), invoiceLineItemDto.SerialNumber);
                    App.Instance.Hud.DisplayToast(msg, Colors.Red, Colors.White);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in CheckForSerialNo : " + ex.Message);

                App.Instance.Hud.DisplayToast("Something went wrong while checking Serial Numbers.", Colors.Red, Colors.White);
                return false;
            }
            return true;
        }

        public void OpenEditSerialNumberText(InvoiceLineItemDto lineItemDto)
        {
            try
            {
                lineItemDto.IsSerialNumberEditableFromEntersale = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task SaveNote(string actionType)
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    using (new Busy(this, true))
                    {

                        if (Invoice?.InvoiceHistories != null && Invoice.InvoiceHistories.Any(a => a.InvoiceFrom == null))
                        {
                            var lst = Invoice.InvoiceHistories.ToList();
                            lst.RemoveAll(a => a.InvoiceFrom == null);
                            if (lst != null && lst.Count > 0)
                                Invoice.InvoiceHistories = new ObservableCollection<InvoiceHistoryDto>(lst);
                            else
                                Invoice.InvoiceHistories = new ObservableCollection<InvoiceHistoryDto>();
                        }
                        var copyinvoice = Invoice.Copy();
                        Invoice = await saleService.UpdateInvoiceNote(Fusillade.Priority.UserInitiated, true, Invoice);
                        Invoice = copyinvoice;
                        IsEditVisible = false;
                    }
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        async Task removePayment(InvoicePaymentDto payment)
        {
            if (EventCallRunning)
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? AppFeatures.AndroidMSecond : AppFeatures.IOSMSecond).Wait();
                EventCallRunning = false;
            });
            try
            {
                //Ticket start:#72472 Couldn't remove store credit payment from sales.by rupesh
                if (Settings.CurrentRegister == null || Settings.CurrentRegister.Registerclosure == null || Settings.CurrentRegister.Registerclosure.StartDateTime == null)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RemovePaymentInCloseRegisterAlertMessage"), Colors.Red, Colors.White);
                    return;
                }
                if (!App.Instance.IsInternetConnected && (payment.PaymentOptionType == PaymentOptionType.Loyalty || payment.PaymentOptionType == PaymentOptionType.Credit))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return;

                }

                var decline = await App.Alert.ShowAlert(LanguageExtension.Localize("RemovePaymentAleartMessage"), null, "Yes", "No");
                if (!decline)
                    return;

                using (new Busy(this, true))
                {
                    //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                    if (payment.InvoicePaymentDetails != null && payment.InvoicePaymentDetails.Any((a => a.Key == InvoicePaymentKey.SurchargeAmount)))
                    {
                        var totalsurcharge = payment.InvoicePaymentDetails.Where(a => a.Key == InvoicePaymentKey.SurchargeAmount).Sum(a => Convert.ToDecimal(a.Value ?? "0").ToPositive());
                        Invoice.TotalPaymentSurcharge -= totalsurcharge;
                        Invoice.SurchargeDisplayInSaleHistory -= totalsurcharge;
                        Invoice.NetAmount -= totalsurcharge;
                    }
                    //end Ticket #73190 By: Pratik

                    InvoiceDto tempInvoiceDto = Invoice;
                    tempInvoiceDto.InvoicePayments.Remove(payment);
                    tempInvoiceDto.TotalPaid = tempInvoiceDto.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);
                    tempInvoiceDto.TotalTender = tempInvoiceDto.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);
                    tempInvoiceDto.ChangeAmount = 0;
                    if (tempInvoiceDto.Status == InvoiceStatus.Completed)
                    {
                        if (tempInvoiceDto.NetAmount > tempInvoiceDto.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount))
                        {
                            tempInvoiceDto.Status = InvoiceStatus.Parked;
                            IsHavingRefund = false;
                            IsHavingExchange = false;


                            var onAccount = tempInvoiceDto.InvoiceHistories.Where(x => x.Status == InvoiceStatus.OnAccount);

                            if (onAccount != null && onAccount.Count() > 0)
                                tempInvoiceDto.Status = InvoiceStatus.OnAccount;

                            var layBy = tempInvoiceDto.InvoiceHistories.Where(x => x.Status == InvoiceStatus.LayBy);
                            if (layBy != null && layBy.Count() > 0)
                                tempInvoiceDto.Status = InvoiceStatus.LayBy;
                        }
                    }
                    if (tempInvoiceDto.Status == InvoiceStatus.BackOrder || tempInvoiceDto.Status == InvoiceStatus.Parked || tempInvoiceDto.Status == InvoiceStatus.LayBy)
                    {
                        if (tempInvoiceDto.InvoicePayments.Sum(x => x.Amount) > 0)
                            IsInvoiceRefundAndDiscard = true;
                        else
                            IsInvoiceRefundAndDiscard = false;
                    }
                    tempInvoiceDto.IsCustomerChange = false;
                    tempInvoiceDto.isSync = false;


                    //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                    if (tempInvoiceDto.Status == InvoiceStatus.OnAccount && tempInvoiceDto.CustomerDetail != null)
                    {
                        CustomerDto_POS LocalCustomer;
                        if (tempInvoiceDto.CustomerDetail.Id > 0)
                            LocalCustomer = customerService.GetLocalCustomerById(tempInvoiceDto.CustomerDetail.Id);
                        else
                            LocalCustomer = customerService.GetLocalCustomerByTempId(tempInvoiceDto.CustomerDetail.TempId);


                        CustomFieldsResponce result = null;
                        result = tempInvoiceDto.CustomFields != null ? JsonConvert.DeserializeObject<CustomFieldsResponce>(tempInvoiceDto.CustomFields) : null;
                        if (result == null)
                            result = new CustomFieldsResponce();
                        tempInvoiceDto.InvoiceOutstanding = new InvoiceOutstanding()
                        {
                            currentSale = tempInvoiceDto.OutstandingAmount,
                            currentOutstanding = LocalCustomer.OutStandingBalance + payment.Amount,
                        };
                        tempInvoiceDto.InvoiceOutstanding.previousOutstanding = tempInvoiceDto.InvoiceOutstanding.currentOutstanding - tempInvoiceDto.InvoiceOutstanding.currentSale;
                        result.invoiceOutstanding = tempInvoiceDto.InvoiceOutstanding;
                        tempInvoiceDto.CustomFields = JsonConvert.SerializeObject(result);

                        LocalCustomer.OutStandingBalance += payment.Amount;
                        customerService.UpdateLocalCustomer(LocalCustomer);
                    }
                    //End Ticket #63876 by Pratik

                    ObservableCollection<InvoiceDto> Invoices = new ObservableCollection<InvoiceDto>() { tempInvoiceDto };
                    await saleService.UpdateLocalInvoiceThenSendItToServer(Invoices);
                    //await saleService.UpdateLocalInvoiceThenSendItToServer(tempInvoiceDto);
                    Invoice = tempInvoiceDto;
                    //}
                    //else
                    //{
                    //    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RemoveCreditPaymentMessage"));
                    //}
                    //Ticket end:#72472.by rupesh
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
                if (App.Instance.IsInternetConnected)
                {
                    SaleInvoiceEmailInput emailrequest = new SaleInvoiceEmailInput()
                    {
                        Email = IsEmailWithPayLink && IsEmailWithPayLinkVisible ? string.Empty : string.Join(";", SelectedEmails.Select(x => x)),
                        SyncReference = Invoice.InvoiceTempId,
                        InvoiceId = Invoice.Id,
                        CustomerId = Invoice.CustomerId,
                        RegisterId = Invoice.RegisterId.Value,
                        InvoiceNumber = Invoice.Number,
                        EmailTemplateType = (Invoice.Status == InvoiceStatus.Quote) ? EmailTemplateType.CustomerQuoteReceipt : (IsEmailWithPayLink && IsEmailWithPayLinkVisible ? EmailTemplateType.CustomerReceiptWithPaymentLink : EmailTemplateType.CustomerReceipt),
                        EmailForGiftCard = IsToPrintGiftReceipt,
                        InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android,
                        IsEmailPaymentLinkActive = IsEmailWithPayLink && IsEmailWithPayLinkVisible,
                        CountryCode = Settings.CountryCode,
                    };
                    if (!string.IsNullOrEmpty(email) && string.IsNullOrEmpty(emailrequest.Email))
                    {
                        emailrequest.Email = email;
                    }
                    var result = await saleService.SendInvoiceEmail(emailrequest);
                    if (result == LanguageExtension.Localize("EmailSuccess"))
                    {

                        App.Instance.Hud.DisplayToast(result, Colors.Green, Colors.White);
                        //Ticket start:#92767 iOS:FR Log when invoices, quotes and reports are sent by email.by rupesh
                        var newInvoiceHistory = new InvoiceHistoryDto();
                        newInvoiceHistory.Status = InvoiceStatus.EmailSent;
                        newInvoiceHistory.StatusName = "Email sent";
                        newInvoiceHistory.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                        newInvoiceHistory.ServerdBy = Invoice.ServedByName;
                        newInvoiceHistory.CreationTime = DateTime.UtcNow;
                        Invoice.InvoiceHistories.Add(newInvoiceHistory);
                        await saleService.UpdateLocalInvoice(Invoice);
                        SetPropertyChanged(nameof(Invoice));
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
            catch (Exception ex)
            {
                ex.Track();
            }
            return false;
        }

        public void SendErrors()
        {
            try
            {
                using (new Busy(this, true))
                {
                    if (Invoice != null)
                    {
                        var Errors = saleService.GetErrors(Invoice.Id, Invoice.InvoiceTempId);
                        if (Errors != null && Errors.Count > 0)
                        {
                            IEmailComposer emailComposer = DependencyService.Get<IEmailComposer>();
                            List<string> ToEmails = new List<string>() { "hello@hikeup.com" };
                            var body = "Please find attached error log.";
                            emailComposer.SendEmail(ToEmails, "Something went wrong (internal error) when synchronizing invoice #" + Invoice.Number + " with cloud.", body, JsonConvert.SerializeObject(Errors));
                        }
                    }
                }
            }
            catch (Exception exx)
            {
                exx.Track();
            }
        }


        private void backgroundHandle_Tapped()
        {
            if (CustomerModel != null)
            {
                CustomerModel.IsOpenSearchCustomerPopUp = 0;
            }
        }


        private void editNoteHandle_Clicked()
        {
            IsEditVisible = true;
            parkSaleDetailPage?._InvoiceNote.Focus();
        }
 
        private void refundHandle_Clicked()
        {
            if (EventCallRunning)
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? AppFeatures.AndroidMSecond : AppFeatures.IOSMSecond).Wait();
                EventCallRunning = false;
            });
            InvoiceRefund?.Invoke(this, Invoice);

        }

        private void exchangeHandle_Clicked()
        {
            if (EventCallRunning)
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? AppFeatures.AndroidMSecond : AppFeatures.IOSMSecond).Wait();
                EventCallRunning = false;
            });

            try
            {
                PropertyInfo myPropInfo;
                bool result = false;

                myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeInvoiceExchangeFeature");
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
            InvoiceExchange?.Invoke(this, Invoice);
        }

        private void CloseEmailView_Clicked()
        {
            BtnEmailText = "Email";
            CustomerEMailId = "";
            IsFrmEmailVisible = false;
            IsSelectedEmailListVisible = false;
            IsSelectEmailListVisible = false;
            IsEmailWithPayLinkVisible = false;
        }

        private async void emailHandle_Clicked()
        {
            if (EventCallRunning)
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? AppFeatures.AndroidMSecond : AppFeatures.IOSMSecond).Wait();
                EventCallRunning = false;
            });

            if (BtnEmailText == "Email")
            {
                IsFrmEmailVisible = true;
                BtnEmailText = "Send";
                IsSelectedEmailListVisible = SelectedEmailListHeight > 0 ? true : false;
                SetPayLink();
            }
            else
            {
                if(IsEmailWithPayLink && IsEmailWithPayLinkVisible)
                {
                    if (!string.IsNullOrEmpty(CustomerEMailId) && CustomerEMailId.IsValidEmail())
                    {
                        BtnEmailText = "Email";
                        IsFrmEmailVisible = false;
                        IsSelectedEmailListVisible = false;
                        IsSelectEmailListVisible = false;
                        var result = await SendEmail(CustomerEMailId);
                        if (result)
                        {
                            CustomerEMailId = "";
                        }
                        IsEmailWithPayLinkVisible = false;
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_InValidMessage"));
                    }
                }
                else
                {
                    var isEmailValid = CustomerEMailId.IsValidEmail();
                    if ((string.IsNullOrEmpty(CustomerEMailId) && SelectedEmails.Count > 0) || isEmailValid)
                    {
                        if (isEmailValid)
                            parkSaleDetailPage?._txtEmail.Unfocus();
                        BtnEmailText = "Email";
                        IsFrmEmailVisible = false;
                        IsSelectedEmailListVisible = false;
                        IsSelectEmailListVisible = false;
                        var result = await SendEmail(CustomerEMailId);
                        if (result)
                        {
                            CustomerEMailId = "";
                        }
                        IsEmailWithPayLinkVisible = false;
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_InValidMessage"));
                    }
                }
            }
        }

        void SetPayLink()
        {
            IsEmailWithPayLinkVisible = Invoice != null && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.AllowOnAccountPaymentHikePay && Invoice.Status == InvoiceStatus.OnAccount  && !IsToPrintGiftReceipt;
            if(IsEmailWithPayLinkVisible)
            {
                IsEmailWithPayLink = true;
                IsSelectedEmailListVisible = false;
            }
            else
            {
                IsEmailWithPayLink = false;
                IsSelectedEmailListVisible = SelectedEmailListHeight > 0 ? true : false;
            }
        }

        private async void duplicateQuote_Clicked()
        {
            if (EventCallRunning)
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? AppFeatures.AndroidMSecond : AppFeatures.IOSMSecond).Wait();
                EventCallRunning = false;
            });
            //var mainpage = (MainPage)_navigationService.MainPage;
            if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                await NavigationService.PopModalAsync();
            //mainpage.ChangePage("EnterSalePage");
            Shell.Current.CurrentItem = Shell.Current.Items[0];
            EnterSalePage.ServedBy = Settings.CurrentUser;
            ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel?.QuoteSelectedTapped(null);
            await ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.DuplicateFromQuoteFromHistory(Invoice);
        }

        public bool ConvertToSaleFromQuoteFromHistory(InvoiceDto invoice)
        {
            try
            {
                InvoiceDto invoiceDtoCopy = null;
                var productDto_s = new List<ProductQuote>();
                foreach (var lineItem in invoice.InvoiceLineItems)
                {
                    lineItem.InvoiceLineItemDetails = null;
                    if (lineItem.InvoiceItemType == InvoiceItemType.CompositeProduct)
                    {
                        foreach (var subLineItem in lineItem.InvoiceLineSubItems)
                        {
                            var product = productService.GetLocalProduct(subLineItem.ItemId);
                            if (product != null && product.TrackInventory && (product.ProductOutlet.Stock < lineItem.Quantity * subLineItem.Quantity) && (!product.AllowOutOfStock && !Settings.StoreGeneralRule.AllowSellingOutofStock))
                            {
                                productDto_s.Add(new ProductQuote()
                                {
                                    Name = product.Name,
                                    Sku = product.Sku,
                                    Stoke = product.Stock,
                                    Order = subLineItem.Quantity.ToString("0.####")
                                });
                            }

                        }
                    }
                    else if (lineItem.InvoiceItemType == InvoiceItemType.Standard)
                    {
                        var product = productService.GetLocalProduct(lineItem.InvoiceItemValue);
                        if (product != null && product.TrackInventory && (product.ProductOutlet.Stock < lineItem.Quantity) && (!product.AllowOutOfStock && !Settings.StoreGeneralRule.AllowSellingOutofStock))
                        {
                            productDto_s.Add(new ProductQuote()
                            {
                                Name = product.Name,
                                Sku = product.Sku,
                                Stoke = product.Stock,
                                Order = lineItem.Quantity.ToString("0.####")
                            });
                        }
                    }
                    else if (lineItem.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
                    {
                        var product = productService.GetLocalProduct(lineItem.InvoiceLineSubItems.FirstOrDefault().ItemId);
                        if (product != null && product.TrackInventory && (product.ProductOutlet.Stock < lineItem.Quantity) && (!product.AllowOutOfStock && !Settings.StoreGeneralRule.AllowSellingOutofStock))
                        {
                            productDto_s.Add(new ProductQuote()
                            {
                                Name = product.Name,
                                Sku = product.Sku,
                                Stoke = product.Stock,
                                Order = lineItem.Quantity.ToString("0.####")
                            });
                        }
                    }
                }
                if (productDto_s.Count > 0)
                {
                    App.Instance.Hud.DisplayActionToast("Not enough stock to convert this quote to sale", Colors.Red, null, null, async () =>
                    {
                        await _navigationService.GetCurrentPage.Navigation.PushModalAsync(new HikePOS.Pages.QuotePopupPage(productDto_s));
                    });
                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        private async void convertToSaleQuote_Clicked()
        {
            try
            {
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return;
                }
                //var mainpage = (MainPage)_navigationService.MainPage;
                var result = ConvertToSaleFromQuoteFromHistory(Invoice);

                if (result)
                {
                    if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                        await NavigationService.PopModalAsync();
                    //mainpage.ChangePage("EnterSalePage");
                    Shell.Current.CurrentItem = Shell.Current.Items[0];
                    EnterSalePage.ServedBy = Settings.CurrentUser;
                    ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.CustomerModel.SelectedCustomer = Invoice.CustomerDetail;
                    await ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.ConvertToSaleFromQuoteFromHistory(Invoice);
                    ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel?.ProcessSaleSelectedTapped(null);

                    if (!(Invoice.CustomerId == 0 && string.IsNullOrEmpty(Invoice.CustomerTempId)) &&
                            Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer)
                    {
                        try
                        {
                            PropertyInfo myPropInfo;
                            bool result1 = false;

                            myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeCustomerDeliverAddressFeature");

                            bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out result1);

                            if (!tempResult)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreFeatureNotAvailable"), Colors.Red, Colors.White);
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        var res = await App.Alert.ShowAlert(LanguageExtension.Localize("DeliveryAddressAlertTitle"), LanguageExtension.Localize("DeliveryAddressAlertMessage"), LanguageExtension.Localize("YesButtonText"), LanguageExtension.Localize("CancelButtonText"));
                        if (res)
                        {
                            ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.CustomerModel.DeliveryCustomerCommand.Execute(Invoice.CustomerDetail);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private void email_Selected(SelectEmail selectedEmail)
        {
            selectedEmail.IsSelected = !selectedEmail.IsSelected;
            IsSelectedEmailListVisible = true;
            IsSelectEmailListVisible = false;

            if (SelectedEmails == null)
            {
                SelectedEmails = new System.Collections.ObjectModel.ObservableCollection<string>();
            }
            if (selectedEmail.IsSelected)
                SelectedEmails.Add(selectedEmail.Email);
            else
                SelectedEmails.Remove(selectedEmail.Email);

            SelectedEmailListHeight = SelectedEmails.Count * 60;
            IsSelectedEmailListVisible = SelectedEmailListHeight > 0 ? true : false;

        }

        public void txtEmail_TextChanged()
        {
            try
            {
                IsSelectEmailListVisible = true;
                SelectEmails = CustomerEmails;
                SelectEmailListHeight = Math.Min(45 * SelectEmails.Count, 100);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private void removeEmail_Clicked(string selectedEmail)
        {
            var selectEmail = SelectEmails.FirstOrDefault(x => x.Email == selectedEmail);
            if (selectEmail != null)
                selectEmail.IsSelected = false;
            SelectedEmails.Remove(selectedEmail);
            SelectedEmailListHeight = SelectedEmails.Count * 60;
            IsSelectedEmailListVisible = SelectedEmailListHeight > 0 ? true : false;

        }

        public void txtEmail_Unfocused(Entry txtEmail)
        {
            if((!IsEmailWithPayLink || !IsEmailWithPayLinkVisible) || (IsEmailWithPayLink || !IsEmailWithPayLinkVisible))
            {
                if (txtEmail.Text.IsValidEmail())
                {
                    if (SelectedEmails == null)
                    {
                        SelectedEmails = new System.Collections.ObjectModel.ObservableCollection<string>();
                    }
                    SelectedEmails.Add(txtEmail.Text);
                    SelectedEmailListHeight = SelectedEmails.Count * 60;
                    IsSelectedEmailListVisible = true;
                    txtEmail.Text = "";
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_InValidMessage"));
                }
            }
        }

        private void taxInvoiceReceipt_Clicked()
        {
            IsToPrintGiftReceipt = false;
            SetPayLink();
        }

        private void giftReceipt_Clicked()
        {
            IsToPrintGiftReceipt = true;
            SetPayLink();
        }

        #endregion

        //Start TICKET #76698 iPAD FR: Next Sale by Pratik
        private async void PreviousSaleClick()
        {
            if (EventCallRunning)
                return;

            EventCallRunning = true;
            CurrentIndex++;
            NextPreviousButtonSet();
            await PrepareNextPreviousSale(ParkSales[CurrentIndex]);
            OnAppearing();
            SetInvoiceDetails();
            EventCallRunning = false;
        }

        private async void NextSaleClick()
        {
            if (EventCallRunning)
                return;

            EventCallRunning = true;
            CurrentIndex--;
            NextPreviousButtonSet();
            await PrepareNextPreviousSale(ParkSales[CurrentIndex]);
            OnAppearing();
            SetInvoiceDetails();
            EventCallRunning = false;
        }

        async Task PrepareNextPreviousSale(InvoiceDto invoice)
        {
            using (new Busy(this, true))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        if (invoice.OutletId == Settings.SelectedOutletId)
                        {

                            if (invoice.Id != 0)
                                invoice = saleService.GetLocalInvoice(invoice.Id);
                            else
                            {
                                invoice = saleService.GetLocalInvoiceByTempId(invoice.InvoiceTempId);
                                if (invoice == null)
                                {
                                    invoice = ParkSales.FirstOrDefault(x => x.InvoiceTempId == invoice.InvoiceTempId);
                                }
                            }
                        }

                        //Ticket start:#23855 iOS - Discount Not Applied When Editing Quotes.by rupesh
                        try
                        {
                            //Ticket start:#63489 IOS: Items Not sequence wise in print receipt.by rupesh
                            if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                                invoice.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(invoice.InvoiceLineItems.OrderBy(x => x.Sequence));
                            else
                            {
                                var orderedInvoiceLineItems = new List<InvoiceLineItemDto>();
                                var tempInvoiceLineItems = invoice.InvoiceLineItems.Where(x => !x.IsExtraproduct).OrderByDescending(x => x.Sequence).ToList();
                                foreach (var item in tempInvoiceLineItems)
                                {
                                    orderedInvoiceLineItems.Add(item);
                                    var subItems = invoice.InvoiceLineItems.Where(x => x.InvoiceExtraItemValueParent == item.Sequence).OrderByDescending(x => x.Sequence);
                                    if (subItems.Any())
                                    {
                                        orderedInvoiceLineItems.AddRange(subItems);
                                    }
                                }
                                invoice.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(orderedInvoiceLineItems);
                            }
                            //Ticket end:#63489 .by rupesh

                            foreach (var item in invoice.InvoiceLineItems)
                            {
                                //Ticket start:#26599 iPad: Discount offer should not be applied to UOM product.by rupesh
                                if (item.InvoiceItemType != InvoiceItemType.UnityOfMeasure && item.Category != null && !string.IsNullOrEmpty(item.Category) && item.CategoryDtos == null)
                                //Ticket end:#26599 .by rupesh
                                {
                                    var categories = JsonConvert.DeserializeObject<List<CategoryDto>>(item.Category);
                                    if (item.Category.Contains("categoryId"))
                                    {
                                        var productCategories = JsonConvert.DeserializeObject<List<ProductCategoryDto>>(item.Category);
                                        foreach (var category in categories)
                                        {
                                            var productCategory = productCategories.ElementAt(categories.IndexOf(category));
                                            category.Id = productCategory.CategoryId;
                                            category.ParentId = productCategory.ParentCategoryId;
                                        }
                                    }
                                    item.CategoryDtos = JsonConvert.SerializeObject(categories);
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                        //Ticket end:#23855 .by rupesh


                        Invoice = invoice;

                        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
                        if (invoice != null)
                        {
                            Invoice.SurchargeDisplayInSaleHistory = (invoice.TotalTip - invoice.TipTaxAmount);
                            //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                            Invoice.SurchargeDisplayInSaleHistory += Invoice.TotalPaymentSurcharge;
                            //End Ticket #73190
                        }
                        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total




                        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.

                        if (Settings.CurrentRegister.IsOpened)
                        {

                            if (invoice.Status == InvoiceStatus.Completed && invoice.InvoiceFrom != InvoiceFrom.iPad && invoice.InvoiceFrom != InvoiceFrom.Android
                                && invoice.InvoiceFrom != InvoiceFrom.Web && invoice.RegisterClosureId == Settings.CurrentRegister.Registerclosure.Id)

                                Invoice.IsSerialNumberEditableFromSaleHistory = true;
                            else
                                Invoice.IsSerialNumberEditableFromSaleHistory = false;
                        }
                        else
                        {
                            Invoice.IsSerialNumberEditableFromSaleHistory = false;
                        }

                        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.


                        // Invoice.InvoicePayments
                        if (Invoice.InvoicePayments != null)
                        {
                            foreach (var item in Invoice.InvoicePayments)
                            {
                                if (item.PaymentOptionType == PaymentOptionType.Linkly)
                                {

                                    foreach (var item1 in item.InvoicePaymentDetails)
                                    {
                                        //if (item1.Key == InvoicePaymentKey.LinklySessionId)
                                        //    item.PaymentOptionName = item.PrintPaymentOptionName = item.PaymentOptionName + "(#" + item1.Value + ")";
                                        //Ticket start:#27580 iPad (Linkly): Payment reference id is not recorded in sales history in web if sale was process in ipad.by rupesh.
                                        if (item1.Value != null && !item1.Value.Contains("****") && item1.Value.Contains("txnRef"))
                                        {
                                            var linklyContents = JsonConvert.DeserializeObject<LinklyResponseRoot>(item1.Value);


                                            if (!string.IsNullOrEmpty(linklyContents.response?.txnRef))
                                                item.PaymentOptionName = item.PrintPaymentOptionName = item.PaymentOptionName + "(#" + linklyContents.response.txnRef.TrimEnd() + ")";

                                        }
                                        //Ticket end:#27580 .by rupesh.

                                    }
                                }
                            }
                        }


                        LoadOutletsDetails();
                        GetTaxDetails();
                        IsOrderDetailVisible = false;
                        CollapseItemDetail("OrderDetail");
                        UpdateRefundHistoryList();

                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                    }
                });
            }
        }

        public void NextPreviousButtonSet()
        {
            PreviousEnable = false;
            NextEnable = false;
            if (ParkSales.Count > 1 && CurrentIndex > 0)
            {
                NextEnable = true;
            }
            if (ParkSales.Count > 1 && CurrentIndex < (ParkSales.Count - 1))
            {
                PreviousEnable = true;
            }
        }
        //End TICKET #76698 by Pratik

        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
        private async void OpenEligibilityProof()
        {
            try
            {
                Uri uri = EligibilityProofPath.GetImageUrl("AgeVerification");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
        //Ticket:end:#90938 .by rupesh

        //Ticket start:#91260 iOS:FR:Duplicating invoices.by rupesh
        private async void DuplicateInvoice()
        {
            if (EventCallRunning)
                return;
            EventCallRunning = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? AppFeatures.AndroidMSecond : AppFeatures.IOSMSecond).Wait();
                EventCallRunning = false;
            });
            var result = IsDuplicateInvoiceFromHistory(Invoice);
            if (result)
            {
                //var mainpage = (MainPage)_navigationService.MainPage;
                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                    await NavigationService.PopModalAsync();
                //mainpage.ChangePage("EnterSalePage");
                Shell.Current.CurrentItem = Shell.Current.Items[0];
                EnterSalePage.ServedBy = Settings.CurrentUser;
                _ = Task.Run(() =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.DuplicateInvoiceFromHistory(Invoice);
                    });
                });
            }
        }

        public bool IsDuplicateInvoiceFromHistory(InvoiceDto invoice)
        {
            try
            {

                var productDto_s = new List<ProductQuote>();
                foreach (var lineItem in invoice.InvoiceLineItems)
                {
                    if (Settings.CurrentRegister != null && Settings.CurrentRegister.Registerclosure != null)
                    {
                        lineItem.RegisterId = Settings.CurrentRegister.Id;
                        lineItem.RegisterName = Settings.CurrentRegister.Name;
                        lineItem.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id;
                    }
                    if (lineItem.InvoiceItemType == InvoiceItemType.CompositeProduct)
                    {
                        foreach (var subLineItem in lineItem.InvoiceLineSubItems)
                        {
                            var product = productService.GetLocalProduct(subLineItem.ItemId);
                            if (product != null && product.TrackInventory && (product.ProductOutlet.Stock < lineItem.Quantity * subLineItem.Quantity) && (!product.AllowOutOfStock && !Settings.StoreGeneralRule.AllowSellingOutofStock))
                            {
                                productDto_s.Add(new ProductQuote()
                                {
                                    Name = product.Name,
                                    Sku = product.Sku,
                                    Stoke = product.Stock,
                                    Order = subLineItem.Quantity.ToString("0.####")
                                });

                            }

                        }
                    }
                    else if (lineItem.InvoiceItemType == InvoiceItemType.Standard)
                    {
                        var product = productService.GetLocalProduct(lineItem.InvoiceItemValue);
                        if (product != null && product.TrackInventory && (product.ProductOutlet.Stock < lineItem.Quantity) && (!product.AllowOutOfStock && !Settings.StoreGeneralRule.AllowSellingOutofStock))
                        {
                            productDto_s.Add(new ProductQuote()
                            {
                                Name = product.Name,
                                Sku = product.Sku,
                                Stoke = product.Stock,
                                Order = lineItem.Quantity.ToString("0.####")
                            });

                        }
                    }
                    else if (lineItem.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
                    {
                        var product = productService.GetLocalProduct(lineItem.InvoiceLineSubItems.FirstOrDefault().ItemId);
                        if (product != null && product.TrackInventory && (product.ProductOutlet.Stock < lineItem.Quantity) && (!product.AllowOutOfStock && !Settings.StoreGeneralRule.AllowSellingOutofStock))
                        {
                            productDto_s.Add(new ProductQuote()
                            {
                                Name = product.Name,
                                Sku = product.Sku,
                                Stoke = product.Stock,
                                Order = lineItem.Quantity.ToString("0.####")
                            });

                        }
                    }

                }
                if (productDto_s.Count > 0)
                {
                    App.Instance.Hud.DisplayActionToast("Not enough stock to process a duplicate sale.", Colors.Red, Colors.White);
                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }
        //Ticket end:#91260 .by rupesh

    }
}
