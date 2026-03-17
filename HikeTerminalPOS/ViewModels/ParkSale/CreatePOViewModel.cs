using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HikePOS.Models;
using HikePOS.Services;
using System.Linq;
using System.Reactive.Linq;

namespace HikePOS.ViewModels
{
    public class CreatePOViewModel : BaseViewModel
    {
        ApiService<IInventoriesApi> inventoryApiService = new ApiService<IInventoriesApi>();
        InventoriesServices inventoryService;

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        SaleServices saleService;

        InvoiceDto _Invoice { get; set; }
        public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }

        ObservableCollection<PurchaseOrderDto> _purchaseOrderes { get; set; }
        public ObservableCollection<PurchaseOrderDto> PurchaseOrderes
        {
            get
            {
                return _purchaseOrderes;
            }
            set
            {
                _purchaseOrderes = value;
                SetPropertyChanged(nameof(PurchaseOrderes));
            }
        }

        public ICommand CreatePOCommand { get; }

        //Ticket Start:#11847 Feature Request - Cannot link backorders to existing PO by rupesh
        public ICommand CreateNewPOCommand { get; }
        public ICommand AddToExistingPOCommand { get; }
        ObservableCollection<PurchaseOrderListDto> _pendingOrderList { get; set; }
        public ObservableCollection<PurchaseOrderListDto> PendingOrderList
        {
            get
            {
                return _pendingOrderList;
            }
            set
            {
                _pendingOrderList = value;
                SetPropertyChanged(nameof(PendingOrderList));
            }
        }
        PurchaseOrderListDto _selectedPendingOrder { get; set; }
        public PurchaseOrderListDto SelectedPendingOrder
        {
            get
            {
                return _selectedPendingOrder;
            }
            set
            {
                _selectedPendingOrder = value;
                SetPropertyChanged(nameof(SelectedPendingOrder));
            }
        }
        bool _isNotPendingOrder { get; set; }
        public bool IsNotPendingOrder
        {
            get
            {
                return _isNotPendingOrder;
            }
            set
            {
                _isNotPendingOrder = value;
                SetPropertyChanged(nameof(IsNotPendingOrder));
            }
        }

        public CreateNewOrAddToExistingPO CreateNewOrAddPOpage;
        private PurchaseOrderDto PurchaseOrder = null;
        //Ticket End
        public CreatePOViewModel()
        {
            CreatePOCommand = new Command<PurchaseOrderDto>(CreatePO);
            inventoryService = new InventoriesServices(inventoryApiService);
            saleService = new SaleServices(saleApiService);
            //Ticket Start:#11847 Feature Request - Cannot link backorders to existing PO by rupesh
            CreateNewPOCommand = new Command(CreateNewPO);
            AddToExistingPOCommand = new Command(AddToExistingPO);
            //Ticket End

        }

