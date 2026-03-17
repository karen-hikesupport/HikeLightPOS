using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.Helpers;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using HikePOS.Enums;
using System.Security.AccessControl;

namespace HikePOS.ViewModels
{
    public class  ProductDetailViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
        ProductServices productService;
        public event EventHandler<UpdatedInvoiceLineItemMessageCenter> UpdateInvoiceLineItem;

        static ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        static CustomerServices customerService = new CustomerServices(customerApiService);


        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
        UserServices userService;

        ObservableCollection<UserListDto> _Users { get; set; }
        public ObservableCollection<UserListDto> Users { get { return _Users; } set { _Users = value; SetPropertyChanged(nameof(Users)); } }

        UserListDto _selectedUser;
        public UserListDto SelectedUser { get { return _selectedUser; } set { _selectedUser = value; SetPropertyChanged(nameof(SelectedUser)); } }

        bool _isServedBy;
        public bool IsServedBy { get { return _isServedBy; } set { _isServedBy = value; SetPropertyChanged(nameof(IsServedBy)); } }
        //end #84287 .by Pratik

        InvoiceLineItemDto _InvoiceItem { get; set; }
        public InvoiceLineItemDto InvoiceItem { get { return _InvoiceItem; } set { _InvoiceItem = value; SetPropertyChanged(nameof(InvoiceItem)); } }

