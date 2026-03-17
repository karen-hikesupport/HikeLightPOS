using System;
using System.Collections.ObjectModel;
using System.Linq;
using HikePOS.Models;
using System.Threading.Tasks;
using HikePOS.Services;
using Fusillade;
using HikePOS.Helpers;
using System.Diagnostics;
using System.Windows.Input;

namespace HikePOS.ViewModels
{

    public class SearchProductViewModel : BaseViewModel
    {

        public EventHandler<EnterSaleItemDto> ProductSelected;

        CancellationTokenSource cancellation;

        ObservableCollection<EnterSaleItemDto> _SearchProducts { get; set; }
        public ObservableCollection<EnterSaleItemDto> SearchProducts { get { return _SearchProducts; } set { _SearchProducts = value; SetPropertyChanged(nameof(SearchProducts)); } }

        public ObservableCollection<ProductDto_POS> AllProduct { get; set; }
        public ObservableCollection<OfferDto> AllOffers { get; set; }
        //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
        public ObservableCollection<ProductUnitOfMeasureDto> AllUnitOfMeasures { get; set; }
        //Ticket end:#20064 .by rupesh
         
        ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
        ProductServices productService;

        EnterSaleItemDto _selectedSearchProducts { get; set; }
        public EnterSaleItemDto SelectedSearchProducts { get { return _selectedSearchProducts; } set { _selectedSearchProducts = value; SetPropertyChanged(nameof(SelectedSearchProducts)); } }

        string _searchPrdouctKeyword { get; set; }
        public string SearchPrdouctKeyword { get { return _searchPrdouctKeyword; } set { _searchPrdouctKeyword = value; SetPropertyChanged(nameof(SearchPrdouctKeyword)); SearchTextTextChanged(); } }

        double _searchPrdouctHeight { get; set; }
        public double SearchPrdouctHeight { get { return _searchPrdouctHeight; } set { _searchPrdouctHeight = value; SetPropertyChanged(nameof(SearchPrdouctHeight)); } }

