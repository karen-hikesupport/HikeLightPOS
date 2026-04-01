using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Fusillade;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using HikePOS.Services.PubNub;
using Newtonsoft.Json;
using System.Diagnostics;
using HikePOS.Interfaces;
using System.Globalization;
using HikePOS.Enums;
using HikePOS.Services.Payment;
using System.Reactive.Linq;
using HikePOS.Resources;
using HikePOS.Resx;
using System.Timers;
using CommunityToolkit.Mvvm.Messaging;
using Android.Widget;

namespace HikePOS.ViewModels
{
    public class EnterSaleViewModel : BaseViewModel
    {

        public static bool IsSaleSucceess;

        public static bool Isfirstcall = true;

        //Start ticket #60456 Pratik
        public PaymentPage paymentpage;
        public PaymentPagePhone paymentpagePhone;
        public EnterSalePage EnterSalePage;
        public EnterSalePagePhone EnterSalePagePhone;
        //End ticket #60456 Pratik

        ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
        ProductServices productService; //= new ProductServices(productApiService);

        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;

        ApiService<IOfferApi> offerApiService = new ApiService<IOfferApi>();
        OfferServices offerService;

        ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        PaymentServices paymentService;

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        SaleServices saleService;

        ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
        OutletServices outletService;

        ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
        UserServices userService;

        ApiService<ITaxApi> taxApiService = new ApiService<ITaxApi>();
        TaxServices taxServices;

        //#94565
        ApiService<IRestaurantApi> RestaurantApiService = new ApiService<IRestaurantApi>();
        RestaurantService RestaurantService;
        //#94565

        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        bool waiting = false;

        #region Public Properties

        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        bool _isServedBy;
        public bool IsServedBy { get { return _isServedBy; } set { _isServedBy = value; SetPropertyChanged(nameof(IsServedBy)); } }
        //end #84287 .by Pratik

