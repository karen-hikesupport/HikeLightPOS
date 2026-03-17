using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
    public class ParkSaleViewModel : BaseViewModel
    {
        public int SkipCount = 0;
        public int TotalCount = 0;
        public int TotalStep = 1;
        public static int MaxHistoryResult = 30;

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        SaleServices saleService;

        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        IMPOPStarBarcode mPOPStarBarcode = null;
        //IScanApiService scanApiService;

        ApiService<IInventoriesApi> inventoryApiService = new ApiService<IInventoriesApi>();
        InventoriesServices inventoryService;

        ObservableRangeCollection<InvoiceDto> _ParkSales { get; set; }
        public ObservableRangeCollection<InvoiceDto> ParkSales { get { return _ParkSales; } set { _ParkSales = value; SetPropertyChanged(nameof(ParkSales)); } }

        ObservableCollection<InvoiceDto> AllLocalSaleItems { get; set; }

        ObservableCollection<InvoiceDto> _ParkFilterSaleItems { get; set; }
        public ObservableCollection<InvoiceDto> ParkFilterSaleItems
        {
            get { return _ParkFilterSaleItems; }
            set
            {
                _ParkFilterSaleItems = value;
                if (value != null)
                {
                    TotalSaleHeader = string.Format("{0} Orders", ParkFilterSaleItems.Count());
                }
                else
                {
                    TotalSaleHeader = string.Format("{0} Orders", "No");
                }
            }
        }

        string _TotalSaleHeader { get; set; } = string.Format("{0} Orders", "No");
        public string TotalSaleHeader { get { return _TotalSaleHeader; } set { _TotalSaleHeader = value; SetPropertyChanged(nameof(TotalSaleHeader)); } }

        string _SelectedMenu { get; set; } = "All";
        public string SelectedMenu { get { return _SelectedMenu; } set { _SelectedMenu = value; SetPropertyChanged(nameof(SelectedMenu)); } }

        public string SearchText { get; set; } = "";

        int _IsOpenFilterOptionPopUp { get; set; } = 0;
        public int IsOpenFilterOptionPopUp { get { return _IsOpenFilterOptionPopUp; } set { _IsOpenFilterOptionPopUp = value; SetPropertyChanged(nameof(IsOpenFilterOptionPopUp)); } }




        string _SelectedDateRange { get; set; } = LanguageExtension.Localize("Today");
        public string SelectedDateRange { get { return _SelectedDateRange; } set { _SelectedDateRange = value; SetPropertyChanged(nameof(SelectedDateRange)); } }


        DateTime _SelectedStartingDate { get; set; } = DateTime.Now.Date;
        public DateTime SelectedStartingDate
        {
            get { return _SelectedStartingDate; }
            set
            {
                _SelectedStartingDate = value;
                SetPropertyChanged(nameof(SelectedStartingDate));
                SetPropertyChanged(nameof(SelectedDateRangeText));
                SetPropertyChanged(nameof(SelectedDateRange));
            }
        }

        DateTime _SelectedEndingDate { get; set; } = DateTime.Now.Date;
        public DateTime SelectedEndingDate
        {
            get
            {
                return _SelectedEndingDate;
            }
            set
            {
                _SelectedEndingDate = value;
                SetPropertyChanged(nameof(SelectedEndingDate));
                SetPropertyChanged(nameof(SelectedDateRangeText));
            }
        }



        DateTime _CustomStartingDate { get; set; } = DateTime.Now.Date;
        public DateTime CustomStartingDate { get { return _CustomStartingDate; } set { _CustomStartingDate = value; SetPropertyChanged(nameof(CustomStartingDate)); } }

        DateTime _CustomEndingDate { get; set; } = DateTime.Now.Date;
        public DateTime CustomEndingDate { get { return _CustomEndingDate; } set { _CustomEndingDate = value; SetPropertyChanged(nameof(CustomEndingDate)); } }

        public string SelectedDateRangeText
        {
            get
            {
                return SelectedStartingDate.ToString("d") + " - " + SelectedEndingDate.ToString("d");
            }
        }

        int _IsOpenFilterDateRangePopUp { get; set; } = 0;
        public int IsOpenFilterDateRangePopUp { get { return _IsOpenFilterDateRangePopUp; } set { _IsOpenFilterDateRangePopUp = value; SetPropertyChanged(nameof(IsOpenFilterDateRangePopUp)); } }


        bool _IsOpenSearchPopUp;
        public bool IsOpenSearchPopUp { get { return _IsOpenSearchPopUp; } set { _IsOpenSearchPopUp = value; SetPropertyChanged(nameof(IsOpenSearchPopUp)); } }

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        bool _isOverdue { get; set; } = true;
        public bool IsOverdue { get { return _isOverdue; } set { _isOverdue = value; SetPropertyChanged(nameof(IsOverdue)); } }

        bool _isOverdueDisplay { get; set; } = false;
        public bool IsOverdueDisplay { get { return _isOverdueDisplay; } set { _isOverdueDisplay = value; SetPropertyChanged(nameof(IsOverdueDisplay)); } }
        //End ticket #76208 by Pratik

        //Ticket start:#30840 Hide Discarded Sales/Void in Sales History.by rupesh
        bool _IsOpenIncludeDiscardPopUp;
        public bool IsOpenIncludeDiscardPopUp { get { return _IsOpenIncludeDiscardPopUp; } set { _IsOpenIncludeDiscardPopUp = value; SetPropertyChanged(nameof(IsOpenIncludeDiscardPopUp)); } }

        bool _IsIncludeDiscard { get; set; } = true;
        public bool IsIncludeDiscard { get { return _IsIncludeDiscard; } set { _IsIncludeDiscard = value; SetPropertyChanged(nameof(IsIncludeDiscard)); } }

        bool _isRefresh;
        public bool IsRefresh { get { return _isRefresh; } set { _isRefresh = value; SetPropertyChanged(nameof(IsRefresh)); } }
        //Ticket end:#30840 .by rupesh


        public static ParkSaleDetailPage detailpage;
        public CreatePOSlider createPOpage;

        //public ICommand OpenInvoiceDetailViewCommand { get; }
        public ICommand GetPOCommand { get; }

        public ICommand RefreshCommand { get; }

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

        //Filter outlet by rupesh
        int _IsOpenFilterOutletPopUp { get; set; } = 0;
        public int IsOpenFilterOutletPopUp { get { return _IsOpenFilterOutletPopUp; } set { _IsOpenFilterOutletPopUp = value; SetPropertyChanged(nameof(IsOpenFilterOutletPopUp)); } }

        //Show filter for selection outlet .By rupesh.
        bool canRefundOnDifferentOutlet { get; set; }
        public bool CanRefundOnDifferentOutlet
        {
            get
            {
                return canRefundOnDifferentOutlet;
            }
            set
            {
                canRefundOnDifferentOutlet = value;
                SetPropertyChanged(nameof(CanRefundOnDifferentOutlet));
            }
        }

        //Show filter for selection outlet .By rupesh.
        string selectedOutletForFilter { get; set; }
        public string SelectedOutletForFilter
        {
            get
            {
                return selectedOutletForFilter;
            }
            set
            {
                selectedOutletForFilter = value;
                SetPropertyChanged(nameof(SelectedOutletForFilter));
                IsOutlateNameDisplay = SelectedOutletForFilter == "All";
            }
        }

        bool _isOutlateNameDisplay;
        public bool IsOutlateNameDisplay
        {
            get
            {
                return _isOutlateNameDisplay;
            }
            set
            {
                _isOutlateNameDisplay = value;
                SetPropertyChanged(nameof(IsOutlateNameDisplay));
            }
        }

        //Ticket start:#22406 Quote sale.by rupesh
        bool _isQuoteEnabled { get; set; } = Settings.StoreGeneralRule.EnableQuoteSale;
        public bool IsQuoteEnabled
        {
            get
            {
                return _isQuoteEnabled;
            }
            set
            {
                _isQuoteEnabled = value;
                SetPropertyChanged(nameof(IsQuoteEnabled));
            }
        }
        //Ticket end:#22406 .by rupesh

        public bool IsOnGoing => Settings.IsRestaurantPOS; //#94565

        //Added by rupesh to filter out sales history as per outlet
        int SelectedOutletId = Settings.SelectedOutletId;
        //Ticket no #10114 by rupesh
        bool FirstTimeLoad;

        public ICommand ParkedListViewSelectionCommand { get; }
        //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
        public ICommand OpenUpdateCustomerNameViewCommand { get; }
        public ICommand UpdateCustomerNameCommand { get; }
        public ICommand CloseUpdateCustomerNameViewCommand { get; }
        public bool IsEditWalkInCustomerName { get; set; } = true;
        public InvoiceDto SelectedInvoiceToUpdateCustomer = null;
        //Ticket end:#45648 .by rupesh

        public ParkSaleViewModel()
        {
            App.Instance.Hud.DisplayProgress(LanguageExtension.Localize("Progress0_Text"));
            saleService = new SaleServices(saleApiService);
            ParkSales = new ObservableRangeCollection<InvoiceDto>();
            inventoryService = new InventoriesServices(inventoryApiService);
            GetPOCommand = new Command<InvoiceDto>(GetPO);
            RefreshCommand = new Command(RefreshSaleHistory);
            OpenUpdateCustomerNameViewCommand = new Command<InvoiceDto>(OpenUpdateCustomerName);
            ParkedListViewSelectionCommand = new Command<InvoiceDto>(ParkedListViewSelection);
            UpdateCustomerNameCommand = new Command<InvoiceDto>(UpdateCustomerName);
            CloseUpdateCustomerNameViewCommand = new Command<InvoiceDto>(CloseUpdateCustomerName);
            //LoadMoreSaleCommand = new Command(LoadMoreSale);
            //SelectMenuCommand = new Command<string>(SelectFilterMenu);


            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.BackgroundInvoiceUpdatedMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.BackgroundInvoiceUpdatedMessenger>(this, ReceivedUpdatedInvoiceHandleAction);
            }
            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.SignalRInvoiceMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.SignalRInvoiceMessenger>(this, ReceivedSignalRInvoiceHandleAction);
            }

            //Ticket #7760 Start:Rounding issue in Arabic language.By Nikhil.
            CountrySpecificCode();

        }

        private async void RefreshSaleHistory()
        {
            IsRefresh = true;
            await FilterWithoutLoader(SearchText, false);
            IsRefresh = false;
        }

        void CountrySpecificCode()
        {
            var CurrentCultureName = DependencyService.Get<IMultilingual>().CurrentCultureInfo.Name;
            IsArabicCulture = (CurrentCultureName == "ar-SY");
        }
        //Ticket #7760 End:By Nikhil.

        public void OnAppearingCall()
        {
               _= LoadData(SearchText);

                /* if (scanApiService == null)
                     scanApiService = DependencyService.Get<IScanApiService>();

                 scanApiService.StartService();*/
            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.BarcodeMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.BarcodeMessenger>(this,async (sender, arg) =>
                {
                    //Ticket start:#58490 iPad: FR For Pick and pack option.by rupesh
                    var lastModalPage = _navigationService.CurrentPage.Navigation.ModalStack;
                    if (!(_navigationService.NavigatedPage is BaseContentPage<ParkSaleViewModel>) || (lastModalPage != null && lastModalPage.Count > 0))//#97721//Ticket:#94698 Temp workaround.by rupesh 
                    {
                        return;
                    }
                    //Ticket start:#58490 .by rupesh
                    SearchSalesByBarcodeAsync(arg.Value, mPOPStarBarcode);
                });
            }

            //Show filter for selection outlet .By rupesh.
            CanRefundOnDifferentOutlet = Settings.StoreGeneralRule.CanRefundOnDifferentOutlet;
                SelectedOutletId = Settings.SelectedOutletId;
                SelectedOutletForFilter = Settings.SelectedOutletName;

        }

        public void OnDisappearingCall()
        {
            //START ticket #76208 IOS:FR:Terms of payments by Pratik
            closeIncludeDiscardedSaleViewHandle_Clicked();
            IsOverdueDisplay = false;
            IsOverdue = false;
            //End ticket #76208 by Pratik

            //scanApiService.CloseService();
            WeakReferenceMessenger.Default.Unregister<Messenger.BarcodeMessenger>(this); 
        }


        public async Task LoadData(string filter_keyword = null)
        {
            await Task.Run(() =>
            {

                using (new Busy(this, false))
                {
                    AllLocalSaleItems = saleService.GetLocalInvoices();
                    if (AllLocalSaleItems != null)
                        ParkFilterSaleItems = new ObservableCollection<InvoiceDto>
                            (AllLocalSaleItems.Where(x => x.TransactionDate.Date >= SelectedStartingDate
                            && x.TransactionDate.Date <= SelectedEndingDate));
                }
                SelectedMenu = "All";
                IsQuoteEnabled = Settings.StoreGeneralRule.EnableQuoteSale;
                _= FilterWithoutLoader(filter_keyword, false);
            });
            App.Instance.Hud.Dismiss();
        }

        public async Task SelectFilterDateRange(string selectedDateRange)
        {
            try
            {
                if (SelectedDateRange != selectedDateRange || SelectedDateRange == "CustomRange")
                {

                    if (selectedDateRange == LanguageExtension.Localize("Today"))
                    {
                        SelectedStartingDate = DateTime.Now.Date;
                        SelectedEndingDate = SelectedStartingDate;
                    }
                    else if (selectedDateRange == LanguageExtension.Localize("Yesterday"))
                    {
                        SelectedStartingDate = DateTime.Now.AddDays(-1).Date;
                        SelectedEndingDate = SelectedStartingDate;
                    }
                    else if (selectedDateRange == LanguageExtension.Localize("Last7Days"))
                    {
                        SelectedStartingDate = DateTime.Now.AddDays(-6).Date;
                        SelectedEndingDate = DateTime.Now.Date;
                    }
                    else if (selectedDateRange == LanguageExtension.Localize("Last30Days"))
                    {
                        SelectedStartingDate = DateTime.Now.AddDays(-29).Date;
                        SelectedEndingDate = DateTime.Now.Date;
                    }
                    else if (selectedDateRange == LanguageExtension.Localize("ThisMonth"))
                    {
                        SelectedStartingDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
                        SelectedEndingDate = DateTime.Now.Date;
                    }
                    else if (selectedDateRange == LanguageExtension.Localize("LastMonth"))
                    {
                        var date = DateTime.Now.AddMonths(-1);
                        SelectedStartingDate = new DateTime(date.Year, date.Month, 1).Date;
                        SelectedEndingDate = SelectedStartingDate.AddMonths(1).AddDays(-1).Date;
                    }
                    else if (selectedDateRange == "CustomRange")
                    {
                        SelectedStartingDate = CustomStartingDate;
                        SelectedEndingDate = CustomEndingDate;
                    }
                    await Filter(SearchText);
                    SelectedDateRange = selectedDateRange;
                    SetPropertyChanged(nameof(SelectedDateRangeText));
                }
                IsOpenFilterDateRangePopUp = 0;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task SelectFilterMenu(string selectedMenu)
        {
            try
            {
                SelectedMenu = selectedMenu;
                //Start #77147 iPad: Filter fetching all data by pratik
                await Filter(SearchText);
                //End #77147 by pratik
                IsOpenFilterOptionPopUp = 0;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task Filter(string keyword = null, bool isFromLoadMore = false)
        {
            using (new Busy(this, true))
            {
                await FilterWithoutLoader(keyword,isFromLoadMore);
            }
        }

        public async Task FilterWithoutLoader(string keyword = null, bool isFromLoadMore = false)
        {
            try
            {
                var CurrentCultureName = DependencyService.Get<IMultilingual>().CurrentCultureInfo.Parent.Name;

                SearchText = keyword;
                if (AllLocalSaleItems != null && AllLocalSaleItems.Any() && !IsRefresh
                && (!App.Instance.IsInternetConnected || SelectedStartingDate.Date > DateTime.Now.AddDays(-6).Date || SelectedMenu == "Unsync"))
                {
                    if (string.IsNullOrEmpty(SearchText))
                    {
                        ParkFilterSaleItems = new ObservableCollection<InvoiceDto>
                            (AllLocalSaleItems.Where
                            (x => x.TransactionStoreDate.Date.ConvertUTCBasedOnCuture(CurrentCultureName) >= SelectedStartingDate
                            && x.TransactionStoreDate.Date.ConvertUTCBasedOnCuture(CurrentCultureName) <= SelectedEndingDate
                            && (x.Status != InvoiceStatus.Pending || x.Status != InvoiceStatus.initial))
                            .OrderByDescending(x => x.TransactionStoreDate));
                    }
                    else
                    {
                        ParkFilterSaleItems = new ObservableCollection<InvoiceDto>(AllLocalSaleItems.Where(x =>
                                    x.TransactionDate.Date.ConvertUTCBasedOnCuture(CurrentCultureName) >= SelectedStartingDate
                                    && x.TransactionDate.Date.ConvertUTCBasedOnCuture(CurrentCultureName) <= SelectedEndingDate
                                    && (x.Status != InvoiceStatus.Pending || x.Status != InvoiceStatus.initial)
                                    && (x.Number.Contains(SearchText) || (!string.IsNullOrEmpty(x.CustomerName) &&
                                    x.CustomerName.ToLower().Contains(SearchText.ToLower()))
                                    || (!string.IsNullOrEmpty(x.CustomerPhone) &&
                                    x.CustomerPhone.Contains(SearchText))
                                    || (x.InvoiceLineItems.Any(y => (y.EnableSerialNumber && y.SerialNumber.Contains(SearchText)))))
                                ).OrderByDescending(x => x.TransactionStoreDate));
                    }
                }
                else
                {
                    ParkFilterSaleItems = new ObservableCollection<InvoiceDto>();
                }
                //Ticket #10869 End : By Nikhil

                if (!isFromLoadMore)
                {
                    TotalCount = 0;
                    TotalStep = 1;
                    SkipCount = 0;
                }

                if ((SelectedOutletId != Settings.SelectedOutletId || 
                (SelectedStartingDate.Date <= DateTime.Now.AddDays(-6).Date) || (ParkFilterSaleItems == null || !ParkFilterSaleItems.Any()))
                && App.Instance.IsInternetConnected && SelectedMenu != "Unsync")
                {
                    ToDate = SelectedEndingDate.ToStoreUTCTime().AddDays(1).AddSeconds(-1);
                    FromDate = SelectedStartingDate.ToStoreUTCTime();
                    var statuses = GetInvoiceStatuses();
                    var searchStatus = GetInvoiceSearchStatus();
                    GetInvoiceInput getInvoice = new GetInvoiceInput()
                    {
                        filter = SearchText,
                        fromDate = FromDate,
                        toDate = ToDate,
                        maxResultCount = MaxHistoryResult,
                        outletId = SelectedOutletId,
                        skipCount = SkipCount,
                        sorting = "transactionDate desc",
                        status = statuses,
                        SearchStatus = searchStatus
                    };

                    var invocedata = await saleService.GetRemoteInvoiceAndUpdateInLocal(Fusillade.Priority.UserInitiated, getInvoice);
                    if (invocedata != null)
                    {
                        TotalCount = invocedata.result.totalCount;
                        if (TotalCount > MaxHistoryResult)
                        {
                            double tstep = (Convert.ToDouble(TotalCount) / MaxHistoryResult);
                            TotalStep = (int)Math.Ceiling(tstep);
                        }
                        if (TotalStep > 1)
                        {
                            SkipCount += MaxHistoryResult;
                        }
                        if (!isFromLoadMore)
                            ParkSales = new ObservableRangeCollection<InvoiceDto>(invocedata.result.items);
                        else
                            ParkSales.AddRange(invocedata.result.items);
                    }
                    else if (!isFromLoadMore)
                        ParkSales = new ObservableRangeCollection<InvoiceDto>();

                }
                else if (ParkFilterSaleItems != null && ParkFilterSaleItems.Any())
                {
                    if(isFromLoadMore)
                        await Task.Delay(1000);
                    if (!IsIncludeDiscard)
                    {
                        ParkFilterSaleItems = new ObservableCollection<InvoiceDto>(ParkFilterSaleItems.Where(x => x.Status != InvoiceStatus.Voided && x.FinancialStatus != FinancialStatus.VoidedQuote));
                    }
                    if (SelectedMenu == "OpenQuotes")
                    {
                        ParkFilterSaleItems = new ObservableCollection<InvoiceDto>(ParkFilterSaleItems.Where(x => x.Status == InvoiceStatus.Quote && x.FinancialStatus == FinancialStatus.Pending));

                    }
                    else if (SelectedMenu == "ClosedQuotes")
                    {
                        ParkFilterSaleItems = new ObservableCollection<InvoiceDto>(ParkFilterSaleItems.Where(x => x.Status == InvoiceStatus.Quote && x.FinancialStatus == FinancialStatus.Closed));

                    }
                    else if (SelectedMenu == "VoidedQuotes")
                    {
                        ParkFilterSaleItems = new ObservableCollection<InvoiceDto>(ParkFilterSaleItems.Where(x => x.FinancialStatus == FinancialStatus.VoidedQuote));

                    }
                    else if (SelectedMenu == "Unsync")
                    {
                        ParkFilterSaleItems = new ObservableCollection<InvoiceDto>(ParkFilterSaleItems.Where(x => !x.isSync));
                    }
                    else if (SelectedMenu != "All")
                    {
                        ParkFilterSaleItems = new ObservableCollection<InvoiceDto>(ParkFilterSaleItems.Where(x => x.Status.ToString() == SelectedMenu));
                    }
                    
                    TotalCount = ParkFilterSaleItems.Count;
                    if (TotalCount > MaxHistoryResult)
                    {
                        double tstep = (Convert.ToDouble(TotalCount) / MaxHistoryResult);
                        TotalStep = (int)Math.Ceiling(tstep);
                    }
                    if (!isFromLoadMore)
                        ParkSales = new ObservableRangeCollection<InvoiceDto>(ParkFilterSaleItems.Take(MaxHistoryResult));
                    else
                        ParkSales.AddRange(ParkFilterSaleItems.Skip(SkipCount).Take(MaxHistoryResult));
                    if (TotalStep > 1)
                    {
                        SkipCount += MaxHistoryResult;
                    }
                }
                else if (SelectedMenu == "Unsync" && (ParkFilterSaleItems == null || !ParkFilterSaleItems.Any()))
                {
                    ParkSales = new ObservableRangeCollection<InvoiceDto>();
                }
               
                if (ParkSales != null && ParkSales.Any())
                {
                    //START ticket #76208 IOS:FR:Terms of payments by Pratik
                    if (SelectedMenu == "OnAccount")
                    {
                        if (IsOverdue)
                            ParkSales = new ObservableRangeCollection<InvoiceDto>(ParkSales.Where(a => a.InvoiceDueDate.HasValue && a.InvoiceDueDate.Value.ToStoreTime() < DateTime.Now.Date));
                    }
                    //End ticket #76208 by Pratik
                }
                else
                {
                    ParkSales?.Clear();
                }
                Debug.WriteLine("Sale count : " + ParkSales.Count());
            }
            catch (Exception ex)
            {
                ex.Track();
                ParkSales?.Clear();
                Debug.WriteLine("Exception in Filter : " + ex.Message);
            }
        }

        List<InvoiceStatus> GetInvoiceStatuses()
        {
            List<InvoiceStatus> status = new List<InvoiceStatus>() { InvoiceStatus.Pending, InvoiceStatus.Completed, InvoiceStatus.Parked, InvoiceStatus.PartialFulfilled, InvoiceStatus.OnAccount, InvoiceStatus.Refunded, InvoiceStatus.BackOrder, InvoiceStatus.Voided, InvoiceStatus.LayBy, InvoiceStatus.Exchange, InvoiceStatus.Quote, InvoiceStatus.OnGoing }; //#94565

            if (SelectedMenu == "LayBy")
            {
                 status = new List<InvoiceStatus>() { InvoiceStatus.LayBy };
            }
            else if (SelectedMenu == "Parked")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.Parked };
            }
            else if (SelectedMenu == "Voided")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.Voided };
            }
            else if (SelectedMenu == "BackOrder")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.BackOrder };
            }
            else if (SelectedMenu == "OnAccount")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.OnAccount };
            }
            else if (SelectedMenu == "Refunded")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.Refunded };
            }
            else if (SelectedMenu == "Completed")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.Completed };
            }
            else if (SelectedMenu == "OpenQuotes")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.Quote };
            }
            else if (SelectedMenu == "ClosedQuotes")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.Quote };
            }
            else if (SelectedMenu == "VoidedQuotes")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.Voided };
            }
            else if (SelectedMenu == "Exchange")
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.Exchange };
            }
            else if (SelectedMenu == "OnGoing")  //#94565
            {
                status = new List<InvoiceStatus>() { InvoiceStatus.OnGoing };
            }
            return status;
            
        }


        InvoiceSearchStatus GetInvoiceSearchStatus()
        {
            InvoiceSearchStatus invoiceSearchStatus = InvoiceSearchStatus.AllSales;
            if (SelectedMenu == "Voided")
            {
                invoiceSearchStatus = InvoiceSearchStatus.Voided;
            }
            if (SelectedMenu == "OpenQuotes")
            {
                invoiceSearchStatus = InvoiceSearchStatus.Quote;
            }
            else if (SelectedMenu == "ClosedQuotes")
            {
                invoiceSearchStatus = InvoiceSearchStatus.ClosedQuote;
            }
            else if (SelectedMenu == "VoidedQuotes")
            {
               invoiceSearchStatus = InvoiceSearchStatus.VoidedQuote;
            }
            return invoiceSearchStatus;
        }

        public void ReceivedUpdatedInvoiceHandleAction(object sender, Messenger.BackgroundInvoiceUpdatedMessenger arg)
        {
            UpdateInvoice(arg.Value);
        }


        public async void GetPO(InvoiceDto invoice)
        {
            try
            {
                //Create po not allowed for back order in other outlet . by rupeesh
                if (invoice.OutletId != Settings.SelectedOutletId)
                {
                    App.Instance.Hud.DisplayToast("You are currently logged in at " + Settings.SelectedOutletName + " and this transaction belongs to " + invoice.OutletName + " . You can re-open partial completed transaction and take payments for your current outlet only");
                    return;
                }

                if (invoice != null && invoice.Id != 0)
                {
                    using (new Busy(this, true))
                    {
                        if (createPOpage == null)
                        {
                            createPOpage = new CreatePOSlider();
                        }

                        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                        {
                            ObservableCollection<PurchaseOrderDto> PurchaseOrderes = await inventoryService.GetRemotePurchaseOrders(Fusillade.Priority.UserInitiated, true, invoice.Id);

                            createPOpage.ViewModel.Invoice = invoice;
                            createPOpage.ViewModel.PurchaseOrderes = PurchaseOrderes;
                            await NavigationService.PushModalAsync(createPOpage);
                        }
                        else
                        {
                            ObservableCollection<PurchaseOrderDto> PurchaseOrderes = inventoryService.GetLocalPurchaseOrder(invoice.Id);
                            if (PurchaseOrderes != null && PurchaseOrderes.Any())
                            {
                                createPOpage.ViewModel.Invoice = invoice;
                                createPOpage.ViewModel.PurchaseOrderes = PurchaseOrderes;
                                await NavigationService.PushModalAsync(createPOpage);
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
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


        public void ReceivedSignalRInvoiceHandleAction(object sender, Messenger.SignalRInvoiceMessenger arg)
        {
            UpdateInvoice(arg.Value);
        }



        public void UpdateInvoice(InvoiceDto invoice)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    //Ticket start:#44091 iPad-in progress sales issue.by rupesh
                    if (invoice.Status == InvoiceStatus.Pending)
                    {
                        return;
                    }
                    //Ticket end:#44091 .by rupesh

                    if (AllLocalSaleItems == null)
                    {
                        AllLocalSaleItems = new ObservableCollection<InvoiceDto>();
                    }

                    //Ticket start:#22406 Quote sale.by rupesh
                    //Ticket start:#23300 iOS - Quote-related Filter Not Working in Sales History.by rupesh
                    if (invoice.Status.ToString() == SelectedMenu || SelectedMenu == "OpenQuotes" || SelectedMenu == "ClosedQuotes" || SelectedMenu == "VoidedQuotes" || SelectedMenu == "All" || (SelectedMenu == "Unsync" && !invoice.isSync))
                    {
                        //Ticket end:#23300 .by rupesh
                        //Ticket end:#22406 .by rupesh

                        var localInvoice = AllLocalSaleItems.FirstOrDefault(x => (x.Id == invoice.Id && x.Id != 0) || (x.InvoiceTempId == invoice.InvoiceTempId && x.InvoiceTempId != null));
                        if (localInvoice != null)
                        {
                            AllLocalSaleItems.Remove(localInvoice);
                        }
                        AllLocalSaleItems.Insert(0, invoice);


                        if (ParkFilterSaleItems == null)
                        {
                            ParkFilterSaleItems = new ObservableCollection<InvoiceDto>();
                        }

                        var tempParkFilterSaleItem = ParkFilterSaleItems.FirstOrDefault(x => (x.Id == invoice.Id && x.Id != 0) || (x.InvoiceTempId == invoice.InvoiceTempId && x.InvoiceTempId != null));
                        if (tempParkFilterSaleItem != null)
                        {
                            ParkFilterSaleItems.Remove(tempParkFilterSaleItem);
                        }
                        ParkFilterSaleItems.Insert(0, invoice);

                        if (ParkSales == null)
                        {
                            ParkSales = new ObservableRangeCollection<InvoiceDto>();
                        }
                        var tempParkSale = ParkSales.FirstOrDefault(x => (x.Id == invoice.Id && x.Id != 0) || (x.InvoiceTempId == invoice.InvoiceTempId && x.InvoiceTempId != null));
                        if (tempParkSale != null)
                        {
                            ParkSales.Remove(tempParkSale);
                        }

                        //Ticket start:#30840,#32904 Hide Discarded Sales/Void in Sales History.by rupesh
                        if (IsIncludeDiscard || !(invoice.Status == InvoiceStatus.Voided || invoice.FinancialStatus == FinancialStatus.VoidedQuote))
                        {
                            ParkSales.Insert(0, invoice);
                        }
                        //Ticket end:#30840,#32904 .by rupesh
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            });
        }

        public void SearchSalesByBarcodeAsync(string barcodenumber, IMPOPStarBarcode mPOPStarBarcode)
        {
            try
            {

                if (AllLocalSaleItems == null)
                {
                    return;
                }
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    using (new Busy(this, true))
                    {
                        InvoiceDto FilterSalesItem = new InvoiceDto();
                        Debug.WriteLine("AllLocalSaleItems : " + JsonConvert.SerializeObject(AllLocalSaleItems));
                        FilterSalesItem = AllLocalSaleItems.FirstOrDefault(x => x.Barcode == barcodenumber);
                        if (FilterSalesItem == null)
                        {
                            FilterSalesItem = AllLocalSaleItems.FirstOrDefault(x => x.Number == barcodenumber);
                        }
                        if (FilterSalesItem == null)
                        {
                            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                            List<InvoiceStatus> status = new List<InvoiceStatus>() { InvoiceStatus.Pending, InvoiceStatus.Completed, InvoiceStatus.Parked, InvoiceStatus.PartialFulfilled, InvoiceStatus.OnAccount, InvoiceStatus.Refunded, InvoiceStatus.BackOrder, InvoiceStatus.Voided, InvoiceStatus.LayBy, InvoiceStatus.Exchange, InvoiceStatus.Quote, InvoiceStatus.OnGoing }; //#94565
                            //End #84293 by Pratik
                            ObservableCollection<InvoiceDto> BarcodeSearchList = await saleService.GetRemoteInvoices(Fusillade.Priority.UserInitiated, true, SelectedOutletId, null, null, status, barcodenumber);
                            if (BarcodeSearchList != null)
                            {
                                FilterSalesItem = BarcodeSearchList.FirstOrDefault(x => x.Barcode == barcodenumber);
                                if (FilterSalesItem == null)
                                {
                                    FilterSalesItem = BarcodeSearchList.FirstOrDefault(x => x.Number == barcodenumber);
                                }
                            }
                        }

                        if (FilterSalesItem != null && !string.IsNullOrEmpty(FilterSalesItem.Barcode))
                        {
                            await OpenInvoiceDetailView(FilterSalesItem);
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInvoiceItemBarcodeMessage"), Colors.Red, Colors.White);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task<bool> OpenInvoiceDetailView(InvoiceDto invoice)
        {
            using (new Busy(this, true))
            {
                try
                {
                    if (detailpage == null)
                    {
                        detailpage = new ParkSaleDetailPage();
                        detailpage.ViewModel.InvoiceRefund += async delegate (object sender, InvoiceDto e)
                        {
                            if (e.Status != InvoiceStatus.BackOrder)
                                await Detailpage_InvoiceRefund(sender, e);

                        };

                        detailpage.ViewModel.InvoiceExchange += async delegate (object sender, InvoiceDto e)
                        {
                            await Detailpage_InvoiceExchange(sender, e);
                        };
                        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location  by Pratik
                        detailpage.ViewModel.InvoiceStatusChanged += delegate (object sender, InvoiceDto e)
                        {
                            var data = ParkSales.FirstOrDefault(a => a.Id == e.Id);
                            if (data != null)
                            {
                                data.Status = e.Status;
                            }
                            else
                            {
                                data = ParkSales.FirstOrDefault(a => a.InvoiceTempId == e.InvoiceTempId);
                                if (data != null)
                                {
                                    data.Status = e.Status;
                                }
                            }
                        };
                        //End #84293  by Pratik
                    }

                    //Start TICKET #76698 iPAD FR: Next Sale by Pratik
                    detailpage.ViewModel.CurrentIndex = ParkSales.IndexOf(invoice);
                    //End TICKET #76698 by Pratik

                    if (detailpage.ViewModel.inPorgress)
                        return true;

                    detailpage.ViewModel.inPorgress = true;
                    detailpage.ViewModel.Invoice = null;
                    await detailpage.ViewModel.Close();                    
                    await NavigationService.PushModalAsync(detailpage);                   
                    detailpage.ViewModel.inPorgress = false;

                    //Added by rupesh for selected outlet only.
                    if (invoice.OutletId == Settings.SelectedOutletId)
                    {
                        InvoiceDto invoice1;
                        if (invoice.Id != 0)
                        {
                            invoice1 = saleService.GetLocalInvoice(invoice.Id);
                            if (invoice1 == null)
                            {
                                invoice = await saleService.GetRemoteInvoice(Fusillade.Priority.UserInitiated, true, invoice.Id);
                            }
                            else
                            {
                                invoice = invoice1;
                            }
                        }
                        else
                        {
                            invoice = saleService.GetLocalInvoiceByTempId(invoice.InvoiceTempId);
                            if (invoice == null)
                            {
                                invoice = AllLocalSaleItems.FirstOrDefault(x => x.InvoiceTempId == invoice.InvoiceTempId);
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

                    detailpage.ViewModel.Invoice = invoice;
                    detailpage.ViewModel.SetInvoiceDetails();

                    //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
                    if (invoice != null)
                    {

                        detailpage.ViewModel.Invoice.SurchargeDisplayInSaleHistory = (invoice.TotalTip - invoice.TipTaxAmount);
                        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                        detailpage.ViewModel.Invoice.SurchargeDisplayInSaleHistory += detailpage.ViewModel.Invoice.TotalPaymentSurcharge;
                        //End Ticket #73190
                    }
                    //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total


                    //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.

                    if (Settings.CurrentRegister.IsOpened && invoice != null )
                    {

                        if (invoice!=null && invoice.Status == InvoiceStatus.Completed && invoice.InvoiceFrom != InvoiceFrom.iPad && invoice.InvoiceFrom != InvoiceFrom.Android
                            && invoice.InvoiceFrom != InvoiceFrom.Web && invoice.RegisterClosureId == Settings.CurrentRegister.Registerclosure.Id)

                            detailpage.ViewModel.Invoice.IsSerialNumberEditableFromSaleHistory = true;
                        else
                            detailpage.ViewModel.Invoice.IsSerialNumberEditableFromSaleHistory = false;
                    }
                    else if(invoice!=null)
                    {
                        detailpage.ViewModel.Invoice.IsSerialNumberEditableFromSaleHistory = false;
                    }

                    //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.


                    // detailpage.ViewModel.Invoice.InvoicePayments
                    if (detailpage.ViewModel.Invoice.InvoicePayments != null)
                    {
                        foreach (var item in detailpage.ViewModel.Invoice.InvoicePayments)
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

                    // List<Task> tasks = new List<Task>();
                    // tasks.Add(detailpage.ViewModel.LoadOutletsDetails());
                    // tasks.Add(detailpage.ViewModel.GetTaxDetails());
                    // tasks.Add(detailpage.ViewModel.CollapseItemDetail("OrderDetail"));
                    // tasks.Add(detailpage.ViewModel.UpdateRefundHistoryList());
                    detailpage.ViewModel.LoadOutletsDetails();
                    detailpage.ViewModel.GetTaxDetails();
                    detailpage.ViewModel.CollapseItemDetail("OrderDetail");
                    detailpage.ViewModel.UpdateRefundHistoryList();
                    // await Task.WhenAll(tasks);

                    //Start TICKET #76698 iPAD FR: Next Sale by Pratik
                    if (Settings.Subscription.Edition.PlanType != null && (Settings.Subscription.Edition.PlanType == PlanType.Plus || Settings.Subscription.Edition.PlanType == PlanType.Trial))
                    {
                        detailpage.ViewModel.ParkSales = ParkSales;
                        detailpage.ViewModel.NextPreviousButtonSet();
                    }
                    //End TICKET #76698 by Pratik

                    return true;
                }
                catch (Exception ex)
                {
                    ex.Track();
                    if (detailpage != null)
                    {
                        await detailpage.ViewModel.Close();
                        detailpage.ViewModel.inPorgress = false;
                    }
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                    return true;
                }

            }

        }
        

        //public ICommand LoadMoreSaleCommand { get; }
        DateTime FromDate;
        DateTime ToDate;

        bool needFilter;
        public async Task LoadMoreSale()
        {
            try
            {
                   // await Task.Delay(10);
                    //Ticket no #10114 by rupesh
                    if (ParkSales.Count < TotalCount)
                    {
                        FirstTimeLoad = false;
                        await Filter(SearchText, true);
                        // if (App.Instance.IsInternetConnected)
                        // {
                        //     if (FromDate != Settings.LastLoadedHistoryDate.AddDays(-7))
                        //     {
                        //       using (new Busy(this, true))
                        //       {

                        //         FromDate = Settings.LastLoadedHistoryDate.AddDays(-7);
                        //         //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                        //         List<InvoiceStatus> status = new List<InvoiceStatus>() { InvoiceStatus.Pending, InvoiceStatus.Completed, InvoiceStatus.Parked, InvoiceStatus.PartialFulfilled, InvoiceStatus.OnAccount, InvoiceStatus.Refunded, InvoiceStatus.BackOrder, InvoiceStatus.Voided, InvoiceStatus.LayBy, InvoiceStatus.Exchange, InvoiceStatus.Quote };
                        //         //End #84293 by Pratik
                        //         var previoussales = await saleService.GetRemoteInvoices(Fusillade.Priority.UserInitiated, true, SelectedOutletId, FromDate, Settings.LastLoadedHistoryDate, status);
                        //         if (previoussales != null && previoussales.Count > 0)
                        //         {
                        //             AllLocalSaleItems = new ObservableCollection<InvoiceDto>(AllLocalSaleItems.Concat(previoussales));
                        //             ParkFilterSaleItems = AllLocalSaleItems;
                        //             needFilter = true;

                        //             Settings.LastLoadedHistoryDate = FromDate;
                        //         }
                        //        }
                        //     }
                            
                        //  }
                    }

                    // if (needFilter)
                    // {
                    //     needFilter = false;
                    //     Filter(SearchText, true);
                    // }

                   // var templst = ParkFilterSaleItems.Skip(ParkSales.Count).Take(30).ToList();


                    // MainThread.BeginInvokeOnMainThread(() =>
                    // {
                    //     ParkSales.AddRange(templst);
                    // });
                
            }
            catch (Exception ex)
            {
                ex.Track();
                Debug.WriteLine("Exception in LoadMoreSale : " + ex.Message);
            }
            return;

        }
        async Task Detailpage_InvoiceExchange(object sender, Models.InvoiceDto Invoice)
        {
            var Permissions = Settings.GrantedPermissionNames;
            if (Permissions == null && Permissions.Any(s => s == "Pages.Tenant.POS.EnterSale"))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("EnterSaleNoPermissionMessage"));
                return;
            }

            if (!App.Instance.IsInternetConnected)
            {
                //Application.Current.MainPage.DisplayAlert("Alert", "You can not refund without internet connection", "Ok");
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundInternetValidationMessage"));
                return;
            }

            if (Invoice.Id == 0)
            {

                await App.Alert.ShowAlert("Alert", "something went wrong, please sync data and try again", "Ok");
                //Application.Current.MainPage.DisplayAlert("Alert", "You can not refund without sync this record", "Ok");
                //App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundSyncValidationMessage"));
                return;
            }
            if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundInvoiceAlertMessage"));
                return;
            }

            //Ticket start: #12153 Prompt of Refunding/Exchanging Giftcards by rupesh
            var isGiftCardOnly = Invoice.InvoiceLineItems.All(x => x.InvoiceItemType == InvoiceItemType.GiftCard);
            if (isGiftCardOnly)
            {
                App.Instance.Hud.DisplayToast("Gift card sale can not be exchanged");
                return;

            }
            //Ticket end: #12153

            var refundInvoice = Invoice.Copy();
            refundInvoice.Id = 0;

            var refundedlogs = Invoice.InvoiceHistories?.Where(x => x.Status == InvoiceStatus.Exchange);
            if (refundedlogs != null && refundedlogs.Count() > 0)
                refundInvoice.Number += "-EX" + refundedlogs.Count();
            else
                refundInvoice.Number += "-EX";

            //Ticket start: #66249 Net sales amount seems to be incorrect By Pratik
            if (refundInvoice.Status == InvoiceStatus.Exchange)
            {
                refundInvoice.DiscountValue = 0;
            }
            else if (refundInvoice.DiscountValue > 0 && !refundInvoice.DiscountIsAsPercentage)
            {
                refundInvoice.DiscountIsAsPercentage = true;
                var TotalEffectiveamount = refundInvoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
                refundInvoice.DiscountValue = (refundInvoice.DiscountValue * 100) / TotalEffectiveamount;
            }
            //Ticket end: #66249

            refundInvoice.Barcode = Settings.CurrentRegister.Id.ToString() + refundInvoice.Number; //  "#" + refundInvoice.Number;
            refundInvoice.Status = InvoiceStatus.Exchange;
            refundInvoice.TransactionDate = Extensions.moment().AddSeconds(1);
            refundInvoice.TotalPaid = 0;
            refundInvoice.RoundingAmount = 0;
            refundInvoice.TotalTender = 0;
            //refundInvoice.fromExchange = true;
            refundInvoice.ChangeAmount = 0;
            refundInvoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
            refundInvoice.ToRefundPayments = Invoice.InvoicePayments;

            refundInvoice.IsReStockWhenRefund = true; //isReStock;
            refundInvoice.ReferenceInvoiceId = Invoice.Id;
            //refundInvoice.referenceStatus = Invoice.referenceStatus;

            //Zoho Ticket: 8082
            refundInvoice.InvoiceTempId = nameof(InvoiceDto) + "_" + Guid.NewGuid().ToString();//Added to change syncReference in exchange sale


            var ExchangeReferenceNote = new ObservableCollection<RefundSummary>();
            ExchangeReferenceNote.Add(new RefundSummary() { Title = "Original sale " + Invoice.Number });


            if (refundInvoice.ExchangeReferenceNote != null)
            {
                Debug.WriteLine("ExchangeReferenceNote before SerializeObject : " + refundInvoice.ExchangeReferenceNote.ToString());

                refundInvoice.ExchangeReferenceNote = JsonConvert.SerializeObject(ExchangeReferenceNote);

                // if (refundInvoice.ExchangeReferenceNote != null)
                Debug.WriteLine("ExchangeReferenceNote : " + refundInvoice.ExchangeReferenceNote.ToString());
            }
            decimal checkAllItemRefunded = 0;
            var isAlreadyRefunded = true;
            var refundLineitems = new ObservableCollection<InvoiceLineItemDto>();
            decimal discountEffectiveAmount = 0;
            foreach (var value in refundInvoice.InvoiceLineItems)
            {
                 //Ticket:start:#90938 IOS:FR Age varification.by rupesh
                 value.InvoiceLineItemDetails = null;
                 //Ticket:end:#90938 .by rupesh
                value.IsExchangedProduct = true;
                if (value.InvoiceItemType != InvoiceItemType.GiftCard && value.InvoiceItemType != InvoiceItemType.Discount)
                {
                    if (value.Quantity > 0)
                    {
                        var quantity = (value.Quantity - value.RefundedQuantity) * (-1);

                        value.Quantity = (quantity % 1) != 0 ? (Math.Round(quantity, 2)) : quantity;
                        value.actualqty = quantity;
                        value.ActionType = ActionType.Refund;
                        value.InvoiceId = Invoice.Id;
                        value.Id = value.Id;


                        checkAllItemRefunded = Math.Abs(value.Quantity);

                        if (checkAllItemRefunded > 0)
                        {
                            isAlreadyRefunded = false;
                        }

                        InvoiceCalculations.CalculateLineItemTotal(value, refundInvoice);

                        //Ticket start:#21386 Android : I can't change the stock manually to exchange product.by rupesh
                        if (checkAllItemRefunded > 0)
                        {
                            refundLineitems.Add(value);
                        }
                        //Ticket end:#21386 .by rupesh
                    }

                    //refundLineitems.Add(value);
                }
                else if (value.InvoiceItemType == InvoiceItemType.Discount && value.EffectiveAmount < 0)
                {

                    value.EffectiveAmount = Math.Abs(value.EffectiveAmount + discountEffectiveAmount);
                    value.SoldPrice = Math.Abs(value.SoldPrice + discountEffectiveAmount);
                    value.RetailPrice = Math.Abs(value.RetailPrice + discountEffectiveAmount);
                    value.TaxExclusiveTotalAmount = Math.Abs(value.TaxExclusiveTotalAmount + discountEffectiveAmount);
                    value.TotalAmount = Math.Abs(value.TotalAmount + discountEffectiveAmount);


                    InvoiceCalculations.CalculateLineItemTotal(value, refundInvoice);
                    refundLineitems.Add(value);
                }
            }
            refundInvoice.InvoiceLineItems = refundLineitems;

            if (!isAlreadyRefunded)
            {
                //added by rupesh for confirmation refund from other outlet from filter.
                var confirmation = true;
                if (refundInvoice.OutletId != Settings.SelectedOutletId)
                {
                    confirmation = await App.Alert.ShowAlert("Confirmation",
"You are currently logged in at " + Settings.SelectedOutletName + " and about to process refund / exchange for a sale processed at " + refundInvoice.OutletName + " .Continue ?", "Yes", "No");
                    refundInvoice.OutletId = Settings.SelectedOutletId;
                }

                if (!confirmation)
                    return;

                if (Invoice.InvoicePayments != null)
                    refundInvoice.InvoiceRefundPayments = new ObservableCollection<InvoicePaymentDto>(Invoice.InvoicePayments.Where(x => !x.IsDeleted));
                else
                    refundInvoice.InvoiceRefundPayments = new ObservableCollection<InvoicePaymentDto>();

               // var mainpage = (MainPage)_navigationService.MainPage;
                await detailpage.ViewModel.Close();
                //mainpage.ChangePage("EnterSalePage");
                Shell.Current.CurrentItem = Shell.Current.Items[0];
                 _ = Task.Run(() =>
                {
                    MainThread.BeginInvokeOnMainThread(async()=> 
                    { 
                        await ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.ExchangeSaleFromHistory(refundInvoice);
                    });
                });
                
            }
            else
            {
                // App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ThisSaleAlreadyRefunded"));
                App.Instance.Hud.DisplayToast("This sale already exchanged");
                return;
            }
        }

        async Task Detailpage_InvoiceRefund(object sender, Models.InvoiceDto Invoice)
        {

            try
            {

                var Permissions = Settings.GrantedPermissionNames;
                if (Permissions == null && Permissions.Any(s => s == "Pages.Tenant.POS.EnterSale"))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("EnterSaleNoPermissionMessage"));
                    return;
                }

                if (!App.Instance.IsInternetConnected)
                {
                    //Application.Current.MainPage.DisplayAlert("Alert", "You can not refund without internet connection", "Ok");
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundInternetValidationMessage"));
                    return;
                }

                if (Invoice.Id == 0)
                {

                    _= App.Alert.ShowAlert("Alert", "something went wrong, please sync data and try again", "Ok");
                    //Application.Current.MainPage.DisplayAlert("Alert", "You can not refund without sync this record", "Ok");
                    //App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundSyncValidationMessage"));
                    return;
                }
                if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundInvoiceAlertMessage"));
                    return;
                }

                //Ticket start: #12153 Prompt of Refunding/Exchanging Giftcards by rupesh
                var isGiftCardOnly = Invoice.InvoiceLineItems.All(x => x.InvoiceItemType == InvoiceItemType.GiftCard);
                if (isGiftCardOnly)
                {
                    App.Instance.Hud.DisplayToast("Gift card sale can not be refunded");
                    return;

                }
                //Ticket end: #12153

                // Note : we added below condition to refund exchange sale without effecting existing refund feature.

                if (Invoice.Status == InvoiceStatus.Exchange)
                {
                    #region Exchange refund section


                    var refundInvoice = Invoice.Copy();
                    refundInvoice.Id = 0;
                    //Ticket start:#52581 Refund(Store Credit) Does Not Reflect On System.by rupesh
                    refundInvoice.InvoiceTempId = null;
                    //Ticket end:#52581 .by rupesh
                    var refundedCount = AllLocalSaleItems.Count(x => x.Status == InvoiceStatus.Refunded && x.ReferenceInvoiceId == refundInvoice.Id);
                    if (refundedCount > 0)
                        refundInvoice.Number += "-RF" + refundedCount;
                    else
                        refundInvoice.Number += "-RF";

                    refundInvoice.Barcode = Settings.CurrentRegister.Id.ToString() + refundInvoice.Number; // "#" + refundInvoice.Number;
                    refundInvoice.Status = InvoiceStatus.Refunded;
                    refundInvoice.TransactionDate = Extensions.moment().AddSeconds(1);
                    refundInvoice.TotalPaid = 0;
                    refundInvoice.RoundingAmount = 0;
                    refundInvoice.TotalTender = 0;
                    //refundInvoice.fromExchange = true;
                    refundInvoice.ChangeAmount = 0;
                    refundInvoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                    refundInvoice.ToRefundPayments = Invoice.InvoicePayments;

                    refundInvoice.IsReStockWhenRefund = true; //isReStock;
                    refundInvoice.ReferenceInvoiceId = Invoice.Id;
                    //refundInvoice.referenceStatus = Invoice.referenceStatus;

                    var ExchangeReferenceNote = new ObservableCollection<RefundSummary>();
                    ExchangeReferenceNote.Add(new RefundSummary() { Title = "Original sale " + Invoice.Number });


                    if (refundInvoice.ExchangeReferenceNote != null)
                    {
                        Debug.WriteLine("ExchangeReferenceNote before SerializeObject : " + refundInvoice.ExchangeReferenceNote.ToString());

                        refundInvoice.ExchangeReferenceNote = JsonConvert.SerializeObject(ExchangeReferenceNote);

                        // if (refundInvoice.ExchangeReferenceNote != null)
                        Debug.WriteLine("ExchangeReferenceNote : " + refundInvoice.ExchangeReferenceNote.ToString());
                    }
                    decimal checkAllItemRefunded = 0;
                    var isAlreadyRefunded = true;
                    var refundLineitems = new ObservableCollection<InvoiceLineItemDto>();
                    foreach (var value in refundInvoice.InvoiceLineItems)
                    {
                        if (value.InvoiceItemType != InvoiceItemType.GiftCard && value.InvoiceItemType != InvoiceItemType.Discount)
                        {
                            if (value.Quantity > 0)
                            {
                                var quantity = (value.Quantity - value.RefundedQuantity) * (-1);

                                value.Quantity = (quantity % 1) != 0 ? (Math.Round(quantity, 2)) : quantity;
                                value.actualqty = quantity;
                                value.ActionType = ActionType.Refund;
                                value.InvoiceId = Invoice.Id;
                                value.Id = value.Id;
                                checkAllItemRefunded = Math.Abs(value.Quantity);

                                if (checkAllItemRefunded > 0)
                                {
                                    isAlreadyRefunded = false;
                                }
                                InvoiceCalculations.CalculateLineItemTotal(value, refundInvoice);
                                //Ticket start:#21386 Android : I can't change the stock manually to exchange product.by rupesh
                                if (checkAllItemRefunded > 0)
                                {
                                    refundLineitems.Add(value);
                                }
                                //Ticket end:#21386 .by rupesh
                            }

                            //refundLineitems.Add(value);
                        }
                        else if (value.InvoiceItemType == InvoiceItemType.Discount && value.EffectiveAmount < 0)
                        {
                            refundLineitems.Add(value);
                        }
                    }

                    refundInvoice.InvoiceLineItems = refundLineitems;

                    if (!isAlreadyRefunded)
                    {
                        //added by rupesh for confirmation refund from other outlet from filter.

                        var confirmation = true;
                        if (refundInvoice.OutletId != Settings.SelectedOutletId)
                        {
                            confirmation = await App.Alert.ShowAlert("Confirmation",
"You are currently logged in at " + Settings.SelectedOutletName + " and about to process refund / exchange for a sale processed at " + refundInvoice.OutletName + " .Continue ?", "Yes", "No");
                            refundInvoice.OutletId = Settings.SelectedOutletId;
                        }

                        if (!confirmation)
                            return;

                        refundInvoice.InvoiceRefundPayments = new ObservableCollection<InvoicePaymentDto>(Invoice.InvoicePayments.Where(x => !x.IsDeleted));

                        //var mainpage = (MainPage)_navigationService.MainPage;
                        await detailpage.ViewModel.Close();
                        //mainpage.ChangePage("EnterSalePage");
                        Shell.Current.CurrentItem = Shell.Current.Items[0];
                        _ = Task.Run(() =>
                        {
                            MainThread.BeginInvokeOnMainThread(async()=> 
                            { 
                                await ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.ExchangeSaleFromHistory(refundInvoice);
                            });
                        });
                    }
                    else
                    {
                        //Ticket #9972 Start : Exchange related issues. By Nikhil
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ThisSaleAlreadyRefunded"));
                        //Ticket #9972 End : By Nikhil
                        return;
                    }
                    
                    #endregion
                }
                else
                {
                    #region Normal refund flow

                    //Ticket start:#26645 iPad: Discount offer sale should be refunded.by rupesh
                    if (Invoice.ReferenceInvoiceId == null ||  Invoice.Status == InvoiceStatus.Completed && Invoice.InvoiceHistories?.FirstOrDefault()?.Status == InvoiceStatus.Quote)
                    //Ticket end:#26645 .by rupesh
                    {
                        InvoiceDto refundInvoice;
                        //Added by rupesh for selected outlet only.
                        if (Invoice.OutletId == Settings.SelectedOutletId)
                        {
                            refundInvoice =  saleService.GetLocalInvoice(Invoice.Id);
                        }
                        else
                        {
                            refundInvoice = Invoice.Copy();
                        }
                        if (refundInvoice == null)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoFoundSales"));
                            return;
                        }

                        refundInvoice.CustomerDetail = Invoice.CustomerDetail;
                        //Ticket start:#29036,#29024 iPad - Delivery address should be print in refund sale receipt.by rupesh
                        refundInvoice.DeliveryAddress = Invoice.DeliveryAddress;
                        //Ticket start:#29036,#29024 .by rupesh
                        decimal checkAllItemRefunded = 0;
                        var isAlreadyRefunded = true;

                        if (refundInvoice.InvoiceLineItems != null)
                        {
                            //Ticket start:#21386 Android : I can't change the stock manually to exchange product.by rupesh
                            var refundLineitems = new ObservableCollection<InvoiceLineItemDto>();
                           // foreach (var item in refundInvoice.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount))
                            foreach (var item in refundInvoice.InvoiceLineItems)
                            {

                               if (item.InvoiceItemType != InvoiceItemType.Discount && item.InvoiceItemType != InvoiceItemType.GiftCard)
                                {



                                    var tmpQuantity = (item.Quantity - item.RefundedQuantity) * -1;
                                    checkAllItemRefunded = Math.Abs(tmpQuantity);

                                    if (checkAllItemRefunded > 0)
                                    {
                                        isAlreadyRefunded = false;
                                        refundLineitems.Add(item);

                                    }

                                }
                               else
                                {
                                    refundLineitems.Add(item);
                                }

                            }
                            refundInvoice.InvoiceLineItems = refundLineitems;
                            //Ticket end:#21386 .by rupesh

                            if (!isAlreadyRefunded)
                            {
                                //added by rupesh for confirmation refund from other outlet from filter.
                                var confirmation = true;
                                if (refundInvoice.OutletId != Settings.SelectedOutletId)
                                {
                                    confirmation = await App.Alert.ShowAlert("Confirmation",
"You are currently logged in at " + Settings.SelectedOutletName + " and about to process refund / exchange for a sale processed at " + refundInvoice.OutletName + " .Continue ?", "Yes", "No");
                                    refundInvoice.OutletId = Settings.SelectedOutletId;
                                }

                                if (!confirmation)
                                    return;

                                refundInvoice.IsReStockWhenRefund = true;

                                var currentRegister = Settings.CurrentRegister;
                                if (currentRegister != null)
                                {

                                    refundInvoice.RegisterId = currentRegister.Id;
                                    refundInvoice.RegisterName = currentRegister.Name;

                                    //Ticket #12850 Tax in Register Summaries Calculated Wrongly. By Nikhil	 
                                    var registerColusreId = 0;
                                    if (currentRegister.Registerclosure != null)
                                        registerColusreId = currentRegister.Registerclosure.Id;

                                    refundInvoice.RegisterClosureId = registerColusreId;
                                    refundInvoice.InvoiceLineItems.ForEach(x =>
                                    {
                                        x.RegisterId = currentRegister.Id;
                                        x.RegisterClosureId = registerColusreId;
                                    });
                                    //Ticket #12850 End. By Nikhil
                                }

                                var offerLineItem = refundInvoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.Discount);
                                if (offerLineItem != null)
                                {
                                    //var tempInvoice = refundInvoice.Copy();
                                    var tempOfferLineItem = new List<InvoiceLineItemDto>();

                                    while (offerLineItem.Count() > 0)
                                    {
                                        var invoiceItem = offerLineItem.FirstOrDefault()?.Copy();
                                        if (invoiceItem == null)
                                            return;
                                        refundInvoice.InvoiceLineItems.Remove(offerLineItem.FirstOrDefault());


                                        invoiceItem.Id = 0;
                                        invoiceItem.InvoiceId = 0;

                                        decimal discountEffectiveAmount = 0;
                                        foreach (var item in Invoice.InvoiceLineItems)
                                        {
                                            if (item.InvoiceItemType != InvoiceItemType.Discount)
                                            {
                                                if (item.RefundedQuantity >= 0)
                                                {
                                                    //var amount = value.refundedQuantity * value.soldPrice;
                                                    //discountEffectiveAmount = discountEffectiveAmount + amount;

                                                    var discountedValue = InvoiceCalculations.GetValuefromPercent(item.SoldPrice, item.OfferDiscountPercent);
                                                    var amount = discountedValue * item.RefundedQuantity;
                                                    discountEffectiveAmount = discountEffectiveAmount + amount;
                                                    //retailPrice = invoiceItem.retailPrice - discountedValue;
                                                }
                                            }
                                        }

                                        //invoiceItem.EffectiveAmount = invoiceItem.EffectiveAmount + discountEffectiveAmount;
                                        //invoiceItem.SoldPrice = invoiceItem.SoldPrice + discountEffectiveAmount;
                                        //invoiceItem.RetailPrice = invoiceItem.RetailPrice + discountEffectiveAmount;
                                        //invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount + discountEffectiveAmount;
                                        //invoiceItem.TotalAmount = invoiceItem.TotalAmount + discountEffectiveAmount;


                                        invoiceItem.EffectiveAmount = Math.Abs(invoiceItem.EffectiveAmount + discountEffectiveAmount);
                                        invoiceItem.SoldPrice = Math.Abs(invoiceItem.SoldPrice + discountEffectiveAmount);
                                        invoiceItem.RetailPrice = Math.Abs(invoiceItem.RetailPrice + discountEffectiveAmount);
                                        invoiceItem.TaxExclusiveTotalAmount = Math.Abs(invoiceItem.TaxExclusiveTotalAmount + discountEffectiveAmount);
                                        invoiceItem.TotalAmount = Math.Abs(invoiceItem.TotalAmount + discountEffectiveAmount);

                                        //Add temp properties..
                                        //invoiceItem.actualEffectiveAmount = invoiceItem.effectiveAmount + discountEffectiveAmount;
                                        //invoiceItem.actualSoldPrice = invoiceItem.soldPrice + discountEffectiveAmount;
                                        //invoiceItem.actualRetailPrice = invoiceItem.retailPrice + discountEffectiveAmount;
                                        //invoiceItem.actualTaxExclusiveTotalAmount = invoiceItem.taxExclusiveTotalAmount + discountEffectiveAmount;
                                        //invoiceItem.actualTotalAmount = invoiceItem.totalAmount + discountEffectiveAmount;
                                        //Add temp properties..

                                        invoiceItem.Quantity = invoiceItem.Quantity * -1;
                                        tempOfferLineItem.Add(invoiceItem);
                                    }
                                    foreach (var item in tempOfferLineItem)
                                    {
                                        refundInvoice.InvoiceLineItems.Add(item);
                                    }

                                }
                            }
                            else
                            {
                                if (Invoice.Status == InvoiceStatus.Exchange)
                                {
                                    App.Instance.Hud.DisplayToast("This sale already exchanged");
                                    return;
                                }
                                else
                                {
                                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ThisSaleAlreadyRefunded"));
                                    return;
                                }

                            }
                        }
                        else
                        {
                            return;
                        }

                        if (!isAlreadyRefunded)
                        {
                            var refundedCount = AllLocalSaleItems.Count(x => x.Status == InvoiceStatus.Refunded && x.ReferenceInvoiceId == refundInvoice.Id);
                            if (refundedCount > 0)
                                refundInvoice.Number += "-RF" + refundedCount;
                            else
                                refundInvoice.Number += "-RF";

                            refundInvoice.InvoiceRefundPayments = new ObservableCollection<InvoicePaymentDto>(Invoice.InvoicePayments.Where(x => !x.IsDeleted));
                            //var mainpage = (MainPage)_navigationService.MainPage;
                            await detailpage.ViewModel.Close();
                            //mainpage.ChangePage("EnterSalePage");
                             Shell.Current.CurrentItem = Shell.Current.Items[0];
                            _ = Task.Run(() =>
                            {
                                MainThread.BeginInvokeOnMainThread(async()=> 
                                { 
                                    await((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.RefundSaleFromHistory(refundInvoice);
                                });
                            });
                        }

                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
        async Task Detailpage_InvoiceRefund1(object sender, Models.InvoiceDto Invoice)
        {
            var Permissions = Settings.GrantedPermissionNames;
            if (Permissions == null && Permissions.Any(s => s == "Pages.Tenant.POS.EnterSale"))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("EnterSaleNoPermissionMessage"));
                return;
            }

            if (!App.Instance.IsInternetConnected)
            {
                //Application.Current.MainPage.DisplayAlert("Alert", "You can not refund without internet connection", "Ok");
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundInternetValidationMessage"));
                return;
            }

            if (Invoice.Id == 0)
            {
                //Application.Current.MainPage.DisplayAlert("Alert", "You can not refund without sync this record", "Ok");
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundSyncValidationMessage"));
                return;
            }
            if (Settings.CurrentRegister == null || !Settings.CurrentRegister.IsOpened)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundInvoiceAlertMessage"));
                return;
            }

            InvoiceDto refundInvoice = saleService.GetLocalInvoice(Invoice.Id);

            refundInvoice.Id = 0;

            //var refundedlogs = abpCommonHelpers.$filter("where")(vm.invoice.invoiceHistories, { status: app.consts.invoiceStatus.refunded });
            var refundedCount = AllLocalSaleItems.Count(x => x.Status == InvoiceStatus.Refunded && x.ReferenceInvoiceId == Invoice.Id);
            if (refundedCount > 0)
                refundInvoice.Number += "-RF" + refundedCount;
            else
                refundInvoice.Number += "-RF";

            refundInvoice.Barcode = Settings.CurrentRegister.Id.ToString() + refundInvoice.Number; // "#" + refundInvoice.Number;
            refundInvoice.Status = InvoiceStatus.Refunded;

            refundInvoice.TransactionDate = Extensions.moment();
            refundInvoice.TotalPaid = 0;
            refundInvoice.RoundingAmount = 0;
            refundInvoice.TotalTender = 0;
            refundInvoice.ChangeAmount = 0;


            refundInvoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
            refundInvoice.ToRefundPayments = Invoice.InvoicePayments;

            refundInvoice.IsReStockWhenRefund = true;
            refundInvoice.ReferenceInvoiceId = Invoice.Id;
            //refundInvoice.referenceStatus = vm.invoice.status;

            refundInvoice.InvoiceRefundPayments = Invoice.InvoicePayments;

            decimal checkAllItemRefunded = 0;
            var isAlreadyRefunded = true;
            var isAlreadyExchanged = true;
            var refundLineitems = new ObservableCollection<InvoiceLineItemDto>();
            foreach (var value in refundInvoice.InvoiceLineItems)
            {
                //item.isExchangeDiscount = true;
                if (value.InvoiceItemType != InvoiceItemType.GiftCard && value.InvoiceItemType != InvoiceItemType.Discount)
                {
                    if (Invoice.Status == InvoiceStatus.Exchange)
                    {
                        if (value.Quantity > 0)
                        {
                            var quantity = (value.Quantity - value.RefundedQuantity) * (-1);

                            value.Quantity = (quantity % 1) != 0 ? (Math.Round(quantity, 2)) : quantity;
                            value.actualqty = quantity;
                            value.ActionType = ActionType.Refund;
                            value.InvoiceId = Invoice.Id;
                            value.Id = value.Id;

                            checkAllItemRefunded = Math.Abs(value.Quantity);

                            if (checkAllItemRefunded > 0)
                            {
                                // isAlreadyRefunded = false;
                                isAlreadyExchanged = false;
                            }
                            InvoiceCalculations.CalculateLineItemTotal(value, refundInvoice);
                            refundLineitems.Add(value);
                        }
                    }
                    else
                    {
                        var quantity = (value.Quantity - value.RefundedQuantity) * (-1);

                        value.Quantity = (quantity % 1) != 0 ? (Math.Round(quantity, 2)) : quantity;
                        value.actualqty = quantity;
                        checkAllItemRefunded = Math.Abs(value.Quantity);
                        value.ActionType = ActionType.Refund;
                        value.InvoiceId = 0;
                        value.Id = 0;

                        if (checkAllItemRefunded > 0)
                        {
                            isAlreadyRefunded = false;
                        }
                        InvoiceCalculations.CalculateLineItemTotal(value, refundInvoice);
                        refundLineitems.Add(value);
                    }
                }
                else if (value.InvoiceItemType == InvoiceItemType.Discount && value.EffectiveAmount < 0)
                {
                    refundLineitems.Add(value);
                }
            }

            refundInvoice.InvoiceLineItems = refundLineitems;


            if (!isAlreadyRefunded && !isAlreadyExchanged)
            {
                //var refundedCount1 = AllLocalSaleItems.Count(x => x.Status == InvoiceStatus.Refunded && x.ReferenceInvoiceId == refundInvoice.Id);
                //if (refundedCount1 > 0)
                //    refundInvoice.Number += "-RF" + refundedCount1;
                //else
                //refundInvoice.Number += "-RF";

                // refundInvoice.InvoiceRefundPayments = new ObservableCollection<InvoicePaymentDto>(Invoice.InvoicePayments.Where(x => !x.IsDeleted));


               // var mainpage = (MainPage)_navigationService.MainPage;
                await detailpage.ViewModel.Close();
                //mainpage.ChangePage("EnterSalePage");
                 Shell.Current.CurrentItem = Shell.Current.Items[0];
                await ((BaseContentPage<EnterSaleViewModel>)_navigationService.CurrentPage).ViewModel.invoicemodel.RefundSaleFromHistory(refundInvoice);
            }
            else
            {
                if (isAlreadyRefunded)
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ThisSaleAlreadyRefunded"));
                if (isAlreadyExchanged)
                    App.Instance.Hud.DisplayToast("This sale already exchanged");
                return;
            }

        }

        public void FilterByOutlet(OutletDto_POS selectedOutlet)
        {
            try
            {
                //if (Settings.SelectedOutletId == selectedOutlet.Id)
                //    SelectedOutletId = 0;
                //else

                if (App.Instance.IsInternetConnected)
                {
                    SelectedOutletId = selectedOutlet.Id;
                    SelectedOutletForFilter = selectedOutlet.Title;
                    //Start #77147 iPad: Filter fetching all data by pratik
                    Filter(SearchText);
                    //End #77147 pratik
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);

                }

                IsOpenFilterOutletPopUp = 0;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
        public void OpenUpdateCustomerName(InvoiceDto invoice)
        {
            try
            {
                if(SelectedInvoiceToUpdateCustomer != null)
                    SelectedInvoiceToUpdateCustomer.IsToUpdateCustomerName = false;
                invoice.IsToUpdateCustomerName = true;
                invoice.UpdatedCustomerName = invoice.CustomerName;
                SelectedInvoiceToUpdateCustomer = invoice;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        public async void ParkedListViewSelection(InvoiceDto invoice)
        {
            if (invoice != null)
            {
                await OpenInvoiceDetailView(invoice);
            }
            if (SelectedInvoiceToUpdateCustomer != null)
                CloseUpdateCustomerName(SelectedInvoiceToUpdateCustomer);
            //Ticket end:#45648 .by rupesh
        }

        public async void UpdateCustomerName(InvoiceDto invoice)
        {
            invoice.IsToUpdateCustomerName = false;
            try
            {
                if (!string.IsNullOrEmpty(invoice.UpdatedCustomerName?.Trim()))
                {
                    invoice.CustomerName = invoice.UpdatedCustomerName.Trim();
                    await saleService.UpdateCustomerName(Fusillade.Priority.UserInitiated, true, invoice);
                }
                invoice.UpdatedCustomerName = "";
            }
            catch (Exception)
            {

            }

        }
        public void CloseUpdateCustomerName(InvoiceDto invoice)
        {
            invoice.IsToUpdateCustomerName = false;
            invoice.UpdatedCustomerName = "";

        }
        //Ticket end:#45648 .by rupesh
        CancellationTokenSource cancellation = new CancellationTokenSource();
        public ICommand FilterOutletHandleClickeCommand => new Command(filterOutletHandle_Clicked);
        public ICommand IncludeDiscardedSaleViewHandleClickedCommand => new Command(includeDiscardedSaleViewHandle_Clicked);
        public ICommand CloseIncludeDiscardedSaleViewHandleClickedCommand => new Command(closeIncludeDiscardedSaleViewHandle_Clicked);
        public ICommand IncludeDiscardSaledClickedCommand => new Command(includeDiscardSaled_Clicked);
        public ICommand UpdateCustomerUnfocusedCommand => new Command<Entry>(updateCustomer_Unfocused);
        public ICommand SearchHandleTextChangedCommand => new Command<Entry>(searchHandle_TextChanged);
        public ICommand CloseSearchSaleHandleClickedCommand => new Command(closeSearchSaleHandle_Clicked);
        public ICommand SearchSaleHandleClickedCommand => new Command(searchSaleHandle_Clicked);
        public ICommand FilterOptionHandleClickedCommand => new Command(filterOptionHandle_Clicked);
        public ICommand FilterDateRangeHandleClickedCommand => new Command(filterDateRangeHandle_Clicked);
        public ICommand SliderMenuHandleClickedCommand => new Command(sliderMenuHandle_Clicked);
        public ICommand OverlayHandleTappedCommand => new Command(overlayHandle_Tapped);

        //Start ticket #76208 IOS:FR:Terms of payments by Pratik
        public ICommand OverdueClickedCommand => new Command(Overdue_Clicked);
        private void Overdue_Clicked()
        {
            IsOverdue = !IsOverdue;
            _= Filter();
        }
        //End ticket #76208  by Pratik

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

        private void filterDateRangeHandle_Clicked()
        {
            if (IsOpenFilterDateRangePopUp == 0)
            {
                IsOpenFilterDateRangePopUp = 1;
            }
            else
            {
                IsOpenFilterDateRangePopUp = 0;
            }
        }

        private void filterOptionHandle_Clicked()
        {
            if (IsOpenFilterOptionPopUp == 0)
            {
                IsOpenFilterOptionPopUp = 1;
            }
            else
            {
                IsOpenFilterOptionPopUp = 0;
            }
        }

        private void overlayHandle_Tapped()
        {
            IsOpenFilterOptionPopUp = 0;
            IsOpenFilterDateRangePopUp = 0;
            IsOpenFilterOutletPopUp = 0;

        }


        private void searchSaleHandle_Clicked()
        {
            if (IsOpenSearchPopUp == false)
            {
                IsOpenSearchPopUp = true;
                IsOpenIncludeDiscardPopUp = false;
                if (_navigationService.NavigatedPage is ParkSalePagePhone parkPhoneSalePage)
                {
                    parkPhoneSalePage._SearchSaleEntry.Focus();

                }
                else if(_navigationService.NavigatedPage is ParkSalePage parkSalePage)
                {
                    parkSalePage._SearchSaleEntry.Focus();
                }
            }
        }

       private void closeSearchSaleHandle_Clicked()
        {
            if (IsOpenSearchPopUp == true)
            {
                if (_navigationService.NavigatedPage is ParkSalePagePhone parkPhoneSalePage)
                {
                    SearchSaleEntry_Unfocused(parkPhoneSalePage._SearchSaleEntry, null);

                }
                else if(_navigationService.NavigatedPage is ParkSalePage parkSalePage)
                {
                    SearchSaleEntry_Unfocused(parkSalePage._SearchSaleEntry, null);
                }

            }
        }

        public void SearchSaleEntry_Unfocused(object sender, FocusEventArgs e)
        {
            if (IsOpenSearchPopUp == true)
            {
                IsOpenSearchPopUp = false;
                SearchText = ((Entry)sender).Text;
            }
        }

        private void searchHandle_TextChanged(Entry sender)
        {
            var searchvalue = ((Entry)sender).Text;
            //if (!string.IsNullOrEmpty(searchvalue))
            //{
            stop();
            start(searchvalue);
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(searchvalue))
            //    {
            //        ViewModel.Filter(searchvalue, false);
            //    }
            //}
        }

        private void start(string textvalue)
        {
            var timespan = TimeSpan.FromSeconds(1.5);
            CancellationTokenSource cts = this.cancellation; // safe copy
            ContentPage parkSalePage;
            if (_navigationService.NavigatedPage is ParkSalePagePhone)
                 parkSalePage = (ParkSalePagePhone)_navigationService.NavigatedPage;
            else
                 parkSalePage = (ParkSalePage)_navigationService.NavigatedPage;

            parkSalePage.Dispatcher.StartTimer(timespan,
                () =>
                {
                    if (cts.IsCancellationRequested)
                        return false;
                    try
                    {
                        _= Filter(textvalue, false);
                        stop();
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                    return false; // or true for periodic behavior
                });
        }


        private void stop()
        {
            Interlocked.Exchange(ref cancellation, new CancellationTokenSource()).Cancel();
        }

        //Added to filter outlet by rupesh
        private void filterOutletHandle_Clicked()
        {
            if (IsOpenFilterOutletPopUp == 0)
            {
                IsOpenFilterOutletPopUp = 1;
            }
            else
            {
                IsOpenFilterOutletPopUp = 0;
            }
        }

        //Ticket start:#30840 Hide Discarded Sales/Void in Sales History.by rupesh
        private void includeDiscardedSaleViewHandle_Clicked()
        {
            if (IsOpenIncludeDiscardPopUp == false)
            {
                IsOpenIncludeDiscardPopUp = true;
                IsOpenSearchPopUp=false;
            }
        }

        private void closeIncludeDiscardedSaleViewHandle_Clicked()
        {
            if (IsOpenIncludeDiscardPopUp == true)
            {
                IsOpenIncludeDiscardPopUp = false;
            }
        }

        private void includeDiscardSaled_Clicked()
        {
            IsIncludeDiscard = !IsIncludeDiscard;
            //Start #77147 iPad: Filter fetching all data by pratik
            _= Filter(SearchText);
            //End #77147 by pratik
        }
        //Ticket end:#30840 .by rupesh
        //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
        private void updateCustomer_Unfocused(Entry sender)
        {
            try
            {
                var invoice = (InvoiceDto)((Entry)sender).BindingContext;
                invoice.IsToUpdateCustomerName = false;

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        //Ticket end:#45648 .by rupesh


    }


}
