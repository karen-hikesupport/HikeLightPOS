
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class VariantProductModel : BaseViewModel
    {

        #region variant product methods
        public int CustomerId { get; set; }

        ProductDto_POS _VariantProduct { get; set; }
        public ProductDto_POS VariantProduct { get { return _VariantProduct; } set { _VariantProduct = value; SetPropertyChanged(nameof(VariantProduct)); } }

        decimal _Quantity { get; set; } = 1;
        public decimal Quantity { get { return _Quantity; } set { _Quantity = value; StrQuantity = value.ToString(); SetPropertyChanged(nameof(Quantity)); } }

        string _StrQuantity { get; set; }
        public string StrQuantity { get { return _StrQuantity; } set { _StrQuantity = value; SetPropertyChanged(nameof(StrQuantity)); } }
        #endregion

        ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
        ProductServices productService;
        public event EventHandler<ProductDto_POS> AddVariantProduct;
        //Ticket #423 Start:Variant product stock issue. By Nikhil.
        List<ProductVarientsDto> productVarients = new List<ProductVarientsDto>();
        Dictionary<int, int> Selection = new Dictionary<int, int>();
        //Ticket #423 End:By Nikhil.

        //Ticket Start:#11403 How is variant display order by default setup in Hike App? by Rupesh
        public ObservableCollection<string> OrderList { get; set; }
        int _selectedIndex { get; set; }
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                SetPropertyChanged(nameof(SelectedIndex));
                if (VariantProduct != null && _selectedIndex != Settings.VariantOrderIndex)
                {
                    LoadData();
                    Settings.VariantOrderIndex = _selectedIndex;

                }
            }
        }
        //Ticket End by Rupesh

        public VariantProductModel()
        {
            Title = "Variant product";
            //SelectProductVariantCommand = new Command<ProductAttributeValueDto>(SelectProductVariant);
            productService = new ProductServices(productApiService);

            //Ticket Start:#11403 How is variant display order by default setup in Hike App? by Rupesh
            OrderList = new ObservableCollection<string> { "Alphabetical order", "Order of entry" };
            //Ticket End by Rupesh

        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            //  VariantListVIew.ItemHeight = rootview.Height - rootview.Margin.Top - rootview.Margin.Top - VariantListVIew.Margin.Top - VariantListVIew.Margin.Bottom - VariantListVIew.Padding.Top - VariantListVIew.Padding.Bottom - VariantListVIew.Y;
            //Ticket Start:#11403 How is variant display order by default setup in Hike App? by Rupesh
            SelectedIndex = Settings.VariantOrderIndex;
            //Ticket End by Rupesh
            LoadData();
        }


        #region Command
        public ICommand SettingCommand => new Command(SettingTapped);
        public ICommand DecreaseHandleCommand => new Command(DecreaseTapped);
        public ICommand IncreaseHandleCommand => new Command(IncreaseTapped);
        public ICommand QuantityUnfocusedCommand => new Command(QuantityUnfocused); 
        public ICommand VariantAttributeSelectCommand => new Command<ProductAttributeValueDto>(VariantAttributeSelect);
        #endregion

        #region Command Execution

        public void SettingTapped()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void DecreaseTapped()
        {
            if (Quantity != 1)
            {
                Quantity--;
            }
            else
            {
                Quantity = -1;
            }
        }

        void IncreaseTapped()
        {
            if (Quantity != -1)
            {
                Quantity++;
            }
            else
            {
                Quantity = 1;
            }
        }

        void QuantityUnfocused()
        {
            try
            {
                decimal decimalparseresult;
                if (decimal.TryParse(StrQuantity, out decimalparseresult))
                {
                    Quantity = Math.Round(decimalparseresult, 2); ;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void VariantAttributeSelect(ProductAttributeValueDto data)
        {
            try
            {
                var isLast = SelectProductVariant(data);
                if (isLast)
                {
                    AddVariantProductHandle_Clicked();
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        #endregion

        #region Methods
        public void LoadData()
        {
            if (VariantProduct.ProductAttributes != null)
            {
                
                VariantProduct.ProductAttributes = new ObservableCollection<HikePOS.Models.ProductAttributeDto>(VariantProduct.ProductAttributes.OrderBy(x => x.Sequence));
                int cnt = 0;
                if (VariantProduct.ProductAttributes.Count>0)
                    cnt = VariantProduct.ProductAttributes.OrderByDescending(a => a.ProductAttributeValues.Count).FirstOrDefault().ProductAttributeValues.Count;
                for (int i = 0; i < VariantProduct.ProductAttributes.Count(); i++)
                {
                    VariantProduct.ProductAttributes[i].ProductAtrHeight = (60 * cnt) + 40;
                    VariantProduct.ProductAttributes[i].Sequence = i + 1;
                    //Ticket Start:#11403 How is variant display order by default setup in Hike App? by Rupesh
                    //Ticket start: #66592 Incorrect 'Order of Entry' on iPad.by rupesh
                    if (SelectedIndex == 1)
                        VariantProduct.ProductAttributes[i].ProductAttributeValues = new ObservableCollection<ProductAttributeValueDto>(VariantProduct.ProductAttributes[i].ProductAttributeValues.OrderBy(x => x.ProductAttributeValueId));
                    //Ticket end: #66592.by rupesh
                    else
                        VariantProduct.ProductAttributes[i].ProductAttributeValues = new ObservableCollection<ProductAttributeValueDto>(VariantProduct.ProductAttributes[i].ProductAttributeValues.OrderBy(x => x.Value));
                    //Ticket End by Rupesh
                }

                if (VariantProduct.ProductAttributes.Count() == 1)
                {
                    var next_attribute = VariantProduct.ProductAttributes.FirstOrDefault();
                    var productVariants = VariantProduct.ProductVarients.Where(x => (x.VariantAttributesValues.Where(k => k.AttributeId == next_attribute.AttributeId).ToList()).Count > 0).ToList();
                    var attributeValueIds = productVariants.SelectMany(x => x.VariantAttributesValues).ToList().Select(k => k.AttributeValueId).ToList();
                    next_attribute.ProductAttributeValues.Where(x => attributeValueIds.Contains(x.AttributeValueId)).All(a =>
                    {
                        a.IsEnable = true;
                        //var ProductIds = productVariants.Select(x => x.VariantOutlet.;
                        //var product = productService.GetLocalProduct(productVariants.FirstOrDefault(s => s.VariantAttributesValues.Where(k => k.AttributeValueId == a.AttributeValueId && k.AttributeId == a.AttributeId).Any()));
                        var ProductId = productVariants.FirstOrDefault(s => s.VariantAttributesValues.Where(k => k.AttributeValueId == a.AttributeValueId && k.AttributeId == a.AttributeId).Any()).ProductVarientId;
                        var product = productService.GetLocalProductSync(ProductId);
                        a.Stock = (product.ProductOutlet.Stock).ToString("0.####");
                        if (Settings.StoreGeneralRule != null)
                        {
                            //bool result = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.BackOrder") && TrackInventory;
                            a.IsStockVisible = (!string.IsNullOrEmpty(a.Stock) && Settings.StoreGeneralRule.ShowStockCountOnEnterSale && product.TrackInventory);
                        }
                        else
                        {
                            a.IsStockVisible = false;
                        }
                        //a.IsStockVisible = a.IsStockVisible && product.TrackInventory;
                        return true;
                    });
                }
            }
        }

        void AddVariantProductHandle_Clicked()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            var attributeValues = VariantProduct.ProductAttributes.OrderBy(k => k.Sequence).SelectMany(x => x.ProductAttributeValues).Where(l => l.IsSelected).ToList();

            if (VariantProduct.ProductAttributes.Count() == attributeValues.Count())
            {

                var productVarients = VariantProduct.ProductVarients.Where(x => x.VariantAttributesValues.Count() == attributeValues.Count()).ToList();

                for (int i = 0; attributeValues.Count() > i; i++)
                {
                    productVarients = productVarients.Where(x => x.VariantAttributesValues.Count(d => d.Value == attributeValues[i].Value) > 0).ToList();
                }
                var variantproduct = productVarients.FirstOrDefault();
                if (variantproduct != null)
                {
                    ProductDto_POS product = productService.GetLocalProduct(variantproduct.ProductVarientId);
                    if (product != null)
                    {
                        //var result = await InvoiceCalculations.CheckstockValidation(CustomerId, product, Quantity, 0);
                        //if (result.IsValid)
                        //{
                        AddVariantProduct?.Invoke(this, product);
                        //}
                    }
                    else
                    {
                        //await Application.Current.MainPage.DisplayAlert("Alert", "Sorry! We didn't find variant product", "Ok");
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoFoundVariantProduct"));
                    }

                }
                else
                {
                    //await Application.Current.MainPage.DisplayAlert("Alert", "Sorry! We didn't find variant product", "Ok");
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoFoundVariantProduct"));
                }
            }
            else if (VariantProduct.ProductAttributes.Count() > attributeValues.Count())
            {
                string varient_name = VariantProduct.ProductAttributes[attributeValues.Count()].Name;
                //await Application.Current.MainPage.DisplayAlert("Alert", "Please select " + varient_name.ToLower(), "Ok");
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Please_Select_LabelText") + varient_name.ToLower());
            }
            IsBusy = false;
        }

        public bool SelectProductVariant(ProductAttributeValueDto data)
        {
            try
            {
                if (VariantProduct != null && VariantProduct.ProductAttributes != null)
                {
                    var productAttributes = VariantProduct.ProductAttributes;
                    var selected_attribute = productAttributes.FirstOrDefault(x => x.ProductAttributeId == data.ProductAttributeId);

                    if (selected_attribute != null)
                    {
                        var selected_attributValue = selected_attribute.ProductAttributeValues.FirstOrDefault(x => x.ProductAttributeValueId == data.ProductAttributeValueId);
                        if (selected_attributValue.IsEnable)
                        {

                            selected_attribute.ProductAttributeValues.Where(x => x.ProductAttributeValueId != data.ProductAttributeValueId).All(a =>
                            {
                                a.IsSelected = false;
                                return true;
                            });
                            selected_attributValue.IsSelected = true;

                            int current_sequenc = selected_attribute.Sequence + 1;
                            while (current_sequenc <= productAttributes.Count)
                            {

                                var next_attribute = productAttributes.FirstOrDefault(x => x.Sequence == (current_sequenc));
                                if (next_attribute != null)
                                {
                                    next_attribute.ProductAttributeValues.All(a =>
                                    {
                                        a.IsSelected = false;
                                        a.IsEnable = false;
                                        return true;
                                    });

                                    if (current_sequenc == selected_attribute.Sequence + 1)
                                    {
                                        //Ticket #423 Start:Variant product stock issue. By Nikhil.
                                        SetSelection();
                                        productVarients = VariantProduct.ProductVarients.Where(x =>
                                             !Selection.Values.Except(x.VariantAttributesValues.Select(y => y.AttributeValueId)).Any()
                                        ).ToList();
                                        //Ticket #423 End:By Nikhil.

                                        var attributeValueIds = productVarients.SelectMany(x => x.VariantAttributesValues).ToList().Select(k => k.AttributeValueId).ToList();
                                         
                                        next_attribute.ProductAttributeValues.Where(x => attributeValueIds.Contains(x.AttributeValueId)).All(a =>
                                         {
                                             a.IsEnable = true;
                                             if (selected_attribute.Sequence + 1 == productAttributes.Count)
                                             {
                                                 var ProductId = productVarients.FirstOrDefault(s => s.VariantAttributesValues.Where(k => k.AttributeValueId == a.AttributeValueId && k.AttributeId == a.AttributeId).Any()).ProductVarientId;
                                                 var product = productService.GetLocalProductSync(ProductId);
                                                 a.Stock = (product.ProductOutlet.Stock).ToString("0.####");
                                                 if (Settings.StoreGeneralRule != null)
                                                 { 
                                                     a.IsStockVisible = (!string.IsNullOrEmpty(a.Stock) && Settings.StoreGeneralRule.ShowStockCountOnEnterSale && product.TrackInventory);
                                                 }
                                                 else
                                                 {
                                                     a.IsStockVisible = false;
                                                 } 
                                             }
                                             else
                                             {
                                                 a.Stock = "";
                                             }
                                             return true;
                                         });
                                    }
                                }
                                current_sequenc++;
                            }



                            if (selected_attribute.Sequence + 1 > productAttributes.Count)
                            {

                                var ProductAttributes = VariantProduct.ProductAttributes.OrderBy(k => k.Sequence).ToList();

                                if (ProductAttributes != null)
                                {
                                    for (int i = 0; i < ProductAttributes.Count(); i++)
                                    {
                                        if (ProductAttributes[i].ProductAttributeValues != null && ProductAttributes[i].ProductAttributeValues.Count(x => x.IsSelected) < 1)
                                        {
                                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Please_Select_LabelText") + ProductAttributes[i].Name);
                                            selected_attributValue.IsSelected = false;
                                            return false;
                                        }
                                    }
                                }
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                }
                return false;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        //Ticket #423 Start:Variant product stock issue. By Nikhil.
        void SetSelection()
        {
            ClearSelection();

            VariantProduct.ProductAttributes.All(x =>
            {
                x.ProductAttributeValues.Where(y => y.IsSelected).All(z =>
                   {
                       Selection.Add(z.AttributeId, z.AttributeValueId);
                       return true;
                   });
                return true;
            });
            Debug.WriteLine("Selection : " + Selection.Count);
        }

        void ClearSelection()
        {
            Selection.Clear();
            productVarients.Clear();
            VariantProduct.ProductAttributes.All(a =>
            {
                a.ProductAttributeValues.All(b =>
                {
                    b.Stock = "0";
                    b.IsStockVisible = false;
                    return true;
                });
                return true;
            });
        }
        //Ticket #423 End:By Nikhil.  

        #endregion
    }
}
