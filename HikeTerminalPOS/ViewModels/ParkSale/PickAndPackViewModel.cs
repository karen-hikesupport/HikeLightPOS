using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.Helpers;
using CommunityToolkit.Mvvm.Messaging;
using HikePOS.Enums;
using RestSharp.Extensions;

namespace HikePOS.ViewModels
{
	public class PickAndPackViewModel : BaseViewModel
	{
		public ICommand ClickedMenuCommand { get; set; }
		public ICommand ClickedSaveCommand { get; set; }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public event EventHandler<InvoiceDto> InoviceChanged;
        //End #84293 by Pratik

        ObservableCollection<InvoiceLineItemDto> _InvoiceLineItems { get; set; }
		public ObservableCollection<InvoiceLineItemDto> InvoiceLineItems
		{
			get
			{
				return _InvoiceLineItems;
			}
			set
			{
				_InvoiceLineItems = value;
				SetPropertyChanged(nameof(InvoiceLineItems));
			}
		}
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
			}
		}

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public ObservableCollection<ProductAvailableStocks> productAvailableStocks { get; set; }
        public bool IsChanged { get; set; }
        public ObservableCollection<InvoiceLineItemDto> CopyInvoiceLineItems { get; set; }
        ApiService<IOutletApi> outletApiService = new ApiService<IOutletApi>();
        OutletServices outletService;

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        SaleServices saleService;

        ApiService<IProductApi> ProductApiService = new ApiService<IProductApi>();
        ProductServices productService;

        bool _isAllowFulfilment;
        public bool IsAllowFulfilment
        {
            get
            {
                return _isAllowFulfilment;
            }
            set
            {
                _isAllowFulfilment = value;
                SetPropertyChanged(nameof(IsAllowFulfilment));
            }
        }

        ObservableCollection<OutletDto_POS> _outlets;
        public ObservableCollection<OutletDto_POS> Outlets
        {
            get
            {
                return _outlets;
            }
            set
            {
                _outlets = value;
                SetPropertyChanged(nameof(Outlets));
            }
        }

        OutletDto_POS Prev_SelectedOutlet;
        OutletDto_POS _selectedOutlet;
        public OutletDto_POS SelectedOutlet
        {
            get
            {
                return _selectedOutlet;
            }
            set
            {
                if (_selectedOutlet != null && value != null && value.Id != _selectedOutlet.Id && _selectedOutlet == Prev_SelectedOutlet)
                {
                    _selectedOutlet = value;
                    SetPropertyChanged(nameof(SelectedOutlet));
                    var copyval = Prev_SelectedOutlet.Copy();
                    MainThread.BeginInvokeOnMainThread(async () => { 
                        var result = await App.Alert.ShowAlert("Change outlet", "Click continue to change fulfilment outlet for this sale", "Continue", "Cancel");
                        if (result)
                        {
                            await GetProductAvailableStocks();
                            Prev_SelectedOutlet = SelectedOutlet;
                        }
                        else
                        {
                            SelectedOutlet = copyval;
                        }
                    });
                }
                else
                {
                    _selectedOutlet = value;
                    SetPropertyChanged(nameof(SelectedOutlet));
                    Prev_SelectedOutlet = SelectedOutlet;
                }
                PickAndPackPage?.SelelectedPicker(); 
            }
        }
       // public int SelectedOutletIndex => (Outlets == null || SelectedOutlet == null) ? -1 : Outlets.IndexOf(SelectedOutlet);
        //End #84293 by Pratik

        bool _IsSaveEnabled { get; set; }
		public bool IsSaveEnabled
		{
			get
			{
				return _IsSaveEnabled;
			}
			set
			{
				_IsSaveEnabled = value;
				SetPropertyChanged(nameof(IsSaveEnabled));
			}
		}
		ReceiptTemplateDto _currentReceiptTemplate { get; set; }
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
		ShopDto _GeneralShopDto { get; set; }
		public ShopDto GeneralShopDto { get { return _GeneralShopDto; } set { _GeneralShopDto = value; SetPropertyChanged(nameof(GeneralShopDto)); } }
		OutletDto_POS _currentOutlat { get; set; }
		public OutletDto_POS CurrentOutlet
		{
			get { return _currentOutlat; }
			set
			{
				_currentOutlat = value;
				SetPropertyChanged(nameof(CurrentOutlet));
			}
		}

		RegisterDto _currentRegister { get; set; }
		public RegisterDto CurrentRegister
		{
			get { return _currentRegister; }
			set
			{
				_currentRegister = value;
				SetPropertyChanged(nameof(CurrentRegister));
			}
		}

        bool _isEnabledOutlets = true;
		public bool IsEnabledOutlets
		{
			get { return _isEnabledOutlets; }
			set
			{
				_isEnabledOutlets = value;
				SetPropertyChanged(nameof(IsEnabledOutlets));
			}
		}

		private double pickAndPackPrintItemListHeight;
		public double PickAndPackPrintItemListHeight
		{
			get { return pickAndPackPrintItemListHeight; }
			set
			{
				pickAndPackPrintItemListHeight = value;
				SetPropertyChanged(nameof(PickAndPackPrintItemListHeight));
			}
		}

        ObservableCollection<InvoiceItemFulfillmentDisplay> _fulfillmentInvoiceLineItems;
        public ObservableCollection<InvoiceItemFulfillmentDisplay> FulfillmentInvoiceLineItems { get { return _fulfillmentInvoiceLineItems; } set { _fulfillmentInvoiceLineItems = value; SetPropertyChanged(nameof(FulfillmentInvoiceLineItems)); } }


		public PickAndPackPage PickAndPackPage;
		ImageSource _BarcodeImage { get; set; }// = "barCode.JPG";
		public ImageSource BarcodeImage { get { return _BarcodeImage; } set { _BarcodeImage = value; SetPropertyChanged(nameof(BarcodeImage)); } }

		//Ticket start:#39934 iPad: Feature request - How to meet with ZATCA requirement.by rupesh
		ImageSource _QRCodeImage { get; set; }// = "qrCodeImage.JPG";
		public ImageSource QRCodeImage { get { return _QRCodeImage; } set { _QRCodeImage = value; SetPropertyChanged(nameof(QRCodeImage)); } }
		//Ticket end:#39934 .by rupesh
		SubscriptionDto _subscription { get; set; }
		public SubscriptionDto Subscription { get { return _subscription; } set { _subscription = value; SetPropertyChanged(nameof(Subscription)); } }

        DateTime _OrderDate { get; set; }
        public DateTime OrderDate { get { return _OrderDate; } set { _OrderDate = value; SetPropertyChanged(nameof(OrderDate)); } }
        DateTime _IssueDate { get; set; }
        public DateTime IssueDate { get { return _IssueDate; } set { _IssueDate = value; SetPropertyChanged(nameof(IssueDate)); } }
        //Ticket start:#71299 iPad - Feature: Adding the stock items manually in Pick and Pack.by rupesh
        public ICommand OpenUpdateQuantityViewCommand { get; }
        //public ICommand UpdateQuantityCommand { get; }
        public ICommand CloseQuantityViewCommand { get; }
        public InvoiceLineItemDto SelectedLineItemToUpdateQuantity = null;
        public decimal LastPickAndPackQuantity = 0;
        //Ticket end:#71299 .by rupesh

        public PickAndPackViewModel()
        {
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            outletService = new OutletServices(outletApiService);
            productService = new ProductServices(ProductApiService);
            saleService = new SaleServices(saleApiService);
            ClickedSaveCommand = new Command(async () => { await SaveFulfilment(); });
            //End #84293 by Pratik

            //Ticket start:#71299 iPad - Feature: Adding the stock items manually in Pick and Pack.by rupesh
            OpenUpdateQuantityViewCommand = new Command<Entry>(OpenUpdateQuantity);
            //UpdateQuantityCommand = new Command<InvoiceLineItemDto>(UpdateQuantity);
            CloseQuantityViewCommand = new Command<InvoiceLineItemDto>(CloseQuantityView);
            //Ticket end:#71299 .by rupesh

          
        }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        private async Task SaveFulfilment()
        {
            try
            {
                if(DeviceInfo.Platform == DevicePlatform.Android)
                    IsEnabledOutlets = false;
                if (!IsAllowFulfilment)
                {
                    OrderDate = Invoice.CreationTime.ToStoreTime();
                    IssueDate = Extensions.moment().ToStoreTime();
                }
                if (Invoice.InvoiceFulfillments == null)
                {
                    Invoice.InvoiceFulfillments = new ObservableCollection<InvoiceFulfillmentDto>();
                }
                foreach (var item in Invoice.InvoiceLineItems)
                {
                    if ((item.PickAndPackQuantity + item.fulfillmentQuantity) > item.Quantity)
                    {
                        item.PickAndPackQuantity = item.Quantity - Convert.ToDecimal(item.DisplayfulfillmentQuantity ?? "0");
                    }
                    if (item.PickAndPackQuantity > Convert.ToDecimal(item.AvailableQuantity))
                    {
                        item.PickAndPackQuantity = Convert.ToDecimal(item.AvailableQuantity);
                    }
                    if (item.InvoiceItemFulfillments == null)
                    {
                        item.InvoiceItemFulfillments = new ObservableCollection<InvoiceItemFulfillmentDto>();
                    }
                }
                Invoice.InvoiceFulfillCount = Invoice.InvoiceFulfillments.Count + 1;
                var firstdata = Invoice.InvoiceFulfillments.FirstOrDefault(a => a.OutletId == SelectedOutlet.Id && a.Id == 0);
                if (firstdata == null)
                {
                    Invoice.InvoiceFulfillments.Add(new InvoiceFulfillmentDto
                    {
                        InvoiceId = Invoice.Id,
                        OutletId = SelectedOutlet.Id,
                        CarrierName = string.Empty
                    });
                }
 
                if (Invoice.CustomerId == null)
                    Invoice.CustomerDetail = null;

                Invoice.InvoiceTempId = nameof(InvoiceDto) + "_" + Guid.NewGuid().ToString();

                using (new Busy(this, true))
                {
                    var result = await saleService.CreateOrUpdateInvoicefulfillment(Fusillade.Priority.UserInitiated, true, Invoice);
                    if (result != null)
                    {
                        IsChanged = true;
                        result.InvoiceHistories = new ObservableCollection<InvoiceHistoryDto>(result.InvoiceHistories?.OrderBy(x=>x.CreationTime));
                        Invoice = result;
                        InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Custom && x.InvoiceItemType != InvoiceItemType.Discount));
                        if (productAvailableStocks != null && productAvailableStocks.Count > 0)
                        {
                            foreach (var item in productAvailableStocks)
                            {
                                var item1 = InvoiceLineItems.FirstOrDefault(a => a.Id == item.invoiceItemId);
                                if (item1 != null)
                                {
                                    item1.PickAndPackQuantity = 0;
                                    item1.AvailableQuantity = (item.stock - Convert.ToDecimal(item1.DisplayfulfillmentQuantity)).ToString("0.##");
                                }
                            }
                        }
                        CopyInvoiceLineItems = InvoiceLineItems.Copy();

                        if (Invoice.InvoiceFulfillments != null && Invoice.InvoiceFulfillments.Count > 0)
                        {
                            Invoice.InvoiceFulfillments = new ObservableCollection<InvoiceFulfillmentDto>(Invoice.InvoiceFulfillments.OrderBy(a => a.Id));
                            foreach (var item in Invoice.InvoiceFulfillments)
                            {
                                if (item.InvoiceItemFulfillments != null && item.InvoiceItemFulfillments.Count > 0)
                                {
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
                        if (Invoice.InvoiceFulfillments != null && Invoice.InvoiceFulfillments.Count > 0)
                        {

                            FulfillmentInvoiceLineItems = Invoice.InvoiceFulfillments.Last().InvoiceLineItems;

                            IssueDate = Invoice.InvoiceFulfillments.Last().CreationTime.ToStoreTime();
                        }

                        IsSaveEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            finally
            {
                if(DeviceInfo.Platform == DevicePlatform.Android)
                    IsEnabledOutlets = true;
            }
        }
        //End #84293 by Pratik

        public override async void OnAppearing()
		{
			base.OnAppearing();
			try
			{
                //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                SelectedOutlet = Settings.SelectedOutlet;
                IsAllowFulfilment = false;
                if (Settings.StoreGeneralRule.AllowPartialFulfilled)
                {
                    IsAllowFulfilment = true;
                    LoadOutletsData();
                }
                //end #84293 .by Pratik

                //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                await GetProductAvailableStocks();
                //End #84293 by Pratik

                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.BarcodeMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.BarcodeMessenger>(this, (sender, arg) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (arg?.Value == null)
                            {
                                App.Instance.Hud.DisplayToast("Please try again", Colors.Red, Colors.White);
                                return;
                            }
                            var lineItem = InvoiceLineItems.FirstOrDefault(x => x.Barcode.ToLower() == arg.Value.ToLower());
                            var lineItemToUpdate = InvoiceLineItems.FirstOrDefault(x => x.Barcode.ToLower() == arg.Value.ToLower() && (x.PickAndPackQuantity + Convert.ToDecimal(x.DisplayfulfillmentQuantity)) < x.Quantity);
                            if (lineItemToUpdate != null)
                            {
                                    lineItemToUpdate.PickAndPackQuantity += 1;
                                    var count = InvoiceLineItems.Count(x => x.Quantity == x.PickAndPackQuantity);
                                    //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                                    if (!IsAllowFulfilment)
                                    {
                                        if (count == InvoiceLineItems.Count)
                                        {
                                            IsSaveEnabled = true;
                                        }
                                    }
                                    else
                                    {
                                        var oldqnt = CopyInvoiceLineItems.Sum(a => a.PickAndPackQuantity);
                                        var newqnt = InvoiceLineItems.Sum(a => a.PickAndPackQuantity);
                                        if (oldqnt != newqnt)
                                            IsSaveEnabled = true;
                                        else
                                            IsSaveEnabled = false;
                                    }
                                    //End #84293 by Pratik
                            }
                            else if(lineItem != null && (lineItem.PickAndPackQuantity + Convert.ToDecimal(lineItem.DisplayfulfillmentQuantity)) >= lineItem.Quantity)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ScanQtyMoreThanQty"), Colors.Red, Colors.White);
                            }
                            else
                            {
                                App.Instance.Hud.DisplayToast("No product found", Colors.Red, Colors.White);
                            }
                        });
                    });
                }

			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}
		public void CalculatePickAndPackPrintHeight(string printerType = "")
		{
			var tmp = Invoice.InvoiceLineItems;
			//Ticket end:#33854 .by rupesh
			double itemHeight = 0;


			foreach (var item in tmp)
			{
				//Ticket start:#46369 iPad: FR - SKU showing in Gift and delivery docket Print receipt.by rupesh
				var title = item.ProductTitleWithSku;
				//Ticket end:#46369 .by rupesh
				if (printerType.Contains("POP"))
					itemHeight += 22 * ((title.Length / 27) + 1) + 1;
				else
					itemHeight += 22 * ((title.Length / 45) + 1) + 1;

				//if (item.ProductTitleWithQuantity.Length > 60)
				//    itemHeight += 15 + item.ProductTitleWithQuantity.Length;
				//else if (item.ProductTitleWithQuantity.Length > 40)
				//    itemHeight += 25 + item.ProductTitleWithQuantity.Length;
				//else
				//    itemHeight += 60;
			}
			PickAndPackPrintItemListHeight = itemHeight ;

		}

		public override void OnDisappearing()
		{
			base.OnDisappearing();
			try
			{
                //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                IsChanged = false;
                //End #84293 by Pratik
                WeakReferenceMessenger.Default.Unregister<Messenger.BarcodeMessenger>(this);

			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}
        public ICommand CloseHandleClickedCommand => new Command(closeHandle_Clicked);
        private async void closeHandle_Clicked()
        {
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            if (!IsChanged)
            {
                var result = true;
                //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                if (InvoiceLineItems.Any(x => x.PickAndPackQuantity > 0) && !IsAllowFulfilment)
                {
                    //End #84293 by Pratik
                    result = await App.Alert.ShowAlert("Alert", "Your selection will not be saved. Are you sure you want to close this page without scanning all the remaining items?", "Ok", "Cancel");
                }
                if (result)
                {

                    await close();
                }
            }
            else
            {
                await close();
                InoviceChanged?.Invoke(null, Invoice);
            }
            //End #84293 by Pratik
        }
        public async Task close()
        {
            try
            {
                InvoiceLineItems.ForEach(x => {x.PickAndPackQuantity = 0; x.IsToUpdatePickAndPack = false;  });
                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                    await NavigationService.PopModalAsync();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        //Ticket start:#71299 iPad - Feature: Adding the stock items manually in Pick and Pack.by rupesh
        public void OpenUpdateQuantity(Entry customEntry)
        {
            customEntry.Focus();
            var invoiceLineItemDto = (InvoiceLineItemDto)customEntry.BindingContext;
            if (SelectedLineItemToUpdateQuantity != null)
                SelectedLineItemToUpdateQuantity.IsToUpdatePickAndPack = false;


            invoiceLineItemDto.IsToUpdatePickAndPack = true;
            SelectedLineItemToUpdateQuantity = invoiceLineItemDto;
            LastPickAndPackQuantity = invoiceLineItemDto.PickAndPackQuantity;
            invoiceLineItemDto.IsEdited = false;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                SetData(invoiceLineItemDto);
            }
        }
        public void UpdateQuantity(Entry customEntry)
        {
            var invoiceLineItemDto = (InvoiceLineItemDto)customEntry.BindingContext;
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            if (invoiceLineItemDto == null)
                return;
           
            invoiceLineItemDto.IsToUpdatePickAndPack = false;
            if (invoiceLineItemDto.PickAndPackQuantity < 0)
                invoiceLineItemDto.PickAndPackQuantity = 0;
            if ((invoiceLineItemDto.PickAndPackQuantity + Convert.ToDecimal(invoiceLineItemDto.DisplayfulfillmentQuantity)) > invoiceLineItemDto.Quantity)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ScanQtyMoreThanQty"), Colors.Red, Colors.White);
                invoiceLineItemDto.PickAndPackQuantity = invoiceLineItemDto.Quantity - Convert.ToDecimal(invoiceLineItemDto.DisplayfulfillmentQuantity);
            }
            if (invoiceLineItemDto.PickAndPackQuantity > Convert.ToDecimal(invoiceLineItemDto.AvailableQuantity))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NotEnoughQuantity"), Colors.Red, Colors.White);
                invoiceLineItemDto.PickAndPackQuantity = Convert.ToDecimal(invoiceLineItemDto.AvailableQuantity);
            }
            if (!IsAllowFulfilment)
            {
                var count = InvoiceLineItems.Count(x => x.Quantity == (x.PickAndPackQuantity + Convert.ToDecimal(x.DisplayfulfillmentQuantity)));
                if (count == InvoiceLineItems.Count)
                    IsSaveEnabled = true;
                else
                    IsSaveEnabled = false;
            }
            else
            {
                var oldqnt = CopyInvoiceLineItems.Sum(a => a.PickAndPackQuantity);
                var newqnt = InvoiceLineItems.Sum(a => a.PickAndPackQuantity);
                if(oldqnt != newqnt)
                    IsSaveEnabled = true;
                else
                    IsSaveEnabled = false;
            }
            //End #84293 by Pratik
            invoiceLineItemDto.IsEdited = true;

        }

        public void SetData(InvoiceLineItemDto invoiceLineItemDto)
        {
           foreach(var item in InvoiceLineItems.Where(a=>a.Id != invoiceLineItemDto.Id))
           {
                if(item.PickAndPackQuantity > 0)
                {
                    item.IsEdited = true;
                    item.IsToUpdatePickAndPack = false;
                }
           }
            var count = InvoiceLineItems.Count(x => x.Quantity == x.PickAndPackQuantity);
            if (count == InvoiceLineItems.Count)
                IsSaveEnabled = true;
            else
                IsSaveEnabled = false;
        }

        public void CheckQuantity()
        {
            var count = InvoiceLineItems.Count(x => x.Quantity == x.PickAndPackQuantity);
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            if (!IsAllowFulfilment)
            {
                if (count == InvoiceLineItems.Count)
                    IsSaveEnabled = true;
                else
                    IsSaveEnabled = false;
            }
            else
            {
                var oldqnt = CopyInvoiceLineItems.Sum(a => a.PickAndPackQuantity);
                var newqnt = InvoiceLineItems.Sum(a => a.PickAndPackQuantity);
                if (oldqnt != newqnt)
                    IsSaveEnabled = true;
                else
                    IsSaveEnabled = false;
            }
            //Start #84293 by Pratik
        }
        public void CloseQuantityView(InvoiceLineItemDto invoiceLineItemDto)
        {
            invoiceLineItemDto.IsToUpdatePickAndPack = false;
            invoiceLineItemDto.PickAndPackQuantity = LastPickAndPackQuantity;
        }
        //Ticket end:#71299 .by rupesh


        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public async void LoadOutletsData()
        {
            try
            {
                Outlets = new ObservableCollection<OutletDto_POS>();
                var res = outletService.GetLocalOutlets();
                if (res != null)
                {
                    var result = res.Where(x => x.IsActive == true).ToList();
                    if (result != null)
                        Outlets = new ObservableCollection<OutletDto_POS>(result);


                }
                else
                {
                    res = await outletService.GetRemoteOutlets(Fusillade.Priority.UserInitiated, false);
                    if (res != null)
                    {
                        var result = res.Where(x => x.IsActive == true).ToList();
                        if (result != null)
                            Outlets = new ObservableCollection<OutletDto_POS>(result);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            finally
            {
                if (Outlets.Any(a => a.Id == Settings.SelectedOutletId))
                {
                    SelectedOutlet = Outlets.First(a => a.Id == Settings.SelectedOutletId);
                }
            }
        }

        public async Task GetProductAvailableStocks()
        {
            try
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    using (new Busy(this, true))
                    {
                        productAvailableStocks = new ObservableCollection<ProductAvailableStocks>();
                        ProductAvailableStocksRequest stocksRequest = new ProductAvailableStocksRequest();
                        stocksRequest.invoiceId = Invoice.Id;
                        stocksRequest.outletId = SelectedOutlet.Id;
                        var res = await productService.GetProductAvailableStocks(Fusillade.Priority.UserInitiated, stocksRequest);
                        if (res != null && res.Count > 0)
                        {
                            productAvailableStocks = res;
                            foreach (var item in productAvailableStocks)
                            {
                                var item1 = InvoiceLineItems.FirstOrDefault(a => a.Id == item.invoiceItemId);
                                if (item1 != null)
                                {
                                    item1.PickAndPackQuantity = 0;
                                    item1.AvailableQuantity = item.stock.ToString("0.##");
                                }
                            }
                        }
                    }
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        //end #84293 .by Pratik

    }


}

	


