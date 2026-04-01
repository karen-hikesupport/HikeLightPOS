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
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using HikePOS.Services.Payment;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Serialization;

namespace HikePOS.ViewModels
{
    public class RefundAndVoidViewModel : BaseViewModel
    {
        #region Services
        ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        PaymentServices paymentService;

        ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
        public ProductServices productService;

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        public SaleServices saleService;

        ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
        public OutletServices outletService;

        ApiService<ISubmitLogApi> logApiService = new ApiService<ISubmitLogApi>();
        SubmitLogServices logService;
        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;
        INadaTapToPay nadaTapToPay;
        INadaPayTerminalAppService HikeTerminalPay;

        #endregion

        #region Properties
        private InvoiceDto _Invoice;
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
            }
        }

        private List<PaymentOptionDto> _PaymentOptionList;
        public List<PaymentOptionDto> PaymentOptionList
        {
            get { return _PaymentOptionList; }
            set
            {
                _PaymentOptionList = value;
                SetPropertyChanged(nameof(PaymentOptionList));
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
                SetPropertyChanged(nameof(RefundAmountText));
            }
        }

        private decimal _tenderAmount = 0;
        public decimal TenderAmount
        {
            get { return _tenderAmount; }
            set
            {
                _tenderAmount = value;
                SetPropertyChanged(nameof(TenderAmount));
                //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
                if (PaymentOptionList != null)
                {
                    foreach (var item in PaymentOptionList.Where(a => a.PaymentOptionType == PaymentOptionType.Cash))
                    {
                        var amount = CashPaymentRoundingAmount();
                        item.DisplaySubName = Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent ? amount.ToString("C") : "";
                    }
                }
                //Ticket end:#91262 .by rupesh
            }
        }

        private string _firstButtonText;
        public string FirstButtonText { get { return _firstButtonText; } set { _firstButtonText = value; SetPropertyChanged(nameof(FirstButtonText)); } }

        private string _secondButtonText;
        public string SecondButtonText { get { return _secondButtonText; } set { _secondButtonText = value; SetPropertyChanged(nameof(SecondButtonText)); } }

        private string _windcavePopupText = string.Empty;
        public string WindcavePopupText { get { return _windcavePopupText; } set { _windcavePopupText = value; SetPropertyChanged(nameof(WindcavePopupText)); } }

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
                if (value)
                {
                    NewSaleEvent?.Invoke(this, EventArgs.Empty);
                }
                SetPropertyChanged(nameof(IsSuccessPaymentActive));

            }
        }

        public event EventHandler NewSaleEvent;

        private bool _IsPayment_PartialyPaid;
        public bool IsPayment_PartialyPaid { get { return _IsPayment_PartialyPaid; } set { _IsPayment_PartialyPaid = value; SetPropertyChanged(nameof(IsPayment_PartialyPaid)); } }

        public List<string> VantivCloudReceiptList;
        public List<string> AssemblyeConduitTempReceiptList;


        Dictionary<string, string> logRequestDetails = new Dictionary<string, string>();

        ObservableCollection<InvoicePaymentDetailDto> RedundAndVoidInvoicePaymentDetails = null;

        public EventHandler<PaymentOptionDto> InvoiceRefund;
        public bool IsPartiallyPaid = false;
        public InvoiceStatus Status;
        #endregion

        #region Page Declaration
        PaymentOptionDto SelectedPaymentOption = null;
        //  IFiska fiska;
        // IPaypalHere paypal;
        public RefundAndVoidPage RefundAndVoidPage;
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();

        #endregion

        #region Commands
        public ICommand WindcaveButtonSelectedOption { get; }
        public ICommand CloseHandleClickedCommand => new Command(closeHandle_Clicked);
        public ICommand SelectPaymentHandleClickedCommand => new Command<PaymentOptionDto>(selectPaymentHandle_Clicked);
        #endregion

        public RefundAndVoidViewModel()
        {
            paymentService = new PaymentServices(paymentApiService);
            productService = new ProductServices(productApiService);
            saleService = new SaleServices(saleApiService);
            outletService = new OutletServices(outletApiService);
            customerService = new CustomerServices(customerApiService);
            logService = new SubmitLogServices(logApiService);
            AssemblyeConduitTempReceiptList = new List<string>();
            VantivCloudReceiptList = new List<string>();
            AssemblyeConduitTempReceiptList.Clear();
            nadaTapToPay = DependencyService.Get<INadaTapToPay>();
           // HikeTerminalPay = DependencyService.Get<INadaPayTerminalRemoteAppService>();

        }

        public void LoadPaymentOption()
        {
            TenderAmount = (-1) * Invoice.TotalPaid;
            var TempPaymentOptions = paymentService.GetLocalPaymentOptions();

            TempPaymentOptions.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.PayPal
                                     || x.PaymentOptionType == Enums.PaymentOptionType.PayPalhere
                                     || x.PaymentOptionType == Enums.PaymentOptionType.Tyro
                                     || x.PaymentOptionType == Enums.PaymentOptionType.Mint
                                     || x.PaymentOptionType == Enums.PaymentOptionType.VantivIpad
                                     || x.PaymentOptionType == Enums.PaymentOptionType.VantivCloud
                                     || x.PaymentOptionType == Enums.PaymentOptionType.AssemblyPayment
                                     || x.PaymentOptionType == PaymentOptionType.EVOPayment
                                     || x.PaymentOptionType == PaymentOptionType.VerifonePaymark
                                     || x.PaymentOptionType == PaymentOptionType.PayJunction
                                     || x.PaymentOptionType == PaymentOptionType.eConduit
                                     || x.PaymentOptionType == PaymentOptionType.Moneris
                                     || x.PaymentOptionType == PaymentOptionType.NorthAmericanBankcard
                                     || x.PaymentOptionType == PaymentOptionType.TD
                                     || x.PaymentOptionType == PaymentOptionType.Elavon
                                     || x.PaymentOptionType == PaymentOptionType.Square
                                     || x.PaymentOptionType == PaymentOptionType.Castle
                                     || x.PaymentOptionType == PaymentOptionType.Clover
                                     || x.PaymentOptionType == PaymentOptionType.HikePayTapToPay
                                     || x.PaymentOptionType == PaymentOptionType.HikePay)
                              .All(x =>
                              {
                                  if (!string.IsNullOrEmpty(x.ConfigurationDetails) || x.PaymentOptionType == Enums.PaymentOptionType.AssemblyPayment)
                                  {
                                      x.IsConfigered = true;
                                  }
                                  else
                                  {
                                      if (x.PaymentOptionType == Enums.PaymentOptionType.TD || x.PaymentOptionType == Enums.PaymentOptionType.Elavon)
                                      {
                                          x.IsConfigered = true;
                                      }
                                      else
                                      {
                                          x.IsConfigered = false;
                                      }
                                  }
                                  return true;
                              });

            // var paymenttypes = TempPaymentOptions?.Where(x => !x.IsDeleted &&
            //            ((x.PaymentOptionType != Enums.PaymentOptionType.Tyro
            //             && x.PaymentOptionType != Enums.PaymentOptionType.PayPal
            //             && x.PaymentOptionType != Enums.PaymentOptionType.PayPalhere
            //             && x.PaymentOptionType != Enums.PaymentOptionType.iZettle
            //             && x.PaymentOptionType != Enums.PaymentOptionType.Mint
            //             && x.PaymentOptionType != Enums.PaymentOptionType.VantivCloud
            //             && x.PaymentOptionType != Enums.PaymentOptionType.VantivIpad
            //             && x.PaymentOptionType != Enums.PaymentOptionType.AssemblyPayment
            //             && x.PaymentOptionType != Enums.PaymentOptionType.TD
            //             && x.PaymentOptionType != PaymentOptionType.Elavon
            //             && x.PaymentOptionType != PaymentOptionType.Square
            //             && x.PaymentOptionType == PaymentOptionType.Castle
            //             && x.PaymentOptionType == PaymentOptionType.Clover
            //             && x.PaymentOptionType == PaymentOptionType.HikePayTapToPay)
            //             || (x.RegisterPaymentOptions == null || x.RegisterPaymentOptions.Count < 1 || x.RegisterPaymentOptions.Any(y => y.RegisterId == Settings.CurrentRegister.Id))));

            var paymenttypes = TempPaymentOptions?
                .Where(x => !x.IsDeleted &&
                            Enum.IsDefined(typeof(PaymentOptionType), x.PaymentOptionType) &&
                            (x.RegisterPaymentOptions == null ||
                             x.RegisterPaymentOptions.Count < 1 ||
                             x.RegisterPaymentOptions.Any(y => y.RegisterId == Settings.CurrentRegister.Id)) &&
                            (x.PaymentOptionType != PaymentOptionType.TyroTapToPay ||
                             (Settings.TyroTapToPayConfiguration != null && x.Id == Settings.TyroTapToPayConfiguration.Id)) &&
                            (x.PaymentOptionType != PaymentOptionType.HikePayTapToPay ||
                             (x.ConfigurationDetails != null &&
                              JsonConvert.DeserializeObject<NadaPayConfigurationDto>(x.ConfigurationDetails)?.OutletId == Settings.SelectedOutletId))
                      );

            //  Note : below code for remove Tap To Pay related payment
            // paymenttypes = paymenttypes.Where(x => !(x.PaymentOptionType == PaymentOptionType.TyroTapToPay && (Settings.TyroTapToPayConfiguration == null || x.Id != Settings.TyroTapToPayConfiguration.Id)));
            // paymenttypes = paymenttypes.Where(x =>
            // {
            //     if (x.PaymentOptionType != PaymentOptionType.HikePayTapToPay)
            //         return true;

            //     if (x.ConfigurationDetails == null)
            //         return false;

            //     var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(x.ConfigurationDetails);
            //     if (config == null)
            //         return false;

            //     return config.OutletId == Settings.SelectedOutletId;
            // });

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                paymenttypes = paymenttypes.Where(x => x.PaymentOptionType != PaymentOptionType.TD
                && x.PaymentOptionType != PaymentOptionType.Elavon);
            }

 
            paymenttypes.All(x => { x.DisplayName = x.Name; return true; });

            var tmppayment = paymenttypes.Where(x => x.IsConfigered && !x.IsDeleted && x.IsActive && x.PaymentOptionType != PaymentOptionType.Credit && x.PaymentOptionType != PaymentOptionType.Ecommerce && x.PaymentOptionType != PaymentOptionType.GiftCard && x.PaymentOptionType != PaymentOptionType.Layby && x.PaymentOptionType != PaymentOptionType.Loyalty && x.PaymentOptionType != PaymentOptionType.OnAccount).ToList();

            tmppayment = tmppayment.Where(x => x.RegisterPaymentOptions.Count() == 0 || x.RegisterPaymentOptions.Any(y => y.RegisterId == Settings.CurrentRegister.Id)).ToList();

            //Start #94427 Disable cash option on a single register By Pratik
            if (tmppayment != null && Settings.CurrentRegister != null && !Settings.CurrentRegister.IsActiveCashPayment)
            {
                tmppayment = tmppayment.Where(a => a.PaymentOptionType != PaymentOptionType.Cash).ToList();
            }
            //End #94427 By Pratik

            tmppayment = tmppayment.OrderBy(x => x.DisplayName).ToList();
            //Ticket start:#94416 iOS:FR :Refund and discard option work-around.by rupesh
            if (Invoice.CustomerId != null && Invoice.CustomerId > 0)
            {
                var customer = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                if (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.RefundSaleWithStoreCredit"))
                {
                    foreach (var creditBalance in paymenttypes.Where(x => x.IsConfigered && !x.IsDeleted && x.IsActive && x.PaymentOptionType == PaymentOptionType.Credit))
                    {
                        if (creditBalance != null)
                        {
                            customer.CreditBalance = customer.CreditBalance ?? 0;
                            creditBalance.DisplayName = "Store credit";
                            creditBalance.DisplaySubName = customer.CreditBalance.Value.ToString("C");
                            tmppayment.Add(creditBalance);
                        }
                    }
                }
            }
            //Ticket end:#94416 .by rupesh

            PaymentOptionList = new List<PaymentOptionDto>();
            foreach (var payment in tmppayment)
            {
                PaymentOptionList.Add(payment);
                // Note : below code is used to add Moto option 
                if (payment.PaymentOptionType == PaymentOptionType.AssemblyPayment)
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

                    PaymentOptionList.Add(temp);
                }
            }

            //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
            if (PaymentOptionList != null)
            {
                foreach (var item in PaymentOptionList.Where(a => a.PaymentOptionType == PaymentOptionType.Cash))
                {
                    var amount = CashPaymentRoundingAmount();
                    item.DisplaySubName = Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent ? amount.ToString("C") : "";
                }
            }
            //Ticket end:#91262 .by rupesh

            RefundAmountText = string.Empty;
            var payments = Invoice.InvoicePayments;

            if (payments != null)
            {
                var tempString = string.Empty;
                foreach (var item in Invoice.InvoicePayments)
                {
                    string amount = string.Format("{0:C}", item.Amount.ToString("C"));
                    var tempstring1 = item.PaymentOptionName + " " + amount;
                    tempString = tempString + " " + tempstring1;
                }
                RefundAmountText = "This sale was originally paid with "
                        + tempString + ".";
            }
        }

        #region selected payment while payment
        public async Task<bool> selectPaymentOption(PaymentOptionDto paymentOption)
        {
            if (paymentOption == null)
            {
                return false;
            }

            if (paymentOption != null && (paymentOption.PaymentOptionType == PaymentOptionType.Credit || paymentOption.PaymentOptionType == PaymentOptionType.Layby || paymentOption.PaymentOptionType == PaymentOptionType.Loyalty || paymentOption.PaymentOptionType == PaymentOptionType.OnAccount) && (Invoice.CustomerId == null || Invoice.CustomerId < 2))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("PayButtonCutomerValidation"));
                return false;
            }
            SelectedPaymentOption = paymentOption;
            using (new Busy(this, true))
            {
                try
                {
                    bool result = false;
                    if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.HikePayTapToPay)
                    {
                        if (!App.Instance.IsInternetConnected)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"));
                            return false;
                        }

                        if (string.IsNullOrEmpty(paymentOption.ConfigurationDetails))
                        {
                            App.Instance.Hud.DisplayToast(paymentOption.Name + " is not configured!Please configure from Web and sync data from Setting");
                            return false;
                        }

                        if (Invoice.TenderAmount > (Invoice.TotalPay))
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("AmountValidationForIntegratedPaymentTypeMessage"));
                            return false;
                        }


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
                                    return false;
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
                            NadaTapToPayDto saleResult = null;
                            if (amount > 0)
                            {
                                // SaveInLocalbeforePayment(tempobject, false);
                                saleResult = await nadaTapToPay.Sale(amount, Invoice.Currency, lastReference);
                            }
                            else
                            {
                                NadaPayTransactionDto nadaTapToPayDtoResponse = new NadaPayTransactionDto();
                                decimal authorizedAmount = 0;
                                decimal requestedAmount = amount.ToPositive();

                                var tapToPaySells = Invoice.InvoicePayments?
                                        .Where(x => x.ActionType == ActionType.Sell && (x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay))
                                        ?? Enumerable.Empty<InvoicePaymentDto>();

                                var tapToPayRefunds = Invoice.InvoicePayments?
                                        .Where(x => x.ActionType == ActionType.Refund && (x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay))
                                        ?? Enumerable.Empty<InvoicePaymentDto>();


                                if(tapToPaySells.Count() > 1)
                                {
                                    var refundSlider = new AdyenRefundPaymentPage();
                                    refundSlider.ViewModel.InvoiceRefundPayments = new ObservableCollection<RefundPaymentDto>();
                                    foreach(var refundPayment in tapToPaySells)
                                    {
                                        NadaPayTransactionDto nadaTapToPayDtoReturnResponse = null;
                                        decimal refundedamt = 0;
                                       // decimal payamt = 0;
                                        var refundDetail = refundPayment.InvoicePaymentDetails
                                        .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);
                                    
                                        try
                                        {
                                            nadaTapToPayDtoReturnResponse = JsonConvert.DeserializeObject<NadaPayTransactionDto>(refundDetail?.Value);
                                        }
                                        catch
                                        {
                                            App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                            return false;
                                        }

                                        foreach (var payment in tapToPayRefunds)
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
                                               return false;
                                            }

                                        }

                                        refundSlider.ViewModel.InvoiceRefundPayments.Add(new RefundPaymentDto
                                        {
                                            Amount =  refundPayment.Amount,
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
                                            var Selected_RefundPayment = tapToPaySells.FirstOrDefault(a=> a.Id == refundPayment.Id);
                                            result = await HandlePartialRefund(paymentOption, Selected_RefundPayment, null,refundPayment, true);
                                            refundSlider.ViewModel.IsBusy = false;
                                            if (result)
                                            {
                                                //refundSlider?.Close();
                                                while (_navigationService.RootPage.Navigation.ModalStack != null && _navigationService.RootPage.Navigation.ModalStack.Count > 0)
                                                    await _navigationService.RootPage.Navigation.PopModalAsync();

                                                InvoiceRefund?.Invoke(this, paymentOption);
                                            }
                                        }
                                    };

                                    await NavigationService.PushModalAsync(refundSlider);
                                    return false;
                                }

                             
                                foreach (var refundPayment in tapToPaySells)
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
                                        authorizedAmount = nadaTapToPayDtoResponse.Amount;
                                        lastReference = nadaTapToPayDtoResponse?.SaleTransactionId;
                                        tempobject = "Sale: " + paymentOption.ConfigurationDetails + " tempInvoiceID: " + Invoice.InvoiceTempId + " Amount: " + amount.ToString() + "Last reference: " + lastReference + " Invoice.Note: " + Invoice.Note;
                        
                                    }
                                    catch
                                    {
                                        App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                        return false;
                                    }

                                    var payments = tapToPayRefunds
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
                                            Invoice.Status = Status;
                                            return false;
                                        }
                                    }
                                    if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                                        break;
                                }

                                if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                                {
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
                                        requestedAmount,Invoice.Currency, Guid.NewGuid().ToString(), merchantReference, poiTransactionId, poiTransactionTimeStamp, config.BalanceAccountId, surchargeAmount,fixedCommission, variableCommissionPercentage, paymentFee);
                                }
                                else
                                {
                                    App.Instance.Hud.DisplayToast(
                                        $"Refund amount for {paymentOption.Name} should be equal or less than the original sale amount",
                                        Colors.Red, Colors.White
                                    );
                                    Invoice.Status = Status;
                                    return false;
                                }
                            }
                            if (saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.Result != null && saleResult.SaleToPOIResponse.TransactionResponse.Response.Result.ToLower().Contains("success"))
                            {
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
                                        };
                                RedundAndVoidInvoicePaymentDetails = InvoicePaymentDetails;
                                SendLogsToServer(paymentOption, null, logRequestDetails);
                                Invoice.TenderAmount = 0;
                                RefundAndVoidTransactionComplete(paymentOption);
                                ObservableCollection<InvoiceDto> tempInvoices1 = new ObservableCollection<InvoiceDto>() { Invoice };
                                await saleService.UpdateLocalInvoiceThenSendItToServer(tempInvoices1);
                                WeakReferenceMessenger.Default.Send(new Messenger.SignalRInvoiceMessenger(Invoice));
                                result = true;
                                Logger.SyncLogger("----Hikepay Status---\n" + "Success" + "\n----HikepayTapToPay response---\n" + transactionResult);

                                return result;
                            }
                            else
                            {
                                result = false;
                                Invoice.Status = Status;
                                if (string.IsNullOrEmpty(saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.ErrorCondition))
                                    App.Instance.Hud.DisplayToast("Failed", Colors.Red, Colors.White);
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
                            Invoice.Status = Status;
                            App.Instance.Hud.DisplayToast("Hikepay transaction is not allowed");
                        }
                        //Ticket end:#78050 . by rupesh
                    }
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.HikePay)
                    {
                        if (!App.Instance.IsInternetConnected)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"));
                            return false;
                        }

                        if (string.IsNullOrEmpty(paymentOption.ConfigurationDetails))
                        {
                            App.Instance.Hud.DisplayToast(paymentOption.Name + " is not configured!Please configure from Web and sync data from Setting");
                            return false;
                        }

                        if (Invoice.TenderAmount > (Invoice.TotalPay))
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("AmountValidationForIntegratedPaymentTypeMessage"));
                            return false;
                        }

                        try
                        {
                              if (paymentOption.ConfigurationDetails != null)
                              {
                                var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
                                // Ensure service instance
                                HikeTerminalPay ??=  DependencyService.Get<INadaPayTerminalLocalAppService>();
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
                                    saleResult = await HikeTerminalPay.CreateNadaPayTerminalSale(amount, Invoice.Currency, lastReference, paymentOption, Invoice.InvoiceTempId);
                                }
                                else
                                {
                                    NadaPayTransactionDto hikePayTerminalResponse = new NadaPayTransactionDto();
                                    // decimal authorizedAmount = 0;
                                    // decimal requestedAmount = amount.ToPositive();

                                    // var hikePayTerminalRefunds = Invoice.InvoiceRefundPayments?
                                    //     .Where(x => x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay)
                                    //     ?? Enumerable.Empty<InvoicePaymentDto>();

                                    // NadaPayTransactionDto nadaTapToPayDtoResponse = new NadaPayTransactionDto();
                                    decimal authorizedAmount = 0;
                                    decimal requestedAmount = amount.ToPositive();

                                    var terminalPaySells = Invoice.InvoicePayments?
                                            .Where(x => x.ActionType == ActionType.Sell && (x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay))
                                            ?? Enumerable.Empty<InvoicePaymentDto>();

                                    var terminalPayRefunds = Invoice.InvoicePayments?
                                            .Where(x => x.ActionType == ActionType.Refund && (x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay))
                                            ?? Enumerable.Empty<InvoicePaymentDto>();

                                    if(terminalPaySells.Count() > 1)
                                    {
                                        var refundSlider = new AdyenRefundPaymentPage();
                                        refundSlider.ViewModel.InvoiceRefundPayments = new ObservableCollection<RefundPaymentDto>();
                                        foreach(var refundPayment in terminalPaySells)
                                        {
                                            NadaPayTransactionDto nadaTapToPayDtoReturnResponse = null;
                                            decimal refundedamt = 0;
                                        // decimal payamt = 0;
                                            var refundDetail = refundPayment.InvoicePaymentDetails
                                            .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);
                                        
                                            try
                                            {
                                                nadaTapToPayDtoReturnResponse = JsonConvert.DeserializeObject<NadaPayTransactionDto>(refundDetail?.Value);
                                            }
                                            catch
                                            {
                                                App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                                return false;
                                            }

                                            foreach (var payment in terminalPayRefunds)
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
                                                    return false;
                                                }

                                            }

                                            refundSlider.ViewModel.InvoiceRefundPayments.Add(new RefundPaymentDto
                                            {
                                                Amount =  refundPayment.Amount,
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
                                                var Selected_RefundPayment = terminalPaySells.FirstOrDefault(a=> a.Id == refundPayment.Id);
                                                result = await HandlePartialRefund(paymentOption, Selected_RefundPayment, null,refundPayment, true);
                                                refundSlider.ViewModel.IsBusy = false;
                                                if (result)
                                                {
                                                    //refundSlider?.Close();
                                                    while (_navigationService.RootPage.Navigation.ModalStack != null && _navigationService.RootPage.Navigation.ModalStack.Count > 0)
                                                        await _navigationService.RootPage.Navigation.PopModalAsync();

                                                    InvoiceRefund?.Invoke(this, paymentOption);
                                                }
                                            }
                                        };

                                        await NavigationService.PushModalAsync(refundSlider);
                                        return false;
                                    }

                                    foreach (var refundPayment in terminalPaySells)
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
                                        catch
                                        {
                                            App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                                            return false;
                                        }

                                        var payments = terminalPayRefunds
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
                                                Invoice.Status = Status;
                                                return false;
                                            }
                                        }
                                        if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                                            break;
                                    }

                                    if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                                    {
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
                                            return false;
                                        }
                                        var fixedCommission = rule.SplitLogic?.Commission?.FixedAmount ?? 0;
                                        var variableCommissionPercentage = rule.SplitLogic?.Commission?.VariablePercentage ?? 0;
                                        var paymentFee = rule.SplitLogic?.PaymentFee ?? 0;

                                        saleResult = await HikeTerminalPay.CreateNadaPayTerminalRefund(isPartial,
                                            requestedAmount,Invoice.Currency, Guid.NewGuid().ToString(), merchantReference, poiTransactionId, poiTransactionTimeStamp, surchargeAmount, fixedCommission,variableCommissionPercentage, paymentFee, paymentOption, hikePayTerminalResponse);
                                        IsBusy = false;
                                    }
                                    else
                                    {
                                        App.Instance.Hud.DisplayToast(
                                            $"Refund amount for {paymentOption.Name} should be equal or less than the original sale amount",
                                            Colors.Red, Colors.White
                                        );
                                        Invoice.Status = Status;
                                        return false;
                                    }
                                }

                                if (saleResult?.DecodeAdditonalResponse != null && saleResult.PaymentStatusSuccess)
                                {
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
                                    };
                                  
                                    RedundAndVoidInvoicePaymentDetails = InvoicePaymentDetails;
                                    SendLogsToServer(paymentOption, null, logRequestDetails);
                                    Invoice.TenderAmount = 0;
                                    RefundAndVoidTransactionComplete(paymentOption);
                                    ObservableCollection<InvoiceDto> tempInvoices1 = new ObservableCollection<InvoiceDto>() { Invoice };
                                    await saleService.UpdateLocalInvoiceThenSendItToServer(tempInvoices1);
                                    WeakReferenceMessenger.Default.Send(new Messenger.SignalRInvoiceMessenger(Invoice));
                                    result = true;
                                    Logger.SyncLogger("----Hikepay Status---\n" + "Success" + "\n----HikepayTapToPay response---\n" + transactionResult);
                                    return result;
                                }
                                else
                                {
                                    result = false;
                                    Invoice.Status = Status;
                                    if (string.IsNullOrEmpty(saleResult?.RefusalReason))
                                        App.Instance.Hud.DisplayToast("Failed", Colors.Red, Colors.White);
                                    else
                                        App.Instance.Hud.DisplayToast(saleResult.RefusalReason, Colors.Red, Colors.White);

                                    var transactionResult = "Empty";
                                    if (saleResult != null)
                                        transactionResult = JsonConvert.SerializeObject(saleResult);
                                    Logger.SyncLogger("----Hikepay Status---\n" + "Failed" + "\n----HikepayTapToPay response---\n" + transactionResult);
                                }
                            }
                            else
                            {
                                result = false;
                                Invoice.Status = Status;
                                App.Instance.Hud.DisplayToast("Hikepay transaction is not allowed");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.SyncLogger("----HikeTerminalpay Status---\n" + "Failed" + "\n---- HikeTerminalpay response---\n" + ex.Message);
                        }

                    }
                    
                    //Ticket start:#94416 iOS:FR :Refund and discard option work-around.by rupesh
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.Credit)
                    {
                        if (!App.Instance.IsInternetConnected)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"));
                            return false;
                        }
                        if (Invoice.TenderAmount > (Invoice.TotalPay))
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("AmountValidationForIntegratedPaymentTypeMessage"));
                            return false;
                        }

                        var customerResult = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                        customerResult.CreditBalance -= Invoice.TenderAmount;
                        Invoice.CustomerDetail.CreditBalance = customerResult.CreditBalance;
                        customerService.UpdateLocalCustomer(customerResult);
                        result = true;

                    }
                    //Ticket end:#94416 .by rupesh
                    else if (paymentOption.PaymentOptionType == Enums.PaymentOptionType.Cash || paymentOption.PaymentOptionType == Enums.PaymentOptionType.Card)
                    {
                        result = true;
                    }
                    if (result)
                    {
                        Invoice.TenderAmount = 0;
                        RefundAndVoidTransactionComplete(paymentOption);
                        ObservableCollection<InvoiceDto> tempInvoices1 = new ObservableCollection<InvoiceDto>() { Invoice };
                        await saleService.UpdateLocalInvoiceThenSendItToServer(tempInvoices1);
                        WeakReferenceMessenger.Default.Send(new Messenger.SignalRInvoiceMessenger(Invoice));
                        // ClosePage();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
            return false;
        }

        private async Task<bool> HandlePartialRefund(PaymentOptionDto paymentOption, InvoicePaymentDto selected_RefundPayment, object value, RefundPaymentDto refundPayment, bool isadyen = false)
        {
            if(isadyen)
            {
                return await HandlePartialRefundCall(paymentOption, selected_RefundPayment, value,refundPayment);
            }
            else
            {
                using (new Busy(this, false))
                {
                    return await HandlePartialRefundCall(paymentOption, selected_RefundPayment, value,refundPayment);
                }
            }
        }

        private async Task<bool> HandlePartialRefundCall(PaymentOptionDto paymentOption, InvoicePaymentDto selected_RefundPayment, object value, RefundPaymentDto refundPayment)
        {
            if (selected_RefundPayment.PaymentOptionType == PaymentOptionType.HikePayTapToPay)
            {
                var tapToPayRefunds = Invoice.InvoicePayments?
                                            .Where(x => x.ActionType == ActionType.Refund && (x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay))
                                            ?? Enumerable.Empty<InvoicePaymentDto>();

                decimal amount = Convert.ToDecimal(refundPayment.TenderedAmount);
                NadaTapToPayDto saleResult;
                string tempobject = string.Empty;
                NadaPayTransactionDto nadaTapToPayDtoResponse = null;
                decimal authorizedAmount = 0;
                decimal requestedAmount = amount.ToPositive();

                var paymentDetail = selected_RefundPayment.InvoicePaymentDetails
                    .FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData);

                try
                {
                    nadaTapToPayDtoResponse = JsonConvert.DeserializeObject<NadaPayTransactionDto>(paymentDetail?.Value);
                    // if (nadaTapToPayDtoResponse.SaleToPOIResponse == null && (int)selected_RefundPayment.PaymentOptionType == 49)
                    // {
                    //     var hikePayDto = JsonConvert.DeserializeObject<HikePayFromCloud>(paymentDetail?.Value);
                    //     nadaTapToPayDtoResponse = HikePayFromCloud.MapToNada(hikePayDto);
                    // }
                    var lastReference = nadaTapToPayDtoResponse.SaleTransactionId;
                    tempobject = "Sale: " + paymentOption.ConfigurationDetails + " tempInvoiceID: " + Invoice.InvoiceTempId + " Amount: " + amount.ToString() + "Last reference: " + lastReference + " Invoice.Note: " + Invoice.Note;

                    authorizedAmount = nadaTapToPayDtoResponse.Amount;
                }
                catch
                {
                    App.Instance.Hud.DisplayToast("Invalid payment data", Colors.Red, Colors.White);
                    return false;
                }

                // -----------------------------
                // 2) Subtract amounts from related payments
                // -----------------------------
                var payments = tapToPayRefunds
                    .Where(x => x.PaymentOptionId == selected_RefundPayment.PaymentOptionId);

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

                if (authorizedAmount >= requestedAmount && authorizedAmount > 0)
                {
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
                    var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
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
                            })}
                        };
                    RedundAndVoidInvoicePaymentDetails = InvoicePaymentDetails;
                    SendLogsToServer(paymentOption, null, logRequestDetails);
                    Invoice.TenderAmount = 0;
                    TenderAmount = amount;
                    RefundAndVoidTransactionComplete(paymentOption);
                    ObservableCollection<InvoiceDto> tempInvoices1 = new ObservableCollection<InvoiceDto>() { Invoice };
                    await saleService.UpdateLocalInvoiceThenSendItToServer(tempInvoices1);
                    WeakReferenceMessenger.Default.Send(new Messenger.SignalRInvoiceMessenger(Invoice));
                    Logger.SyncLogger("----Hikepay Status---\n" + "Success" + "\n----HikepayTapToPay response---\n" + transactionResult);
                    return true;
                }
                else
                {
                    Invoice.Status = Status;
                    if (string.IsNullOrEmpty(saleResult?.SaleToPOIResponse?.TransactionResponse?.Response?.ErrorCondition))
                        App.Instance.Hud.DisplayToast("Failed", Colors.Red, Colors.White);
                    else
                        App.Instance.Hud.DisplayToast(saleResult.SaleToPOIResponse.TransactionResponse.Response.ErrorMessage, Colors.Red, Colors.White);

                    var transactionResult = "Empty";
                    if (saleResult != null)
                        transactionResult = JsonConvert.SerializeObject(saleResult);
                    Logger.SyncLogger("----Hikepay Status---\n" + "Failed" + "\n----HikepayTapToPay response---\n" + transactionResult);
                    return false;
                }
            }
            else if (selected_RefundPayment.PaymentOptionType == PaymentOptionType.HikePay)
            {
                var tapToPayRefunds = Invoice.InvoicePayments?
                            .Where(x => x.ActionType == ActionType.Refund && (x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay))
                            ?? Enumerable.Empty<InvoicePaymentDto>();

                decimal amount = Convert.ToDecimal(refundPayment.TenderedAmount);
                HikePayTerminalResponse saleResult;
                string tempobject = string.Empty;
                NadaPayTransactionDto nadaTapToPayDtoResponse = null;
                decimal authorizedAmount = 0;
                decimal requestedAmount = amount.ToPositive();

                // -----------------------------
                // 1) Get base authorized amount
                // -----------------------------
                var paymentDetail = selected_RefundPayment.InvoicePaymentDetails
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
                var payments = tapToPayRefunds
                    .Where(x => x.PaymentOptionId == selected_RefundPayment.PaymentOptionId);

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
                    RedundAndVoidInvoicePaymentDetails = InvoicePaymentDetails;
                    SendLogsToServer(paymentOption, null, logRequestDetails);
                    Invoice.TenderAmount = 0;
                    TenderAmount = amount;
                    RefundAndVoidTransactionComplete(paymentOption);
                    ObservableCollection<InvoiceDto> tempInvoices1 = new ObservableCollection<InvoiceDto>() { Invoice };
                    await saleService.UpdateLocalInvoiceThenSendItToServer(tempInvoices1);
                    WeakReferenceMessenger.Default.Send(new Messenger.SignalRInvoiceMessenger(Invoice));
                    Logger.SyncLogger("----Hikepay Status---\n" + "Success" + "\n----HikepayTapToPay response---\n" + transactionResult);
                    return true;

                }
                else
                {
                    Invoice.Status = Status;
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

                return false;

        }


        private void RefundInvoice(PaymentOptionDto paymentOptionDto)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                return;
            }
        }

        private async void paypalpage_PaymentSuccessed(Message<PaypalPaymentResult> paypalresponse)
        {
            if (paypalresponse == null)
            {
                return;
            }

            if (paypalresponse.Type == MessageType.Success && paypalresponse.Result != null)
            {
                if (SelectedPaymentOption != null && SelectedPaymentOption.PaymentOptionType == PaymentOptionType.PayPal)
                {
                    ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();
                    InvoicePaymentDetails.Add(new InvoicePaymentDetailDto { Key = InvoicePaymentKey.PaypalResponse, Value = paypalresponse.Result.ToJson() });

                    Invoice.TenderAmount = 0;
                    RefundAndVoidTransactionComplete(SelectedPaymentOption);
                    ObservableCollection<InvoiceDto> tempInvoices1 = new ObservableCollection<InvoiceDto>() { Invoice };
                    await saleService.UpdateLocalInvoiceThenSendItToServer(tempInvoices1);
                    WeakReferenceMessenger.Default.Send(new Messenger.SignalRInvoiceMessenger(Invoice));

                    ClosePage();
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

        public async Task<bool> addIntegratedPaymentToSell(PaymentOptionDto paymentType, string transactionResult, ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails)
        {
            try
            {
                string temppaymetGuid = new Guid().ToString();
                InvoicePaymentDto payment = new InvoicePaymentDto()
                {
                    PaymentOptionId = paymentType.Id,
                    SyncPaymentReference = temppaymetGuid,
                    PaymentOptionName = paymentType.Name,
                    PaymentOptionType = paymentType.PaymentOptionType
                };

                var outstanding = Invoice.NetAmount.ToPositive() - Invoice.TotalPaid.ToPositive();
                if (Invoice.TenderAmount.ToPositive() >= outstanding)
                {
                    payment.Amount = Invoice.NetAmount - Invoice.TotalPaid;
                }
                else
                {
                    payment.Amount = Invoice.TenderAmount;
                }

                payment.TenderedAmount = Invoice.TenderAmount;
                payment.IsPaid = true;
                payment.RegisterId = Settings.CurrentRegister.Id;
                payment.RegisterName = Settings.CurrentRegister.Name;
                payment.OutletId = Settings.SelectedOutletId;
                payment.OutletName = Settings.SelectedOutletName;

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
                    || payment.PaymentOptionType == PaymentOptionType.VantivCloud || payment.PaymentOptionType == PaymentOptionType.AssemblyPayment || payment.PaymentOptionType == PaymentOptionType.Castle || payment.PaymentOptionType == PaymentOptionType.Clover || payment.PaymentOptionType == PaymentOptionType.HikePayTapToPay || payment.PaymentOptionType == PaymentOptionType.HikePay)
                {
                    payment.InvoicePaymentDetails = InvoicePaymentDetails;
                }
                if (Invoice.InvoicePayments == null)
                {
                    Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                }

                if (Invoice.InvoicePayments == null)
                {
                    Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                }

                Invoice.InvoicePayments.Add(payment);
                var Result = await calculateInvoicePayment(payment.PaymentOptionType);

                Invoice.TenderAmount = Invoice.NetAmount - Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);
                return Result;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }



        }

        async Task<bool> calculateInvoicePayment(PaymentOptionType PaymentType)
        {
            try
            {
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.RoundUptoFiveCent && PaymentType == PaymentOptionType.Cash)
                {
                    if (Invoice.Status != InvoiceStatus.Refunded)
                    {
                        var tempRoundAmount = (Invoice.InvoicePayments.Sum(x => x.RoundingAmount) - Invoice.RoundingAmount);
                        Invoice.RoundingAmount = Invoice.InvoicePayments.Sum(x => x.RoundingAmount);
                        Invoice.NetAmount += tempRoundAmount;
                    }
                    else
                    {
                        decimal tempRoundAmount = 0;
                        if (Invoice.RoundingAmount == 0)
                            tempRoundAmount = (Invoice.InvoicePayments.Sum(x => x.RoundingAmount) - Invoice.RoundingAmount);
                        else
                            tempRoundAmount = Invoice.RoundingAmount;
                        Invoice.NetAmount += tempRoundAmount;
                    }
                }
                var outstanding = Invoice.NetAmount.ToPositive() - Invoice.TotalPaid.ToPositive();
                if (Invoice.TenderAmount.ToPositive() > outstanding)
                {
                    Invoice.TotalTender = Invoice.TotalPaid + Invoice.TenderAmount;
                }
                else
                {
                    Invoice.TotalTender = Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);
                }

                var iscomplete = false;

                if (Invoice.Status == InvoiceStatus.Refunded)
                {
                    var tempValue = Invoice.TotalTender.ToPositive() - Invoice.NetAmount.ToPositive();

                    if (tempValue.ToPositive() <= 0.05m)
                    {
                        Invoice.NetAmount = Invoice.TotalTender;

                    }
                }

                if (Invoice.TotalTender.ToPositive() >= Invoice.NetAmount.ToPositive())
                {
                    var tempNetAmount = Math.Round(Invoice.NetAmount.ToPositive(), Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);

                    Invoice.ChangeAmount = Invoice.TotalTender.ToPositive() - tempNetAmount;
                    iscomplete = true;
                }
                else
                {
                    var tempAmount = Invoice.NetAmount.ToPositive() - Invoice.TotalTender.ToPositive();

                    if (tempAmount < 0.005m)
                    {
                        Invoice.NetAmount = Invoice.TotalTender.ToPositive();
                        Invoice.ChangeAmount = Invoice.TotalTender.ToPositive() - Invoice.NetAmount.ToPositive();
                        iscomplete = true;

                    }

                    if (Invoice.Status == InvoiceStatus.Refunded)
                    {
                        if (Invoice.NetAmount >= Invoice.TotalTender)
                        {
                            Invoice.ChangeAmount = Invoice.NetAmount.ToPositive() - Invoice.TotalTender.ToPositive();
                            Invoice.RoundingAmount = Invoice.RoundingAmount.ToPositive();
                            iscomplete = true;
                        }
                    }
                }

                Invoice.TotalPaid = Invoice.TotalTender - Invoice.ChangeAmount;

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
                    if (Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy || Invoice.Status == InvoiceStatus.BackOrder || Invoice.Status == InvoiceStatus.OnAccount)
                    {
                        if (Settings.StoreGeneralRule.ParkingPaidOrder || Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy)
                        {
                            bool result = false;
                            if (SelectedPaymentOption.PaymentOptionType != PaymentOptionType.VantivIpad)
                            {
                                result = Invoice?.InvoiceFloorTable != null ? true : await App.Alert.ShowAlert("Confirmation", LanguageExtension.Localize("HasOrderFulfilled"), "Yes", "No");//#94565
                            }
                            else
                            {
                                result = true;
                            }

                            if (result)
                            {
                                Invoice.Status = InvoiceStatus.Completed;
                                Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
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
                            Invoice.Status = InvoiceStatus.Completed;
                        }
                        return await sale_completed();
                    }
                }
                else
                {
                    await sale_uploadedTolocaldb();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        async Task<bool> sale_completed()
        {
            try
            {
                if (!(Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange))
                {
                    if (Settings.StoreGeneralRule.ParkingPaidOrder || Invoice.Status == InvoiceStatus.Parked)
                        Invoice.Status = InvoiceStatus.Parked;
                    else
                    {
                        Invoice.Status = InvoiceStatus.Completed;
                    }
                }
                Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        async Task<InvoiceDto> sale_uploadedTolocaldb()
        {
            if (string.IsNullOrEmpty(Invoice.Number))
            {
                var Register = Settings.CurrentRegister;
                Invoice.Number = Register.Prefix + (Register.ReceiptNumber + 1) + Register.Suffix;
                Invoice.Barcode = Settings.CurrentRegister.Id.ToString() + Invoice.Number;
                Invoice.TransactionDate = Extensions.moment();
                Invoice.FinalizeDate = Invoice.TransactionDate;
                outletService.UpdateLocalRegisterReceiptNumber(Settings.CurrentRegister.Id, Settings.SelectedOutletId);
            }

            if (EnterSalePage.ServedBy == null)
            {
                EnterSalePage.ServedBy = Settings.CurrentUser;
            }

            Invoice.ServedBy = EnterSalePage.ServedBy.Id;
            Invoice.ServedByName = EnterSalePage.ServedBy.FullName;

            if (Invoice.CustomerId == null || (Invoice.CustomerId == 0 && string.IsNullOrEmpty(Invoice.CustomerTempId)) || Invoice.CustomerName == "Select customer")
            {
                Invoice.CustomerId = null;
                Invoice.CustomerName = "Walk in";
                Invoice.CustomerTempId = "";
                Invoice.CustomerDetail = new CustomerDto_POS();
            }
            return await saleService.UpdateLocalInvoice(Invoice);
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
                else if (paymentType.PaymentOptionType == Enums.PaymentOptionType.Afterpay)
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
                if (Invoice.TenderAmount.ToPositive() >= outstanding)
                {
                    payment.Amount = Invoice.NetAmount - Invoice.TotalPaid;
                }
                else
                {
                    payment.Amount = Invoice.TenderAmount;
                }

                payment.TenderedAmount = Invoice.TenderAmount;
                payment.IsPaid = true;
                payment.RegisterId = Settings.CurrentRegister.Id;
                payment.RegisterName = Settings.CurrentRegister.Name;
                payment.OutletId = Settings.SelectedOutletId;
                payment.OutletName = Settings.SelectedOutletName;
                payment.ServedBy = Settings.CurrentUser.FullName;

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
                    || payment.PaymentOptionType == PaymentOptionType.TD
                    || Extensions.IseConduitPayment(payment.PaymentOptionType))
                {
                    payment.InvoicePaymentDetails = InvoicePaymentDetails;
                }
                if (Invoice.InvoicePayments == null)
                {
                    Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                }

                if (Invoice.InvoicePayments == null)
                {
                    Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                }

                Invoice.InvoicePayments.Add(payment);
                var Result = await calculateInvoicePayment(payment.PaymentOptionType);

                Invoice.TenderAmount = Invoice.NetAmount - Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);
                return Result;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public async Task<bool> addPaymentToSell(PaymentOptionDto paymentType, string giftcardNumber)
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
                // 8148, 7582 Loyalty issue
                //Start Ticket #75526 Myob rounding issue By Pratik
                Invoice.NetAmount = Math.Round(Invoice.NetAmount, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                //End Ticket #75526 By Pratik
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
                    //Ticket end:#35310 .by rupesh

                }
                payment.Amount = amount + payment.RoundingAmount;
                payment.TenderedAmount = Invoice.TenderAmount; //payment.roundingAmount;

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
                payment.PaymentFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;

                if (Invoice.Status == InvoiceStatus.Refunded)
                    payment.ActionType = ActionType.Refund;
                else
                    payment.ActionType = ActionType.Sell;

                payment.PaymentDate = Extensions.moment();
                payment.PaymentFrom = InvoiceFrom.iPad;
                payment.InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();
                if (payment.PaymentOptionType == PaymentOptionType.GiftCard)
                {
                    payment.InvoicePaymentDetails.Add(
                        new InvoicePaymentDetailDto
                        {
                            Key = InvoicePaymentKey.GiftCardNumber,
                            Value = giftcardNumber
                        });
                }
                if (Invoice.InvoicePayments == null)
                {
                    Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                }
                Invoice.InvoicePayments.Add(payment);

                foreach (var item in Invoice.InvoicePayments)
                {
                    if (item.BackorderPayment > 0)
                        Invoice.BackorderPayments.Add(item);
                }

                var Result = await calculateInvoicePayment(payment.PaymentOptionType);

                var temps = Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);

                Invoice.TenderAmount = Invoice.NetAmount - Invoice.InvoicePayments.Where(x => !x.IsDeleted).Sum(x => x.Amount);

                if (Convert.ToDouble(Invoice.TenderAmount) < 0.005 && Convert.ToDouble(Invoice.TenderAmount) > 0.00)
                {
                    Invoice.TenderAmount = 0.00m;

                }

                return Result;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }

        }

        InvoicePaymentKey GetInvoicePaymentKey(PaymentOptionType paymentOptionType)
        {
            if (paymentOptionType == PaymentOptionType.NorthAmericanBankcard)
                return InvoicePaymentKey.NorthAmericanBankcard;
            else if (paymentOptionType == PaymentOptionType.PayJunction)
                return InvoicePaymentKey.PayJunction;
            else if (paymentOptionType == PaymentOptionType.EVOPayment)
                return InvoicePaymentKey.EVOPayment;
            else if (paymentOptionType == PaymentOptionType.VerifonePaymark)
                return InvoicePaymentKey.VerifonePaymark;
            else return InvoicePaymentKey.eConduit;
        }

        string EConduitReceiptData(EconduitContentResponse econduitContent)
        {
            string receipt = string.Empty;
            try
            {
                receipt = " " + Settings.CurrentUser.FullName
                                     + "\n " + Invoice.OutletName
                                     + "\n \n " + econduitContent.TransactionDate.ToString("dd-MM-yyyy h:mm")
                                     + "\n \n " + econduitContent.TransType.ToUpper()
                                     + "\n \n " + econduitContent.CardType.ToUpper()
                                     + "\n " + "ACCT: " + "               **********" + econduitContent.Last4
                                     + "\n " + "EXP: " + "                 ****"
                                     + "\n " + "ENTRY: " + "             " + econduitContent.EntryMethod.ToUpper()
                                     + "\n " + "APPROVAL: " + "      " + econduitContent.AuthCode
                                     + "\n " + "TOTAL: " + "              " + Settings.StoreCurrencySymbol.ToString() + string.Format("{0:0.00}", econduitContent.Amount)
                                     + "\n \n " + "I agree to pay above total amount"
                                     + "\n " + "according to card issuer agreement."
                                     + "\n \n " + "X____________________________"
                                     + "\n" + "        " + "SIGNATURE"
                                     + "\n \n" + "        " + econduitContent.ResultCode.ToUpper()
                                     + "\n \n" + "        " + "CUSTOMER COPY";
            }
            catch (Exception e)
            {
                e.Track();
                Debug.WriteLine("Exception in EConduitReceiptData : " + e.Message);
            }
            return receipt;
        }

        void RefundAndVoidTransactionComplete(PaymentOptionDto paymentOption)
        {
            try
            {
                var InvoicePayments = new ObservableRangeCollection<InvoicePaymentDto>();
                var RefundPayment = Invoice.InvoicePayments.FirstOrDefault(x => x.PaymentOptionType == paymentOption.PaymentOptionType)?.Copy();
                if (RefundPayment == null)
                    RefundPayment = new InvoicePaymentDto();
                RefundPayment.Id = 0;
                //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
                RefundPayment.Amount = CashPaymentRoundingAmount().ToPositive() * -1;
                RefundPayment.RoundingAmount = NetAmountRoundingAmount(TenderAmount);
                RefundPayment.TenderedAmount = TenderAmount;
                //Ticket End:#91262 by rupesh
                RefundPayment.PaymentDate = Extensions.moment();
                RefundPayment.PaymentOptionId = paymentOption.Id;
                RefundPayment.PaymentOptionName = paymentOption.Name;
                RefundPayment.PrintPaymentOptionName = paymentOption.Name;
                RefundPayment.PaymentOptionType = paymentOption.PaymentOptionType;
                RefundPayment.InvoicePaymentDetails = RedundAndVoidInvoicePaymentDetails;

                //Start #81875 #83886  Discrepancy in payment received and net sales. By Pratik
                if (Settings.CurrentRegister?.Registerclosure != null)
                {
                    RefundPayment.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id;
                    RefundPayment.RegisterId = Settings.CurrentRegister.Id;
                    RefundPayment.RegisterName = Settings.CurrentRegister.Name;
                }
                //End #81875 #83886 By Pratik

                RefundPayment.ActionType = ActionType.Refund;
                InvoicePayments.Add(RefundPayment);
                Invoice.InvoiceRefundPayments = new ObservableCollection<InvoicePaymentDto>(Invoice.InvoicePayments.Where(x => !x.IsDeleted));

                foreach (var item in InvoicePayments)
                {
                    Invoice.InvoicePayments.Add(item);
                }
                Invoice.TotalPaid = Invoice.TotalPaid - CashPaymentRoundingAmount().ToPositive();  //Ticket start:#91262 by rupesh
                Invoice.TotalTender = Invoice.TotalPaid;
                if (Invoice.TotalPaid == 0)
                {
                    if (Invoice.Status == InvoiceStatus.BackOrder)
                    {
                        Invoice.Status = InvoiceStatus.RefundedAndDiscardBO;
                    }
                    else
                    {
                        Invoice.Status = InvoiceStatus.RefundedAndDiscard;
                    }

                }
                else
                {
                    Invoice.Status = Status;
                }
                Invoice.isSync = false;
                Invoice.IsCustomerChange = false;

                TenderAmount = (-1) * Invoice.TotalPaid;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void ClosePage()
        {
            IsPartiallyPaid = true;
            if (Invoice.TotalPaid == 0)
                _ = close();
        }
        #endregion


        void SendLogsToServer(PaymentOptionDto paymentOption, Dictionary<string, string> requestMap, object paymentResponse = null)
        {
            Extensions.SendLogsToServer(logService, paymentOption, requestMap, "", paymentResponse);
        }

        private async void closeHandle_Clicked()
        {
            await close();
        }

        public async Task close()
        {
            try
            {
                var result = true;
                if (TenderAmount != 0 && IsPartiallyPaid)
                    result = await App.Alert.ShowAlert("Are you sure?", "This sale is partially refunded?", "Yes", "Cancel");
                if (result)
                {
                    if (_navigationService.RootPage.Navigation.ModalStack != null && _navigationService.RootPage.Navigation.ModalStack.Count > 0)
                        await _navigationService.RootPage.Navigation.PopModalAsync();

                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private void selectPaymentHandle_Clicked(PaymentOptionDto paymentOption)
        {
            if (paymentOption != null)
            {
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return;
                }
                selectPaymnet(paymentOption);
            }
        }

        private async void selectPaymnet(PaymentOptionDto paymentOption)
        {
            Invoice.TenderAmount = (-1) * TenderAmount.ToPositive();
            Status = Invoice.Status;
            Invoice.Status = InvoiceStatus.RefundedAndDiscard;
            var result = await selectPaymentOption(paymentOption);

            if (result)
            {
                while (_navigationService.RootPage.Navigation.ModalStack != null && _navigationService.RootPage.Navigation.ModalStack.Count > 0)
                    await _navigationService.RootPage.Navigation.PopModalAsync();

                InvoiceRefund?.Invoke(this, paymentOption);
            }

        }

        //Ticket start:#91262 iOS:FR Round-off totals to nearest  Not Come true.by rupesh
        private decimal CashPaymentRoundingAmount()
        {
            var amount = TenderAmount;
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

        private decimal NetAmountRoundingAmount(decimal amount)
        {
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
    }

    public static class AssemblyOrEconduitReceiptData
    {
        public static List<string> AssemblyReceipt = null;
        public static string InvoiceID = null;
    }
}