        public SearchProductViewModel()
        {
            Title = "Search";
            cancellation = new CancellationTokenSource();

            SearchProducts = new ObservableCollection<EnterSaleItemDto>(); 
            AllProduct = new ObservableCollection<ProductDto_POS>();
            productService = new ProductServices(productApiService);

            PropertyChanged += (sender, e) =>
            {
                try
                {
                    if (e.PropertyName == "SearchProducts")
                    {
                        if (SearchProducts == null)
                        {
                            SearchPrdouctHeight = 0;
                        }
                        else
                        {
                            if (SearchProducts.Count < 5)
                            {
                                SearchPrdouctHeight = SearchProducts.Count * 65;
                            }
                            else
                            {
                                SearchPrdouctHeight = 5 * 61;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }; 
        }

        //public override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    SearchProducts = new ObservableCollection<EnterSaleItemDto>();
        //}

        #region Command
        public ICommand CloseCommand => new Command(CloseTapped);
        public ICommand ProductSelectCommand => new Command(ProductSelectTapped);
        public ICommand TextChangedCommand => new Command(TextChangedTapped);
        #endregion

        #region Command Execution

        public void TextChangedTapped()
        {
            try
            {
                SearchTextTextChanged();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void CloseTapped()
        {
            try
            {
                SearchPrdouctKeyword = "";
                if (SearchProducts != null)
                {
                    SearchProducts.Clear();
                }
                SearchPrdouctHeight = 0;

                ClosePopupTapped();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task CloseAsyncTapped()
        {
            try
            {
                SearchPrdouctKeyword = "";
                if (SearchProducts != null)
                {
                    SearchProducts.Clear();
                }
                SearchPrdouctHeight = 0;

                await ClosePopupTapped_Task();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void ProductSelectTapped()
        {
            try
            {
                if (SelectedSearchProducts != null)
                {
                    ProductSelected?.Invoke(this, SelectedSearchProducts);
                }
            }
            catch (Exception ex)
            {

                ex.Track();
            }
        }

        #endregion

        void SearchTextTextChanged()
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchPrdouctKeyword))
                {
                    Stop();
                    Start(SearchPrdouctKeyword);
                }
                else
                {
                    SearchPrdouctHeight = 0;
                    SearchProducts.Clear();
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void Start(string textvalue)
        {
            try
            {
                var timespan = TimeSpan.FromSeconds(0.5);
                CancellationTokenSource cts = cancellation; // safe copy
                Dispatcher.GetForCurrentThread().StartTimer(timespan,
                    () =>
                    {
                        try
                        {
                            if (cts.IsCancellationRequested)
                                return false;
                            try
                            {
                                SearchProduct(textvalue);
                                Stop();
                            }
                            catch (Exception ex)
                            {
                                ex.Track();
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                        return false; // or true for periodic behavior
                    });
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void Stop()
        {
            try
            {
                Interlocked.Exchange(ref cancellation, new CancellationTokenSource()).Cancel();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void SearchProduct(string name_barcode_sku)
        {
           
            if (string.IsNullOrEmpty(name_barcode_sku))// || name_barcode_sku.Length < 3)
            {
                SearchProducts.Clear();
                return;
            }
            if (SearchProducts != null && SearchProducts.Count > 0 && name_barcode_sku.Length > 0 && name_barcode_sku.Length < 3)
            {
                return;
            }
            SearchProducts.Clear();
            if (name_barcode_sku.Length < 3)
            {
                return;
            }

            using (new Busy(this, true))
            {
                try
                {
                    //  Debug.WriteLine("all product : " + Newtonsoft.Json.JsonConvert.SerializeObject(AllProduct).ToString());
                    // var tmpsearchproducts = await productService.GetLocalSearchProduct(name_barcode_sku);

                    var tmpsearchproducts = productService.GetLocalSearchProductByFilter(name_barcode_sku);

                    //var tmpsearchproducts = AllProduct.Where(x => x.IsActive && (x.Name.ToLower().Contains(name_barcode_sku.ToLower())
                    //                                             || (!string.IsNullOrEmpty(x.Sku) && !string.IsNullOrWhiteSpace(x.Sku) && x.Sku.ToLower().Contains(name_barcode_sku.ToLower()))
                    //                                             || (!string.IsNullOrEmpty(x.BarCode) && !string.IsNullOrWhiteSpace(x.BarCode) && x.BarCode.ToLower().Contains(name_barcode_sku.ToLower()))
                    //                                             || (!string.IsNullOrEmpty(x.BranName) && !string.IsNullOrWhiteSpace(x.BranName) && x.BranName.ToLower().Contains(name_barcode_sku.ToLower())))
                    //                                          ).ToList();

                    var tmpsearchoffers = AllOffers.Where(x => x.IsActive && x.OfferType == Enums.OfferType.Composite && (x.Name.ToLower().Contains(name_barcode_sku.ToLower())
                                                                 || (!string.IsNullOrEmpty(x.Sku) && !string.IsNullOrWhiteSpace(x.Sku) && x.Sku.ToLower().Contains(name_barcode_sku.ToLower()))
                                                                 || (!string.IsNullOrEmpty(x.BarCode) && !string.IsNullOrWhiteSpace(x.BarCode) && x.BarCode.ToLower().Contains(name_barcode_sku.ToLower())))
                                                              );

                    //tmpsearchproducts.Where(x => x.ItemImage == null && x.ParentId != null && x.ParentId > 0).All(a =>
                    //{
                    //    var product = AllProduct.FirstOrDefault(p => p.Id == a.ParentId);
                    //    if (product != null)
                    //    {
                    //        a.ItemImage = product.ItemImage;
                    //    }
                    //    return true;
                    //});

                    if (tmpsearchproducts != null)
                    {
                        //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                        if (AllUnitOfMeasures != null)
                        {
                            //Ticket start:#26322 iPad - Composite product issue (filtered uom as well).by rupesh
                          //  var tmpAllUnitOfMeasures = AllUnitOfMeasures.Where(x => (AllProduct.Count(k => k.Id == x.ProductId) > 0)).OrderByDescending(x => x.Name);
                            //Ticket end:#26322 .by rupesh
                            var tmpSearchUnitOfMeasures = AllUnitOfMeasures.Where(x => (x.Name.ToLower().Contains(name_barcode_sku.ToLower())
                                                                 || (!string.IsNullOrEmpty(x.Sku) && !string.IsNullOrWhiteSpace(x.Sku) && x.Sku.ToLower().Contains(name_barcode_sku.ToLower()))
                                                                 || (!string.IsNullOrEmpty(x.BarCode) && !string.IsNullOrWhiteSpace(x.BarCode) && x.BarCode.ToLower().Contains(name_barcode_sku.ToLower())))
                                                              ).OrderByDescending(x => x.Name);
                            if (tmpSearchUnitOfMeasures != null)
                            {
                                foreach (var item in tmpSearchUnitOfMeasures)
                                {
                                    var product = productService.GetLocalProduct(item.ProductId);
                                    if (product != null)
                                    {
                                        product = productService.GetLocalProduct(item.MeasureProductId);
                                        if (product != null)
                                        {
                                            var unitOfMeasureProduct = product.Copy();
                                            unitOfMeasureProduct.Id = item.Id;
                                            unitOfMeasureProduct.Name = item.Name;
                                            unitOfMeasureProduct.BarCode = item.BarCode;
                                            unitOfMeasureProduct.Sku = item.Sku;
                                            unitOfMeasureProduct.ProductUnitOfMeasureDto = item;
                                            unitOfMeasureProduct.IsUnitOfMeasure = true;
                                            //Ticket start:#26980 iPad: UOM product price should be showing in search option.by rupesh
                                            unitOfMeasureProduct.UOMSellingPrice = unitOfMeasureProduct.ProductOutlet.SellingPrice * unitOfMeasureProduct.ProductUnitOfMeasureDto.Qty;
                                            //Ticket end:#26980 .by rupesh
                                            //Start #84444 iOS: FR: remove item price at search bar that includes tax by Pratik
                                            unitOfMeasureProduct.UOMSearchPrice = (Helpers.Settings.StoreGeneralRule.TaxInclusive ? unitOfMeasureProduct.ProductOutlet.SellingPrice : unitOfMeasureProduct.ProductOutlet.PriceExcludingTax) * unitOfMeasureProduct.ProductUnitOfMeasureDto.Qty;
                                            //End #84444 by Pratik
                                            if (unitOfMeasureProduct.ParentId != null)
                                            {
                                                var parentProduct = productService.GetLocalProduct(unitOfMeasureProduct.ParentId.Value);
                                                unitOfMeasureProduct.ItemImageUrl = parentProduct.ItemImageUrl;
                                                unitOfMeasureProduct.ItemImage = parentProduct.ItemImage;
                                            }

                                            tmpsearchproducts.Add(unitOfMeasureProduct);
                                        }
                                    }

                                }
                            }
                        }
                        //Ticket end:#20064 .by rupesh
                     
                        var searchitems1 = tmpsearchproducts.Select(x =>
                        {
                            return new EnterSaleItemDto()
                            {
                                ItemType = Enums.InvoiceItemType.Standard,
                                Product = x
                            };
                        });


                        var searchitem2 = tmpsearchoffers.Select(x =>
                        {
                            return new EnterSaleItemDto()
                            {
                                ItemType = Enums.InvoiceItemType.Composite,
                                Offer = x
                            };
                        });

                        var searchitems = searchitems1.Concat(searchitem2); 

                        //Ticket #12305 Start : Composite Products Not Searchable with Barcodes. By Nikhil
                        // Note : Need to check product/offer null
                        var tempSearch = searchitems.Where(s => (s.Product != null || s.Offer != null));
                        if (tempSearch.Count() > 0)
                        {
                            //start #91259 Items on POS screen search bar not showing in orde By Pratik
                            if(Settings.StoreGeneralRule.ActivateSmartSearchOnPOS)
                                SearchProducts = new ObservableCollection<EnterSaleItemDto>(tempSearch?.ToList().OrderBy(x => (x.Product != null) ? x.Product.Id : x.Offer.Id));
                            else
                                SearchProducts = new ObservableCollection<EnterSaleItemDto>(tempSearch?.ToList().OrderBy(x => (x.Product != null) ? x.Product.Name : x.Offer.Name));
                            //end #91259 By Pratik
                        }
                        //Ticket #12305 End.By Nikhil
                        else
                            SearchProducts = new ObservableCollection<EnterSaleItemDto>(searchitems?.ToList());

                        //Ticket start:#36542 Move Out of Stock to end in Point of Sale Products Listings.by rupesh
                        if(Settings.StoreGeneralRule.ActivateSmartSearchOnPOS)
                        {
                            Func<string, int> intParser = input =>
                            {
                                int result;
                                if (!int.TryParse(input, out result))
                                    return int.MaxValue;
                                
                                //return result > 0 ? 1 : result;
                                return result > 0 ? 0 : (result == 0 ? 1 : 2);  //start #91259 By Pratik
                                
                            };

                            //start #91259 Items on POS screen search bar not showing in orde By Pratik
                            SearchProducts = new ObservableCollection<EnterSaleItemDto>(SearchProducts.OrderBy(x => intParser(x.Product?.Stock)));
                            //SearchProducts = new ObservableCollection<EnterSaleItemDto>(SearchProducts.OrderBy(x => intParser(x.Product?.Stock)).Reverse());
                            //end #91259 By Pratik
                           
                        }
                        //Ticket end:#36542 .by rupesh
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write("eror while product searching : " + ex.ToString());
                    ex.Track();
                    return;
                }
            }
        }
    }
}
