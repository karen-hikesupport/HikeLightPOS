using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Pages;
using HikePOS.Services;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
    public class InvoiceViewModel : BaseViewModel
    {

        PeerCommunicatorViewModel peerCommunicatorViewModel;

        #region Declare services 
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        ProductServices productService;
        CustomerServices customerService;
        OutletServices outletService;
        SaleServices saleService;

        ApiService<ITaxApi> taxApiService = new ApiService<ITaxApi>();
        TaxServices taxServices;

         //#94565
        ApiService<IRestaurantApi> RestaurantApiService = new ApiService<IRestaurantApi>();
        RestaurantService RestaurantService;
        //#94565

        #endregion

        #region Customer properties
        CustomerViewModel _CustomerModel { get; set; }
        public CustomerViewModel CustomerModel { get { return _CustomerModel; } set { _CustomerModel = value; SetPropertyChanged(nameof(CustomerModel)); } }
        #endregion

        #region register properties
        RegisterDto _register { get; set; } = Settings.CurrentRegister;
        public RegisterDto Register { get { return _register; } set { _register = value; SetPropertyChanged(nameof(Register)); } }
        #endregion

        #region Invoice properties

        public decimal ZeroPrice => 0;
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
                SendPeerNotification(Invoice);
                SetPropertyChanged(nameof(IsRestaurantPOS));
                IsOrdered = Invoice?.InvoiceLineItems == null ? false : Invoice.InvoiceLineItems.Any(); //#94565
            }
        }

        ObservableCollection<InvoiceLineItemDto> _invoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
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
        #endregion

        #region Offers properties
        ObservableCollection<OfferDto> _offers { get; set; }
        public ObservableCollection<OfferDto> offers
        {
            get
            {
                return _offers;
            }
            set
            {
                _offers = value;
                SetPropertyChanged(nameof(offers));
            }
        }
        #endregion

        #region Discount and tip properties
        int _IsOpenDiscountPopUp { get; set; } = 0;
        public int IsOpenDiscountPopUp
        {
            get { return _IsOpenDiscountPopUp; }
            set
            {
                _IsOpenDiscountPopUp = value;
                SetPropertyChanged(nameof(IsOpenDiscountPopUp));
                if (_navigationService?.NavigatedPage != null && _navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel> baseContentPage)
                {
                    if (baseContentPage?.ViewModel != null)
                        baseContentPage.ViewModel.IsOpenBackground = value;
                }
            }
        }

        string _amountValue { get; set; }
        public string AmountValue
        {
            get
            {
                return _amountValue;
            }
            set { _amountValue = value; SetPropertyChanged(nameof(AmountValue)); }
        }


        string _discountType { get; set; } = "Percentage";
        public string DiscountType { get { return _discountType; } set { _discountType = value; SetPropertyChanged(nameof(DiscountType)); } }


        public string CurrencySymbol { get; set; } = Settings.StoreCurrencySymbol;



        #endregion

        int _IsOpenTaxViewPopUp { get; set; } = 0;
        public int IsOpenTaxViewPopUp
        {
            get { return _IsOpenTaxViewPopUp; }
            set
            {
                _IsOpenTaxViewPopUp = value;
                SetPropertyChanged(nameof(IsOpenTaxViewPopUp));
                if (_navigationService?.NavigatedPage != null && _navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel> baseContentPage)
                {
                    if (baseContentPage?.ViewModel != null)
                        baseContentPage.ViewModel.IsOpenBackground = value;
                }
            }
        }

        public ICommand RemoveInvoiceTaxItemCommand { get; }

        public ICommand RemoveInvoiceItemCommand { get; }

        #region DiscountTipCommands
        public ICommand DeleteDiscountCommand { get; }
        public ICommand ApplyDiscountCommand { get; }

        public ICommand CloseDiscountTipCommand { get; }
        public ICommand ChangeDiscountTypeCommand { get; }
        #endregion

        int _IsOpenOptionPopUp { get; set; } = 0;
        public int IsOpenOptionPopUp
        {
            get { return _IsOpenOptionPopUp; }
            set
            {
                _IsOpenOptionPopUp = value;
                SetPropertyChanged(nameof(IsOpenOptionPopUp));
                if (_navigationService?.NavigatedPage != null && _navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel> baseContentPage)
                {
                    if (baseContentPage?.ViewModel != null)
                        baseContentPage.ViewModel.IsOpenBackground = value;
                }
            }
        }

        //Start Ticket #73185 iPad - Feature: Parked Option, By:Pratik
        bool _isParkSale;
        public bool IsParkSale { get { return _isParkSale; } set { _isParkSale = value; SetPropertyChanged(nameof(IsParkSale)); } }
        //End Ticket #73185, By:Pratik

        public ICommand ToolMenuCommand { get; }

        Color _GiftcardButtonTextColor { get; set; }

        public Color GiftcardButtonTextColor { get { return _GiftcardButtonTextColor; } set { _GiftcardButtonTextColor = value; SetPropertyChanged(nameof(GiftcardButtonTextColor)); } }


        bool _permissionDoCustomSale { get; set; }
        public bool PermissionDoCustomSale { get { return _permissionDoCustomSale; } set { _permissionDoCustomSale = value; SetPropertyChanged(nameof(PermissionDoCustomSale)); } }

        bool _permissionGiftCardSale { get; set; }
        public bool PermissionGiftCardSale { get { return _permissionGiftCardSale; } set { _permissionGiftCardSale = value; SetPropertyChanged(nameof(PermissionGiftCardSale)); } }

        bool _discardVisible { get; set; } = true;
        public bool DiscardVisible { get { return _discardVisible; } set { _discardVisible = value; SetPropertyChanged(nameof(DiscardVisible)); } }

        bool _discardAndVoidVisible { get; set; }
        public bool DiscardAndVoidVisible { get { return _discardAndVoidVisible; } set { _discardAndVoidVisible = value; SetPropertyChanged(nameof(DiscardAndVoidVisible)); } }

        bool _parkedVisible { get; set; }
        public bool ParkedVisible { get { return _parkedVisible; } set { _parkedVisible = value; SetPropertyChanged(nameof(ParkedVisible)); } }

        //Ticket start:#43972 OPEN CASH OPTION CAN ABLE TO DISABLE.by rupesh
        bool _permissionOpenCashDrawer { get; set; }
        public bool PermissionOpenCashDrawer { get { return _permissionOpenCashDrawer; } set { _permissionOpenCashDrawer = value; SetPropertyChanged(nameof(PermissionOpenCashDrawer)); } }
        //Ticket end:#43972 .by rupesh

        #region  Restaurant POS properties
         //#94565
        CanvanceTableLayout _table;
        public CanvanceTableLayout Table
        {
            get { return _table; }
            set
            {
                _table = value;
                SetPropertyChanged(nameof(Table));
                SetPropertyChanged(nameof(IsTableSelect));
                IsRelease = !IsOrdered && Table != null && Settings.IsRestaurantPOS;
                IsParkSale = Table == null && Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.Park");
            }
        }

        public bool IsTableSelect => !string.IsNullOrEmpty(Table?.Name);

        public bool IsRestaurantPOS => Settings.IsRestaurantPOS && (Invoice == null || (Invoice != null && (Invoice.Status == InvoiceStatus.OnGoing || Invoice.Status == InvoiceStatus.initial || Invoice.Status == InvoiceStatus.Pending)));

        public bool IsTabelVisible => Settings.IsRestaurantPOS;

        bool _isRelease;
        public bool IsRelease
        {
            get { return _isRelease; }
            set
            {
                _isRelease = value;
                SetPropertyChanged(nameof(IsRelease));
            }
        }

        bool _isOrdered;
        public bool IsOrdered
        {
            get { return _isOrdered; }
            set
            {
                _isOrdered = value;
                SetPropertyChanged(nameof(IsOrdered));
                SetPropertyChanged(nameof(IsRestaurantPOS));
                IsRelease = !IsOrdered && Table != null && Settings.IsRestaurantPOS;
            }
        }
        //#94565
        #endregion

        VariantProductPage variantpage;
        AllUserPage allUserPage;
        PromptPopupPage promptPopupPage;
        RefundAndVoidPage refundAndVoidPage;
        private List<InvoiceLineItemDto> lineItemCount;
        //Ticket start #65869:iOS: User level discount limit permission not working. by rupesh
        private ApproveAdminPage ApproveAdminPage;
        private bool SkipAdminAppoval = false;
        //Ticket end #65869. by rupesh

        //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
        private AgeVerificationEventArgs AgeVerificationProofData = null;
        //Ticket:end:#90938,#94423 .by rupesh
        public InvoiceViewModel(ProductServices _productService, CustomerServices _customerService, OutletServices _outletService, SaleServices _saleService)
        {
            //peerCommunicatorViewModel = new PeerCommunicatorViewModel();

            // peerCommunicatorViewModel.HikePeer_MessageReceived(null);

            #region Create service instances
            productService = _productService;
            customerService = _customerService;
            outletService = _outletService;
            saleService = _saleService;
            taxServices = new TaxServices(taxApiService);
            RestaurantService = new RestaurantService(RestaurantApiService);
            GiftcardButtonTextColor = Colors.Black;
            #endregion
            CustomerModel = new CustomerViewModel(customerService, saleService);
            //Ticket start:#34531 iPad: delivery address popup is still showing after the delivery address slider is open.by rupesh
            bool isPopNotDisplayed = true;
            CustomerModel.CustomerChanged += async (object sender, CustomerDto_POS e) =>
            {

                // await Task.Run(async () =>
                // {
                // await MainThread.InvokeOnMainThreadAsync(async ()=> { 

                if (Invoice?.InvoiceLineItems != null && Invoice.InvoiceLineItems.Count > 4)
                    IsLoad = true;
                ((CustomerViewModel)sender).IsOpenSearchCustomerPopUp = 0;
               
                bool isCustomerOnSelect = true;

                if (Invoice != null && e != null)
                {
                    if(Invoice.CustomerId == null && e.Id == 0)
                        isCustomerOnSelect = false;
                    Invoice.CustomerId = e.Id;
                    Invoice.CustomerTempId = e.TempId;
                    Invoice.CustomerName = (string.IsNullOrEmpty(e.FirstName) ? "" : (e.FirstName.ToUppercaseFirstCharacter() + " ")) + (string.IsNullOrEmpty(e.LastName) ? "" : e.LastName.ToUppercaseFirstCharacter());
                }
                
                if(isCustomerOnSelect)
                {
                    await Task.Delay(1);
                    await customerOnSelect(e);
                    await Task.Delay(1);
                }
                if (IsLoad)
                    IsLoad = false;
                //});
                //});
                //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
                //Start #77336 POS ipad cash register crashing regularly By Pratik
                if (e != null && !(e.Id == 0 && string.IsNullOrEmpty(e.TempId)) &&
                        Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer && !Settings.IsQuoteSale && isPopNotDisplayed)
                //END #77336 By Pratik
                {
                    isPopNotDisplayed = false;
                    //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
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
                    //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var result = await App.Alert.ShowAlert(LanguageExtension.Localize("DeliveryAddressAlertTitle"), LanguageExtension.Localize("DeliveryAddressAlertMessage"), LanguageExtension.Localize("YesButtonText"), LanguageExtension.Localize("CancelButtonText"));
                        isPopNotDisplayed = true;
                        if (result)
                        {
                            CustomerModel.DeliveryCustomerCommand.Execute(Invoice.CustomerDetail);
                        }
                    });

                }
                //Ticket end:#34531 .by rupesh
                //Ticket end:#26664 .by rupesh
            };

            RemoveInvoiceTaxItemCommand = new Command<LineItemTaxDto>(RemoveInvoiceTaxItem);

            RemoveInvoiceItemCommand = new Command<InvoiceLineItemDto>(removeSellItem);
            DeleteDiscountCommand = new Command(DeleteDiscount);
            ApplyDiscountCommand = new Command(ApplyDiscount);
            ChangeDiscountTypeCommand = new Command<string>(ChangeDiscountType);
            CloseDiscountTipCommand = new Command(CloseDiscount);
            ToolMenuCommand = new Command<string>(async (obj) =>
            {
                await ToolMenu(obj);

            });

            //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
            CustomerModel.DeliveryAddressChanged += (object sender, CustomerAddressDto e) =>
            {
                DeliveryAddressOnSelect(e);
            };
            //Ticket end:#26664 .by rupesh


        }

        private void GetTaxList()
        {
            if (TaxList == null || (TaxList != null && TaxList.Count <= 0))
                TaxList = taxServices.GetLocalTaxes();

            if (TaxList != null)
            {
                if (IsTaxListDisplay)
                {
                    if (Invoice != null && Invoice.CustomerDetail != null && Invoice.CustomerDetail.ToAllowForTaxExempt)
                    {
                        SelectedShippingTax = TaxList.FirstOrDefault(x => x.Id == 1);//1 is fixed id for NoTax
                    }
                    else
                    {
                        var Register = Settings.CurrentRegister;
                        if (Register != null && Register.DefaultTax != null)
                        {
                            //Ticket start:#35076 iPad: surcharge and shipping tax are changed.by rupesh
                            if (SelectedShippingTax == null)
                                SelectedShippingTax = TaxList.FirstOrDefault(x => x.Id == Register.DefaultTax);
                            //Ticket end:#35076 .by rupesh
                        }
                        else
                        {
                            SelectedShippingTax = TaxList.FirstOrDefault(x => x.Id == 1);
                        }
                    }
                }
                else
                {

                    if (Invoice != null && Invoice.CustomerDetail != null && Invoice.CustomerDetail.ToAllowForTaxExempt)
                    {
                        SelectedTax = TaxList.FirstOrDefault(x => x.Id == 1);//1 is fixed id for NoTax
                    }
                    else
                    {
                        var Register = Settings.CurrentRegister;
                        if (Register != null && Register.DefaultTax != null)
                        {
                            //Ticket start:#35076 iPad: surcharge and shipping tax are changed.by rupesh
                            if (SelectedTax == null)
                                SelectedTax = TaxList.FirstOrDefault(x => x.Id == Register.DefaultTax);
                            //Ticket end:#35076 .by rupesh
                        }
                        else
                        {
                            SelectedTax = TaxList.FirstOrDefault(x => x.Id == 1);
                        }
                    }

                }
            }
        }

        public void OpenOption()
        {
            if (IsOpenOptionPopUp == 1)
            {
                IsOpenOptionPopUp = 0;
            }
            else
            {
                IsOpenOptionPopUp = 1;
            }
        }

        public async Task ToolMenu(string option)
        {
            try
            {

                if (EventCallRunning)
                {
                    return;
                }

                EventCallRunning = true;
                bool IsOpenRegister = false;
                if (Settings.CurrentRegister != null)
                {
                    if (Settings.CurrentRegister.Registerclosure != null && Settings.CurrentRegister.Registerclosure.StartDateTime != null && Settings.CurrentRegister.Registerclosure.EndDateTime == null)
                    {
                        IsOpenRegister = true;
                    }
                    else
                    {
                        IsOpenRegister = false;
                    }
                }

                switch (option)
                {
                    case "AddGiftCard":

                        //Ticket start:#22406 Quote sale.by rupesh .
                        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                        if (Settings.IsQuoteSale || Settings.IsBackorderSaleSelected)
                        {
                            return;
                        }
                        //Ticket end:#92764 .by rupesh
                        //Ticket end:#22406 .by rupesh .

                        if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RegisterOpenAlertMessage"));
                            return;
                        }

                        if (Invoice == null)
                        {
                            await addNewSale();
                        }

                        if (Invoice.Status == InvoiceStatus.Refunded)
                        {
                            return;
                        }

                        var addgiftcardpage = new AddGiftCardPage();
                        addgiftcardpage.ViewModel.CustomerDetail = Invoice.CustomerDetail;
                        addgiftcardpage.ViewModel.GiftCardAdded += (object sender, GiftCardDto e) =>
                        {
                            //Ticket start:#93377,#93386 iOS: Add validation for giftcard.by rupesh
                            if (Invoice?.InvoiceLineItems?.Any(x => x.InvoiceItemType == InvoiceItemType.GiftCard && x.GiftCardNumber == e.Number) == true)
                            {
                                App.Instance.Hud.DisplayToast("A gift card with same number already added to the cart.", Colors.Red, Colors.White);
                                return;
                            }
                            //Ticket end:#93377,#93386 .by rupesh
                            addgiftCard(e);
                            addgiftcardpage.ViewModel.ClosePopupTapped();
                        };
                        if (_navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel> enterSalePage)
                            enterSalePage.ViewModel.IsOpenPopup = true;
                        await NavigationService.PushModalAsync(addgiftcardpage);
                        break;
                    case "CustomSale":
                        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                        if (Settings.IsBackorderSaleSelected)
                        {
                            return;
                        }
                        //Ticket end:#92764 .by rupesh
                        if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RegisterOpenAlertMessage"));
                            return;
                        }

                        if (Invoice == null)
                        {
                            await addNewSale();
                        }

                        using (new Busy(this, true))
                        {
                            if (Invoice.Status == InvoiceStatus.Refunded)
                            {
                                return;
                            }
                            var customsalepage = new CustomSalePage();
                            customsalepage.ViewModel.Invoice = Invoice;
                            customsalepage.ViewModel.CustomSale = new InvoiceLineItemDto();
                            customsalepage.ViewModel.CustomsaleAdded += (object sender, InvoiceLineItemDto invoiceItem) =>
                            {
                                if (invoiceItem != null)
                                {
                                    invoiceItem.InvoiceItemType = InvoiceItemType.Custom;
                                    invoiceItem.InvoiceItemValue = 0;
                                    invoiceItem.Sequence = Invoice.InvoiceLineItems.Count + 1;
                                    invoiceItem.ItemCost = invoiceItem.ItemCost;
                                    invoiceItem.SoldPrice = invoiceItem.RetailPrice;
                                    invoiceItem.DiscountIsAsPercentage = true;
                                    //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                                            Invoice.InvoiceLineItems.Add(invoiceItem);
                                        else
                                            Invoice.InvoiceLineItems.Insert(0, invoiceItem);
                                    });
                                    //End:#45375 .by rupesh
                                    //Ticket start:#74632 iOS: Add Flat Markup% to Customer Group Pricing (FR).by rupesh
                                    invoiceItem = InvoiceCalculations.CustomerMarkupDiscount(Invoice, invoiceItem, Invoice.CustomerDetail);
                                    //Ticket end:#74632.by rupesh
                                    invoiceItem = InvoiceCalculations.CalculateLineItemTotal(invoiceItem, Invoice);
                                    Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                                    InvoiceLineItems = Invoice.InvoiceLineItems;
                                }
                                customsalepage.ViewModel.ClosePopupTapped();
                            };

                            //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil 
                            customsalepage.ViewModel.CanChangeTax = !Invoice.CustomerDetail?.ToAllowForTaxExempt ?? false;
                            //Ticket #10921 End. By Nikhil
                            if (_navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel> enterSalePage1)
                                enterSalePage1.ViewModel.IsOpenPopup = true;
                            await NavigationService.PushModalAsync(customsalepage);
                        }
                        break;
                    case "Voided":
                        if (IsOpenRegister && Invoice != null && Invoice.Status != InvoiceStatus.Pending)
                        {
                            using (new Busy(this, true))
                            {
                                Invoice.Status = InvoiceStatus.Voided;
                                Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                                //addNewSale();
                                Invoice = null;
                                InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                            }
                        }
                        break;
                    case "Parked":
                        //#36024 iPad: without serial number, the sale should not be park.
                        bool isValidSerialNo = CheckForSerialNo();
                        if (!isValidSerialNo)
                        {
                            return;
                        }
                        //#36024 iPad: without serial number, the sale should not be park.

                        //Ticket start:#22406 Quote sale.by rupesh .
                        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                        if (Settings.IsQuoteSale || Settings.IsBackorderSaleSelected)
                        {
                            return;
                        }
                        //Ticket end:#92764.by rupesh
                        //Ticket end:#22406 .by rupesh .

                        if (Invoice.Status == InvoiceStatus.initial)
                            Invoice.Status = InvoiceStatus.Pending;


                        if (IsOpenRegister && Invoice != null && Invoice.InvoiceLineItems != null
                            && Invoice.InvoiceLineItems.Count > 0
                            && (Invoice.Status == InvoiceStatus.Pending
                            || Invoice.Status == InvoiceStatus.Parked))
                        {

                            if (InvoiceCalculations.CheckHasBackOrder(Invoice) && (Invoice.CustomerId == null || Invoice.CustomerId == 0))
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

                            if (promptPopupPage == null)
                            {
                                //Start ticket #60456 Pratik
                                promptPopupPage = new PromptPopupPage()
                                {
                                    Title = LanguageExtension.Localize("ParkedAlertTitle"),
                                    Description = LanguageExtension.Localize("ParkedAlertBody"),
                                    Placeholder = LanguageExtension.Localize("AddNotePlaceholder"),
                                    YesButtonText = LanguageExtension.Localize("SaveButtonText"),
                                    NoButtonText = LanguageExtension.Localize("CancelButtonText"),
                                    SaveAndPrintButtonText = LanguageExtension.Localize("SaveAndPrintButtonText"),
                                };

                                promptPopupPage.SavedAndPrint += async (object sender, string e) =>
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(e))
                                        {
                                            Invoice.Note = e;
                                        }

                                        using (new Busy(this, true))
                                        {
                                            Invoice.Status = InvoiceStatus.Parked;
                                            //Ticket start:#73669 iOS : QR not printed for Park sale.by rupesh
                                            var currentReceiptTemplate = Settings.CurrentRegister.ParkOrderReceiptTemplate ?? Settings.CurrentRegister.ReceiptTemplate;
                                            if (currentReceiptTemplate.ShowQRCode)
                                            {
                                                System.Globalization.NumberFormatInfo numberFormatWithComma = new System.Globalization.NumberFormatInfo();
                                                numberFormatWithComma.NumberDecimalSeparator = ".";
                                                numberFormatWithComma.NumberDecimalDigits = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalDigits;
                                                var qrCode = Settings.StoreName + " " + currentReceiptTemplate.VATNumber + " " + Invoice.TransactionDate.ToString("o") + " " + Invoice.NetAmount.ToString("N", numberFormatWithComma) + " " + Invoice.TotalTax.Value.ToString("N", numberFormatWithComma);
                                                var encodedQrCode = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(qrCode));
                                                Invoice.QRString = encodedQrCode;
                                            }
                                            //Ticket start:#73669 .by rupesh
                                            Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);

                                            if (Invoice.ReferenceInvoiceId != 0 && Invoice.Status == InvoiceStatus.Parked && Invoice.InvoiceHistories?.LastOrDefault(a => a.Status != InvoiceStatus.EmailSent)?.Status == InvoiceStatus.Quote)
                                            {
                                                var UpdatedInvoice = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                                                UpdatedInvoice.FinancialStatus = FinancialStatus.Closed;
                                                UpdatedInvoice.ReferenceNote = "Converted sale " + Invoice.Number;
                                                await saleService.UpdateLocalInvoice(UpdatedInvoice);
                                            }

                                            var vm = ((BaseContentPage<EnterSaleViewModel>)_navigationService.NavigatedPage).ViewModel;
                                            vm.PrepareInvoiceRecipt();
                                            var paymentpage = vm.ParkPayment();
                                            if(!Settings.IsTextPrint)
                                            {
                                                vm.InvocePrintReceiptSummaryView.BindingContext = paymentpage.ViewModel;
                                                await paymentpage.ViewModel.PrintInvoice(false, vm.InvocePrintReceiptSummaryView, null, null, null);
                                            }
                                            else
                                                await paymentpage.ViewModel.TextPrintInvoice(false,"invoice",false,false,false);

                                            // this.promptPopupPage.
                                            Invoice = null;
                                            InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                                            // if(!Settings.IsTextPrint)
                                            // {
                                            //     if (_navigationService.NavigatedPage is EnterSalePagePhone)
                                            //         ((EnterSalePagePhone)_navigationService.NavigatedPage)._PrintReceiptSummaryView.Content = null;
                                            //     else
                                            //         ((EnterSalePage)_navigationService.NavigatedPage)._PrintReceiptSummaryView.Content = null;
                                            // }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.Track();
                                    }
                                };
                                //End ticket #60456 Pratik
                                promptPopupPage.Saved += async (object sender, string e) =>
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(e))
                                        {
                                            Invoice.Note = e;
                                        }

                                        using (new Busy(this, true))
                                        {
                                            Invoice.Status = InvoiceStatus.Parked;
                                            Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                                            if (Invoice.ReferenceInvoiceId != 0 && Invoice.Status == InvoiceStatus.Parked && Invoice.InvoiceHistories?.LastOrDefault(a => a.Status != InvoiceStatus.EmailSent)?.Status == InvoiceStatus.Quote)
                                            {
                                                var UpdatedInvoice = saleService.GetLocalInvoice(Invoice.ReferenceInvoiceId.Value);
                                                UpdatedInvoice.FinancialStatus = FinancialStatus.Closed;
                                                UpdatedInvoice.ReferenceNote = "Converted sale " + Invoice.Number;
                                                await saleService.UpdateLocalInvoice(UpdatedInvoice);

                                            }

                                            Invoice = null;
                                            InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.Track();
                                    }
                                };
                            }
                            //promptPopupPage.Value = Invoice.Note;
                            await NavigationService.PushModalAsync(promptPopupPage);
                        }

                        break;
                    case "Discard":
                        //#94565
                        var result = false;
                        //Ticket start:#25881 Hike Store : Unable to discard sale # 000119 on iPad. by rupesh.
                        if (IsOpenRegister && Invoice != null &&
                            (Invoice.Status == InvoiceStatus.Parked
                            || Invoice.Status == InvoiceStatus.LayBy
                            || Invoice.Status == InvoiceStatus.OnAccount
                            || Invoice.Status == InvoiceStatus.BackOrder
                            || Invoice.Status == InvoiceStatus.initial
                            || Invoice.Status == InvoiceStatus.Pending
                            || Invoice.Status == InvoiceStatus.OnGoing
                            || Invoice.Status == InvoiceStatus.Quote)
                            //|| Invoice.LocalInvoiceStatus == LocalInvoiceStatus.Pending)
                            && (Invoice.InvoicePayments == null
                            || !Invoice.InvoicePayments.Any(x => !x.IsDeleted)))
                        {
                            if (Settings.IsRestaurantPOS && !Invoice.InvoiceLineItems.Any())
                                result = true;
                            else
                            {
                                var alerttitle = "Discard this sale?";
                                var alertmsg = "Click 'Yes' to permanently delete this transaction.";
                                result = await App.Alert.ShowAlert(alerttitle, alertmsg, "Yes", "Cancel");
                            }
                            if (result)
                            {
                                using (new Busy(this, true))
                                {
                                    if (Invoice.Status == InvoiceStatus.initial || Invoice.Status == InvoiceStatus.Pending)
                                    {
                                        saleService.RemoveLocalPendingInvoice(Invoice, true);
                                    }
                                    else
                                    {
                                        Invoice.Status = InvoiceStatus.Voided;
                                        Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                                    }
                                    Invoice = null;
                                    InvoiceCalculations.PreviousInvoiceLineItem = null;
                                    InvoiceCalculations.OffersToSelectManually = null;
                                    InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                                }
                            }
                        }
                        else
                        {
                            //addNewSale();
                            Invoice = null;
                            InvoiceCalculations.PreviousInvoiceLineItem = null;
                            InvoiceCalculations.OffersToSelectManually = null;
                            InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                            Table = null; //#94565
                        }
                        IsOrdered = Invoice?.InvoiceLineItems == null ? false : Invoice.InvoiceLineItems.Any(); //#94565
                        SendPeerNotification(Invoice);
                        if (_navigationService.NavigatedPage is BaseContentPage<CheckOutViewModel>)
                        {
                            var vm = ((BaseContentPage<CheckOutViewModel>)_navigationService.NavigatedPage).ViewModel;
                            vm?.UpdateViews();
                            if (result)
                            {
                                if (vm.OccupiedTable > 0)
                                    vm.OccupiedTable--;
                                vm?.SetOccupiedTable();
                                Table = null;//#94565
                            }
                        }
                        //#94565
                        break;
                    case "RefundAndVoid":

                        if (IsOpenRegister && Invoice != null && (Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy || Invoice.Status == InvoiceStatus.BackOrder || Invoice.Status == InvoiceStatus.OnAccount) && Invoice.InvoicePayments != null && Invoice.InvoicePayments.Any())
                        {
                            using (new Busy(this, true))
                            {
                                if (refundAndVoidPage == null)
                                {
                                    refundAndVoidPage = new RefundAndVoidPage();
                                    refundAndVoidPage.ViewModel.InvoiceRefund += async delegate (object sender, PaymentOptionDto e)
                                    {
                                        using (new Busy(this, true))
                                        {
                                            if (e != null && refundAndVoidPage.ViewModel.Invoice != null)
                                            {
                                                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                                                    await NavigationService.PopModalAsync();

                                                Invoice = null;
                                                InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                                                WeakReferenceMessenger.Default.Send(new Messenger.UpdateShopDataMessenger(true));
                                            }
                                        }
                                    };
                                }
                                refundAndVoidPage.ViewModel.Invoice = Invoice;
                                refundAndVoidPage.ViewModel.LoadPaymentOption();

                                MainThread.BeginInvokeOnMainThread(async () =>
                                {
                                    await NavigationService.PushModalAsync(refundAndVoidPage);

                                });
                            }
                        }
                        else
                        {
                            Invoice = null;
                            InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                        }

                        break;
                    case "LayBy":
                        try
                        {
                            if (InvoiceCalculations.CheckHasBackOrder(Invoice) && (Invoice.CustomerId == null || Invoice.CustomerId == 0))
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("BackOrderCutomerValidation"), Colors.Red, Colors.White);
                                return;
                            }

                            using (new Busy(this, true))
                            {
                                Invoice.Status = InvoiceStatus.LayBy;
                                Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                                InvoiceLineItems = Invoice.InvoiceLineItems;
                                await addNewSale();
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                        break;
                    case "OpenCashDrawer":
                        await OpenCashDrawerManual();
                        break;

                    //Ticket start:#22406 Quote sale.by rupesh .
                    case "ParkAndContinue":
                        Invoice.Status = InvoiceStatus.Parked;
                        Invoice = await InvoiceCalculations.FinaliseOrder(Invoice, offers, saleService, outletService, productService);
                        Invoice = null;
                        InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                        break;

                    //Ticket start:#29035 iPad - Quote sale should not be voided when user click on Point of sale option.by rupesh
                    //Ticket start:#92764.by rupesh
                    case "CancelQuote" or "CancelBackorder":
                        Invoice = null;
                        InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                        break;
                        //Ticket end:#92764. by rupesh
                        //Ticket end:#29035.by rupesh
                        //Ticket end:#22406 Quote sale.by rupesh .


                }
            }
            catch (Exception ex)
            {
                EventCallRunning = false;
                ex.Track();
            }
            finally
            {
                EventCallRunning = false;
                IsOpenOptionPopUp = 0;
            }
        }

        //Start #45654 FR - Bypassing the Who is Selling screen - Pratik
        public async Task<bool> ByPassWhoSelling()
        {
            var tcs = new TaskCompletionSource<bool>();
            var autolockpage = new AutoLockPage();
            autolockpage.ViewModel.CurrentUser = null;
            autolockpage.HasBackButtton = false;
            autolockpage.HasSwitchButtton = false;
            autolockpage.AuthennticationSuccessed += async (object sender, bool e) =>
            {
                try
                {
                    if (e && sender != null && sender is AutoLockPage)
                    {
                        var Autolockpage = (AutoLockPage)sender;
                        Settings.CurrentUser = Autolockpage.ViewModel.CurrentUser;
                        Settings.CurrentUserEmail = Autolockpage.ViewModel.CurrentUser.EmailAddress;
                        Settings.GrantedPermissionNames = Autolockpage.ViewModel.CurrentUser.GrantedPermissionNames;
                        if (EnterSalePage.ServedBy != null && EnterSalePage.ServedBy.Id == Autolockpage.ViewModel.CurrentUser.Id
                        && _navigationService.IsFlyoutPage && NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                        {
                            SetIsClosePopupOrNot();
                        }
                        EnterSalePage.ServedBy = Autolockpage.ViewModel.CurrentUser;
                        WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("All-Data"));
                        WeakReferenceMessenger.Default.Send(new Messenger.UpdateShopDataMessenger(true));
                        if (Settings.CurrentUser.AllowOverrideLangugeSettingOverGeneralSetting)
                        {
                            Settings.StoreCulture = Settings.CurrentUser.Language;
                            var objGetTimeZoneService = DependencyService.Get<HikePOS.Services.IGetTimeZoneService>();
                            Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(Settings.CurrentUser.IANATimeZone);
                            Settings.StoreTimeZoneInfoId = Settings.CurrentUser.IANATimeZone;
                            if (Settings.StoreCulture.ToLower() == "ar" || Settings.StoreCulture.ToLower() == "ar-kw")
                            {
                                Settings.SymbolForDecimalSeperatorForNonDot = ",";
                            }
                            Extensions.SetCulture(Settings.StoreCulture.ToLower());
                        }
                        else
                        {
                            Settings.StoreCulture = Settings.StoreZoneAndFormatDetail.Language;
                            var objGetTimeZoneService = DependencyService.Get<HikePOS.Services.IGetTimeZoneService>();
                            Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(Settings.StoreZoneAndFormatDetail.IanaTimeZone);
                            Settings.StoreTimeZoneInfoId = Settings.StoreZoneAndFormatDetail.IanaTimeZone;
                            Extensions.SetCulture(Settings.StoreCulture.ToLower());
                        }
                        tcs.SetResult(true);
                        tcs = null;
                        await Autolockpage.Close();
                    }
                }
                catch (Exception ex)
                {
                    SetIsClosePopupOrNot(false);
                    ex.Track();
                }
            };
            ((BaseContentPage<EnterSaleViewModel>)_navigationService.NavigatedPage).ViewModel.IsOpenPopup = true;//Ticket:#99388 scanner problem.
            await NavigationService.PushModalAsync(autolockpage);
            return await tcs.Task;
        }

        void SetIsClosePopupOrNot(bool isclose = true)
        {
            var lastpage = _navigationService.NavigatedPage;
            if (lastpage != null && lastpage is BaseContentPage<EnterSaleViewModel> baseContentPage)
            {
                baseContentPage.ViewModel.IsClosePopup = isclose;
            }
        }
        //End #45654 - Pratik


        public async Task<bool> OpenCashDrawerManual()
        {
            try
            {
                // using (new Busy(this, true))
                // {
                //await Task.Delay(1);


                // Note: Zoho ticket 7702
                //if (Settings.GetCachePrinters != null && Settings.GetCachePrinters.Any(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint) && x.EnableCashDrawer == true))
                if (Settings.GetCachePrinters != null && Settings.GetCachePrinters.Any(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint)))
                {
                    var print = DependencyService.Get<IPrint>();

                    ObservableCollection<Printer> AvailablePrinter;

                    // Note: Zoho ticket 7702
                    //AvailablePrinter = new ObservableCollection<Printer>(Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint) && x.EnableCashDrawer == true).ToList());
                    AvailablePrinter = new ObservableCollection<Printer>(Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint)).ToList());
                    if (AvailablePrinter != null && AvailablePrinter.Count > 0)
                    {
                        var mPOPStarBarcode = DependencyService.Get<IMPOPStarBarcode>();
                        //Ticket starts #70775:The client wants to connect  usb scanner to mc3 print in ipad.by rupesh
                        var mPOPPrinterConfigure = AvailablePrinter != null && AvailablePrinter.Any(x => (!string.IsNullOrEmpty(x.ModelName) && x.ModelName.Contains("POP")) || x.EnableUSBScanner);
                        //var mPOPPrinterConfigure = AvailablePrinter != null && AvailablePrinter.Any();
                        //Ticket end #70775.by rupesh
                        if (mPOPPrinterConfigure)
                        {
                            mPOPStarBarcode.CloseService();
                        }

                        foreach (Printer objPrinter in AvailablePrinter)
                        {
                            await print.DoPrint(null, null, null, null, 0, 0, 0, 0, true, objPrinter, null, null, null);
                        }

                        if (mPOPPrinterConfigure)
                        {
                            mPOPStarBarcode.StartService();
                        }
                        var CashDrawerLogInput = new CashDrawerLogInput()
                        {
                            openTime = DateTime.UtcNow,
                            outletId = Settings.CurrentRegister.OutletID,
                            registerId = Settings.CurrentRegister.Id,
                            registerClosureId = Settings.CurrentRegister.Registerclosure.Id,
                            openedFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android
                        };
                        await saleService.AddCashDrawerLog(Fusillade.Priority.Background, true, CashDrawerLogInput);
                    }
                }
                else
                {
                    //await Application.Current.MainPage.DisplayAlert("Alert", "Please select printer in setting menu", "Ok");
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("PrinterValidationMessage"));
                }
                return true;
                // }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return true;
        }

        #region Customer group methods
        public ObservableCollection<CustomerGroupDto> getCustomerGroups()
        {
            using (new Busy(this, true))
            {
                try
                {
                    var cgroup = customerService.GetLocalCustomerGroups();
                    if (cgroup == null)
                    {
                        return new ObservableCollection<CustomerGroupDto>();
                    }
                    else
                    {
                        return cgroup;
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                    return new ObservableCollection<CustomerGroupDto>();
                }
            }
        }
        #endregion

        #region search/select customer methods
        public async Task customerOnSelect(CustomerDto_POS customer)
        {
            try
            {
                if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RegisterOpenAlertMessage"));
                    return;
                }
                if (Invoice == null)
                {
                    await addNewSale();
                    //Ticket start:#33582 iPad: getting previously customer name getting in quote sale.by rupesh
                    await CustomerModel.SelectedCustomerChanged(customer);
                    //Ticket end:#33582 .by rupesh

                }
                // await Task.Run(async() =>
                // {
                await InvoiceCalculations.CustomerOnSelectAsync(customer, Invoice, offers, productService, taxServices);
                // });
                InvoiceLineItems = Invoice.InvoiceLineItems;
                SendPeerNotification(Invoice);
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }
        #endregion

        #region invoice line item info methods
        public async Task updateSellItem(UpdatedInvoiceLineItemMessageCenter updatedInvoiceLineItem)
        {
            try
            {
                var sellInvoiceitem = updatedInvoiceLineItem.invoiceLineItemDto;
                var item = Invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == sellInvoiceitem.InvoiceItemValue && x.InvoiceItemType == sellInvoiceitem.InvoiceItemType && x.InvoiceExtraItemValueParent == sellInvoiceitem.InvoiceExtraItemValueParent && x.Sequence == sellInvoiceitem.Sequence);
                if (item != null)
                {
                    var ItemOldQuantity = item.Quantity;
                    item.Quantity = sellInvoiceitem.Quantity;
                    item.RetailPrice = sellInvoiceitem.RetailPrice;
                    item.SoldPrice = sellInvoiceitem.SoldPrice;
                    item.DiscountedQty = sellInvoiceitem.DiscountedQty;
                    item.DiscountValue = sellInvoiceitem.DiscountValue;
                    item.DiscountIsAsPercentage = sellInvoiceitem.DiscountIsAsPercentage;
                    item.Description = sellInvoiceitem.Description;
                    item.Notes = sellInvoiceitem.Notes;
                    item.OffersNote = sellInvoiceitem.OffersNote;
                    item.TaxId = sellInvoiceitem.TaxId;
                    item.TaxName = sellInvoiceitem.TaxName;
                    item.TaxRate = sellInvoiceitem.TaxRate;
                    item.LineItemTaxes = sellInvoiceitem.LineItemTaxes;
                    item.MarkupValue = sellInvoiceitem.MarkupValue;  //Start ticket#103384 
                    //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
                    item.ServedByName = sellInvoiceitem.ServedByName;
                    item.CreatorUserId = sellInvoiceitem.CreatorUserId;
                    //end #84287 .by Pratik

                    item.approvedByUser =  sellInvoiceitem.approvedByUser; //#95241
                    item.approvedByUserName =  sellInvoiceitem.approvedByUserName; //#95241

                    if (item.InvoiceItemType == InvoiceItemType.Standard)
                    {
                        //START Ticket #74344 iOS and WEB :: Discount Issue : FOR CustomerPricebookDiscount By pratik
                        // item = await InvoiceCalculations.CustomerPricebookDiscount(Invoice, item, Invoice.CustomerDetail);
                        //End Ticket #74344 By pratik
                        var product = productService.GetLocalProduct(item.InvoiceItemValue);
                        var result = updatedInvoiceLineItem.result;//await InvoiceCalculations.CheckstockValidation(product, sellInvoiceitem.Quantity, sellInvoiceitem.BackOrderQty);
                        if (result.IsValid)
                        {

                            sellInvoiceitem.BackOrderQty = result.BackOrderQty;
                            if (result.BackOrderQty > 0)
                            {
                                if (result.Validatedstock <= 0)
                                    sellInvoiceitem.Quantity = 0;
                                else
                                    sellInvoiceitem.Quantity = result.Quantity;

                                sellInvoiceitem.Description = result.BackOrderQty + " Quantity in back order";
                            }
                            item.BackOrderQty = sellInvoiceitem.BackOrderQty;
                            item.Quantity = sellInvoiceitem.Quantity;
                            //item.DiscountValue = sellInvoiceitem.DiscountValue;
                            //item.DiscountIsAsPercentage = sellInvoiceitem.DiscountIsAsPercentage;
                            item.RetailPrice = sellInvoiceitem.RetailPrice;
                            item.Description = sellInvoiceitem.Description;
                            if (Invoice.Status != InvoiceStatus.Refunded && sellInvoiceitem.Quantity > 0)
                            {
                                Invoice = await InvoiceCalculations.AddofferTolineItem(Invoice, offers, product, item, productService);
                            }
                            using (new Busy(this, true))
                            {
                                await InvoiceCalculations.UpdatelineItem(Invoice, offers, product, item, productService, 0);
                                InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                            }
                        }
                        else
                        {
                            using (new Busy(this, true))
                            {
                                if (result.BackOrderQty > 0)
                                {
                                    if (result.Validatedstock <= 0)
                                        item.Quantity = 0;
                                    else
                                    {
                                        item.Quantity = ItemOldQuantity;
                                    }
                                }
                                await InvoiceCalculations.UpdatelineItem(Invoice, offers, product, item, productService, 0);
                                //item = InvoiceCalculations.CalculateLineItemTotal(item, Invoice);
                                InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                            }
                        }

                    }
                    //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                    else if (item.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
                    {
                        //START Ticket #74344 iOS and WEB :: Discount Issue : FOR CustomerPricebookDiscount By pratik
                        // item = await InvoiceCalculations.CustomerPricebookDiscount(Invoice, item, Invoice.CustomerDetail);
                        //End Ticket #74344 By pratik
                        var product = productService.GetLocalUnitOfMeasureProduct(item.InvoiceItemValue);
                        var result = updatedInvoiceLineItem.result;//await InvoiceCalculations.CheckstockValidation(product, sellInvoiceitem.Quantity, sellInvoiceitem.BackOrderQty);
                        if (result.IsValid)
                        {

                            sellInvoiceitem.BackOrderQty = result.BackOrderQty;
                            if (result.BackOrderQty > 0)
                            {
                                if (result.Validatedstock <= 0)
                                    sellInvoiceitem.Quantity = 0;
                                else
                                    sellInvoiceitem.Quantity = result.Quantity;

                                sellInvoiceitem.Description = result.BackOrderQty + " Quantity in back order";
                            }
                            item.BackOrderQty = sellInvoiceitem.BackOrderQty;
                            item.Quantity = sellInvoiceitem.Quantity;
                            //item.DiscountValue = sellInvoiceitem.DiscountValue;
                            //item.DiscountIsAsPercentage = sellInvoiceitem.DiscountIsAsPercentage;
                            item.RetailPrice = sellInvoiceitem.RetailPrice;
                            item.Description = sellInvoiceitem.Description;
                            if (Invoice.Status != InvoiceStatus.Refunded && sellInvoiceitem.Quantity > 0)
                            {
                                Invoice = await InvoiceCalculations.AddofferTolineItem(Invoice, offers, product, item, productService);
                            }
                            using (new Busy(this, true))
                            {
                                await InvoiceCalculations.UpdatelineItem(Invoice, offers, product, item, productService, 0);
                                InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                            }

                        }
                        else
                        {
                            using (new Busy(this, true))
                            {
                                if (result.BackOrderQty > 0)
                                {
                                    if (result.Validatedstock <= 0)
                                        item.Quantity = 0;
                                    else
                                    {
                                        item.Quantity = ItemOldQuantity;
                                    }
                                }
                                await InvoiceCalculations.UpdatelineItem(Invoice, offers, product, item, productService, 0);
                                //item = InvoiceCalculations.CalculateLineItemTotal(item, Invoice);
                                InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                            }
                        }

                    }
                    //Ticket end:#20064 .by rupesh
                    else
                    {
                        using (new Busy(this, true))
                        {
                            var product = productService.GetLocalProduct(item.InvoiceItemValue);
                            sellInvoiceitem = InvoiceCalculations.CalculateLineItemTotal(sellInvoiceitem, Invoice);
                            item.BackOrderQty = sellInvoiceitem.BackOrderQty;
                            item.Quantity = sellInvoiceitem.Quantity;
                            item.DiscountValue = sellInvoiceitem.DiscountValue;
                            item.DiscountIsAsPercentage = sellInvoiceitem.DiscountIsAsPercentage;
                            item.RetailPrice = sellInvoiceitem.RetailPrice;
                            item.Description = sellInvoiceitem.Description;

                            await InvoiceCalculations.UpdatelineItem(Invoice, offers, product, item, productService, 0);
                            //item = InvoiceCalculations.CalculateLineItemTotal(item, Invoice);
                            InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                        }
                    }

                    SendPeerNotification(Invoice);
                    //Ticket #9624 Start: Extra product line item not showing on web issue. Following code commented to stop rearranging lineitems after editing.        By Nikhil.
                    //Invoice.InvoiceLineItems.Remove(Invoice.InvoiceLineItems.First(x => x.InvoiceItemValue == sellInvoiceitem.InvoiceItemValue && x.InvoiceItemType == sellInvoiceitem.InvoiceItemType && x.InvoiceExtraItemValueParent == sellInvoiceitem.InvoiceExtraItemValueParent && x.Sequence == sellInvoiceitem.Sequence));
                    //Invoice.InvoiceLineItems.Add(item);
                    //Ticket #9624 End:By Nikhil.

                    InvoiceLineItems = Invoice.InvoiceLineItems;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        async void removeSellItem(InvoiceLineItemDto sellInvoiceitem)
        {
            try
            {
                if (!sellInvoiceitem.isEnable || IsLoad)
                {
                    return;
                }

                IsLoad = true;

                var exchangeLineItem = Invoice.InvoiceLineItems.Where(a => a.Quantity < 0);

                if (Invoice.Status == InvoiceStatus.Exchange && exchangeLineItem.Count() <= 1 && sellInvoiceitem.Id > 0)
                {
                    IsLoad = false;
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Minimum 1 line item is required to exchange product"));
                    return;
                }

                bool res = false;

                var itemCount = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount)?.ToList();
                int index = -1;

                // Start ticket #22335
                if ((Invoice.Status != InvoiceStatus.OnAccount) || (itemCount.Count != 1))
                {
                    // end ticket #22335
                    var tempOnAccoutInvoice = Invoice;

                    // bool res = false;
                    InvoiceDto tempInvoice = null;

                    if (Invoice.Status == InvoiceStatus.Pending)
                    {
                        if (Invoice.TotalPaid > 0)
                        {
                            if (itemCount == null || itemCount.Count == 1)
                            {
                                res = await App.Alert.ShowAlert("Confirmation", "Are you sure you wish to remove last item, it will also remove Payment for this sale?", "Yes", "No");
                                if (!res)
                                {
                                    IsLoad = false;
                                    return;
                                }
                                else
                                {
                                    tempInvoice = Invoice;
                                    var result = saleService.RemovePendingLocalInvoice(tempInvoice);
                                }
                            }
                        }
                    }
                    // #94565
                    else if (Invoice.InvoiceFloorTable != null || Invoice.Status == InvoiceStatus.OnGoing)
                    {
                        if (itemCount == null || itemCount.Count == 1)
                        {
                            res = await App.Alert.ShowAlert("Are you sure?", "This is the last item in this invoice; if it is removed, a new invoice will be created.", "Yes", "No");
                            if (!res)
                            {
                                IsLoad = false;
                                return;
                            }
                        }
                    }
                    // #94565


                    //if (sellInvoiceitem.InvoiceExtraItemValueParent != null)
                    //{
                    var extraProducts = Invoice.InvoiceLineItems.Where(x => x.InvoiceExtraItemValueParent == sellInvoiceitem.Sequence);
                    if (extraProducts != null && extraProducts.Count() > 0)
                    {
                        while (extraProducts != null && extraProducts.Count() > 0)
                        {
                            //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                            Invoice.InvoiceLineItems.Remove(extraProducts.FirstOrDefault());
                        }
                        if (DeviceInfo.Platform == DevicePlatform.Android)
                            InvoiceLineItems = null;
                        InvoiceLineItems = Invoice.InvoiceLineItems;
                    }


                    //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                    //if (Invoice.Status != InvoiceStatus.Refunded)
                    if (Invoice.Status != InvoiceStatus.Refunded && sellInvoiceitem.Quantity > 0)
                    //END Ticket #74344 By pratik
                    {
                        int? productId = sellInvoiceitem.InvoiceItemValue;
                        if (sellInvoiceitem.InvoiceItemValueParent != null)
                            productId = sellInvoiceitem.InvoiceItemValue;

                        var offerLineItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);

                        if (offerLineItem != null)
                        {
                            while (offerLineItem != null && offerLineItem.Count() > 0)
                            {

                                //Ticket start: #65926 Discounts are not working properly  by Pratik
                                var firstoffer = offerLineItem.FirstOrDefault();

                                var applyofferLineItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == firstoffer.InvoiceItemValueParent && (x.OfferId == null || x.OfferId == firstoffer.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount
                                && x.IsExchangedProduct == false);

                                applyofferLineItem?.ForEach(a => a.OfferDiscountPercent = null);
                                Invoice.InvoiceLineItems.Remove(firstoffer);

                                // Invoice.InvoiceLineItems.Remove(offerLineItem.FirstOrDefault());

                                //Ticket end: #65926 by Pratik
                            }
                            if (DeviceInfo.Platform == DevicePlatform.Android)
                                InvoiceLineItems = null;
                            InvoiceLineItems = Invoice.InvoiceLineItems;
                        }
                    }

                    if (sellInvoiceitem.BackOrderQty > 0)
                        sellInvoiceitem.BackOrderQty = 0;

                    var product = productService.GetLocalProduct(sellInvoiceitem.InvoiceItemValue);

                    index = Invoice.InvoiceLineItems.IndexOf(sellInvoiceitem);
                    if (index > -1)
                    {
                        Invoice.InvoiceLineItems.RemoveAt(index);
                    }
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                        InvoiceLineItems = null;
                    InvoiceLineItems = Invoice.InvoiceLineItems;
                    //Ticket #1998 start decreasing discount if product removed. fixed by rupesh
                    if ((Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange) && !Invoice.DiscountIsAsPercentage && sellInvoiceitem.TotalAmount <= 0)
                    {
                        decimal totalAmount = exchangeLineItem.Sum(x => x.TotalAmount) + sellInvoiceitem.TotalAmount;
                        Invoice.DiscountValue -= sellInvoiceitem.TotalAmount * Invoice.DiscountValue / totalAmount;
                    }
                    //Ticket #1998 End . fixed by rupesh

                    //if(product.ProductOutlet.Committedstock > 0)
                    //product.ProductOutlet.Committedstock = 0;
                    sellInvoiceitem.Quantity = 0;
                    //Start Ticket #73532 iOS: Inclusve Discount products get removed with discount on POS screen while removing any other product with exclusive discount offer setting applied By Pratik
                    //Start Ticket #73624 iOS Discount: Buy X units of brand A and get Y units of brand B for free : Not working (All Discount with Exchange) By Pratik
                    // if (Invoice.Status == InvoiceStatus.Exchange || Invoice.Status == InvoiceStatus.Refunded)
                    await InvoiceCalculations.UpdatelineItem(Invoice, offers, product, sellInvoiceitem, productService, 0);
                    InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                    //else
                    //End Ticket #73624 (All Discount with Exchange) By Pratik
                    //End Ticket #73532 By Pratik

                    // await InvoiceCalculations.UpdateStock(Invoice, productService);

                    // Start ticket #22335
                    //Ticket start:#24138 iOS - Wrong Logic When Removing All Products from The Cart.by rupesh
                    //Tickeet start:#26646,#26946,#84289iPad: park sale missing in sales history.by rupesh
                    if (Invoice.Status != InvoiceStatus.OnAccount && Invoice.Status != InvoiceStatus.Parked && Invoice.Status != InvoiceStatus.LayBy && Invoice.Status != InvoiceStatus.Quote && Invoice.Status != InvoiceStatus.BackOrder && Invoice.Status != InvoiceStatus.OnGoing) // #94565
                    {
                        // End ticket #22335,#84289
                        saleService.RemoveLocalPendingInvoice(Invoice, false);
                    }
                    //Ticket end:#24138,#26646,#26946  .by rupesh

                    lineItemCount = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount)?.ToList();
                }
                else
                { // Start ticket #22335
                    lineItemCount = null;
                    // end ticket #22335
                }

                if (lineItemCount == null || lineItemCount.Count == 0)
                {
                    // #94565
                    if (Invoice.InvoiceFloorTable != null || Invoice.Status == InvoiceStatus.OnGoing)
                    {
                        if (res)
                        {
                            Invoice?.InvoiceLineItems?.Clear();
                            Invoice = null;
                            InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                            res = false;
                        }
                        InvoiceCalculations.PreviousInvoiceLineItem = null;
                        InvoiceCalculations.OffersToSelectManually = null;
                        Table = null;
                    }
                    else
                    {
                        //totalQuantity = 0;
                        var customerId = Invoice.CustomerId;
                        var customerName = Invoice.CustomerName; //'Cash sale'; //app.localize('guest');
                        var customerGroupId = Invoice.CustomerGroupId;
                        var customerGroupName = Invoice.CustomerGroupName;
                        var customerGroupDiscount = Invoice.CustomerGroupDiscount;
                        var customerGroupDiscountType = Invoice.CustomerGroupDiscountType;
                        var customerGroupDiscountNote = Invoice.CustomerGroupDiscountNote;
                        var customerGroupDiscountNoteInside = Invoice.CustomerGroupDiscountNoteInside;
                        var customerGroupDiscountNoteInsidePrice = Invoice.CustomerGroupDiscountNoteInsidePrice;
                        var priceListCustomerCurrentLoyaltyPoints = Invoice.PriceListCustomerCurrentLoyaltyPoints;
                        var customer = Invoice.CustomerDetail;
                        var customerPhone = Invoice.CustomerPhone;

                        await addNewSale(false);
                        await Task.Delay(10);
                        Invoice.CustomerId = customerId;
                        Invoice.CustomerName = customerName; //'Cash sale'; //app.localize('guest');
                        Invoice.CustomerGroupId = customerGroupId;
                        Invoice.CustomerGroupName = customerGroupName;
                        Invoice.CustomerGroupDiscount = customerGroupDiscount;
                        Invoice.CustomerGroupDiscountType = customerGroupDiscountType;
                        Invoice.CustomerGroupDiscountNote = customerGroupDiscountNote;
                        Invoice.CustomerGroupDiscountNoteInside = customerGroupDiscountNoteInside;
                        Invoice.CustomerGroupDiscountNoteInsidePrice = customerGroupDiscountNoteInsidePrice;
                        Invoice.CustomerGroupDiscountType = customerGroupDiscountType;
                        Invoice.PriceListCustomerCurrentLoyaltyPoints = priceListCustomerCurrentLoyaltyPoints;
                        Invoice.CustomerDetail = customer;
                        //Ticket start:#29657 Seach customer using phone number.by rupesh
                        Invoice.CustomerPhone = customerPhone;
                        //Ticket end:#29657 .by rupesh
                        if (Invoice.CustomerDetail != null && Invoice.CustomerDetail.CustomerGroupId != null && Invoice.Status != InvoiceStatus.Refunded)
                        {
                            var customerGroup = customerService.GetLocalCustomerGroupById(Invoice.CustomerDetail.CustomerGroupId.Value);
                            if (customerGroup != null && customerGroup.CustomerGroupDiscountType == 1)
                            {
                                Invoice.CustomerGroupDiscountNote = Invoice.CustomerGroupName + ":" + Invoice.CustomerGroupDiscount + "% " + "off";
                                Invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + Invoice.CustomerGroupName + ", " + "Item discount" + " " + Invoice.CustomerGroupDiscount + "% ";
                            }
                            else
                            {
                                Invoice.CustomerGroupDiscountNote = Invoice.CustomerGroupName;
                            }
                        }

                        if (res)
                        {
                            await addNewSale();
                            Invoice = null;
                            InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                            res = false;
                        }
                        InvoiceCalculations.PreviousInvoiceLineItem = null;
                        InvoiceCalculations.OffersToSelectManually = null;
                    }
                    // #94565
                }
                else
                {
                    // InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);

                    if (!string.IsNullOrEmpty(sellInvoiceitem.OffersNote))
                    {
                        var tempInvoiceLineItems = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount).Copy();
                        bool istotalcall = false;
                        foreach (var item in tempInvoiceLineItems)
                        {
                            var result = productService.GetLocalProduct(item.InvoiceItemValue);
                            //Exchange Issue ticket 7987, 7657
                            if (result != null && Invoice.Status != InvoiceStatus.Refunded && item.InvoiceItemType != InvoiceItemType.Discount && item.TotalAmount > 0)
                            {
                                istotalcall = true;
                                await InvoiceCalculations.AddofferTolineItem(Invoice, offers, result, item, productService, false);
                                InvoiceCalculations.CalculateLineItemTotal(item, Invoice, false);
                            }
                        }
                        if (istotalcall)
                            InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                        //End Ticket #73624 (All Discount with Exchange) By Pratik

                    }
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                        InvoiceLineItems = null;
                    InvoiceLineItems = Invoice.InvoiceLineItems;
                    if (res)
                    {
                        await addNewSale();
                        Invoice = null;
                        InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                        res = false;
                    }
                }

                IsLoad = false;

            }
            catch (Exception ex)
            {
                IsLoad = false;
                ex.Track();
            }
            finally
            {
                IsOrdered = Invoice?.InvoiceLineItems == null ? false : Invoice.InvoiceLineItems.Any(); //#94565
                SendPeerNotification(Invoice);
                IsLoad = false;
            }
        }
        #endregion

        #region Add, reopen, refund, return sale/Invoice
        public async Task addNewSale(bool AllowToAskUserSelection = true)
        {
            //EnterSalePage.StaticInvoice = null;
            //EnterSalePage.StaticInvoiceStatus = CurrentInvoiceStatus.Reopen;
            using (new Busy(this, false))
            {
                try
                {

                    Register = Settings.CurrentRegister;
                    var StoreGeneralRules = Settings.StoreGeneralRule;
                    var StoreZoneAndFormatDetail = Settings.StoreZoneAndFormatDetail;

                    Invoice = new InvoiceDto()
                    {
                        Number = string.Empty,
                        TransactionDate = Extensions.moment(),
                        CustomerId = null,
                        CustomerTempId = string.Empty,
                        CustomerGroupId = null,
                        CustomerGroupDiscount = null,
                        CustomerGroupDiscountNote = "",
                        OutletId = Register.OutletID,
                        OutletName = Register.OutletName,
                        RegisterId = Register.Id,
                        CurrentRegister = Register.Id,
                        RegisterName = Register.Name,
                        Status = InvoiceStatus.initial,
                        LocalInvoiceStatus = LocalInvoiceStatus.Pending,
                        TaxInclusive = StoreGeneralRules.TaxInclusive,
                        ApplyTaxAfterDiscount = StoreGeneralRules.ApplyTaxAfterDiscount,
                        DiscountIsAsPercentage = true,
                        DiscountValue = 0,
                        DiscountNote = "",
                        TipIsAsPercentage = true,
                        TipValue = 0,
                        SubTotal = 0,
                        TotalDiscount = 0,
                        TotalShippingCost = 0,
                        OtherCharges = 0,
                        Tax = 0,
                        TotalTip = 0,
                        RoundingAmount = 0,
                        NetAmount = 0,
                        TotalTender = 0,
                        ChangeAmount = 0,
                        TotalPaid = 0,
                        Currency = StoreZoneAndFormatDetail.Currency,
                        ServedBy = 0,
                        Note = "",
                        InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android,
                        CanBeModified = false,
                        TrackNumber = "",
                        TrackURL = "",
                        TrackDetails = "",
                        ReceiptHTML = "",
                        IsReStockWhenRefund = false,
                        //Ticket start:#50092 iOS: Date issue on the Receipt.by rupesh
                        CreationTime = Extensions.moment()
                        //Ticket end:#50092 .by rupesh

                    };
                    InvoiceLineItems = Invoice.InvoiceLineItems;
                    if (Settings.CurrentRegister != null && Settings.CurrentRegister.Registerclosure != null)
                    {
                        Invoice.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id;
                    }

                    _ = CustomerModel.SelectedCustomerChanged(null);
                    DiscardVisible = true;
                    ParkedVisible = true;

                    EnterSalePage.ServedBy = Settings.CurrentUser;
                    //Start #45654 FR - Bypassing the Who is Selling screen - Pratik
                    if (AllowToAskUserSelection && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EachUserHasAUniquePinCode)
                    {
                        await ByPassWhoSelling();
                        if (_navigationService.CurrentPage?.Navigation?.ModalStack != null && _navigationService.CurrentPage.Navigation.ModalStack.Count > 0)
                            await _navigationService.CurrentPage.Navigation.PopModalAsync();
                    }
                    else if (AllowToAskUserSelection && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.SwitchUserAfterEachSale)
                    {
                        //End #45654 - Pratik
                        if (allUserPage == null)
                        {
                            allUserPage = new AllUserPage();
                            allUserPage.ViewModel.SelectedUser += async (object sender, UserListDto e) =>
                            {
                                await allUserPage.ViewModel.Close();
                                EnterSalePage.ServedBy = e;
                            };
                        }
                        await NavigationService.PushModalAsync(allUserPage);
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
                finally
                { 
                    IsOrdered = Invoice?.InvoiceLineItems == null ? false : Invoice.InvoiceLineItems.Any(); //#94565
                }
            }
        }

        public async Task ReopenSaleFromHistory(InvoiceDto invoice = null)
        {
            try
            {

                #region
                /*
                    Below code is used to get pending invoice from localdatabase
                    and display in entersale page when application open.
                */
                if (Settings.IsEnterSaleFirstTimeLoad)
                {
                    Settings.IsEnterSaleFirstTimeLoad = false;
                    var tempinvoice = saleService.GetPendingInvoiceFromDB();
                    var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(tempinvoice);

                    invoice = JsonConvert.DeserializeObject<InvoiceDto>(jsonString);
                }
                #endregion

                if (invoice == null)
                    return;

                Invoice = invoice.Copy();
                //    InvoiceLineItems = Invoice.InvoiceLineItems;
                //if (Invoice.Status == InvoiceStatus.Parked || Invoice.Status == InvoiceStatus.LayBy || Invoice.Status == InvoiceStatus.BackOrder || Invoice.Status == InvoiceStatus.OnAccount)
                
                //#94565
                Table = null;
                if (Settings.IsRestaurantPOS && invoice.InvoiceFloorTable != null && invoice.InvoiceFloorTable.TableId.HasValue && invoice.InvoiceFloorTable.TableId > 0)
                {
                    Table = new CanvanceTableLayout() { TableId = invoice.InvoiceFloorTable.TableId };
                    var floors = RestaurantService.GetLocalFloors(Settings.SelectedOutletId);
                    var selectedFloor = floors?.FirstOrDefault(f => f.CanvanceLayout.Objects.Any(o => o.TableId == Invoice.InvoiceFloorTable.TableId));
                    Table = selectedFloor?.CanvanceLayout.Objects.FirstOrDefault(o => o.TableId == Invoice.InvoiceFloorTable.TableId);
                }
                //#94565

                if (invoice.Status != InvoiceStatus.Parked &&
                invoice.Status != InvoiceStatus.OnGoing &&
                invoice.Status != InvoiceStatus.LayBy &&
                invoice.Status != InvoiceStatus.Pending &&
                invoice.Status != InvoiceStatus.OnAccount) //#94565
                {
                    Invoice.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                }
                Invoice.isSync = false;

                //Ticket #12708 Discount Not Applied to BO When Fulfilling The Order. By Nikhil	 
                Invoice = await AddOfferToReOpenedBackOrder(Invoice);
                //Ticket #12708 End. By Nikhil

                //Ticket start:#72192 Discount name on invoice.by rupesh
                if (Invoice.Status == InvoiceStatus.Parked)
                {
                    var temp = Invoice.InvoiceLineItems.Where(a => a.InvoiceItemType != InvoiceItemType.Discount).Copy();

                    foreach (var item in temp)
                    {
                        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
                        item.InvoiceLineItemDetails = null;
                        //Ticket:end:#90938 IOS:FR Age varification.by rupesh
                        InvoiceLineItemDto tempItem;
                        if (item.Id == 0)
                            tempItem = Invoice.InvoiceLineItems.First(x => x.InvoiceItemValue == item.InvoiceItemValue && x.InvoiceItemValueParent == item.InvoiceItemValueParent && x.Sequence == item.Sequence);
                        else
                            tempItem = Invoice.InvoiceLineItems.First(x => x.Id == item.Id);

                        if (tempItem != null)
                        {
                            tempItem.OfferId = null;
                            tempItem.OffersNote = "";
                            tempItem.OfferDiscountPercent = 0;
                            tempItem.IsReopenFromSaleHistory = true;
                            tempItem.ReopenQuantity = tempItem.Quantity;
                            var product = productService.GetLocalProduct(tempItem.InvoiceItemValue);
                            //Ticket start:#73622 iOS: Custom sale total got remove after reoving Payment type from Sales history. by rupesh
                            if (product != null)
                            {
                                item.DisableDiscountIndividually = product.DisableDiscountIndividually; //Start #92641 iOS: correct "Exclude this product from any and all discount offers" By Pratik
                                tempItem.DisableDiscountIndividually = product.DisableDiscountIndividually; //Start #92641 iOS: correct "Exclude this product from any and all discount offers" By Pratik
                                Invoice = await InvoiceCalculations.AddofferTolineItem(Invoice, offers, product, tempItem, productService, false);
                            }
                            else
                            {
                                tempItem = InvoiceCalculations.CalculateLineItemTotal(tempItem, Invoice, false);
                            }
                            //Ticket end:#73622. by rupesh

                        }
                        //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                        else
                        {
                            var localprod = productService.GetLocalProductDB(item.InvoiceItemValue);
                            if (localprod != null)
                            {
                                item.DisableDiscountIndividually = localprod.DisableDiscountIndividually;
                            }
                        }
                        //End #92641 by Pratik
                    }
                }
                else
                {
                    foreach (var item in Invoice.InvoiceLineItems)
                    {
                        //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                        var localprod = productService.GetLocalProductDB(item.InvoiceItemValue);
                        if (localprod != null)
                        {
                            item.DisableDiscountIndividually = localprod.DisableDiscountIndividually;
                        }
                        //End #92641 by Pratik
                        //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
                        item.IsReopenFromSaleHistory = true;
                        item.ReopenQuantity = item.Quantity;
                        //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
                        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
                        item.InvoiceLineItemDetails = null;
                        //Ticket:end:#90938 .by rupesh
                        InvoiceCalculations.CalculateLineItemTotal(item, Invoice, false);
                    }

                }

                //Ticket end:#72192 .by rupesh
                //
                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                InvoiceLineItems = Invoice.InvoiceLineItems;
                if (Invoice.CustomerGroupId != null)
                {
                    CustomerGroupDto invoiceCustomerGroup = null;
                    if (Invoice.CustomerDetail?.CustomerGroupId != null)
                        invoiceCustomerGroup = customerService.GetLocalCustomerGroupById(Invoice.CustomerDetail.CustomerGroupId.Value);

                    if (Invoice.CustomerName == null)
                    {
                        if (Invoice.CustomerDetail != null)
                        {

                            Invoice.CustomerGroupName = invoiceCustomerGroup?.Name;

                            if (invoiceCustomerGroup?.CustomerGroupDiscountType == 2)
                                Invoice.CustomerGroupDiscountType = true;
                            else if (invoiceCustomerGroup?.CustomerGroupDiscountType == 1)
                                Invoice.CustomerGroupDiscountType = false;
                            else
                                Invoice.CustomerGroupDiscountType = false;
                        }
                    }
                    else
                    {
                        if (Invoice.CustomerDetail != null && invoiceCustomerGroup != null)
                        {
                            Invoice.CustomerGroupName = invoiceCustomerGroup.Name;

                            if (invoiceCustomerGroup.CustomerGroupDiscountType == 2)
                                Invoice.CustomerGroupDiscountType = true;
                            else if (invoiceCustomerGroup.CustomerGroupDiscountType == 1)
                                Invoice.CustomerGroupDiscountType = false;
                            else
                                Invoice.CustomerGroupDiscountType = false;
                        }
                    }
                }


                //Start #45654 FR - Bypassing the Who is Selling screen - Pratik
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EachUserHasAUniquePinCode)
                {
                    await ByPassWhoSelling();
                }
                else if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.SwitchUserAfterEachSale)
                {
                    //End #45654 Pratik
                    if (allUserPage == null)
                    {
                        allUserPage = new AllUserPage();
                        allUserPage.ViewModel.SelectedUser += async (object sender, UserListDto e) =>
                        {
                            await allUserPage.ViewModel.Close();
                            EnterSalePage.ServedBy = e;
                        };
                    }

                    await NavigationService.PushModalAsync(allUserPage);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //Ticket #12708 Discount Not Applied to BO When Fulfilling The Order. By Nikhil	 
        async Task<InvoiceDto> AddOfferToReOpenedBackOrder(InvoiceDto invoice)
        {
            //using (new Busy(this, true))
            //{
            if (invoice.Status == InvoiceStatus.BackOrder)
            {
                //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                var hasInStock = !invoice.IsEditBackOrderFromSaleHistory ? await saleService.HasInStock(Fusillade.Priority.Background, invoice.Id) : true;
                //Ticket end:#92764.by rupesh
                if (hasInStock)
                {
                    foreach (var item in invoice.InvoiceLineItems)
                    {
                        var product = productService.GetLocalProduct(item.InvoiceItemValue);
                        item.DisableDiscountIndividually = product.DisableDiscountIndividually; //#92641 pratik
                        invoice = await InvoiceCalculations.AddofferTolineItem(invoice, offers, product, item, productService);
                    }
                    InvoiceLineItems = Invoice.InvoiceLineItems;
                }
            }
            return invoice;
            //}
        }
        //Ticket #12708 End. By Nikhil

        public async Task RefundSaleFromHistory1(InvoiceDto invoice)
        {
            try
            {
                Invoice = InvoiceCalculations.CalculateInvoiceTotal(invoice, offers, productService);
                InvoiceLineItems = Invoice.InvoiceLineItems;
                if (Invoice.CustomerId != null && Invoice.CustomerId > 1)
                {
                    CustomerDto_POS cust = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                    if (cust != null)
                    {
                        CustomerModel.SelectedCustomer = cust;
                    }
                }
                //Start #45654 FR - Bypassing the Who is Selling screen - Pratik
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EachUserHasAUniquePinCode)
                {
                    await ByPassWhoSelling();
                }
                else if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.SwitchUserAfterEachSale)
                {
                    //End #45654 Pratik
                    if (allUserPage == null)
                    {
                        allUserPage = new AllUserPage();
                        allUserPage.ViewModel.SelectedUser += async (object sender, UserListDto e) =>
                        {
                            await allUserPage.ViewModel.Close();
                            EnterSalePage.ServedBy = e;
                        };
                    }

                    await NavigationService.PushModalAsync(allUserPage);
                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task RefundSaleFromHistory(InvoiceDto invoice)
        {
            try
            {
                Invoice = invoice.Copy();
                Invoice.Id = 0;
                Invoice.InvoiceTempId = null;
                Invoice.isSync = false;
                Invoice.Barcode = Settings.CurrentRegister.Id.ToString() + Invoice.Number; // "#" + Invoice.Number;
                Invoice.Status = InvoiceStatus.Refunded;
                Invoice.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                Invoice.TransactionDate = Extensions.moment();
                //Ticket start:#50092 iOS: Date issue on the Receipt.by rupesh
                Invoice.CreationTime = Invoice.TransactionDate;
                //Ticket end:#50092 .by rupesh
                Invoice.TotalPaid = 0;
                Invoice.TotalTender = 0;
                Invoice.ChangeAmount = 0;
                Invoice.TotalShippingCost = Invoice.TotalShippingCost > 0 ? Invoice.TotalShippingCost.ToNegative() : 0;
                //Ticket start:#33812 iPad: New Feature Request :: Shipping charge showing Tax Exclusive in sales history page.by rupesh
                Invoice.ShippingTaxAmount = Invoice.ShippingTaxAmount > 0 ? Invoice.ShippingTaxAmount.Value.ToNegative() : 0;
                //Ticket end:#33812 .by rupesh
                //Ticket start:#35310 iPad: rounding not calculating properly in refunded sale.by rupesh
                Invoice.RoundingAmount = Invoice.RoundingAmount.ToNegative();
                //Ticket end:#35310 .by rupesh
                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                Invoice.TotalPaymentSurcharge = 0;// Invoice.TotalPaymentSurcharge.ToNegative();
                //end Ticket #73190
                Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                Invoice.ReferenceInvoiceId = invoice.Id;
                if (invoice.InvoicePayments != null)
                    Invoice.ToRefundPayments = invoice.InvoicePayments;
                //refundInvoice.ReferenceStatus = SelectedSale.Status;

                Invoice.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(Invoice.InvoiceLineItems.Where(a => a.InvoiceItemType != InvoiceItemType.GiftCard)); //#92530

                foreach (var item in Invoice.InvoiceLineItems)
                {
                    if (item.InvoiceItemType != InvoiceItemType.GiftCard)
                    {
                        item.Quantity = (item.Quantity - item.RefundedQuantity) * -1;
                        item.actualqty = item.Quantity;
                        item.ActionType = ActionType.Refund;
                        item.InvoiceId = 0;
                        item.Id = 0;
                        InvoiceCalculations.CalculateLineItemTotal(item, Invoice);
                    }
                    //Ticket start:#23855,#51787  iOS - Discount Not Applied When Editing Quotes.by rupesh
                    try
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
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                    //Ticket end:#23855,#51787 .by rupesh


                }
                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                InvoiceLineItems = Invoice.InvoiceLineItems;
                if (Invoice.CustomerId != null && Invoice.CustomerId > 1)
                {
                    CustomerDto_POS cust = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                    if (cust != null)
                    {
                        CustomerModel.SelectedCustomer = cust;
                    }
                }
                //Start #45654 FR - Bypassing the Who is Selling screen - Pratik
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EachUserHasAUniquePinCode)
                {
                    await ByPassWhoSelling();
                }
                else if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.SwitchUserAfterEachSale)
                {
                    //End #45654 Pratik
                    if (allUserPage == null)
                    {
                        allUserPage = new AllUserPage();
                        allUserPage.ViewModel.SelectedUser += async (object sender, UserListDto e) =>
                        {
                            await allUserPage.ViewModel.Close();
                            EnterSalePage.ServedBy = e;
                        };
                    }

                    await NavigationService.PushModalAsync(allUserPage);
                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        public async Task ExchangeSaleFromHistory(InvoiceDto invoice)
        {
            try
            {
                if (invoice.IsReStockWhenRefund)
                    ReStockWhenRefundCalled(invoice);
                Invoice.TotalShippingCost = Invoice.TotalShippingCost > 0 ? Invoice.TotalShippingCost.ToNegative() : 0;
                //Ticket start:#33812 iPad: New Feature Request :: Shipping charge showing Tax Exclusive in sales history page.by rupesh
                Invoice.ShippingTaxAmount = Invoice.ShippingTaxAmount > 0 ? Invoice.ShippingTaxAmount.Value.ToNegative() : 0;
                //Ticket end:#33812 .by rupesh
                Invoice = InvoiceCalculations.CalculateInvoiceTotal(invoice, offers, productService);
                InvoiceLineItems = Invoice.InvoiceLineItems;

                //START Ticket #74344 iOS and WEB :: Discount Issue : FOR CustomerPricebookDiscount By pratik
                if (Invoice.CustomerGroupId != null)
                {
                    CustomerGroupDto invoiceCustomerGroup = null;
                    if (Invoice.CustomerDetail.CustomerGroupId != null)
                        invoiceCustomerGroup = customerService.GetLocalCustomerGroupById(Invoice.CustomerDetail.CustomerGroupId.Value);

                    if (Invoice.CustomerName == null)
                    {
                        if (Invoice.CustomerDetail != null)
                        {

                            Invoice.CustomerGroupName = invoiceCustomerGroup?.Name;

                            if (invoiceCustomerGroup?.CustomerGroupDiscountType == 2)
                                Invoice.CustomerGroupDiscountType = true;
                            else if (invoiceCustomerGroup?.CustomerGroupDiscountType == 1)
                                Invoice.CustomerGroupDiscountType = false;
                            else
                                Invoice.CustomerGroupDiscountType = false;
                        }
                    }
                    else
                    {
                        if (Invoice.CustomerDetail != null && invoiceCustomerGroup != null)
                        {
                            Invoice.CustomerGroupName = invoiceCustomerGroup.Name;

                            if (invoiceCustomerGroup.CustomerGroupDiscountType == 2)
                                Invoice.CustomerGroupDiscountType = true;
                            else if (invoiceCustomerGroup.CustomerGroupDiscountType == 1)
                                Invoice.CustomerGroupDiscountType = false;
                            else
                                Invoice.CustomerGroupDiscountType = false;
                        }
                    }
                }
                //END Ticket #74344 By pratik

                if (Invoice.CustomerId != null && Invoice.CustomerId > 1)
                {
                    CustomerDto_POS cust = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                    if (cust != null)
                    {
                        CustomerModel.SelectedCustomer = cust;
                    }
                }
                //Start #45654 FR - Bypassing the Who is Selling screen - Pratik
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EachUserHasAUniquePinCode)
                {
                    await ByPassWhoSelling();
                }
                else if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.SwitchUserAfterEachSale)
                {
                    //End #45654 Pratik
                    if (allUserPage == null)
                    {
                        allUserPage = new AllUserPage();
                        allUserPage.ViewModel.SelectedUser += async (object sender, UserListDto e) =>
                        {
                            await allUserPage.ViewModel.Close();
                            EnterSalePage.ServedBy = e;
                        };
                    }

                    await NavigationService.PushModalAsync(allUserPage);
                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void ReStockWhenRefundCalled(InvoiceDto invoice = null)
        {
            if (Invoice == null)
            {
                Invoice = invoice;
                InvoiceLineItems = Invoice.InvoiceLineItems;
            }

            if (Invoice != null)
            {
                using (new Busy(this, true))
                {
                    if (invoice == null)
                        Invoice.IsReStockWhenRefund = !Invoice.IsReStockWhenRefund;
                    foreach (var item in Invoice.InvoiceLineItems)
                    {
                        if (Invoice.IsReStockWhenRefund && item.ActionType == ActionType.Refund)
                        {
                            item.ActionType = ActionType.RefundReturn;
                        }
                        else if (!Invoice.IsReStockWhenRefund && item.ActionType == ActionType.RefundReturn)
                        {
                            item.ActionType = ActionType.Refund;
                        }
                    }
                }
            }
        }


        #endregion

        void addgiftCard(GiftCardDto giftCardDto)
        {
            if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RegisterOpenAlertMessage"));
                return;
            }




            if (Invoice == null)
            {
                addNewSale();
            }

            var invoiceItem = new InvoiceLineItemDto()
            {
                InvoiceItemType = InvoiceItemType.GiftCard,
                InvoiceItemValue = 0,
                Sequence = Invoice.InvoiceLineItems.Count + 1,
                Title = "GiftCard",
                Description = "",
                Quantity = 1,
                ItemCost = giftCardDto.Amount,
                RetailPrice = giftCardDto.Amount,
                SoldPrice = giftCardDto.Amount,
                GiftCardNumber = giftCardDto.Number,
                DiscountIsAsPercentage = false,
                DiscountValue = 0,
                TaxId = 1,
                TaxName = "No Tax",
                TaxRate = 0,
                //Ticket start:#14390 iOS - Gift Card Total Is Wrong. by rupesh
                CustomSaleRetailPrice = giftCardDto.Amount,
                //Ticket end:#14390.by rupesh

                giftCardInfo = new GiftCardInfo()
                {
                    addedTopUp = giftCardDto.IsTopupGiftCard,
                    //#28721 Email for Sending Gift Cards
                    fromEmail = giftCardDto.FromEmail,
                    fromName = giftCardDto.FromName,
                    recipientEmail = giftCardDto.RecipientEmail,
                    recipientMessage = giftCardDto.RecipientMessage,
                    recipientName = giftCardDto.RecipientName
                    //#28721 Email for Sending Gift Cards

                },
            };
            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    Invoice.InvoiceLineItems.Add(invoiceItem);
                else
                    Invoice.InvoiceLineItems.Insert(0, invoiceItem);
            });
            InvoiceLineItems = Invoice.InvoiceLineItems;
            //End:#45375 .by rupesh
            invoiceItem = InvoiceCalculations.CalculateLineItemTotal(invoiceItem, Invoice);
            Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
            InvoiceLineItems = Invoice.InvoiceLineItems;
        }


        #region Product methods
        public async Task selectProduct(ProductDto_POS product)
        {
            try
            {
                //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                AgeVerificationProofData = null;
                if (product.AgeVerificationToSellProduct && Invoice?.InvoiceLineItems?.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails.Select(subItem => subItem.Value)).FirstOrDefault() == null)
                {
                    AgeVerificationProofData = await CheckForAgeVerifcation(product.AgeVerificationLimit);
                    if (AgeVerificationProofData == null)
                        return;
                }
                //Ticket:end:#90938,#94423.by rupesh

                if (product == null && product.IsActive)
                {
                    return;
                }

                if (Settings.CurrentRegister == null || (Settings.CurrentRegister != null && !Settings.CurrentRegister.IsOpened))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RegisterOpenAlertMessage"));
                    return;
                }

                //Ticket start:#33945 iOS - Quote Shouldn't Verify Inventory Lock Status.by rupesh
                if (product.ProductOutlet != null && product.ProductOutlet.IsLocked && !Settings.IsQuoteSale)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ProductIsCurrentlyLockedMessage"), Colors.Red, Colors.White);
                    return;
                }
                //Ticket end:#33945 .by rupesh

                //Ticket start:#35107 iPad: composite product should not be add during stock take.by rupesh
                if (product.ProductOutlet != null && product.ProductType == ProductType.Composite && !Settings.IsQuoteSale)
                {
                    AgeVerificationProofData = null;
                    foreach (var item in product.ProductCompositeItems)
                    {
                        var subProduct = productService.GetLocalProductDB(item.CompositeProductId);

                        if (subProduct != null)//#39711 IOS App process sale issue
                        {
                            if (subProduct.ProductOutlet.IsLocked)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ProductIsCurrentlyLockedMessage"), Colors.Red, Colors.White);
                                return;
                            }
                            //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                            if (AgeVerificationProofData == null && subProduct.AgeVerificationToSellProduct && Invoice?.InvoiceLineItems?.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails.Select(subItem => subItem.Value)).FirstOrDefault() == null)
                            {
                                AgeVerificationProofData = await CheckForAgeVerifcation(product.AgeVerificationLimit);
                                if (AgeVerificationProofData == null)
                                    return;
                            }
                            //Ticket:end:#90938,#94423.by rupesh

                        }
                    }
                }
                //Ticket end:#35107 .by rupesh
                //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                if (Settings.IsBackorderSaleSelected && product.ProductType == ProductType.Composite)
                {
                    App.Instance.Hud.DisplayToast("Composite products cannot be added to backorders.", Colors.Red, Colors.White);
                    return;
                }
                //Ticket end:#92764 .by rupesh
                if (Invoice == null)
                {
                    await addNewSale();
                }

                if (Invoice.Status == InvoiceStatus.Refunded)
                {
                    return;
                }

                if (product.HasVarients)
                {
                    if (_navigationService.IsFlyoutPage && NavigationService.ModalStack != null
                      && NavigationService.ModalStack.Count > 0 && NavigationService.ModalStack.Last() is VariantProductPage)
                    {
                        return;
                    }
                    else
                    {
                        using (var variantpage = new VariantProductPage())
                        {
                            variantpage.ViewModel.AddVariantProduct += async (object sender, ProductDto_POS e) =>
                            {
                                try
                                {

                                    await variantpage.ViewModel.ClosePopupTapped_Task();
                                    if (variantpage.ViewModel != null)
                                    {
                                        //Ticket start:#33945 iOS - Quote Shouldn't Verify Inventory Lock Status.by rupesh
                                        if (e.ProductOutlet != null && e.ProductOutlet.IsLocked && !Settings.IsQuoteSale)
                                        {

                                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ProductIsCurrentlyLockedMessage"),
                                                                                        Colors.Red, Colors.White);
                                            return;
                                        }
                                        //Ticket end:#33945 .by rupesh

                                        await select_variant(e, variantpage.ViewModel.Quantity);
                                        //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                                        if (AgeVerificationProofData != null && Invoice?.InvoiceLineItems?.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails.Select(subItem => subItem.Value)).FirstOrDefault() == null)
                                        {
                                            var lineItem = Invoice.InvoiceLineItems?.LastOrDefault();
                                            lineItem.InvoiceLineItemDetails = new ObservableCollection<InvoiceLineItemDetailDto>();
                                            if (AgeVerificationProofData.Base64 != null)
                                                lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.ImageUpload, Value = AgeVerificationProofData.Base64, CreationTime = DateTime.Now.ToStoreTime() });
                                            if (AgeVerificationProofData.DateOfBirth != null)
                                                lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.BirthDate, Value = AgeVerificationProofData.DateOfBirth.ToString(), CreationTime = DateTime.Now.ToStoreTime() });
                                            if (AgeVerificationProofData.Age > 0)
                                                lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.AgeLimit, Value = AgeVerificationProofData.Age.ToString(), CreationTime = DateTime.Now.ToStoreTime() });

                                        }
                                        //Ticket:end:#90938,#94423 .by rupesh
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.Track();
                                }
                                finally
                                { 
                                    IsOrdered = Invoice?.InvoiceLineItems == null ? false : Invoice.InvoiceLineItems.Any(); //#94565
                                }
                            };
                            if (variantpage.ViewModel != null)
                            {
                                variantpage.ViewModel.Quantity = 1;
                                variantpage.ViewModel.VariantProduct = product.Copy();
                                variantpage.ViewModel.CustomerId = Invoice.CustomerId ?? 0;
                            }
                                //variantpage.ViewModel.LoadData();
                                ((BaseContentPage<EnterSaleViewModel>)_navigationService.NavigatedPage).ViewModel.IsOpenPopup = true;
                            await NavigationService.PushModalAsync(variantpage);
                        }
                    }
                }
                //else if (product.ProductType == ProductType.Service)
                //{
                //Service
                //}
                else if (product.ProductType == ProductType.Category)
                {
                    //Category
                }
                else
                {
                    //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                    // if (!product.IsUnitOfMeasure)
                    //     product = productService.GetLocalProduct(product.Id);
                    //Ticket end:#20064 .by rupesh

                    if (product.ProductExtras != null && product.ProductExtras.Count() > 0)
                    {
                        await OpenExtraProudct(product);
                    }
                    else
                    {
                        var productsLength = Invoice.InvoiceLineItems.Count();
                        var sequence = productsLength + 1;
                        await InvoiceCalculations.CheckstockAndaddToCart(Invoice, offers, product, 1, productService, taxServices, null, sequence);
                        _ = Task.Run(() =>
                        {
                            SendPeerNotification(Invoice);
                        });
                        InvoiceLineItems = Invoice.InvoiceLineItems;

                    }
                }
                //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                if (AgeVerificationProofData != null && Invoice?.InvoiceLineItems?.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails.Select(subItem => subItem.Value)).FirstOrDefault() == null)
                {
                    var lineItem = Invoice.InvoiceLineItems?.LastOrDefault();
                    lineItem.InvoiceLineItemDetails = new ObservableCollection<InvoiceLineItemDetailDto>();
                    if (AgeVerificationProofData.Base64 != null)
                        lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.ImageUpload, Value = AgeVerificationProofData.Base64, CreationTime = DateTime.Now.ToStoreTime() });
                    if (AgeVerificationProofData.DateOfBirth != null)
                        lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.BirthDate, Value = AgeVerificationProofData.DateOfBirth.ToString(), CreationTime = DateTime.Now.ToStoreTime() });
                    if (AgeVerificationProofData.Age > 0)
                        lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.AgeLimit, Value = AgeVerificationProofData.Age.ToString(), CreationTime = DateTime.Now.ToStoreTime() });

                }
                //Ticket:end:#90938,#94423 .by rupesh
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            finally
            { 
                IsOrdered = Invoice?.InvoiceLineItems == null ? false : Invoice.InvoiceLineItems.Any(); //#94565
            }
        }

        async Task OpenExtraProudct(ProductDto_POS product)
        {
            try
            {
                if (product.ProductExtras.Count < 1)
                {
                    return;
                }
                using (new Busy(this, true))
                {
                    ObservableCollection<ProductDto_POS> lstExtraProducts = new ObservableCollection<ProductDto_POS>();
                    foreach (var item in product.ProductExtras)
                    {
                        var tmpproduct = productService.GetLocalProduct(item);
                        if (tmpproduct != null && !tmpproduct.HasVarients && tmpproduct.IsActive) //start #94426 Product Optional Extras display enhancement By Pratik
                        {
                            lstExtraProducts.Add(tmpproduct);
                        }
                    }
                    if (lstExtraProducts != null && lstExtraProducts.Count > 0)
                    {
                        if (_navigationService.IsFlyoutPage && NavigationService.ModalStack != null
                         && NavigationService.ModalStack.Count > 0 && NavigationService.ModalStack.Last() is ExtraProductPage)
                        {
                            return;
                        }
                        else
                        {
                            var extraproductpage = new ExtraProductPage();
                            extraproductpage.ViewModel.Products = lstExtraProducts;
                            extraproductpage.ExtraProductAdded += async (object sender, System.Collections.Generic.IEnumerable<ProductDto_POS> e) =>
                            {
                                try
                                {
                                    var productsLength = Invoice.InvoiceLineItems.Count();
                                    var sequence = productsLength + 1;
                                    var extrasequence = sequence;

                                    //Ticket start:#63489 IOS: Items Not sequence wise in print receipt.by rupesh
                                    if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                                    {
                                        Invoice = await InvoiceCalculations.CheckstockAndaddToCart(Invoice, offers, product, 1, productService, taxServices, null, sequence);

                                        if (e != null || e.Count() > 0)
                                        {
                                            AgeVerificationProofData = null;
                                            foreach (var item in e)
                                            {
                                                //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                                                if (AgeVerificationProofData == null && item.AgeVerificationToSellProduct && Invoice?.InvoiceLineItems?.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails.Select(subItem => subItem.Value)).FirstOrDefault() == null)
                                                {
                                                    AgeVerificationProofData = await CheckForAgeVerifcation(product.AgeVerificationLimit);
                                                    if (AgeVerificationProofData == null)
                                                        return;
                                                }
                                                //Ticket:end:#90938,#94423.by rupesh
                                                if ((item.ProductType == ProductType.Standard || item.ProductType == ProductType.Service) && !item.HasVarients)
                                                {

                                                    // Extra product not show in history by PR
                                                    var extraParentValue = sequence;
                                                    //var extraParentValue = product.Id;
                                                    //End Extra product not show in history by PR
                                                    extrasequence = extrasequence + 1;
                                                    Invoice = await InvoiceCalculations.CheckstockAndaddToCart(Invoice, offers, item, 1, productService, taxServices, extraParentValue, extrasequence);

                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        if (e != null || e.Count() > 0)
                                        {
                                            AgeVerificationProofData = null;
                                            foreach (var item in e)
                                            {
                                                //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                                                if (AgeVerificationProofData == null && item.AgeVerificationToSellProduct && Invoice?.InvoiceLineItems?.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails.Select(subItem => subItem.Value)).FirstOrDefault() == null)
                                                {
                                                    AgeVerificationProofData = await CheckForAgeVerifcation(product.AgeVerificationLimit);
                                                    if (AgeVerificationProofData == null)
                                                        return;
                                                }
                                                //Ticket:end:#90938,#94423.by rupesh
                                                if ((item.ProductType == ProductType.Standard || item.ProductType == ProductType.Service) && !item.HasVarients)
                                                {

                                                    // Extra product not show in history by PR
                                                    var extraParentValue = sequence;
                                                    //var extraParentValue = product.Id;
                                                    //End Extra product not show in history by PR
                                                    extrasequence = extrasequence + 1;
                                                    Invoice = await InvoiceCalculations.CheckstockAndaddToCart(Invoice, offers, item, 1, productService, taxServices, extraParentValue, extrasequence);

                                                }
                                            }
                                        }

                                        Invoice = await InvoiceCalculations.CheckstockAndaddToCart(Invoice, offers, product, 1, productService, taxServices, null, sequence);

                                    }

                                    InvoiceLineItems = Invoice.InvoiceLineItems;
                                    //Ticket end:#63489 .by rupesh
                                    SendPeerNotification(Invoice);

                                    await extraproductpage.Close();

                                    //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                                    if (AgeVerificationProofData != null && Invoice?.InvoiceLineItems?.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails.Select(subItem => subItem.Value)).FirstOrDefault() == null)
                                    {
                                        var lineItem = Invoice.InvoiceLineItems?.LastOrDefault();
                                        lineItem.InvoiceLineItemDetails = new ObservableCollection<InvoiceLineItemDetailDto>();
                                        if (AgeVerificationProofData.Base64 != null)
                                            lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.ImageUpload, Value = AgeVerificationProofData.Base64, CreationTime = DateTime.Now.ToStoreTime() });
                                        if (AgeVerificationProofData.DateOfBirth != null)
                                            lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.BirthDate, Value = AgeVerificationProofData.DateOfBirth.ToString(), CreationTime = DateTime.Now.ToStoreTime() });
                                        if (AgeVerificationProofData.Age > 0)
                                            lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.AgeLimit, Value = AgeVerificationProofData.Age.ToString(), CreationTime = DateTime.Now.ToStoreTime() });

                                    }
                                    //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                                }
                                catch (Exception ex)
                                {
                                    ex.Track();
                                }
                                finally
                                {
                                    IsOrdered = Invoice?.InvoiceLineItems == null ? false : Invoice.InvoiceLineItems.Any(); //#94565
                                }
                            };

                            if (_navigationService != null && _navigationService.CurrentPage != null)
                            {
                                await NavigationService.PushModalAsync(extraproductpage);
                            }
                        }
                    }

                    //start #94426 Product Optional Extras display enhancement By Pratik
                    else
                    {
                        try
                        {
                            if (_navigationService.IsFlyoutPage && NavigationService.ModalStack != null
                                && NavigationService.ModalStack.Count > 0 && NavigationService.ModalStack.Last() is ExtraProductPage)
                            {
                                return;
                            }
                            else
                            {
                                var productsLength = Invoice.InvoiceLineItems.Count();
                                var sequence = productsLength + 1;
                                var extrasequence = sequence;

                                Invoice = await InvoiceCalculations.CheckstockAndaddToCart(Invoice, offers, product, 1, productService, taxServices, null, sequence);

                                InvoiceLineItems = Invoice.InvoiceLineItems;
                                SendPeerNotification(Invoice);

                                //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                                if (AgeVerificationProofData != null && Invoice?.InvoiceLineItems?.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails.Select(subItem => subItem.Value)).FirstOrDefault() == null)
                                {
                                    var lineItem = Invoice.InvoiceLineItems?.LastOrDefault();
                                    lineItem.InvoiceLineItemDetails = new ObservableCollection<InvoiceLineItemDetailDto>();
                                    if (AgeVerificationProofData.Base64 != null)
                                        lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.ImageUpload, Value = AgeVerificationProofData.Base64, CreationTime = DateTime.Now.ToStoreTime() });
                                    if (AgeVerificationProofData.DateOfBirth != null)
                                        lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.BirthDate, Value = AgeVerificationProofData.DateOfBirth.ToString(), CreationTime = DateTime.Now.ToStoreTime() });
                                    if (AgeVerificationProofData.Age > 0)
                                        lineItem.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.AgeLimit, Value = AgeVerificationProofData.Age.ToString(), CreationTime = DateTime.Now.ToStoreTime() });

                                }
                                //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }
                    //end #94426 By Pratik
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                Helpers.Logger.SyncLogger("OpenExtraProudct - \n" + ex.Message + "\n" + ex.Message);
            }
        }

        async Task select_variant(ProductDto_POS product, decimal quantity)
        {
            if (product.ParentId.HasValue && (product.ProductExtras == null || (product.ProductExtras != null && !product.ProductExtras.Any())))
            {
                var parentProduct = productService.GetLocalProduct(product.ParentId.Value);
                if (parentProduct?.ProductExtras != null && parentProduct?.ProductExtras.Count() > 0)
                {
                    product.ProductExtras = parentProduct.ProductExtras;
                    productService.UpdateLocalProduct(product);
                }
            }
            if (product?.ProductExtras != null && product.ProductExtras.Any())
            {
                await OpenExtraProudct(product);
            }
            else
            {
                var productsLength = Invoice.InvoiceLineItems.Count();
                var sequence = productsLength + 1;
                Invoice = await InvoiceCalculations.CheckstockAndaddToCart(Invoice, offers, product, quantity, productService, taxServices, null, sequence);
                SendPeerNotification(Invoice);
                InvoiceLineItems = Invoice.InvoiceLineItems;

            }
        }

        #endregion

        #region offer methods
        async Task<bool> CheckCompositestockAndaddToCart(InvoiceDto invoice, OfferDto compositeOffer, int qunatity)
        {
            var haveStock = true;

            var hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemType == InvoiceItemType.Composite && x.InvoiceItemValue == compositeOffer.Id);
            if (hasInvoiceItem != null)
            {
                foreach (var item in hasInvoiceItem.InvoiceLineSubItems)
                {
                    if (haveStock)
                    {
                        var product = productService.GetLocalProduct(item.ItemId);
                        var checkvalidateQty = (hasInvoiceItem.Quantity * item.Quantity) + (qunatity * item.Quantity);

                        var result = await InvoiceCalculations.CheckCompositestockValidation(product, checkvalidateQty, 0);
                        if (!result.IsValid)
                            haveStock = false;

                    }
                }
            }
            else
            {
                foreach (var item in compositeOffer.OfferItems)
                {
                    if (haveStock)
                    {
                        var product = productService.GetLocalProduct(item.OfferOnId);
                        var checkvalidateQty = item.CompositeQty;
                        var result = await InvoiceCalculations.CheckCompositestockValidation(product, checkvalidateQty ?? 0, 0);
                        if (!result.IsValid)
                            haveStock = false;
                    }
                }
            }

            //if (!haveStock)
            //{
            //    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CompositeOutofStockMessage"), Colors.Red, Colors.White);
            //}
            //else
            //{
            //    await selectOffer(compositeOffer);
            //}
            return haveStock;
        }


        public async Task selectOffer(OfferDto offer)
        {
            try
            {
                if (offer.OfferType == OfferType.Composite)
                {
                    if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RegisterOpenAlertMessage"));
                        return;
                    }

                    if (Invoice == null)
                    {
                        await addNewSale();
                    }

                    bool IsValidStock = await CheckCompositestockAndaddToCart(Invoice, offer, 1);
                    if (!IsValidStock)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CompositeOutofStockMessage"), Colors.Red, Colors.White);
                        return;
                    }

                    var hasInvoiceItem = Invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemType == InvoiceItemType.Composite && x.InvoiceItemValue == offer.Id);
                    if (hasInvoiceItem != null)
                    {
                        hasInvoiceItem.Quantity += 1;
                        InvoiceCalculations.CalculateLineItemTotal(hasInvoiceItem, Invoice);
                        Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                        InvoiceLineItems = Invoice.InvoiceLineItems;
                        return;
                    }
                    decimal totalCost = 0;
                    foreach (var item in offer.OfferItems)
                    {
                        totalCost += (productService.GetLocalProductDB(item.OfferOnId)).ProductOutlet.CostPrice * item.CompositeQty ?? 0;
                    }

                    var invoiceItem = new InvoiceLineItemDto()
                    {
                        InvoiceItemType = InvoiceItemType.Composite,
                        InvoiceItemValue = offer.Id,
                        Sequence = 1,
                        Title = offer.Name,
                        Description = "",
                        Quantity = 1,
                        //Ticket #9578 Start: Composite Offer Icon not appearing issue. By Nikhil.
                        OffersNote = offer?.Description,
                        //Ticket #9578 End:By Nikhil. 
                        ItemCost = totalCost,
                        RetailPrice = offer.OfferAmount,
                        DiscountIsAsPercentage = true,
                        DiscountValue = 0,
                        ActionType = ActionType.Sell
                    };

                    if (offer.TaxID != null)
                    {
                        invoiceItem.TaxId = offer.TaxID ?? 1;
                        invoiceItem.TaxName = offer.TaxName;
                        invoiceItem.TaxRate = offer.TaxRate ?? 0;
                    }
                    else
                    {
                        invoiceItem.TaxId = 1;
                        invoiceItem.TaxName = "No Tax";
                        invoiceItem.TaxRate = 0;
                    }

                    invoiceItem.InvoiceLineSubItems = new ObservableCollection<InvoiceLineSubItemDto>();
                    foreach (var temp in offer.OfferItems)
                    {

                        var product = productService.GetLocalProductDB(temp.OfferOnId);

                        var subitem = new InvoiceLineSubItemDto
                        {
                            ItemId = product.Id,
                            ItemName = product.Name,
                            Quantity = temp.CompositeQty ?? 0
                        };
                        invoiceItem.InvoiceLineSubItems.Add(subitem);
                    }

                    //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                            Invoice.InvoiceLineItems.Add(invoiceItem);
                        else
                            Invoice.InvoiceLineItems.Insert(0, invoiceItem);
                    });
                    InvoiceLineItems = Invoice.InvoiceLineItems;
                    //End:#45375 .by rupesh
                    InvoiceCalculations.CalculateLineItemTotal(invoiceItem, Invoice);
                    Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                    InvoiceLineItems = Invoice.InvoiceLineItems;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async void OpenTax()
        {
            if (Invoice?.InvoiceLineItems?.Count > 0)
            {

                if (IsOpenTaxViewPopUp == 1)
                {
                    IsOpenTaxViewPopUp = 0;
                }
                else
                {
                    IsLoad = true;
                    Invoice.Taxgroup = await InvoiceCalculations.CalculateTax(Invoice);
                    IsLoad = false;
                    IsOpenTaxViewPopUp = 1;
                }
            }
        }

        async void RemoveInvoiceTaxItem(LineItemTaxDto tax)
        {
            if (tax != null)
            {
                //foreach (var item in Invoice.InvoiceLineItems)
                //Maui Changes remove tax logic
                foreach (var item in Invoice.InvoiceLineItems.Where(x => x.TaxId == tax.TaxId))
                {
                    //Maui Changes remove tax logic
                    // if (item.TaxId == tax.TaxId)
                    //{
                    //Ticket start:#78054 iOS: while remove tax at that time price changed.by rupesh
                    //if (Invoice.TaxInclusive)
                    //{
                    //    item.RetailPrice -= InvoiceCalculations.CalculateTaxInclusive(item.RetailPrice, item.TaxRate);
                    //}
                    //Ticket end:#78054 .by rupesh
                    item.TaxId = 1;
                    item.TaxName = "NoTax";
                    item.TaxRate = 0;
                    //}
                    InvoiceCalculations.CalculateLineItemTotal(item, Invoice, false);
                }
                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                Invoice.Taxgroup = await InvoiceCalculations.CalculateTax(Invoice);
                InvoiceLineItems = Invoice.InvoiceLineItems;
            }
        }


        #endregion
        bool _isDiscount { get; set; } = true;
        public bool isDiscount { get { return _isDiscount; } set { _isDiscount = value; SetPropertyChanged(nameof(isDiscount)); } }

        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
        bool _IsTaxListDisplay { get; set; } = true;
        public bool IsTaxListDisplay { get { return _IsTaxListDisplay; } set { _IsTaxListDisplay = value; SetPropertyChanged(nameof(IsTaxListDisplay)); } }
        bool _isShippingTaxListDisplay { get; set; } = true;
        public bool IsShippingTaxListDisplay { get { return _isShippingTaxListDisplay; } set { _isShippingTaxListDisplay = value; SetPropertyChanged(nameof(IsShippingTaxListDisplay)); } }
        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total


        string _PopupTitle { get; set; }
        public string PopupTitle { get { return _PopupTitle; } set { _PopupTitle = value; SetPropertyChanged(nameof(PopupTitle)); } }


        //#22607 iOS - Enable Shipping Option on POS Screen

        bool _IsShippingPurpose { get; set; }
        public bool IsShippingPurpose { get { return _IsShippingPurpose; } set { _IsShippingPurpose = value; SetPropertyChanged(nameof(IsShippingPurpose)); } }

        ObservableCollection<TaxDto> _taxList { get; set; }
        public ObservableCollection<TaxDto> TaxList
        {
            get { return _taxList; }
            set
            {
                _taxList = value;
                SetPropertyChanged(nameof(TaxList));
            }
        }

        TaxDto _selectedTax { get; set; }
        public TaxDto SelectedTax
        {
            get
            {
                return _selectedTax;
            }
            set
            {
                _selectedTax = value;
                SetPropertyChanged(nameof(SelectedTax));
            }
        }

        TaxDto _selectedShippingTax { get; set; }
        public TaxDto SelectedShippingTax
        {
            get
            {
                return _selectedShippingTax;
            }
            set
            {
                _selectedShippingTax = value;
                SetPropertyChanged(nameof(SelectedShippingTax));
            }
        }
        string Discountortip;
        //end #22607 iOS - Enable Shipping Option on POS Screen

        public void OpenDiscount(string discountortip)
        {
            Discountortip = discountortip;
            if (Invoice?.InvoiceLineItems?.Count > 0)
            {
                //Ticket #9969 Start : invoice discount disable for refund. By Nikhil
                if (discountortip == "Discount" && Invoice.Status == InvoiceStatus.Refunded)
                    return;
                //Ticket #9969 End : By Nikhil

                if (IsOpenDiscountPopUp == 1)
                {
                    IsOpenDiscountPopUp = 0;
                }
                else
                {
                    if (discountortip == "Discount")
                    {
                        IsTaxListDisplay = IsShippingTaxListDisplay = IsShippingPurpose = true;
                        isDiscount = true;
                        if (Invoice.DiscountIsAsPercentage)
                            DiscountType = "Percentage";
                        else
                            DiscountType = "Amount";

                        AmountValue = Invoice.DiscountValue == 0 ? "0" : Invoice.DiscountValue.ToString("0.####");
                        PopupTitle = LanguageExtension.Localize("DiscountText");
                    }
                    else if (discountortip == "ShippingCharge")
                    {
                        IsTaxListDisplay = true;
                        IsShippingTaxListDisplay = IsShippingPurpose = false;
                        isDiscount = false;
                        //if (Invoice.TipIsAsPercentage)
                        //    DiscountType = "Percentage";
                        //else
                        DiscountType = "Amount";

                        AmountValue = Invoice.TotalShippingCost == 0 ? "0" : Invoice.TotalShippingCost.ToString("0.####");
                        PopupTitle = LanguageExtension.Localize("ShippingCharge");

                        GetTaxList();
                        if (TaxList != null && Invoice.shippingTaxId != null)
                            SelectedShippingTax = TaxList.FirstOrDefault(x => x.Id == Invoice.shippingTaxId);

                    }
                    //"Tip"
                    else if (discountortip == "Tip")
                    {
                        IsShippingTaxListDisplay = true;
                        isDiscount = false;
                        IsTaxListDisplay = false;
                        IsShippingPurpose = true;
                        if (Invoice.TipIsAsPercentage)
                            DiscountType = "Percentage";
                        else
                            DiscountType = "Amount";

                        AmountValue = Invoice.TipValue == 0 ? "0" : Invoice.TipValue.ToString("0.####");
                        PopupTitle = LanguageExtension.Localize("TipPopupTitle");
                        GetTaxList();
                        if (TaxList != null && Invoice.TipTaxId != null)
                            SelectedTax = TaxList.FirstOrDefault(x => x.Id == Invoice.TipTaxId);

                    }
                    else
                    {
                        IsShippingTaxListDisplay = true;
                        IsTaxListDisplay = isDiscount = false;
                        IsShippingPurpose = true;
                        if (Invoice.TipIsAsPercentage)
                            DiscountType = "Percentage";
                        else
                            DiscountType = "Amount";

                        AmountValue = Invoice.TipValue.ToString("0.####");
                        PopupTitle = LanguageExtension.Localize("TipPopupTitle");
                    }

                    IsOpenDiscountPopUp = 1;
                }
            }
        }


        public void ChangeDiscountType(string type)
        {
            DiscountType = type;
        }
        //DiscountType

        void ApplyDiscount()
        {
            try
            {
                AmountValue = string.IsNullOrEmpty(AmountValue) ? "0" : AmountValue;
                if (DiscountType == "Amount")
                {
                    decimal amountValue = Convert.ToDecimal(AmountValue);
                    decimal TotalAmmount = 0;
                    //if (Invoice.TaxInclusive)
                    TotalAmmount = Invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
                    //else
                    //{
                    //    TotalAmmount = Invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
                    //    TotalAmmount += Math.Round(Invoice.TotalTax, 2);
                    //}



                    if (amountValue <= TotalAmmount.ToPositive())
                    {

                        if (isDiscount)
                        {
                            Invoice.DiscountValue = amountValue;
                            Invoice.DiscountIsAsPercentage = false;
                            Invoice.DiscoutType = "mannual";
                            //Ticket start #65869:iOS: User level discount limit permission not working. by rupesh
                            var maxDiscount = Settings.CurrentUser.MaximumDiscount;
                            var totalEffectiveamount = Invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
                            var discountPercent = InvoiceCalculations.GetPercentfromValue(amountValue, totalEffectiveamount);
                            if (maxDiscount > 0 && !SkipAdminAppoval && discountPercent > maxDiscount)
                            {
                                OpenAdminApprovalPage();
                                return;
                            }

                            SkipAdminAppoval = false;
                            //Ticket end #65869. by rupesh
                        }
                        else
                        {
                            if (!IsShippingPurpose)
                            {
                                ApplyShippingCost(SelectedShippingTax);
                            }
                            else
                            {
                                Invoice.TipValue = amountValue;
                                Invoice.TipIsAsPercentage = false;
                            }

                        }
                        Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                        if (!IsTaxListDisplay)
                            ApplyTipSurcharge(SelectedTax);
                        CloseDiscount();
                    }
                    else
                    {
                        if (isDiscount)
                        {
                            //Application.Current.MainPage.DisplayAlert("Authentication error", "Please enter valid discount", "Ok");
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("DiscountValidationMessage"));
                        }
                        else
                        {
                            //Application.Current.MainPage.DisplayAlert("Authentication error", "Please enter valid tip", "Ok");

                            if (!IsShippingPurpose)
                            {
                                ApplyShippingCost(SelectedShippingTax);
                                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                                CloseDiscount();
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("TipValidationMessage"));
                            }
                        }
                    }
                }
                else
                {
                    if (Convert.ToDecimal(AmountValue) > 100)
                    {
                        if (isDiscount)
                        {
                            //Application.Current.MainPage.DisplayAlert("Authentication error", "Please enter valid discount", "Ok");
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("DiscountValidationMessage"));
                        }
                        else
                        {
                            //Application.Current.MainPage.DisplayAlert("Authentication error", "Please enter valid tip", "Ok");
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("TipValidationMessage"));
                        }
                    }
                    else
                    {

                        if (isDiscount)
                        {
                            Invoice.DiscountValue = Convert.ToDecimal(AmountValue);
                            Invoice.DiscountIsAsPercentage = true;
                            Invoice.DiscoutType = "mannual";
                            //Ticket start #65869:iOS: User level discount limit permission not working. by rupesh
                            var maxDiscount = Settings.CurrentUser.MaximumDiscount;
                            if (maxDiscount > 0 && !SkipAdminAppoval && Invoice.DiscountValue > maxDiscount)
                            {
                                OpenAdminApprovalPage();
                                return;
                            }                          
                            SkipAdminAppoval = false;
                            //Ticket end #65869. by rupesh
                        }
                        else
                        {
                            Invoice.TipValue = Convert.ToDecimal(AmountValue);

                            Invoice.TipIsAsPercentage = true;
                        }
                        Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                        if (!IsTaxListDisplay)
                            ApplyTipSurcharge(SelectedTax);
                        CloseDiscount();
                    }
                }
                InvoiceLineItems = Invoice.InvoiceLineItems;
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }
        //Ticket start #65869:iOS: User level discount limit permission not working. by rupesh
        void OpenAdminApprovalPage()
        {
            if (ApproveAdminPage == null)
            {
                ApproveAdminPage = new ApproveAdminPage();
                ApproveAdminPage.ViewModel.Users = new ObservableCollection<UserListDto>(ApproveAdminPage.ViewModel.Users);
                ApproveAdminPage.SelectedUser += async (object sender, UserListDto e) =>
                {
                    if (Invoice != null)
                    {
                        Invoice.approvedByUser = e.Id;
                        Invoice.approvedByUserName = e.FullName;
                    }
                    await ApproveAdminPage.Close();
                    SkipAdminAppoval = true;
                    ApplyDiscount();
                };
            }
            ServiceLocator.Get<INavigationService>().GetCurrentPage.Navigation.PushModalAsync(ApproveAdminPage);

        }
        //Ticket end #65869. by rupesh
        void DeleteDiscount()
        {
            if (isDiscount)
            {
                Invoice.DiscountValue = 0;
            }
            else
            {
                Invoice.TipValue = 0;
            }
            Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
            InvoiceLineItems = Invoice.InvoiceLineItems;
            CloseDiscount();
        }

        void CloseDiscount()
        {
            if (IsOpenDiscountPopUp == 1)
            {
                SelectedTax = SelectedShippingTax = null;
                IsOpenDiscountPopUp = 0;
            }
        }

        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
        public void ApplyTipSurcharge(TaxDto taxDto)
        {
            try
            {

                if (taxDto == null)
                {
                    if (taxServices != null)
                    {
                        var defaultTax = Extensions.GetDefaultTaxRecord(taxServices);
                        taxDto = Extensions.defaultTax;
                    }

                }

                if (taxDto != null)
                {
                    Invoice.TipTaxName = taxDto.Name;
                    Invoice.TipTaxRate = taxDto.Rate;
                    Invoice.TipTaxId = taxDto.Id;

                    Invoice.TipTaxAmount = InvoiceCalculations.CalculateTaxInclusive(Invoice.TotalTip, taxDto.Rate);
                    Invoice.SurchargeDisplayValuecal = Invoice.TotalTip - Invoice.TipTaxAmount;

                    //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
                    //Invoice.TotalTax = Invoice.TotalTax + Invoice.TipTaxAmount;
                    return;

                    if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.ApplySurchargeOnTaxInclusiveTotal)
                    {
                        if (Invoice.TipIsAsPercentage)
                        {
                            //Invoice.TipTaxAmount = InvoiceCalculations.GetValuefromPercent(Invoice.TotalTip, taxDto.Rate);

                            //   calculateTaxInclusive


                            //Invoice.TipTaxAmount = (Invoice.TotalTip * taxDto.Rate) / (100 + taxDto.Rate);
                            Invoice.TipTaxAmount = InvoiceCalculations.CalculateTaxInclusive(Invoice.TotalTip, taxDto.Rate);
                            Invoice.SurchargeDisplayValuecal = Invoice.TotalTip - Invoice.TipTaxAmount;


                        }
                        else
                        {
                            Invoice.TipTaxAmount = (Invoice.TotalTip * taxDto.Rate) / (100 + taxDto.Rate);

                            Invoice.SurchargeDisplayValuecal = Invoice.TipValue - Invoice.TipTaxAmount;
                        }
                    }
                    else
                    {
                        if (Invoice.TipIsAsPercentage)
                        {
                            //Invoice.TipTaxAmount = InvoiceCalculations.GetValuefromPercent(Invoice.TotalTip, taxDto.Rate);
                            Invoice.TipTaxAmount = (Invoice.TotalTip * taxDto.Rate) / (100 + taxDto.Rate);
                            Invoice.SurchargeDisplayValuecal = Invoice.TotalTip - Invoice.TipTaxAmount;

                        }
                        else
                        {
                            Invoice.TipTaxAmount = (Invoice.TotalTip * taxDto.Rate) / (100 + taxDto.Rate);

                            Invoice.SurchargeDisplayValuecal = Invoice.TipValue - Invoice.TipTaxAmount;
                        }
                    }
                }




            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while ApplyShippingCost" + ex.ToString());
            }
        }

        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total

        public void ApplyShippingCost(TaxDto taxDto)
        {
            try
            {
                if (taxDto == null)
                {
                    if (taxServices != null)
                    {
                        var defaultTax = Extensions.GetDefaultTaxRecord(taxServices);
                        taxDto = Extensions.defaultTax;
                    }

                }

                Invoice.shippingTaxId = taxDto.Id;
                Invoice.ShippingTaxName = taxDto.Name;
                Invoice.ShippingTaxRate = taxDto.Rate;
                Invoice.TotalShippingCost = Convert.ToDecimal(AmountValue);


                Invoice.ShippingTaxAmount = InvoiceCalculations.CalculateTaxInclusive(Invoice.TotalShippingCost, taxDto.Rate);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while ApplyShippingCost" + ex.ToString());
            }

        }

        //Ticket start:#22406 Quote sale. by rupesh
        public async Task DuplicateFromQuoteFromHistory(InvoiceDto invoice)
        {
            try
            {
                Invoice = invoice.Copy();
                Invoice.Id = 0;
                Invoice.InvoiceTempId = null;
                Invoice.isSync = false;
                Invoice.Number = null;
                Invoice.Barcode = null;
                Invoice.Status = InvoiceStatus.initial;
                Invoice.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                Invoice.TransactionDate = Extensions.moment();
                //Ticket start:#50092 iOS: Date issue on the Receipt.by rupesh
                Invoice.CreationTime = Invoice.TransactionDate;
                //Ticket end:#50092 .by rupesh
                Invoice.TotalPaid = 0;
                Invoice.TotalTender = 0;
                Invoice.ChangeAmount = 0;
                Invoice.CustomerId = null;
                Invoice.CustomerName = null;
                Invoice.CustomerTempId = null;
                Invoice.CustomerDetail = null;
                Invoice.CustomerGroupDiscount = null;
                Invoice.CustomerGroupDiscountNote = null;
                Invoice.CustomerGroupDiscountNoteInside = null;
                Invoice.CustomerGroupDiscountNoteInsidePrice = 0;
                Invoice.CustomerGroupId = null;
                Invoice.CustomerGroupName = null;
                Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                Invoice.ReferenceInvoiceId = invoice.Id;
                if (invoice.InvoicePayments != null)
                    Invoice.ToRefundPayments = invoice.InvoicePayments;
                //refundInvoice.ReferenceStatus = SelectedSale.Status;

                foreach (var item in Invoice.InvoiceLineItems)
                {
                    //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                    var localprod = productService.GetLocalProductDB(item.InvoiceItemValue);
                    if (localprod != null)
                    {
                        item.DisableDiscountIndividually = localprod.DisableDiscountIndividually;
                    }
                    //End #92641 by Pratik

                    //Ticket:start:#90938, IOS:FR Age varification.by rupesh
                    item.InvoiceLineItemDetails = null;
                    //Ticket:end:#90938 .by rupesh

                    if (item.InvoiceItemType != InvoiceItemType.GiftCard)
                    {
                        item.InvoiceId = 0;
                        item.Id = 0;
                        item.CustomerGroupDiscountPercent = null;
                        item.CustomerGroupLoyaltyPoints = 0;
                        InvoiceCalculations.CalculateLineItemTotal(item, Invoice);
                    }

                    //Ticket start:#23855 iOS - Discount Not Applied When Editing Quotes.by rupesh
                    try
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
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                    //Ticket end:#23855 .by rupesh

                }
                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                InvoiceLineItems = Invoice.InvoiceLineItems;
                //Start #45654 FR - Bypassing the Who is Selling screen - Pratik
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EachUserHasAUniquePinCode)
                {
                    await ByPassWhoSelling();
                }
                else if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.SwitchUserAfterEachSale)
                {
                    //End #45654 Pratik
                    if (allUserPage == null)
                    {
                        allUserPage = new AllUserPage();
                        allUserPage.ViewModel.SelectedUser += async (object sender, UserListDto e) =>
                        {
                            await allUserPage.ViewModel.Close();
                            EnterSalePage.ServedBy = e;
                        };
                    }

                    await NavigationService.PushModalAsync(allUserPage);
                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        public async Task<bool> ConvertToSaleFromQuoteFromHistory(InvoiceDto invoice)
        {
            try
            {
                Invoice = invoice.Copy();
                Invoice.Id = 0;
                Invoice.InvoiceTempId = null;
                Invoice.isSync = false;
                Invoice.Number = null;
                Invoice.Barcode = null;
                Invoice.Status = InvoiceStatus.initial;
                Invoice.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                Invoice.TransactionDate = Extensions.moment();
                //Ticket start:#50092 iOS: Date issue on the Receipt.by rupesh
                Invoice.CreationTime = Invoice.TransactionDate;
                //Ticket end:#50092 .by rupesh
                Invoice.TotalPaid = 0;
                Invoice.TotalTender = 0;
                Invoice.ChangeAmount = 0;
                Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                Invoice.ReferenceInvoiceId = invoice.Id;
                if (invoice.InvoicePayments != null)
                    Invoice.ToRefundPayments = invoice.InvoicePayments;

                foreach (var item in Invoice.InvoiceLineItems)
                {
                    //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                    var localprod = productService.GetLocalProductDB(item.InvoiceItemValue);
                    if (localprod != null)
                    {
                        item.DisableDiscountIndividually = localprod.DisableDiscountIndividually;
                    }
                    //End #92641 by Pratik
                    item.InvoiceLineItemDetails = null;  //Ticket:start:#90938 IOS:FR Age varification.by rupesh

                    if (item.InvoiceItemType != InvoiceItemType.GiftCard)
                    {
                        item.InvoiceId = 0;
                        item.Id = 0;
                        InvoiceCalculations.CalculateLineItemTotal(item, Invoice);
                    }

                    //Ticket start:#23855 iOS - Discount Not Applied When Editing Quotes.by rupesh
                    try
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
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                    //Ticket end:#23855 .by rupesh
                }
                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                InvoiceLineItems = Invoice.InvoiceLineItems;
                if (Invoice.CustomerId != null && Invoice.CustomerId > 1)
                {
                    CustomerDto_POS cust = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                    if (cust != null)
                    {
                        CustomerModel.SelectedCustomer = cust;
                    }
                }
                //Start #45654 FR - Bypassing the Who is Selling screen - Pratik
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EachUserHasAUniquePinCode)
                {
                    await ByPassWhoSelling();
                }
                else if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.SwitchUserAfterEachSale)
                {
                    //End #45654 Pratik
                    var tcs = new TaskCompletionSource<bool>();
                    if (allUserPage == null)
                    {
                        allUserPage = new AllUserPage();
                    }
                    allUserPage.ViewModel.SelectedUser += async (object sender, UserListDto e) =>
                    {
                        await allUserPage.ViewModel.Close();
                        EnterSalePage.ServedBy = e;
                        tcs.SetResult(true);
                        tcs = null;
                        if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                            await NavigationService.PopModalAsync();
                        allUserPage = null;

                    };

                    await NavigationService.PushModalAsync(allUserPage);
                    return await tcs.Task;
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }
        //Ticket end:#22406 Quote sale. by rupesh

        //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
        #region search/select deliverAddress methods
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
        #endregion
        //Ticket end:#26664 .by rupesh

        public void SendPeerNotification(InvoiceDto invoice)
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS && !string.IsNullOrEmpty(Settings.CustomerAppConfigFrom) && Settings.CustomerAppConfigFrom == Models.Enum.CustomerDisplayConfigureType.ActivateForIPad.ToString())
            {
                if (peerCommunicatorViewModel == null)
                    peerCommunicatorViewModel = new PeerCommunicatorViewModel(saleService);
                peerCommunicatorViewModel.StartPeerConnection();

                Debug.WriteLine("Peer Invoice : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoice));
                peerCommunicatorViewModel.SendPeerNotification(Invoice, offers);
            }

        }

        public bool CheckForSerialNo()
        {
            try
            {
                if (Invoice?.InvoiceLineItems == null)
                    return false;

                var invoiceLineItems = Invoice.InvoiceLineItems;

                //Check if serial number is missing
                var missingSerialNoItems = invoiceLineItems.Where(x => x.EnableSerialNumber && string.IsNullOrEmpty(x.SerialNumber));
                if (missingSerialNoItems != null && missingSerialNoItems.Any())
                {
                    var msg = string.Format(LanguageExtension.Localize("Please enter Serial Number for {0}"), missingSerialNoItems.First().Title);
                    App.Instance.Hud.DisplayToast(msg, Colors.Red, Colors.White);
                    return false;
                }

                //Check if same product has same serial number
                string duplicateSerialNo = string.Empty;
                var query = invoiceLineItems.Where(x => x.EnableSerialNumber)
                    .GroupBy(y => y.InvoiceItemValue, y => y.SerialNumber);
                foreach (var group in query)
                {
                    var duplicates = group.GroupBy(x => x).Where(y => y.Count() > 1).Select(z => z.Key).ToList();
                    if (duplicates != null && duplicates.Any())
                    {
                        duplicateSerialNo = duplicates.FirstOrDefault();
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(duplicateSerialNo))
                {
                    var msg = string.Format(LanguageExtension.Localize("Duplicate serial number: {0}. This serial number item is already added to the cart."), duplicateSerialNo);
                    App.Instance.Hud.DisplayToast(msg, Colors.Red, Colors.White);
                    return false;
                }


            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in CheckForSerialNo : " + ex.Message);
                ex.Track();
                App.Instance.Hud.DisplayToast("Something went wrong while checking Serial Numbers.", Colors.Red, Colors.White);
                return false;
            }
            return true;
        }

        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
        async Task<AgeVerificationEventArgs> CheckForAgeVerifcation(int age)
        {
            var tcs = new TaskCompletionSource<AgeVerificationEventArgs>();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var ageVerificationPopupPage = new AgeVerificationPopupPage();
                ageVerificationPopupPage.Age = age;
                ageVerificationPopupPage.Confirmed += (object sender, AgeVerificationEventArgs e) =>
                {
                    tcs.SetResult(e);
                };
                await NavigationService.PushModalAsync(ageVerificationPopupPage);
            });
            return await tcs.Task;
        }
        //Ticket:end:#90938 .by rupesh

        //Ticket start:#91260 iOS:FR:Duplicating invoices.by rupesh
        public async Task<bool> DuplicateInvoiceFromHistory(InvoiceDto invoice)
        {
            try
            {
                Invoice = invoice.Copy();
                Invoice.Id = 0;
                Invoice.InvoiceTempId = null;
                Invoice.isSync = false;
                Invoice.Number = null;
                Invoice.Barcode = null;
                if (Invoice.InvoiceLineItems.Any(x => x.InvoiceItemType == InvoiceItemType.GiftCard))
                    Invoice.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.GiftCard));
                if (Settings.CurrentRegister != null && Settings.CurrentRegister.Registerclosure != null)
                {
                    Invoice.RegisterId = Settings.CurrentRegister.Id;
                    Invoice.RegisterName = Settings.CurrentRegister.Name;
                    Invoice.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id;
                    Invoice.OutletId = Settings.SelectedOutletId;
                }

                Invoice.InvoiceFloorTables = null; //#94565
                Invoice.FloorTableName = string.Empty; //#94565

                Invoice.Status = InvoiceStatus.initial;
                Invoice.InvoiceFrom = DeviceInfo.Platform == DevicePlatform.iOS ? InvoiceFrom.iPad : InvoiceFrom.Android;
                Invoice.TransactionDate = Extensions.moment();
                Invoice.CreationTime = Invoice.TransactionDate;
                Invoice.TotalPaid = 0;
                Invoice.TotalTender = 0;
                Invoice.ChangeAmount = 0;
                Invoice.RoundingAmount = 0;
                Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                Invoice.InvoiceHistories = new ObservableCollection<InvoiceHistoryDto>();
                if (invoice.InvoicePayments != null)
                    Invoice.ToRefundPayments = invoice.InvoicePayments;

                foreach (var item in Invoice.InvoiceLineItems)
                {
                    if (Settings.CurrentRegister != null && Settings.CurrentRegister.Registerclosure != null)
                    {
                        item.RegisterId = Settings.CurrentRegister.Id;
                        item.RegisterName = Settings.CurrentRegister.Name;
                        item.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id;
                    }
                    if (item.InvoiceItemType != InvoiceItemType.GiftCard)
                    {
                        var product = productService.GetLocalProduct(item.InvoiceItemValue);
                        if (product != null)
                        {
                            //Start #92641 iOS: correct "Exclude this product from any and all discount offers" same as web By Pratik
                            item.DisableDiscountIndividually = product.DisableDiscountIndividually;
                            //End #92641 by Pratik
                            if (product.AgeVerificationToSellProduct && Invoice?.InvoiceLineItems?.Where(x => x.InvoiceLineItemDetails != null).SelectMany(x => x.InvoiceLineItemDetails.Select(subItem => subItem.Value)).FirstOrDefault() != null)
                            {
                                AgeVerificationProofData = await CheckForAgeVerifcation(product.AgeVerificationLimit);
                                if (AgeVerificationProofData == null)
                                    return false;
                                item.InvoiceLineItemDetails = new ObservableCollection<InvoiceLineItemDetailDto>();
                                if (AgeVerificationProofData.Base64 != null)
                                    item.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.ImageUpload, Value = AgeVerificationProofData.Base64, CreationTime = DateTime.Now.ToStoreTime() });
                                if (AgeVerificationProofData.DateOfBirth != null)
                                    item.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.BirthDate, Value = AgeVerificationProofData.DateOfBirth.ToString(), CreationTime = DateTime.Now.ToStoreTime() });
                                if (AgeVerificationProofData.Age > 0)
                                    item.InvoiceLineItemDetails.Add(new InvoiceLineItemDetailDto { Key = InvoiceItemAgeVerification.AgeLimit, Value = AgeVerificationProofData.Age.ToString(), CreationTime = DateTime.Now.ToStoreTime() });

                            }
                        }
                        item.InvoiceId = 0;
                        item.Id = 0;
                        InvoiceCalculations.CalculateLineItemTotal(item, Invoice);

                        try
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
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }
                }
                Invoice = InvoiceCalculations.CalculateInvoiceTotal(Invoice, offers, productService);
                InvoiceLineItems = Invoice.InvoiceLineItems;
                if (Invoice.CustomerId != null && Invoice.CustomerId > 1)
                {
                    CustomerDto_POS cust = customerService.GetLocalCustomerById(Invoice.CustomerId.Value);
                    if (cust != null)
                    {
                        CustomerModel.SelectedCustomer = cust;
                    }
                }
                //Start #45654 FR - Bypassing the Who is Selling screen - Pratik
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EachUserHasAUniquePinCode)
                {
                    return await ByPassWhoSelling();

                }
                else if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.SwitchUserAfterEachSale)
                {
                    //End #45654 Pratik
                    var tcs = new TaskCompletionSource<bool>();
                    if (allUserPage == null)
                    {
                        allUserPage = new AllUserPage();
                    }
                    allUserPage.ViewModel.SelectedUser += async (object sender, UserListDto e) =>
                    {
                        await allUserPage.ViewModel.Close();
                        EnterSalePage.ServedBy = e;
                        tcs.SetResult(true);
                        tcs = null;
                        if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                            await NavigationService.PopModalAsync();
                        allUserPage = null;

                    };

                    await NavigationService.PushModalAsync(allUserPage);
                    return await tcs.Task;

                }
                else
                {
                    return true;

                }
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
