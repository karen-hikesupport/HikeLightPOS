using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models.Enum;
using HikePOS.Resources;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public class InvoiceDto : FullAuditedPassiveEntityDto
    {
        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        [JsonIgnore]
        public EventHandler<decimal> AmountChanged;
        //End ticket #73190 By Pratik
        public InvoiceDto()
        {
            InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
            InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
            InvoiceHistories = new ObservableCollection<InvoiceHistoryDto>();
            Taxgroup = new ObservableCollection<LineItemTaxDto>();
            BackorderPayments = new ObservableCollection<InvoicePaymentDto>();
        }

        [JsonProperty("SyncReference")]
        public string InvoiceTempId { get; set; }

        public string KeyId { get; set; }

        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.

        [JsonIgnore]
        bool _IsSerialNumberEditableFromSaleHistory { get; set; }

        public bool IsSerialNumberEditableFromSaleHistory { get { return _IsSerialNumberEditableFromSaleHistory; } set { _IsSerialNumberEditableFromSaleHistory = value; SetPropertyChanged(nameof(IsSerialNumberEditableFromSaleHistory)); } }

        //[JsonIgnore]
        //public bool IsSerialNumberEditableFromSaleHistory { get; set; } = false;
        //#36895 iPad: Feature request - serial number option is enable for completed sale came from woo to hike.

        #region Below property for display app

        public int TanentId { get; set; } = 0;

        public bool IscallFromPayment { get; set; } = false;

        public InvoiceUpdateFrom InvoiceUpdateFrom { get; set; }

        #endregion



        [JsonIgnore]
        string _Number { get; set; }

        public string Number { get { return _Number; } set { _Number = value; SetPropertyChanged(nameof(Number)); } }

        //Ticket start:#13735 iOS - Performance of Sales History by rupesh
        [JsonIgnore]
        public DateTime _TransactionDate { get; set; }
        public DateTime TransactionDate
        {
            get
            {
                return _TransactionDate;
            }
            set
            {
                _TransactionDate = value;
                TransactionStoreDate = TransactionDate.ToStoreTime();
            }
        }

        [JsonIgnore]
        public DateTime TransactionStoreDate { get; set; }
        // public DateTime TransactionStoreDate { get { return TransactionDate.ToStoreTime(); } }
        //Ticket end:#13735 iOS 

        [JsonIgnore]
        int? _CustomerId { get; set; }

        public int? CustomerId { get { return _CustomerId; } set { _CustomerId = value; SetPropertyChanged(nameof(CustomerId)); } }


        // Start #73186 iPad  :iPad - Lay-by completion date option: Same as parked sale By Pratik
        [JsonIgnore]
        public DateTime FinalizeDateStoreDate => TransactionDate.ToStoreTime();
        // End ##73186 iPad By Pratik
        //{
        //    get
        //    {
        //        // Start #73186 iPad  :iPad - Lay-by completion date option: Same as parked sale By Pratik
        //        //Start #40634 iPad  :: Feature request - About How to Handle Transaction Date of Parked Sales
        //        //if (FinalizeDate != null)
        //        //{
        //        //    return FinalizeDate.Value.ToStoreTime();
        //        //}
        //        //else
        //        //{
        //        //return TransactionDate.ToStoreTime();
        //        //}
        //        //#40634 iPad: End by nutan
        //        // End ##73186 iPad By Pratik

        //    }
        //}

        //[JsonIgnore]
        public string CustomerTempId { get; set; }

        [JsonIgnore]
        CustomerDto_POS _customerDetail { get; set; }

        public CustomerDto_POS CustomerDetail
        {
            get
            {
                return _customerDetail;
            }
            set
            {
                _customerDetail = value;
                SetPropertyChanged(nameof(CustomerDetail));

            }
        }

        [JsonIgnore]
        string _customerName { get; set; }

        public string CustomerGroupName { get; set; }

        public string CustomerName
        {
            get
            {
                return _customerName;
            }
            set
            {
                _customerName = value;
                SetPropertyChanged(nameof(CustomerName));
            }
        }

        public int? CustomerGroupId { get; set; }
        public decimal? CustomerGroupDiscount { get; set; }

        [JsonIgnore]
        string _customerGroupDiscountNote { get; set; }

        public string CustomerGroupDiscountNote
        {
            get { return _customerGroupDiscountNote; }
            set
            {
                _customerGroupDiscountNote = value;
                SetPropertyChanged(nameof(CustomerGroupDiscountNote));
            }
        }

        public string CustomerGroupDiscountNoteInside { get; set; }
        public decimal CustomerGroupDiscountNoteInsidePrice { get; set; }

        public int OutletId { get; set; }

        public string OutletName { get; set; }
        public int? RegisterId { get; set; }
        public string RegisterName { get; set; }

        public string Barcode { get; set; }

        public bool IsExchangeSale { get; set; } //#97493

        [JsonIgnore]
        InvoiceStatus _Status { get; set; }
        public InvoiceStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                SetPropertyChanged(nameof(Status));
                //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                SetPropertyChanged(nameof(StatusText));
                SetPropertyChanged(nameof(StatusColor));
                //End #84293 by Pratik
                SetPropertyChanged(nameof(StatusHeight));
            }
        }

        [JsonIgnore]
        public double StatusHeight => (Status == InvoiceStatus.Refunded || Status == InvoiceStatus.Exchange) ? 40 : 0;

        [JsonIgnore]
        public Color StatusColor
        {
            get
            {
                var intstatus = (int)Status;
                //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                if (intstatus == 0 || intstatus == 1 || intstatus == 5 || intstatus == 6 || intstatus == 11 || intstatus == 12 || intstatus == 16) //#94565
                {
                    //End #84293 by Pratik
                    return AppColors.ProductOrangeColor;
                }
                else if (intstatus == 2 || intstatus == 7)
                    return AppColors.HikeColor;
                else
                    return AppColors.ProductGrayColor;
            }
        }

        [JsonIgnore]
        LocalInvoiceStatus _LocalInvoiceStatus { get; set; }
        public LocalInvoiceStatus LocalInvoiceStatus
        {
            get
            {
                return _LocalInvoiceStatus;
            }
            set
            {
                _LocalInvoiceStatus = value;
                SetPropertyChanged(nameof(LocalInvoiceStatus));
            }
        }



        string _CurrentPaymentObject { get; set; }



        public string CurrentPaymentObject
        {
            get
            {
                return _CurrentPaymentObject;
            }
            set
            {
                _CurrentPaymentObject = value;
                SetPropertyChanged(nameof(CurrentPaymentObject));
            }
        }

        public bool TaxInclusive { get; set; }

        public bool ApplyTaxAfterDiscount { get; set; }

        [JsonIgnore]
        bool _discountIsAsPercentage { get; set; }

        public bool DiscountIsAsPercentage
        {
            get { return _discountIsAsPercentage; }
            set
            {
                _discountIsAsPercentage = value;
                SetPropertyChanged(nameof(DiscountIsAsPercentage));
            }
        }

        [JsonIgnore]
        decimal _discountValue { get; set; }

        public decimal DiscountValue
        {
            get { return _discountValue; }
            set
            {
                //_discountValue = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                _discountValue = value;
                SetPropertyChanged(nameof(DiscountValue));
            }
        }

        [JsonIgnore]
        string _discoutType { get; set; }
        public string DiscoutType
        {
            get { return _discoutType; }
            set
            {
                _discoutType = value;
                SetPropertyChanged(nameof(DiscoutType));
            }
        }

        [JsonIgnore]
        string _discountNote { get; set; }

        public string DiscountNote
        {
            get { return _discountNote; }
            set
            {
                _discountNote = value;
                SetPropertyChanged(nameof(DiscountNote));
            }
        }

        [JsonIgnore]
        bool _tipIsAsPercentage { get; set; }

        public bool TipIsAsPercentage
        {
            get { return _tipIsAsPercentage; }
            set
            {
                _tipIsAsPercentage = value;
                SetPropertyChanged(nameof(TipIsAsPercentage));
            }
        }

        [JsonIgnore]
        decimal _tipValue { get; set; } = 0;

        public decimal TipValue
        {
            get { return _tipValue; }
            set
            {
                _tipValue = value;
                SetPropertyChanged(nameof(TipValue));
            }
        }



        [JsonIgnore]
        decimal _subTotal { get; set; } = 0;

        public decimal SubTotal
        {
            get { return _subTotal; }
            set
            {
                _subTotal = value;
                SetPropertyChanged(nameof(SubTotal));
            }
        }

        public decimal GiftCardTotal { get; set; }

        [JsonIgnore]
        decimal _totalDiscount { get; set; } = 0;

        public decimal TotalDiscount
        {
            get { return _totalDiscount; }
            set
            {
                _totalDiscount = value;
                SetPropertyChanged(nameof(TotalDiscount));
            }
        }

        [JsonIgnore]
        decimal _totalShippingCost { get; set; } = 0;

        public decimal TotalShippingCost
        {
            get { return _totalShippingCost; }
            set
            {
                _totalShippingCost = value;
                SetPropertyChanged(nameof(TotalShippingCost));
            }
        }

        [JsonIgnore]
        int? _shippingTaxId { get; set; } = 0;

        public int? shippingTaxId
        {
            get { return _shippingTaxId; }
            set
            {
                _shippingTaxId = value;
                SetPropertyChanged(nameof(shippingTaxId));
            }
        }

        [JsonIgnore]
        decimal? _shippingTaxRate { get; set; } = 0;

        public decimal? ShippingTaxRate
        {
            get { return _shippingTaxRate; }
            set
            {
                _shippingTaxRate = value;
                SetPropertyChanged(nameof(ShippingTaxRate));
            }
        }


        [JsonIgnore]
        string _shippingTaxName { get; set; }

        public string ShippingTaxName
        {
            get { return _shippingTaxName; }
            set
            {
                _shippingTaxName = value;
                SetPropertyChanged(nameof(ShippingTaxName));
            }
        }


        [JsonIgnore]
        decimal? _shippingTaxAmount { get; set; } = 0;

        public decimal? ShippingTaxAmount
        {
            get { return _shippingTaxAmount; }
            set
            {
                _shippingTaxAmount = value;
                SetPropertyChanged(nameof(ShippingTaxAmount));
            }
        }
        //Ticket start:#33812 iPad: New Feature Request :: Shipping charge showing Tax Exclusive in sales history page.by rupesh
        [JsonIgnore]
        public decimal? ShippingTaxAmountExclusive
        {
            get { return TotalShippingCost - _shippingTaxAmount; }
        }

        [JsonIgnore]
        public decimal? _TotalTax { get; set; }
        public decimal? TotalTax
        {
            get
            {   //#34750 iPad: invoice level discount is not working properly. :  Apply rounding
                //Ticket start:#42060 iPad: Tax rounding issue is available in Kuwait (English).by rupesh
                //Ticket start:#49654 Small difference in Sales (inc tax) amount on Sale summery page.by rupesh
                return Math.Round(Tax, 4) + Math.Round(TipTaxAmount, 4) + ShippingTaxAmount;
                //Ticket end:#49654.by rupesh
                //Ticket end:#42060.by rupesh
            }
            set
            {
                _TotalTax = value;
                SetPropertyChanged(nameof(TotalTax));

            }
        }
        //Ticket end:#33812 .by rupesh

        // public int shippingTaxId { get; set; }
        // public double shippingTaxRate { get; set; }
        // public string shippingTaxName { get; set; }
        // public double shippingTaxAmount { get; set; }



        [JsonIgnore]
        decimal _otherCharges { get; set; } = 0;

        public decimal OtherCharges
        {
            get { return _otherCharges; }
            set
            {
                _otherCharges = value;
                SetPropertyChanged(nameof(OtherCharges));
            }
        }

        //Ticket start:#34913 iPad: Tax is not reflect properly in iPad to web, web to iPad.by rupesh
        //Ticket start:#52177 Sale discount error.by rupesh
        [JsonIgnore]
        decimal? _Tax { get; set; }

        public decimal Tax
        {
            get
            {
                if (_Tax == null && _TotalTax != null)
                    _Tax = (decimal)(_TotalTax - TipTaxAmount - ShippingTaxAmount);
                return _Tax ?? 0;
            }
            set
            {
                _Tax = value;
                SetPropertyChanged(nameof(Tax));
            }
        }
        //Ticket end:#52177 .by rupesh
        //Ticket end:#34913 .by rupesh

        [JsonIgnore]
        decimal _totalTip { get; set; } = 0;


        public decimal TotalTip
        {
            get { return _totalTip; }
            set
            {
                _totalTip = value;
                SetPropertyChanged(nameof(TotalTip));
            }
        }

        //Ticket start:#36019 iPad: Surcharge amount showing different in print receipt.by rupesh
        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        [JsonIgnore]
        public decimal TotalTipTaxExclusive
        {
            get { return (TotalTip + TotalPaymentSurcharge) - TipTaxAmount; }
        }
        //Ticket end:#36019 .by rupesh
        //End Ticket #73190 By: Pratik

        [JsonIgnore]
        decimal _roundingAmount { get; set; } = 0;

        public decimal RoundingAmount
        {
            get { return _roundingAmount; }
            set
            {
                _roundingAmount = value;
                SetPropertyChanged(nameof(RoundingAmount));
            }
        }

        [JsonIgnore]
        decimal _netAmount { get; set; } = 0;

        public decimal NetAmount
        {
            get { return _netAmount; }
            set
            {
                //_netAmount = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                _netAmount = value;
                SetPropertyChanged(nameof(NetAmount));
                SetPropertyChanged(nameof(OutstandingAmount));
                SetPropertyChanged(nameof(OutstandingText));
                SetPropertyChanged(nameof(IsPartialPaid));
            }
        }

        [JsonIgnore]
        decimal _totalPaid { get; set; } = 0;

        public decimal TotalPaid
        {
            get { return _totalPaid; }
            set
            {
                //_totalPaid = Math.Round(value, 2, MidpointRounding.AwayFromZero);;
                _totalPaid = value;
                SetPropertyChanged(nameof(TotalPaid));
                SetPropertyChanged(nameof(IsPartialPaid));
            }
        }

        [JsonIgnore]
        decimal _totalPay { get; set; }

        public decimal TotalPay
        {
            get { return _totalPay; }
            set
            {
                _totalPay = value;
                StrTenderAmount = _totalPay.ToString("C");
                SetPropertyChanged(nameof(TotalPay));
                SetPropertyChanged(nameof(InvoiceLineItemsCnt));
                SetPropertyChanged(nameof(GroupInvoiceLineItems));
            }
        }

        [JsonIgnore]
        decimal _TenderAmount { get; set; }

        public decimal TenderAmount
        {
            get
            {
                return _TenderAmount;
            }
            set
            {
                if (value != _TenderAmount)
                {
                    //_TenderAmount = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                    _TenderAmount = value;
                    SetPropertyChanged(nameof(TenderAmount));
                    //StrTenderAmount = TenderAmount.ToString("C");
                    UpdateQuickCashOption(value);
                    //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                    AmountChanged?.Invoke(this, TenderAmount);
                    //End ticket #73190 By Pratik
                }
            }
        }

        [JsonIgnore]
        string _strTenderAmount { get; set; }

        public string StrTenderAmount
        {
            get
            {
                return _strTenderAmount;
            }
            set
            {
                _strTenderAmount = value;
                if (!string.IsNullOrEmpty(value))
                {
                    decimal result;
                    if (decimal.TryParse(value, NumberStyles.Currency, null, out result))
                        TenderAmount = result;
                    else
                        TenderAmount = 0;
                }
                else
                    TenderAmount = 0;
                SetPropertyChanged(nameof(StrTenderAmount));
            }
        }

        [JsonIgnore]
        decimal _QuickCashFirstOption { get; set; }
        public decimal QuickCashFirstOption { get { return _QuickCashFirstOption; } set { _QuickCashFirstOption = value; SetPropertyChanged(nameof(QuickCashFirstOption)); } }

        [JsonIgnore]
        decimal _QuickCashSecondOption { get; set; }
        public decimal QuickCashSecondOption { get { return _QuickCashSecondOption; } set { _QuickCashSecondOption = value; SetPropertyChanged(nameof(QuickCashSecondOption)); } }

        [JsonIgnore]
        decimal _QuickCashThirdOption { get; set; }
        public decimal QuickCashThirdOption { get { return _QuickCashThirdOption; } set { _QuickCashThirdOption = value; SetPropertyChanged(nameof(QuickCashThirdOption)); } }

        [JsonIgnore]
        decimal _QuickCashFourOption { get; set; }
        public decimal QuickCashFourOption { get { return _QuickCashFourOption; } set { _QuickCashFourOption = value; SetPropertyChanged(nameof(QuickCashFourOption)); } }

        public OnAccountPONumberRequest InvoiceDetail { get; set; } //Start #91991 By Pratik

        [JsonIgnore]
        decimal _totalTender { get; set; }

        public decimal TotalTender { get; set; }

        [JsonIgnore]
        decimal _changeAmount { get; set; }

        public decimal ChangeAmount
        {
            get { return _changeAmount; }
            set
            {
                //_changeAmount = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                _changeAmount = value;
                SetPropertyChanged(nameof(ChangeAmount));
            }
        }

        //decimal _outstandingAmount { get; set; }
        //public decimal OutstandingAmount
        //{
        //	get { return _outstandingAmount; }
        //	set
        //	{
        //		_outstandingAmount = value;
        //		SetPropertyChanged(nameof(OutstandingAmount));
        //	}
        //}




        public string Currency { get; set; }

        public int ServedBy { get; set; }
        public string ServedByName { get; set; }

        [JsonIgnore]
        string _note { get; set; }

        public string Note
        {
            get { return _note; }
            set
            {
                _note = value;
                SetPropertyChanged(nameof(Note));
            }
        }

        //Ticket start:#35165 iOS - Wrong Source in Sale History of Quotes After Converted to Sales.by rupesh
        public InvoiceFrom? InvoiceFrom { get; set; }
        //Ticket end:#35165 .by rupesh
        public string InvoiceFromName { get; set; }


        public int? ReferenceInvoiceId { get; set; }

        //[JsonIgnore]
        public string ReferenceTempInvoiceId { get; set; }

        public string ReferenceNote { get; set; }

        public string ExchangeReferenceNote { get; set; }


        [JsonIgnore]
        bool _IsReStockWhenRefund { get; set; }

        public bool IsReStockWhenRefund { get { return _IsReStockWhenRefund; } set { _IsReStockWhenRefund = value; SetPropertyChanged(nameof(IsReStockWhenRefund)); } }

        public bool CanBeModified { get; set; }

        public string TrackNumber { get; set; }
        public string TrackURL { get; set; }
        public string TrackDetails { get; set; }
        public string ReceiptHTML { get; set; }

        ObservableCollection<InvoiceLineItemDto> _invoiceLineItems;
        public ObservableCollection<InvoiceLineItemDto> InvoiceLineItems
        {
            get { return _invoiceLineItems; }
            set
            {
                _invoiceLineItems = value;
                SetPropertyChanged(nameof(InvoiceLineItems));
            }
        }

        [JsonIgnore]
        public string InvoiceLineItemsCnt => InvoiceLineItems == null ? "0" : InvoiceLineItems.Where(item => (Status == InvoiceStatus.Refunded ? item.Quantity < 0 : item.Quantity > 0) && (item.InvoiceItemType == Enums.InvoiceItemType.Standard
                            || item.InvoiceItemType == Enums.InvoiceItemType.Custom
                            || item.InvoiceItemType == Enums.InvoiceItemType.Composite
                            || item.InvoiceItemType == Enums.InvoiceItemType.CompositeProduct
                            || item.InvoiceItemType == Enums.InvoiceItemType.UnityOfMeasure
                            || item.InvoiceItemType == Enums.InvoiceItemType.GiftCard)).Sum(a => a.Quantity).ToPositive().ToString("0.####");

        [JsonIgnore]
        decimal _discountPercentValue;
        [JsonIgnore]
        public decimal DiscountPercentValue
        {
            get { return _discountPercentValue; }
            set
            {
                _discountPercentValue = value;
                SetPropertyChanged(nameof(DiscountPercentValue));
            }
        }

        [JsonIgnore]
        public object GroupInvoiceLineItems
        {
            get
            {
                if (Settings.StoreGeneralRule.ShowGroupProductsByCategory)
                {
                    var groupLineiItemGroupList = new ObservableCollection<InvoiceLineiItemGroup>();
                    if (InvoiceLineItems != null && Settings.StoreGeneralRule != null)
                    {
                        var items = InvoiceLineItems;
                        var groupeditemsList = items.Where(x => !x.IsExtraproduct).GroupBy(u => u.categoryId).Select(grp => grp.ToList()).ToList();
                        var isextra = items.Any(a => a.IsExtraproduct);
                        foreach (var groupedItem in groupeditemsList)
                        {
                            var invoiceLineiItemGroup = new InvoiceLineiItemGroup();
                            invoiceLineiItemGroup.Title = "NONE";
                            var firstItem = groupedItem.FirstOrDefault();
                            if (firstItem?.CategoryDtos != null)
                            {
                                var productCategories = JsonConvert.DeserializeObject<List<CategoryDto>>(firstItem.CategoryDtos);
                                if (productCategories != null && productCategories.Count > 0)
                                {
                                    invoiceLineiItemGroup.Title = productCategories[0].Name?.ToUpper();
                                }
                            }

                            if (isextra)
                            {
                                foreach (var item in groupedItem)
                                {
                                    invoiceLineiItemGroup.Add(item);
                                    if (item.HasExtraproduct || items.Any(x => x.InvoiceExtraItemValueParent == item.Sequence))
                                    {
                                        var extraProductsItems = items.Where(x => x.InvoiceExtraItemValueParent == item.Sequence);
                                        foreach (var e in extraProductsItems)
                                        {
                                            invoiceLineiItemGroup.Add(e);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                invoiceLineiItemGroup = new InvoiceLineiItemGroup(invoiceLineiItemGroup.Title, groupedItem);
                            }
                            groupLineiItemGroupList.Add(invoiceLineiItemGroup);
                        }
                    }
                    return groupLineiItemGroupList;
                }
                else
                    return InvoiceLineItems;
            }
        }

        public ObservableCollection<InvoicePaymentDto> InvoicePayments1 { get; set; }
        //public ObservableCollection<InvoicePaymentDto> InvoicePayments { get; set; }

        ObservableCollection<InvoicePaymentDto> _invoicePayments { get; set; }
        public ObservableCollection<InvoicePaymentDto> InvoicePayments
        {
            get { return _invoicePayments; }
            set
            {
                _invoicePayments = value;
                SetPropertyChanged(nameof(InvoicePayments));
            }
        }

        public ObservableCollection<InvoicePaymentDto> ToRefundPayments { get; set; }

        public ObservableCollection<InvoicePaymentDto> BackorderPayments { get; set; }

        //Ticket start :#95027.by rupesh
        [JsonIgnore]
        ObservableCollection<InvoiceHistoryDto> _InvoiceHistories { get; set; }
        public ObservableCollection<InvoiceHistoryDto> InvoiceHistories { get { return _InvoiceHistories; } set { _InvoiceHistories = value; SetPropertyChanged(nameof(InvoiceHistories)); } }
        //Ticket End :#95027.by rupesh

        [JsonIgnore]
        ObservableCollection<InvoicePaymentDto> _InvoiceRefundPayments { get; set; }
        public ObservableCollection<InvoicePaymentDto> InvoiceRefundPayments { get { return _InvoiceRefundPayments; } set { _InvoiceRefundPayments = value; SetPropertyChanged(nameof(InvoiceRefundPayments)); } }


        [JsonIgnore]
        public ObservableCollection<InvoicePaymentDto> ActiveInvoicePayments
        {
            get
            {
                if (InvoicePayments != null)
                {
                    return new ObservableCollection<InvoicePaymentDto>(InvoicePayments.Where(x => !x.IsDeleted));
                }
                else
                {
                    return InvoicePayments;
                }

            }
        }

        [JsonIgnore]
        ObservableCollection<LineItemTaxDto> _taxgroup { get; set; }
        public ObservableCollection<LineItemTaxDto> Taxgroup { get { return _taxgroup; } set { _taxgroup = value; SetPropertyChanged(nameof(Taxgroup)); } }

        [JsonIgnore]
        ObservableCollection<LineItemTaxDto> _ReceiptTaxList { get; set; }
        public ObservableCollection<LineItemTaxDto> ReceiptTaxList { get { return _ReceiptTaxList; } set { _ReceiptTaxList = value; SetPropertyChanged(nameof(ReceiptTaxList)); } }

        public DateTime CreationTime { get; set; }

        [JsonIgnore]
        public DateTime CreationStoreTime
        {
            get
            {
                return CreationTime.ToStoreTime();
            }
        }

        public DateTime? LastModificationTime { get; set; }
        public bool DoNotUpdateInvenotry { get; set; }
        public ThirdPartySyncStatus ThirdPartySyncStatus { get; set; }
        public decimal LoyaltyPoints { get; set; }
        public decimal PriceListCustomerCurrentLoyaltyPoints { get; set; }
        public decimal CustomerCurrentLoyaltyPoints { get; set; }
        public decimal LoyaltyPointsValue { get; set; }

        [JsonIgnore]
        decimal _outstandingAmount { get; set; } = 0;

        public decimal OutstandingAmount
        {
            get
            {
                //Ticket start:#22406 Quote sale.by rupesh
                if (Status == InvoiceStatus.Quote)
                {
                    return 0;
                }
                //Ticket end:#22406 .by rupesh

                var result = Math.Round(NetAmount.ToPositive(), 2, MidpointRounding.AwayFromZero) - Math.Round(TotalPaid.ToPositive(), 2, MidpointRounding.AwayFromZero);
                //var result = NetAmount - TotalPaid;
                return result;
                //return _outstandingAmount; 

            }
            set
            {
                _outstandingAmount = value;
                SetPropertyChanged(nameof(OutstandingAmount));
            }
        }

        [JsonIgnore]
        public bool IsPartialPaid
        {
            get
            {
                // return NetAmount > TotalPaid && TotalPaid != 0;

                return Math.Round(NetAmount, 2, MidpointRounding.AwayFromZero) > Math.Round(TotalPaid, 2, MidpointRounding.AwayFromZero) && Math.Round(TotalPaid, 2, MidpointRounding.AwayFromZero) != 0;
            }
        }

        [JsonIgnore]
        public string OutstandingText
        {
            get
            {
                switch (Status)
                {

                    case InvoiceStatus.Completed:
                        return "Paid";
                    //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                    case InvoiceStatus.Parked:
                    case InvoiceStatus.PartialFulfilled:
                        //End #84293 by Pratik
                        if (NetAmount > TotalPaid && TotalPaid != 0)
                        {
                            //return string.Format("Partially paid ({0}{1})",Settings.StoreZoneAndFormatDetail.CurrencySymbol,TotalPaid);
                            return string.Format("Partially paid ({0})", TotalPaid.ToString("C"));
                        }
                        else if (NetAmount == TotalPaid && NetAmount != 0)
                        {
                            return "Paid";
                        }
                        else
                        {
                            return "Pending";
                        }

                    case InvoiceStatus.Voided:
                        //Ticket start:#25357 iOS - About Register Summary Change for Refunded and Discarded Sales.by rupesh
                        return FinancialStatus == FinancialStatus.Refunded ? "Refunded" : "Voided";
                    //Ticket end:#25357 .by rupesh
                    case InvoiceStatus.Refunded:
                        return "Refunded";
                    case InvoiceStatus.Exchange:
                        return "Exchanged";
                    case InvoiceStatus.LayBy:
                        if (NetAmount > TotalPaid && TotalPaid != 0)
                        {
                            //return string.Format("Partially paid ({0}{1})",Settings.StoreZoneAndFormatDetail.CurrencySymbol,TotalPaid);
                            return string.Format("Partially paid ({0})", TotalPaid.ToString("C"));
                        }
                        //Ticket start:#26168 iPad : Shopify sales not showing up paid in Hike store.by rupesh
                        else if (NetAmount == TotalPaid)
                        {
                            return "Paid";
                        }
                        //Ticket end:#26168 .by rupesh
                        else
                        {
                            return "Pending";
                        }
                    case InvoiceStatus.OnAccount:
                    case InvoiceStatus.OnGoing: //#94565
                        return "Pending";
                    case InvoiceStatus.Pending:
                        return "Pending";
                    case InvoiceStatus.BackOrder:
                        if (NetAmount > TotalPaid && TotalPaid != 0)
                        {
                            //return string.Format("Partially paid ({0}{1})",Settings.StoreZoneAndFormatDetail.CurrencySymbol,TotalPaid);
                            return string.Format("Partially paid ({0})", TotalPaid.ToString("C"));
                        }
                        else
                        {
                            return "Pending";
                        }
                    case InvoiceStatus.Quote:
                        return FinancialStatus == FinancialStatus.Closed ? "Converted to sale" : "Open";

                    default:
                        return "-";
                }
            }
        }

        [JsonIgnore]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case InvoiceStatus.Parked:
                        if ((NetAmount - TotalPaid) == 0)
                        {
                            //return "AWAITING FULFILLMENT";
                            return "Awaiting fulfillment";
                        }
                        else
                        {
                            //return Status.ToString().ToUpper();
                            return Status.ToString();
                        }
                    case InvoiceStatus.LayBy:
                        if ((NetAmount - TotalPaid) == 0)
                        {
                            //return "AWAITING FULFILLMENT";
                            return "Awaiting fulfillment";
                        }
                        else
                        {
                            //return Status.ToString().ToUpper();
                            return Status.ToString();
                        }
                    case InvoiceStatus.OnGoing:
                        return "OnGoing"; //#94565
                    case InvoiceStatus.Completed:
                        return "Complete";
                    //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
                    case InvoiceStatus.PartialFulfilled:
                        return "Partial Fulfilled";
                    //End #84293 by Pratik
                    default:
                        //return Status.ToString().ToUpper();
                        return Status.ToString();
                }
            }
        }



        //public string FulfillmentStatus { get; set; }  
        //public string PaymentStatus { get; set; }


        bool _isSync { get; set; }
        public bool isSync
        {
            get { return _isSync; }
            set
            {
                _isSync = value;
                SetPropertyChanged(nameof(isSync));
            }
        }

        bool _isCustomerChange { get; set; }
        public bool IsCustomerChange
        {
            get { return _isCustomerChange; }
            set
            {
                _isCustomerChange = value;
                SetPropertyChanged(nameof(IsCustomerChange));
            }
        }

        bool _createdPoOrNot { get; set; }
        public bool CreatedPoOrNot
        {
            get { return _createdPoOrNot; }
            set
            {
                _createdPoOrNot = value;
                SetPropertyChanged(nameof(CreatedPoOrNot));
                SetPropertyChanged(nameof(FullfilmentStatus));
            }
        }

        [JsonIgnore]
        public string FullfilmentStatus
        {
            get
            {
                if (Status == InvoiceStatus.BackOrder)
                {
                    if (CreatedPoOrNot)
                        return Status + " | " + LanguageExtension.Localize("POCreatedText");
                    else
                        return Status + " | " + LanguageExtension.Localize("CreatePOText");
                }
                else
                    return Status.ToString();
            }
        }

        public int? CurrentRegister { get; set; }
        public DateTime? FinalizeDate { get; set; }
        public int? RegisterClosureId { get; set; }
        public bool HasError { get; set; }

        void UpdateQuickCashOption(decimal TenderAmount)
        {

            QuickCashFirstOption = 0;
            QuickCashSecondOption = 0;
            QuickCashThirdOption = 0;
            QuickCashFourOption = 0;

            var QuickCashOptions = TenderAmount.getQuickCashOptions();
            if (QuickCashOptions != null && QuickCashOptions.Count > 0)
            {
                QuickCashFirstOption = QuickCashOptions[0];

                if (QuickCashOptions.Count > 1)
                {
                    QuickCashSecondOption = QuickCashOptions[1];
                }
                if (QuickCashOptions.Count > 2)
                {
                    QuickCashThirdOption = QuickCashOptions[2];
                }
                if (QuickCashOptions.Count > 3)
                {
                    QuickCashFourOption = QuickCashOptions[3];
                }
            }
        }

        [JsonIgnore]
        public bool CustomerGroupDiscountType { get; set; }


        [JsonIgnore]
        public string ChangeAmmountDetail
        {
            get
            {
                if (ChangeAmount == 0 || InvoicePayments == null || !InvoicePayments.Any())
                    return "";
                if (Convert.ToDouble(ChangeAmount) < 0.005 && Convert.ToDouble(ChangeAmount) > 0.00)
                    return "";


                return "(Tendered : " + InvoicePayments.LastOrDefault().TenderedAmount + ", " + Settings.CurrentRegister.ReceiptTemplate?.ChangeLable + ": " + ChangeAmount + ")";
            }
        }

        public decimal? BackOrdertotalPaid { get; set; }

        decimal _backOrdertotal { get; set; }
        public decimal BackOrdertotal { get { return _backOrdertotal; } set { _backOrdertotal = value; SetPropertyChanged(nameof(BackOrdertotal)); } }

        public decimal BackorderDeposite { get; set; }
        //Ticket #9209 Start:Back order deposit amount issue. By Nikhil.
        [JsonIgnore]
        string _strBackorderDeposite { get; set; }
        public string StrBackorderDeposite
        {
            get
            {
                return _strBackorderDeposite;
            }
            set
            {
                _strBackorderDeposite = value;
                if (!string.IsNullOrEmpty(value))
                {
                    decimal result;
                    if (decimal.TryParse(value, out result))
                        BackorderDeposite = result;
                    else
                        BackorderDeposite = 0;
                }
                else
                    BackorderDeposite = 0;
                SetPropertyChanged(nameof(StrBackorderDeposite));
            }
        }

        //Ticket #9209 End:By Nikhil.

        //Ticket start:#22406 Quote sale.by rupesh
        public FinancialStatus FinancialStatus { get; set; }
        //Ticket end:#22406 Quote sale.by rupesh

        //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
        [JsonIgnore]
        int? _DeliveryAddressId { get; set; }
        public int? DeliveryAddressId { get { return _DeliveryAddressId; } set { _DeliveryAddressId = value; SetPropertyChanged(nameof(DeliveryAddressId)); } }

        [JsonIgnore]
        CustomerAddressDto _DeliveryAddress { get; set; }
        public CustomerAddressDto DeliveryAddress { get { return _DeliveryAddress; } set { _DeliveryAddress = value; SetPropertyChanged(nameof(DeliveryAddress)); } }
        //Ticket end:#26664 IOS - New feature :: Customer delivery address.by rupesh



        //#33590,#33849 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total

        [JsonIgnore]
        int? _TipTaxId { get; set; }
        public int? TipTaxId { get { return _TipTaxId; } set { _TipTaxId = value; SetPropertyChanged(nameof(TipTaxId)); } }

        [JsonIgnore]
        decimal? _TipTaxRate { get; set; }
        public decimal? TipTaxRate { get { return _TipTaxRate; } set { _TipTaxRate = value; SetPropertyChanged(nameof(TipTaxRate)); } }


        [JsonIgnore]
        string _TipTaxName { get; set; }
        public string TipTaxName { get { return _TipTaxName; } set { _TipTaxName = value; SetPropertyChanged(nameof(TipTaxName)); } }



        [JsonIgnore]
        decimal _TipTaxAmount { get; set; }
        public decimal TipTaxAmount { get { return _TipTaxAmount; } set { _TipTaxAmount = value; SetPropertyChanged(nameof(TipTaxAmount)); } }

        [JsonIgnore]
        decimal _SurchargeDisplayValuecal { get; set; }
        public decimal SurchargeDisplayValuecal { get { return _SurchargeDisplayValuecal; } set { _SurchargeDisplayValuecal = value; SetPropertyChanged(nameof(SurchargeDisplayValuecal)); } }


        [JsonIgnore]
        decimal _SurchargeDisplayInSaleHistory { get; set; }
        public decimal SurchargeDisplayInSaleHistory { get { return _SurchargeDisplayInSaleHistory; } set { _SurchargeDisplayInSaleHistory = value; SetPropertyChanged(nameof(SurchargeDisplayInSaleHistory)); } }



        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
        /*
         public ObservableCollection<InvoiceTaxDto> InvoiceTaxes { get; set; }*/
        //Ticket start:#29657 Seach customer using phone number.by rupesh
        public string CustomerPhone { get; set; }
        //Ticket end:#29657 .by rupesh

        public string QRString { get; set; }

        //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
        [JsonIgnore]
        bool _IsToUpdateCustomerName { get; set; }
        [JsonIgnore]
        public bool IsToUpdateCustomerName { get { return _IsToUpdateCustomerName; } set { _IsToUpdateCustomerName = value; SetPropertyChanged(nameof(IsToUpdateCustomerName)); } }
        [JsonIgnore]
        string _UpdatedCustomerName { get; set; }
        [JsonIgnore]
        public string UpdatedCustomerName { get { return _UpdatedCustomerName; } set { _UpdatedCustomerName = value; SetPropertyChanged(nameof(UpdatedCustomerName)); } }
        //Ticket end:#45648 .by rupesh

        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        public decimal TotalPaymentSurcharge { get; set; }
        [JsonIgnore]
        decimal _totalTipSurcharge { get; set; }
        [JsonIgnore]
        public decimal TotalTipSurcharge { get { return _totalTipSurcharge; } set { _totalTipSurcharge = value; SetPropertyChanged(nameof(TotalTipSurcharge)); } }
        //End ticket #73190 By Pratik

        //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
        public string CustomFields { get; set; }
        public InvoiceOutstanding InvoiceOutstanding { get; set; }
        //End Ticket #63876 by Pratik

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public DateTime? InvoiceDueDate { get; set; }
        //End ticket #76208 by Pratik
        public string CreatedBy { get; set; }
        public int? CreatorUserId { get; set; }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        [JsonIgnore]
        private ObservableCollection<InvoiceFulfillmentDto> _invoiceFulfillments;
        public ObservableCollection<InvoiceFulfillmentDto> InvoiceFulfillments { get { return _invoiceFulfillments; } set { _invoiceFulfillments = value; SetPropertyChanged(nameof(InvoiceFulfillments)); } }

        public int InvoiceFulfillCount { get; set; }
        //End #84293 by Pratik

        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
        public string AgeVerificationImageUpload { get; set; }
        //Ticket:end:#90938 .by rupesh
        //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time.by rupesh
        public bool IsEditBackOrderFromSaleHistory { get; set; }
        //Ticket end:#84289 .by rupesh

        //#94565
        private string _floorTableName;
        public string FloorTableName
        {
            get
            {
                string valstr = string.Empty;
                if (!string.IsNullOrEmpty(_floorTableName))
                {
                    if(_floorTableName.StartsWith("<br/>"))
                        valstr = _floorTableName.Replace("<br/>", "");
                    else
                        valstr = _floorTableName.Replace("<br/>", " - ");
                }
                if (InvoiceFloorTable?.TableName != null && string.IsNullOrEmpty(valstr))
                {
                    if (string.IsNullOrEmpty(InvoiceFloorTable.FloorName))
                        valstr = InvoiceFloorTable.TableName;
                    else
                        valstr = InvoiceFloorTable.FloorName + " - " + InvoiceFloorTable.TableName;
                }
                return valstr;
            }
            set { _floorTableName = value; SetPropertyChanged(nameof(FloorTableName)); }
        }

        [JsonIgnore]
        public InvoiceFloorTableDto InvoiceFloorTable => InvoiceFloorTables?.FirstOrDefault();
        
        private ObservableCollection<InvoiceFloorTableDto> _invoiceFloorTables;
        public ObservableCollection<InvoiceFloorTableDto> InvoiceFloorTables { get { return _invoiceFloorTables; } set { _invoiceFloorTables = value; SetPropertyChanged(nameof(InvoiceFloorTables)); } }
        //#94565

        //#95241
        public int? approvedByUser { get; set; }
        private string _approvedByUserName;
        public string approvedByUserName { get { return _approvedByUserName; } set { _approvedByUserName = value; SetPropertyChanged(nameof(approvedByUserName)); } }
        //#95241


        public InvoiceDB ToModel()
        {
            InvoiceDB invoiceDB = new InvoiceDB
            {
                Id = Id,
                IsActive = IsActive,
                InvoiceTempId = string.IsNullOrEmpty(InvoiceTempId) ? nameof(InvoiceDto) + "_" + Guid.NewGuid().ToString() : InvoiceTempId,
                KeyId = Id > 0 ? Id.ToString() : InvoiceTempId,
                IsSNEFSaleHistory = IsSerialNumberEditableFromSaleHistory,
                TanentId = TanentId,
                IscallFromPayment = IscallFromPayment,
                InvoiceUpdateFrom = (int)InvoiceUpdateFrom,
                Number = Number,
                TransactionDate = TransactionDate,
                CustomerId = CustomerId,
                CustomerTempId = CustomerTempId,
                CustomerDetail = CustomerDetail?.ToModel(),
                CustomerGroupName = CustomerGroupName,
                CustomerName = CustomerName,
                CustomerGroupId = CustomerGroupId,
                CustomerGroupDiscount = CustomerGroupDiscount,
                CGDNote = CustomerGroupDiscountNote,
                CGDNInside = CustomerGroupDiscountNoteInside,
                CGDNInsidePrice = CustomerGroupDiscountNoteInsidePrice,
                OutletId = OutletId,
                OutletName = OutletName,
                RegisterId = RegisterId,
                RegisterName = RegisterName,
                Barcode = Barcode,
                Status = (int)Status,
                LocalInvoiceStatus = (int)LocalInvoiceStatus,
                CurrentPaymentObject = CurrentPaymentObject,
                TaxInclusive = TaxInclusive,
                ApplyTaxAfterDiscount = ApplyTaxAfterDiscount,
                DiscountIsAsPercentage = DiscountIsAsPercentage,
                DiscountValue = DiscountValue,
                DiscoutType = DiscoutType,
                DiscountNote = DiscountNote,
                TipIsAsPercentage = TipIsAsPercentage,
                TipValue = TipValue,
                SubTotal = SubTotal,
                GiftCardTotal = GiftCardTotal,
                TotalDiscount = TotalDiscount,
                TotalShippingCost = TotalShippingCost,
                shippingTaxId = shippingTaxId,
                ShippingTaxRate = ShippingTaxRate,
                ShippingTaxName = ShippingTaxName,
                ShippingTaxAmount = ShippingTaxAmount,
                TotalTax = TotalTax,
                OtherCharges = OtherCharges,
                Tax = Tax,
                TotalTip = TotalTip,
                RoundingAmount = RoundingAmount,
                NetAmount = NetAmount,
                TotalPaid = TotalPaid,
                TotalPay = TotalPay,
                TenderAmount = TenderAmount,
                StrTenderAmount = StrTenderAmount,
                QuickCashFirstOption = QuickCashFirstOption,
                QuickCashSecondOption = QuickCashSecondOption,
                QuickCashThirdOption = QuickCashThirdOption,
                QuickCashFourOption = QuickCashFourOption,
                TotalTender = TotalTender,
                ChangeAmount = ChangeAmount,
                Currency = Currency,
                ServedBy = ServedBy,
                ServedByName = ServedByName,
                Note = Note,
                InvoiceFrom = (int)InvoiceFrom,
                InvoiceFromName = InvoiceFromName,
                ReferenceInvoiceId = ReferenceInvoiceId,
                ReferenceTempInvoiceId = ReferenceTempInvoiceId,
                ReferenceNote = ReferenceNote,
                ExchangeReferenceNote = ExchangeReferenceNote,
                IsReStockWhenRefund = IsReStockWhenRefund,
                CanBeModified = CanBeModified,
                TrackNumber = TrackNumber,
                TrackURL = TrackURL,
                TrackDetails = TrackDetails,
                ReceiptHTML = ReceiptHTML,
                CreationTime = CreationTime,
                LastModificationTime = LastModificationTime,
                DoNotUpdateInvenotry = DoNotUpdateInvenotry,
                ThirdPartySyncStatus = (int)ThirdPartySyncStatus,
                LoyaltyPoints = LoyaltyPoints,
                PLCCLoyaltyPoints = PriceListCustomerCurrentLoyaltyPoints,
                CCLoyaltyPoints = CustomerCurrentLoyaltyPoints,
                LoyaltyPointsValue = LoyaltyPointsValue,
                OutstandingAmount = OutstandingAmount,
                isSync = isSync,
                IsCustomerChange = IsCustomerChange,
                CreatedPoOrNot = CreatedPoOrNot,
                CurrentRegister = CurrentRegister,
                FinalizeDate = FinalizeDate,
                RegisterClosureId = RegisterClosureId,
                HasError = HasError,
                BackOrdertotalPaid = BackOrdertotalPaid,
                BackOrdertotal = BackOrdertotal,
                BackorderDeposite = BackorderDeposite,
                StrBackorderDeposite = StrBackorderDeposite,
                FinancialStatus = (int)FinancialStatus,
                DeliveryAddressId = DeliveryAddressId,
                DeliveryAddress = DeliveryAddress?.ToModel(),
                TipTaxId = TipTaxId,
                TipTaxRate = TipTaxRate,
                TipTaxName = TipTaxName,
                TipTaxAmount = TipTaxAmount,
                SurchargeDisplayValuecal = SurchargeDisplayValuecal,
                SDInSaleHistory = SurchargeDisplayInSaleHistory,
                CustomerPhone = CustomerPhone,
                QRString = QRString,
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                TotalPaymentSurcharge = TotalPaymentSurcharge,
                //End ticket #73190
                //START ticket #76208 IOS:FR:Terms of payments by Pratik
                InvoiceDueDate = InvoiceDueDate,
                //End ticket #76208 by Pratik
                //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                CustomFields = CustomFields,
                InvoiceOutstanding = InvoiceOutstanding?.ToModel(),
                //End Ticket #63876 by Pratik
                CreatedBy = CreatedBy,
                CreatorUserId = CreatorUserId,
                //Ticket:start:#90938 IOS:FR Age varification.by rupesh
                AgeVerificationImageUpload = AgeVerificationImageUpload,
                //Ticket:end:#90938.by rupesh
                InvoiceFulfillCount = InvoiceFulfillCount,  //Start #84293 by Pratik
                InvoiceDetail = InvoiceDetail?.ToModel(), //Start #91991 By Pratik
                //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time.by rupesh
                IsEditBackOrderFromSaleHistory = IsEditBackOrderFromSaleHistory,
                //Ticket end:#84289 .by rupesh
                TableId = InvoiceFloorTables?.FirstOrDefault()?.TableId ?? 0, //#94565
                FloorTableName = FloorTableName,   //#94565
                approvedByUserName = approvedByUserName, //#95241
                approvedByUser = approvedByUser, //#95241
                IsExchangeSale = IsExchangeSale //#97493

            };

            InvoiceLineItems?.ForEach(i => invoiceDB.InvoiceLineItems.Add(i.ToModel()));
            InvoicePayments1?.ForEach(i => invoiceDB.InvoicePayments1.Add(i.ToModel()));
            InvoicePayments?.ForEach(i => invoiceDB.InvoicePayments.Add(i.ToModel()));
            ToRefundPayments?.ForEach(i => invoiceDB.ToRefundPayments.Add(i.ToModel()));
            BackorderPayments?.ForEach(i => invoiceDB.BackorderPayments.Add(i.ToModel()));
            InvoiceHistories?.ForEach(i => invoiceDB.InvoiceHistories.Add(i.ToModel()));
            InvoiceRefundPayments?.ForEach(i => invoiceDB.InvoiceRefundPayments.Add(i.ToModel()));
            Taxgroup?.ForEach(i => invoiceDB.Taxgroup.Add(i.ToModel()));
            ReceiptTaxList?.ForEach(i => invoiceDB.ReceiptTaxList.Add(i.ToModel()));
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            InvoiceFulfillments?.ForEach(i => invoiceDB.InvoiceFulfillments.Add(i.ToModel()));
            //End #84293 by Pratik

            InvoiceFloorTables?.ForEach(i => invoiceDB.InvoiceFloorTables.Add(i.ToModel())); //#94565
            return invoiceDB;
        }

        public static InvoiceDto FromModel(InvoiceDB invoiceDB)
        {
            if (invoiceDB == null)
                return null;

            InvoiceDto invoiceDto = new InvoiceDto
            {
                Id = invoiceDB.Id,
                IsActive = invoiceDB.IsActive,
                InvoiceTempId = invoiceDB.InvoiceTempId,
                KeyId = invoiceDB.KeyId,
                IsSerialNumberEditableFromSaleHistory = invoiceDB.IsSNEFSaleHistory,
                TanentId = invoiceDB.TanentId,
                IscallFromPayment = invoiceDB.IscallFromPayment,
                InvoiceUpdateFrom = (InvoiceUpdateFrom)invoiceDB.InvoiceUpdateFrom,
                Number = invoiceDB.Number,
                TransactionDate = invoiceDB.TransactionDate.UtcDateTime,
                CustomerId = invoiceDB.CustomerId,
                CustomerTempId = invoiceDB.CustomerTempId,
                CustomerDetail = invoiceDB.CustomerDetail != null ? CustomerDto_POS.FromModel(invoiceDB.CustomerDetail) : null,
                CustomerGroupName = invoiceDB.CustomerGroupName,
                CustomerName = invoiceDB.CustomerName,
                CustomerGroupId = invoiceDB.CustomerGroupId,
                CustomerGroupDiscount = invoiceDB.CustomerGroupDiscount,
                CustomerGroupDiscountNote = invoiceDB.CGDNote,
                CustomerGroupDiscountNoteInside = invoiceDB.CGDNInside,
                CustomerGroupDiscountNoteInsidePrice = invoiceDB.CGDNInsidePrice,
                OutletId = invoiceDB.OutletId,
                OutletName = invoiceDB.OutletName,
                RegisterId = invoiceDB.RegisterId,
                RegisterName = invoiceDB.RegisterName,
                Barcode = invoiceDB.Barcode,
                Status = (InvoiceStatus)invoiceDB.Status,
                LocalInvoiceStatus = (LocalInvoiceStatus)invoiceDB.LocalInvoiceStatus,
                CurrentPaymentObject = invoiceDB.CurrentPaymentObject,
                TaxInclusive = invoiceDB.TaxInclusive,
                ApplyTaxAfterDiscount = invoiceDB.ApplyTaxAfterDiscount,
                DiscountIsAsPercentage = invoiceDB.DiscountIsAsPercentage,
                DiscountValue = invoiceDB.DiscountValue,
                DiscoutType = invoiceDB.DiscoutType,
                DiscountNote = invoiceDB.DiscountNote,
                TipIsAsPercentage = invoiceDB.TipIsAsPercentage,
                TipValue = invoiceDB.TipValue,
                SubTotal = invoiceDB.SubTotal,
                GiftCardTotal = invoiceDB.GiftCardTotal,
                TotalDiscount = invoiceDB.TotalDiscount,
                TotalShippingCost = invoiceDB.TotalShippingCost,
                shippingTaxId = invoiceDB.shippingTaxId,
                ShippingTaxRate = invoiceDB.ShippingTaxRate,
                ShippingTaxName = invoiceDB.ShippingTaxName,
                ShippingTaxAmount = invoiceDB.ShippingTaxAmount,
                TotalTax = invoiceDB.TotalTax,
                OtherCharges = invoiceDB.OtherCharges,
                Tax = invoiceDB.Tax,
                TotalTip = invoiceDB.TotalTip,
                RoundingAmount = invoiceDB.RoundingAmount,
                NetAmount = invoiceDB.NetAmount,
                TotalPaid = invoiceDB.TotalPaid,
                TotalPay = invoiceDB.TotalPay,
                TenderAmount = invoiceDB.TenderAmount,
                StrTenderAmount = invoiceDB.StrTenderAmount,
                QuickCashFirstOption = invoiceDB.QuickCashFirstOption,
                QuickCashSecondOption = invoiceDB.QuickCashSecondOption,
                QuickCashThirdOption = invoiceDB.QuickCashThirdOption,
                QuickCashFourOption = invoiceDB.QuickCashFourOption,
                TotalTender = invoiceDB.TotalTender,
                ChangeAmount = invoiceDB.ChangeAmount,
                Currency = invoiceDB.Currency,
                ServedBy = invoiceDB.ServedBy,
                ServedByName = invoiceDB.ServedByName,
                Note = invoiceDB.Note,
                InvoiceFrom = (InvoiceFrom)invoiceDB.InvoiceFrom,
                InvoiceFromName = invoiceDB.InvoiceFromName,
                ReferenceInvoiceId = invoiceDB.ReferenceInvoiceId,
                ReferenceTempInvoiceId = invoiceDB.ReferenceTempInvoiceId,
                ReferenceNote = invoiceDB.ReferenceNote,
                ExchangeReferenceNote = invoiceDB.ExchangeReferenceNote,
                IsReStockWhenRefund = invoiceDB.IsReStockWhenRefund,
                CanBeModified = invoiceDB.CanBeModified,
                TrackNumber = invoiceDB.TrackNumber,
                TrackURL = invoiceDB.TrackURL,
                TrackDetails = invoiceDB.TrackDetails,
                ReceiptHTML = invoiceDB.ReceiptHTML,
                CreationTime = invoiceDB.CreationTime.UtcDateTime,
                LastModificationTime = invoiceDB.LastModificationTime?.UtcDateTime,
                DoNotUpdateInvenotry = invoiceDB.DoNotUpdateInvenotry,
                ThirdPartySyncStatus = (ThirdPartySyncStatus)invoiceDB.ThirdPartySyncStatus,
                LoyaltyPoints = invoiceDB.LoyaltyPoints,
                PriceListCustomerCurrentLoyaltyPoints = invoiceDB.PLCCLoyaltyPoints,
                CustomerCurrentLoyaltyPoints = invoiceDB.CCLoyaltyPoints,
                LoyaltyPointsValue = invoiceDB.LoyaltyPointsValue,
                OutstandingAmount = invoiceDB.OutstandingAmount,
                isSync = invoiceDB.isSync,
                IsCustomerChange = invoiceDB.IsCustomerChange,
                CreatedPoOrNot = invoiceDB.CreatedPoOrNot,
                CurrentRegister = invoiceDB.CurrentRegister,
                FinalizeDate = invoiceDB.FinalizeDate?.UtcDateTime,
                RegisterClosureId = invoiceDB.RegisterClosureId,
                HasError = invoiceDB.HasError,
                BackOrdertotalPaid = invoiceDB.BackOrdertotalPaid,
                BackOrdertotal = invoiceDB.BackOrdertotal,
                BackorderDeposite = invoiceDB.BackorderDeposite,
                StrBackorderDeposite = invoiceDB.StrBackorderDeposite,
                FinancialStatus = (FinancialStatus)invoiceDB.FinancialStatus,
                DeliveryAddressId = invoiceDB.DeliveryAddressId,
                DeliveryAddress = invoiceDB.DeliveryAddress != null ? CustomerAddressDto.FromModel(invoiceDB.DeliveryAddress) : null,
                TipTaxId = invoiceDB.TipTaxId,
                TipTaxRate = invoiceDB.TipTaxRate,
                TipTaxName = invoiceDB.TipTaxName,
                TipTaxAmount = invoiceDB.TipTaxAmount,
                SurchargeDisplayValuecal = invoiceDB.SurchargeDisplayValuecal,
                SurchargeDisplayInSaleHistory = invoiceDB.SDInSaleHistory,
                CustomerPhone = invoiceDB.CustomerPhone,
                QRString = invoiceDB.QRString,
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                TotalPaymentSurcharge = invoiceDB.TotalPaymentSurcharge,
                //End ticket #73190
                //START ticket #76208 IOS:FR:Terms of payments by Pratik
                InvoiceDueDate = invoiceDB.InvoiceDueDate?.UtcDateTime,
                //End ticket #76208 by Pratik
                //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                CustomFields = invoiceDB.CustomFields,
                InvoiceOutstanding = InvoiceOutstanding.FromModel(invoiceDB.InvoiceOutstanding),
                //End Ticket #63876 by Pratik
                CreatedBy = invoiceDB.CreatedBy,
                CreatorUserId = invoiceDB.CreatorUserId,
                //Ticket:start:#90938 IOS:FR Age varification.by rupesh
                AgeVerificationImageUpload = invoiceDB.AgeVerificationImageUpload,
                //Ticket:end:#90938 .by rupesh
                InvoiceFulfillCount = invoiceDB.InvoiceFulfillCount,  //Start #84293 by Pratik
                InvoiceDetail = OnAccountPONumberRequest.FromModel(invoiceDB.InvoiceDetail), //Start #91991 By Pratik
                //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time.by rupesh
                IsEditBackOrderFromSaleHistory = invoiceDB.IsEditBackOrderFromSaleHistory,
                //Ticket end:#84289 .by rupesh
                FloorTableName = invoiceDB.FloorTableName,   //#94565
                approvedByUserName = invoiceDB.approvedByUserName, //#95241
                approvedByUser = invoiceDB.approvedByUser, //#95241
                IsExchangeSale = invoiceDB.IsExchangeSale //#97493
            };

            if (invoiceDB.InvoiceLineItems != null)
                invoiceDto.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(invoiceDB.InvoiceLineItems.Select(a => InvoiceLineItemDto.FromModel(a)));

            if (invoiceDB.InvoicePayments1 != null)
                invoiceDto.InvoicePayments1 = new ObservableCollection<InvoicePaymentDto>(invoiceDB.InvoicePayments1.Select(a => InvoicePaymentDto.FromModel(a)));

            if (invoiceDB.InvoicePayments != null)
                invoiceDto.InvoicePayments = new ObservableCollection<InvoicePaymentDto>(invoiceDB.InvoicePayments.Select(a => InvoicePaymentDto.FromModel(a)));

            if (invoiceDB.ToRefundPayments != null)
                invoiceDto.ToRefundPayments = new ObservableCollection<InvoicePaymentDto>(invoiceDB.ToRefundPayments.Select(a => InvoicePaymentDto.FromModel(a)));

            if (invoiceDB.BackorderPayments != null)
                invoiceDto.BackorderPayments = new ObservableCollection<InvoicePaymentDto>(invoiceDB.BackorderPayments.Select(a => InvoicePaymentDto.FromModel(a)));

            if (invoiceDB.InvoiceHistories != null)
                invoiceDto.InvoiceHistories = new ObservableCollection<InvoiceHistoryDto>(invoiceDB.InvoiceHistories.Select(a => InvoiceHistoryDto.FromModel(a)));

            if (invoiceDB.InvoiceRefundPayments != null)
                invoiceDto.InvoiceRefundPayments = new ObservableCollection<InvoicePaymentDto>(invoiceDB.InvoiceRefundPayments.Select(a => InvoicePaymentDto.FromModel(a)));

            if (invoiceDB.Taxgroup != null)
                invoiceDto.Taxgroup = new ObservableCollection<LineItemTaxDto>(invoiceDB.Taxgroup.Select(a => LineItemTaxDto.FromModel(a)));

            if (invoiceDB.ReceiptTaxList != null)
                invoiceDto.ReceiptTaxList = new ObservableCollection<LineItemTaxDto>(invoiceDB.ReceiptTaxList.Select(a => LineItemTaxDto.FromModel(a)));

            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            if (invoiceDB.InvoiceFulfillments != null)
                invoiceDto.InvoiceFulfillments = new ObservableCollection<InvoiceFulfillmentDto>(invoiceDB.InvoiceFulfillments.Select(a => InvoiceFulfillmentDto.FromModel(a)));
            //End #84293 by Pratik

             //#94565
            if (invoiceDB.InvoiceFloorTables != null)
                invoiceDto.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>(invoiceDB.InvoiceFloorTables.Select(a => InvoiceFloorTableDto.FromModel(a))?.ToList());
           //#94565

            return invoiceDto;
        }

    }

    /*
     Below DTO is used only for get last pending invoice from database
     and dispay invoice in entersale page when appication open.This DTO
    should have only one record.


    Reason: we are using InvoiceDto for all invoice related operation
            like storing invoice from the server and displaying in sale history
            that's why InvoiceDTO having lots of data. If we use the same
            DTO(InvoiceDto) for getting pending we will face performance issue
            that's why we have created LocalInvoiceDto
    */

    public class LocalInvoiceDto : FullAuditedPassiveEntityDto
    {
        public LocalInvoiceDto()
        {
            InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
            InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
            InvoiceHistories = new ObservableCollection<InvoiceHistoryDto>();
            Taxgroup = new ObservableCollection<LineItemTaxDto>();
            BackorderPayments = new ObservableCollection<InvoicePaymentDto>();
        }

        [JsonProperty("SyncReference")]
        public string InvoiceTempId { get; set; }

        [JsonIgnore]
        string _Number { get; set; }

        public string Number { get { return _Number; } set { _Number = value; SetPropertyChanged(nameof(Number)); } }

        //Ticket start:#13735 iOS - Performance of Sales History by rupesh
        [JsonIgnore]
        public DateTime _TransactionDate { get; set; }
        public DateTime TransactionDate
        {
            get
            {
                return _TransactionDate;
            }
            set
            {
                _TransactionDate = value;
                TransactionStoreDate = TransactionDate.ToStoreTime();
            }
        }

        public OnAccountPONumberRequest InvoiceDetail { get; set; } //Start #91991 By Pratik

        [JsonIgnore]
        public DateTime TransactionStoreDate { get; set; }
        // public DateTime TransactionStoreDate { get { return TransactionDate.ToStoreTime(); } }
        //Ticket end:#13735 iOS 

        [JsonIgnore]
        int? _CustomerId { get; set; }

        public int? CustomerId { get { return _CustomerId; } set { _CustomerId = value; SetPropertyChanged(nameof(CustomerId)); } }


        [JsonIgnore]
        public DateTime FinalizeDateStoreDate
        {
            get
            {
                //if (FinalizeDate != null)
                //{
                //    return FinalizeDate.Value.ToStoreTime();
                //}
                //else
                //{
                //    return TransactionDate.ToStoreTime();
                //}
                //Start #40634 iPad  :: Feature request - About How to Handle Transaction Date of Parked Sales
                if (Settings.StoreGeneralRule.HandleDateOfParkedSales)
                {
                    return CreationTime.ToStoreTime();
                }
                else if (FinalizeDate != null)
                {
                    return FinalizeDate.Value.ToStoreTime();
                }
                else
                {
                    return TransactionDate.ToStoreTime();
                }
                //#40634 iPad: End by nutan

            }
        }

        //[JsonIgnore]
        public string CustomerTempId { get; set; }

        [JsonIgnore]
        CustomerDto_POS _customerDetail { get; set; }

        public CustomerDto_POS CustomerDetail { get { return _customerDetail; } set { _customerDetail = value; SetPropertyChanged(nameof(CustomerDetail)); } }

        [JsonIgnore]
        string _customerName { get; set; }

        public string CustomerGroupName { get; set; }

        public string CustomerName
        {
            get
            {
                return _customerName;
            }
            set
            {
                _customerName = value;
                SetPropertyChanged(nameof(CustomerName));
            }
        }

        public bool IsExchangeSale { get; set; } //#97493

        public int? CustomerGroupId { get; set; }
        public decimal? CustomerGroupDiscount { get; set; }

        [JsonIgnore]
        string _customerGroupDiscountNote { get; set; }

        public string CustomerGroupDiscountNote
        {
            get { return _customerGroupDiscountNote; }
            set
            {
                _customerGroupDiscountNote = value;
                SetPropertyChanged(nameof(CustomerGroupDiscountNote));
            }
        }

        public string CustomerGroupDiscountNoteInside { get; set; }
        public decimal CustomerGroupDiscountNoteInsidePrice { get; set; }

        public int OutletId { get; set; }

        public string OutletName { get; set; }
        public int? RegisterId { get; set; }
        public string RegisterName { get; set; }

        public string Barcode { get; set; }

        [JsonIgnore]
        InvoiceStatus _Status { get; set; }
        public InvoiceStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                SetPropertyChanged(nameof(Status));
            }
        }

        [JsonIgnore]
        LocalInvoiceStatus _LocalInvoiceStatus { get; set; }
        public LocalInvoiceStatus LocalInvoiceStatus
        {
            get
            {
                return _LocalInvoiceStatus;
            }
            set
            {
                _LocalInvoiceStatus = value;
                SetPropertyChanged(nameof(LocalInvoiceStatus));
            }
        }



        string _CurrentPaymentObject { get; set; }



        public string CurrentPaymentObject
        {
            get
            {
                return _CurrentPaymentObject;
            }
            set
            {
                _CurrentPaymentObject = value;
                SetPropertyChanged(nameof(CurrentPaymentObject));
            }
        }

        public bool TaxInclusive { get; set; }

        public bool ApplyTaxAfterDiscount { get; set; }

        [JsonIgnore]
        bool _discountIsAsPercentage { get; set; }

        public bool DiscountIsAsPercentage
        {
            get { return _discountIsAsPercentage; }
            set
            {
                _discountIsAsPercentage = value;
                SetPropertyChanged(nameof(DiscountIsAsPercentage));
            }
        }

        [JsonIgnore]
        decimal _discountValue { get; set; }

        public decimal DiscountValue
        {
            get { return _discountValue; }
            set
            {
                //_discountValue = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                _discountValue = value;
                SetPropertyChanged(nameof(DiscountValue));
            }
        }

        [JsonIgnore]
        string _discoutType { get; set; }
        public string DiscoutType
        {
            get { return _discoutType; }
            set
            {
                _discoutType = value;
                SetPropertyChanged(nameof(DiscoutType));
            }
        }

        [JsonIgnore]
        string _discountNote { get; set; }

        public string DiscountNote
        {
            get { return _discountNote; }
            set
            {
                _discountNote = value;
                SetPropertyChanged(nameof(DiscountNote));
            }
        }

        [JsonIgnore]
        bool _tipIsAsPercentage { get; set; }

        public bool TipIsAsPercentage
        {
            get { return _tipIsAsPercentage; }
            set
            {
                _tipIsAsPercentage = value;
                SetPropertyChanged(nameof(TipIsAsPercentage));
            }
        }

        [JsonIgnore]
        decimal _tipValue { get; set; } = 0;

        public decimal TipValue
        {
            get { return _tipValue; }
            set
            {
                _tipValue = value;
                SetPropertyChanged(nameof(TipValue));
            }
        }

        [JsonIgnore]
        decimal _subTotal { get; set; } = 0;

        public decimal SubTotal
        {
            get { return _subTotal; }
            set
            {
                _subTotal = value;
                SetPropertyChanged(nameof(SubTotal));
            }
        }

        public decimal GiftCardTotal { get; set; }

        [JsonIgnore]
        decimal _totalDiscount { get; set; } = 0;

        public decimal TotalDiscount
        {
            get { return _totalDiscount; }
            set
            {
                _totalDiscount = value;
                SetPropertyChanged(nameof(TotalDiscount));
            }
        }

        [JsonIgnore]
        decimal _totalShippingCost { get; set; } = 0;

        public decimal TotalShippingCost
        {
            get { return _totalShippingCost; }
            set
            {
                _totalShippingCost = value;
                SetPropertyChanged(nameof(TotalShippingCost));
            }
        }
        [JsonIgnore]
        int? _shippingTaxId { get; set; } = 0;

        public int? shippingTaxId
        {
            get { return _shippingTaxId; }
            set
            {
                _shippingTaxId = value;
                SetPropertyChanged(nameof(shippingTaxId));
            }
        }

        [JsonIgnore]
        decimal? _shippingTaxRate { get; set; } = 0;

        public decimal? ShippingTaxRate
        {
            get { return _shippingTaxRate; }
            set
            {
                _shippingTaxRate = value;
                SetPropertyChanged(nameof(ShippingTaxRate));
            }
        }


        [JsonIgnore]
        string _shippingTaxName { get; set; }

        public string ShippingTaxName
        {
            get { return _shippingTaxName; }
            set
            {
                _shippingTaxName = value;
                SetPropertyChanged(nameof(ShippingTaxName));
            }
        }


        [JsonIgnore]
        decimal? _shippingTaxAmount { get; set; } = 0;

        public decimal? ShippingTaxAmount
        {
            get { return _shippingTaxAmount; }
            set
            {
                _shippingTaxAmount = value;
                SetPropertyChanged(nameof(ShippingTaxAmount));
            }
        }
        //Ticket start:#33812 iPad: New Feature Request :: Shipping charge showing Tax Exclusive in sales history page.by rupesh
        [JsonIgnore]
        public decimal? ShippingTaxAmountExclusive
        {
            get { return TotalShippingCost - _shippingTaxAmount; }
        }
        [JsonIgnore]
        public decimal? _TotalTax { get; set; }
        public decimal? TotalTax
        {
            get
            {
                return Tax + TipTaxAmount + ShippingTaxAmount;
            }
            set
            {
                _TotalTax = value;
                SetPropertyChanged(nameof(TotalTax));

            }
        }
        //Ticket end:#33812 .by rupesh


        [JsonIgnore]
        decimal _otherCharges { get; set; } = 0;

        public decimal OtherCharges
        {
            get { return _otherCharges; }
            set
            {
                _otherCharges = value;
                SetPropertyChanged(nameof(OtherCharges));
            }
        }

        [JsonIgnore]
        decimal _Tax { get; set; }

        public decimal Tax
        {
            get
            {
                if (_Tax == 0 && _TotalTax > 0)
                    _Tax = (decimal)(_TotalTax - TipTaxAmount - ShippingTaxAmount);
                return _Tax;
            }
            set
            {
                _Tax = value;
                SetPropertyChanged(nameof(Tax));
            }
        }

        [JsonIgnore]
        decimal _totalTip { get; set; } = 0;


        public decimal TotalTip
        {
            get { return _totalTip; }
            set
            {
                _totalTip = value;
                SetPropertyChanged(nameof(TotalTip));
            }
        }

        //Ticket start:#36019 iPad: Surcharge amount showing different in print receipt.by rupesh
        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        [JsonIgnore]
        public decimal TotalTipTaxExclusive
        {
            get { return (TotalTip + TotalPaymentSurcharge) - TipTaxAmount; }
        }
        //Ticket end:#36019 .by rupesh
        //End Ticket #73190 By: Pratik

        [JsonIgnore]
        decimal _roundingAmount { get; set; } = 0;

        public decimal RoundingAmount
        {
            get { return _roundingAmount; }
            set
            {
                _roundingAmount = value;
                SetPropertyChanged(nameof(RoundingAmount));
            }
        }

        [JsonIgnore]
        decimal _netAmount { get; set; } = 0;

        public decimal NetAmount
        {
            get { return _netAmount; }
            set
            {
                //_netAmount = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                _netAmount = value;
                SetPropertyChanged(nameof(NetAmount));
                SetPropertyChanged(nameof(OutstandingAmount));
                SetPropertyChanged(nameof(OutstandingText));
                SetPropertyChanged(nameof(IsPartialPaid));
            }
        }

        [JsonIgnore]
        decimal _totalPaid { get; set; } = 0;

        public decimal TotalPaid
        {
            get { return _totalPaid; }
            set
            {
                //_totalPaid = Math.Round(value, 2, MidpointRounding.AwayFromZero);;
                _totalPaid = value;
                SetPropertyChanged(nameof(TotalPaid));
                SetPropertyChanged(nameof(IsPartialPaid));
            }
        }

        [JsonIgnore]
        decimal _totalPay { get; set; }

        public decimal TotalPay
        {
            get { return _totalPay; }
            set
            {
                //_totalPay = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                _totalPay = value;
                StrTenderAmount = _totalPay.ToString("C");
                SetPropertyChanged(nameof(TotalPay));
            }
        }

        [JsonIgnore]
        decimal _TenderAmount { get; set; }

        public decimal TenderAmount
        {
            get
            {
                return _TenderAmount;
            }
            set
            {
                if (value != _TenderAmount)
                {
                    //_TenderAmount = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                    _TenderAmount = value;
                    SetPropertyChanged(nameof(TenderAmount));
                    //StrTenderAmount = TenderAmount.ToString("C");
                    UpdateQuickCashOption(value);
                }
            }
        }

        [JsonIgnore]
        string _strTenderAmount { get; set; }

        public string StrTenderAmount
        {
            get
            {
                return _strTenderAmount;
            }
            set
            {
                // Ticket #9243 Start:Following code is commented to resolve issue; Unable to enter negative sign for tender amount.By Nikhil.
                //if (value == "-")
                //    value = "0"; 
                //Ticket #9243 End:By Nikhil.

                _strTenderAmount = value;
                if (!string.IsNullOrEmpty(value))
                {
                    decimal result;
                    if (decimal.TryParse(value, NumberStyles.Currency, null, out result))
                        TenderAmount = result;
                    else
                        TenderAmount = 0;
                }
                else
                    TenderAmount = 0;
                SetPropertyChanged(nameof(StrTenderAmount));
            }
        }

        [JsonIgnore]
        decimal _QuickCashFirstOption { get; set; }
        public decimal QuickCashFirstOption { get { return _QuickCashFirstOption; } set { _QuickCashFirstOption = value; SetPropertyChanged(nameof(QuickCashFirstOption)); } }

        [JsonIgnore]
        decimal _QuickCashSecondOption { get; set; }
        public decimal QuickCashSecondOption { get { return _QuickCashSecondOption; } set { _QuickCashSecondOption = value; SetPropertyChanged(nameof(QuickCashSecondOption)); } }

        [JsonIgnore]
        decimal _QuickCashThirdOption { get; set; }
        public decimal QuickCashThirdOption { get { return _QuickCashThirdOption; } set { _QuickCashThirdOption = value; SetPropertyChanged(nameof(QuickCashThirdOption)); } }

        [JsonIgnore]
        decimal _QuickCashFourOption { get; set; }
        public decimal QuickCashFourOption { get { return _QuickCashFourOption; } set { _QuickCashFourOption = value; SetPropertyChanged(nameof(QuickCashFourOption)); } }




        [JsonIgnore]
        decimal _totalTender { get; set; }

        public decimal TotalTender { get; set; }

        [JsonIgnore]
        decimal _changeAmount { get; set; }

        public decimal ChangeAmount
        {
            get { return _changeAmount; }
            set
            {
                //_changeAmount = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                _changeAmount = value;
                SetPropertyChanged(nameof(ChangeAmount));
            }
        }

        //decimal _outstandingAmount { get; set; }
        //public decimal OutstandingAmount
        //{
        //	get { return _outstandingAmount; }
        //	set
        //	{
        //		_outstandingAmount = value;
        //		SetPropertyChanged(nameof(OutstandingAmount));
        //	}
        //}




        public string Currency { get; set; }

        public int ServedBy { get; set; }
        public string ServedByName { get; set; }

        [JsonIgnore]
        string _note { get; set; }

        public string Note
        {
            get { return _note; }
            set
            {
                _note = value;
                SetPropertyChanged(nameof(Note));
            }
        }


        public InvoiceFrom InvoiceFrom { get; set; }
        public string InvoiceFromName { get; set; }


        public int? ReferenceInvoiceId { get; set; }

        //[JsonIgnore]
        public string ReferenceTempInvoiceId { get; set; }

        public string ReferenceNote { get; set; }

        public string ExchangeReferenceNote { get; set; }


        [JsonIgnore]
        bool _IsReStockWhenRefund { get; set; }

        public bool IsReStockWhenRefund { get { return _IsReStockWhenRefund; } set { _IsReStockWhenRefund = value; SetPropertyChanged(nameof(IsReStockWhenRefund)); } }

        public bool CanBeModified { get; set; }

        public string TrackNumber { get; set; }
        public string TrackURL { get; set; }
        public string TrackDetails { get; set; }
        public string ReceiptHTML { get; set; }

        public ObservableCollection<InvoiceLineItemDto> InvoiceLineItems { get; set; }

        public ObservableCollection<InvoicePaymentDto> InvoicePayments1 { get; set; }
        public ObservableCollection<InvoicePaymentDto> InvoicePayments { get; set; }
        public ObservableCollection<InvoicePaymentDto> ToRefundPayments { get; set; }

        public ObservableCollection<InvoicePaymentDto> BackorderPayments { get; set; }

        public ObservableCollection<InvoiceHistoryDto> InvoiceHistories { get; set; }

        [JsonIgnore]
        ObservableCollection<InvoicePaymentDto> _InvoiceRefundPayments { get; set; }
        public ObservableCollection<InvoicePaymentDto> InvoiceRefundPayments { get { return _InvoiceRefundPayments; } set { _InvoiceRefundPayments = value; SetPropertyChanged(nameof(InvoiceRefundPayments)); } }


        [JsonIgnore]
        public ObservableCollection<InvoicePaymentDto> ActiveInvoicePayments
        {
            get
            {
                if (InvoicePayments != null)
                {
                    return new ObservableCollection<InvoicePaymentDto>(InvoicePayments.Where(x => !x.IsDeleted));
                }
                else
                {
                    return InvoicePayments;
                }

            }
        }

        public ObservableCollection<LineItemTaxDto> Taxgroup { get; set; }

        [JsonIgnore]
        ObservableCollection<LineItemTaxDto> _ReceiptTaxList { get; set; }
        public ObservableCollection<LineItemTaxDto> ReceiptTaxList { get { return _ReceiptTaxList; } set { _ReceiptTaxList = value; SetPropertyChanged(nameof(ReceiptTaxList)); } }

        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public bool DoNotUpdateInvenotry { get; set; }
        public ThirdPartySyncStatus ThirdPartySyncStatus { get; set; }



        public decimal LoyaltyPoints { get; set; }


        public decimal PriceListCustomerCurrentLoyaltyPoints { get; set; }
        public decimal CustomerCurrentLoyaltyPoints { get; set; }
        public decimal LoyaltyPointsValue { get; set; }

        [JsonIgnore]
        decimal _outstandingAmount { get; set; } = 0;

        public decimal OutstandingAmount
        {
            get
            {
                var result = Math.Round(NetAmount.ToPositive(), 2, MidpointRounding.AwayFromZero) - Math.Round(TotalPaid.ToPositive(), 2, MidpointRounding.AwayFromZero);
                //var result = NetAmount - TotalPaid;
                return result;
                //return _outstandingAmount; 

            }
            set
            {
                _outstandingAmount = value;
                SetPropertyChanged(nameof(OutstandingAmount));
            }
        }

        [JsonIgnore]
        public bool IsPartialPaid
        {
            get
            {
                // return NetAmount > TotalPaid && TotalPaid != 0;

                return Math.Round(NetAmount, 2, MidpointRounding.AwayFromZero) > Math.Round(TotalPaid, 2, MidpointRounding.AwayFromZero) && Math.Round(TotalPaid, 2, MidpointRounding.AwayFromZero) != 0;
            }
        }

        [JsonIgnore]
        public string OutstandingText
        {
            get
            {
                switch (Status)
                {
                    case InvoiceStatus.Completed:
                        return "Paid";
                    case InvoiceStatus.Parked:
                        if (NetAmount > TotalPaid && TotalPaid != 0)
                        {
                            //return string.Format("Partially paid ({0}{1})",Settings.StoreZoneAndFormatDetail.CurrencySymbol,TotalPaid);
                            return string.Format("Partially paid ({0})", TotalPaid.ToString("C"));
                        }
                        else if (NetAmount == TotalPaid && NetAmount != 0)
                        {
                            return "Paid";
                        }
                        else
                        {
                            return "Pending";
                        }
                    case InvoiceStatus.Voided:
                        //Ticket start:#25357 iOS - About Register Summary Change for Refunded and Discarded Sales.by rupesh
                        return FinancialStatus == FinancialStatus.Refunded ? "Refunded" : "Voided";
                    //Ticket end:#25357 .by rupesh
                    case InvoiceStatus.Refunded:
                        return "Refunded";
                    case InvoiceStatus.Exchange:
                        return "Exchanged";
                    case InvoiceStatus.LayBy:
                        if (NetAmount > TotalPaid && TotalPaid != 0)
                        {
                            //return string.Format("Partially paid ({0}{1})",Settings.StoreZoneAndFormatDetail.CurrencySymbol,TotalPaid);
                            return string.Format("Partially paid ({0})", TotalPaid.ToString("C"));
                        }
                        //Ticket start:#26168 iPad : Shopify sales not showing up paid in Hike store.by rupesh
                        else if (NetAmount == TotalPaid)
                        {
                            return "Paid";
                        }
                        //Ticket end:#26168 .by rupesh
                        else
                        {
                            return "Pending";
                        }
                    case InvoiceStatus.OnAccount:
                        return "Pending";
                    case InvoiceStatus.Pending:
                        return "Pending";
                    case InvoiceStatus.BackOrder:
                        if (NetAmount > TotalPaid && TotalPaid != 0)
                        {
                            //return string.Format("Partially paid ({0}{1})",Settings.StoreZoneAndFormatDetail.CurrencySymbol,TotalPaid);
                            return string.Format("Partially paid ({0})", TotalPaid.ToString("C"));
                        }
                        else
                        {
                            return "Pending";
                        }
                    case InvoiceStatus.Quote:
                        return FinancialStatus == FinancialStatus.Closed ? "Converted to sale" : "Open";

                    default:
                        return "-";
                }
            }
        }

        [JsonIgnore]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case InvoiceStatus.Parked:
                        if ((NetAmount - TotalPaid) == 0)
                        {
                            //return "AWAITING FULFILLMENT";
                            return "Awaiting fulfillment";
                        }
                        else
                        {
                            //return Status.ToString().ToUpper();
                            return Status.ToString();
                        }
                    case InvoiceStatus.LayBy:
                        if ((NetAmount - TotalPaid) == 0)
                        {
                            //return "AWAITING FULFILLMENT";
                            return "Awaiting fulfillment";
                        }
                        else
                        {
                            //return Status.ToString().ToUpper();
                            return Status.ToString();
                        }
                    case InvoiceStatus.Completed:
                        return "Complete";
                    default:
                        //return Status.ToString().ToUpper();
                        return Status.ToString();
                }
            }
        }



        //public string FulfillmentStatus { get; set; }  
        //public string PaymentStatus { get; set; }


        bool _isSync { get; set; }
        public bool isSync
        {
            get { return _isSync; }
            set
            {
                _isSync = value;
                SetPropertyChanged(nameof(isSync));
            }
        }

        bool _isCustomerChange { get; set; }
        public bool IsCustomerChange
        {
            get { return _isCustomerChange; }
            set
            {
                _isCustomerChange = value;
                SetPropertyChanged(nameof(IsCustomerChange));
            }
        }

        bool _createdPoOrNot { get; set; }
        public bool CreatedPoOrNot
        {
            get { return _createdPoOrNot; }
            set
            {
                _createdPoOrNot = value;
                SetPropertyChanged(nameof(CreatedPoOrNot));
                SetPropertyChanged(nameof(FullfilmentStatus));
            }
        }

        [JsonIgnore]
        public string FullfilmentStatus
        {
            get
            {
                if (Status == InvoiceStatus.BackOrder)
                {
                    if (CreatedPoOrNot)
                        return Status + " | " + LanguageExtension.Localize("POCreatedText");
                    else
                        return Status + " | " + LanguageExtension.Localize("CreatePOText");
                }
                else
                    return Status.ToString();
            }
        }

        public int? CurrentRegister { get; set; }
        public DateTime? FinalizeDate { get; set; }
        public int? RegisterClosureId { get; set; }
        public bool HasError { get; set; }

        void UpdateQuickCashOption(decimal TenderAmount)
        {

            QuickCashFirstOption = 0;
            QuickCashSecondOption = 0;
            QuickCashThirdOption = 0;
            QuickCashFourOption = 0;

            var QuickCashOptions = TenderAmount.getQuickCashOptions();
            if (QuickCashOptions != null && QuickCashOptions.Count > 0)
            {
                QuickCashFirstOption = QuickCashOptions[0];

                if (QuickCashOptions.Count > 1)
                {
                    QuickCashSecondOption = QuickCashOptions[1];
                }
                if (QuickCashOptions.Count > 2)
                {
                    QuickCashThirdOption = QuickCashOptions[2];
                }
                if (QuickCashOptions.Count > 3)
                {
                    QuickCashFourOption = QuickCashOptions[3];
                }
            }
        }

        [JsonIgnore]
        public bool CustomerGroupDiscountType { get; set; }


        [JsonIgnore]
        public string ChangeAmmountDetail
        {
            get
            {
                if (ChangeAmount == 0 || InvoicePayments == null || !InvoicePayments.Any())
                    return "";
                if (Convert.ToDouble(ChangeAmount) < 0.005 && Convert.ToDouble(ChangeAmount) > 0.00)
                    return "";


                return "(Tendered : " + InvoicePayments.LastOrDefault().TenderedAmount + ", " + Settings.CurrentRegister.ReceiptTemplate?.ChangeLable + ": " + ChangeAmount + ")";
            }
        }

        public decimal? BackOrdertotalPaid { get; set; }

        decimal _backOrdertotal { get; set; }
        public decimal BackOrdertotal { get { return _backOrdertotal; } set { _backOrdertotal = value; SetPropertyChanged(nameof(BackOrdertotal)); } }

        public decimal BackorderDeposite { get; set; }
        //Ticket #9209 Start:Back order deposit amount issue. By Nikhil.
        [JsonIgnore]
        string _strBackorderDeposite { get; set; }
        public string StrBackorderDeposite
        {
            get
            {
                return _strBackorderDeposite;
            }
            set
            {
                _strBackorderDeposite = value;
                if (!string.IsNullOrEmpty(value))
                {
                    decimal result;
                    if (decimal.TryParse(value, out result))
                        BackorderDeposite = result;
                    else
                        BackorderDeposite = 0;
                }
                else
                    BackorderDeposite = 0;
                SetPropertyChanged(nameof(StrBackorderDeposite));
            }
        }
        //Ticket #9209 End:By Nikhil.

        //Ticket start:#22406 Quote sale.by rupesh
        public FinancialStatus FinancialStatus { get; set; }
        //Ticket end:#22406 Quote sale.by rupesh

        //Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
        [JsonIgnore]
        int? _DeliveryAddressId { get; set; }
        public int? DeliveryAddressId { get { return _DeliveryAddressId; } set { _DeliveryAddressId = value; SetPropertyChanged(nameof(DeliveryAddressId)); } }

        [JsonIgnore]
        CustomerAddressDto _DeliveryAddress { get; set; }
        public CustomerAddressDto DeliveryAddress { get { return _DeliveryAddress; } set { _DeliveryAddress = value; SetPropertyChanged(nameof(DeliveryAddress)); } }
        //Ticket end:#26664 IOS - New feature :: Customer delivery address.by rupesh


        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total

        public int? TipTaxId { get; set; }
        public decimal? TipTaxRate { get; set; }
        public string TipTaxName { get; set; }
        public decimal TipTaxAmount { get; set; }
        public decimal SurchargeDisplayValuecal { get; set; }
        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
        /*
        public ObservableCollection<InvoiceTaxDto> InvoiceTaxes { get; set; }*/

        //Ticket start:#29657 Seach customer using phone number.by rupesh
        public string CustomerPhone { get; set; }
        //Ticket end:#29657 .by rupesh
        public string QRString { get; set; }

        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        public decimal TotalPaymentSurcharge { get; set; }
        [JsonIgnore]
        decimal _totalTipSurcharge { get; set; }
        [JsonIgnore]
        public decimal TotalTipSurcharge { get { return _totalTipSurcharge; } set { _totalTipSurcharge = value; SetPropertyChanged(nameof(TotalTipSurcharge)); } }
        //End ticket #73190 By Pratik

        //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
        public string CustomFields { get; set; }
        public InvoiceOutstanding InvoiceOutstanding { get; set; }
        //End Ticket #63876 by Pratik

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public DateTime? InvoiceDueDate { get; set; }
        //End ticket #76208 by Pratik
        public string CreatedBy { get; set; }
        public int? CreatorUserId { get; set; }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public ObservableCollection<InvoiceFulfillmentDto> InvoiceFulfillments { get; set; }
        //End #84293 by Pratik

        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
        public string AgeVerificationImageUpload { get; set; }
        //Ticket:end:#90938 .by rupesh

        //#94565
        public string FloorTableName { get; set; }
        [JsonIgnore]
        private ObservableCollection<InvoiceFloorTableDto> _invoiceFloorTables;
        public ObservableCollection<InvoiceFloorTableDto> InvoiceFloorTables { get { return _invoiceFloorTables; } set { _invoiceFloorTables = value; SetPropertyChanged(nameof(InvoiceFloorTables)); } }
        //#94565
        
        //#95241
        public int? approvedByUser { get; set; }
        public string approvedByUserName { get; set; }
        //#95241


        public LocalInvoiceDB ToModel()
        {
            LocalInvoiceDB invoiceDB = new LocalInvoiceDB
            {
                Id = Id,
                IsActive = IsActive,
                InvoiceTempId = string.IsNullOrEmpty(InvoiceTempId) ? nameof(LocalInvoiceDto) + "_" + Guid.NewGuid().ToString() : InvoiceTempId,
                Number = Number,
                TransactionDate = TransactionDate,
                CustomerId = CustomerId,
                CustomerTempId = CustomerTempId,
                CustomerDetail = CustomerDetail?.ToModel(),
                CustomerGroupName = CustomerGroupName,
                CustomerName = CustomerName,
                CustomerGroupId = CustomerGroupId,
                CustomerGroupDiscount = CustomerGroupDiscount,
                CustomerGroupDiscountNote = CustomerGroupDiscountNote,
                CustomerGroupDiscountNoteInside = CustomerGroupDiscountNoteInside,
                CustomerGroupDiscountNoteInsidePrice = CustomerGroupDiscountNoteInsidePrice,
                OutletId = OutletId,
                OutletName = OutletName,
                RegisterId = RegisterId,
                RegisterName = RegisterName,
                Barcode = Barcode,
                Status = (int)Status,
                LocalInvoiceStatus = (int)LocalInvoiceStatus,
                CurrentPaymentObject = CurrentPaymentObject,
                TaxInclusive = TaxInclusive,
                ApplyTaxAfterDiscount = ApplyTaxAfterDiscount,
                DiscountIsAsPercentage = DiscountIsAsPercentage,
                DiscountValue = DiscountValue,
                DiscoutType = DiscoutType,
                DiscountNote = DiscountNote,
                TipIsAsPercentage = TipIsAsPercentage,
                TipValue = TipValue,
                SubTotal = SubTotal,
                GiftCardTotal = GiftCardTotal,
                TotalDiscount = TotalDiscount,
                TotalShippingCost = TotalShippingCost,
                shippingTaxId = shippingTaxId,
                ShippingTaxRate = ShippingTaxRate,
                ShippingTaxName = ShippingTaxName,
                ShippingTaxAmount = ShippingTaxAmount,
                TotalTax = TotalTax,
                OtherCharges = OtherCharges,
                Tax = Tax,
                TotalTip = TotalTip,
                RoundingAmount = RoundingAmount,
                NetAmount = NetAmount,
                TotalPaid = TotalPaid,
                TotalPay = TotalPay,
                TenderAmount = TenderAmount,
                StrTenderAmount = StrTenderAmount,
                QuickCashFirstOption = QuickCashFirstOption,
                QuickCashSecondOption = QuickCashSecondOption,
                QuickCashThirdOption = QuickCashThirdOption,
                QuickCashFourOption = QuickCashFourOption,
                TotalTender = TotalTender,
                ChangeAmount = ChangeAmount,
                Currency = Currency,
                ServedBy = ServedBy,
                ServedByName = ServedByName,
                Note = Note,
                InvoiceFrom = (int)InvoiceFrom,
                InvoiceFromName = InvoiceFromName,
                ReferenceInvoiceId = ReferenceInvoiceId,
                ReferenceTempInvoiceId = ReferenceTempInvoiceId,
                ReferenceNote = ReferenceNote,
                ExchangeReferenceNote = ExchangeReferenceNote,
                IsReStockWhenRefund = IsReStockWhenRefund,
                CanBeModified = CanBeModified,
                TrackNumber = TrackNumber,
                TrackURL = TrackURL,
                TrackDetails = TrackDetails,
                ReceiptHTML = ReceiptHTML,
                CreationTime = CreationTime,
                LastModificationTime = LastModificationTime,
                DoNotUpdateInvenotry = DoNotUpdateInvenotry,
                ThirdPartySyncStatus = (int)ThirdPartySyncStatus,
                LoyaltyPoints = LoyaltyPoints,
                PriceListCustomerCurrentLoyaltyPoints = PriceListCustomerCurrentLoyaltyPoints,
                CustomerCurrentLoyaltyPoints = CustomerCurrentLoyaltyPoints,
                LoyaltyPointsValue = LoyaltyPointsValue,
                OutstandingAmount = OutstandingAmount,
                isSync = isSync,
                IsCustomerChange = IsCustomerChange,
                CreatedPoOrNot = CreatedPoOrNot,
                CurrentRegister = CurrentRegister,
                FinalizeDate = FinalizeDate,
                RegisterClosureId = RegisterClosureId,
                HasError = HasError,
                BackOrdertotalPaid = BackOrdertotalPaid,
                BackOrdertotal = BackOrdertotal,
                BackorderDeposite = BackorderDeposite,
                StrBackorderDeposite = StrBackorderDeposite,
                FinancialStatus = (int)FinancialStatus,
                DeliveryAddressId = DeliveryAddressId,
                DeliveryAddress = DeliveryAddress?.ToModel(),
                TipTaxId = TipTaxId,
                TipTaxRate = TipTaxRate,
                TipTaxName = TipTaxName,
                TipTaxAmount = TipTaxAmount,
                SurchargeDisplayValuecal = SurchargeDisplayValuecal,
                CustomerPhone = CustomerPhone,
                QRString = QRString,
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                TotalPaymentSurcharge = TotalPaymentSurcharge,
                //End ticket #73190
                //START ticket #76208 IOS:FR:Terms of payments by Pratik
                InvoiceDueDate = InvoiceDueDate,
                //End ticket #76208 by Pratik
                //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                CustomFields = CustomFields,
                InvoiceOutstanding = InvoiceOutstanding?.ToModel(),
                //End Ticket #63876 by Pratik
                CreatedBy = CreatedBy,
                CreatorUserId = CreatorUserId,
                //Ticket:start:#90938 IOS:FR Age varification.by rupesh
                AgeVerificationImageUpload = AgeVerificationImageUpload,
                //Ticket:end:#90938 .by rupesh                 
                InvoiceDetail = InvoiceDetail?.ToModel(), //Start #91991 By Pratik
                TableId = InvoiceFloorTables?.FirstOrDefault()?.TableId ?? 0,//#94565
                FloorTableName = FloorTableName,//#94565
                approvedByUserName = approvedByUserName, //#95241
                approvedByUser = approvedByUser, //#95241
                IsExchangeSale = IsExchangeSale //#97493
            };

            InvoiceLineItems?.ForEach(i => invoiceDB.InvoiceLineItems.Add(i.ToModel()));
            InvoicePayments1?.ForEach(i => invoiceDB.InvoicePayments1.Add(i.ToModel()));
            InvoicePayments?.ForEach(i => invoiceDB.InvoicePayments.Add(i.ToModel()));
            ToRefundPayments?.ForEach(i => invoiceDB.ToRefundPayments.Add(i.ToModel()));
            BackorderPayments?.ForEach(i => invoiceDB.BackorderPayments.Add(i.ToModel()));
            InvoiceHistories?.ForEach(i => invoiceDB.InvoiceHistories.Add(i.ToModel()));
            InvoiceRefundPayments?.ForEach(i => invoiceDB.InvoiceRefundPayments.Add(i.ToModel()));
            Taxgroup?.ForEach(i => invoiceDB.Taxgroup.Add(i.ToModel()));
            ReceiptTaxList?.ForEach(i => invoiceDB.ReceiptTaxList.Add(i.ToModel()));
            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            InvoiceFulfillments?.ForEach(i => invoiceDB.InvoiceFulfillments.Add(i.ToModel()));
            //End #84293 by Pratik
            InvoiceFloorTables?.ForEach(i => invoiceDB.InvoiceFloorTables.Add(i.ToModel())); //#94565

            return invoiceDB;
        }

        public static LocalInvoiceDto FromModel(LocalInvoiceDB invoiceDB)
        {
            if (invoiceDB == null)
                return null;

            LocalInvoiceDto invoiceDto = new LocalInvoiceDto
            {
                Id = invoiceDB.Id,
                IsActive = invoiceDB.IsActive,
                InvoiceTempId = invoiceDB.InvoiceTempId,
                Number = invoiceDB.Number,
                TransactionDate = invoiceDB.TransactionDate.UtcDateTime,
                CustomerId = invoiceDB.CustomerId,
                CustomerTempId = invoiceDB.CustomerTempId,
                CustomerDetail = invoiceDB.CustomerDetail != null ? CustomerDto_POS.FromModel(invoiceDB.CustomerDetail) : null,
                CustomerGroupName = invoiceDB.CustomerGroupName,
                CustomerName = invoiceDB.CustomerName,
                CustomerGroupId = invoiceDB.CustomerGroupId,
                CustomerGroupDiscount = invoiceDB.CustomerGroupDiscount,
                CustomerGroupDiscountNote = invoiceDB.CustomerGroupDiscountNote,
                CustomerGroupDiscountNoteInside = invoiceDB.CustomerGroupDiscountNoteInside,
                CustomerGroupDiscountNoteInsidePrice = invoiceDB.CustomerGroupDiscountNoteInsidePrice,
                OutletId = invoiceDB.OutletId,
                OutletName = invoiceDB.OutletName,
                RegisterId = invoiceDB.RegisterId,
                RegisterName = invoiceDB.RegisterName,
                Barcode = invoiceDB.Barcode,
                Status = (InvoiceStatus)invoiceDB.Status,
                LocalInvoiceStatus = (LocalInvoiceStatus)invoiceDB.LocalInvoiceStatus,
                CurrentPaymentObject = invoiceDB.CurrentPaymentObject,
                TaxInclusive = invoiceDB.TaxInclusive,
                ApplyTaxAfterDiscount = invoiceDB.ApplyTaxAfterDiscount,
                DiscountIsAsPercentage = invoiceDB.DiscountIsAsPercentage,
                DiscountValue = invoiceDB.DiscountValue,
                DiscoutType = invoiceDB.DiscoutType,
                DiscountNote = invoiceDB.DiscountNote,
                TipIsAsPercentage = invoiceDB.TipIsAsPercentage,
                TipValue = invoiceDB.TipValue,
                SubTotal = invoiceDB.SubTotal,
                GiftCardTotal = invoiceDB.GiftCardTotal,
                TotalDiscount = invoiceDB.TotalDiscount,
                TotalShippingCost = invoiceDB.TotalShippingCost,
                shippingTaxId = invoiceDB.shippingTaxId,
                ShippingTaxRate = invoiceDB.ShippingTaxRate,
                ShippingTaxName = invoiceDB.ShippingTaxName,
                ShippingTaxAmount = invoiceDB.ShippingTaxAmount,
                TotalTax = invoiceDB.TotalTax,
                OtherCharges = invoiceDB.OtherCharges,
                Tax = invoiceDB.Tax,
                TotalTip = invoiceDB.TotalTip,
                RoundingAmount = invoiceDB.RoundingAmount,
                NetAmount = invoiceDB.NetAmount,
                TotalPaid = invoiceDB.TotalPaid,
                TotalPay = invoiceDB.TotalPay,
                TenderAmount = invoiceDB.TenderAmount,
                StrTenderAmount = invoiceDB.StrTenderAmount,
                QuickCashFirstOption = invoiceDB.QuickCashFirstOption,
                QuickCashSecondOption = invoiceDB.QuickCashSecondOption,
                QuickCashThirdOption = invoiceDB.QuickCashThirdOption,
                QuickCashFourOption = invoiceDB.QuickCashFourOption,
                TotalTender = invoiceDB.TotalTender,
                ChangeAmount = invoiceDB.ChangeAmount,
                Currency = invoiceDB.Currency,
                ServedBy = invoiceDB.ServedBy,
                ServedByName = invoiceDB.ServedByName,
                Note = invoiceDB.Note,
                InvoiceFrom = (InvoiceFrom)invoiceDB.InvoiceFrom,
                InvoiceFromName = invoiceDB.InvoiceFromName,
                ReferenceInvoiceId = invoiceDB.ReferenceInvoiceId,
                ReferenceTempInvoiceId = invoiceDB.ReferenceTempInvoiceId,
                ReferenceNote = invoiceDB.ReferenceNote,
                ExchangeReferenceNote = invoiceDB.ExchangeReferenceNote,
                IsReStockWhenRefund = invoiceDB.IsReStockWhenRefund,
                CanBeModified = invoiceDB.CanBeModified,
                TrackNumber = invoiceDB.TrackNumber,
                TrackURL = invoiceDB.TrackURL,
                TrackDetails = invoiceDB.TrackDetails,
                ReceiptHTML = invoiceDB.ReceiptHTML,
                CreationTime = invoiceDB.CreationTime.UtcDateTime,
                LastModificationTime = invoiceDB.LastModificationTime?.UtcDateTime,
                DoNotUpdateInvenotry = invoiceDB.DoNotUpdateInvenotry,
                ThirdPartySyncStatus = (ThirdPartySyncStatus)invoiceDB.ThirdPartySyncStatus,
                LoyaltyPoints = invoiceDB.LoyaltyPoints,
                PriceListCustomerCurrentLoyaltyPoints = invoiceDB.PriceListCustomerCurrentLoyaltyPoints,
                CustomerCurrentLoyaltyPoints = invoiceDB.CustomerCurrentLoyaltyPoints,
                LoyaltyPointsValue = invoiceDB.LoyaltyPointsValue,
                OutstandingAmount = invoiceDB.OutstandingAmount,
                isSync = invoiceDB.isSync,
                IsCustomerChange = invoiceDB.IsCustomerChange,
                CreatedPoOrNot = invoiceDB.CreatedPoOrNot,
                CurrentRegister = invoiceDB.CurrentRegister,
                FinalizeDate = invoiceDB.FinalizeDate?.UtcDateTime,
                RegisterClosureId = invoiceDB.RegisterClosureId,
                HasError = invoiceDB.HasError,
                BackOrdertotalPaid = invoiceDB.BackOrdertotalPaid,
                BackOrdertotal = invoiceDB.BackOrdertotal,
                BackorderDeposite = invoiceDB.BackorderDeposite,
                StrBackorderDeposite = invoiceDB.StrBackorderDeposite,
                FinancialStatus = (FinancialStatus)invoiceDB.FinancialStatus,
                DeliveryAddressId = invoiceDB.DeliveryAddressId,
                DeliveryAddress = invoiceDB.DeliveryAddress != null ? CustomerAddressDto.FromModel(invoiceDB.DeliveryAddress) : null,
                TipTaxId = invoiceDB.TipTaxId,
                TipTaxRate = invoiceDB.TipTaxRate,
                TipTaxName = invoiceDB.TipTaxName,
                TipTaxAmount = invoiceDB.TipTaxAmount,
                SurchargeDisplayValuecal = invoiceDB.SurchargeDisplayValuecal,
                CustomerPhone = invoiceDB.CustomerPhone,
                QRString = invoiceDB.QRString,
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                TotalPaymentSurcharge = invoiceDB.TotalPaymentSurcharge,
                //End ticket #73190
                //START ticket #76208 IOS:FR:Terms of payments by Pratik
                InvoiceDueDate = invoiceDB.InvoiceDueDate?.UtcDateTime,
                //End ticket #76208 by Pratik
                //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                CustomFields = invoiceDB.CustomFields,
                InvoiceOutstanding = InvoiceOutstanding.FromModel(invoiceDB.InvoiceOutstanding),
                //End Ticket #63876 by Pratik
                CreatedBy = invoiceDB.CreatedBy,
                CreatorUserId = invoiceDB.CreatorUserId,
                //Ticket:start:#90938 IOS:FR Age varification.by rupesh
                AgeVerificationImageUpload = invoiceDB.AgeVerificationImageUpload,
                //Ticket:end:#90938.by rupesh                
                InvoiceDetail = OnAccountPONumberRequest.FromModel(invoiceDB.InvoiceDetail), //Start #91991 By Pratik
                FloorTableName = invoiceDB.FloorTableName,//#94565
                approvedByUserName = invoiceDB.approvedByUserName, //#95241
                approvedByUser = invoiceDB.approvedByUser, //#95241
                IsExchangeSale = invoiceDB.IsExchangeSale //#97493
            };

            if (invoiceDB.InvoiceLineItems != null)
                invoiceDto.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(invoiceDB.InvoiceLineItems.Select(a => InvoiceLineItemDto.FromModel(a))?.ToList());

            if (invoiceDB.InvoicePayments1 != null)
                invoiceDto.InvoicePayments1 = new ObservableCollection<InvoicePaymentDto>(invoiceDB.InvoicePayments1.Select(a => InvoicePaymentDto.FromModel(a))?.ToList());

            if (invoiceDB.InvoicePayments != null)
                invoiceDto.InvoicePayments = new ObservableCollection<InvoicePaymentDto>(invoiceDB.InvoicePayments.Select(a => InvoicePaymentDto.FromModel(a))?.ToList());

            if (invoiceDB.ToRefundPayments != null)
                invoiceDto.ToRefundPayments = new ObservableCollection<InvoicePaymentDto>(invoiceDB.ToRefundPayments.Select(a => InvoicePaymentDto.FromModel(a))?.ToList());

            if (invoiceDB.BackorderPayments != null)
                invoiceDto.BackorderPayments = new ObservableCollection<InvoicePaymentDto>(invoiceDB.BackorderPayments.Select(a => InvoicePaymentDto.FromModel(a))?.ToList());

            if (invoiceDB.InvoiceHistories != null)
                invoiceDto.InvoiceHistories = new ObservableCollection<InvoiceHistoryDto>(invoiceDB.InvoiceHistories.Select(a => InvoiceHistoryDto.FromModel(a))?.ToList());

            if (invoiceDB.InvoiceRefundPayments != null)
                invoiceDto.InvoiceRefundPayments = new ObservableCollection<InvoicePaymentDto>(invoiceDB.InvoiceRefundPayments.Select(a => InvoicePaymentDto.FromModel(a))?.ToList());

            if (invoiceDB.Taxgroup != null)
                invoiceDto.Taxgroup = new ObservableCollection<LineItemTaxDto>(invoiceDB.Taxgroup.Select(a => LineItemTaxDto.FromModel(a))?.ToList());

            if (invoiceDB.ReceiptTaxList != null)
                invoiceDto.ReceiptTaxList = new ObservableCollection<LineItemTaxDto>(invoiceDB.ReceiptTaxList.Select(a => LineItemTaxDto.FromModel(a))?.ToList());

            //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
            if (invoiceDB.InvoiceFulfillments != null)
                invoiceDto.InvoiceFulfillments = new ObservableCollection<InvoiceFulfillmentDto>(invoiceDB.InvoiceFulfillments.Select(a => InvoiceFulfillmentDto.FromModel(a))?.ToList());
            //End #84293 by Pratik

            //#94565
            if (invoiceDB.InvoiceFloorTables != null)
                invoiceDto.InvoiceFloorTables = new ObservableCollection<InvoiceFloorTableDto>(invoiceDB.InvoiceFloorTables.Select(a => InvoiceFloorTableDto.FromModel(a))?.ToList());
           //#94565

            return invoiceDto;
        }

    }

    public partial class InvoiceDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        [PrimaryKey]
        public string KeyId { get; set; }
        public string InvoiceTempId { get; set; }
        public bool IsSNEFSaleHistory { get; set; }
        public int TanentId { get; set; } = 0;
        public bool IscallFromPayment { get; set; } = false;
        public int InvoiceUpdateFrom { get; set; }
        public string Number { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerTempId { get; set; }

        public CustomerDB_POS CustomerDetail { get; set; }
        public string CustomerGroupName { get; set; }

        public bool IsExchangeSale { get; set; } //#97493

        public string CustomerName { get; set; }
        public int? CustomerGroupId { get; set; }
        public decimal? CustomerGroupDiscount { get; set; }

        public string CGDNote { get; set; }
        public string CGDNInside { get; set; }
        public decimal CGDNInsidePrice { get; set; }

        public int OutletId { get; set; }

        public string OutletName { get; set; }
        public int? RegisterId { get; set; }
        public string RegisterName { get; set; }

        public string Barcode { get; set; }

        public int Status { get; set; }
        public int LocalInvoiceStatus { get; set; }

        public string CurrentPaymentObject { get; set; }
        public bool TaxInclusive { get; set; }

        public bool ApplyTaxAfterDiscount { get; set; }


        public bool DiscountIsAsPercentage { get; set; }

        public decimal DiscountValue { get; set; }
        public string DiscoutType { get; set; }

        public string DiscountNote { get; set; }
        public bool TipIsAsPercentage { get; set; }

        public decimal TipValue { get; set; }


        public decimal SubTotal { get; set; }

        public decimal GiftCardTotal { get; set; }


        public decimal TotalDiscount { get; set; }

        public decimal TotalShippingCost { get; set; }

        public int? shippingTaxId { get; set; }
        public decimal? ShippingTaxRate { get; set; }

        public string ShippingTaxName { get; set; }
        public decimal? ShippingTaxAmount { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalTip { get; set; }
        public decimal RoundingAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPay { get; set; }
        public decimal TenderAmount { get; set; }

        public string StrTenderAmount { get; set; }

        public decimal QuickCashFirstOption { get; set; }

        public decimal QuickCashSecondOption { get; set; }

        public decimal QuickCashThirdOption { get; set; }
        public decimal QuickCashFourOption { get; set; }

        public decimal TotalTender { get; set; }

        public decimal ChangeAmount { get; set; }

        public string Currency { get; set; }

        public int ServedBy { get; set; }
        public string ServedByName { get; set; }


        public string Note { get; set; }

        public int? InvoiceFrom { get; set; }
        public string InvoiceFromName { get; set; }


        public int? ReferenceInvoiceId { get; set; }

        public string ReferenceTempInvoiceId { get; set; }

        public string ReferenceNote { get; set; }

        public string ExchangeReferenceNote { get; set; }

        public bool IsReStockWhenRefund { get; set; }
        public bool CanBeModified { get; set; }

        public string TrackNumber { get; set; }
        public string TrackURL { get; set; }
        public string TrackDetails { get; set; }
        public string ReceiptHTML { get; set; }

        public IList<InvoiceLineItemDB> InvoiceLineItems { get; }
        public IList<InvoicePaymentDB> InvoicePayments1 { get; }
        public IList<InvoicePaymentDB> InvoicePayments { get; }
        public IList<InvoicePaymentDB> ToRefundPayments { get; }

        public IList<InvoicePaymentDB> BackorderPayments { get; }

        public IList<InvoiceHistoryDB> InvoiceHistories { get; }

        public IList<InvoicePaymentDB> InvoiceRefundPayments { get; }

        public IList<LineItemTaxDB> Taxgroup { get; }

        public IList<LineItemTaxDB> ReceiptTaxList { get; }

        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset? LastModificationTime { get; set; }
        public bool DoNotUpdateInvenotry { get; set; }
        public int ThirdPartySyncStatus { get; set; }
        public decimal LoyaltyPoints { get; set; }
        public decimal PLCCLoyaltyPoints { get; set; }
        public decimal CCLoyaltyPoints { get; set; }
        public decimal LoyaltyPointsValue { get; set; }

        public OnAccountPONumberRequestDB InvoiceDetail { get; set; } //Start #91991 By Pratik

        public decimal OutstandingAmount { get; set; }

        public bool isSync { get; set; }
        public bool IsCustomerChange { get; set; }
        public bool CreatedPoOrNot { get; set; }

        public int? CurrentRegister { get; set; }
        public DateTimeOffset? FinalizeDate { get; set; }
        public int? RegisterClosureId { get; set; }
        public bool HasError { get; set; }


        public decimal? BackOrdertotalPaid { get; set; }

        public decimal BackOrdertotal { get; set; }
        public decimal BackorderDeposite { get; set; }
        public string StrBackorderDeposite { get; set; }
        public int FinancialStatus { get; set; }
        public int? DeliveryAddressId { get; set; }

        public CustomerAddressDB DeliveryAddress { get; set; }



        public int? TipTaxId { get; set; }

        public decimal? TipTaxRate { get; set; }

        public string TipTaxName { get; set; }

        public decimal TipTaxAmount { get; set; }

        public decimal SurchargeDisplayValuecal { get; set; }


        public decimal SDInSaleHistory { get; set; }

        public string CustomerPhone { get; set; }
        public string QRString { get; set; }

        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        public decimal TotalPaymentSurcharge { get; set; }
        //End ticket #73190 By Pratik

        //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
        public string CustomFields { get; set; }
        public InvoiceOutstandingDB InvoiceOutstanding { get; set; }
        //End Ticket #63876 by Pratik

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public DateTimeOffset? InvoiceDueDate { get; set; }
        //End ticket #76208 by Pratik
        public string CreatedBy { get; set; }
        public int? CreatorUserId { get; set; }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public IList<InvoiceFulfillmentDB> InvoiceFulfillments { get; }
        public int InvoiceFulfillCount { get; set; }
        //End #84293 by Pratik

        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
        public string AgeVerificationImageUpload { get; set; }
        //Ticket:end:#90938 .by rupesh
        //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time.by rupesh
        public bool IsEditBackOrderFromSaleHistory { get; set; }
        //Ticket end:#84289 .by rupesh
        public IList<InvoiceFloorTableDB> InvoiceFloorTables { get; } //#94565
        public int TableId { get; set; } //#94565
        public string FloorTableName { get; set; } //#94565

        //#95241
        public int? approvedByUser { get; set; }
        public string approvedByUserName { get; set; }
        //#95241


    }

    public partial class LocalInvoiceDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        [PrimaryKey]
        public string InvoiceTempId { get; set; }
        public bool IsSerialNumberEditableFromSaleHistory { get; set; }
        public int TanentId { get; set; } = 0;
        public bool IscallFromPayment { get; set; } = false;
        public int InvoiceUpdateFrom { get; set; }
        public string Number { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerTempId { get; set; }

        public bool IsExchangeSale { get; set; } //#97493

        public CustomerDB_POS CustomerDetail { get; set; }
        public string CustomerGroupName { get; set; }

        public string CustomerName { get; set; }
        public int? CustomerGroupId { get; set; }
        public decimal? CustomerGroupDiscount { get; set; }

        public string CustomerGroupDiscountNote { get; set; }
        public string CustomerGroupDiscountNoteInside { get; set; }
        public decimal CustomerGroupDiscountNoteInsidePrice { get; set; }

        public int OutletId { get; set; }

        public string OutletName { get; set; }
        public int? RegisterId { get; set; }
        public string RegisterName { get; set; }

        public string Barcode { get; set; }

        public int Status { get; set; }
        public int LocalInvoiceStatus { get; set; }

        public string CurrentPaymentObject { get; set; }
        public bool TaxInclusive { get; set; }

        public bool ApplyTaxAfterDiscount { get; set; }

        public OnAccountPONumberRequestDB InvoiceDetail { get; set; } //Start #91991 By Pratik
        public bool DiscountIsAsPercentage { get; set; }

        public decimal DiscountValue { get; set; }
        public string DiscoutType { get; set; }

        public string DiscountNote { get; set; }
        public bool TipIsAsPercentage { get; set; }

        public decimal TipValue { get; set; }


        public decimal SubTotal { get; set; }

        public decimal GiftCardTotal { get; set; }


        public decimal TotalDiscount { get; set; }

        public decimal TotalShippingCost { get; set; }

        public int? shippingTaxId { get; set; }
        public decimal? ShippingTaxRate { get; set; }

        public string ShippingTaxName { get; set; }
        public decimal? ShippingTaxAmount { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalTip { get; set; }
        public decimal RoundingAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPay { get; set; }
        public decimal TenderAmount { get; set; }

        public string StrTenderAmount { get; set; }

        public decimal QuickCashFirstOption { get; set; }

        public decimal QuickCashSecondOption { get; set; }

        public decimal QuickCashThirdOption { get; set; }
        public decimal QuickCashFourOption { get; set; }

        public decimal TotalTender { get; set; }

        public decimal ChangeAmount { get; set; }

        public string Currency { get; set; }

        public int ServedBy { get; set; }
        public string ServedByName { get; set; }


        public string Note { get; set; }

        public int? InvoiceFrom { get; set; }
        public string InvoiceFromName { get; set; }


        public int? ReferenceInvoiceId { get; set; }

        public string ReferenceTempInvoiceId { get; set; }

        public string ReferenceNote { get; set; }

        public string ExchangeReferenceNote { get; set; }

        public bool IsReStockWhenRefund { get; set; }
        public bool CanBeModified { get; set; }

        public string TrackNumber { get; set; }
        public string TrackURL { get; set; }
        public string TrackDetails { get; set; }
        public string ReceiptHTML { get; set; }

        public IList<InvoiceLineItemDB> InvoiceLineItems { get; }
        public IList<InvoicePaymentDB> InvoicePayments1 { get; }
        public IList<InvoicePaymentDB> InvoicePayments { get; }
        public IList<InvoicePaymentDB> ToRefundPayments { get; }

        public IList<InvoicePaymentDB> BackorderPayments { get; }

        public IList<InvoiceHistoryDB> InvoiceHistories { get; }

        public IList<InvoicePaymentDB> InvoiceRefundPayments { get; }

        public IList<LineItemTaxDB> Taxgroup { get; }

        public IList<LineItemTaxDB> ReceiptTaxList { get; }

        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset? LastModificationTime { get; set; }
        public bool DoNotUpdateInvenotry { get; set; }
        public int ThirdPartySyncStatus { get; set; }
        public decimal LoyaltyPoints { get; set; }
        public decimal PriceListCustomerCurrentLoyaltyPoints { get; set; }
        public decimal CustomerCurrentLoyaltyPoints { get; set; }
        public decimal LoyaltyPointsValue { get; set; }


        public decimal OutstandingAmount { get; set; }

        public bool isSync { get; set; }
        public bool IsCustomerChange { get; set; }
        public bool CreatedPoOrNot { get; set; }

        public int? CurrentRegister { get; set; }
        public DateTimeOffset? FinalizeDate { get; set; }
        public int? RegisterClosureId { get; set; }
        public bool HasError { get; set; }


        public decimal? BackOrdertotalPaid { get; set; }

        public decimal BackOrdertotal { get; set; }
        public decimal BackorderDeposite { get; set; }
        public string StrBackorderDeposite { get; set; }
        public int FinancialStatus { get; set; }
        public int? DeliveryAddressId { get; set; }

        public CustomerAddressDB DeliveryAddress { get; set; }



        public int? TipTaxId { get; set; }

        public decimal? TipTaxRate { get; set; }

        public string TipTaxName { get; set; }

        public decimal TipTaxAmount { get; set; }

        public decimal SurchargeDisplayValuecal { get; set; }


        public decimal SurchargeDisplayInSaleHistory { get; set; }

        public string CustomerPhone { get; set; }
        public string QRString { get; set; }
        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        public decimal TotalPaymentSurcharge { get; set; }
        //End ticket #73190 By Pratik

        //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
        public string CustomFields { get; set; }
        public InvoiceOutstandingDB InvoiceOutstanding { get; set; }
        //End Ticket #63876 by Pratik

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public DateTimeOffset? InvoiceDueDate { get; set; }
        //End ticket #76208 by Pratik

        public string CreatedBy { get; set; }
        public int? CreatorUserId { get; set; }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public IList<InvoiceFulfillmentDB> InvoiceFulfillments { get; }
        public int InvoiceFulfillCount { get; set; }
        //End #84293 by Pratik

        //Ticket:start:#90938 IOS:FR Age varification.by rupesh
        public string AgeVerificationImageUpload { get; set; }
        //Ticket:end:#90938.by rupesh

        public IList<InvoiceFloorTableDB> InvoiceFloorTables { get; } //#94565
        public int TableId { get; set; } //#94565   
        public string FloorTableName { get; set; } //#94565
       
        //#95241
        public int? approvedByUser { get; set; }
        public string approvedByUserName { get; set; }
        //#95241

    }

    //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
    public partial class InvoiceOutstanding
    {
        public decimal? previousOutstanding { get; set; }
        public decimal? currentSale { get; set; }
        public decimal? currentOutstanding { get; set; }
        public InvoiceOutstandingDB ToModel()
        {
            InvoiceOutstandingDB invoiceOutstandingDB = new InvoiceOutstandingDB
            {
                previousOutstanding = previousOutstanding,
                currentSale = currentSale,
                currentOutstanding = currentOutstanding
            };
            return invoiceOutstandingDB;
        }
        public static InvoiceOutstanding FromModel(InvoiceOutstandingDB invoiceOutstandingDB)
        {
            if (invoiceOutstandingDB == null)
                return null;
            InvoiceOutstanding invoiceOutstanding = new InvoiceOutstanding
            {
                previousOutstanding = invoiceOutstandingDB.previousOutstanding,
                currentSale = invoiceOutstandingDB.currentSale,
                currentOutstanding = invoiceOutstandingDB.currentOutstanding
            };
            return invoiceOutstanding;

        }

    }
    public partial class InvoiceOutstandingDB : IRealmObject
    {
        public decimal? previousOutstanding { get; set; }
        public decimal? currentSale { get; set; }
        public decimal? currentOutstanding { get; set; }

    }

    public class CustomFieldsResponce
    {
        public object shippingAddress { get; set; }
        public object billingAddress { get; set; }
        public InvoiceOutstanding invoiceOutstanding { get; set; }
    }
    //End Ticket #63876 by Pratik 

    //Start #91991 By Pratik
    public class OnAccountPONumberRequest
    {
        public int Key { get; set; }
        public string Value { get; set; }
        public int InvoiceId { get; set; }
        public string CreatedBy { get; set; }
        public int Id { get; set; }

        public OnAccountPONumberRequestDB ToModel()
        {
            OnAccountPONumberRequestDB model = new OnAccountPONumberRequestDB
            {
                Key = Key,
                Value = Value,
                Id = Id,
                InvoiceId = InvoiceId,
                CreatedBy = CreatedBy
            };
            return model;
        }
        public static OnAccountPONumberRequest FromModel(OnAccountPONumberRequestDB modeldb)
        {
            if (modeldb == null)
                return null;
            OnAccountPONumberRequest model = new OnAccountPONumberRequest
            {
                Key = modeldb.Key,
                Value = modeldb.Value,
                Id = modeldb.Id,
                InvoiceId = modeldb.InvoiceId,
                CreatedBy = modeldb.CreatedBy
            };
            return model;
        }
    }

    public partial class OnAccountPONumberRequestDB : IRealmObject
    {
        public int Key { get; set; }
        public string Value { get; set; }
        public int InvoiceId { get; set; }
        public string CreatedBy { get; set; }
        public int Id { get; set; }
    }
    //End #91991 By Pratik


    //#94565
    public class InvoiceFloorTableDto
    {
        public int InvoiceId { get; set; }
        public int? FloorId { get; set; }
        public int? TableId { get; set; }
        public DateTime? AssignedDateTime { get; set; }
        public DateTime? ReleasedDateTime { get; set; }
        public int Id { get; set; }
        public string TableName { get; set; }
        public string FloorName { get; set; }

        public InvoiceFloorTableDB ToModel()
        {
            InvoiceFloorTableDB model = new InvoiceFloorTableDB
            {
                Id = Id,
                AssignedDateTime = AssignedDateTime,
                ReleasedDateTime = ReleasedDateTime,
                FloorId = FloorId,
                TableId = TableId,
                InvoiceId = InvoiceId,
                TableName = TableName,
                FloorName = FloorName,
            };
            return model;
        }
        public static InvoiceFloorTableDto FromModel(InvoiceFloorTableDB modeldb)
        {
            if (modeldb == null)
                return null;
            InvoiceFloorTableDto model = new InvoiceFloorTableDto
            {
                Id = modeldb.Id,
                AssignedDateTime = modeldb.AssignedDateTime?.UtcDateTime,
                ReleasedDateTime = modeldb.ReleasedDateTime?.UtcDateTime,
                FloorId = modeldb.FloorId,
                TableId = modeldb.TableId,
                InvoiceId = modeldb.InvoiceId,
                TableName = modeldb.TableName,
                FloorName = modeldb.FloorName,
            };
            return model;
        }
    }

    public partial class InvoiceFloorTableDB : IRealmObject
    {
        public int InvoiceId { get; set; }
        public int? FloorId { get; set; }
        public int? TableId { get; set; }
        public DateTimeOffset? AssignedDateTime { get; set; }
        public DateTimeOffset? ReleasedDateTime { get; set; }
        public int Id { get; set; }
        public string TableName { get; set; }
        public string FloorName { get; set; }
    }
    
}