        public double FirstColumn => ((DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) * 65) / 100;
        public double SecondColumn => ((DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) * 35) / 100;
        public double ProductColumn => (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 101.7;

        public OutletDto_POS CurrentOutlet { get; set; }
        public IEnumerable<PaymentOptionDto> AllPaymentOptionList { get; set; }

        private Color _mainGridBackgroundColor = Resources.AppColors.MainBackgroundColor;
        public Color MainGridBackgroundColor { get { return _mainGridBackgroundColor; } set { _mainGridBackgroundColor = value; SetPropertyChanged(nameof(MainGridBackgroundColor)); } }

        private Color _quoteBtnColor = Colors.Transparent;
        public Color QuoteBtnColor { get { return _quoteBtnColor; } set { _quoteBtnColor = value; SetPropertyChanged(nameof(QuoteBtnColor)); } }

        private Color _backorderBtnColor = Colors.Transparent;
        public Color BackorderBtnColor { get { return _backorderBtnColor; } set { _backorderBtnColor = value; SetPropertyChanged(nameof(BackorderBtnColor)); } }

        private Color _processSaleColor = Resources.AppColors.HikeColor;
        public Color ProcessSaleColor { get { return _processSaleColor; } set { _processSaleColor = value; SetPropertyChanged(nameof(ProcessSaleColor)); } }

        private string _lblPayText = LanguageExtension.Localize("PayButtonText");
        public string LblPayText { get { return _lblPayText; } set { _lblPayText = value; SetPropertyChanged(nameof(LblPayText)); } }

        private bool _isPaymentSummary = false;
        public bool IsPaymentSummary { get { return _isPaymentSummary; } set { _isPaymentSummary = value; SetPropertyChanged(nameof(IsPaymentSummary)); } }
        private bool _isNotes = false;
        public bool IsNotes { get { return _isNotes; } set { _isNotes = value; SetPropertyChanged(nameof(IsNotes)); } }

        private bool _deliveryCustomerVisible = true;
        public bool DeliveryCustomerVisible { get { return _deliveryCustomerVisible; } set { _deliveryCustomerVisible = value; SetPropertyChanged(nameof(DeliveryCustomerVisible)); } }

        private bool _discountEnabled = true;
        public bool DiscountEnabled { get { return _discountEnabled; } set { _discountEnabled = value; SetPropertyChanged(nameof(DiscountEnabled)); } }

        private bool _taxEnabled = true;
        public bool TaxEnabled { get { return _taxEnabled; } set { _taxEnabled = value; SetPropertyChanged(nameof(TaxEnabled)); } }

        private double _expandHeight = 100;
        public double ExpandHeight { get { return _expandHeight; } set { _expandHeight = value; SetPropertyChanged(nameof(ExpandHeight)); } }
        bool _displayFolder = false;
        public bool DisplayFolder { get { return _displayFolder; } set { _displayFolder = value; SetPropertyChanged(nameof(DisplayFolder)); } }

        ColorType? _layoutColor;
        public ColorType? LayoutColor { get { return _layoutColor; } set { _layoutColor = value; SetPropertyChanged(nameof(LayoutColor)); } }

        string _selectedCategoryName;
        public string SelectedCategoryName { get { return _selectedCategoryName; } set { _selectedCategoryName = value; SetPropertyChanged(nameof(SelectedCategoryName)); } }

        private bool _tipVisible = false;
        public bool TipVisible { get { return _tipVisible; } set { _tipVisible = value; SetPropertyChanged(nameof(TipVisible)); } }

        private bool _shippingVisible = true;
        public bool ShippingVisible { get { return _shippingVisible; } set { _shippingVisible = value; SetPropertyChanged(nameof(ShippingVisible)); } }

        private bool _editCustomerVisible = true;
        public bool EditCustomerVisible { get { return _editCustomerVisible; } set { _editCustomerVisible = value; SetPropertyChanged(nameof(EditCustomerVisible)); } }

        private bool _removeCustomerVisible = true;
        public bool RemoveCustomerVisible { get { return _removeCustomerVisible; } set { _removeCustomerVisible = value; SetPropertyChanged(nameof(RemoveCustomerVisible)); } }

        private bool _noteIsFocused = false;
        public bool NoteIsFocused { get { return _noteIsFocused; } set { _noteIsFocused = value; SetPropertyChanged(nameof(NoteIsFocused)); } }

        private bool _isActive = false;
        public bool IsActive { get { return _isActive; } set { _isActive = value; SetPropertyChanged(nameof(IsActive)); } }

        //Start ticket #76209 iOS:FR: User Permission : Add a permission for allowing 'Issue a Quote' By Pratik
        bool _isQuoteDisplay = false;
        public bool IsQuoteDisplay { get { return _isQuoteDisplay; } set { _isQuoteDisplay = value; SetPropertyChanged(nameof(IsQuoteDisplay)); SetPropertyChanged(nameof(IsQuoteOrBackorderDisplay)); } }
        //End ticket #76209 By Pratik
        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
        bool _isBackorderDisplay = false;
        public bool IsBackorderDisplay { get { return _isBackorderDisplay; } set { _isBackorderDisplay = value; SetPropertyChanged(nameof(IsBackorderDisplay)); SetPropertyChanged(nameof(IsQuoteOrBackorderDisplay)); } }
        public bool IsQuoteOrBackorderDisplay { get { return false; } } //((IsQuoteDisplay || IsBackorderDisplay) && !Settings.IsRestaurantPOS); } } //#94565
        //Ticket end:#92764.by rupesh

        //#38783 iPad: Feature request - Register's Name in Process Sale
        string _OutletName { get; set; }
        public string OutletName { get { return _OutletName; } set { _OutletName = value; SetPropertyChanged(nameof(OutletName)); } }


        string _RegisterName { get; set; }
        public string RegisterName { get { return _RegisterName; } set { _RegisterName = value; SetPropertyChanged(nameof(RegisterName)); } }

        //Start #90940 ios :FR Batch number on cash register By Pratik
        string _batchNumber;
        public string BatchNumber { get { return _batchNumber; } set { _batchNumber = value; SetPropertyChanged(nameof(BatchNumber)); } }
        //End #90940 By Pratik

        bool _IsMultipleRegister { get; set; }
        public bool IsMultipleRegister { get { return _IsMultipleRegister; } set { _IsMultipleRegister = value; SetPropertyChanged(nameof(IsMultipleRegister)); } }

        //#38783 iPad: Feature request - Register's Name in Process Sale

        CategoryDto _SelectedCategory { get; set; } = new CategoryDto();
        public CategoryDto SelectedCategory
        {
            get { return _SelectedCategory; }
            set
            {
                //if (!IsOpenRegister)
                //	return;
                SelectedCategory.IsSelected = false;
                SelectedCategory.IsLoad = false;
                _SelectedCategory = value;
                if (value != null)
                {
                    SelectedCategory.IsSelected = true;
                    SelectedCategory.IsLoad = false;
                    SetPropertyChanged(nameof(SelectedCategory));
                    if (!Isfirstcall)
                        loadProduct();
                    else
                        Isfirstcall = false;
                }
            }
        }

        public InvoiceLayoutSellDto InvoiceLayoutSellDetail { get; set; }


        public IList<CategoryDto> AllCategories { get; set; }


        ObservableCollection<CategoryDto> _Categories { get; set; }
        public ObservableCollection<CategoryDto> Categories { get { return _Categories; } set { _Categories = value; SetPropertyChanged(nameof(Categories)); } }

        ObservableCollection<EnterSaleItemDto> _EnterSaleItems { get; set; }
        public ObservableCollection<EnterSaleItemDto> EnterSaleItems { get { return _EnterSaleItems; } set { _EnterSaleItems = value; SetPropertyChanged(nameof(EnterSaleItems)); } }

        ObservableCollection<ProductDto_POS> _AllProducts { get; set; }
        public ObservableCollection<ProductDto_POS> AllProducts { get { return _AllProducts; } set { _AllProducts = value; InvoiceCalculations.Products = value; } }

        //Ticket Start:#20064 Unit of measurement feature for iPad app.by rupesh
        ObservableCollection<ProductUnitOfMeasureDto> _AllUnitOfMeasures { get; set; }
        public ObservableCollection<ProductUnitOfMeasureDto> AllUnitOfMeasures { get { return _AllUnitOfMeasures; } set { _AllUnitOfMeasures = value; } }
        //Ticket End:#20064 .by rupesh

        ObservableCollection<OfferDto> _Offers { get; set; }
        public ObservableCollection<OfferDto> Offers
        {
            get { return _Offers; }
            set { _Offers = value; if (invoicemodel != null) { invoicemodel.offers = value; } SetPropertyChanged(nameof(Offers)); }
        }

        ObservableCollection<ProductDto_POS> _SearchProducts { get; set; }
        public ObservableCollection<ProductDto_POS> SearchProducts { get { return _SearchProducts; } set { _SearchProducts = value; SetPropertyChanged(nameof(SearchProducts)); } }


        static bool _isOpenRegister { get; set; }
        public bool IsOpenRegister { get { return _isOpenRegister; } set { _isOpenRegister = value; SetPropertyChanged(nameof(IsOpenRegister)); } }
        //Ticket start:#74634 iOS FR: Pin Restriction.by rupesh
        static bool _isOpenRegisterPermission { get; set; }
        public bool IsOpenRegisterPermission { get { return _isOpenRegisterPermission; } set { _isOpenRegisterPermission = value; SetPropertyChanged(nameof(IsOpenRegisterPermission)); } }
        //Ticket end:#74634.by rupesh

        int _IsOpenBackground { get; set; } = 0;
        public int IsOpenBackground
        {
            get
            {
                return _IsOpenBackground;

            }
            set
            {
                _IsOpenBackground = value;
                SetPropertyChanged(nameof(IsOpenBackground));
            }
        }

        InvoiceViewModel _invoicemodel { get; set; }
        public InvoiceViewModel invoicemodel
        {
            get { return _invoicemodel; }
            set
            {
                _invoicemodel = value;
                SetPropertyChanged(nameof(invoicemodel));
            }
        }


        public InvoiceLineItemDto _SelectedInvoiceItem { get; set; } = null;
        public InvoiceLineItemDto SelectedInvoiceItem
        {
            get
            {
                return _SelectedInvoiceItem;
            }
            set
            {
                _SelectedInvoiceItem = value;
                SetPropertyChanged(nameof(SelectedInvoiceItem));
                if (value != null && value.isEnable)
                {
                    SelectInvoiceItem(value);
                }
                else
                {
                    _SelectedInvoiceItem = null;
                    SetPropertyChanged(nameof(SelectedInvoiceItem));
                }

            }
        }

        //Ticket #7760 Start:Rounding issue in Arabic language.By Nikhil.
        bool isArabicCulture { get; set; } = false;
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
        //Ticket #7760 End:By Nikhil. 
        bool _isPhoneGridVisible = true;
        public bool IsPhoneGridVisible
        {
            get
            {
                return _isPhoneGridVisible;
            }
            set
            {
                _isPhoneGridVisible = value;
                SetPropertyChanged(nameof(IsPhoneGridVisible));
            }
        }

        // public SignalRListener signalRListener;

        GeneralRuleDto _shopGeneralRule { get; set; } = Settings.StoreGeneralRule;
        public GeneralRuleDto ShopGeneralRule { get { return _shopGeneralRule; } set { _shopGeneralRule = value; SetPropertyChanged(nameof(ShopGeneralRule)); SetPropertyChanged(nameof(invoicemodel)); } }


        //#32357 iPad :: Feature request :: Show Item Count on POS Screen
        static bool _isItemCountVisible { get; set; } = Settings.StoreGeneralRule.ShowTotalQuantityOfItemsInBasket;
        public bool IsItemCountVisible { get { return _isItemCountVisible; } set { _isItemCountVisible = value; SetPropertyChanged(nameof(IsItemCountVisible)); } }
        //#32357 iPad :: Feature request :: Show Item Count on POS Screen


        SearchProductPage searchProductPage;
        ProductDetailPage productdetail;
        OpenRegisterPage openRegisterPage;


        //Ticket start:#37205 iOS - Sorting Products on Process Sale Page.by rupesh
        string _SelectedSortingMenu { get; set; } = "ProductNameAtoZ";
        public string SelectedSortingMenu { get { return _SelectedSortingMenu; } set { _SelectedSortingMenu = value; SetPropertyChanged(nameof(SelectedSortingMenu)); } }
        int _IsOpenSortingFilterPopUp { get; set; } = 0;
        public int IsOpenSortingFilterPopUp { get { return _IsOpenSortingFilterPopUp; } set { _IsOpenSortingFilterPopUp = value; SetPropertyChanged(nameof(IsOpenSortingFilterPopUp)); } }
        //Ticket end:#37205 .by rupesh

        //Ticket start:#37205 iOS - Sorting Products on Process Sale Page.by rupesh
        static bool _isHavingSortingEnabled { get; set; }
        public bool IsHavingSortingEnabled { get { return _isHavingSortingEnabled; } set { _isHavingSortingEnabled = value; SetPropertyChanged(nameof(IsHavingSortingEnabled)); } }
        //Ticket end:#37205 .by rupesh

        //Start #92766 FR POS - BE ABLE TO GO BACK By Pratik
        bool IsSubCategory = false;
        public List<CategoryDto> SubCategorys;
        //End #92766 By Pratik

        //#94565
        int _occupiedTable { get; set; }
        public int OccupiedTable { get { return _occupiedTable; } set { _occupiedTable = value; SetPropertyChanged(nameof(OccupiedTable)); } }

        public ObservableCollection<OccupideTableDto> OccupiedTables { get; set; }
        //#94565

        public ObservableCollection<FloorDto> FloorList { get; set; } //#94565
        
        #endregion

        #region Constructor
        public EnterSaleViewModel()
        {
            App.Instance.Hud.DisplayProgress(LanguageExtension.Localize("Progress0_Text"));
            productService = new ProductServices(productApiService);
            offerService = new OfferServices(offerApiService);
            paymentService = new PaymentServices(paymentApiService);
            outletService = new OutletServices(outletApiService);
            customerService = new CustomerServices(customerApiService);
            saleService = new SaleServices(saleApiService);
            userService = new UserServices(userApiService);
            taxServices = new TaxServices(taxApiService);
            RestaurantService = new RestaurantService(RestaurantApiService); //#94565
            Categories = new ObservableCollection<CategoryDto>();



            EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
            PaymentCommand = new Command(Payment);
            SearchProductCommand = new Command(SearchProduct);
            ShowOpenRegisterCommand = new Command(ShowOpenRegister);
            PaymentSummaryCommand = new Command(PaymentSummaryClick);
            NotesCommand = new Command(NotesClick);

            //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
            IsServedBy = Settings.StoreGeneralRule.ServedByLineItem;
            //end #84287 .by Pratik
            invoicemodel = new InvoiceViewModel(productService, customerService, outletService, saleService);
            Settings.IsEnterSaleFirstTimeLoad = true;
            _ = invoicemodel.ReopenSaleFromHistory(null);

            invoicemodel.CustomerModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsOpenSearchCustomerPopUp")
                {
                    if (invoicemodel.CustomerModel.IsOpenSearchCustomerPopUp == 1)
                    {
                        IsActive = true;
                        //#97186 By PR
                        if (invoicemodel.CustomerModel?.SearchCustomerView != null)
                            invoicemodel.CustomerModel.SearchCustomerView.FocusOnEntry();
                        //#97186 By PR
                    }
                    else
                    {
                        IsActive = false;
                        //#97186 By PR
                        if (invoicemodel.CustomerModel?.SearchCustomerView != null)
                            invoicemodel.CustomerModel.SearchCustomerView.UnFocusOnEntry();
                        //#97186 By PR
                    }

                }
            };

            if (Settings.IsAblyAsRealTime)
                SetupAbly();
            else
                SetupPubNub();

            //if (mPOPStarBarcode == null)
            //    mPOPStarBarcode = DependencyService.Get<IMPOPStarBarcode>();
            //mPOPStarBarcode.StartService();


            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.ResetDataMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.ResetDataMessenger>(this, (sender, arg) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (invoicemodel != null && invoicemodel.Invoice != null)
                        {
                            invoicemodel.Invoice = null;
                            invoicemodel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                        }
                    });
                });
            }

            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.ProductStockChangeMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.ProductStockChangeMessenger>(this, (sender, arg) =>
                {
                    if (arg.Value != null)
                        updateEntersaleProductStock(arg.Value);
                });
            }

            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.BackgroundInvoiceUpdatedMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.BackgroundInvoiceUpdatedMessenger>(this, (sender, arg) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        InvoiceDto Invoice = arg.Value;
                        Debug.WriteLine("BackgroundInvoiceUpdated data : " + Newtonsoft.Json.JsonConvert.SerializeObject(Invoice));

                        //#31380 iPad - Previous sale customer should not be redirect in POS screen after successfully completed sale.
                        //Ticket start:#44177 Shopify sale has been replaced with incorrect Hike invoice.by rupesh
                        if (invoicemodel.Invoice != null && Invoice.InvoiceTempId != null && invoicemodel.Invoice.InvoiceTempId == Invoice.InvoiceTempId)
                        {
                            //Ticket start:#44177 .by rupesh
                            invoicemodel.Invoice.Id = Invoice.Id;



                            //Ticket start:#27033 Hike store : iPAD : Can’t press email receipt button while processing a sale as it is greyed out.by rupesh

                            invoicemodel.Invoice.CustomerId = Invoice.CustomerId;
                            invoicemodel.Invoice.CustomerDetail = Invoice.CustomerDetail;
                            invoicemodel.Invoice.CustomerName = Invoice.CustomerName;
                            //Ticket start:#29657 Seach customer using phone number.by rupesh
                            invoicemodel.Invoice.CustomerPhone = Invoice.CustomerPhone;
                            //Ticket end:#29657 .by rupesh

                            //Ticket start:#42068 iPad: multiple entry updated on sales history.by rupesh
                            invoicemodel.Invoice.InvoiceHistories = Invoice.InvoiceHistories;
                            //Ticket end:#42068 .by rupesh


                            //Ticket end:#27033 .by rupesh
                        }

                        //#31380 iPad - Previous sale customer should not be redirect in POS screen after successfully completed sale.
                    });
                });
            }

            //Ticket #7760 Start:Rounding issue in Arabic language.By Nikhil.
            CountrySpecificCode();
            //Ticket #7760 End:By Nikhil. 

            //Tmp code to view print preview if there is no printer.,
            //TempPrinterCode(); 
        }

        //#94565
        private FloorDto getFlooreId(int tblid)
        {
            return FloorList?.FirstOrDefault(a => a.CanvanceLayout != null && a.CanvanceLayout.Objects.Any(a => a.TableId == tblid));
        }
        //#94565


        #endregion

        #region Command
        public ICommand PaymentCommand { get; }
        public ICommand PaymentSummaryCommand { get; }
        public ICommand NotesCommand { get; }
        public ICommand SearchProductCommand { get; }
        public ICommand ShowOpenRegisterCommand { get; }
        public ICommand CategoryTappedCommand => new Command<CategoryDto>(CategoryTapped);

        public ICommand CloseCategoryTappedCommand => new Command(CloseCategoryTapped); //Start #90945 By Pratik
        public ICommand MoreOptionCommand => new Command(MoreOptionTapped);
        public ICommand BackgroundHandleCommand => new Command(BackgroundHandleTapped);
        public ICommand RestockHandleCommand => new Command(RestockHandleTapped);
        public ICommand OpenDiscountCommand => new Command(OpenDiscountTapped);
        public ICommand OpenTipCommand => new Command(OpenTipTapped);
        public ICommand OpenTaxCommand => new Command(OpenTaxTapped);
        public ICommand AddShippingChargeCommand => new Command(AddShippingChargeTapped);
        public ICommand SortingFilterMenuOpenCommand => new Command(SortingFilterMenuOpenTapped);
        public ICommand QuoteSelectedCommand => new Command<object>(QuoteSelectedTapped);
        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
        public ICommand BackorderSelectedCommand => new Command<object>(BackorderSelected);
        //Ticket end:#92764.by rupesh
        public ICommand ProcessSaleSelectedCommand => new Command<object>(ProcessSaleSelectedTapped);
        public ICommand RegisterLblCommand => new Command(RegisterLblTapped);
        public ICommand SliderMenuHandleClickedCommand => new Command(sliderMenuHandle_Clicked);

        public ICommand FloorCommand => new Command(FloorClick);  //#94565    

        public ICommand ProductSelectCommand
        {
            get
            {
                return new Command((sender) =>
                {
                    if (sender != null || sender is EnterSaleItemDto)
                    {
                        EnterSaleItemDto data1 = ((EnterSaleItemDto)sender);
                        if (data1.ItemType == InvoiceItemType.Standard && data1.Product != null)
                        {
                            AddProductToSale(data1.Product);
                        }
                        //Start #92766 FR POS - BE ABLE TO GO BACK By Pratik
                        else if (data1.ItemType == InvoiceItemType.Back && data1.Category != null)
                        {
                            if (data1.Category.ParentId.HasValue && data1.Category.ParentId > 0)
                            {
                                IsSubCategory = true;
                                var cat = SubCategorys.Last();
                                SelectedCategory = cat;
                                SubCategorys.Remove(cat);
                            }
                            else
                            {
                                SubCategorys = new List<CategoryDto>();
                                SelectedCategory = data1.Category;
                            }
                        } //End #92766 By Pratik
                        else if (data1.ItemType == InvoiceItemType.Category && data1.Category != null)
                        {
                            //Start #92766 FR POS - BE ABLE TO GO BACK By Pratik
                            IsSubCategory = true;//Ticket:#95015
                            if (SubCategorys == null)
                                SubCategorys = new List<CategoryDto>();
                            SubCategorys.Add(SelectedCategory.Copy());
                            //End #92766 By Pratik
                            SelectedCategory = data1.Category;
                        }
                        else if (data1.ItemType == InvoiceItemType.Composite && data1.Offer != null)
                        {
                            AddOfferToSale(data1.Offer);
                        }
                    }
                });
            }
        }

        public ICommand InvoiceSelectCommand
        {
            get
            {
                return new Command((sender) =>
                {
                    if (sender != null || sender is InvoiceLineItemDto)
                    {
                        InvoiceLineItemDto data = ((InvoiceLineItemDto)sender);
                        if (data != null && data.isEnable)
                        {
                            SelectInvoiceItem(data);
                        }
                    }
                    SelectedInvoiceItem = null;
                });
            }
        }

        public ICommand RemainingProductCommand => new Command(RemainingProductTapped);
        public ICommand BarcodeScanSelectedCommand
        {
            get
            {
                return new Command(async (sender) =>
                {
                    if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0
                        && NavigationService.ModalStack.LastOrDefault() is CameraScanPage)
                        return;
                    IsOpenPopup = true;

                    var cameraScanPage = new CameraScanPage();
                    cameraScanPage.CameraBarcodeReader += CameraScanPage_CameraBarcodeReader;
                    await NavigationService.PushModalAsync(cameraScanPage);
                });

            }
        }
        public ICommand GoToCartCommand => new Command(GoToCart_Click);

        public ICommand OrderedCommand => new Command(Ordered_Click);  //#94565   

        public ICommand BackToPhoneGridCommand => new Command(BackToPhoneGrid_Click);

        private async void CameraScanPage_CameraBarcodeReader(object sender, List<string> e)
        {
            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0 && NavigationService.ModalStack.LastOrDefault() is CameraScanPage)
            {
                IsClosePopup = true;
                await this.NavigationService.PopModalAsync();
            }
            _ = AddProductByBarcode(e.First(), e);

            //});
        }

        #endregion

        #region Command Execution

        //#94565
        bool IsFloorClick;
        private async void FloorClick(object obj)
        {
            if (IsFloorClick)
                return;
            IsFloorClick = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? 2000 : 1000).Wait();
                IsFloorClick = false;
            });
            if (!invoicemodel.IsRestaurantPOS)
                return;
            await SetOccupiedTableWithLoader();
            RestaurantFloorPlanPage restaurantFloorPlan = new RestaurantFloorPlanPage(OccupiedTables, invoicemodel.Invoice);
            restaurantFloorPlan.ClosedPaged += ClosedRestaurantFloorPlanPage;
            await NavigationService.PushAsync(restaurantFloorPlan);
        }

        private async void ClosedRestaurantFloorPlanPage(object sender, CanvanceTableLayout table)
        {
            try
            {
                InvoiceDto invoice = null;
                if (table.TableId != null)
                    invoice = saleService.GetLocalInvoiceByTableId(table.TableId.Value);
                if (invoicemodel.Table == null && invoicemodel.Invoice?.InvoiceLineItems?.Count > 0)
                {
                    if (invoice != null)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ItemWithOccupideTableMsg"), Colors.Red, Colors.White);
                        return;
                    }
                    else
                    {
                        var result = await App.Alert.ShowAlert(
                            LanguageExtension.Localize("AlertLbl"),
                            LanguageExtension.Localize("ItemWithOutOccupideTableMsg"),
                            LanguageExtension.Localize("YesButtonText"),
                            LanguageExtension.Localize("CancelButtonText")
                        );

                        if (result)
                        {
                            OccupiedTable++;
                            invoicemodel.Table = table;
                            var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                            invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                            invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                            {
                                TableId = invoicemodel.Table?.TableId ?? 0,
                                AssignedDateTime = DateTime.UtcNow,
                                FloorId = floor == null ? 0 : floor.Id,
                                FloorName = floor?.Name,
                                TableName = invoicemodel.Table?.Name
                            });
                            invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;
                            invoicemodel.Invoice.InvoiceFloorTable.AssignedDateTime = DateTime.UtcNow;
                            await saleService.UpdateLocalInvoice(invoicemodel.Invoice);
                            SetOccupiedTable();
                            return;
                        }
                        else
                        {
                            invoicemodel.Table = null;
                            invoicemodel.Invoice?.InvoiceLineItems.Clear();
                            invoicemodel.Invoice = null;
                            invoicemodel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                            SetOccupiedTable();
                            return;
                        }
                    }

                }
                invoicemodel.Table = table;
                if (invoice == null && invoicemodel.Table?.TableId != null)
                {
                    using (new Busy(this, true))
                    {
                        OccupiedTable++;
                        if (OccupiedTables != null && OccupiedTables.Any(a => a.tableId.HasValue && a.tableId == invoicemodel.Table.TableId))
                        {
                            var invoiceid = OccupiedTables.First(a => a.tableId.HasValue && a.tableId == invoicemodel.Table.TableId).invoiceId;
                            if (invoiceid > 0)
                            {
                                invoice = saleService.GetLocalInvoice(invoiceid);
                                if (invoice == null)
                                    invoice = await saleService.GetRemoteInvoice(Priority.UserInitiated, true, invoiceid);
                                if (invoice != null)
                                {
                                    invoicemodel.Invoice = invoice;
                                    InvoiceCalculations.CalculateInvoiceTotal(invoicemodel.Invoice, invoicemodel.offers, productService);
                                }
                            }
                        }
                        if (invoice == null)
                        {
                            await invoicemodel.addNewSale();
                        }

                        if (invoicemodel.Invoice.InvoiceFloorTables?.FirstOrDefault() is not { Id: > 0 })
                        {
                            var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                            invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                            invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                            {
                                TableId = invoicemodel.Table?.TableId ?? 0,
                                AssignedDateTime = DateTime.UtcNow,
                                FloorId = floor == null ? 0 : floor.Id,
                                FloorName = floor?.Name,
                                TableName = invoicemodel.Table?.Name
                            });
                        }
                        if (invoicemodel.Invoice.InvoiceFloorTables?.FirstOrDefault() != null)
                            invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;
                        await saleService.UpdateLocalInvoice(invoicemodel.Invoice);
                    }
                }
                else
                {
                    ReopenSaleFromHistory(invoice);
                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //#94565

        private void PaymentSummaryClick(object obj)
        {
            IsPaymentSummary = !IsPaymentSummary;
            SetExpandHeight();
        }

        private void NotesClick(object obj)
        {
            IsNotes = !IsNotes;
            SetExpandHeight();
        }

        public void SetExpandHeight()
        {
            if (IsNotes & IsPaymentSummary)
                ExpandHeight = 294 + AddExpandHeight();
            else if (!IsNotes & IsPaymentSummary)
                ExpandHeight = 255 + AddExpandHeight();
            else if (!IsNotes & !IsPaymentSummary)
                ExpandHeight = 100;
            else if (IsNotes & !IsPaymentSummary)
                ExpandHeight = 140;
        }

        double AddExpandHeight()
        {
            double height = 0;
            if (TipVisible)
                height += 40;
            if (ShippingVisible)
                height += 40;
            return height;
        }

        private void sliderMenuHandle_Clicked()
        {
            try
            {
                _navigationService.MainPage.FlyoutIsPresented = !_navigationService.MainPage.FlyoutIsPresented;

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        private void RemainingProductTapped()
        {
            if (!waiting)
            {
                waiting = true; //Start #93039 By Pratik
                Task.Run(async () =>
                {
                    await Task.Delay(200); //Start #93039 By Pratik
                    loadMoreProduct(true);
                    waiting = false;
                });
            }
        }
        private void MoreOptionTapped()
        {
            invoicemodel.OpenOption();
        }
        private async void RegisterLblTapped()
        {
            if (IsMultipleRegister)
            {
                //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    return;
                }
                //End #90945 By Pratik

                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0
                      && NavigationService.ModalStack.LastOrDefault() is SelectOutletRegisterPage)
                    return;

                var selectOutletRegisterPage = new SelectOutletRegisterPage(true);
                selectOutletRegisterPage.ViewModel.RegisterIsSelected += async (obj, success) =>
                {
                    if (success)
                    {
                        //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                        await selectOutletRegisterPage.ViewModel.Close();
                        using (new Busy(this, true))
                        {
                            Settings.CurrentRegister = await outletService.GetRemoteRegisterById(Priority.UserInitiated, true, Settings.CurrentRegister.Id);
                            productService.ClearCurrentLayout();
                            await productService.GetCurrentLayout(Priority.UserInitiated, true, Settings.CurrentRegister.Id);
                            getCurrentLayout();
                            if (Categories != null)
                                Categories.Where(x => x.IsSelected).ForEach(a => a.IsSelected = false);
                            if (Categories != null && Categories.Any(a => a.ISLayout))
                                CategoryTapped(Categories.First(a => a.ISLayout));
                            EnterSalePage?.ScrollCategoryToStart();
                            if (Categories != null && !Categories.Any(a => a.IsSelected))
                            {
                                SelectedCategory = Categories.First();
                            }
                        }
                        //End #90945 By Pratik
                    }
                };
                selectOutletRegisterPage.ViewModel.SetStores(new System.Collections.ObjectModel.ObservableCollection<OutletDto_POS> { Settings.SelectedOutlet });
                IsOpenPopup = true;
                await NavigationService.PushModalAsync(selectOutletRegisterPage);
            }
        }
        private void CategoryTapped(CategoryDto categoryDto)
        {
            SubCategorys = new List<CategoryDto>(); //Start #92766 FR POS - BE ABLE TO GO BACK By Pratik
            if (SelectedCategory != categoryDto)
                SelectedCategory = categoryDto;
        }

        //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
        private void CloseCategoryTapped()
        {
            SelectedCategory = Categories.First();
        }
        //End #90945 By Pratik

        private void BackgroundHandleTapped()
        {
            //#97186 By PR
            if (invoicemodel.CustomerModel?.SearchCustomerView != null && invoicemodel.CustomerModel.IsOpenSearchCustomerPopUp == 1)
                invoicemodel.CustomerModel.SearchCustomerView.UnFocusOnEntry();
            //#97186 By PR
            invoicemodel.CustomerModel.IsOpenSearchCustomerPopUp = 0;
            invoicemodel.IsOpenTaxViewPopUp = 0;
            invoicemodel.IsOpenOptionPopUp = 0;
            invoicemodel.IsOpenDiscountPopUp = 0;
            IsOpenSortingFilterPopUp = 0;
        }
        private void RestockHandleTapped()
        {
            invoicemodel.ReStockWhenRefundCalled();
        }
        private void OpenDiscountTapped()
        {
            if (DiscountEnabled)
                invoicemodel.OpenDiscount("Discount");
        }
        private void OpenTipTapped()
        {
            invoicemodel.OpenDiscount("Tip");
        }
        private void OpenTaxTapped()
        {
            if (TaxEnabled)
                invoicemodel.OpenTax();
        }
        private void AddShippingChargeTapped()
        {
            invoicemodel.OpenDiscount("ShippingCharge");
        }
        private void SortingFilterMenuOpenTapped()
        {
            if (IsOpenSortingFilterPopUp == 0)
                IsOpenSortingFilterPopUp = 1;
            else
                IsOpenSortingFilterPopUp = 0;
        }

        void hideAllView()
        {
            if (invoicemodel?.CustomerModel != null)
            {
                //#97186 By PR
                if (invoicemodel.CustomerModel.SearchCustomerView != null && invoicemodel.CustomerModel.IsOpenSearchCustomerPopUp == 1)
                    invoicemodel.CustomerModel.SearchCustomerView.UnFocusOnEntry();
                //#97186 By PR
                invoicemodel.CustomerModel.IsOpenSearchCustomerPopUp = 0;
            }
            IsOpenSortingFilterPopUp = 0;
            if (invoicemodel != null)
            {
                invoicemodel.IsOpenDiscountPopUp = 0;
                invoicemodel.IsOpenTaxViewPopUp = 0;
                invoicemodel.IsOpenOptionPopUp = 0;
            }
        }

        public async void QuoteSelectedTapped(object e)
        {
            hideAllView();
            if ((Settings.IsQuoteSale && e != null) || (invoicemodel.Invoice != null && (invoicemodel.Invoice.Status != InvoiceStatus.initial
                    && invoicemodel.Invoice.Status != InvoiceStatus.Pending && invoicemodel.Invoice.Status != InvoiceStatus.Quote)))
            {
                return;
            }
            //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
            if (Settings.IsBackorderSaleSelected && invoicemodel.Invoice != null && invoicemodel.Invoice.InvoiceLineItems?.Count > 0 && e != null)
            {
                var result = await App.Alert.ShowAlert("Exit Backorder Mode?", "Once you have left backorder mode. any sales you make will be as quotes.", "Continue", "Cancel");
                if (result)
                    await invoicemodel.ToolMenu("CancelBackorder");
                else
                    return;

            }
            //Ticket end:#92764.by rupesh
            //Ticket start:#23298 iOS - Should Allow Switching to Quote When Nothing Is in Cart.by rupesh
            else if (invoicemodel.Invoice != null && invoicemodel.Invoice.InvoiceLineItems?.Count > 0 && e != null)
            {
                var result = await App.Alert.ShowAlert("Hold up! You currently have a sale in progress", "Before you can switch to Quote Mode, you have to complete the sale on the Sell screen.To keep the changes you’ve made to it, return to that sale to complete it.Or you can choose to discard changes made to that sale and switch to Quote mode.", "Park & Continue", "Cancel");
                if (result)
                {
                    //Ticket:#27064 iPad - Backorder sale without selecting customer issue.by rupesh
                    if (InvoiceCalculations.CheckHasBackOrder(invoicemodel.Invoice) && (invoicemodel.Invoice.CustomerId == null || invoicemodel.Invoice.CustomerId == 0))
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("BackOrderCutomerValidation"), Colors.Red, Colors.White);
                        return;
                    }
                    //Ticket:#27064 .by rupesh

                    await invoicemodel.ToolMenu("ParkAndContinue");
                }
                else
                    return;


            }
            else if (e != null)
            {
                await App.Alert.ShowAlert("Issuing quote", "Quotation will not affect inventory count or sales report data.", "Ok");
            }
            //Ticket start:#23298 .by rupesh

            Settings.IsQuoteSale = true;
            Settings.IsBackorderSaleSelected = false;
            QuoteBtnColor = AppColors.HikeColor;
            ProcessSaleColor = Colors.Transparent;
            BackorderBtnColor = Colors.Transparent;
            //Ticket start:#23405 iOS - Change UI of Quote.by rupesh
            MainGridBackgroundColor = AppColors.QuoteLightPinkColor;
            //Ticket end:#23405 .by rupesh
            LblPayText = "Quote";
            //Ticket start:#30226 iPad: delivery address popup should not be show in quote sale.by rupesh
            if (invoicemodel.Invoice != null)
            {
                invoicemodel.Invoice.DeliveryAddressId = null;
                invoicemodel.Invoice.DeliveryAddress = null;
            }
            DeliveryCustomerVisible = false;
            //Ticket end:#30226 .by rupesh
        }
        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
        public async void BackorderSelected(object e)
        {
            hideAllView();
            if ((Settings.IsBackorderSaleSelected && e != null) || (invoicemodel.Invoice != null && (invoicemodel.Invoice.Status != InvoiceStatus.initial
                    && invoicemodel.Invoice.Status != InvoiceStatus.Pending && invoicemodel.Invoice.Status != InvoiceStatus.BackOrder)))
            {
                return;
            }
            if (Settings.IsQuoteSale && invoicemodel.Invoice != null && invoicemodel.Invoice.InvoiceLineItems?.Count > 0 && e != null)
            {
                var result = await App.Alert.ShowAlert("Exit Quotation Mode?", "Once you've left Quote mode, any sales and payments you make from now on will be affected inventory and sale report.", "Continue", "Cancel");
                if (result)
                    await invoicemodel.ToolMenu("CancelQuote");
                else
                    return;

            }
            else if (invoicemodel.Invoice != null && invoicemodel.Invoice.InvoiceLineItems?.Count > 0 && e != null)
            {
                var result = await App.Alert.ShowAlert("Hold up! You currently have a sale in progress", "Before you can switch to Backorder Mode, you have to complete the sale on the Sell screen. To keep the changes you’ve made to it, return to that sale to complete it. Or you can choose to discard changes made to that sale and switch to Backorder mode.", "Park & Continue", "Cancel");
                if (result)
                {
                    if (InvoiceCalculations.CheckHasBackOrder(invoicemodel.Invoice) && (invoicemodel.Invoice.CustomerId == null || invoicemodel.Invoice.CustomerId == 0))
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("BackOrderCutomerValidation"), Colors.Red, Colors.White);
                        return;
                    }

                    await invoicemodel.ToolMenu("ParkAndContinue");
                }
                else
                    return;


            }
            else if (e != null)
            {
                await App.Alert.ShowAlert("Switching to Backorder mode", "Items will be placed as backorders regardless of stock availability.", "Ok");
            }
            Settings.IsBackorderSaleSelected = true;
            Settings.IsQuoteSale = false;
            BackorderBtnColor = AppColors.HikeColor;
            QuoteBtnColor = Colors.Transparent;
            ProcessSaleColor = Colors.Transparent;
            MainGridBackgroundColor = AppColors.BackorderLightCreamColor;
            LblPayText = "Backorder";
            if (invoicemodel.Invoice != null)
            {
                invoicemodel.Invoice.DeliveryAddressId = null;
                invoicemodel.Invoice.DeliveryAddress = null;
            }
            DeliveryCustomerVisible = false;

        }
        //Ticket end:#92764.by rupesh
        public async void ProcessSaleSelectedTapped(object e)
        {
            hideAllView();
            //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
            if ((!Settings.IsQuoteSale && !Settings.IsBackorderSaleSelected && e != null) || (invoicemodel.Invoice != null && (invoicemodel.Invoice.Status != InvoiceStatus.initial
                   && invoicemodel.Invoice.Status != InvoiceStatus.Pending && invoicemodel.Invoice.Status != InvoiceStatus.Quote && invoicemodel.Invoice.Status != InvoiceStatus.BackOrder) && e != null))
            {       //Ticket end:#92764.by rupesh
                return;
            }

            //Ticket start:#23298 iOS - Should Allow Switching to Quote When Nothing Is in Cart.by rupesh
            if (Settings.IsQuoteSale && invoicemodel.Invoice != null && invoicemodel.Invoice.InvoiceLineItems?.Count > 0 && e != null)
            {
                var result = await App.Alert.ShowAlert("Exit Quotation Mode?", "Once you've left Quote mode, any sales and payments you make from now on will be affected inventory and sale report.", "Continue", "Cancel");
                //Ticket start:#25667 IOS : switch the quote to process sale, quote item should be voided.by rupesh
                //Ticket start:#29035 iPad - Quote sale should not be voided when user click on Point of sale option.by rupesh
                if (result)
                    await invoicemodel.ToolMenu("CancelQuote");
                //Ticket end:#29035 .by rupesh
                else
                    return;
                //Ticket end:#25667 .by rupesh

            }
            //Ticket end:#23298 .by rupesh
            //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
            else if (Settings.IsBackorderSaleSelected && invoicemodel.Invoice != null && invoicemodel.Invoice.InvoiceLineItems?.Count > 0 && e != null)
            {
                var result = await App.Alert.ShowAlert("Exit Backorder Mode?", "Once you've left Backorder mode, any sales and payments you make from now on will be affected inventory and sale report.", "Continue", "Cancel");
                if (result)
                    await invoicemodel.ToolMenu("CancelBackorder");
                else
                    return;

            }
            //Ticket end:#92764 .by rupesh
            Settings.IsQuoteSale = false;
            Settings.IsBackorderSaleSelected = false;
            ProcessSaleColor = AppColors.HikeColor;
            QuoteBtnColor = Colors.Transparent;
            BackorderBtnColor = Colors.Transparent;
            //Ticket start:#23405 iOS - Change UI of Quote.by rupesh
            MainGridBackgroundColor = AppColors.MainBackgroundColor;
            //Ticket end:#23405 .by rupesh
            LblPayText = LanguageExtension.Localize("PayButtonText");
        }
        #endregion

        #region Life Cycle
        bool firsttimecall = true;
        public override void OnAppearing()
        {
            base.OnAppearing();
            //#94565
            if (RestaurantFloorPlanViewModel.FromRestaurant)
            {
                RestaurantFloorPlanViewModel.FromRestaurant = false;
                return;
            }
            //#94565

            if (IsSaleSucceess)
            {
                IsSaleSucceess = false;
                Task.Run(async () =>
                {
                    await Task.Delay(DeviceInfo.Platform == DevicePlatform.iOS ? 10 : 200);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        //#94565
                        if (Settings.IsRestaurantPOS)
                        {
                            invoicemodel.Table = null;
                        }
                        //#94565

                        invoicemodel?.Invoice?.InvoiceLineItems?.Clear();
                        invoicemodel.Invoice = null;
                        invoicemodel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                        if (SelectedCategory != null && SelectedCategory.ISLayout)
                        {
                            CloseCategoryTapped();
                        }
                    });
                });
            }

            if (IsClosePopup == true)
            {
                IsClosePopup = false;
                IsBusy = false;
                IsLoad = false;
                SetOccupiedTable();
                return;
            }
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500);
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        UpdateViews();
                        SetOccupiedTable();
                        if (firsttimecall)
                        {
                            ExpandHeight = 100;
                            firsttimecall = false;
                            IsPaymentSummary = false;
                        }

                        //#94565
                        if (FloorList == null || (FloorList != null && FloorList.Count <= 0))
                        {
                            var floors = RestaurantService.GetLocalFloors(Settings.SelectedOutletId);
                            FloorList = new ObservableCollection<FloorDto>(floors);
                            if (floors != null)
                                FloorList = new ObservableCollection<FloorDto>(floors);
                            else
                                FloorList = new ObservableCollection<FloorDto>();
                        }
                        //#94565

                        SetExpandHeight();
                        //Start ticket #76209 iOS:FR: User Permission : Add a permission for allowing 'Issue a Quote' By Pratik
                        IsQuoteDisplay = Settings.StoreGeneralRule.EnableQuoteSale && Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.Quote");
                        //End ticket #76209 By Pratik
                        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                        IsBackorderDisplay = Settings.StoreGeneralRule.EnableBackOrder && Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.BackOrder");
                        //Ticket end:#92764.by rupesh
                        //#38783 iPad: Feature request - Register's Name in Process Sale
                        OutletName = Settings.CurrentRegister.OutletName;
                        RegisterName = Settings.CurrentRegister.Name;
                        //Start #90940 ios :FR Batch number on cash register By Pratik
                        BatchNumber = (string.IsNullOrEmpty(Settings.CurrentRegister?.Registerclosure?.refNumber) ? string.Empty : " (" + Settings.CurrentRegister.Registerclosure.refNumber + ")");
                        //End #90940 By Pratik

                        IsMultipleRegister = Settings.SelectedOutlet?.OutletRegisters.Count > 1 ? true : false;
                        //#38783 iPad: Feature request - Register's Name in Process Sale
                        IsPaymentClicked = false;
                        //Ticket start:#29907 Notification of register opens for log.by rupesh
                        ShowAlertToCloseOpenedCashRegister();
                        //Ticket end:#29907 .by rupesh

                        if (!HasInitialized)
                        {
                            HasInitialized = true;
                            // if (EnterSalePage != null)
                            //     EnterSalePage.Customernamelbl.PropertyChanged += CustomerNameLableChanged;
                            // else
                            //     EnterSalePagePhone.Customernamelbl.PropertyChanged += CustomerNameLableChanged;
                        }

                        // if (EnterSalePage != null && EnterSalePage.InvoiceNote.IsFocused)
                        //     EnterSalePage.InvoiceNote.Unfocus();
                        // else if (EnterSalePagePhone != null && EnterSalePagePhone.InvoiceNote.IsFocused)
                        //     EnterSalePagePhone.InvoiceNote.Unfocus();

                        if (EnterSalePage.PaymentActiveUpdated)
                        {
                            EnterSalePage.PaymentActiveUpdated = false;
                            loadPayments();
                        }

                        if (EnterSalePage.DataUpdated)
                        {
                            EnterSalePage.DataUpdated = false;
                            await LoadInititalData();
                        }
                        else
                        {
                            try
                            {
                                loadMoreProduct();
                            }
                            catch (Exception ex)
                            {
                                ex.Track();
                            }
                        }

                        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                        //Ticket start:#22406 Quote sale.by rupesh
                        if (IsQuoteOrBackorderDisplay)
                        {
                            //Start ticket #76209 iOS:FR: User Permission : Add a permission for allowing 'Issue a Quote' By Pratik
                            if (Settings.IsQuoteSale && IsQuoteDisplay && (invoicemodel.Invoice == null || (invoicemodel.Invoice.Status == InvoiceStatus.initial
                            || invoicemodel.Invoice.Status == InvoiceStatus.Pending || invoicemodel.Invoice.Status == InvoiceStatus.Quote)))
                            {
                                //End ticket #76209 By Pratik
                                QuoteSelectedTapped(null);
                            }
                            else if (Settings.IsBackorderSaleSelected && IsBackorderDisplay && (invoicemodel.Invoice == null || (invoicemodel.Invoice.Status == InvoiceStatus.initial
                            || invoicemodel.Invoice.Status == InvoiceStatus.Pending || invoicemodel.Invoice.Status == InvoiceStatus.BackOrder)))
                            {
                                BackorderSelected(null);
                            }
                            else
                            {
                                ProcessSaleSelectedTapped(null);
                            }
                        }
                        else
                        {
                            Settings.IsQuoteSale = false;
                        }
                        //Ticket end:#22406 Quote sale.by rupesh
                        //Ticket end:#92764.by rupesh

                        invoicemodel.NavigationService = NavigationService;

                        //#32357 iPad :: Feature request :: Show Item Count on POS Screen
                        IsItemCountVisible = Settings.StoreGeneralRule.ShowTotalQuantityOfItemsInBasket;
                        //#32357 iPad :: Feature request :: Show Item Count on POS Screen

                        //Debug.WriteLine("GetLocalInvoices 1: " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                        //var invoices = await BlobCache.LocalMachine.GetAllObjects<InvoiceDto>();
                        //Debug.WriteLine("GetLocalInvoices 2: " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));

                        //if (scanApiService == null)
                        //    scanApiService = DependencyService.Get<IScanApiService>();
                        //scanApiService.StartService();

                    });

                    if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.UpdateShopDataMessenger>(this))
                    {
                        WeakReferenceMessenger.Default.Register<Messenger.UpdateShopDataMessenger>(this, (sender, arg) =>
                        {
                            MainThread.BeginInvokeOnMainThread(UpdateViews);
                        });
                    }

                    if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.BarcodeMessenger>(this))
                    {
                        WeakReferenceMessenger.Default.Register<Messenger.BarcodeMessenger>(this, (sender, arg) =>
                        {
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                var page = _navigationService.CurrentPage;
                                //Ticket start:#43809 iPad- Gift card Related Issues. by rupesh
                                if (page == null || (page is BaseContentPage<EnterSaleViewModel>))//#97721
                                {
                                    //Start ticket:#102270 Couldnt scan barcodes on iPad. By PR
                                    if (arg.Value.Length == 12 && arg.Value.All(char.IsDigit))
                                    {
                                        List<string> barcodes = new List<string>();
                                        var originalbarcode = "0" + arg.Value;
                                        barcodes.Add(originalbarcode);
                                        barcodes.Add(arg.Value);
                                        await AddProductByBarcode(originalbarcode, barcodes);
                                    }
                                    else
                                        await AddProductByBarcode(arg.Value);
                                    //End ticket:#102270
                                }
                                //Ticket end:#43809 . by rupeshv
                            });
                        });
                    }

                    if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.DataStreamNetworkChangeMessenger>(this))
                    {
                        WeakReferenceMessenger.Default.Register<Messenger.DataStreamNetworkChangeMessenger>(this, (sender, arg) =>
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                if (arg.Value)
                                {
                                    SetupAbly();
                                }
                                else
                                {
                                    SetupPubNub();
                                }
                            });
                        });
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            });
            
        }

        public void PrepareInvoiceRecipt()
        {
        }


        public override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsOpenPopup == true)
            {
                IsOpenPopup = false;
                return;
            }
            try
            {
                invoicemodel.CustomerModel.IsOpenSearchCustomerPopUp = 0;
                invoicemodel.IsOpenOptionPopUp = 0;
                invoicemodel.IsOpenDiscountPopUp = 0;
                invoicemodel.IsOpenTaxViewPopUp = 0;

                //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                //Ticket start:#25812 IOS : quote screen should not be showing when came to sales history to POS.by rupesh
                if (IsQuoteOrBackorderDisplay && _navigationService.IsFlyoutPage && !(_navigationService.RootPage is BaseContentPage<EnterSaleViewModel>))
                {
                    ProcessSaleSelectedTapped(null);
                }
                //Ticket end:#25812 .by rupesh
                //Ticket end:#92764.by rupesh
                IsActive = false;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            WeakReferenceMessenger.Default.Unregister<Messenger.BarcodeMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.DataStreamNetworkChangeMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.UpdateShopDataMessenger>(this);
        }

        #endregion

        #region Methods

        //94565
        public void SetOccupiedTable()
        {
            if (Settings.IsRestaurantPOS)
            {
                Task.Run(async () =>
                {
                    var results = saleService.GetOccupiedTables();
                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                        OccupiedTables = await saleService.GetFloorSalesList(Fusillade.Priority.Background);
                    if (results != null && results.Count > 0)
                    {
                        if (OccupiedTables == null)
                            OccupiedTables = new ObservableCollection<OccupideTableDto>();

                        foreach (var item in results)
                        {
                            if (item.InvoiceFloorTable != null && item.InvoiceFloorTable.TableId > 0 && !OccupiedTables.Any(a => a.tableId.HasValue && a.tableId == item.InvoiceFloorTable.TableId))
                            {
                                OccupiedTables.Add(new OccupideTableDto() { tableId = item.InvoiceFloorTable.TableId, assignedDateTime = item.InvoiceFloorTable.AssignedDateTime });
                            }
                        }
                    }
                    if (OccupiedTables != null)
                    {
                        OccupiedTable = OccupiedTables.Count(a => a.tableId.HasValue && a.tableId > 0);
                    }
                });
            }
        }

        public async Task SetOccupiedTableWithLoader()
        {
            using (new Busy(this, true))
            {
                var results = saleService.GetOccupiedTables();
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    OccupiedTables = await saleService.GetFloorSalesList(Fusillade.Priority.UserInitiated);
                if (results != null && results.Count > 0)
                {
                    if (OccupiedTables == null)
                        OccupiedTables = new ObservableCollection<OccupideTableDto>();

                    foreach (var item in results)
                    {
                        if (item.InvoiceFloorTable != null && item.InvoiceFloorTable.TableId > 0 && !OccupiedTables.Any(a => a.tableId.HasValue && a.tableId == item.InvoiceFloorTable.TableId))
                        {
                            OccupiedTables.Add(new OccupideTableDto() { tableId = item.InvoiceFloorTable.TableId, assignedDateTime = item.InvoiceFloorTable.AssignedDateTime });
                        }
                    }
                }
                if (OccupiedTables != null)
                {
                    OccupiedTable = OccupiedTables.Count(a => a.tableId.HasValue && a.tableId > 0);
                }
            }
            ;
        }

        //94565

        void CustomerNameLableChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
                CustomerNameLableChanged();
        }

        void TempPrinterCode()
        {
            var printer1 = new Printer
            {
                ModelName = "MPOP",
                width = 384,
                PrimaryReceiptPrint = true,
                NumberOfPrintCopy = 1,
                IsAutoPrintReceipt = true,
                Port = "123",
            };

            var printer2 = new Printer
            {
                ModelName = "Sample Printer",
                width = 576,
                PrimaryReceiptPrint = true,
                NumberOfPrintCopy = 1,
                IsAutoPrintReceipt = true,
                Port = "456",
            };

            var printers = new ObservableCollection<Printer>();
            printers.Add(printer1);
            //printers.Add(printer2);
            Settings.GetCachePrinters = printers;
        }

        //Ticket #7760 Start:Rounding issue in Arabic language.By Nikhil.
        void CountrySpecificCode()
        {
            var CurrentCultureName = DependencyService.Get<IMultilingual>().CurrentCultureInfo.Name;
            IsArabicCulture = (CurrentCultureName == "ar-SY");
        }
        //Ticket #7760 End:By Nikhil.

        public async Task LoadInititalData()
        {
            using (new Busy(this, true))
            {
                try
                {
                    // if(DeviceInfo.Idiom != DeviceIdiom.Phone)
                    {
                        Categories = new ObservableCollection<CategoryDto>();
                        getCategories();
                        // }
                        loadOffers();
                        loadPayments();
                        LoadCurrentOutlet();
                        getCustomers();
                        getAllUnitOfMeasures();

                        // if (Categories != null && Categories.Any() && DeviceInfo.Idiom != DeviceIdiom.Phone)
                        if (Categories != null && Categories.Any())

                        {
                            SelectedCategory = Categories.FirstOrDefault();
                            loadMoreProduct();
                            if (SelectedCategory != null)
                                SelectedCategory.IsSelected = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }

            }
            ;

        }

        public async void ReopenSaleFromHistory(InvoiceDto invoice = null)
        {
            invoicemodel.IsLoad = true;
            using (new Busy(this, true))
            {
                await invoicemodel.ReopenSaleFromHistory(invoice);
                invoicemodel.IsLoad = false;
            }
        }

        public void getAllProducts()
        {
            try
            {
                AllProducts = productService.GetLocalProducts();
                //Debug.WriteLine("AllProducts : " + AllProducts.Count());
                //Zendesk ticket 7003

                //Ticket start:#26322 iPad - Composite product issue (&& !item.DisableSellIndividually).by rupesh
                var tempAllProducts = AllProducts.Where(item => item.SalesChannels.Any(
                    cer => cer == (int)SellsChannel.PointOfSale) && !item.DisableSellIndividually);
                //Ticket end:#26322 .by rupesh

                AllProducts = new ObservableCollection<ProductDto_POS>(tempAllProducts);


            }
            catch (Exception ex)
            {
                ex.Track();
            }
            if (AllProducts == null)
            {
                AllProducts = new ObservableCollection<ProductDto_POS>();
            }
        }

        //Ticket Start:#20064 Unit of measurement feature for iPad app.by rupesh
        public void getAllUnitOfMeasures()
        {
            try
            {
                var tempAllUnitOfMeasures = productService.GetLocalUnityOfMeasures();
                AllUnitOfMeasures = new ObservableCollection<ProductUnitOfMeasureDto>(tempAllUnitOfMeasures);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            if (AllUnitOfMeasures == null)
            {
                AllUnitOfMeasures = new ObservableCollection<ProductUnitOfMeasureDto>();
            }
        }
        //Ticket End:#20064 .by rupesh

        public void getCustomers()
        {
            try
            {
                CustomerViewModel.AllCustomer = customerService.GetLocalCustomers();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            if (Categories == null)
            {
                CustomerViewModel.AllCustomer = new ObservableCollection<CustomerDto_POS>();
            }
        }

        //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
        public void getCurrentLayout()
        {
            try
            {
                InvoiceLayoutSellDetail = productService.GetCurrentLayout();
                if (InvoiceLayoutSellDetail?.RegisterLayoutOptions != null && InvoiceLayoutSellDetail.RegisterLayoutOptions.Count > 0)
                {
                    if (Categories == null)
                        Categories = new ObservableCollection<CategoryDto>();
                    if (Categories.Count <= 0)
                    {
                        Categories.Add(new CategoryDto() { Name = InvoiceLayoutSellDetail.Name, ISLayout = true, IsActive = true, Id = InvoiceLayoutSellDetail.Id });
                    }
                    else if (Categories.Any(a => a.ISLayout))
                    {
                        Categories.RemoveAt(0);
                        Categories.Insert(0, new CategoryDto() { Name = InvoiceLayoutSellDetail.Name, ISLayout = true, IsActive = true, Id = InvoiceLayoutSellDetail.Id });
                    }
                    else
                    {
                        Categories.Insert(0, new CategoryDto() { Name = InvoiceLayoutSellDetail.Name, ISLayout = true, IsActive = true, Id = InvoiceLayoutSellDetail.Id });
                    }
                }
                else
                {
                    if (Categories == null)
                        Categories = new ObservableCollection<CategoryDto>();
                    if (Categories.Any(a => a.ISLayout))
                    {
                        var layout = Categories.First(a => a.ISLayout);
                        Categories.Remove(layout);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        //End #90945 By Pratik

        public void getCategories()
        {
            ObservableCollection<CategoryDto> categories = null;
            try
            {
                getCurrentLayout(); //Start #90945 By Pratik
                AllCategories = productService.GetLocalCategories();
                categories = getFilterCategories();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            if (categories == null)
            {
                categories = new ObservableCollection<CategoryDto>();
            }

            //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
            if (SelectedCategory != null && SelectedCategory.Id > 0)
            {
                if (SelectedCategory.ISLayout)
                    categories.Where(x => x.Id == SelectedCategory.Id && x.ISLayout).All(x => { x.IsSelected = true; return true; });
                else
                    categories.Where(x => x.Id == SelectedCategory.Id && !x.ISLayout).All(x => { x.IsSelected = true; return true; });
            }

            if (Categories == null || (Categories != null && Categories.Count <= 0))
                Categories = categories;
            else
            {
                var lst = Categories.ToList();
                lst.AddRange(categories);
                Categories = new ObservableCollection<CategoryDto>(lst);
            }
            //End #90945 By Pratik
        }

        public ObservableCollection<CategoryDto> getFilterCategories()
        {
            try
            {
                bool hideProductTypeIfNoProductInit = false;
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.HideProductTypeIfNoProductInIt)
                    hideProductTypeIfNoProductInit = Settings.StoreGeneralRule.HideProductTypeIfNoProductInIt;


                var categories = new ObservableCollection<CategoryDto>();
                foreach (var item in AllCategories.OrderBy(x => x.Name).OrderBy(x => x.sequence).OrderByDescending(a => a.ISLayout)) // Start ##90945 By Pratik
                {
                    if (item.ParentId == null && item.IsActive == true && (!hideProductTypeIfNoProductInit || item.SubCategoryCount > 0 || (hideProductTypeIfNoProductInit && item.ProductsCount > 0)))
                    {
                        //Ticket start :#72023 Product type with no products appears on POS page on iPad.by rupesh
                        if ((item.SubCategoryCount == 0 || item.ProductsCount > 0) && !categories.Any(a => a.Id == item.Id))
                            categories.Add(item);
                        else
                        {
                            if (item.SubCategoryCount > 0)
                                item.SubCategories = new ObservableCollection<CategoryDto>(AllCategories.Where(x => x.ParentId == item.Id));
                            //Ticket end:#72023.by rupesh
                            int exist = item.SubCategories != null ? item.SubCategories.Count(a => a.IsActive == true && a.ProductsCount > 0) : 0;
                            if (exist > 0 && !categories.Any(a => a.Id == item.Id))
                                categories.Add(item);
                            else
                            {
                                if (!hideProductTypeIfNoProductInit && (item.ProductsCount == 0 || item.SubCategoryCount == 0) && !categories.Any(a => a.Id == item.Id))
                                {
                                    categories.Add(item);
                                }
                            }
                        }
                    }
                }
                return categories;
            }
            catch (Exception ex)
            {
                ex.Track();
                return new ObservableCollection<CategoryDto>();
            }
        }

        public async void loadProduct()
        {
            SelectedCategory.IsLoad = true;
            await LoadProductAsync();
            SelectedCategory.IsLoad = false;
        }

        int step = 0;
        ObservableCollection<EnterSaleItemDto> tmpEnterSaleItems { get; set; }

        public bool loadMoreProduct(bool FromLoadMore = false)
        {
            if (SelectedCategory == null)
            {
                return false;
            }

            try
            {
                if (!FromLoadMore)
                {
                    Categories.All(x =>
                    {
                        x.IsSelected = SelectedCategory.ISLayout ? (x.Id == SelectedCategory.Id && x.ISLayout) : (x.Id == SelectedCategory.Id && !x.ISLayout); //Start #90945 By Pratik
                        return true;
                    });

                    //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                    if (SelectedCategory.ISLayout)
                    {
                        if (string.IsNullOrEmpty(SelectedCategory.LayoutId))
                        {
                            var localaylout = productService.GetLocalCurrentLayoutByID(SelectedCategory.Id);
                            var serializeResult = JsonConvert.DeserializeObject<List<LayoutOptionResponse>>(localaylout.Value);
                            AllProducts = new ObservableCollection<ProductDto_POS>();
                            List<int> lst = serializeResult.Where(x => x.id != null && int.TryParse(x.id, out int id) && id > 0).Select(a => a.measureProductId > 0 ? a.measureProductId.Value : Convert.ToInt32(a.id)).ToList();
                            var prodslst = productService.GetLocalProductsByIds(lst);
                            foreach (var item in serializeResult)
                            {
                                AllProducts.Add(LayoutOptionResponse.ConvertToProductDto(item, prodslst.FirstOrDefault(a => a.Id.ToString() == (item.measureProductId > 0 ? item.measureProductId.Value.ToString() : item.id))));
                            }
                        }
                        else
                        {
                            var localaylout = productService.GetLocalCurrentLayoutByID(SelectedCategory.Id);
                            var serializeResult = JsonConvert.DeserializeObject<List<LayoutOptionResponse>>(localaylout.Value);
                            if (serializeResult.Any(a => a.id == SelectedCategory.LayoutId))
                            {
                                serializeResult = serializeResult.First(a => a.id == SelectedCategory.LayoutId).columns.First();
                            }
                            AllProducts = new ObservableCollection<ProductDto_POS>();
                            List<int> lst = serializeResult.Where(x => x.id != null && int.TryParse(x.id, out int id) && id > 0).Select(a => a.measureProductId > 0 ? a.measureProductId.Value : Convert.ToInt32(a.id)).ToList();
                            var prodslst = productService.GetLocalProductsByIds(lst);
                            foreach (var item in serializeResult)
                            {
                                AllProducts.Add(LayoutOptionResponse.ConvertToProductDto(item, prodslst.FirstOrDefault(a => a.Id.ToString() == (item.measureProductId > 0 ? item.measureProductId.Value.ToString() : item.id))));
                            }
                        }
                    }
                    else
                    {
                        AllProducts = productService.GetLocalProductsByCategoryId(SelectedCategory.Id);
                    }

                    var tmpAllProducts = SelectedCategory.ISLayout ? AllProducts.Take(30) : AllProducts.OrderBy(x => x.Name).Take(30);
                    // var tmpAllProducts = AllProducts.Where(x => x.IsActive && (x.ParentId == 0 || x.ParentId == null) && x.ProductCategories.Contains(SelectedCategory.Id)).OrderBy(x => x.Name).Take(30);
                    if (tmpAllProducts != null)
                    {
                        tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>(tmpAllProducts.Select(product =>
                        {
                            return new EnterSaleItemDto()
                            {
                                ItemType = (product == null || product.ISLayout == true) ? Enums.InvoiceItemType.Category : Enums.InvoiceItemType.Standard,
                                Category = new CategoryDto() { Name = product?.Name, LayoutColor = product?.LayoutColor, IsActive = true, ISLayout = true, Id = SelectedCategory.Id, LayoutId = product?.LayoutId, ProductsCount = product?.ProductsCount ?? 0 },
                                Product = product
                            };
                        }));
                    }
                    else
                    {
                        tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                    }
                    // End #90945 By Pratik
                    step = 1;
                    //Ticket Start:#20064 Unit of measurement feature for iPad app.by rupesh
                    if (AllUnitOfMeasures != null && AllProducts != null && !SelectedCategory.ISLayout) //Start #90945 By Pratik
                    {

                        var tmpAllUnitOfMeasures = AllUnitOfMeasures.Where(x => (AllProducts.Count(k => k.Id == x.ProductId) > 0)).OrderByDescending(x => x.Name);
                        if (tmpAllUnitOfMeasures != null)
                        {
                            foreach (var item in tmpAllUnitOfMeasures)
                            {
                                var product = productService.GetLocalProduct(item.MeasureProductId);
                                if (product != null)
                                {
                                    var unitOfMeasureProduct = product.Copy();
                                    unitOfMeasureProduct.Id = item.Id;
                                    unitOfMeasureProduct.Name = item.Name;
                                    unitOfMeasureProduct.BarCode = item.BarCode;
                                    unitOfMeasureProduct.Sku = item.Sku;
                                    unitOfMeasureProduct.ProductUnitOfMeasureDto = item;
                                    unitOfMeasureProduct.IsUnitOfMeasure = true;
                                    if (unitOfMeasureProduct.ParentId != null)
                                    {
                                        var parentProduct = productService.GetLocalProductDB(unitOfMeasureProduct.ParentId.Value);
                                        unitOfMeasureProduct.ItemImageUrl = parentProduct.ItemImageUrl;
                                    }
                                    tmpEnterSaleItems.Insert(0, new EnterSaleItemDto()
                                    {
                                        ItemType = Enums.InvoiceItemType.Standard,
                                        Product = unitOfMeasureProduct
                                    });
                                }
                            }
                        }
                    }
                    //Ticket End:#20064 .by rupesh

                    if (AllCategories != null && !SelectedCategory.ISLayout) //Start #90945 By Pratik
                    {
                        List<CategoryDto> tempsubcategories = AllCategories.Where(x => !x.ISLayout && x.ParentId == SelectedCategory.Id && x.IsActive).OrderByDescending(x => x.Id).OrderByDescending(x => x.sequence).ToList(); //Start #90945 By Pratik
                        if (tempsubcategories != null)
                        {
                            foreach (var item in tempsubcategories)
                            {
                                //Ticket #13418 Incorrect count of the products on POS screen. By Nikhil
                                //Count calculation commented because count is coming in new API
                                //item.ProductsCount = AllProducts.Count(x => x.IsActive && (x.ParentId == 0 || x.ParentId == null) && x.ProductCategoryDtos.Count(c => c.Id == item.Id && item.Id != 0) > 0); 
                                //Ticket #13418 End.By Nikhil
                                tmpEnterSaleItems.Insert(0, new EnterSaleItemDto()
                                {
                                    ItemType = Enums.InvoiceItemType.Category,
                                    Category = item
                                });
                            }
                        }
                    }

                    if (Offers != null && !SelectedCategory.ISLayout) //Start #90945 By Pratik
                    {
                        foreach (var offer in Offers.Where(x => x.OfferType == Enums.OfferType.Composite).OrderByDescending(x => x.Id))
                        {
                            if (offer != null && offer.OfferItems != null)
                            {
                                foreach (var item in offer.OfferItems)
                                {
                                    var product = productService.GetLocalProductDB(item.OfferOnId);
                                    if (product != null && product.ProductCategories.Any(x => x == SelectedCategory.Id))
                                    {
                                        tmpEnterSaleItems.Insert(0, new EnterSaleItemDto()
                                        {
                                            ItemType = Enums.InvoiceItemType.Composite,
                                            Offer = offer
                                        });
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //Start #92766 FR POS - BE ABLE TO GO BACK By Pratik
                    if (IsSubCategory)
                    {
                        IsSubCategory = false;
                        if (!SelectedCategory.ISLayout)
                        {
                            tmpEnterSaleItems.Insert(0, new EnterSaleItemDto()
                            {
                                ItemType = Enums.InvoiceItemType.Back,
                                Category = SubCategorys.First(a => a.Id == SelectedCategory.ParentId)
                            });
                            Categories.First(a => a.Id == SubCategorys.First().Id).IsSelected = true;
                        }
                    }
                    //End #92766 By Pratik

                }
                else
                {
                    if (AllProducts != null)
                    {
                        //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                        //  var tmpProducts = AllProducts.Where(x => x.IsActive && (x.ParentId == 0 || x.ParentId == null) && x.ProductCategoryDtos.Count(k => k.Id == SelectedCategory.Id) > 0).OrderBy(x => x.Name).Skip(step * 30).Take(30);
                        var tmpProducts = SelectedCategory.ISLayout ? AllProducts.Skip(step * 30).Take(30) : AllProducts.OrderBy(x => x.Name).Skip(step * 30).Take(30);
                        if (tmpProducts != null)
                        {
                            foreach (var product in tmpProducts)
                            {
                                tmpEnterSaleItems.Add(new EnterSaleItemDto()
                                {
                                    ItemType = (product == null || product.ISLayout == true) ? Enums.InvoiceItemType.Category : Enums.InvoiceItemType.Standard,
                                    Category = new CategoryDto() { Name = product?.Name, ISLayout = true, IsActive = true, LayoutId = product?.LayoutId, ProductsCount = product?.ProductsCount ?? 0 },
                                    Product = product
                                });
                            }
                        }
                        //End #90945 By Pratik
                    }
                    step++;
                }

                //Ticket start:#37205 iOS - Sorting Products on Process Sale Page.by rupesh
                if (!FromLoadMore)  //Start #90945 By Pratik
                    SelectSortingFilterMenu(SelectedSortingMenu);
                else
                    EnterSaleItems = tmpEnterSaleItems;
                //Ticket start:#37205 .by rupesh

                DisplayFolder = SelectedCategory.DisplayFolder;
                LayoutColor = SelectedCategory.LayoutColor;
                SelectedCategoryName = SelectedCategory.Name;
            }
            catch (Exception ex)
            {
                if (SelectedCategory != null)
                {
                    DisplayFolder = SelectedCategory.DisplayFolder;
                    LayoutColor = SelectedCategory.LayoutColor;
                    SelectedCategoryName = SelectedCategory.Name;
                }
                ex.Track();
            }

            return true;
        }

        private CancellationTokenSource _cts;

        public async Task LoadProductAsync()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                await LoadProductWithToken(_cts.Token);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task LoadProductWithToken(CancellationToken token)
        {
            await Task.Delay(100, token);
            token.ThrowIfCancellationRequested();
            loadMoreProduct();
        }

        public void loadOffers()
        {
            try
            {
                if (Offers == null)
                {
                    Offers = new ObservableCollection<OfferDto>();
                }
                else
                {
                    Offers.Clear();
                }

                var tmpoffers = offerService.GetLocalOffers();
                if (tmpoffers != null && tmpoffers.Count > 0)
                {
                    Offers = new ObservableCollection<OfferDto>(tmpoffers.Where(x => x.IsActive && (x.IsOfferOnAllOutlet || (x.OfferOutlets.Count(o => o.OutletId == Settings.SelectedOutletId) > 0))));
                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async void AddProductToSale(ProductDto_POS product)
        {
            try
            {
                if (product.IsUnitOfMeasure && !string.IsNullOrEmpty(product.LayoutId) && Convert.ToInt32(product.LayoutId) > 0)
                {
                    product = productService.GetLocalUnitOfMeasureProduct(Convert.ToInt32(product.LayoutId));
                    if (product == null || (product != null && product.Id > 0 && (!product.IsActive || product.DisableSellIndividually || (product.ProductOutlets?.FirstOrDefault(a => a.OutletId == Settings.SelectedOutletId) != null && !product.ProductOutlets.FirstOrDefault(a => a.OutletId == Settings.SelectedOutletId).IsVisible))))
                    {
                        App.Instance.Hud.DisplayToast("This product may be temporarily unavailable, discontinued, or not offered at your specific outlet", Colors.Red, Colors.White);
                        return;
                    }
                }
                else if (product.Id > 0 && (product.ProductDeleted || product.DisableSellIndividually || !product.IsActive || (product.ProductOutlets?.FirstOrDefault(a => a.OutletId == Settings.SelectedOutletId) != null && !product.ProductOutlets.FirstOrDefault(a => a.OutletId == Settings.SelectedOutletId).IsVisible)))
                {
                    App.Instance.Hud.DisplayToast("This product may be temporarily unavailable, discontinued, or not offered at your specific outlet", Colors.Red, Colors.White);
                    return;
                }
                else if (product != null && product.Id <= 0 && string.IsNullOrEmpty(product.Sku))
                {
                    return;
                }

                //TODO pass sale service
                if (product.HasVarients)
                {
                    var varientProducts = productService.GetLocalProductVariants(product.Id);

                    //Assign Product Attrributes 
                    productService.SetProductAttributes(product, varientProducts);

                    //Assign Product Varients 
                    productService.SetProductVarients(product, varientProducts);

                    //Assign Product Extras
                    if (product.ProductExtras != null && product.ProductExtras.Any())
                    {
                        varientProducts.ForEach(
                            a =>
                            {
                                a.ProductExtras = product.ProductExtras;
                            }
                        );
                    }
                }

                if (!product.HasVarients && product.ParentId.HasValue && product.ParentId > 0)
                {
                    var mainProducts = productService.GetLocalProduct(product.ParentId.Value);

                    if (mainProducts.ProductExtras != null && mainProducts.ProductExtras.Any())
                    {
                        product.ProductExtras = mainProducts.ProductExtras;
                        productService.UpdateLocalProduct(product);
                    }
                }

                if (!IsOpenRegister && Settings.CurrentRegister == null || (Settings.CurrentRegister != null && !Settings.CurrentRegister.IsOpened))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RegisterOpenAlertMessage"));
                    return;
                }
                await invoicemodel.selectProduct(product);
                if (invoicemodel.Invoice?.InvoiceLineItems != null)
                {
                    var item = invoicemodel.Invoice.InvoiceLineItems.LastOrDefault(a => a.InvoiceItemValue == product.Id);
                    if (EnterSalePage != null)
                    {
                        EnterSalePage.ScrollInvoiceItemToStart(item);
                    }
                    else if (EnterSalePagePhone != null)
                    {
                        EnterSalePagePhone.ScrollInvoiceItemToStart(item);
                    }
                }
                if (SelectedCategory.ISLayout && InvoiceLayoutSellDetail != null && InvoiceLayoutSellDetail.IsDefault == false)
                {
                    CloseCategoryTapped();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task AddProductByBarcode(string Barcode, List<string> barcodes = null)
        {
            try
            {
                if (string.IsNullOrEmpty(Barcode))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoItemBarcodeMessage"), Colors.Red, Colors.White);
                    return;
                }
                ProductDto_POS product = null;
                if (barcodes != null && barcodes.Count > 1)
                    product = productService.GetLocalProductByBarcode(barcodes.Last());

                if (product == null)
                    product = productService.GetLocalProductByBarcode(Barcode);


                if (product != null && product.IsActive)
                {
                    AddProductToSale(product);
                    return;
                }
                //Ticket start:#36709 Bracodes scanning issue with UOM products on IPad.by rupesh
                ProductUnitOfMeasureDto unitOfMeasure = null;
                if (barcodes != null && barcodes.Count > 1)
                    unitOfMeasure = AllUnitOfMeasures.FirstOrDefault(x => x.BarCode == barcodes.Last());

                if (unitOfMeasure == null)
                    unitOfMeasure = AllUnitOfMeasures.FirstOrDefault(x => x.BarCode == Barcode);

                if (unitOfMeasure != null)
                {
                    product = productService.GetLocalProduct(unitOfMeasure.MeasureProductId);
                    if (product != null)
                    {
                        var unitOfMeasureProduct = product.Copy();
                        unitOfMeasureProduct.Id = unitOfMeasure.Id;
                        unitOfMeasureProduct.Name = unitOfMeasure.Name;
                        unitOfMeasureProduct.BarCode = unitOfMeasure.BarCode;
                        unitOfMeasureProduct.Sku = unitOfMeasure.Sku;
                        unitOfMeasureProduct.ProductUnitOfMeasureDto = unitOfMeasure;
                        unitOfMeasureProduct.IsUnitOfMeasure = true;
                        //Ticket start:#26980 iPad: UOM product price should be showing in search option.by rupesh
                        unitOfMeasureProduct.UOMSellingPrice = unitOfMeasureProduct.ProductOutlet.SellingPrice * unitOfMeasureProduct.ProductUnitOfMeasureDto.Qty;
                        //Ticket end:#26980 .by rupesh
                        if (unitOfMeasureProduct.ParentId != null)
                        {
                            var parentProduct = productService.GetLocalProductDB(unitOfMeasureProduct.ParentId.Value);
                            unitOfMeasureProduct.ItemImageUrl = parentProduct.ItemImageUrl;
                            unitOfMeasureProduct.ItemImage = parentProduct.ItemImage;
                        }
                        AddProductToSale(unitOfMeasureProduct);
                    }
                    return;
                }
                //Ticket end:#36709 .by rupesh

                OfferDto tempOffer = null;
                if (barcodes != null && barcodes.Count > 1)
                    tempOffer = Offers.FirstOrDefault(x => x.BarCode == barcodes.Last());

                if (unitOfMeasure == null)
                    tempOffer = Offers.FirstOrDefault(x => x.BarCode == Barcode);

                if (tempOffer != null)
                {
                    AddOfferToSale(tempOffer);
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoItemBarcodeMessage"), Colors.Red, Colors.White);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async void AddOfferToSale(OfferDto offer)
        {
            if (!IsOpenRegister)
                return;
            await invoicemodel.selectOffer(offer);

        }

        public void loadPayments()
        {
            try
            {
                 var paymentOptions = paymentService.GetLocalPaymentOptions();

                var filteredPaymentOption = paymentOptions
                    .FirstOrDefault(x => x.PaymentOptionType == HikePOS.Enums.PaymentOptionType.HikePay &&
                           (x.RegisterPaymentOptions == null ||
                            x.RegisterPaymentOptions.Count < 1 ||
                            x.RegisterPaymentOptions.Any(y => y.RegisterId == Settings.CurrentRegister.Id)));
                var terminalId = Settings.TerminalId;
                if (filteredPaymentOption != null)
                {
                    AllPaymentOptionList = new ObservableCollection<PaymentOptionDto>()
                {
                         new PaymentOptionDto()
                        {
                            Id =filteredPaymentOption.Id,
                            CanBeConfigered = true,
                            IsActive = true,
                            IsConfigered = true,
                            PaymentOptionType = Enums.PaymentOptionType.HikePay,
                            PaymentOptionName = "HikePay",
                            DisplayName = "Charge",
                            Name = "Charge",
                            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(){
                                new RegisterPaymentOptionDto(){
                                    RegisterId = Settings.CurrentRegister.Id,
                                    RegisterName = Settings.CurrentRegister.Name
                                }
                            },
                            ConfigurationDetails = $"{{\"paymentType\":49,\"ipAddress\":\"127.0.0.1\",\"balanceAccountId\":\"BA3293B22322B75NX94W87295\",\"terminalId\":\"{terminalId}\"}}"
                          }

                };
                }

            }
            catch (Exception ex)
            {
                ex.Track();

            }
        }

        public void LoadCurrentOutlet()
        {
            try
            {
                CurrentOutlet = outletService.GetLocalOutletById(Settings.SelectedOutletId);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        private bool IsPaymentClicked;
        public async void Payment()
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
                // using (new Busy(this, true))
                {
                    if (IsOpenBackground == 0 && invoicemodel != null && invoicemodel.Invoice != null && invoicemodel.Invoice.InvoiceLineItems != null && invoicemodel.Invoice.InvoiceLineItems.Count > 0)
                    {
                        //Ticket start:#22406 Quote sale by rupesh
                        if (Settings.IsQuoteSale && (invoicemodel.Invoice.CustomerId == null || invoicemodel.Invoice.CustomerId == 0))
                        {
                            if (invoicemodel.Invoice.CustomerDetail != null && (!string.IsNullOrEmpty(invoicemodel.Invoice.CustomerDetail.TempId)))
                            {
                                invoicemodel.Invoice.CustomerTempId = invoicemodel.Invoice.CustomerDetail.TempId;
                            }

                            if (string.IsNullOrEmpty(invoicemodel.Invoice.CustomerTempId))
                            {

                                App.Instance.Hud.DisplayToast("Please assign customer to issue a quote.", Colors.Red, Colors.White);
                                return;
                            }
                        }
                        //Ticket end:#22406 Quote sale by rupesh
                        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                        else if (Settings.IsBackorderSaleSelected && (invoicemodel.Invoice.CustomerId == null || invoicemodel.Invoice.CustomerId == 0))
                        {
                            if (invoicemodel.Invoice.CustomerDetail != null && (!string.IsNullOrEmpty(invoicemodel.Invoice.CustomerDetail.TempId)))
                            {
                                invoicemodel.Invoice.CustomerTempId = invoicemodel.Invoice.CustomerDetail.TempId;
                            }

                            if (string.IsNullOrEmpty(invoicemodel.Invoice.CustomerTempId))
                            {
                                App.Instance.Hud.DisplayToast("Please assign customer to create a Backorder.", Colors.Red, Colors.White);
                                return;
                            }
                        }
                        //Ticket end:#92764 .by rupesh
                        if (invoicemodel.Invoice.Status == InvoiceStatus.Exchange)
                        {
                            //Ticket #12270 Start : Allow Exchange Products with Gift Cards. By Nikhil
                            //Ticket Start:#25679 IOS : stuck the pay button when Exchange by UOM product.by rupesh
                            bool exchangeLineItem = invoicemodel.Invoice.InvoiceLineItems.Any(a => a.Quantity > 0 && (a.InvoiceItemType == InvoiceItemType.Standard || a.InvoiceItemType == InvoiceItemType.Custom || a.InvoiceItemType == InvoiceItemType.CompositeProduct
                            || a.InvoiceItemType == InvoiceItemType.GiftCard || a.InvoiceItemType == InvoiceItemType.UnityOfMeasure));
                            //Ticket End:#25679 .by rupesh
                            //Ticket #12270 End. By Nikhil
                            if (!exchangeLineItem)
                            {
                                //App.Instance.Hud.DisplayToast(LanguageExtension.Localize("BackOrderCutomerValidation"), Colors.Red, Colors.White);
                                return;
                            }

                        }

                        // Debug.WriteLine("Customer backorder : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoicemodel));

                        var hasbackorder = InvoiceCalculations.CheckHasBackOrder(invoicemodel.Invoice);

                        if (hasbackorder && (invoicemodel.Invoice.CustomerId == null || invoicemodel.Invoice.CustomerId == 0))
                        {
                            if (invoicemodel.Invoice.CustomerDetail != null && (!string.IsNullOrEmpty(invoicemodel.Invoice.CustomerDetail.TempId)))
                            {
                                invoicemodel.Invoice.CustomerTempId = invoicemodel.Invoice.CustomerDetail.TempId;
                            }

                            if (string.IsNullOrEmpty(invoicemodel.Invoice.CustomerTempId))
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("BackOrderCutomerValidation"), Colors.Red, Colors.White);
                                return;
                            }
                        }

                        bool isValidSerialNo = invoicemodel.CheckForSerialNo();
                        if (!isValidSerialNo)
                        {
                            return;
                        }
                        IsOpenPopup = true;
                        IsClosePopup = true;

                        invoicemodel.Invoice.LocalInvoiceStatus = LocalInvoiceStatus.Processing;

                        if (invoicemodel.Invoice.Status == InvoiceStatus.initial)
                            invoicemodel.Invoice.Status = InvoiceStatus.Pending;

                        if (EnterSalePage != null)
                        {
                            if (paymentpage == null)
                            {
                                paymentpage = new PaymentPage();
                            }
                            if (paymentpage.ViewModel.PaymentOptionList == null)
                            {
                                paymentpage.ViewModel.PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
                            }
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(100);
                                paymentpage.ViewModel.AllPaymentOptionList = AllPaymentOptionList;
                                paymentpage.ViewModel.CurrentOutlet = CurrentOutlet;
                                paymentpage.ViewModel.IsItemCountVisible = Settings.StoreGeneralRule.ShowTotalQuantityOfItemsInBasket;
                                paymentpage.ViewModel.GiftCardBalance = string.Empty;
                                //paymentpage.ViewModel.Invoice = JsonConvert.DeserializeObject<InvoiceDto>(JsonConvert.SerializeObject(invoicemodel.Invoice));
                                if (invoicemodel.Table != null && invoicemodel.Invoice.InvoiceFloorTables?.FirstOrDefault() is not { Id: > 0 })
                                {
                                    var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                                    invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                                    invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                                    {
                                        TableId = invoicemodel.Table?.TableId ?? 0,
                                        AssignedDateTime = DateTime.UtcNow,
                                        FloorId = floor == null ? 0 : floor.Id,
                                        FloorName = floor?.Name,
                                        TableName = invoicemodel.Table?.Name
                                    });
                                    invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;
                                }

                                paymentpage.ViewModel.Invoice = invoicemodel.Invoice;

                                //if (paymentpage.ViewModel.Invoice?.InvoiceLineItems != null && paymentpage.ViewModel.Invoice.InvoiceLineItems.Count >= 16)
                                //    paymentpage.ViewModel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(paymentpage.ViewModel.Invoice.InvoiceLineItems.Take(16));
                                //else
                                paymentpage.ViewModel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(paymentpage.ViewModel.Invoice.InvoiceLineItems);
                                //Ticket #12872 Emailed invoice has partial info. By Nikhil	 
                                paymentpage.ViewModel.EnableSendEmail = false;
                                //Ticket #12872 End. By Nikhil
                                paymentpage.ViewModel.CustomerModel.SelectedCustomer = invoicemodel.CustomerModel.SelectedCustomer;
                                paymentpage.ViewModel.offers = invoicemodel.offers;
                                paymentpage.ViewModel.AssemblyeConduitTempReceiptList = new List<string>();
                                paymentpage.ViewModel.VantivCloudReceiptList = new List<string>();
                                //paymentpage.ViewModel.updatePaymentList();
                                paymentpage.ViewModel.CurrentRegister = Settings.CurrentRegister;
                                //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                                //paymentpage.ViewModel.calculateInvoiceHeight();
                                //Ticket end: #62808.by rupesh
                            });
                            paymentpage.ViewModel.IsSuccessPaymentActive = false;
                            paymentpage.ViewModel.IsAddPaymentActive = true; //Start #92959 By Pratik
                            await Shell.Current.Navigation.PushAsync(paymentpage, true);
                            paymentpage.ViewModel.OnAppearingCall();



                            //Below condition for Ticket 20924
                            //Ticket start:#89634 Discrepancies with stock following sales.by rupesh
                            if (invoicemodel.Invoice.Status == InvoiceStatus.initial ||
                                invoicemodel.Invoice.Status == InvoiceStatus.Pending)
                                await saleService.UpdateLocalInvoice(invoicemodel.Invoice, LocalInvoiceStatus.Processing);
                            //Ticket End:#89634

                            invoicemodel.Invoice.TenderAmount = invoicemodel.Invoice.NetAmount - invoicemodel.Invoice.TotalPaid;
                            invoicemodel.Invoice.StrTenderAmount = invoicemodel.Invoice.TenderAmount.ToString("C");
                            paymentpage.ViewModel.Invoice.TenderAmount = invoicemodel.Invoice.NetAmount - invoicemodel.Invoice.TotalPaid;
                            paymentpage.ViewModel.Invoice.StrTenderAmount = invoicemodel.Invoice.TenderAmount.ToString("C");
                        }
                        else
                        {
                            if (paymentpagePhone == null)
                            {
                                paymentpagePhone = new PaymentPagePhone();
                            }
                            if (paymentpagePhone.ViewModel.PaymentOptionList == null)
                            {
                                paymentpagePhone.ViewModel.PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
                            }
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(100);
                                //  MainThread.BeginInvokeOnMainThread(() =>{
                                paymentpagePhone.ViewModel.AllPaymentOptionList = AllPaymentOptionList;
                                paymentpagePhone.ViewModel.CurrentOutlet = CurrentOutlet;
                                paymentpagePhone.ViewModel.IsItemCountVisible = Settings.StoreGeneralRule.ShowTotalQuantityOfItemsInBasket;
                                paymentpagePhone.ViewModel.GiftCardBalance = string.Empty;
                                //paymentpage.ViewModel.Invoice = JsonConvert.DeserializeObject<InvoiceDto>(JsonConvert.SerializeObject(invoicemodel.Invoice));
                                if (invoicemodel.Table != null && invoicemodel.Invoice.InvoiceFloorTables?.FirstOrDefault() is not { Id: > 0 })
                                {
                                    var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                                    invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                                    invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                                    {
                                        TableId = invoicemodel.Table?.TableId ?? 0,
                                        AssignedDateTime = DateTime.UtcNow,
                                        FloorId = floor == null ? 0 : floor.Id,
                                        FloorName = floor?.Name,
                                        TableName = invoicemodel.Table?.Name
                                    });
                                    invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;

                                }
                                paymentpagePhone.ViewModel.Invoice = invoicemodel.Invoice;
                                //if (paymentpage.ViewModel.Invoice?.InvoiceLineItems != null && paymentpage.ViewModel.Invoice.InvoiceLineItems.Count >= 16)
                                //    paymentpage.ViewModel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(paymentpage.ViewModel.Invoice.InvoiceLineItems.Take(16));
                                //else
                                paymentpagePhone.ViewModel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(paymentpagePhone.ViewModel.Invoice.InvoiceLineItems);
                                //Ticket #12872 Emailed invoice has partial info. By Nikhil	 
                                paymentpagePhone.ViewModel.EnableSendEmail = false;
                                //Ticket #12872 End. By Nikhil
                                paymentpagePhone.ViewModel.CustomerModel.SelectedCustomer = invoicemodel.CustomerModel.SelectedCustomer;
                                paymentpagePhone.ViewModel.offers = invoicemodel.offers;
                                paymentpagePhone.ViewModel.AssemblyeConduitTempReceiptList = new List<string>();
                                paymentpagePhone.ViewModel.VantivCloudReceiptList = new List<string>();
                                //paymentpage.ViewModel.updatePaymentList();
                                paymentpagePhone.ViewModel.CurrentRegister = Settings.CurrentRegister;
                                //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                                //paymentpage.ViewModel.calculateInvoiceHeight();
                                //Ticket end: #62808.by rupesh
                                //  });
                            });
                            paymentpagePhone.ViewModel.IsSuccessPaymentActive = false;
                            paymentpagePhone.ViewModel.IsAddPaymentActive = true; //Start #92959 By Pratik
                            await NavigationService.PushAsync(paymentpagePhone, true);
                            paymentpagePhone.ViewModel.OnAppearingCall();

                            //Below condition for Ticket 20924
                            //Ticket start:#89634 Discrepancies with stock following sales. by rupesh
                            if (invoicemodel.Invoice.Status == InvoiceStatus.initial ||
                                invoicemodel.Invoice.Status == InvoiceStatus.Pending)
                                await saleService.UpdateLocalInvoice(invoicemodel.Invoice, LocalInvoiceStatus.Processing);
                            //Ticket end:#89634

                            invoicemodel.Invoice.TenderAmount = invoicemodel.Invoice.NetAmount - invoicemodel.Invoice.TotalPaid;
                            invoicemodel.Invoice.StrTenderAmount = invoicemodel.Invoice.TenderAmount.ToString("C");
                            paymentpagePhone.ViewModel.Invoice.TenderAmount = invoicemodel.Invoice.NetAmount - invoicemodel.Invoice.TotalPaid;
                            paymentpagePhone.ViewModel.Invoice.StrTenderAmount = invoicemodel.Invoice.TenderAmount.ToString("C");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                Logger.SaleLogger("Payment Exception Msg - " + ex.Message + "||||" + ex.StackTrace);
            }
        }
        async void SearchProduct()
        {
            if (IsBusy)
                return;
            try
            {

                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0
                    && NavigationService.ModalStack.LastOrDefault() is SearchProductPage)
                    return;

                if (searchProductPage == null)
                {
                    searchProductPage = new SearchProductPage();
                    searchProductPage.ViewModel.ProductSelected += async (object sender1, EnterSaleItemDto e1) =>
                    {
                        await searchProductPage.ViewModel.CloseAsyncTapped();

                        if (e1 != null || e1 is EnterSaleItemDto)
                        {
                            EnterSaleItemDto data = ((EnterSaleItemDto)e1);
                            if (data.ItemType == Enums.InvoiceItemType.Standard && data.Product != null)
                            {
                                AddProductToSale(data.Product);
                            }
                            //else if (data.ItemType == InvoiceItemType.Category && data.Category != null)
                            //{
                            //    SelectedCategory = data.Category;
                            //}
                            else if (data.ItemType == Enums.InvoiceItemType.Composite && data.Offer != null)
                            {
                                AddOfferToSale(data.Offer);
                            }
                        }

                    };
                }

                searchProductPage.ViewModel.AllProduct = AllProducts;
                searchProductPage.ViewModel.AllOffers = Offers;
                //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                searchProductPage.ViewModel.AllUnitOfMeasures = AllUnitOfMeasures;
                //Ticket end:#20064 .by rupesh

                IsOpenPopup = true;
                await NavigationService.PushModalAsync(searchProductPage);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        async void SelectInvoiceItem(InvoiceLineItemDto lineitem)
        {
            try
            {
                if (EventCallRunning || lineitem == null)
                    return;


                EventCallRunning = true;
                if (!lineitem.isEnable)
                {
                    return;
                }

                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0
                     && NavigationService.ModalStack.LastOrDefault() is ProductDetailPage)
                    return;

                if (lineitem != null && lineitem.InvoiceItemType != Enums.InvoiceItemType.GiftCard)
                {
                    if (productdetail == null)
                    {
                        productdetail = new ProductDetailPage();
                        productdetail.ViewModel.UpdateInvoiceLineItem += async (object sender, UpdatedInvoiceLineItemMessageCenter e) =>
                        {
                            ((ProductDetailViewModel)sender).ClosePopupTapped();
                            await invoicemodel.updateSellItem(e);
#if IOS
                            if (e?.invoiceLineItemDto != null && !string.IsNullOrEmpty(e.invoiceLineItemDto.Description) && !string.IsNullOrEmpty(e.invoiceLineItemDto.ServedByName))
                            {
                                if (EnterSalePage != null)
                                    EnterSalePage.ReSizeListView();
                                else if (EnterSalePagePhone != null)
                                    EnterSalePagePhone.ReSizeListView();
                            }
#endif
                        };
                    }

                    productdetail.ViewModel.InvoiceItem = lineitem.Copy();
                    SelectedInvoiceItem = null;

                    productdetail.ViewModel.Invoice = invoicemodel.Invoice;

                    //Start ticket#103384 
                    if (productdetail.ViewModel.Invoice?.CustomerGroupId != null)
                    {
                        var customerGroup = customerService.GetLocalCustomerGroupById(productdetail.ViewModel.Invoice.CustomerGroupId.Value);
                        if (customerGroup.CustomerGroupDiscountType == 3)
                        {
                            productdetail.ViewModel.InvoiceItem.MarkupValue = customerGroup.DiscountPercent;
                        }
                    }
                    //End ticket#103384 

                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            IsOpenPopup = true;
                            await NavigationService.PushModalAsync(productdetail);
                        });
                    }
                    else
                    {
                        IsOpenPopup = true;
                        await NavigationService.PushModalAsync(productdetail);
                    }
                    //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                    if (productdetail.ViewModel.InvoiceItem.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
                        productdetail.ViewModel.ProductDetail = productService.GetLocalUnitOfMeasureProduct(lineitem.InvoiceItemValue);
                    else
                        productdetail.ViewModel.ProductDetail = productService.GetLocalProduct(lineitem.InvoiceItemValue);
                    //Ticket end:#20064 .by rupesh

                    //Ticket #13151 Product images don't load in details screen in iPad.By Nikhil
                    //Ticket #14059 iPad: Custom sale on POS/Cart cannot be edited by rupesh
                    if (lineitem.InvoiceItemImage != null)
                        productdetail.ViewModel.ProductDetail.ItemImageUrl = lineitem.InvoiceItemImage;
                    //Ticket #14059 End. by rupesh
                    //Ticket #13151 End.By Nikhil

                    //#36960 iPad: deleted tax showing under the product in cart.
                    var taxtList = taxServices.GetLocalTaxes();
                    productdetail.ViewModel.TaxList = taxtList;
                    //#36960 iPad: deleted tax showing under the product in cart.


                    if (productdetail.ViewModel.TaxList != null)
                    {
                        var selectedTax = productdetail.ViewModel.TaxList.FirstOrDefault(x => x.Id == lineitem.TaxId);
                        if (selectedTax != null)
                            productdetail.ViewModel.SelectedTax = selectedTax;
                        //Ticket start:#43243 iPad: deleted tax showing under the product in cart after data sync.by rupesh
                        else
                            productdetail.ViewModel.SelectedTax = productdetail.ViewModel.TaxList.FirstOrDefault(x => x.Id ==
                         (productdetail.ViewModel.ProductDetail.ProductOutlet?.TaxID ?? 1));
                        //Ticket end:#43243.by rupesh

                    }

                    //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil
                    if (invoicemodel.Invoice.CustomerDetail != null)
                        productdetail.ViewModel.CanChangeTax = !invoicemodel.Invoice.CustomerDetail.ToAllowForTaxExempt;
                    //Ticket #10921 End. By Nikhil


                    //await NavigationService.PushModalAsync(productdetail);

                }

                EventCallRunning = false;
            }
            catch (Exception ex)
            {
                EventCallRunning = false;
                ex.Track();
            }
        }

        async void ShowOpenRegister()
        {
            try
            {
                //Ticket start:#74634 iOS FR: Pin Restriction.by rupesh
                if (!IsOpenRegisterPermission)
                {
                    App.Instance.Hud.DisplayToast("You do not have sufficient permission to open register", Colors.Red, Colors.White);
                    return;
                }
                //Ticket end:#74634 .by rupesh

                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0
                    && NavigationService.ModalStack.LastOrDefault() is OpenRegisterPage)
                    return;

                if (openRegisterPage == null)
                {
                    openRegisterPage = new OpenRegisterPage();
                    openRegisterPage.ViewModel.OpenRegisterResult += (sender, e) =>
                    {
                        IsOpenRegister = e;
                    };
                }

                await NavigationService.PushModalAsync(openRegisterPage);
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        //Ticket start:#37205 iOS - Sorting Products on Process Sale Page.by rupesh
        public void SelectSortingFilterMenu(string selectedMenu)
        {
            try
            {
                SelectedSortingMenu = selectedMenu;
                IsOpenSortingFilterPopUp = 0;

                //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                if (SelectedCategory.ISLayout)
                {
                    EnterSaleItems = tmpEnterSaleItems;
                    return;
                }
                //End #90945 By Pratik

                //Start #93039 Product sorting on the POS screen doesn't work / #92766 FR POS - BE ABLE TO GO BACK By Pratik
                var backitem = tmpEnterSaleItems.Where(x => x.ItemType == InvoiceItemType.Back);
                var entersalesdata = tmpEnterSaleItems.Where(x => x.ItemType == InvoiceItemType.Category);
                var uomproducts = tmpEnterSaleItems.Where(x => x.ItemType != InvoiceItemType.Category && x.ItemType != InvoiceItemType.Back && x.Product != null && x.Product.IsUnitOfMeasure);

                //ObservableCollection<EnterSaleItemDto> tempEnterSaleItems;
                if (SelectedSortingMenu == "CreationTimeOldest")
                {
                    uomproducts = uomproducts.OrderBy(x => x.Product != null ? x.Product.Id : 0);
                    entersalesdata = backitem.Concat<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(uomproducts));
                    tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(tmpEnterSaleItems.Where(x => x.ItemType != InvoiceItemType.Category && x.ItemType != InvoiceItemType.Back && x.Product != null && !x.Product.IsUnitOfMeasure).OrderBy(x => x.Product != null ? x.Product.Id : 0)));
                }
                else if (SelectedSortingMenu == "CreationTimeNewest")
                {
                    uomproducts = uomproducts.OrderByDescending(x => x.Product != null ? x.Product.Id : 0);
                    entersalesdata = backitem.Concat<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(uomproducts));
                    tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(tmpEnterSaleItems.Where(x => x.ItemType != InvoiceItemType.Category && x.ItemType != InvoiceItemType.Back && x.Product != null && !x.Product.IsUnitOfMeasure).OrderByDescending(x => x.Product != null ? x.Product.Id : 0)));
                }
                else if (SelectedSortingMenu == "SKUAtoZ")
                {
                    uomproducts = uomproducts.OrderBy(x => x.Product != null ? x.Product.Sku : "0");
                    entersalesdata = backitem.Concat<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(uomproducts));
                    tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(tmpEnterSaleItems.Where(x => x.ItemType != InvoiceItemType.Category && x.ItemType != InvoiceItemType.Back && x.Product != null && !x.Product.IsUnitOfMeasure).OrderBy(x => x.Product != null ? x.Product.Sku : "0")));
                }
                else if (SelectedSortingMenu == "SKUZtoA")
                {
                    uomproducts = uomproducts.OrderByDescending(x => x.Product != null ? x.Product.Sku : "0");
                    entersalesdata = backitem.Concat<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(uomproducts));
                    tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(tmpEnterSaleItems.Where(x => x.ItemType != InvoiceItemType.Category && x.ItemType != InvoiceItemType.Back && x.Product != null && !x.Product.IsUnitOfMeasure).OrderByDescending(x => x.Product != null ? x.Product.Sku : "0")));
                }
                else if (SelectedSortingMenu == "ProductNameAtoZ")
                {
                    uomproducts = uomproducts.OrderBy(x => x.Product != null ? x.Product.Name : "0");
                    entersalesdata = backitem.Concat<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(uomproducts));
                    tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(tmpEnterSaleItems.Where(x => x.ItemType != InvoiceItemType.Category && x.ItemType != InvoiceItemType.Back && x.Product != null && !x.Product.IsUnitOfMeasure).OrderBy(x => x.Product != null ? x.Product.Name : "0")));
                }
                else if (SelectedSortingMenu == "ProductNameZtoA")
                {
                    uomproducts = uomproducts.OrderByDescending(x => x.Product != null ? x.Product.Name : "0");
                    entersalesdata = backitem.Concat<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(uomproducts));
                    tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(tmpEnterSaleItems.Where(x => x.ItemType != InvoiceItemType.Category && x.ItemType != InvoiceItemType.Back && x.Product != null && !x.Product.IsUnitOfMeasure).OrderByDescending(x => x.Product != null ? x.Product.Name : "0")));
                }
                //Ticket start:#43112 iPad: retail price (low-high) and (high-low) not working for UOM products.by rupesh
                else if (SelectedSortingMenu == "RetailPriceHighToLow")
                {
                    uomproducts = uomproducts.OrderByDescending(x => x.Product != null ? x.Product.ProductOutlet.SellingPrice : decimal.MaxValue);
                    entersalesdata = backitem.Concat<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(uomproducts));
                    tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(tmpEnterSaleItems.Where(x => x.ItemType != InvoiceItemType.Category && x.ItemType != InvoiceItemType.Back && x.Product != null && !x.Product.IsUnitOfMeasure).OrderByDescending(x => x.Product != null ? x.Product.ProductOutlet.SellingPrice : decimal.MaxValue)));
                }
                else
                {
                    uomproducts = uomproducts.OrderBy(x => x.Product != null ? x.Product.ProductOutlet.SellingPrice : decimal.MaxValue);
                    entersalesdata = backitem.Concat<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(uomproducts));
                    tmpEnterSaleItems = new ObservableCollection<EnterSaleItemDto>(entersalesdata.Concat<EnterSaleItemDto>(tmpEnterSaleItems.Where(x => x.ItemType != InvoiceItemType.Category && x.ItemType != InvoiceItemType.Back && x.Product != null && !x.Product.IsUnitOfMeasure).OrderBy(x => x.Product != null ? x.Product.ProductOutlet.SellingPrice : decimal.MinValue)));
                }
                //End #93039 #92766 By Pratik
                //Ticket end:#43112 .by rupesh
                //  entersalepage.waiting = true;
                EnterSaleItems = tmpEnterSaleItems;
                // entersalepage.waiting = true;

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //Ticket end:#37205 .by rupesh

        //Start ticket #60456 Pratik
        public PaymentPage ParkPayment()
        {

            try
            {
                if (IsOpenBackground == 0 && invoicemodel != null && invoicemodel.Invoice != null && invoicemodel.Invoice.InvoiceLineItems != null && invoicemodel.Invoice.InvoiceLineItems.Count > 0)
                {
                    if (_navigationService.NavigatedPage != null && _navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel>)
                    {
                        invoicemodel.Invoice.TenderAmount = invoicemodel.Invoice.NetAmount - invoicemodel.Invoice.TotalPaid;
                        invoicemodel.Invoice.StrTenderAmount = invoicemodel.Invoice.TenderAmount.ToString("C");
                        if (paymentpage == null)
                        {
                            paymentpage = new PaymentPage();
                        }
                        if (paymentpage.ViewModel.PaymentOptionList == null)
                        {
                            paymentpage.ViewModel.PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
                        }

                        paymentpage.ViewModel.CurrentOutlet = CurrentOutlet;
                        paymentpage.ViewModel.AllPaymentOptionList = AllPaymentOptionList;
                        paymentpage.ViewModel.IsItemCountVisible = Settings.StoreGeneralRule.ShowTotalQuantityOfItemsInBasket;
                        paymentpage.ViewModel.GiftCardBalance = string.Empty;
                        paymentpage.ViewModel.Invoice = invoicemodel.Invoice;
                        paymentpage.ViewModel.EnableSendEmail = false;
                        paymentpage.ViewModel.CustomerModel.SelectedCustomer = invoicemodel.CustomerModel.SelectedCustomer;
                        paymentpage.ViewModel.offers = invoicemodel.offers;
                        paymentpage.ViewModel.AssemblyeConduitTempReceiptList = new List<string>();
                        paymentpage.ViewModel.VantivCloudReceiptList = new List<string>();
                        paymentpage.ViewModel.CurrentRegister = Settings.CurrentRegister;
                        paymentpage.ViewModel.GeneralShopDto = Settings.StoreShopDto;
                        paymentpage.ViewModel.ShopGeneralRule = Settings.StoreGeneralRule;
                        paymentpage.ViewModel.Subscription = Settings.Subscription;
                        paymentpage.ViewModel.CurrentUser = Settings.CurrentUser;
                        //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                        //paymentpage.ViewModel.calculateInvoiceHeight();
                        //Ticket end: #62808 iPad:Print Receipt spacing issues.by rupesh
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return paymentpage;

        }
        public PaymentPagePhone ParkPaymentPhone()
        {

            try
            {
                if (IsOpenBackground == 0 && invoicemodel != null && invoicemodel.Invoice != null && invoicemodel.Invoice.InvoiceLineItems != null && invoicemodel.Invoice.InvoiceLineItems.Count > 0)
                {
                    if (_navigationService.CurrentPage != null && _navigationService.CurrentPage is BaseContentPage<EnterSaleViewModel>)
                    {
                        invoicemodel.Invoice.TenderAmount = invoicemodel.Invoice.NetAmount - invoicemodel.Invoice.TotalPaid;
                        invoicemodel.Invoice.StrTenderAmount = invoicemodel.Invoice.TenderAmount.ToString("C");
                        if (paymentpagePhone == null)
                        {
                            paymentpagePhone = new PaymentPagePhone();
                        }
                        if (paymentpagePhone.ViewModel.PaymentOptionList == null)
                        {
                            paymentpagePhone.ViewModel.PaymentOptionList = new ObservableCollection<PaymentOptionDto>();
                        }

                        paymentpagePhone.ViewModel.CurrentOutlet = CurrentOutlet;
                        paymentpagePhone.ViewModel.AllPaymentOptionList = AllPaymentOptionList;
                        paymentpagePhone.ViewModel.IsItemCountVisible = Settings.StoreGeneralRule.ShowTotalQuantityOfItemsInBasket;
                        paymentpagePhone.ViewModel.GiftCardBalance = string.Empty;
                        paymentpagePhone.ViewModel.Invoice = invoicemodel.Invoice;
                        paymentpagePhone.ViewModel.EnableSendEmail = false;
                        paymentpagePhone.ViewModel.CustomerModel.SelectedCustomer = invoicemodel.CustomerModel.SelectedCustomer;
                        paymentpagePhone.ViewModel.offers = invoicemodel.offers;
                        paymentpagePhone.ViewModel.AssemblyeConduitTempReceiptList = new List<string>();
                        paymentpagePhone.ViewModel.VantivCloudReceiptList = new List<string>();
                        paymentpagePhone.ViewModel.CurrentRegister = Settings.CurrentRegister;
                        paymentpagePhone.ViewModel.GeneralShopDto = Settings.StoreShopDto;
                        paymentpagePhone.ViewModel.ShopGeneralRule = Settings.StoreGeneralRule;
                        paymentpagePhone.ViewModel.Subscription = Settings.Subscription;
                        paymentpagePhone.ViewModel.CurrentUser = Settings.CurrentUser;
                        //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                        //paymentpage.ViewModel.calculateInvoiceHeight();
                        //Ticket end: #62808 iPad:Print Receipt spacing issues.by rupesh
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return paymentpagePhone;

        }

        //End ticket #60456 Pratik

        public void UpdateViews()
        {
            try
            {

                if (Settings.StoreGeneralRule != null)
                {
                    ShopGeneralRule = Settings.StoreGeneralRule;
                }

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
                else
                {
                    IsOpenRegister = false;
                }

                if (Settings.GrantedPermissionNames != null)
                {
                    //Start Ticket #73185 iPad - Feature: Parked Option, By:Pratik
                    invoicemodel.IsParkSale = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.Park"));
                    //End Ticket #73185, By:Pratik
                    invoicemodel.PermissionDoCustomSale = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.CustomSale")) && invoicemodel?.Invoice?.Status != InvoiceStatus.BackOrder;//Ticket :#84289. by rupesh

                    //Ticket start:#74634 iOS FR: Pin Restriction.by rupesh
                    IsOpenRegisterPermission = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.OpenRegister");
                    //Ticket end:#74634.by rupesh

                    bool isDiscountPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.TogiveDiscount"));
                    if (isDiscountPermission && ShopGeneralRule != null & ShopGeneralRule.EnableDiscount)
                    {
                        DiscountEnabled = true;
                        //Ticket #1998 by rupesh
                        //Start : Changhes made to resolve crash. By Nikhil.
                        if (invoicemodel != null && invoicemodel.Invoice != null
                            && (invoicemodel.Invoice.Status == InvoiceStatus.Exchange || invoicemodel.Invoice.Status == InvoiceStatus.Refunded))
                        {
                            DiscountEnabled = false;
                        }
                    }
                    else
                    {
                        DiscountEnabled = false;
                    }

                    bool isTaxRemovePermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.ToremoveTax"));
                    if (isTaxRemovePermission && ShopGeneralRule != null)
                    {
                        TaxEnabled = true;
                    }
                    else
                    {
                        TaxEnabled = false;
                    }

                    bool isTipPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.TogiveTips"));
                    if (isTipPermission && ShopGeneralRule != null & ShopGeneralRule.EnableTips)
                    {
                        TipVisible = true;
                    }
                    else
                    {
                        TipVisible = false;
                    }

                    if (ShopGeneralRule != null & ShopGeneralRule.AllowShippingTaxOnPOS)
                    {
                        ShippingVisible = true;
                    }
                    else
                    {
                        ShippingVisible = false;
                    }

                    //invoicemodel.PermissionGiftCardSale = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.GiftCard" || s == "Pages.Tenant.POS.EnterSale")) && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EnableGiftCard && Settings.Subscription != null && Settings.Subscription.Edition != null && Settings.Subscription.Edition.PlanType != null && Settings.Subscription.Edition.PlanType != PlanType.StartUp;

                    invoicemodel.PermissionGiftCardSale = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale")) && !Settings.IsRestaurantPOS && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EnableGiftCardSelling && Settings.StoreGeneralRule.EnableGiftCard && Settings.Subscription != null && Settings.Subscription.Edition != null && Settings.Subscription.Edition.PlanType != null && Settings.Subscription.Edition.PlanType != PlanType.StartUp && invoicemodel?.Invoice?.Status != InvoiceStatus.BackOrder; //#94565//Ticket :#84289. by rupesh
                    //Ticket start:#37205 iOS - Sorting Products on Process Sale Page.by rupesh
                    IsHavingSortingEnabled = CheckSortingProductsOnPOSFeaturePermisson();
                    //Ticket end:#37205 .by rupesh
                    //Ticket start:#43972 OPEN CASH OPTION CAN ABLE TO DISABLE.by rupesh
                    invoicemodel.PermissionOpenCashDrawer = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.OpenCashDrawer");
                    //Ticket end:#43972 .by rupesh
                    var voidSalePermission = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.VoidSale");
                    //Ticket start:#77144. by rupesh
                    if (invoicemodel != null && invoicemodel.Invoice != null && (invoicemodel.Invoice.Status == InvoiceStatus.Parked || invoicemodel.Invoice.Status == InvoiceStatus.LayBy || invoicemodel.Invoice.Status == InvoiceStatus.OnGoing || invoicemodel.Invoice.Status == InvoiceStatus.BackOrder || invoicemodel.Invoice.Status == InvoiceStatus.OnAccount || invoicemodel.Invoice.Status == InvoiceStatus.Pending || invoicemodel.Invoice.Status == InvoiceStatus.Refunded) &&
                        invoicemodel.Invoice.InvoicePayments != null && invoicemodel.Invoice.InvoicePayments.Any(x => !x.IsDeleted))
                    {
                        invoicemodel.DiscardVisible = false;
                    }
                    else
                    {
                        invoicemodel.DiscardVisible = voidSalePermission;
                    }

                    if (invoicemodel != null && invoicemodel.Invoice != null && (invoicemodel.Invoice.Status == InvoiceStatus.Parked || invoicemodel.Invoice.Status == InvoiceStatus.LayBy || invoicemodel.Invoice.Status == InvoiceStatus.BackOrder || invoicemodel.Invoice.Status == InvoiceStatus.OnAccount) &&
                        invoicemodel.Invoice.InvoicePayments != null && invoicemodel.Invoice.InvoicePayments.Any() && invoicemodel.Invoice.Status != InvoiceStatus.BackOrder) //Ticket :#84289. by rupesh
                    {
                        invoicemodel.DiscardAndVoidVisible = voidSalePermission;
                    }
                    else
                    {
                        invoicemodel.DiscardAndVoidVisible = false;
                    }
                    //Ticket end:#77144. by rupesh

                }
                if (invoicemodel == null || invoicemodel.Invoice == null || (invoicemodel.Invoice.CustomerId == 0 && invoicemodel.Invoice.CustomerTempId == null) || invoicemodel.Invoice.CustomerId == null)
                {
                    EditCustomerVisible = false;
                    RemoveCustomerVisible = false;
                    //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
                    DeliveryCustomerVisible = false;
                    //Ticket end:#26664
                }
                else
                {
                    if (Settings.GrantedPermissionNames != null)
                    {
                        //Start #77581 Edit field should be disable if User is not granted with Permission as per web by Pratik
                        bool isEditPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.Customers.Customer.Edit"));
                        //End #77581 by Pratik
                        if (invoicemodel != null && invoicemodel.Invoice != null && invoicemodel.Invoice.CustomerId != null && (invoicemodel.Invoice.CustomerId > 0 || !string.IsNullOrEmpty(invoicemodel.Invoice.CustomerTempId)))
                        {
                            if (isEditPermission)
                                EditCustomerVisible = true;
                            else
                                EditCustomerVisible = false;
                            RemoveCustomerVisible = true;
                        }
                        else
                        {
                            EditCustomerVisible = false;
                            RemoveCustomerVisible = false;
                        }
                    }

                    if (invoicemodel != null && invoicemodel.Invoice.Status == InvoiceStatus.OnAccount)
                    {
                        EditCustomerVisible = false;
                        RemoveCustomerVisible = false;
                    }

                    //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
                    if (Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer && !Settings.IsQuoteSale)
                        DeliveryCustomerVisible = CheckDelieveryAddressPermisson();
                    else
                        DeliveryCustomerVisible = false;
                    //Ticket end:#26664 .by rupesh
                    //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time.by rupesh
                    if (invoicemodel.Invoice.IsEditBackOrderFromSaleHistory)
                    {
                        RemoveCustomerVisible = false;
                    }
                    //Ticket end:#84289.by rupesh

                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private bool CheckSortingProductsOnPOSFeaturePermisson()
        {
            try
            {
                System.Reflection.PropertyInfo myPropInfo;
                bool result = false;
                myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikePOSSortingProductsOnPOSFeature");
                bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out result);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return true;
            }
        }

        //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
        private bool CheckDelieveryAddressPermisson()
        {
            try
            {
                System.Reflection.PropertyInfo myPropInfo;
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
        //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.

        public void CustomerNameLableChanged()
        {
            bool isEditPermission = true;
            if (Settings.GrantedPermissionNames != null)
            {
                //Start #77581 Edit field should be disable if User is not granted with Permission as per web by Pratik
                isEditPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.Customers.Customer.Edit"));
                //End #77581 by Pratik 
            }
            if (invoicemodel != null && invoicemodel.Invoice != null && invoicemodel.Invoice.CustomerId != null && (invoicemodel.Invoice.CustomerId > 0 || !string.IsNullOrEmpty(invoicemodel.Invoice.CustomerTempId)))
            {
                if (isEditPermission)
                    EditCustomerVisible = true;
                else
                    EditCustomerVisible = false;
                RemoveCustomerVisible = true;

                //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
                if (Settings.StoreGeneralRule.RequireDeliveryAddressTocustomer && !Settings.IsQuoteSale)
                {
                    DeliveryCustomerVisible = CheckDelieveryAddressPermisson();
                }

            }
            else
            {
                EditCustomerVisible = false;
                RemoveCustomerVisible = false;
                DeliveryCustomerVisible = false;
                //Ticket end:#26664 .by rupesh
            }
        }
        #endregion

        #region SignalR Methods
        //pubnub replace with ably
        PubNubService pubNubService;
        public void SetupPubNub()
        {

            if (!App.Instance.IsInternetConnected)
                return;

            try
            {
                if (pubNubService == null)
                {
                    //pubnub replace with ably
                    //pubNubService = new PubNubService();
                    pubNubService = new PubNubService();
                    pubNubService.CustomerDeleted += SignalRListener_CustomerDeleted;
                    pubNubService.CustomerReceived += SignalRListener_CustomerReceived;

                    pubNubService.CategoryReceived += SignalRListener_CategoryReceived;
                    pubNubService.CategoryDeleted += SignalRListener_CategoryDeleted;
                    pubNubService.CategoryActiveDeActive += SignalRListener_CategoryActiveDeActive;

                    pubNubService.ProductStockReceived += SignalRListener_ProductStockReceived;
                    pubNubService.ProductDeleted += SignalRListener_ProductDeleted;
                    pubNubService.ProductReceived += SignalRListener_ProductReceived;
                    pubNubService.ProductActiveDeActive += SignalRListener_ProductActiveDeActive;


                    //#32293 iOS - Sync of Sales Details After App Crashing
                    // pubNubService.InvoiceReceived += SignalRListener_InvoiceReceived;


                    //pubNubService.InvoiceDeleted += SignalRListener_InvoiceDeleted;

                    pubNubService.OfferReceived += SignalRListener_OfferReceived;
                    pubNubService.OfferDeleted += SignalRListener_OfferDeleted;
                    pubNubService.OfferActiveDeActive += SignalRListener_OfferActiveDeActive;
                    pubNubService.OfferAssociateToProducts += PubNubService_OfferAssociateToProducts;

                    pubNubService.ShopSettingsReceived += SignalRListener_ShopSettingsReceived;
                    pubNubService.RegisterReceived += SignalRListener_RegisterReceived;

                    pubNubService.UserReceived += SignalRListener_UserReceived;

                    pubNubService.PaymentTypesReceived += SignalRListener_PaymentTypesReceived;
                    pubNubService.ReceiptTemplateReceived += SignalRListener_ReceiptTemplateReceived;
                    pubNubService.ProductWithVariantsReceived += SignalRListener_ProductWithVariantsReceived;
                }

                if (ablyService != null)
                {

                    ablyService.CustomerDeleted -= SignalRListener_CustomerDeleted;
                    ablyService.CustomerReceived -= SignalRListener_CustomerReceived;

                    ablyService.CategoryReceived -= SignalRListener_CategoryReceived;
                    ablyService.CategoryDeleted -= SignalRListener_CategoryDeleted;
                    ablyService.CategoryActiveDeActive -= SignalRListener_CategoryActiveDeActive;

                    ablyService.ProductStockReceived -= SignalRListener_ProductStockReceived;
                    ablyService.ProductDeleted -= SignalRListener_ProductDeleted;
                    ablyService.ProductReceived -= SignalRListener_ProductReceived;
                    ablyService.ProductActiveDeActive -= SignalRListener_ProductActiveDeActive;

                    ablyService.InvoiceReceived -= SignalRListener_InvoiceReceived;
                    //ablyService.InvoiceDeleted += SignalRListener_InvoiceDeleted;

                    ablyService.OfferReceived -= SignalRListener_OfferReceived;
                    ablyService.OfferDeleted -= SignalRListener_OfferDeleted;
                    ablyService.OfferActiveDeActive -= SignalRListener_OfferActiveDeActive;
                    ablyService.OfferAssociateToProducts += PubNubService_OfferAssociateToProducts;

                    ablyService.ShopSettingsReceived -= SignalRListener_ShopSettingsReceived;
                    ablyService.RegisterReceived -= SignalRListener_RegisterReceived;

                    ablyService.UserReceived -= SignalRListener_UserReceived;

                    ablyService.PaymentTypesReceived -= SignalRListener_PaymentTypesReceived;
                    ablyService.ReceiptTemplateReceived -= SignalRListener_ReceiptTemplateReceived;
                    ablyService.ProductWithVariantsReceived -= SignalRListener_ProductWithVariantsReceived;
                    ablyService = null;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        AblyService ablyService;
        private string tdConfiguration;

        public void SetupAbly()
        {
            if (!App.Instance.IsInternetConnected)
                return;

            try
            {

                if (ablyService == null)
                {
                    ablyService = new AblyService();
                    ablyService.CustomerDeleted += SignalRListener_CustomerDeleted;
                    ablyService.CustomerReceived += SignalRListener_CustomerReceived;

                    ablyService.CategoryReceived += SignalRListener_CategoryReceived;
                    ablyService.CategoryDeleted += SignalRListener_CategoryDeleted;
                    ablyService.CategoryActiveDeActive += SignalRListener_CategoryActiveDeActive;

                    ablyService.ProductStockReceived += SignalRListener_ProductStockReceived;
                    ablyService.ProductDeleted += SignalRListener_ProductDeleted;
                    ablyService.ProductReceived += SignalRListener_ProductReceived;
                    ablyService.ProductActiveDeActive += SignalRListener_ProductActiveDeActive;

                    //#32293 iOS - Sync of Sales Details After App Crashing

                    ablyService.InvoiceReceived += SignalRListener_InvoiceReceived;


                    //ablyService.InvoiceDeleted += SignalRListener_InvoiceDeleted;

                    ablyService.OfferReceived += SignalRListener_OfferReceived;
                    ablyService.OfferDeleted += SignalRListener_OfferDeleted;
                    ablyService.OfferActiveDeActive += SignalRListener_OfferActiveDeActive;
                    ablyService.OfferAssociateToProducts += PubNubService_OfferAssociateToProducts;

                    ablyService.ShopSettingsReceived += SignalRListener_ShopSettingsReceived;
                    ablyService.RegisterReceived += SignalRListener_RegisterReceived;

                    ablyService.UserReceived += SignalRListener_UserReceived;

                    ablyService.PaymentTypesReceived += SignalRListener_PaymentTypesReceived;
                    ablyService.ReceiptTemplateReceived += SignalRListener_ReceiptTemplateReceived;
                    ablyService.ProductWithVariantsReceived += SignalRListener_ProductWithVariantsReceived;
                }

                if (pubNubService != null)
                {

                    pubNubService.CustomerDeleted -= SignalRListener_CustomerDeleted;
                    pubNubService.CustomerReceived -= SignalRListener_CustomerReceived;

                    pubNubService.CategoryReceived -= SignalRListener_CategoryReceived;
                    pubNubService.CategoryDeleted -= SignalRListener_CategoryDeleted;
                    pubNubService.CategoryActiveDeActive -= SignalRListener_CategoryActiveDeActive;

                    pubNubService.ProductStockReceived -= SignalRListener_ProductStockReceived;
                    pubNubService.ProductDeleted -= SignalRListener_ProductDeleted;
                    pubNubService.ProductReceived -= SignalRListener_ProductReceived;
                    pubNubService.ProductActiveDeActive -= SignalRListener_ProductActiveDeActive;

                    pubNubService.InvoiceReceived -= SignalRListener_InvoiceReceived;
                    //pubNubService.InvoiceDeleted -= SignalRListener_InvoiceDeleted;

                    pubNubService.OfferReceived -= SignalRListener_OfferReceived;
                    pubNubService.OfferDeleted -= SignalRListener_OfferDeleted;
                    pubNubService.OfferActiveDeActive -= SignalRListener_OfferActiveDeActive;
                    pubNubService.OfferAssociateToProducts -= PubNubService_OfferAssociateToProducts;

                    pubNubService.ShopSettingsReceived -= SignalRListener_ShopSettingsReceived;
                    pubNubService.RegisterReceived -= SignalRListener_RegisterReceived;

                    pubNubService.UserReceived -= SignalRListener_UserReceived;

                    pubNubService.PaymentTypesReceived -= SignalRListener_PaymentTypesReceived;
                    pubNubService.ReceiptTemplateReceived -= SignalRListener_ReceiptTemplateReceived;
                    pubNubService.ProductWithVariantsReceived -= SignalRListener_ProductWithVariantsReceived;
                    pubNubService = null;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }
        public void ReleaseAbly()
        {
            if (ablyService != null)
            {

                ablyService.CustomerDeleted -= SignalRListener_CustomerDeleted;
                ablyService.CustomerReceived -= SignalRListener_CustomerReceived;

                ablyService.CategoryReceived -= SignalRListener_CategoryReceived;
                ablyService.CategoryDeleted -= SignalRListener_CategoryDeleted;
                ablyService.CategoryActiveDeActive -= SignalRListener_CategoryActiveDeActive;

                ablyService.ProductStockReceived -= SignalRListener_ProductStockReceived;
                ablyService.ProductDeleted -= SignalRListener_ProductDeleted;
                ablyService.ProductReceived -= SignalRListener_ProductReceived;
                ablyService.ProductActiveDeActive -= SignalRListener_ProductActiveDeActive;

                ablyService.InvoiceReceived -= SignalRListener_InvoiceReceived;
                //ablyService.InvoiceDeleted += SignalRListener_InvoiceDeleted;

                ablyService.OfferReceived -= SignalRListener_OfferReceived;
                ablyService.OfferDeleted -= SignalRListener_OfferDeleted;
                ablyService.OfferActiveDeActive -= SignalRListener_OfferActiveDeActive;
                ablyService.OfferAssociateToProducts += PubNubService_OfferAssociateToProducts;

                ablyService.ShopSettingsReceived -= SignalRListener_ShopSettingsReceived;
                ablyService.RegisterReceived -= SignalRListener_RegisterReceived;

                ablyService.UserReceived -= SignalRListener_UserReceived;

                ablyService.PaymentTypesReceived -= SignalRListener_PaymentTypesReceived;
                ablyService.ReceiptTemplateReceived -= SignalRListener_ReceiptTemplateReceived;
                ablyService.ProductWithVariantsReceived -= SignalRListener_ProductWithVariantsReceived;
                ablyService.releaseAblyChannels();
                ablyService = null;
            }
        }
        void SignalRListener_CustomerReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    CustomerDto_POS customer = JsonConvert.DeserializeObject<CustomerDto_POS>(e);
                    if (customer != null)
                    {
                        customerService.UpdateLocalCustomer(customer);
                        CustomerViewModel.AllCustomer = customerService.GetLocalCustomers();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_CustomerDeleted(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    customerService.DeleteLocalCustomer(e);
                    CustomerViewModel.AllCustomer = customerService.GetLocalCustomers();
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_CategoryReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    CategoryDto category = JsonConvert.DeserializeObject<CategoryDto>(e);
                    if (category != null)
                    {
                        var result = productService.UpdateLocalCategory(category);
                        if (result)
                        {
                            if (AllCategories == null)
                            {
                                AllCategories = new List<CategoryDto>();
                            }

                            var tmpAllCategory = AllCategories?.FirstOrDefault(s => s.Id == category.Id);

                            if (tmpAllCategory != null && AllCategories.Contains(tmpAllCategory))
                            {
                                AllCategories.Remove(tmpAllCategory);
                            }

                            if (category.ProductsCount == 0)
                            {
                                category.ProductsCount = 1;
                            }

                            AllCategories.Add(category);

                            if (category.ParentId == null || category.ParentId == 0)
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        Categories = getFilterCategories();
                                        //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                                        if (Categories != null && !Categories.Any(a => a.ISLayout))
                                            getCurrentLayout();
                                        //End #90945 By Pratik
                                    });
                                }
                            }

                            if (EnterSaleItems == null)
                            {
                                EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                            }
                            var ItemCategory = EnterSaleItems.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Category && x.Category != null && x.Category.Id == category.Id);
                            if (ItemCategory == null)
                            {
                                if (category.IsActive && category.ParentId != null && category.ParentId != 0 && SelectedCategory != null && SelectedCategory.Id == category.ParentId)
                                {
                                    if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            try
                                            {
                                                EnterSaleItems.Insert(0, new EnterSaleItemDto()
                                                {
                                                    ItemType = Enums.InvoiceItemType.Category,
                                                    Category = category
                                                });
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Track();
                                            }
                                        });
                                    }
                                }
                            }
                            else
                            {
                                if (category.ParentId == ItemCategory.Category.ParentId)
                                {
                                    ItemCategory.Category.Name = category.Name;
                                    ItemCategory.Category.ParentId = category.ParentId;
                                    ItemCategory.Category.ParentName = category.ParentName;
                                    ItemCategory.Category.Description = category.Description;
                                    ItemCategory.Category.sequence = category.sequence;
                                    ItemCategory.Category.CategoryImageId = category.CategoryImageId;
                                    ItemCategory.Category.CreationTime = category.CreationTime;
                                    ItemCategory.Category.LastModificationTime = category.LastModificationTime;
                                    ItemCategory.Category.ProductsCount = category.ProductsCount;
                                    ItemCategory.Category.SubCategories = category.SubCategories;
                                    ItemCategory.Category.IsActive = category.IsActive;
                                }
                                else
                                {

                                    if (EnterSaleItems.Contains(ItemCategory) && _navigationService.IsCurrentPage<EnterSaleViewModel>())
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            try
                                            {
                                                EnterSaleItems.Remove(ItemCategory);
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Track();
                                            }
                                        });
                                    }
                                }
                            }


                            if (ItemCategory != null && EnterSaleItems.Contains(ItemCategory) && !category.IsActive)
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        try
                                        {
                                            EnterSaleItems.Remove(ItemCategory);
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.Track();
                                        }
                                    });
                                }
                            }

                            if (!category.IsActive && SelectedCategory != null && (SelectedCategory.Id == category.Id || SelectedCategory.ParentId == category.Id))
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        try
                                        {
                                            SelectedCategory = Categories.FirstOrDefault();
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.Track();
                                        }
                                    });
                                }
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_CategoryDeleted(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {

                    var result = productService.DeleteLocalCategory(e);
                    if (result)
                    {
                        int intparseresult;
                        if (!string.IsNullOrEmpty(e) && int.TryParse(e, out intparseresult))
                        {
                            var category = AllCategories?.FirstOrDefault(x => x.Id == intparseresult);
                            if (category != null && AllCategories.Contains(category))
                            {
                                try
                                {
                                    AllCategories.Remove(category);
                                    var tmpcategory = Categories?.FirstOrDefault(x => x.Id == category.Id);
                                    if (tmpcategory != null && Categories.Contains(tmpcategory))
                                    {
                                        if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                        {
                                            MainThread.BeginInvokeOnMainThread(() =>
                                            {
                                                try
                                                {
                                                    Categories.Remove(tmpcategory);
                                                    if (SelectedCategory == tmpcategory)
                                                    {
                                                        SelectedCategory = Categories.FirstOrDefault();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    ex.Track();
                                                }
                                            });
                                        }
                                        SetPropertyChanged(nameof(Categories));
                                    }

                                    if (EnterSaleItems == null)
                                    {
                                        EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                                    }

                                    var item = EnterSaleItems.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Category && x.Category != null && x.Category.Id == category.Id);
                                    if (item != null && EnterSaleItems.Contains(item))
                                    {
                                        if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                        {
                                            MainThread.BeginInvokeOnMainThread(() =>
                                            {
                                                try
                                                {
                                                    EnterSaleItems.Remove(item);
                                                }
                                                catch (Exception ex)
                                                {
                                                    ex.Track();
                                                }
                                            });
                                        }
                                        SetPropertyChanged(nameof(EnterSaleItems));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    ex.Track();
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_CategoryActiveDeActive(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    SignalRActivationResponse response = JsonConvert.DeserializeObject<SignalRActivationResponse>(e);
                    if (response != null && response.Id > 0)
                    {
                        var category = productService.GetLocalCategory(response.Id);
                        if (category != null)
                        {
                            category.IsActive = response.IsActive;
                            productService.UpdateLocalCategory(category);
                            if (AllCategories == null)
                            {
                                AllCategories = new List<CategoryDto>();
                            }

                            var tmpAllCategory = AllCategories?.FirstOrDefault(s => s.Id == category.Id);
                            if (tmpAllCategory != null && AllCategories.Contains(tmpAllCategory))
                            {
                                AllCategories.Remove(tmpAllCategory);
                            }
                            AllCategories.Add(category);

                            if (category.ParentId == null || category.ParentId == 0)
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        Categories = getFilterCategories();
                                        //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                                        if (Categories != null && !Categories.Any(a => a.ISLayout))
                                            getCurrentLayout();
                                        //End #90945 By Pratik
                                    });
                                }
                            }

                            if (EnterSaleItems == null)
                            {
                                EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                            }

                            var ItemCategory = EnterSaleItems.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Category && x.Category != null && x.Category.Id == category.Id);

                            if (ItemCategory == null && category.IsActive && category.ParentId != null && category.ParentId != 0 && SelectedCategory != null && SelectedCategory.Id == category.ParentId)
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        try
                                        {
                                            EnterSaleItems.Insert(0, new EnterSaleItemDto()
                                            {
                                                ItemType = Enums.InvoiceItemType.Category,
                                                Category = category
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.Track();
                                        }
                                    });
                                }
                            }
                            if (ItemCategory != null && EnterSaleItems.Contains(ItemCategory) && !category.IsActive)
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        try
                                        {
                                            EnterSaleItems.Remove(ItemCategory);
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.Track();
                                        }
                                    });
                                }
                            }

                            if (Categories != null && Categories.Any() && !category.IsActive && SelectedCategory != null && (SelectedCategory.Id == category.Id || SelectedCategory.ParentId == category.Id))
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        try
                                        {
                                            SelectedCategory = Categories.FirstOrDefault();
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.Track();
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_ProductReceived(object sender, string e)
        {
            try
            {

                if (!string.IsNullOrEmpty(e))
                {
                    ProductDto_POS product = JsonConvert.DeserializeObject<ProductDto_POS>(e);
                    //if(product.HasVarients)

                    Debug.WriteLine("SignalRListener_ProductReceived  : " + Newtonsoft.Json.JsonConvert.SerializeObject(product));
                    if (product != null && product.ProductOutlets != null && product.ProductOutlets.Any(x => x.OutletId == Settings.SelectedOutletId))
                    {
                        product.ProductOutlet = product.ProductOutlets?.FirstOrDefault(x => x.OutletId == Settings.SelectedOutletId);

                        if (product.HasVarients && product.ProductVarients != null && product.ProductVarients.Count > 0)
                        {
                            product.ProductVarients.All(x =>
                            {
                                if (x.VarientOutlets != null && x.VarientOutlets.Count > 0)
                                {
                                    x.VariantOutlet = x.VarientOutlets.FirstOrDefault();
                                }
                                return true;
                            });
                        }


                        var result = productService.UpdateLocalProduct(product);
                        if (result)
                        {

                            if (AllProducts == null)
                            {
                                AllProducts = new ObservableCollection<ProductDto_POS>();
                            }

                            var tmpAllProduct = AllProducts?.FirstOrDefault(s => s.Id == product.Id);
                            if (tmpAllProduct != null && AllProducts.Contains(tmpAllProduct))
                            {
                                AllProducts.Remove(tmpAllProduct);
                            }
                            if (product.IsActive)
                            {
                                AllProducts.Add(product);
                            }

                            if (EnterSaleItems == null)
                            {
                                EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                            }

                            var ItemProduct = EnterSaleItems?.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Standard && x.Product != null && x.Product.Id == product.Id);
                            if (ItemProduct == null)
                            {
                                if (product.IsActive && product.ProductCategories != null && product.ProductCategories.Count(x => x == SelectedCategory.Id) > 0 && product.ParentId == null)
                                {
                                    if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            try
                                            {
                                                EnterSaleItems.Add(new EnterSaleItemDto()
                                                {
                                                    ItemType = Enums.InvoiceItemType.Standard,
                                                    Product = product
                                                });
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Track();
                                            }
                                        });
                                    }
                                }
                            }
                            else if (ItemProduct != null && EnterSaleItems.Contains(ItemProduct) && (!product.IsActive || product.ProductCategories == null || product.ProductCategories.Count(x => x == SelectedCategory.Id) < 1))
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        try
                                        {
                                            EnterSaleItems.Remove(ItemProduct);
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.Track();
                                        }
                                    });
                                }
                            }
                            else
                            {
                                ItemProduct.Product.ParentId = product.ParentId;
                                ItemProduct.Product.Id = product.Id;
                                ItemProduct.Product.Name = product.Name;
                                ItemProduct.Product.Description = product.Description;
                                ItemProduct.Product.Sku = product.Sku;
                                ItemProduct.Product.BarCode = product.BarCode;
                                ItemProduct.Product.Specification = product.Specification;
                                ItemProduct.Product.Depth = product.Depth;
                                ItemProduct.Product.Width = product.Width;
                                ItemProduct.Product.CustomField = product.CustomField;
                                ItemProduct.Product.Height = product.Height;
                                ItemProduct.Product.Weight = product.Weight;
                                ItemProduct.Product.WeightUnit = product.WeightUnit;
                                ItemProduct.Product.ItemImage = product.ItemImage;
                                ItemProduct.Product.ColorTag = product.ColorTag;
                                ItemProduct.Product.ColorType = product.ColorType;
                                ItemProduct.Product.BrandId = product.BrandId;
                                ItemProduct.Product.BranName = product.BranName;
                                ItemProduct.Product.SeasonId = product.SeasonId;
                                ItemProduct.Product.SeasonName = product.SeasonName;
                                ItemProduct.Product.EnableAccountSyncRule = product.EnableAccountSyncRule;
                                ItemProduct.Product.SupplierCode = product.SupplierCode;
                                ItemProduct.Product.SalesCode = product.SalesCode;
                                ItemProduct.Product.PurchaseCode = product.PurchaseCode;
                                ItemProduct.Product.IsPricingDifferentByOutlet = product.IsPricingDifferentByOutlet;
                                ItemProduct.Product.ProductType = product.ProductType;
                                ItemProduct.Product.HasVarients = product.HasVarients;
                                ItemProduct.Product.TrackInventory = product.TrackInventory;
                                ItemProduct.Product.AllowOutOfStock = product.AllowOutOfStock;
                                ItemProduct.Product.Loyalty = product.Loyalty;
                                ItemProduct.Product.ServiceDuration = product.ServiceDuration;
                                ItemProduct.Product.PrinterLocationID = product.PrinterLocationID;
                                ItemProduct.Product.IsInAnyOffer = product.IsInAnyOffer;
                                ItemProduct.Product.CanEditableInThirdParty = product.CanEditableInThirdParty;
                                ItemProduct.Product.EnableSeo = product.EnableSeo;
                                ItemProduct.Product.MetaTitle = product.MetaTitle;
                                ItemProduct.Product.MetaDesc = product.MetaDesc;
                                ItemProduct.Product.MetaKeyword = product.MetaKeyword;
                                ItemProduct.Product.Slug = product.Slug;
                                ItemProduct.Product.isSelected = product.isSelected;
                                ItemProduct.Product.ProductTags = product.ProductTags;
                                //ItemProduct.Product.ProductCategoryDtos = product.ProductCategoryDtos;
                                ItemProduct.Product.SalesChannels = product.SalesChannels;
                                ItemProduct.Product.ProductOutlet = product.ProductOutlet;
                                ItemProduct.Product.ProductSuppliers = product.ProductSuppliers;
                                ItemProduct.Product.ProductAttributes = product.ProductAttributes;
                                ItemProduct.Product.ProductVarients = product.ProductVarients;
                                ItemProduct.Product.ServiceUsers = product.ServiceUsers;
                                ItemProduct.Product.ProductImages = product.ProductImages;
                                ItemProduct.Product.ProductExtras = product.ProductExtras;
                                ItemProduct.Product.IsActive = product.IsActive;
                                ItemProduct.Product.ProductCategories = product.ProductCategories;
                            }

                            if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    try
                                    {
                                        var catitem = EnterSaleItems?.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Category && x.Category != null && product != null && product.ProductCategories != null && product.ProductCategories.Count(p => p == x.Category.Id) > 0);
                                        if (catitem != null)
                                        {
                                            catitem.Category.ProductsCount = AllProducts.Count(x => x.IsActive && (x.ParentId == 0 || x.ParentId == null) && x.ProductCategories.Count(c => c == catitem.Category.Id && catitem.Category.Id != 0) > 0);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.Track();
                                    }
                                });
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_ProductActiveDeActive(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    SignalRActivationResponse response = JsonConvert.DeserializeObject<SignalRActivationResponse>(e);
                    if (response != null && response.Id > 0)
                    {
                        var product = productService.GetLocalProduct(response.Id);
                        if (product != null)
                        {
                            product.IsActive = response.IsActive;
                            productService.UpdateLocalProduct(product);
                            if (AllProducts == null)
                            {
                                AllProducts = new ObservableCollection<ProductDto_POS>();
                            }

                            var tmpAllproduct = AllProducts?.FirstOrDefault(s => s.Id == product.Id);
                            if (tmpAllproduct != null && AllProducts.Contains(tmpAllproduct))
                            {
                                AllProducts.Remove(tmpAllproduct);
                            }

                            AllProducts.Add(product);
                            if (EnterSaleItems == null)
                            {
                                EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                            }

                            var ItemProduct = EnterSaleItems.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Standard && x.Product != null && x.Product.Id == product.Id);
                            if (ItemProduct == null && SelectedCategory != null && product.IsActive && product.ProductCategories.Count(x => x == SelectedCategory.Id) > 0)
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        try
                                        {
                                            EnterSaleItems.Add(new EnterSaleItemDto()
                                            {
                                                ItemType = Enums.InvoiceItemType.Standard,
                                                Product = product
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.Track();
                                        }
                                    });
                                }
                            }
                            if (ItemProduct != null && !product.IsActive && EnterSaleItems.Contains(ItemProduct))
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        try
                                        {
                                            EnterSaleItems.Remove(ItemProduct);
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.Track();
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_ProductDeleted(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    var result = productService.DeleteLocalProduct(e);
                    if (result)
                    {
                        if (AllProducts == null)
                        {
                            AllProducts = new ObservableCollection<ProductDto_POS>();
                        }
                        int intparseresult;
                        if (!string.IsNullOrEmpty(e) && int.TryParse(e, out intparseresult))
                        {
                            var product = AllProducts?.FirstOrDefault(x => x.Id == intparseresult);
                            if (product != null && AllProducts.Contains(product))
                            {
                                try
                                {
                                    AllProducts.Remove(product);
                                    if (EnterSaleItems == null)
                                    {
                                        EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                                    }

                                    var item = EnterSaleItems.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Standard && x.Product != null && x.Product.Id == product.Id);
                                    if (item != null && EnterSaleItems.Contains(item))
                                    {
                                        if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                        {
                                            MainThread.BeginInvokeOnMainThread(() =>
                                            {
                                                try
                                                {
                                                    EnterSaleItems.Remove(item);
                                                }
                                                catch (Exception ex)
                                                {
                                                    ex.Track();
                                                }
                                            });
                                        }
                                        SetPropertyChanged(nameof(EnterSaleItems));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    ex.Track();
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_ProductStockReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    SignalRProductOutletDto productOutlet = JsonConvert.DeserializeObject<SignalRProductOutletDto>(e);
                    if (productOutlet != null && productOutlet.OutletId == Settings.SelectedOutletId)
                    {

                        if (AllProducts == null)
                        {
                            AllProducts = new ObservableCollection<ProductDto_POS>();
                        }
                        var product = productService.GetLocalProduct(productOutlet.ProductId);
                        // var product = AllProducts?.FirstOrDefault(x => x.Id == productOutlet.ProductId);

                        if (product != null && product.ParentId != null && product.ParentId != 0)
                        {
                            var tempVariantProduct = productService.GetLocalProduct(product.ParentId.Value);
                            if (tempVariantProduct != null && tempVariantProduct.ProductVarients != null && tempVariantProduct.HasVarients && tempVariantProduct.ProductVarients.Count > 0 && productOutlet != null)
                            {
                                tempVariantProduct.ProductVarients.All(x =>
                                {
                                    if (x.ProductVarientId == product.Id)
                                    {
                                        if (x.VariantOutlet != null)
                                        {
                                            x.VariantOutlet.OnHandstock = productOutlet.OnHandstock;
                                            x.VariantOutlet.Committedstock = productOutlet.Committedstock;
                                        }
                                    }
                                    return true;
                                });
                                productService.UpdateLocalProduct(tempVariantProduct);
                                //tempVariantProduct.ProductOutlet.OnHandstock = tempVariantProduct.ProductOutlet.OnHandstock - Quantity;
                                WeakReferenceMessenger.Default.Send(new Messenger.ProductStockChangeMessenger(tempVariantProduct));
                            }
                        }
                        if (product != null && product.Id > 0 && product.ProductOutlet != null)
                        {
                            product.ProductOutlet.AvgCostPrice = productOutlet.AvgCostPrice;
                            product.ProductOutlet.Awaitingstock = productOutlet.Awaitingstock;
                            product.ProductOutlet.Committedstock = productOutlet.Committedstock;
                            product.ProductOutlet.CostPrice = productOutlet.CostPrice;
                            product.ProductOutlet.IsVisible = productOutlet.IsVisible;
                            product.ProductOutlet.Markup = productOutlet.Markup;
                            //product.ProductOutlet.MinimumSellingPrice = productOutlet.MinimumSellingPrice;
                            product.ProductOutlet.OnHandstock = productOutlet.OnHandstock;
                            product.ProductOutlet.OutletId = productOutlet.OutletId;
                            product.ProductOutlet.ParentProductId = productOutlet.ParentProductId;
                            product.ProductOutlet.PriceExcludingTax = productOutlet.PriceExcludingTax;
                            product.ProductOutlet.TaxID = productOutlet.TaxID;
                            product.ProductOutlet.SellingPrice = productOutlet.SellingPrice;
                            product.ProductOutlet.ReorderLevel = productOutlet.ReorderLevel;
                            product.ProductOutlet.ReorderValue = productOutlet.ReorderValue;
                            product.ProductOutlet.IsLocked = productOutlet.IsLocked;
                            var result = productService.UpdateLocalProduct(product);
                            if (result)
                            {
                                updateEntersaleProductStock(product);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Track();
            }

        }

        public void updateEntersaleProductStock(ProductDto_POS product)
        {
            try
            {
                if (AllProducts == null)
                {
                    AllProducts = new ObservableCollection<ProductDto_POS>();
                }

                var tmpselectedproduct = productService.GetLocalProduct(product.Id);
                if (tmpselectedproduct != null)
                {
                    var index = AllProducts.IndexOf(tmpselectedproduct);

                    if (index != -1)
                        AllProducts[index] = product;

                    //AllProducts.Remove(tmpselectedproduct);
                    //AllProducts.Add(product);
                }

                if (product != null)
                {
                    //tmpselectedproduct = product;
                    if (product.ProductCategories != null && product.ProductCategories.Any(x => x == SelectedCategory.Id))
                    {
                        var tmp = EnterSaleItems?.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Standard && x.Product != null && x.Product.Id == product.Id);
                        if (tmp != null)
                        {
                            tmp.Product.ProductOutlet = product.ProductOutlet;
                            tmp.Product.ProductSuppliers = product.ProductSuppliers;
                            tmp.Product.ProductAttributes = product.ProductAttributes;
                            tmp.Product.ProductVarients = product.ProductVarients;
                            SetPropertyChanged(nameof(EnterSaleItems));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_OfferReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    OfferDto offer = JsonConvert.DeserializeObject<OfferDto>(e);
                    if (offer != null)
                    {
                        var result = offerService.UpdateLocalOffer(offer);
                        if (result)
                        {
                            if (Offers == null)
                            {
                                Offers = new ObservableCollection<OfferDto>();
                            }

                            var tmpAlloffer = Offers?.FirstOrDefault(s => s.Id == offer.Id);

                            if (EnterSaleItems == null)
                            {
                                EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                            }

                            if (tmpAlloffer != null)
                            {

                                var enterOffer = EnterSaleItems.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Composite && x.Offer != null && x.Offer.Id == tmpAlloffer.Id);
                                if (enterOffer != null && EnterSaleItems.Contains(enterOffer))
                                {
                                    if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            try
                                            {
                                                EnterSaleItems.Remove(enterOffer);
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Track();
                                            }
                                        });
                                    }
                                }

                                Offers.Remove(tmpAlloffer);
                            }
                            if (offer.IsActive && (offer.IsOfferOnAllOutlet || (offer.OfferOutlets.Count(o => o.OutletId == Settings.SelectedOutletId) > 0)))
                            {
                                Offers.Add(offer);
                                if (offer.OfferType == Enums.OfferType.Composite)
                                {
                                    if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            try
                                            {
                                                EnterSaleItems.Insert(0, new EnterSaleItemDto()
                                                {
                                                    ItemType = Enums.InvoiceItemType.Composite,
                                                    Offer = offer
                                                });
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Track();
                                            }
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_OfferDeleted(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    var result = offerService.DeleteLocalOffer(e);
                    if (result)
                    {
                        if (Offers == null)
                        {
                            Offers = new ObservableCollection<OfferDto>();
                        }
                        int intparseresult;
                        if (!string.IsNullOrEmpty(e) && int.TryParse(e, out intparseresult))
                        {
                            var offer = Offers?.FirstOrDefault(x => x.Id == intparseresult);
                            if (offer != null)
                            {
                                if (EnterSaleItems == null)
                                {
                                    EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                                }

                                try
                                {
                                    var enterOffer = EnterSaleItems.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Composite && x.Offer != null && x.Offer.Id == offer.Id);
                                    if (enterOffer != null && EnterSaleItems.Contains(enterOffer))
                                    {
                                        if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                        {
                                            MainThread.BeginInvokeOnMainThread(() =>
                                            {
                                                try
                                                {
                                                    EnterSaleItems.Remove(enterOffer);
                                                }
                                                catch (Exception ex)
                                                {
                                                    ex.Track();
                                                }
                                            });
                                        }
                                    }
                                    Offers.Remove(offer);

                                }
                                catch (Exception ex)
                                {
                                    ex.Track();
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_OfferActiveDeActive(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    SignalRActivationResponse response = JsonConvert.DeserializeObject<SignalRActivationResponse>(e);
                    if (response != null && response.Id > 0)
                    {
                        //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                        var offer = offerService.GetLocalOffer(response.Id);
                        //End #84438 by Pratik
                        if (offer != null)
                        {
                            offer.IsActive = response.IsActive;
                            offerService.UpdateLocalOffer(offer);

                            if (Offers == null)
                            {
                                Offers = new ObservableCollection<OfferDto>();
                            }

                            if (EnterSaleItems == null)
                            {
                                EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                            }

                            var tmpAllOffer = Offers?.FirstOrDefault(s => s.Id == offer.Id);
                            if (tmpAllOffer != null)
                            {
                                var enterOffer = EnterSaleItems.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Composite && x.Offer != null && x.Offer.Id == offer.Id);
                                if (enterOffer != null && EnterSaleItems.Contains(enterOffer))
                                {
                                    if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            try
                                            {
                                                EnterSaleItems.Remove(enterOffer);
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Track();
                                            }
                                        });
                                    }
                                }
                                Offers.Remove(tmpAllOffer);
                            }
                            if (offer.IsActive)
                            {
                                Offers.Add(offer);
                                if (offer.OfferType == Enums.OfferType.Composite)
                                {
                                    if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            try
                                            {
                                                EnterSaleItems.Insert(0, new EnterSaleItemDto()
                                                {
                                                    ItemType = Enums.InvoiceItemType.Composite,
                                                    Offer = offer
                                                });
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Track();
                                            }
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        void PubNubService_OfferAssociateToProducts(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    ProductPubnub response = JsonConvert.DeserializeObject<ProductPubnub>(e);
                    if (response != null && response.ProductId > 0)
                    {
                        var localproduct = productService.GetLocalProduct(response.ProductId);
                        localproduct.IsInAnyOffer = response.IsAnyOffer;
                        var result = productService.UpdateLocalProduct(localproduct);
                        if (result)
                        {
                            var product = productService.GetLocalProduct(response.ProductId);
                            if (product != null)
                            {
                                product.IsInAnyOffer = response.IsAnyOffer;
                                if (product.ProductCategories != null && product.ProductCategories.Any(x => x == SelectedCategory.Id))
                                {
                                    var tmp = EnterSaleItems?.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Standard && x.Product != null && x.Product.Id == product.Id);
                                    if (tmp != null)
                                    {
                                        tmp.Product.IsInAnyOffer = product.IsInAnyOffer;
                                        SetPropertyChanged(nameof(EnterSaleItems));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        async void SignalRListener_InvoiceReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    InvoiceDto invoice = JsonConvert.DeserializeObject<InvoiceDto>(e);
                    if (invoice != null && invoice.OutletId == Settings.SelectedOutletId)
                    {
                        invoice.isSync = true;
                        var result = await saleService.UpdateLocalInvoice(invoice);
                        SetOccupiedTable();
                        if (result != null && result.Id > 0)
                        {
                            if (result != null && result.Id != 0)
                            {
                                WeakReferenceMessenger.Default.Send(new Messenger.SignalRInvoiceMessenger(result));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        void SignalRListener_ShopSettingsReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    ShopGeneralDto shopResponse = JsonConvert.DeserializeObject<ShopGeneralDto>(e);
                    if (shopResponse != null)
                    {
                        if (shopResponse.Shop != null)
                        {
                            Settings.StoreId = shopResponse.Shop.Id;
                            Settings.TenantId = shopResponse.Shop.TenantId;
                            Settings.StoreName = shopResponse.Shop.TradingName;
                            Settings.StoreShopDto = shopResponse.Shop;
                        }
                        if (shopResponse.GeneralRule != null)
                        {
                            if (ShopGeneralRule != null && Settings.StoreGeneralRule.ShowGroupProductsByCategory != shopResponse.GeneralRule.ShowGroupProductsByCategory)
                            {
                                Settings.StoreGeneralRule = shopResponse.GeneralRule;
                                ShopGeneralRule = shopResponse.GeneralRule;
                            }
                            else
                            {
                                Settings.StoreGeneralRule = shopResponse.GeneralRule;
                                ShopGeneralRule = shopResponse.GeneralRule;
                            }

                            /* if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.HideProductTypeIfNoProductInIt)
                             {
                                 Categories = getFilterCategories();

                                 if (Categories != null && Categories.Any() && SelectedCategory != null)
                                 {
                                     var category = Categories.FirstOrDefault(x => x.Id == SelectedCategory.Id);
                                     if (category == null && _navigationService.IsCurrentPage<EnterSaleViewModel>())
                                     {
                                         MainThread.BeginInvokeOnMainThread(() =>
                                         {
                                             try
                                             {
                                                 SelectedCategory = Categories.FirstOrDefault();
                                             }
                                             catch (Exception ex)
                                             {
                                                 ex.Track();
                                             }
                                         });

                                     }
                                 }
                             }
                             else
                             {
                                 Categories = new ObservableCollection<CategoryDto>(AllCategories.Where(x => x.IsActive && (x.ParentId == 0 || x.ParentId == null)).OrderBy(x => x.Name).OrderBy(x => x.sequence).OrderByDescending(a=>a.ISLayout));// Start ##90945 By Pratik
                             }
                             //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
                             if(Categories != null && !Categories.Any(a=>a.ISLayout))
                                 getCurrentLayout();*/
                            //End #90945 By Pratik
                            WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("AllowSwitchBetweenUser"));

                        }
                        if (shopResponse.ZoneAndFormat != null)
                        {
                            Settings.StoreZoneAndFormatDetail = shopResponse.ZoneAndFormat;
                            var objGetTimeZoneService = DependencyService.Get<IGetTimeZoneService>();
                            Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(shopResponse.ZoneAndFormat.IanaTimeZone); //TimeZoneInfo.Local;
                            Settings.StoreTimeZoneInfoId = shopResponse.ZoneAndFormat.IanaTimeZone;
                            Settings.StoreCurrencySymbol = shopResponse.ZoneAndFormat.CurrencySymbol;
                            Settings.StoreCurrencyCode = shopResponse.ZoneAndFormat.Currency;
                            Settings.SymbolForDecimalSeperatorForNonDot = shopResponse.ZoneAndFormat.SymbolForDecimalSeperatorForNonDot;

                            Settings.StoreCulture = shopResponse.ZoneAndFormat.Language;
                            if (!string.IsNullOrEmpty(shopResponse.ZoneAndFormat.Language) && shopResponse.ZoneAndFormat.Language.Contains("3d"))
                            {
                                shopResponse.ZoneAndFormat.Language = shopResponse.ZoneAndFormat.Language.Replace("3d", "");
                            }

                            if (string.IsNullOrEmpty(Settings.StoreCulture))
                            {
                                Settings.StoreCulture = "en";
                            }

                            if (string.IsNullOrEmpty(Settings.StoreCurrencySymbol))
                            {
                                Settings.StoreCurrencySymbol = "$";
                            }

                            if (Settings.CurrentUser.AllowOverrideLangugeSettingOverGeneralSetting)
                            {
                                Settings.StoreCulture = Settings.CurrentUser.Language;
                                Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(Settings.CurrentUser.IANATimeZone);
                                Settings.StoreTimeZoneInfoId = Settings.CurrentUser.IANATimeZone;
                                if (Settings.StoreCulture.ToLower() == "ar" || Settings.StoreCulture.ToLower() == "ar-kw")
                                {
                                    Settings.SymbolForDecimalSeperatorForNonDot = ",";
                                }

                            }

                            Extensions.SetCulture(Settings.StoreCulture.ToLower());
                        }
                        WeakReferenceMessenger.Default.Send(new Messenger.UpdateShopDataMessenger(true));
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        void SignalRListener_RegisterReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    var registerdata = JsonConvert.DeserializeObject<SignalRRegisterResponseModel>(e);
                    if (registerdata != null && registerdata.RegisterId > 0)
                    {
                        RegisterDto localRegister = outletService.GetLocalRegisterById(registerdata.RegisterId, registerdata.OutletId);
                        if (localRegister != null && localRegister.Id > 0)
                        {
                            //registerdata
                            localRegister.IsOpened = registerdata.IsOpen;
                            localRegister = registerdata.RegisterResult;
                            outletService.UpdateLocalRegister(localRegister);
                            if (Settings.CurrentRegister != null && Settings.CurrentRegister.Id == localRegister.Id)
                                IsOpenRegister = Settings.CurrentRegister.IsOpened;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_UserReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    var userdata = JsonConvert.DeserializeObject<UserListDto>(e);
                    if (userdata != null)
                    {
                        userService.UpdateLocalUser(userdata);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_PaymentTypesReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    var PaymentOptiondata = JsonConvert.DeserializeObject<PaymentOptionDto>(e);
                    if (PaymentOptiondata != null)
                    {
                        var oldPaymentOption = paymentService.GetLocalPaymentOption(PaymentOptiondata.Id);
                        if (oldPaymentOption != null && oldPaymentOption.Id == PaymentOptiondata.Id)
                        {
                            PaymentOptiondata.IsConfigered = oldPaymentOption.IsConfigered;
                            PaymentOptiondata.ConfigurationDetails = oldPaymentOption.ConfigurationDetails;
                        }
                        var result = paymentService.UpdateLocalPaymentOption(PaymentOptiondata);
                        loadPayments();
                        if (result)
                        {
                            WeakReferenceMessenger.Default.Send(new Messenger.SignalRPaymentTypesMessenger(PaymentOptiondata));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void SignalRListener_ReceiptTemplateReceived(object sender, string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    var ReceiptTemplate = JsonConvert.DeserializeObject<ReceiptTemplateDto>(e);
                    if (ReceiptTemplate != null)
                    {
                        outletService.UpdateTemplatesReceipt(ReceiptTemplate);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        async void SignalRListener_ProductWithVariantsReceived(object sender, string e)
        {
            try
            {
                Debug.WriteLine("SignalRListener_ProductWithVariantsReceived : " + e);
                if (!string.IsNullOrEmpty(e))
                {
                    var product = await productService.GetRemoteProductDetail(Priority.Background, int.Parse(e), true);

                    if (product != null && product.ProductOutlets != null && product.ProductOutlets.Any(x => x.OutletId == Settings.SelectedOutletId && x.IsVisible == true))
                    {
                        product.ProductOutlet = product.ProductOutlets?.FirstOrDefault(x => x.OutletId == Settings.SelectedOutletId);
                        var result = productService.UpdateLocalProduct(product);
                        if (result)
                        {
                            if (AllProducts == null)
                            {
                                AllProducts = new ObservableCollection<ProductDto_POS>();
                            }

                            var tmpAllProduct = AllProducts?.FirstOrDefault(s => s.Id == product.Id);
                            if (tmpAllProduct != null && AllProducts.Contains(tmpAllProduct))
                            {
                                AllProducts.Remove(tmpAllProduct);
                            }
                            if (product.IsActive)
                            {
                                AllProducts.Add(product);
                            }

                            if (EnterSaleItems == null)
                            {
                                EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                            }

                            var ItemProduct = EnterSaleItems?.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Standard && x.Product != null && x.Product.Id == product.Id);
                            if (ItemProduct == null)
                            {
                                if (product.IsActive && product.ProductCategories != null && product.ProductCategories.Count(x => x == SelectedCategory.Id) > 0)
                                {
                                    if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            try
                                            {
                                                EnterSaleItems.Add(new EnterSaleItemDto()
                                                {
                                                    ItemType = Enums.InvoiceItemType.Standard,
                                                    Product = product
                                                });
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Track();
                                            }
                                        });
                                    }
                                }
                            }
                            else if (ItemProduct != null && EnterSaleItems.Contains(ItemProduct) && (!product.IsActive || product.ProductCategories == null || product.ProductCategories.Count(x => x == SelectedCategory.Id) < 1))
                            {
                                if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        try
                                        {
                                            EnterSaleItems.Remove(ItemProduct);
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.Track();
                                        }
                                    });
                                }
                            }
                            else
                            {
                                ItemProduct.Product.ParentId = product.ParentId;
                                ItemProduct.Product.Id = product.Id;
                                ItemProduct.Product.Name = product.Name;
                                ItemProduct.Product.Description = product.Description;
                                ItemProduct.Product.Sku = product.Sku;
                                ItemProduct.Product.BarCode = product.BarCode;
                                ItemProduct.Product.Specification = product.Specification;
                                ItemProduct.Product.Depth = product.Depth;
                                ItemProduct.Product.Width = product.Width;
                                ItemProduct.Product.Height = product.Height;
                                ItemProduct.Product.Weight = product.Weight;
                                ItemProduct.Product.WeightUnit = product.WeightUnit;
                                ItemProduct.Product.ItemImage = product.ItemImage;
                                ItemProduct.Product.ColorTag = product.ColorTag;
                                ItemProduct.Product.ColorType = product.ColorType;
                                ItemProduct.Product.BrandId = product.BrandId;
                                ItemProduct.Product.BranName = product.BranName;
                                ItemProduct.Product.SeasonId = product.SeasonId;
                                ItemProduct.Product.SeasonName = product.SeasonName;
                                ItemProduct.Product.EnableAccountSyncRule = product.EnableAccountSyncRule;
                                ItemProduct.Product.SupplierCode = product.SupplierCode;
                                ItemProduct.Product.SalesCode = product.SalesCode;
                                ItemProduct.Product.PurchaseCode = product.PurchaseCode;
                                ItemProduct.Product.IsPricingDifferentByOutlet = product.IsPricingDifferentByOutlet;
                                ItemProduct.Product.ProductType = product.ProductType;
                                ItemProduct.Product.HasVarients = product.HasVarients;
                                ItemProduct.Product.TrackInventory = product.TrackInventory;
                                ItemProduct.Product.AllowOutOfStock = product.AllowOutOfStock;
                                ItemProduct.Product.Loyalty = product.Loyalty;
                                ItemProduct.Product.ServiceDuration = product.ServiceDuration;
                                ItemProduct.Product.PrinterLocationID = product.PrinterLocationID;
                                ItemProduct.Product.IsInAnyOffer = product.IsInAnyOffer;
                                ItemProduct.Product.CanEditableInThirdParty = product.CanEditableInThirdParty;
                                ItemProduct.Product.EnableSeo = product.EnableSeo;
                                ItemProduct.Product.MetaTitle = product.MetaTitle;
                                ItemProduct.Product.MetaDesc = product.MetaDesc;
                                ItemProduct.Product.MetaKeyword = product.MetaKeyword;
                                ItemProduct.Product.Slug = product.Slug;
                                ItemProduct.Product.isSelected = product.isSelected;
                                ItemProduct.Product.ProductTags = product.ProductTags;
                                ItemProduct.Product.ProductCategories = product.ProductCategories;
                                ItemProduct.Product.SalesChannels = product.SalesChannels;
                                ItemProduct.Product.ProductOutlet = product.ProductOutlet;
                                ItemProduct.Product.ProductSuppliers = product.ProductSuppliers;
                                ItemProduct.Product.ProductAttributes = product.ProductAttributes;
                                ItemProduct.Product.ProductVarients = product.ProductVarients;
                                ItemProduct.Product.ServiceUsers = product.ServiceUsers;
                                ItemProduct.Product.ProductImages = product.ProductImages;
                                ItemProduct.Product.ProductExtras = product.ProductExtras;
                                ItemProduct.Product.IsActive = product.IsActive;

                            }

                            if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    try
                                    {
                                        var catitem = EnterSaleItems?.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Category && x.Category != null && product != null && product.ProductCategories != null && product.ProductCategories.Count(p => p == x.Category.Id) > 0);
                                        if (catitem != null)
                                        {
                                            catitem.Category.ProductsCount = AllProducts.Count(x => x.IsActive && (x.ParentId == 0 || x.ParentId == null) && x.ProductCategories.Count(c => c == catitem.Category.Id && catitem.Category.Id != 0) > 0);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.Track();
                                    }
                                });
                            }

                        }
                    }
                    else if (product != null && product.ProductOutlets != null)
                    {
                        var tmpAllProduct = AllProducts?.FirstOrDefault(s => s.Id == product.Id);
                        if (tmpAllProduct != null && AllProducts.Contains(tmpAllProduct))
                        {
                            AllProducts.Remove(tmpAllProduct);
                        }

                        if (EnterSaleItems == null)
                        {
                            EnterSaleItems = new ObservableCollection<EnterSaleItemDto>();
                        }

                        var ItemProduct = EnterSaleItems?.FirstOrDefault(x => x.ItemType == Enums.InvoiceItemType.Standard && x.Product != null && x.Product.Id == product.Id);
                        if (ItemProduct != null && EnterSaleItems.Contains(ItemProduct) && (product.ProductCategories == null || product.ProductCategories.Count(x => x == SelectedCategory.Id) > 0))
                        {
                            if (_navigationService.IsCurrentPage<EnterSaleViewModel>())
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    try
                                    {
                                        EnterSaleItems.Remove(ItemProduct);
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.Track();
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //Ticket start:#29907 Notification of register opens for log.by rupesh
        void ShowAlertToCloseOpenedCashRegister()
        {
            var register = Settings.CurrentRegister;
            if (register.Registerclosure?.StartDateTime != null)
            {
                var days = Convert.ToInt32((DateTime.Now.ToStoreTime() - register.Registerclosure.StartDateTime.Value.ToStoreTime()).TotalDays);
                if (days >= 1 && EnterSalePage.DataUpdated)
                {

                    Application.Current.Dispatcher.Dispatch(async () =>
                    {
                        var storeOpenTime = string.Format("opened on {0:dddd, dd MMMM yyyy} at {0:hh.mmtt}", register.Registerclosure.StartDateTime.Value.ToStoreTime());
                        var result = await App.Alert.ShowAlert(String.Format(LanguageExtension.Localize("CloseRegisterAlertTitle"), register.Name, days), String.Format(LanguageExtension.Localize("CloseRegisterAlertMessage"), register.Name, storeOpenTime), LanguageExtension.Localize("GoToCashRegister"), LanguageExtension.Localize("IgnoreText"));
                        if (result)
                        {
                            //var mainpage = _navigationService.MainPage;
                            //mainpage.ChangePage("CashRegisterPage");
                            Shell.Current.CurrentItem = Shell.Current.Items[2];
                        }
                    });
                }
            }
        }
        //Ticket end:#29907 .by rupesh

        //Start #90946 iOS:FR  Item Serial Number Tracking by Pratik
        public async Task CheckSerialNumber(InvoiceLineItemDto invoiceLineItemDto)
        {
            try
            {
                using (new Busy(this, true))
                {
                    var result = await saleService.GetPOSSerialNumber(Priority.UserInitiated, new POSSerialNumberRequest() { outletId = Settings.SelectedOutletId, productId = invoiceLineItemDto.InvoiceItemValue, serialNumber = invoiceLineItemDto.SerialNumber.Trim() });
                    if (result == null)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SerialNumberIsNotValid"), Colors.Red, Colors.White);
                        invoiceLineItemDto.SerialNumber = string.Empty;
                    }
                    else if (result != null && result.soldDate.HasValue)
                    {
                        invoiceLineItemDto.SerialNumber = string.Empty;
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SerialNumberSold"), Colors.Red, Colors.White);
                    }
                }
            }
            catch (Exception ex)
            {
                invoiceLineItemDto.SerialNumber = string.Empty;
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                ex.Track();
            }
        }
        //End #90946 by Pratik
        public async void GoToCart_Click()
        {
            //IsPhoneGridVisible = false;
            // if (CheckOutPage == null)
            // {
            //     CheckOutPage = new CheckOutPage();

            //     CheckOutPage.ViewModel.invoicemodel = invoicemodel;
            //     CheckOutPage.ViewModel.productService = productService;
            //     CheckOutPage.ViewModel.offerService = offerService;
            //     CheckOutPage.ViewModel.paymentService = paymentService;
            //     CheckOutPage.ViewModel.outletService = outletService;
            //     CheckOutPage.ViewModel.customerService = customerService;
            //     CheckOutPage.ViewModel.saleService = saleService;
            //     CheckOutPage.ViewModel.userService = userService;
            //     CheckOutPage.ViewModel.taxServices = taxServices;
            //     CheckOutPage.ViewModel.RestaurantService = RestaurantService;
            //     CheckOutPage.ViewModel.EnterSaleItems = EnterSaleItems;
            //     CheckOutPage.ViewModel.Categories = Categories;
            //     CheckOutPage.ViewModel.SelectedCategory = SelectedCategory;
            //     CheckOutPage.ViewModel.AllProducts = AllProducts;
            //     CheckOutPage.ViewModel.AllUnitOfMeasures = AllUnitOfMeasures;
            //     CheckOutPage.ViewModel.Offers = Offers;
            //     CheckOutPage.ViewModel.AllPaymentOptionList = AllPaymentOptionList;
            //     CheckOutPage.ViewModel.UpdateCheckOutPage();
            // }
            //await NavigationService.PushAsync(CheckOutPage);
            var parameters = new Dictionary<string, object>
            {
                { "InvoiceModel", invoicemodel },
                { "ProductService", productService },
                { "OfferService", offerService },
                { "PaymentService", paymentService },
                { "OutletService", outletService },
                { "CustomerService", customerService },
                { "SaleService", saleService },
                { "UserService", userService },
                { "TaxServices", taxServices },
                { "RestaurantService", RestaurantService },
                { "EnterSaleItems", EnterSaleItems },
                { "Categories", Categories },
                { "SelectedCategory", SelectedCategory },
                { "AllProducts", AllProducts },
                { "AllUnitOfMeasures", AllUnitOfMeasures },
                { "Offers", Offers },
                { "AllPaymentOptionList", AllPaymentOptionList }
            };

            await Shell.Current.GoToAsync("CheckOutPage", parameters);

        }

        //#94565
        private async void Ordered_Click()
        {
            if (IsPaymentClicked)
                return;
            IsPaymentClicked = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? 2000 : 1000).Wait();
                IsPaymentClicked = false;
            });

            using (new Busy(this, true))
            {
                try
                {
                    if (invoicemodel.Invoice != null && (invoicemodel.Invoice.Status != InvoiceStatus.Parked || invoicemodel.Invoice.Status != InvoiceStatus.OnAccount
                    || invoicemodel.Invoice.Status != InvoiceStatus.Completed || invoicemodel.Invoice.Status != InvoiceStatus.LayBy))
                    {
                        if (invoicemodel.Invoice?.InvoiceLineItems != null && invoicemodel.Invoice.InvoiceLineItems.Count > 0 &&
                            string.IsNullOrEmpty(invoicemodel.Invoice.FloorTableName) && invoicemodel.Invoice.InvoiceFloorTable == null)
                        {
                            Pages.OrderNamePopupPage orderNamePopupPage = new Pages.OrderNamePopupPage();
                            orderNamePopupPage.AddOrderName += AddOrderName;
                            await NavigationService.PushModalAsync(orderNamePopupPage);
                            return;
                        }
                        else
                        {
                            if (invoicemodel.Invoice?.InvoiceLineItems != null && invoicemodel.Invoice.InvoiceLineItems.Count > 0)
                            {
                                if (invoicemodel.Table != null && invoicemodel.Invoice.InvoiceFloorTables?.FirstOrDefault() is not { Id: > 0 })
                                {
                                    var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                                    invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                                    invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                                    {
                                        TableId = invoicemodel.Table?.TableId ?? 0,
                                        AssignedDateTime = DateTime.UtcNow,
                                        FloorId = floor == null ? 0 : floor.Id,
                                        FloorName = floor?.Name,
                                        TableName = invoicemodel.Table?.Name
                                    });
                                    invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;
                                }
                                invoicemodel.Invoice.Status = InvoiceStatus.OnGoing;
                                await InvoiceCalculations.FinaliseOrder(invoicemodel.Invoice, invoicemodel.offers, saleService, outletService, productService);
                                invoicemodel.Invoice = null;
                                invoicemodel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                                invoicemodel.Table = null;
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ProcessedOrder"), Colors.Green, Colors.White, TimeSpan.FromSeconds(2));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
        }

        private async void AddOrderName(object sender, string e)
        {
            try
            {
                if (invoicemodel.Invoice?.InvoiceLineItems != null && invoicemodel.Invoice.InvoiceLineItems.Count > 0)
                {
                    invoicemodel.Invoice.FloorTableName = e;
                    if (invoicemodel.Table != null && invoicemodel.Invoice.InvoiceFloorTables?.FirstOrDefault() is not { Id: > 0 })
                    {
                        var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                        invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                        invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                        {
                            TableId = invoicemodel.Table?.TableId ?? 0,
                            AssignedDateTime = DateTime.UtcNow,
                            FloorId = floor == null ? 0 : floor.Id,
                            FloorName = floor?.Name,
                            TableName = invoicemodel.Table?.Name
                        });
                        invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;
                    }
                    else
                    {
                        invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                        invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                        {
                            AssignedDateTime = DateTime.UtcNow,
                            FloorName = string.Empty,
                            TableName = e
                        });
                    }
                    invoicemodel.Invoice.Status = InvoiceStatus.OnGoing;
                    await InvoiceCalculations.FinaliseOrder(invoicemodel.Invoice, invoicemodel.offers, saleService, outletService, productService);
                    invoicemodel.Invoice = null;
                    invoicemodel.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                    invoicemodel.Table = null;
                    App.Instance.Hud.DisplayToast("Order Processing...", Colors.Green, Colors.White);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //#94565

        private void BackToPhoneGrid_Click()
        {
            IsPhoneGridVisible = true;
        }
        public async void CancelNadaPayTerminalAnyPendingSale()
        {

            if (Settings.HikePayVerify != null && Settings.HikePayVerify.InvoiceSyncReference == invoicemodel.Invoice?.InvoiceTempId)
            {
                using (new Busy(this, false))
                {
                    try
                    {
                        var paymentOption = paymentService.GetLocalPaymentOption(Settings.HikePayVerify.PaymentId);
                        if (paymentOption != null)
                        {
                            var config = JsonConvert.DeserializeObject<NadaPayConfigurationDto>(paymentOption.ConfigurationDetails);
                            var lastReference = Settings.HikePayVerify.SaleReference;
                            var amount = (invoicemodel.Invoice.TenderAmount + (paymentOption.DisplaySurcharge ?? 0));
                            HikePayTerminalResponse saleResult = null;
                            INadaPayTerminalAppService hikeTerminalPay = DependencyService.Get<INadaPayTerminalLocalAppService>();
                            if (amount > 0)
                            {
                                saleResult = await hikeTerminalPay.CreateNadaPayTerminalCancel(amount, invoicemodel.Invoice.Currency, lastReference, paymentOption, Settings.HikePayVerify.ServiceId);

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.SyncLogger("----HikeTerminalpay Status---\n" + "Failed" + "\n---- HikeTerminalpay response---\n" + ex.Message);

                    }

                }
            }
        }
    

    #endregion

}

}
