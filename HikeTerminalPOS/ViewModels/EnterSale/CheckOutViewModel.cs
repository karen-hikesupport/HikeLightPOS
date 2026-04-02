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
    public class CheckOutViewModel : BaseViewModel
    {

       // public  void ApplyQueryAttributes(IDictionary<string, object> query)
       // {
            //invoicemodel = new InvoiceViewModel(null, null, null, null);
            // if (query.ContainsKey("ProductService"))
            //     productService = query["ProductService"] as ProductServices;
            // if (query.ContainsKey("OfferService"))
            //     offerService = query["OfferService"] as OfferServices;
            // if (query.ContainsKey("PaymentService"))
            //     paymentService = query["PaymentService"] as PaymentServices;
            // if (query.ContainsKey("OutletService"))
            //     outletService = query["OutletService"] as OutletServices;
            // if (query.ContainsKey("CustomerService"))
            //     customerService = query["CustomerService"] as CustomerServices;
            // if (query.ContainsKey("SaleService"))
            //     saleService = query["SaleService"] as SaleServices;
            // if (query.ContainsKey("UserService"))
            //     userService = query["UserService"] as UserServices;
            // if (query.ContainsKey("TaxServices"))
            //     taxServices = query["TaxServices"] as TaxServices;
            // if (query.ContainsKey("RestaurantService"))
            //     RestaurantService = query["RestaurantService"] as RestaurantService;
            // if (query.ContainsKey("EnterSaleItems"))
            //     EnterSaleItems = query["EnterSaleItems"] as ObservableCollection<EnterSaleItemDto>;

            // if (query.ContainsKey("Categories"))
            //     Categories = query["Categories"] as ObservableCollection<CategoryDto>;

            // if (query.ContainsKey("SelectedCategory"))
            //     SelectedCategory = query["SelectedCategory"] as CategoryDto;

            // if (query.ContainsKey("AllProducts"))
            //     AllProducts = query["AllProducts"] as ObservableCollection<ProductDto_POS>;

            // if (query.ContainsKey("AllUnitOfMeasures"))
            //     AllUnitOfMeasures = query["AllUnitOfMeasures"] as ObservableCollection<ProductUnitOfMeasureDto>;

            // if (query.ContainsKey("Offers"))
            //     Offers = query["Offers"] as ObservableCollection<OfferDto>;

            // if (query.ContainsKey("AllPaymentOptionList"))
            //     AllPaymentOptionList = query["AllPaymentOptionList"] as IEnumerable<PaymentOptionDto>;
            // await Task.Delay(500);
            // if (query.ContainsKey("InvoiceModel"))
            //     invoicemodel = query["InvoiceModel"] as InvoiceViewModel;

            // UpdateCheckOutPage();
       // }
        public static bool IsSaleSucceess;


        public PaymentPage paymentpage;
        public PaymentPagePhone paymentpagePhone;
        public EnterSalePage EnterSalePage;
        public CheckOutPage EnterSalePagePhone;

       // ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
        public ProductServices productService; //= new ProductServices(productApiService);

        //ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        public CustomerServices customerService;

        //ApiService<IOfferApi> offerApiService = new ApiService<IOfferApi>();
        public OfferServices offerService;

        //ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        public PaymentServices paymentService;

        //ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        public SaleServices saleService;

        //ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
        public OutletServices outletService;

        //ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
        public UserServices userService;

        //ApiService<ITaxApi> taxApiService = new ApiService<ITaxApi>();
        public TaxServices taxServices;

        //#94565
        //ApiService<IRestaurantApi> RestaurantApiService = new ApiService<IRestaurantApi>();
        public RestaurantService RestaurantService;
        //#94565

        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        bool waiting = false;

        #region Public Properties

        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        bool _isServedBy;
        public bool IsServedBy { get { return _isServedBy; } set { _isServedBy = value; SetPropertyChanged(nameof(IsServedBy)); } }
        //end #84287 .by Pratik


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
        public CategoryDto SelectedCategory;

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
        public CheckOutViewModel()
        {
            PaymentCommand = new Command(Payment);
            SearchProductCommand = new Command(SearchProduct);
            PaymentSummaryCommand = new Command(PaymentSummaryClick);
            NotesCommand = new Command(NotesClick);
            ShowOpenRegisterCommand = new Command(ShowOpenRegister);

            //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
            IsServedBy = Settings.StoreGeneralRule.ServedByLineItem;
            //end #84287 .by Pratik
           

            
            //Ticket #7760 Start:Rounding issue in Arabic language.By Nikhil.
            CountrySpecificCode();
            //Ticket #7760 End:By Nikhil. 

            //Tmp code to view print preview if there is no printer.,
            //TempPrinterCode(); 
        }
        public void UpdateCheckOutPage()
        {
            // invoicemodel = new InvoiceViewModel(productService, customerService, outletService, saleService);
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
        }
   

        #endregion

        #region Command
        public ICommand PaymentCommand { get; }
        public ICommand PaymentSummaryCommand { get; }
        public ICommand NotesCommand { get; }
        public ICommand SearchProductCommand { get; }
        public ICommand ShowOpenRegisterCommand { get; }
        public ICommand MoreOptionCommand => new Command(MoreOptionTapped);
        public ICommand BackgroundHandleCommand => new Command(BackgroundHandleTapped);
        public ICommand RestockHandleCommand => new Command(RestockHandleTapped);
        public ICommand OpenDiscountCommand => new Command(OpenDiscountTapped);
        public ICommand OpenTipCommand => new Command(OpenTipTapped);
        public ICommand OpenTaxCommand => new Command(OpenTaxTapped);
        public ICommand AddShippingChargeCommand => new Command(AddShippingChargeTapped);

       
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

        public ICommand OrderedCommand => new Command(Ordered_Click);  //#94565   

        public ICommand BackToEnterSaleCommand => new Command(BackToEnterSale_Click);

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
        private void MoreOptionTapped()
        {
            invoicemodel.OpenOption();
        }
       
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
                       
                        //#94565
                        if (FloorList == null || (FloorList != null && FloorList.Count <= 0))
                        {
                            // var floors = RestaurantService.GetLocalFloors(Settings.SelectedOutletId);
                            // FloorList = new ObservableCollection<FloorDto>(floors);
                            // if (floors != null)
                            //     FloorList = new ObservableCollection<FloorDto>(floors);
                            // else
                            //     FloorList = new ObservableCollection<FloorDto>();
                        }
                        //#94565

                        SetExpandHeight();
                        IsPaymentClicked = false;

                        //#32357 iPad :: Feature request :: Show Item Count on POS Screen
                        IsItemCountVisible = Settings.StoreGeneralRule.ShowTotalQuantityOfItemsInBasket;
                        //#32357 iPad :: Feature request :: Show Item Count on POS Screen


                    });

                    if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.UpdateShopDataMessenger>(this))
                    {
                        WeakReferenceMessenger.Default.Register<Messenger.UpdateShopDataMessenger>(this, (sender, arg) =>
                        {
                            MainThread.BeginInvokeOnMainThread(UpdateViews);
                        });
                    }


                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            });
            
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

      

        public async void ReopenSaleFromHistory(InvoiceDto invoice = null)
        {
            invoicemodel.IsLoad = true;
            using (new Busy(this, true))
            {
                await invoicemodel.ReopenSaleFromHistory(invoice);
                invoicemodel.IsLoad = false;
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
                                // if (invoicemodel.Table != null && invoicemodel.Invoice.InvoiceFloorTables?.FirstOrDefault() is not { Id: > 0 })
                                // {
                                //     var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                                //     invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                                //     invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                                //     {
                                //         TableId = invoicemodel.Table?.TableId ?? 0,
                                //         AssignedDateTime = DateTime.UtcNow,
                                //         FloorId = floor == null ? 0 : floor.Id,
                                //         FloorName = floor?.Name,
                                //         TableName = invoicemodel.Table?.Name
                                //     });
                                //     invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;
                                // }

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
                                    // var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                                    // invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                                    // invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                                    // {
                                    //     TableId = invoicemodel.Table?.TableId ?? 0,
                                    //     AssignedDateTime = DateTime.UtcNow,
                                    //     FloorId = floor == null ? 0 : floor.Id,
                                    //     FloorName = floor?.Name,
                                    //     TableName = invoicemodel.Table?.Name
                                    // });
                                    // invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;

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
                                    // var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                                    // invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                                    // invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                                    // {
                                    //     TableId = invoicemodel.Table?.TableId ?? 0,
                                    //     AssignedDateTime = DateTime.UtcNow,
                                    //     FloorId = floor == null ? 0 : floor.Id,
                                    //     FloorName = floor?.Name,
                                    //     TableName = invoicemodel.Table?.Name
                                    // });
                                    // invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;
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
                        // var floor = getFlooreId(invoicemodel.Table?.TableId ?? 0);
                        // invoicemodel.Invoice.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>();
                        // invoicemodel.Invoice.InvoiceFloorTables.Add(new InvoiceFloorTableDto()
                        // {
                        //     TableId = invoicemodel.Table?.TableId ?? 0,
                        //     AssignedDateTime = DateTime.UtcNow,
                        //     FloorId = floor == null ? 0 : floor.Id,
                        //     FloorName = floor?.Name,
                        //     TableName = invoicemodel.Table?.Name
                        // });
                        // invoicemodel.Invoice.FloorTableName = invoicemodel.Invoice.InvoiceFloorTables[0].FloorName + " - " + invoicemodel.Invoice.InvoiceFloorTables[0].TableName;
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

        private void BackToEnterSale_Click()
        {
            if(invoicemodel?.Invoice?.Status != InvoiceStatus.Refunded)
              NavigationService.PopAsync(true);
        }
        
    
}

}