        InvoiceDto _Invoice { get; set; }
        public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }

       //Ticket start:#63314 Product description not visible on product info section.by rupesh
        string _ProductDescription { get; set; }
        public string ProductDescription { get { return _ProductDescription; } set { _ProductDescription = value; SetPropertyChanged(nameof(ProductDescription)); } }
        //Ticket end:#63314 .by rupesh


        ProductDto_POS _ProductDetail { get; set; }
        public ProductDto_POS ProductDetail { get { return _ProductDetail; } set { _ProductDetail = value; SetPropertyChanged(nameof(ProductDetail)); } }

        ProductImageDto _selectedProductImage { get; set; }
        public ProductImageDto SelectedProductImage { get { return _selectedProductImage; } set { _selectedProductImage = value; SetPropertyChanged(nameof(SelectedProductImage)); } }


        ObservableCollection<TaxDto> _taxList { get; set; }
        public ObservableCollection<TaxDto> TaxList { get { return _taxList; } set { _taxList = value; SetPropertyChanged(nameof(TaxList)); } }

        TaxDto _selectedTax { get; set; }
        public TaxDto SelectedTax
        {
            get { return _selectedTax; }
            set
            {
                _selectedTax = value;
                SetPropertyChanged(nameof(SelectedTax)); SelectedTaxChange();
            }
        }


        bool _isOfflineStock { get; set; } = false;
        public bool IsOfflineStock { get { return _isOfflineStock; } set { _isOfflineStock = value; SetPropertyChanged(nameof(IsOfflineStock)); } }

        //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil
        bool _canChangeTax { get; set; } = true;
        public bool CanChangeTax { get { return _canChangeTax; } set { _canChangeTax = value; SetPropertyChanged(nameof(CanChangeTax)); } }
        //Ticket #10921 End. By Nikhil

        int _IsProductInfoStatus { get; set; } = 0;
        public int IsProductInfoStatus { get { return _IsProductInfoStatus; } set { _IsProductInfoStatus = value; SetPropertyChanged(nameof(IsProductInfoStatus)); } }

        string _TaxAmountValueText { get; set; }
        public string TaxAmountValueText { get { return _TaxAmountValueText; } set { _TaxAmountValueText = value; SetPropertyChanged(nameof(TaxAmountValueText)); } }

        bool _pageIsEnabled = true;
        public bool PageIsEnabled { get { return _pageIsEnabled; } set { _pageIsEnabled = value; SetPropertyChanged(nameof(PageIsEnabled)); } }

        bool _variantProductIsEnabled = true;
        public bool VariantProductIsEnabled { get { return _variantProductIsEnabled; } set { _variantProductIsEnabled = value; SetPropertyChanged(nameof(VariantProductIsEnabled)); } }

        //Ticket start:#67721 Cost price is still showing in POS screen even after unchecking the box.by Pratik
        bool _isCostPrice;
        public bool IsCostPrice { get { return _isCostPrice; } set { _isCostPrice = value; SetPropertyChanged(nameof(IsCostPrice)); } }

        decimal _costPrice;
        public decimal CostPrice { get { return _costPrice; } set { _costPrice = value; SetPropertyChanged(nameof(CostPrice)); } }
        //Ticket end:#67721.by Pratik


        //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
        ProductDto_POS _MeasureProductDetail { get; set; }
        public ProductDto_POS MeasureProductDetail { get { return _MeasureProductDetail; } set { _MeasureProductDetail = value; SetPropertyChanged(nameof(MeasureProductDetail)); } }
        //Ticket end:#20064 .by rupesh

        //Ticket start:#91265 iOS:FR Show Product Type on POS screen. by rupesh
        private string _productTypes { get; set; }
        public string ProductTypes { get { return _productTypes; } set { _productTypes = value; SetPropertyChanged(nameof(ProductTypes)); } }
        //Ticket end:#91265. by rupesh

        ApproveAdminPage approveAdminPage;

        public ProductDetailViewModel()
        {
            Title = "Product details";
            productService = new ProductServices(productApiService);
            userService = new UserServices(userApiService);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            //Ticket start:#67721 Cost price is still showing in POS screen even after unchecking the box.by Pratik
            bool isCostPricePermission = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.ViewCostPrice");
            IsCostPrice = isCostPricePermission;
            //Ticket end:#67721.by Pratik

            PageIsEnabled = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.TogiveLineItemDiscount"); //#95241

            //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
            if (Settings.StoreGeneralRule.ServedByLineItem)
            {
                LoadUserData();
            }
            else
            {
                IsServedBy = false;
            }
            //end #84287 .by Pratik

            //Ticket start:#26839 iPad - Edit line item issue in cart.by rupesh
            VariantProductIsEnabled = true;
            //Ticket end:#26839 .by rupesh
             ProductDescription = "";
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            IsProductInfoStatus = 0;

            //Ticket start:#26839 iPad - Edit line item issue in cart.by rupesh
            PageIsEnabled = false;
            VariantProductIsEnabled = false;
            //Ticket end:#26839 .by rupesh


        }

        #region Command
        public ICommand SaveCommand => new Command(SaveTapped);
        public ICommand DecreaseHandleCommand => new Command(DecreaseTapped);
        public ICommand IncreaseHandleCommand => new Command(IncreaseTapped);
        public ICommand QuantityUnfocusedCommand => new Command(QuantityUnfocused);
        public ICommand PriceUnfocusedCommand => new Command(PriceUnfocused);
        public ICommand DiscountUnfocusedCommand => new Command(DiscountUnfocused);
        public ICommand ProductInfoCommand => new Command(ProductInfoTapped);
        public ICommand MoreDetailsCommand => new Command(MoreDetailsTapped);
        public ICommand StockDetailsCommand => new Command(StockDetailsTapped);
        public ICommand ProductImagesSelectCommand => new Command(ProductImagesSelect);
        #endregion

        #region Command Execution

        void ProductImagesSelect()
        {
            if (SelectedProductImage != null)
            {
                ProductDetail.ItemImage = new Guid(SelectedProductImage.ImageName);
            }
        }

        void PriceUnfocused()
        {
            if (!string.IsNullOrEmpty(InvoiceItem.StrRetailPrice))
            {
                //Start ticket#103384 
                // Start ticket #78058 Flat Mark-up percentage (%) NOT WROKING IF 0% by Pratik
                var retailPrice = InvoiceItem.RetailPrice;
                if (Invoice?.CustomerGroupId != null && InvoiceItem.CustomerGroupDiscountPercent != null && InvoiceItem.CustomerGroupDiscountPercent > 0)
                {
                    var discountperItem = InvoiceCalculations.GetValuefromPercent(InvoiceItem.RetailPrice, InvoiceItem.CustomerGroupDiscountPercent);
                    retailPrice -= discountperItem;
                }
                InvoiceItem.DiscountValue = InvoiceCalculations.GetDiscountPercentfromValue(retailPrice, Convert.ToDecimal(InvoiceItem.StrRetailPrice));
                InvoiceItem.SoldPrice = Convert.ToDecimal(InvoiceItem.StrRetailPrice);
                var markup = InvoiceCalculations.GetDiscountPercentfromValue(InvoiceItem.RetailPrice, Convert.ToDecimal(InvoiceItem.StrRetailPrice));
                if (InvoiceItem.SoldPrice > InvoiceItem.RetailPrice)
                   InvoiceItem.MarkupValue = Math.Round(markup , Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero)  * -1;
                else
                   InvoiceItem.MarkupValue = 0;
                // End ticket #78058 by Pratik
                //End ticket#103384 
               
            }
        }

        void DiscountUnfocused()
        {
            try
            {
                if (!string.IsNullOrEmpty(InvoiceItem.StrDiscountValue) && InvoiceItem.StrDiscountValue != ".")
                {
                    //Start ticket#103384 
                    var retailPrice = InvoiceItem.RetailPrice;
                    if (Invoice?.CustomerGroupId != null && InvoiceItem.CustomerGroupDiscountPercent != null && InvoiceItem.CustomerGroupDiscountPercent > 0)
                    {
                        var discountperItem = InvoiceCalculations.GetValuefromPercent(InvoiceItem.RetailPrice, InvoiceItem.CustomerGroupDiscountPercent);
                        retailPrice -= discountperItem;
                    }
                    if (!decimal.TryParse(InvoiceItem.StrDiscountValue, out var discountValue) || discountValue == 0)
                    {
                        InvoiceItem.StrRetailPrice = retailPrice.ToString();
                    }
                    else
                    {
                        InvoiceItem.StrRetailPrice = (retailPrice - InvoiceCalculations.GetValuefromPercent(retailPrice, Convert.ToDecimal(InvoiceItem.StrDiscountValue))).ToString();
                    }
                    InvoiceItem.DiscountValue = Convert.ToDecimal(InvoiceItem.StrDiscountValue);
                   
                    if (Invoice?.CustomerGroupId == null)
                    {
                        if (InvoiceItem.DiscountValue < 0)
                            InvoiceItem.MarkupValue = Math.Round(InvoiceItem.DiscountValue , Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                        else
                            InvoiceItem.MarkupValue = 0;

                        InvoiceItem.SoldPrice = Convert.ToDecimal(InvoiceItem.StrRetailPrice);
                    }
                    //End ticket#103384 

                }
            }
            catch (Exception ex)
            {
                ex.Track();
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("DiscountValidationMessage"), Colors.Red, Colors.White);
            }
        }

        void ProductInfoTapped()
        {
            IsProductInfoStatus = 0;
        }

        void MoreDetailsTapped()
        {
            IsProductInfoStatus = 1;
           
            if (ProductDetail == null || (ProductDetail != null && ProductDetail.ParentId != null && ProductDetail.ParentId > 0))  //start:#90941 by pratik
            {
                LoadProductDetails();
            }
            //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
            LoadMeasureProductStockDetails();
            //Ticket end:#20064 .by rupesh
            //Ticket start:#63314 Product description not visible on product info section.by rupesh
            LoadProductDescription();
            //Ticket end:#63314 .by rupesh
            //Ticket start:#91265 iOS:FR Show Product Type on POS screen. by rupesh
             if (ProductDetail.ProductCategories != null && ProductDetail.ProductCategories.Count > 0)
             {
                var categories = productService.GetLocalCategoriesByIds(ProductDetail.ProductCategories.ToList());
                ProductTypes =  string.Join(", ", categories?.Select(x=>x.Name).ToList());
             }
            //Ticket end:#91265. by rupesh

        }

        async void StockDetailsTapped()
        {
            try
            {
                IsProductInfoStatus = 2;
                await LoadProductStockDetails();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async void SaveTapped(object skipLock = null)
        {
            try
            {
                //if (string.IsNullOrEmpty(InvoiceItem.StrDiscountValue))
                //    InvoiceItem.DiscountValue = 0;
                //else
                //InvoiceItem.DiscountValue = Convert.ToDecimal(InvoiceItem.StrDiscountValue);

                //Ticket start:#71297 iPad- Feature: A lock the sale price.by rupesh
                bool isDiscountAProductPriceToLowerThanItsCostPrice = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.DiscountAProductPriceToLowerThanItsCostPrice");
                bool isRequireManagerPermissionToDiscountAPriceLowerThanCost = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.RequireManagerPermissionToDiscountAPriceLowerThanCost");

                //#96092
                InvoiceItem.SoldPrice = Math.Round(InvoiceItem.SoldPrice, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero); 
                //#96092

                //#95241
                decimal discnt = 0;
                decimal priceval = InvoiceItem.SoldPrice;
                if (InvoiceItem.RetailPrice > 0)
                {
                    if (!string.IsNullOrEmpty(InvoiceItem.StrDiscountValue) && decimal.TryParse(InvoiceItem.StrDiscountValue, out discnt))
                    {
                        priceval = InvoiceItem.RetailPrice - ((InvoiceItem.RetailPrice * discnt) / 100);
                    }
                }
                //#95241

                if (skipLock == null && !isDiscountAProductPriceToLowerThanItsCostPrice && priceval < InvoiceItem.ItemCost)
                {
                    await App.Alert.ShowAlert("", "A product cannot be discounted to a price lower than its cost price.", "Ok");
                    return;
                }
                else if (skipLock == null && isRequireManagerPermissionToDiscountAPriceLowerThanCost && priceval < InvoiceItem.ItemCost)
                {
                    if (approveAdminPage == null)
                    {
                        approveAdminPage = new ApproveAdminPage();
                        approveAdminPage.ViewModel.Users = new ObservableCollection<UserListDto>(approveAdminPage.ViewModel.Users);
                        approveAdminPage.SelectedUser += async (object sender, UserListDto e) =>
                        {
                            InvoiceItem.approvedByUser = e.Id;
                            InvoiceItem.approvedByUserName = e.FullName;
                            await approveAdminPage.Close();
                            SaveTapped(true);
                        };
                    }
                    await _navigationService.GetCurrentPage.Navigation.PushModalAsync(approveAdminPage);
                    return;
                }
                //Ticket end:#71297.by rupesh

                if (InvoiceItem.RetailPrice <= 0.0m)
                {
                    if (!string.IsNullOrEmpty(InvoiceItem.StrRetailPrice))
                        InvoiceItem.RetailPrice = Convert.ToDecimal(InvoiceItem.StrRetailPrice);
                }

                var result = new BackorderResult();
                //Ticket start:#15538 iOS - Exchange Quantity Validation.by rupesh
                //Ticket start:#21386 Android : I can't change the stock manually to exchange product.by rupesh
                if ((Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange) && InvoiceItem.ActionType != ActionType.Sell)
                //Ticket end:#21386 .by rupesh
                {
                    if (InvoiceItem.Quantity >= 0) // >InvoiceItem.actualqty)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("IncreaseRefundProductQuantityValidationMessage"), Colors.Red, Colors.White);
                        return;
                    }
                    else if (InvoiceItem.Quantity < InvoiceItem.actualqty)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("RefundProductQuantityGreaterValidationMessage"), Colors.Red, Colors.White);
                        return;
                    }
                }
                //Ticket end:#15538 .by rupesh

                //Ticket start:#22898 Composite sale not working properly. by rupesh
                //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.Standard || InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.CompositeProduct || InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.UnityOfMeasure)
                //Ticket end:#22898. by rupesh
                //Ticket end:#20064 .by rupesh
                {
                    if (ProductDetail == null)
                    {
                        LoadProductDetails();
                    }


                    //#37248 iPad: BO shouldn't be created when the sale came from the Woo commerce.
                    bool isBackorderCheck = true;
                    if (Invoice.InvoiceFrom != InvoiceFrom.iPad && Invoice.InvoiceFrom != InvoiceFrom.Android
                            && Invoice.InvoiceFrom != InvoiceFrom.Web)
                    {
                        isBackorderCheck = false;
                    }
                    //#37248 iPad: BO shouldn't be created when the sale came from the Woo commerce.


                    //bool res = checkstockValidation(ProductDetail, InvoiceItem.Quantity - 1 );//Added InvoiceItem.BackOrderQty to avoid backorder decimal issue
                    //Start #85662 iOS :Back order Not working while 1 line item setting is true By Pratik
                    //Ticket start:#63960 iPad: Product sold on iPad even after 0 stock.by rupesh
                    //result = await InvoiceCalculations.CheckstockValidation(ProductDetail, InvoiceItem.Quantity, InvoiceItem.BackOrderQty, Invoice, isBackorderCheck, InvoiceItem);
                    result = await InvoiceCalculations.CheckstockValidation(ProductDetail, InvoiceItem.Quantity, Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct ? 0 : InvoiceItem.BackOrderQty, Invoice, isBackorderCheck, InvoiceItem);
                    //Ticket end:#63960.by rupesh
                    //End #85662 By Pratik
                    if (result == null)
                        return;
                    bool res = result.IsValid;
                    if (!res)
                    {
                        return;
                    }

                }
                else if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.Composite)
                {
                    bool haveStock = await CheckCompositestockValidation(InvoiceItem.Quantity);

                    if (haveStock)
                    {
                        //if (string.IsNullOrEmpty(InvoiceItem.StrRetailPrice))
                        //    InvoiceItem.DiscountValue = InvoiceCalculations.GetDiscountPercentfromValue(InvoiceItem.RetailPrice, InvoiceItem.SoldPrice);
                        //else
                        //    InvoiceItem.DiscountValue = InvoiceCalculations.GetDiscountPercentfromValue(InvoiceItem.RetailPrice, Convert.ToDecimal(InvoiceItem.StrRetailPrice));

                        //if (!InvoiceItem.DiscountIsAsPercentage)
                        //InvoiceItem.DiscountValue = InvoiceCalculations.GetValuefromPercent(InvoiceItem.RetailPrice, InvoiceItem.DiscountValue);
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CompositeOutofStockMessage"), Colors.Red, Colors.White);
                        return;
                    }
                }
                else
                {
                    //if (string.IsNullOrEmpty(InvoiceItem.StrRetailPrice))
                    //    InvoiceItem.DiscountValue = InvoiceCalculations.GetDiscountPercentfromValue(InvoiceItem.SoldPrice, InvoiceItem.RetailPrice);
                    //else
                    //    InvoiceItem.DiscountValue = InvoiceCalculations.GetDiscountPercentfromValue(InvoiceItem.RetailPrice, Convert.ToDecimal(InvoiceItem.StrRetailPrice));

                    //if (!InvoiceItem.DiscountIsAsPercentage)
                    //InvoiceItem.DiscountValue = InvoiceCalculations.GetValuefromPercent(InvoiceItem.RetailPrice, InvoiceItem.DiscountValue);
                }

                //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
                if (IsServedBy)
                {
                    InvoiceItem.ServedByName = SelectedUser.FullName;
                    InvoiceItem.CreatorUserId = SelectedUser.Id;
                }
                //end #84287 .by Pratik

                UpdatedInvoiceLineItemMessageCenter UpdatedInvoiceLineItem = new UpdatedInvoiceLineItemMessageCenter() { invoiceLineItemDto = InvoiceItem, result = result };

                UpdateInvoiceLineItem?.Invoke(this, UpdatedInvoiceLineItem);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void DecreaseTapped()
        {
            if (Invoice.Status == Enums.InvoiceStatus.Refunded && InvoiceItem.Quantity > 0)
            {
                InvoiceItem.Quantity = -1;
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("IncreaseRefundProductQuantityValidationMessage"), Colors.Red, Colors.White);
                return;
            }

            //if(Invoice.Status != Enums.InvoiceStatus.Refunded && InvoiceItem.Quantity <= 1)
            //{
            //    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("DecreaseSaleProductQuantityValidationMessage"), Colors.Red, Colors.White);
            //    return;
            //}

            if (InvoiceItem.Quantity != 1)
            {
                InvoiceItem.Quantity--;
            }
            else
            {
                InvoiceItem.Quantity = -1;
            }
        }

        async void IncreaseTapped()
        {
            if (Invoice.Status == Enums.InvoiceStatus.Refunded && InvoiceItem.Quantity > 0)
            {
                InvoiceItem.Quantity = -1;
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("IncreaseRefundProductQuantityValidationMessage"), Colors.Red, Colors.White);
                return;
            }

            if (InvoiceItem.Quantity != -1)
            {
                //Ticket start:#22898 Composite sale not working properly. by rupesh
                //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.Standard || InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.CompositeProduct || InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.UnityOfMeasure)
                //Ticket end:#20064 .by rupesh
                //Ticket end:#22898. by rupesh
                {
                    if (ProductDetail == null)
                    {
                        LoadProductDetails();
                    }

                    //Start Ticket #65352 iOS: 1 QTY product add multiple time in line items By Pratik
                    // var result = await InvoiceCalculations.CheckstockValidation(ViewModel.ProductDetail, ViewModel.InvoiceItem.Quantity + 1, ViewModel.InvoiceItem.BackOrderQty, ViewModel.Invoice,true,ViewModel.InvoiceItem);
                    //Ticket end:#63960.by rupesh
                    // bool res = result.IsValid;
                    // if (res)
                    //{
                    InvoiceItem.Quantity++;
                    // }
                    //End Ticket #65352 By Pratik
                }
                else if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.Composite)
                {
                    bool haveStock = await CheckCompositestockValidation(InvoiceItem.Quantity + 1);

                    if (haveStock)
                        InvoiceItem.Quantity++;
                    else
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CompositeOutofStockMessage"), Colors.Red, Colors.White);

                }
                else
                {
                    InvoiceItem.Quantity++;
                }
            }
            else
            {
                InvoiceItem.Quantity = 1;
            }
        }

        void QuantityUnfocused()
        {
            try
            {
                decimal decimalparseresult;
                if (decimal.TryParse(InvoiceItem.StrQuantity, out decimalparseresult))
                {


                    if (Invoice.Status == Enums.InvoiceStatus.Refunded && decimalparseresult >= 0)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("IncreaseRefundProductQuantityValidationMessage"), Colors.Red, Colors.White);
                        InvoiceItem.Quantity = -1;
                        return;
                    }

                    InvoiceItem.Quantity = decimalparseresult; //Math.Round(decimalparseresult,2);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        #endregion

        #region Methods
        public void LoadProductDetails()
        {
            using (new Busy(this, true))
            {
                if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.Standard)  //&& (ProductDetail == null || ProductDetail.Id < 1))
                {
                    var tmpProductDetail =  productService.GetLocalProduct(InvoiceItem.InvoiceItemValue);
                    if (tmpProductDetail != null && tmpProductDetail.ParentId != null && tmpProductDetail.ParentId > 0)
                    {
                        ProductDto_POS ParentProduct =  productService.GetLocalProduct(tmpProductDetail.ParentId.Value);
                        if (tmpProductDetail.ItemImage == null && ParentProduct != null && ParentProduct.ItemImage != null)
                        {
                            tmpProductDetail.ItemImage = ParentProduct.ItemImage;
                            //Ticket start:#28373 iPad: variant image is not showing in product info.by rupesh
                            tmpProductDetail.ItemImageUrl = ParentProduct.ItemImageUrl;
                            //Ticket end:#28373 .by rupesh
                        }

                        if (ParentProduct != null && ParentProduct.HasVarients)
                        {
                            ObservableCollection<ProductImageDto> productimages = new ObservableCollection<ProductImageDto>();
                            foreach (var product in ParentProduct.ProductVarients)
                            {
                                ProductDto_POS tmpProduct = productService.GetLocalProduct(product.ProductVarientId);
                                if (tmpProduct != null && tmpProduct.ItemImage != null)
                                {
                                    productimages.Add(new ProductImageDto()
                                    {
                                        ImageName = tmpProduct.ItemImage.Value.ToString()
                                    });
                                }
                            }
                            tmpProductDetail.ProductImages = productimages;
                            //Ticket start:#90941 iOS:FR: Custom filed on product detailpage by pratik
                            tmpProductDetail.CustomField = ParentProduct.CustomField;
                            //Ticket end:#90941 by pratik

                        }
                    }
                    ProductDetail = tmpProductDetail;
                    CostPrice = ProductDetail.ProductOutlet.CostPrice;
                }
                //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                else if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.UnityOfMeasure) 
                {
                    var tmpProductDetail = productService.GetLocalProduct(InvoiceItem.InvoiceLineSubItems.FirstOrDefault().ItemId);
                    if (tmpProductDetail != null && tmpProductDetail.ParentId != null && tmpProductDetail.ParentId > 0)
                    {
                        ProductDto_POS ParentProduct = productService.GetLocalProduct(tmpProductDetail.ParentId.Value);
                        if (tmpProductDetail.ItemImage == null && ParentProduct != null && ParentProduct.ItemImage != null)
                        {
                            tmpProductDetail.ItemImage = ParentProduct.ItemImage;
                        }

                        if (ParentProduct != null && ParentProduct.HasVarients)
                        {
                            ObservableCollection<ProductImageDto> productimages = new ObservableCollection<ProductImageDto>();
                            foreach (var product in ParentProduct.ProductVarients)
                            {
                                ProductDto_POS tmpProduct = productService.GetLocalProduct(product.ProductVarientId);
                                if (tmpProduct != null && tmpProduct.ItemImage != null)
                                {
                                    productimages.Add(new ProductImageDto()
                                    {
                                        ImageName = tmpProduct.ItemImage.Value.ToString()
                                    });
                                }
                            }
                            tmpProductDetail.ProductImages = productimages;
                            //Ticket start:#90941 iOS:FR: Custom filed on product detailpage by pratik
                            tmpProductDetail.CustomField = ParentProduct.CustomField;
                            //Ticket end:#90941 by pratik
                        }
                    }
                    ProductDetail = tmpProductDetail;

                    CostPrice = ProductDetail.ProductOutlet.CostPrice;
                    if(ProductDetail.ProductUnitOfMeasureDto != null && ProductDetail.ProductUnitOfMeasureDto.Qty > 0)
                        CostPrice = ProductDetail.ProductUnitOfMeasureDto.Qty *  ProductDetail.ProductOutlet.CostPrice;
                }
                //Ticket end:#20064 .by rupesh

            }

        }

        public async Task LoadProductStockDetails()
        {
            try
            {
                using (new Busy(this, true))
                {
                    if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.Standard)  //&& (ProductDetail == null || ProductDetail.Id < 1))
                    {
                        //Ticket start:#28373 iPad: variant image is not showing in product info.by rupesh
                        LoadProductDetails();
                        //Ticket end:#28373 .by rupesh
                        if (ProductDetail != null)
                        {
                            CostPrice = ProductDetail.ProductOutlet.CostPrice;
                            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                            {
                                var ProductStockOutlets = await productService.GetRemoteProductStocksByProductId(Fusillade.Priority.UserInitiated, ProductDetail.Id);
                                if (ProductStockOutlets != null && ProductStockOutlets.Any())
                                    ProductDetail.ProductOutlets = ProductStockOutlets;
                                IsOfflineStock = false;
                            }
                            else
                            {
                                ProductDetail.ProductOutlets = new ObservableCollection<ProductOutletDto_POS>() { ProductDetail.ProductOutlet };
                                IsOfflineStock = true;
                            }
                        }
                    }
                    //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                    else if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.UnityOfMeasure) 
                    {
                        //Ticket start:#24687 Product Info Not Showing Completely for Some UoM Products.by rupesh
                        ProductDetail = productService.GetLocalUnitOfMeasureProduct(InvoiceItem.InvoiceItemValue);
                        if (ProductDetail != null)
                        {
                            CostPrice = ProductDetail.ProductOutlet.CostPrice;
                            if(ProductDetail.ProductUnitOfMeasureDto != null && ProductDetail.ProductUnitOfMeasureDto.Qty > 0)
                                CostPrice = ProductDetail.ProductUnitOfMeasureDto.Qty *  ProductDetail.ProductOutlet.CostPrice;
                            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                            {
                                var ProductStockOutlets = await productService.GetRemoteProductStocksByProductId(Fusillade.Priority.UserInitiated, ProductDetail.ProductUnitOfMeasureDto.MeasureProductId);
                                if (ProductStockOutlets != null && ProductStockOutlets.Any())
                                    ProductDetail.ProductOutlets = ProductStockOutlets;
                                IsOfflineStock = false;
                            }
                            else
                            {
                                ProductDetail.ProductOutlets = new ObservableCollection<ProductOutletDto_POS>() { ProductDetail.ProductOutlet };
                                IsOfflineStock = true;
                            }
                        }
                        //Ticket end:#24687 .by rupesh
                    }
                    //Ticket end:#20064 .by rupesh

                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async Task<bool> CheckCompositestockValidation(decimal Quantity)
        {
            bool haveStock = true;
            foreach (var item in InvoiceItem.InvoiceLineSubItems)
            {
                if (haveStock)
                {
                    var product = productService.GetLocalProduct(item.ItemId);
                    var checkvalidateQty = (Quantity * item.Quantity);

                    var result = await InvoiceCalculations.CheckCompositestockValidation(product, checkvalidateQty, 0);
                    if (!result.IsValid)
                        haveStock = false;

                }
            }
            return haveStock;
        }

        public void SelectedTaxChange()
        {
            InvoiceItem.LineItemTaxes = new ObservableCollection<LineItemTaxDto>();
            if (SelectedTax != null && SelectedTax.SubTaxes != null)
            {

                //Ticket start:#66015 iOS: Tax removed from product does not work correctly on Cart.by pratik
                if (Settings.StoreGeneralRule.RetailPriceUpdateWhenTaxChangeCart && Invoice.TaxInclusive)
                {
                    var _price = InvoiceItem.RetailPrice - InvoiceCalculations.CalculateTaxInclusive(InvoiceItem.RetailPrice, InvoiceItem.TaxRate);
                    InvoiceItem.RetailPrice = Math.Abs(Convert.ToDecimal((_price) * (1 + (SelectedTax.Rate / 100))));
                }
                //Ticket end:#66015 .by pratik

                InvoiceItem.TaxId = SelectedTax.Id;
                InvoiceItem.TaxName = SelectedTax.Name;
                InvoiceItem.TaxRate = SelectedTax.Rate;

                foreach (var item in SelectedTax.SubTaxes)
                {
                    var subtax = new LineItemTaxDto
                    {
                        Id = item.Id,
                        TaxId = item.TaxId,
                        TaxRate = item.Rate,
                        TaxName = item.Name
                    };

                    InvoiceItem.LineItemTaxes.Add(subtax);
                }


                TaxAmountValueText = SelectedTax.Rate + "%  |  " + (changeLineItemDiscountByPrice(InvoiceItem.RetailPrice, InvoiceItem.SoldPrice)).ToString("c");

            }
        }
        public decimal changeLineItemDiscountByPrice(decimal fromPrice, decimal price)
        {
            //InvoiceItem.DiscountValue = InvoiceCalculations.GetDiscountPercentfromValue(fromPrice, price);

            var tempinvoiceItem = InvoiceItem.Copy();


            tempinvoiceItem = InvoiceCalculations.CalculateLineItemTotal(tempinvoiceItem, Invoice);

            InvoiceItem.TaxAmount = tempinvoiceItem.TaxAmount;
            return tempinvoiceItem.TaxAmount;

        }

        //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
        public void LoadMeasureProductStockDetails()
        {
            //Ticket start:#24687 Product Info Not Showing Completely for Some UoM Products.by rupesh
            if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.UnityOfMeasure)
            {
                MeasureProductDetail = productService.GetLocalProduct(InvoiceItem.InvoiceLineSubItems.FirstOrDefault().ItemId);

            }
            //Ticket end:#24687 .by rupesh

        }
        //Ticket end:#20064 .by rupesh

        //Ticket start:#63314 Product description not visible on product info section.by rupesh
        public async void LoadProductDescription()
        {
            try
            {
                //using (new Busy(this, true))
                //{
                    if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.Standard)
                    { 
                        if (ProductDetail != null)
                        {
                            CostPrice = ProductDetail.ProductOutlet.CostPrice;
                            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                            {
                                var productDescription = await productService.GetProductDescription(Fusillade.Priority.UserInitiated, ProductDetail.Id);
                                ProductDescription = "<span style=\"font-size:" + 16 + "\">" + productDescription + "</span>";
                                ProductDetail.Description = productDescription;
                            }
                        }
                    }
                    else if (InvoiceItem.InvoiceItemType == Enums.InvoiceItemType.UnityOfMeasure)
                    {
                        ProductDetail = productService.GetLocalUnitOfMeasureProduct(InvoiceItem.InvoiceItemValue);
                        if (ProductDetail != null)
                        {
                            CostPrice = ProductDetail.ProductOutlet.CostPrice;
                            if(ProductDetail.ProductUnitOfMeasureDto != null && ProductDetail.ProductUnitOfMeasureDto.Qty > 0)
                                CostPrice = ProductDetail.ProductUnitOfMeasureDto.Qty *  ProductDetail.ProductOutlet.CostPrice;
                            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                            {
                                var productDescription = await productService.GetProductDescription(Fusillade.Priority.UserInitiated, ProductDetail.Id);
                                ProductDescription = "<span style=\"font-size:" + 16 + "\">" + productDescription + "</span>";
                            }
                        }
                    }

                //}
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        //Ticket end:#63314 .by rupesh

        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        public async void LoadUserData()
        {
            try
            {
                IsServedBy = !Settings.IsQuoteSale;
                Users = new ObservableCollection<UserListDto>();
                var res = userService.GetLocalUsers();
                if (res != null)
                {
                    var result = res.Where(x => x.Outlets != null && x.Roles != null && x.Roles.Any() && x.Outlets.Any(s => s.OutletId == Settings.SelectedOutletId)).Select(x => { x.Roles = new ObservableCollection<UserListRoleDto>() { x.Roles.FirstOrDefault() }; return x; });
                    if (result != null)
                        Users = new ObservableCollection<UserListDto>(result);
                }
                else
                {
                    res = await userService.GetRemoteUsers(Fusillade.Priority.UserInitiated, false);
                    if (res != null)
                    {
                        var result = res.Where(x => x.Outlets != null && x.Roles != null && x.Roles.Any() && x.Outlets.Any(s => s.OutletId == Settings.SelectedOutletId)).Select(x => { x.Roles = new ObservableCollection<UserListRoleDto>() { x.Roles.FirstOrDefault() }; return x; });
                        if (result != null)
                            Users = new ObservableCollection<UserListDto>(result);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            finally
            {
                if (Users != null && Users.Count <= 0)
                {
                    IsServedBy = false;
                }
                else
                {
                    if (!InvoiceItem.CreatorUserId.HasValue)
                    {
                        InvoiceItem.CreatorUserId = Settings.CurrentUser.Id;
                        InvoiceItem.ServedByName = Settings.CurrentUser.FullName;
                    }
                    if (Users.Any(a => a.Id == InvoiceItem.CreatorUserId))
                        SelectedUser = Users.First(a => a.Id == InvoiceItem.CreatorUserId);
                }
            }
        }
        //end #84287 .by Pratik


        #endregion
    }
}