        public async void CreatePO(PurchaseOrderDto purchaseOrder)
        {
            try
            {
                //Ticket Start:#11847 Feature Request - Cannot link backorders to existing PO by rupesh
                using (new Busy(this, true))
                {
                    if (CreateNewOrAddPOpage == null)
                    {
                        CreateNewOrAddPOpage = new CreateNewOrAddToExistingPO();
                    }
                    CreateNewOrAddPOpage.BindingContext = this;
                    PurchaseOrder = purchaseOrder;
                    PendingOrderList = null;
                    IsNotPendingOrder = false;
                    SelectedPendingOrder = null;
                    await NavigationService.PushModalAsync(CreateNewOrAddPOpage);
                     //Ticket start:#84288 iOS: FR:Backorder Management.by rupesh
                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    {
                        PendingOrderList = await inventoryService.GetPendingOrdersListBySupplier(Fusillade.Priority.UserInitiated, PurchaseOrder.OutletId, PurchaseOrder.SupplierId);
                        if (PendingOrderList == null || !PendingOrderList.Any())
                            IsNotPendingOrder = true;

                    }
                     //Ticket end:#84288 .by rupesh


                }
                //Ticket End

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //Ticket Start:#11847 Feature Request - Cannot link backorders to existing PO by rupesh

        public async void CreateNewPO()
        {
            try
            {

                using (new Busy(this, true))
                {

                    if (PurchaseOrder != null)
                    {
                        using (new Busy(this, true))
                        {
                            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                            {
                                //Ticket start:#84288 iOS: FR:Backorder Management.by rupesh
                                POReferenceDto poReference = await inventoryService.GetPOReferenceSettting(Fusillade.Priority.UserInitiated, PurchaseOrder.OutletId);
                                if(poReference != null)
                                {
                                if (poReference.sequenceNumber == 0)
                                   poReference.sequenceNumber = 1;

                                if (poReference.prefix != null && poReference.suffix != null)
                                    PurchaseOrder.RefNumber = poReference.prefix + poReference.sequenceNumber.ToString() + poReference.suffix;
                                else if (poReference.prefix != null && poReference.suffix == null)
                                    PurchaseOrder.RefNumber = poReference.prefix + poReference.sequenceNumber.ToString();
                                else if (poReference.prefix == null && poReference.suffix != null)
                                    PurchaseOrder.RefNumber =  poReference.sequenceNumber.ToString() + poReference.suffix;
                                else
                                    PurchaseOrder.RefNumber =  poReference.sequenceNumber.ToString();


                                PurchaseOrder.PONumber = "Order - " + DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss");
                                PurchaseOrder.PODate = DateTime.Now;
                                PurchaseOrder.DueDate = DateTime.Now;
                                PurchaseOrder.Id = 0;

                                PurchaseOrderDto objPurchaseOrder = await inventoryService.CreateorUpdatePO(Fusillade.Priority.UserInitiated, true, PurchaseOrder);
                                if (objPurchaseOrder != null)
                                {
                                    var purchaseOrderLineItem = objPurchaseOrder.PurchaseOrderLineItems.First();
                                    purchaseOrderLineItem.POCreatedNumber = objPurchaseOrder.RefNumber;
                                    purchaseOrderLineItem.POLineStatus = objPurchaseOrder.Status;
                                    PurchaseOrder.Id = objPurchaseOrder.Id;
                                    PurchaseOrder.PurchaseOrderLineItems = objPurchaseOrder.PurchaseOrderLineItems;
                                    Invoice.CreatedPoOrNot = true;
                                    Invoice = await saleService.UpdateLocalInvoice(Invoice);

                                    await CreateNewOrAddPOpage.Close();

                                }
                                }
                                //Ticket start:#84288.by rupesh

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

        public async void AddToExistingPO()
        {
            try
            {
                if (SelectedPendingOrder == null)
                    return;

                using (new Busy(this, true))
                {
                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    {
                        bool result = await inventoryService.AssignBackorderToPO(Fusillade.Priority.UserInitiated, SelectedPendingOrder.Id, Invoice.Id, PurchaseOrder);
                        if (result)
                        {
                           //Ticket start:#84288 iOS: FR:Backorder Management.by rupesh
                            var objPurchaseOrder = PurchaseOrder;
                            objPurchaseOrder.RefNumber = SelectedPendingOrder.RefNumber;
                             var purchaseOrderLineItem = PurchaseOrder.PurchaseOrderLineItems.First();
                            purchaseOrderLineItem.POCreatedNumber = SelectedPendingOrder.RefNumber;
                            purchaseOrderLineItem.POLineStatus = SelectedPendingOrder.Status;
                            purchaseOrderLineItem.Id = SelectedPendingOrder.Id;
                           //Ticket end:#84288 .by rupesh

                            Invoice.CreatedPoOrNot = true;
                            Invoice = await saleService.UpdateLocalInvoice(Invoice);

                            await CreateNewOrAddPOpage.Close();

                        }

                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }


            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        //Ticket End
        public ICommand CloseHandleClickedCommand => new Command(closeHandle_Clicked);
        private async void closeHandle_Clicked()
        {
            await close();
        }

        public async Task close()
        {
            try
            {
                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                    await NavigationService.PopModalAsync();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

    }
}
