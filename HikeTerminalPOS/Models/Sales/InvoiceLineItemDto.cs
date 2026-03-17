using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
    public class InvoiceLineItemDto : FullAuditedPassiveEntityDto
    {
        public InvoiceLineItemDto()
        {
            InvoiceLineSubItems = new ObservableCollection<InvoiceLineSubItemDto>();
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            InvoiceItemFulfillments = new ObservableCollection<InvoiceItemFulfillmentDto>();
            //End #84293 by Pratik
        }

        public GiftCardInfo giftCardInfo { get; set; }

        public int InvoiceId { get; set; }
        public InvoiceItemType InvoiceItemType { get; set; }
        public int InvoiceItemValue { get; set; }
        public string InvoiceItemImage { get; set; }

        [JsonIgnore]
        public string FullInvoiceItemImage =>  InvoiceItemImage?.GetImageUrl("Product_Medium_Entersale")?.ToString() ?? string.Empty;
        //Ticket #11252 End. By Nikhil

        //Start #90941 iOS:FR: Custom filed on product detailpage By Pratik
        public string CustomField { get; set; }
        //End #90941 By Pratik

        public int Sequence { get; set; }

        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        [JsonIgnore]
        string _servedByName;
        public string ServedByName
        {
            get
            {
                if (Settings.CurrentUser != null && Settings.CurrentUser.Id == CreatorUserId)
                    return string.Empty;

                return _servedByName;
            }
            set
            {
                _servedByName = value;
                SetPropertyChanged(nameof(ServedByName));
            }
        }
        public int? CreatorUserId { get; set; }
        //end #84287 .by Pratik


        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.
        //[JsonIgnore]
        //public bool IsSerialNumberEditableFromEntersale { get; set; } = false;



        [JsonIgnore]
        bool _IsSerialNumberEditableFromEntersale { get; set; } = false;

        public bool IsSerialNumberEditableFromEntersale
        {
            get
            {
                return _IsSerialNumberEditableFromEntersale;
            }
            set
            {
                _IsSerialNumberEditableFromEntersale = value;
                SetPropertyChanged(nameof(IsSerialNumberEditableFromEntersale));
            }
        }
        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.

        public bool EnableSerialNumber { get; set; }


        

        string _serialNumber { get; set; }
        public string SerialNumber
        {
            get
            {
                return _serialNumber;
            }
            set
            {
                _serialNumber = value;

                var s = IsReopenFromSaleHistory;

                if ((string.IsNullOrEmpty(_serialNumber) && EnableSerialNumber)) //#35112 iPad: Can't Add S/N to Orders from WooCommerce
                {
                    IsSerialNumberEditableFromEntersale = true;
                }
                
                SetPropertyChanged(nameof(SerialNumber));
            }
        }

        [JsonIgnore]
        string _title { get; set; }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                SetPropertyChanged(nameof(Title));
            }
        }

        [JsonIgnore]
        public string ProductTitleWithQuantity
        {
            get
            {
                string val = Price1;
                return Quantity + " X " + Title + " @ " + Price1;
            }
        }

        //Ticket start:#90943 iOS:FR Display Barcode or SKU instead of Product Name .by Pratik
        //Ticket start:#46369 iPad: FR - SKU showing in Gift and delivery docket Print receipt.by rupesh
        [JsonIgnore]
        public string ProductTitleWithSku
        {
            get
            {
                return Title +  (string.IsNullOrEmpty(SKUWithLabel) ? "" :  "(" + SKUWithLabel + ")");
            }
        }
        //Ticket end:#46369 .by rupesh
       
        [JsonIgnore]
        public string ProductSKUTitleWithQuantity => Quantity + " X " + Sku + " @ " + Price1;

        private string _productTitle;
        public string ProductTitle
        {
            get
            {
                if (_productTitle == null || _productTitle == "")
                    return Settings.CurrentRegister.ReceiptTemplate.ReplaceProductNameWithSKU ? SKUWithLabel : (Settings.CurrentRegister.ReceiptTemplate.PrintSKU ? ProductTitleWithSku : Title);
                else
                    return _productTitle;
            }
            set
            {
                _productTitle = value;
                SetPropertyChanged(nameof(ProductTitle));
            }
        }
      //Ticket end:#90943 .by Pratik


        [JsonIgnore]
        public bool IsExtraproduct
        {
            get
            {
                if (InvoiceExtraItemValueParent != null)
                    return true;
                else
                    return false;

            }
        }

        [JsonIgnore]
        public bool HasExtraproduct
        {
            get; set;
        }

        [JsonIgnore]
        string _description { get; set; }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                SetPropertyChanged(nameof(Description));
            }
        }

        [JsonIgnore]
        string _Notes { get; set; }

        public string Notes
        {
            get { return _Notes; }
            set
            {
                _Notes = value;
                SetPropertyChanged(nameof(Notes));
            }
        }

        public string Category { get; set; }
      //  [JsonIgnore]
        public string CategoryDtos { get; set; }

        public string Brand { get; set; }
        public string Tags { get; set; }
        public string Season { get; set; }

        [JsonIgnore]
        public int  categoryId
        {
            get
            {
                if (CategoryDtos == null)
                    return 0;
                var productCategories = JsonConvert.DeserializeObject<System.Collections.Generic.List<CategoryDto>>(CategoryDtos);
                return productCategories.FirstOrDefault() != null ? productCategories.FirstOrDefault().Id  : 0;

            }
        }



        [JsonIgnore]
        public decimal TotalReatilAmount
        {
            get
            {
                //Ticket #12405 Start : Sales Total Not Correct. By Nikhil
                // return RetailPrice * Quantity;

                //Ticket start:#45690 Extra penny added to the subtotal and total once in a while.by rupesh
                return Math.Round(RetailPrice, Settings.StoreDecimalDigit) * Quantity;
                //Ticket end:#45690 .by rupesh

                //Following code commented to resolve rounding issue. 
                //var CrossMultilingual = DependencyService.Get<IMultilingual>();
                ////Ticket #9414 Start : Invoice line item rounding  issue. By Nikhil.
                //return Math.Round(RetailPrice, CrossMultilingual.CurrentCultureInfo.NumberFormat.CurrencyDecimalDigits
                //    , MidpointRounding.AwayFromZero) * Quantity;
                ////Ticket #9414 End:By Nikhil. 
                //Ticket #12405 End. By Nikhil  
            }
        }

        [JsonIgnore]
        public bool IsTotalReatilAmountVisible
        {
            //Ticket start:#74632.by rupesh
            get
            {
                return TotalReatilAmount.ToPositive() > TotalAmount.ToPositive() && isEnable;
            }
            //Ticket end:#74632.by rupesh
        }

        [JsonIgnore]
        public bool isEnable
        {
            get
            {
                return InvoiceItemType != InvoiceItemType.Discount;
            }
        }

        [JsonIgnore]
        decimal _quantity { get; set; }

       // [JsonIgnore]
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                //_quantity = value;
                StrQuantity = value.ToString();
                decimal result = 0;
                decimal.TryParse(value.ToString("0.####"), out result);
                _quantity = result;
                SetPropertyChanged(nameof(Quantity));
                SetPropertyChanged(nameof(TotalReatilAmount));
                SetPropertyChanged(nameof(ProductTitleWithQuantity));
                SetPropertyChanged(nameof(RoundedQuantity));
            }
        }

        //Ticket start:#13971 Discrepancy between sales receipt, iPad's sales history, Web version by rupesh

        //[JsonProperty("Quantity")]
        [JsonIgnore]
        public string RoundedQuantity
        {
            get
            {
                return Quantity.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                //Ticket start:#14018 Issue with sale and exchange processed in iPad Hike App (IOS).by rupesh
                //Ticket start:#14403 iOS - Not Able to Change The Price for Refund. by rupesh
                decimal result = 0;
               // decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture,out result);
                decimal.TryParse(value, out result);
                Quantity = result;
                //Ticket end:#14403 by rupesh
                //Ticket End:#14018 by rupesh
                //Ticket End:#13971 by rupesh


            }
        }
        //Ticket end:#13971.by rupesh


        [JsonIgnore]
        string _strQuantity { get; set; }

        public string StrQuantity
        {
            get { return _strQuantity; }
            set
            {
                _strQuantity = value;
                SetPropertyChanged(nameof(StrQuantity));
            }
        }

        [JsonIgnore]
        public decimal RefundedQuantity { get; set; }

        [JsonProperty("RefundedQuantity")]
        public string RoundedRefundedQuantity
        {
            get
            {
                return RefundedQuantity.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value ?? "0", out result);
                RefundedQuantity = result;
            }
        }

        [JsonIgnore]
        decimal _soldPrice { get; set; }

        public decimal SoldPrice
        {
            get { return _soldPrice; }
            set
            {
                _soldPrice = value;
                SetPropertyChanged(nameof(SoldPrice));
                SetPropertyChanged(nameof(Price1));
                StrRetailPrice = _soldPrice.ToString("C");// String.Format("{0:0.00}", Math.Round(_soldPrice, 2));
                SetPropertyChanged(nameof(TotalReatilAmount));
            }
        }


        public string Price1
        {
            get { return string.Format("{0:C}", SoldPrice.ToString("C")); }
        }

        [JsonIgnore]
        decimal _totalAmount { get; set; }

        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                SetPropertyChanged(nameof(TotalAmount));
                SetPropertyChanged(nameof(IsTotalReatilAmountVisible));
            }
        }

        public decimal? BackOrderQty { get; set; }

        public int TaxId { get; set; }
        public string TaxName { get; set; }
        public decimal TaxRate { get; set; }

        [JsonIgnore]
        public decimal AdditionalLoyalty { get; set; }

        [JsonIgnore]
        decimal _taxAmount { get; set; }
        public decimal TaxAmount
        {
            get { return _taxAmount; }
            set
            {
                _taxAmount = value;
                SetPropertyChanged(nameof(TaxAmount));
            }
        }

        public string SalesCode { get; set; }

        [JsonIgnore]
        decimal _effectiveAmount { get; set; }
        public decimal EffectiveAmount
        {
            get { return _effectiveAmount; }
            set
            {
                _effectiveAmount = value;
                SetPropertyChanged(nameof(EffectiveAmount));
            }
        }

        decimal _taxExclusiveTotalAmount { get; set; }
        public decimal TaxExclusiveTotalAmount { get { return _taxExclusiveTotalAmount; } set { _taxExclusiveTotalAmount = value; SetPropertyChanged(nameof(TaxExclusiveTotalAmount)); } }
        public string GiftCardNumber { get; set; }
        public bool DiscountIsAsPercentage { get; set; }
        decimal _DiscountValue { get; set; }
        public decimal DiscountValue
        {
            get
            {
                return _DiscountValue;
            }
            set
            {
                _DiscountValue = value;
                StrDiscountValue = Math.Round(value, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero).ToString();
                //Start ticket#103384 
                // if (value < 0)
                //     MarkupValue = Math.Round(value , Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero) * -1;
                // else
                //     MarkupValue = 0;
                //End ticket#103384 
                SetPropertyChanged(nameof(DiscountValue));
            }
        }

        string _StrDiscountValue { get; set; }
        public string StrDiscountValue
        {
            get
            {
                return _StrDiscountValue;
            }
            set
            {
                _StrDiscountValue = value;
                SetPropertyChanged(nameof(StrDiscountValue));
            }
        }

        decimal? _MarkupValue;
        public decimal? MarkupValue
        {
            get
            {
                return _MarkupValue;
            }
            set
            {
                _MarkupValue = value;
                SetPropertyChanged(nameof(MarkupValue));
            }
        }


        decimal _ItemCost { get; set; }
        public decimal ItemCost
        {
            get
            {
                return _ItemCost;
            }
            set
            {
                _ItemCost = value;
                StrItemCost = value.ToString();
                SetPropertyChanged(nameof(ItemCost));
            }
        }
        //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
        public decimal AvgCostPrice { get; set; }
        //Ticket end:#20064 .by rupesh
        string _StrItemCost { get; set; }
        public string StrItemCost
        {
            get
            {
                return _StrItemCost;
            }
            set
            {
                _StrItemCost = value;
                SetPropertyChanged(nameof(StrItemCost));

            }
        }


        public decimal DiscountedQty { get; set; }

        [JsonIgnore]
        decimal _RetailPrice { get; set; }
        public decimal RetailPrice
        {
            get { return _RetailPrice; }
            set
            {
                _RetailPrice = value;
                SetPropertyChanged(nameof(RetailPrice));
                //StrRetailPrice = String.Format("{0:0.00}", Math.Round(_RetailPrice, 2));
                SetPropertyChanged(nameof(TotalReatilAmount));
            }
        }

        //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil
        [JsonIgnore]
        public decimal CustomSaleRetailPrice { get; set; }
        //Ticket #10921 End. By Nikhil

        [JsonIgnore]
        string _strRetailPrice { get; set; }

        [JsonIgnore]
        public string StrRetailPrice
        {
            get
            {
                return _strRetailPrice;
            }
            set
            {
                _strRetailPrice = value;
                SetPropertyChanged(nameof(StrRetailPrice));
            }
        }





        public decimal TotalDiscount { get; set; }
        public ActionType ActionType { get; set; }

        [JsonIgnore]
        string _offersNote { get; set; }

        public string OffersNote
        {
            get { return _offersNote; }
            set
            {
                _offersNote = value;
                SetPropertyChanged(nameof(OffersNote));
            }
        }
        [JsonIgnore]
        int? _offerId { get; set; }

        public int? OfferId
        {
            get { return _offerId; }
            set
            {
                _offerId = value;
                SetPropertyChanged(nameof(OfferId));
            }
        }

        public int? InvoiceItemValueParent { get; set; }
        public int? InvoiceExtraItemValueParent { get; set; }

        //public string Sku { get; set; }


        [JsonIgnore]
        string _Sku { get; set; }


        public string Sku
        {
            get { return _Sku; }
            set
            {
                _Sku = value;
                SetPropertyChanged(nameof(Sku));

            }
        }

        //Ticket #11211 Start : Sale Refund issue. By Nikhil
        [JsonIgnore]
        ObservableCollection<LineItemTaxDto> _lineItemTaxes { get; set; }
        public ObservableCollection<LineItemTaxDto> LineItemTaxes
        {
            get
            {
                if (_lineItemTaxes == null)
                    _lineItemTaxes = new ObservableCollection<LineItemTaxDto>();
                return _lineItemTaxes;
            }
            set { _lineItemTaxes = value; }
        }
        //Ticket #11211 End : By Nikhil

        public ObservableCollection<InvoiceLineSubItemDto> InvoiceLineSubItems { get; set; }

        [JsonIgnore]
        public bool SeperatorVisible
        {
            get
            {
                return InvoiceExtraItemValueParent == null;
            }
        }

        [JsonIgnore]
        public bool InveresSeperatorVisible => !SeperatorVisible;

        public int? RegisterId { get; set; }
        public String RegisterName { get; set; }
        public int? RegisterClosureId { get; set; }


        [JsonIgnore]
        public decimal TotalQuantity { get; set; }

        public decimal? CustomerGroupDiscountPercent { get; set; }

        public decimal CustomerGroupLoyaltyPoints { get; set; }


        public Decimal? OfferDiscountPercent { get; set; }


        public decimal actualqty { get; set; }


        [JsonIgnore]
        public string SKUWithLabel
        {
            get
            {

                if (!string.IsNullOrEmpty(Sku))
                    return "SKU: " + Sku;
                else
                    return string.Empty;
            }
        }


        [JsonIgnore]
        bool _IsExchangedProduct { get; set; }

        public bool IsExchangedProduct
        {
            get { return _IsExchangedProduct; }
            set
            {
                _IsExchangedProduct = value;
                SetPropertyChanged(nameof(IsExchangedProduct));


            }
        }

        //Ticket start:#24753 iOS - Product Barcode Missing in Receipts.by rupesh
        [JsonIgnore]
        string _Barcode { get; set; }
        public string Barcode
        {
            get { return _Barcode; }
            set
            {
                _Barcode = value;
                SetPropertyChanged(nameof(BarcodeWithLabel));

            }
        }
        [JsonIgnore]
        public string BarcodeWithLabel
        {
            get
            {

                if (!string.IsNullOrEmpty(Barcode))
                    return "Barcode: " + Barcode;
                else
                    return string.Empty;
            }
        }
        //Ticket end:#24753 .by rupesh

        public bool IsOfferAdded { get; set; }



        //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
        public bool IsReopenFromSaleHistory { get; set; } = false;
        public decimal ReopenQuantity { get; set; } = 0;
        //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales

        //Ticket start:#58490 iPad: FR For Pick and pack option.by rupesh
        [JsonIgnore]
        decimal _pickAndPackQuantity { get; set; }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        [JsonProperty("scanQuantity")]
        public decimal PickAndPackQuantity
        {
            get { return _pickAndPackQuantity; }
            set
            {
                _pickAndPackQuantity = value;
                SetPropertyChanged(nameof(PickAndPackQuantity));
                SetPropertyChanged(nameof(IsAllQuantityScanned));
            }
        }
       

        [JsonIgnore]
        public bool IsAllQuantityScanned
        {
            get
            {
                return Quantity == (PickAndPackQuantity + (string.IsNullOrEmpty(DisplayfulfillmentQuantity)?0:Convert.ToDecimal(DisplayfulfillmentQuantity)) ) ? true : false;
            }
        }
        //End #84293 by Pratik
        //Ticket end:#58490 .by rupesh

        //Ticket start:#60344 Tax Calculations not matched for Web Sale if sale is done on iOS.by rupesh
        //Ticket start:#60810 iOS Tax issue.by Pratik
        //Ticket start: #65926 Discounts are not working properly  by Pratikk
        //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
        [JsonIgnore]
        public decimal taxAmountNew { get; set; }

        [JsonIgnore]
        decimal? _discountOfferTax { get; set; }
        public decimal? DiscountOfferTax
        {
            get
            {
                if (InvoiceItemType == InvoiceItemType.Discount)
                    return null;
                if (_discountOfferTax.HasValue)
                    return _discountOfferTax;
                else
                {
                    if (OfferDiscountPercent.HasValue)
                    {
                        var amount = TaxAmount * (OfferDiscountPercent / 100) * -1;
                        return amount;
                    }
                    else
                        return null;
                }
            }
            set { _discountOfferTax = value; SetPropertyChanged(nameof(DiscountOfferTax)); }

        }

        [JsonIgnore]
        public decimal isDiscountAdded { get; set; }
        //End Ticket #74344 By pratik

        //Ticket start: #65926 Discounts are not working properly .by Rupesh
        [JsonIgnore]
        public decimal? DiscountOfferAmount
        {
            get
            {
                if (InvoiceItemType == InvoiceItemType.Discount)
                    return null;
                else if (OfferDiscountPercent.HasValue)
                {
                    var amount = TaxExclusiveTotalAmount * (OfferDiscountPercent / 100) * -1;
                    return amount;
                }
                else
                    return null;
            }
        }
        //Ticket end: #65926  .by Rupesh
        //Ticket end:#60810 .by Pratik
        //Ticket end:#60344.by rupesh
        //Ticket end: #65926  by Pratik

        //Ticket start:#68994,#73763 Discounts not working on iPad.by rupesh
        public bool DisableDiscountIndividually { get; set; }
        //Ticket end:#68994,#73763 .by rupesh
        //Ticket start:#71299 iPad - Feature: Adding the stock items manually in Pick and Pack.by rupesh
        [JsonIgnore]
        bool _IsToUpdatePickAndPack { get; set; }
        [JsonIgnore]
        public bool IsToUpdatePickAndPack { get { return _IsToUpdatePickAndPack; } set { _IsToUpdatePickAndPack = value; SetPropertyChanged(nameof(IsToUpdatePickAndPack)); } }
        //Ticket end:#71299 .by rupesh
        //Ticket:start:#71296 iPad - Feature: Product Loyalty point exclusion.by rupesh
        public bool DisableAdditionalLoyalty { get; set; }
        [JsonIgnore]
        bool _isEdited { get; set; }
        [JsonIgnore]
        public bool IsEdited
        {
            get { return _isEdited; }
            set
            {
                _isEdited = value;
                SetPropertyChanged(nameof(IsEdited));
            }
        }

        //Ticket:end:#71296 .by rupesh


        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public ObservableCollection<InvoiceItemFulfillmentDto> InvoiceItemFulfillments { get; set; }

        [JsonIgnore]
        public string DisplayfulfillmentQuantity => (InvoiceItemFulfillments == null ? 0 : InvoiceItemFulfillments.Sum(a => a.FulfillmentQuantity)).ToString("0.##");

        public decimal fulfillmentQuantity => ((DisplayfulfillmentQuantity != null ? Convert.ToDecimal(DisplayfulfillmentQuantity) : 0) + PickAndPackQuantity);

        [JsonIgnore]
        string _availableQuantity;
        [JsonIgnore]
        public string AvailableQuantity
        {
            get { return _availableQuantity; }
            set
            {
                _availableQuantity = value;
                SetPropertyChanged(nameof(AvailableQuantity));
            }
        }
        //End #84293 by Pratik

        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
        public ObservableCollection<InvoiceLineItemDetailDto> InvoiceLineItemDetails { get; set; }
        //Ticket:end:#90938 .by rupesh
        
        public InvoiceLineItemStatus Status { get; set; } //#94565

        //#95241
        public int? approvedByUser { get; set; }
        private string _approvedByUserName;
        public string approvedByUserName { get { return _approvedByUserName; } set { _approvedByUserName = value; SetPropertyChanged(nameof(approvedByUserName)); } }
        //#95241


        public InvoiceLineItemDB ToModel()
        {
            InvoiceLineItemDB invoiceLineItemDB = new InvoiceLineItemDB
            {
                Id = Id,
                IsActive = IsActive,
                giftCardInfo = giftCardInfo?.ToModel(),
                InvoiceId = InvoiceId,
                InvoiceItemType = (int)InvoiceItemType,
                InvoiceItemValue = InvoiceItemValue,
                InvoiceItemImage = InvoiceItemImage,
                Sequence = Sequence,
                IsSerialNumberEditableFromEntersale = IsSerialNumberEditableFromEntersale,
                EnableSerialNumber = EnableSerialNumber,
                SerialNumber = SerialNumber,
                Title = Title,
                Description = Description,
                Notes = Notes,
                Category = Category,
                CategoryDtos = CategoryDtos,
                Brand = Brand,
                Tags = Tags,
                Season = Season,
                Quantity = Quantity,
                StrQuantity = StrQuantity,
                RefundedQuantity = RefundedQuantity,
                RoundedRefundedQuantity = RoundedRefundedQuantity,
                SoldPrice = SoldPrice,
                TotalAmount = TotalAmount,
                BackOrderQty = BackOrderQty,
                TaxId = TaxId,
                TaxName = TaxName,
                TaxRate = TaxRate,
                TaxAmount = TaxAmount,
                SalesCode = SalesCode,
                EffectiveAmount = EffectiveAmount,
                TaxExclusiveTotalAmount = TaxExclusiveTotalAmount,
                GiftCardNumber = GiftCardNumber,
                DiscountIsAsPercentage = DiscountIsAsPercentage,
                DiscountValue = DiscountValue,
                StrDiscountValue = StrDiscountValue,
                MarkupValue = MarkupValue,
                ItemCost = ItemCost,
                AvgCostPrice = AvgCostPrice,
                StrItemCost = StrItemCost,
                DiscountedQty = DiscountedQty,
                RetailPrice = RetailPrice,
                TotalDiscount = TotalDiscount,
                ActionType = (int)ActionType,
                OffersNote = OffersNote,
                OfferId = OfferId,
                InvoiceItemValueParent = InvoiceItemValueParent,
                InvoiceExtraItemValueParent = InvoiceExtraItemValueParent,
                Sku = Sku,
                RegisterId = RegisterId,
                RegisterName = RegisterName,
                RegisterClosureId = RegisterClosureId,
                CustomerGroupDiscountPercent = CustomerGroupDiscountPercent,
                CustomerGroupLoyaltyPoints = CustomerGroupLoyaltyPoints,
                OfferDiscountPercent = OfferDiscountPercent,
                actualqty = actualqty,
                IsExchangedProduct = IsExchangedProduct,
                Barcode = Barcode,
                IsOfferAdded = IsOfferAdded,
                IsReopenFromSaleHistory = IsReopenFromSaleHistory,
                ReopenQuantity = ReopenQuantity,
                PickAndPackQuantity = PickAndPackQuantity,
                DisableDiscountIndividually = DisableDiscountIndividually,
                DisableAdditionalLoyalty = DisableAdditionalLoyalty,
                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                isDiscountAdded = isDiscountAdded,
                DiscountOfferTax = DiscountOfferTax,
                //End Ticket #74344 By pratik
                //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
                ServedByName = ServedByName,
                CreatorUserId = CreatorUserId,
                //end #84287 .by Pratik
                //Start #90941 iOS:FR: Custom filed on product detailpage By Pratik
                CustomField = CustomField,
                //End #90941 By Pratik
                approvedByUserName = approvedByUserName, //#95241
                approvedByUser = approvedByUser, //#95241
            };
            LineItemTaxes?.ForEach(i => invoiceLineItemDB.LineItemTaxes.Add(i.ToModel()));
            InvoiceLineSubItems?.ForEach(i => invoiceLineItemDB.InvoiceLineSubItems.Add(i.ToModel()));
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            InvoiceItemFulfillments?.ForEach(i => invoiceLineItemDB.InvoiceItemFulfillments.Add(i.ToModel()));
            //end #84293 by Pratik
            //Ticket:start:#90938 IOS:FR Age varification.by rupesh
            InvoiceLineItemDetails?.ForEach(i => invoiceLineItemDB.InvoiceLineItemDetails.Add(i.ToModel()));
            //Ticket:end:#90938 .by rupesh

            return invoiceLineItemDB;
        }
        public static InvoiceLineItemDto FromModel(InvoiceLineItemDB invoiceLineItemDB)
        {
            if (invoiceLineItemDB == null)
                return null;
            InvoiceLineItemDto invoiceLineItemDto = new InvoiceLineItemDto
            {
                Id = invoiceLineItemDB.Id,
                IsActive = invoiceLineItemDB.IsActive,
                giftCardInfo = invoiceLineItemDB.giftCardInfo != null ? GiftCardInfo.FromModel(invoiceLineItemDB.giftCardInfo) : null,
                InvoiceId = invoiceLineItemDB.InvoiceId,
                InvoiceItemType = (InvoiceItemType)invoiceLineItemDB.InvoiceItemType,
                InvoiceItemValue = invoiceLineItemDB.InvoiceItemValue,
                InvoiceItemImage = invoiceLineItemDB.InvoiceItemImage,
                Sequence = invoiceLineItemDB.Sequence,
                IsSerialNumberEditableFromEntersale = invoiceLineItemDB.IsSerialNumberEditableFromEntersale,
                EnableSerialNumber = invoiceLineItemDB.EnableSerialNumber,
                SerialNumber = invoiceLineItemDB.SerialNumber,
                Title = invoiceLineItemDB.Title,
                Description = invoiceLineItemDB.Description,
                Notes = invoiceLineItemDB.Notes,
                Category = invoiceLineItemDB.Category,
                CategoryDtos = invoiceLineItemDB.CategoryDtos,
                Brand = invoiceLineItemDB.Brand,
                Tags = invoiceLineItemDB.Tags,
                Season = invoiceLineItemDB.Season,
                Quantity = invoiceLineItemDB.Quantity,
                StrQuantity = invoiceLineItemDB.StrQuantity,
                RefundedQuantity = invoiceLineItemDB.RefundedQuantity,
                RoundedRefundedQuantity = invoiceLineItemDB.RoundedRefundedQuantity,
                SoldPrice = invoiceLineItemDB.SoldPrice,
                TotalAmount = invoiceLineItemDB.TotalAmount,
                BackOrderQty = invoiceLineItemDB.BackOrderQty,
                TaxId = invoiceLineItemDB.TaxId,
                TaxName = invoiceLineItemDB.TaxName,
                TaxRate = invoiceLineItemDB.TaxRate,
                TaxAmount = invoiceLineItemDB.TaxAmount,
                SalesCode = invoiceLineItemDB.SalesCode,
                EffectiveAmount = invoiceLineItemDB.EffectiveAmount,
                TaxExclusiveTotalAmount = invoiceLineItemDB.TaxExclusiveTotalAmount,
                GiftCardNumber = invoiceLineItemDB.GiftCardNumber,
                DiscountIsAsPercentage = invoiceLineItemDB.DiscountIsAsPercentage,
                DiscountValue = invoiceLineItemDB.DiscountValue,
                StrDiscountValue = invoiceLineItemDB.StrDiscountValue,
                MarkupValue = invoiceLineItemDB.MarkupValue,
                ItemCost = invoiceLineItemDB.ItemCost,
                AvgCostPrice = invoiceLineItemDB.AvgCostPrice,
                StrItemCost = invoiceLineItemDB.StrItemCost,
                DiscountedQty = invoiceLineItemDB.DiscountedQty,
                RetailPrice = invoiceLineItemDB.RetailPrice,
                TotalDiscount = invoiceLineItemDB.TotalDiscount,
                ActionType = (ActionType)invoiceLineItemDB.ActionType,
                OffersNote = invoiceLineItemDB.OffersNote,
                OfferId = invoiceLineItemDB.OfferId,
                InvoiceItemValueParent = invoiceLineItemDB.InvoiceItemValueParent,
                InvoiceExtraItemValueParent = invoiceLineItemDB.InvoiceExtraItemValueParent,
                Sku = invoiceLineItemDB.Sku,
                RegisterId = invoiceLineItemDB.RegisterId,
                RegisterName = invoiceLineItemDB.RegisterName,
                RegisterClosureId = invoiceLineItemDB.RegisterClosureId,
                CustomerGroupDiscountPercent = invoiceLineItemDB.CustomerGroupDiscountPercent,
                CustomerGroupLoyaltyPoints = invoiceLineItemDB.CustomerGroupLoyaltyPoints,
                OfferDiscountPercent = invoiceLineItemDB.OfferDiscountPercent,
                actualqty = invoiceLineItemDB.actualqty,
                IsExchangedProduct = invoiceLineItemDB.IsExchangedProduct,
                Barcode = invoiceLineItemDB.Barcode,
                IsOfferAdded = invoiceLineItemDB.IsOfferAdded,
                IsReopenFromSaleHistory = invoiceLineItemDB.IsReopenFromSaleHistory,
                ReopenQuantity = invoiceLineItemDB.ReopenQuantity,
                PickAndPackQuantity = invoiceLineItemDB.PickAndPackQuantity,
                DisableDiscountIndividually = invoiceLineItemDB.DisableDiscountIndividually,
                DisableAdditionalLoyalty = invoiceLineItemDB.DisableAdditionalLoyalty,
                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                isDiscountAdded = invoiceLineItemDB.isDiscountAdded,
                DiscountOfferTax = invoiceLineItemDB.DiscountOfferTax,
                //End Ticket #74344 By pratik
                //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
                ServedByName = invoiceLineItemDB.ServedByName,
                CreatorUserId = invoiceLineItemDB.CreatorUserId,
                //end #84287 .by Pratik
                //Start #90941 iOS:FR: Custom filed on product detailpage By Pratik
                CustomField = invoiceLineItemDB.CustomField,
                //End #90941 By Pratik
                approvedByUserName = invoiceLineItemDB.approvedByUserName, //#95241
                approvedByUser = invoiceLineItemDB.approvedByUser, //#95241
            };
            if (invoiceLineItemDB.LineItemTaxes != null)
                invoiceLineItemDto.LineItemTaxes = new ObservableCollection<LineItemTaxDto>(invoiceLineItemDB.LineItemTaxes.Select(a => LineItemTaxDto.FromModel(a)));
            if (invoiceLineItemDB.InvoiceLineSubItems != null)
                invoiceLineItemDto.InvoiceLineSubItems = new ObservableCollection<InvoiceLineSubItemDto>(invoiceLineItemDB.InvoiceLineSubItems.Select(a => InvoiceLineSubItemDto.FromModel(a)));
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            if (invoiceLineItemDB.InvoiceItemFulfillments != null)
                invoiceLineItemDto.InvoiceItemFulfillments = new ObservableCollection<InvoiceItemFulfillmentDto>(invoiceLineItemDB.InvoiceItemFulfillments.Select(a => InvoiceItemFulfillmentDto.FromModel(a)));
            //End #84293 by Pratik
            //Ticket:start:#90938 IOS:FR Age varification.by rupesh
            if (invoiceLineItemDB.InvoiceLineItemDetails != null)
                invoiceLineItemDto.InvoiceLineItemDetails = new ObservableCollection<InvoiceLineItemDetailDto>(invoiceLineItemDB.InvoiceLineItemDetails.Select(a => InvoiceLineItemDetailDto.FromModel(a)));
            //Ticket:end:#90938 .by rupesh
            return invoiceLineItemDto;

        }

    }

    public class GiftCardInfo
    {
        public bool addedTopUp { get; set; }

        //#28721 Email for Sending Gift Cards
        public string recipientName { get; set; }
        public string recipientEmail { get; set; }
        public string recipientMessage { get; set; }
        public string fromName { get; set; }
        public string fromEmail { get; set; }
        //#28721 Email for Sending Gift Cards
        public GiftCardInfoDB ToModel()
        {
            GiftCardInfoDB giftCardInfoDB = new GiftCardInfoDB
            {
                addedTopUp = addedTopUp,
                recipientName = recipientName,
                recipientEmail = recipientEmail,
                recipientMessage = recipientMessage,
                fromName = fromName,
                fromEmail = fromEmail

            };
            return giftCardInfoDB;
       }
        public static GiftCardInfo FromModel(GiftCardInfoDB giftCardInfoDB)
        {
            if (giftCardInfoDB == null)
                return null;
            GiftCardInfo giftCardInfo = new GiftCardInfo
            {
                addedTopUp = giftCardInfoDB.addedTopUp,
                recipientName = giftCardInfoDB.recipientName,
                recipientEmail = giftCardInfoDB.recipientEmail,
                recipientMessage = giftCardInfoDB.recipientMessage,
                fromName = giftCardInfoDB.fromName,
                fromEmail = giftCardInfoDB.fromEmail

            };
            return giftCardInfo;

        }

    }
   
    public partial class InvoiceLineItemDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public GiftCardInfoDB giftCardInfo { get; set; }

        public int InvoiceId { get; set; }
        public int InvoiceItemType { get; set; }
        public int InvoiceItemValue { get; set; }
        public string InvoiceItemImage { get; set; }
        public int Sequence { get; set; }

        public bool IsSerialNumberEditableFromEntersale { get; set; }

        public bool EnableSerialNumber { get; set; }
        public string SerialNumber { get; set; }
        public string Title { get; set; }


        public string Description { get; set; }

        public string Notes { get; set; }

        public string Category { get; set; }
        public string CategoryDtos { get; set; }

        public string Brand { get; set; }
        public string Tags { get; set; }
        public string Season { get; set; }

        public decimal Quantity { get; set; }

        public string StrQuantity { get; set; }
        public decimal RefundedQuantity { get; set; }

        public string RoundedRefundedQuantity { get; set; }

        public decimal SoldPrice { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal? BackOrderQty { get; set; }

        public int TaxId { get; set; }
        public string TaxName { get; set; }
        public decimal TaxRate { get; set; }

        public decimal TaxAmount { get; set; }

        public string SalesCode { get; set; }

        public decimal EffectiveAmount { get; set; }
        public decimal TaxExclusiveTotalAmount { get; set; }
        public string GiftCardNumber { get; set; }
        public bool DiscountIsAsPercentage { get; set; }
        public decimal DiscountValue { get; set; }
        public string StrDiscountValue { get; set; }
        public decimal? MarkupValue { get; set; }
        public decimal ItemCost { get; set; }
        public decimal AvgCostPrice { get; set; }
        public string StrItemCost { get; set; }
        public decimal DiscountedQty { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public int ActionType { get; set; }

        public string OffersNote { get; set; }

        public int? OfferId { get; set; }
        public int? InvoiceItemValueParent { get; set; }
        public int? InvoiceExtraItemValueParent { get; set; }

        public string Sku { get; set; }

        public IList<LineItemTaxDB> LineItemTaxes { get; }

        public IList<InvoiceLineSubItemDB> InvoiceLineSubItems { get; }


        public int? RegisterId { get; set; }
        public String RegisterName { get; set; }
        public int? RegisterClosureId { get; set; }


        public decimal? CustomerGroupDiscountPercent { get; set; }

        public decimal CustomerGroupLoyaltyPoints { get; set; }


        public Decimal? OfferDiscountPercent { get; set; }


        public decimal actualqty { get; set; }


        public bool IsExchangedProduct { get; set; }

        public string Barcode { get; set; }

        public bool IsOfferAdded { get; set; }


        public bool IsReopenFromSaleHistory { get; set; }
        public decimal ReopenQuantity { get; set; }
        public decimal PickAndPackQuantity { get; set; }
        public bool DisableDiscountIndividually { get; set; }
        public bool DisableAdditionalLoyalty { get; set; }
        //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
        public decimal isDiscountAdded { get; set; }
        public decimal? DiscountOfferTax { get; set; }
        //End Ticket #74344 By pratik

        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        public string ServedByName { get; set; }
        public int? CreatorUserId { get; set; }
        //end #84287 .by Pratik

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public IList<InvoiceItemFulfillmentDB> InvoiceItemFulfillments { get; }
        //End #84293 by Pratik
        //Start #90941 iOS:FR: Custom filed on product detailpage By Pratik
        public string CustomField { get; set; }
        //End #90941 By Pratik
        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
        public IList<InvoiceLineItemDetailDB> InvoiceLineItemDetails { get; }
        //Ticket:end:#90938 .by rupesh

        //#95241
        public int? approvedByUser { get; set; }
        public string approvedByUserName { get; set; }
        //#95241
    }

    public partial class GiftCardInfoDB : IRealmObject
    {
        public bool addedTopUp { get; set; }

        //#28721 Email for Sending Gift Cards
        public string recipientName { get; set; }
        public string recipientEmail { get; set; }
        public string recipientMessage { get; set; }
        public string fromName { get; set; }
        public string fromEmail { get; set; }
        //#28721 Email for Sending Gift Cards
    }

}
