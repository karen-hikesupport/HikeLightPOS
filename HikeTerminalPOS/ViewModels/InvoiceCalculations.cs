using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using HikePOS.Models;
using HikePOS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
    public static class InvoiceCalculations
    {
        public static readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        public static ObservableCollection<ProductDto_POS> Products = new ObservableCollection<ProductDto_POS>();

        static ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        static CustomerServices customerService = new CustomerServices(customerApiService);


        static ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        static SaleServices saleService = new SaleServices(saleApiService);

        //Ticket start:#63960 iPad: Product sold on iPad even after 0 stock.by rupesh
        static ApiService<IProductApi> productApiService = new ApiService<IProductApi>();
        static ProductServices productService;
        //Ticket end:#63960.by rupesh

        //Start #84438 iOS : FR :add discount offers on product tag by Pratik
        static ApiService<IOfferApi> offerApiService = new ApiService<IOfferApi>();
        static OfferServices offerServices;
        //End #84438 by Pratik

        //Ticket start:#30959 iPad - New feature request :: Rule for discount offers when there are more than one offers applicable.by rupesh
        public  static ObservableCollection<OfferDto> OffersToSelectManually = null;
        public static InvoiceLineItemDto PreviousInvoiceLineItem = null;


        //Ticket start:#30959 .by rupesh
        public static decimal GetValuefromPercent(decimal fromValue, decimal? percent)
        {
            try
            {
                //if (percent != null && percent != 0)
                //{
                //    return (fromValue * percent.Value) / 100;
                //}

                if (percent == null)
                {
                    return 0;
                }
                if (fromValue == 0 && percent < 0)
                    fromValue = 1;
                else if (fromValue == 0)
                    return 0;//fromValue = 1; 

                return (fromValue * percent.Value) / 100;

            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return 0;

        }

        public static decimal GetPercentfromValue(decimal fromValue, decimal Tovalue)
        {
            try
            {
                if (Tovalue == 0)
                    return 0;
                return (fromValue * 100) / Tovalue;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return 0;
        }

        public static decimal GetDiscountPercentfromValue(decimal fromPrice, decimal price)
        {
            try
            {
                //if (fromPrice != 0 && price != 0)
                //{
                //    return (1 - (price / fromPrice)) * 100;
                //}

                if (fromPrice == 0)
                    return -(price) * 100;
                else
                    return (1 - (price / fromPrice)) * 100;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return 0;
        }

        public static decimal GetTaxAmountfromSellPrice(decimal sellprice, decimal taxRate)
        {
            try
            {
                return (100 * sellprice) / (100 + taxRate);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return 0;
        }

        public static decimal GetdiscountqtyfromBuyxGetx(decimal buyx, decimal getx, decimal actualQty)
        {
            try
            {
                if ((buyx + getx) > actualQty)
                    return 0;
                if ((buyx + getx) == actualQty)
                    return getx;

                if ((buyx + getx) < actualQty)
                {
                    var loopLength = actualQty / (buyx + getx);
                    var remainQty = actualQty;
                    var freeQty = 0;
                    for (var i = 1; i <= Convert.ToInt32(loopLength); i++)
                    {
                        remainQty = remainQty - (buyx + getx);
                        if (remainQty >= 0)
                            freeQty++;
                    }
                    return freeQty * getx;
                }

                //int x = buyx;
                //int y = getx;
                //decimal i = actualQty;

                //var r = i % (x + y);
                //var n = (i - r) / (x + y);
                //var py = Math.Max(0, r - x) + (n * y);
                //var px = i - py;
                //return px;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return actualQty;
        }

        public static decimal GetTaxAmountfromPercent(decimal fromPrice, decimal taxRate, decimal TotalTax)
        {
            try
            {
                return (fromPrice / TotalTax) * taxRate;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return 0;
        }

        public static decimal CalculateTaxInclusive(decimal price, decimal? TaxRate)
        {
            try
            {
                if (TaxRate == null)
                {
                    return 0;
                }

                var t1 = ((float)TaxRate.Value) * ((float)price);
                var t2 = (float)(100 + TaxRate);
                return (decimal)(t1 / t2);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return 0;
        }

        public static InvoiceLineItemDto CalculateLineItemTotal(InvoiceLineItemDto invoiceItem, InvoiceDto invoice, bool CallCalculateInvoiceTotal = true)
        {
            if (invoiceItem == null)
            {
                return null;
            }

            var discount = invoiceItem.DiscountValue;
            decimal discountperItem = 0;
            var copyretailPrice = invoiceItem.RetailPrice;
            var retailPrice = invoiceItem.RetailPrice;

            var maxQty = 0;

            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
            //Ticket start:#68994,#73763 Discounts not working on iPad.by rupesh
            if (invoice.CustomerGroupDiscount != null && invoice.CustomerGroupDiscount != 0
                && invoiceItem.InvoiceItemType != InvoiceItemType.GiftCard
                && invoiceItem.InvoiceItemType != InvoiceItemType.Discount
                && !invoiceItem.DisableDiscountIndividually)
            {
                if (invoice.Status == InvoiceStatus.Exchange)
                {
                    if (invoice.CustomerId > 0 && invoiceItem.IsExchangedProduct && invoiceItem.SoldPrice != invoiceItem.RetailPrice)
                    {
                        discountperItem = GetValuefromPercent(invoiceItem.RetailPrice, invoice.CustomerGroupDiscount);
                        retailPrice -= discountperItem;
                    }
                    else if (invoice.CustomerId > 0 && !invoiceItem.IsExchangedProduct)
                    {
                        discountperItem = GetValuefromPercent(invoiceItem.RetailPrice, invoice.CustomerGroupDiscount);
                        retailPrice -= discountperItem;
                    }
                }
                else
                {
                    discountperItem = GetValuefromPercent(invoiceItem.RetailPrice, invoice.CustomerGroupDiscount);
                    retailPrice -= discountperItem;
                }
            }
            else
            {
                if (invoiceItem.CustomerGroupDiscountPercent != null && invoiceItem.InvoiceItemType != InvoiceItemType.GiftCard && invoiceItem.InvoiceItemType != InvoiceItemType.Discount && !invoiceItem.DisableDiscountIndividually)
                {
                    if (invoice.Status == InvoiceStatus.Exchange)
                    {
                        if (invoiceItem.IsExchangedProduct && invoiceItem.SoldPrice != invoiceItem.RetailPrice)
                        {
                            discountperItem = GetValuefromPercent(invoiceItem.RetailPrice, invoiceItem.CustomerGroupDiscountPercent);
                            retailPrice -= discountperItem;
                        }
                        else if (invoice.CustomerId > 0 && !invoiceItem.IsExchangedProduct)
                        {
                            discountperItem = GetValuefromPercent(invoiceItem.RetailPrice, invoiceItem.CustomerGroupDiscountPercent);
                            retailPrice -= discountperItem;
                        }
                    }
                    else
                    {
                        discountperItem = GetValuefromPercent(invoiceItem.RetailPrice, invoiceItem.CustomerGroupDiscountPercent);
                        retailPrice -= discountperItem;
                    }
                }
            }
            //Ticket end:#68994,#73763.by rupesh
            //End Ticket #74344 By pratik

            if (discount == 0)
                invoiceItem.DiscountIsAsPercentage = true;

            if (invoiceItem.DiscountIsAsPercentage)
            {
                discountperItem += GetValuefromPercent(retailPrice, discount);
            }
            else
                discountperItem += discount;

            if (retailPrice > 0 && discountperItem > invoiceItem.RetailPrice)
                discountperItem = invoiceItem.RetailPrice;

            if (invoiceItem.MarkupValue == 0 || invoiceItem.MarkupValue == null)
            {
                invoiceItem.TotalDiscount = discountperItem * invoiceItem.Quantity;
                invoiceItem.SoldPrice = Math.Round((invoiceItem.RetailPrice - discountperItem), Settings.StoreDecimalDigit,MidpointRounding.AwayFromZero); //#96092
            }

            //invoiceItem.TotalDiscount = discountperItem * invoiceItem.Quantity;
            //invoiceItem.SoldPrice = invoiceItem.RetailPrice - discountperItem;
            //var TotalAmount = Math.Round((invoiceItem.SoldPrice * invoiceItem.Quantity), 2);

            //Ticket #12405 Start : Sales Total Not Correct. By Nikhil
            // var TotalAmount = invoiceItem.SoldPrice * invoiceItem.Quantity;

            //Ticket start:#45690 Extra penny added to the subtotal and total once in a while.by rupesh
            //Ticket start:#74344 iOS and WEB :: Discount Issue.by rupesh
            var TotalAmount = Math.Round(invoiceItem.SoldPrice * invoiceItem.Quantity, Settings.StoreDecimalDigit,MidpointRounding.AwayFromZero);
            //Ticket start:#74344 iOS and WEB :: Discount Issue.by rupesh
            //Ticket end:#45690 .by rupesh

            //Following code commented to resolve rounding issue. 
            //var CrossMultilingual = DependencyService.Get<IMultilingual>();
            //Ticket #9414 Start : Invoice line item rounding  issue. By Nikhil.
            //var TotalAmount = Math.Round(invoiceItem.SoldPrice, CrossMultilingual.CurrentCultureInfo.NumberFormat.CurrencyDecimalDigits
            //    , MidpointRounding.AwayFromZero) * invoiceItem.Quantity;
            //Ticket #9414 End : By Nikhil.
            //Ticket #12405 End. By Nikhil


            //if (invoice.Status == InvoiceStatus.Refunded)
            //    invoiceItem.TotalAmount = -TotalAmount;
            //else
            invoiceItem.TotalAmount = TotalAmount;



            if (invoice.TaxInclusive)
                invoiceItem.TaxAmount = CalculateTaxInclusive(invoiceItem.TotalAmount, invoiceItem.TaxRate);
            else
                invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TotalAmount, invoiceItem.TaxRate);


            if (invoice.TaxInclusive)
            {
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TotalAmount - invoiceItem.TaxAmount;
                invoiceItem.EffectiveAmount = invoiceItem.TaxExclusiveTotalAmount + invoiceItem.TaxAmount;
            }
            else
            {
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TotalAmount;
                invoiceItem.EffectiveAmount = invoiceItem.TaxExclusiveTotalAmount;
            }

            foreach (var item in invoiceItem.LineItemTaxes)
            {
                //Subtax calculation changes
                if (invoice.TaxInclusive)
                {
                    //Start Inclusive Tax Pratik
                    item.TaxAmount = ((invoiceItem.TotalAmount - GetValuefromPercent(Math.Abs(invoiceItem.TotalAmount), invoiceItem.OfferDiscountPercent)) * item.TaxRate) / (100 + invoiceItem.TaxRate);

                    //item.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount - (GetValuefromPercent((Math.Abs(invoiceItem.SoldPrice) - Math.Abs(invoiceItem.TaxAmount)), invoiceItem.OfferDiscountPercent) * invoiceItem.Quantity), item.TaxRate);

                    //End Inclusive Tax Pratik
                }
                else
                {
                    item.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount - (GetValuefromPercent(invoiceItem.SoldPrice, invoiceItem.OfferDiscountPercent) * invoiceItem.Quantity), item.TaxRate);
                }
                //item.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, item.TaxRate);              
                // item.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount - (GetValuefromPercent(invoiceItem.SoldPrice, invoiceItem.OfferDiscountPercent) * invoiceItem.Quantity), item.TaxRate);
            }

            invoiceItem.RetailPrice = copyretailPrice;
            if (CallCalculateInvoiceTotal)
                CalculateInvoiceTotal(invoice, null, null);

            return invoiceItem;
        }

        public static decimal CalculateGiftCardTotal(InvoiceDto invoice)
        {

            decimal giftCardTotal = 0;

            foreach (var item in invoice.InvoiceLineItems)
            {
                if (item.InvoiceItemType == InvoiceItemType.GiftCard)
                {
                    giftCardTotal += item.SoldPrice;
                }
            }

            return giftCardTotal;
        }

        //Remove Code "try catch in this method" when app live

        public static InvoiceDto CalculateInvoiceTotal(InvoiceDto invoice, ObservableCollection<OfferDto> offers, ProductServices productServices)
        {
            try
            {

                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                //Ticket start: #65926 Discounts are not working properly .by Rupesh
                var excludingAmmountTotal = invoice.InvoiceLineItems.Sum(x => x.TaxExclusiveTotalAmount);

                //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                var ExcludeDiscountProductsAmtTotal = invoice.InvoiceLineItems.Where(a => a.DisableDiscountIndividually).Sum(x => x.TaxExclusiveTotalAmount);
                //End #92641 by Pratik

                //Start Ticket #73190 #67052 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                if (excludingAmmountTotal == 0)
                {
                    excludingAmmountTotal = invoice.InvoiceLineItems.Sum(x => (x.TotalAmount - x.TaxAmount));

                    //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                    ExcludeDiscountProductsAmtTotal = invoice.InvoiceLineItems.Where(a => a.DisableDiscountIndividually).Sum(x => (x.TotalAmount - x.TaxAmount));
                    //End #92641 by Pratik
                }
                //Start Ticket #73190
                //Ticket end: #65926. by Rupesh

                //Ticket start:#62496 iOS Tax total not matched with web.by rupesh
                var taxamountTotal = invoice.InvoiceLineItems.Sum(x => x.TaxAmount);

                //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                var ExcludeDiscountTax = invoice.InvoiceLineItems.Where(a => a.DisableDiscountIndividually).Sum(x => x.TaxAmount);
                //End #92641 by Pratik

                //Ticket end:#62496 .by rupesh
                //End Ticket #74344 By pratik

                //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                var ExcludeDiscountEffectiveamount = invoice.InvoiceLineItems.Where(a => a.DisableDiscountIndividually).Sum(x => x.EffectiveAmount);
                //End #92641 by Pratik

                var TotalEffectiveamount = invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);

                if (offers != null && invoice.Status != InvoiceStatus.Exchange) //#94424
                {
                    invoice = GetOrderValueOffer(invoice, offers, TotalEffectiveamount, productServices);
                }

                invoice.GiftCardTotal = invoice.InvoiceLineItems.Where(a => a.InvoiceItemType == InvoiceItemType.GiftCard).Sum(a => a.SoldPrice);  

                decimal discountPercentValue = 0;
                if (!invoice.DiscountIsAsPercentage)
                {
                    //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                    var total = (invoice.Status == InvoiceStatus.Refunded ? (TotalEffectiveamount - ExcludeDiscountEffectiveamount).ToPositive() : TotalEffectiveamount - ExcludeDiscountEffectiveamount);
                    //var total = (invoice.Status == InvoiceStatus.Refunded ? TotalEffectiveamount.ToPositive() : TotalEffectiveamount);
                    //End #92641 by Pratik
                    if (total != 0)
                        discountPercentValue = (invoice.DiscountValue * 100) / total;
                }
                else
                {
                    discountPercentValue = invoice.DiscountValue;
                }

                //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                decimal ExcludeDiscountAmt = 0;
                if (invoice.TaxInclusive)
                    ExcludeDiscountAmt = ExcludeDiscountProductsAmtTotal;
                else
                    ExcludeDiscountAmt = ExcludeDiscountEffectiveamount;
                //End #92641 by Pratik

                if (invoice.TaxInclusive)
                    invoice.SubTotal = excludingAmmountTotal;
                else
                    invoice.SubTotal = TotalEffectiveamount;

                //Ticket start:#43319 IOS: Exchange sale amount incorrect in iPad.by rupesh
                //Ticket start:#49654 Small difference in Sales (inc tax) amount on Sale summery page.by rupesh
                invoice._TotalTax = Math.Round(taxamountTotal, 4) + Math.Round(invoice.TipTaxAmount, 4) + invoice.ShippingTaxAmount;
                //Ticket end:#49654 .by rupesh
                //Ticket end:#43319.by rupesh
                invoice.Tax = taxamountTotal;

                //Ticket #1998 Start:Negative discount value. By rupesh.
                if (invoice.Status == InvoiceStatus.Exchange)
                {
                    //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                    //Ticket start: #66249 Net sales amount seems to be incorrect By Pratik
                    var ExcexcludingAmmountTotal = invoice.InvoiceLineItems.Where(a => a.IsExchangedProduct).Sum(x => x.TaxExclusiveTotalAmount);

                    //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                    var ExchnageExcludeProductsAmtTotal = invoice.InvoiceLineItems.Where(a => a.DisableDiscountIndividually && a.IsExchangedProduct).Sum(x => x.TaxExclusiveTotalAmount);
                    //End #92641 by Pratik

                    if (ExcexcludingAmmountTotal == 0)
                    {
                        ExcexcludingAmmountTotal = invoice.InvoiceLineItems.Where(a => a.IsExchangedProduct).Sum(x => (x.TotalAmount - x.TaxAmount));
                        //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                        ExchnageExcludeProductsAmtTotal = invoice.InvoiceLineItems.Where(a => a.DisableDiscountIndividually && a.IsExchangedProduct).Sum(x => (x.TotalAmount - x.TaxAmount));
                        //End #92641 by Pratik
                    }


                    var ExcTotalEffectiveamount = invoice.InvoiceLineItems.Where(a => a.IsExchangedProduct).Sum(x => x.EffectiveAmount);
                    //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                    var ExchnageExcludeEffectiveTotal = invoice.InvoiceLineItems.Where(a => a.DisableDiscountIndividually && a.IsExchangedProduct).Sum(x => x.EffectiveAmount);
                    //End #92641 by Pratik

                    var ExctaxamountTotal = invoice.InvoiceLineItems.Where(a => a.IsExchangedProduct).Sum(x => x.TaxAmount);
                    //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                    var ExcExcludetaxamountTotal = invoice.InvoiceLineItems.Where(a => a.DisableDiscountIndividually && a.IsExchangedProduct).Sum(x => x.TaxAmount);
                    //End #92641 by Pratik

                    //start #94424 FR Need to delete Gift cards from discount offer By Pratik
                    if (invoice.DiscoutType == "ordervalue" && invoice.GiftCardTotal > 0)
                    {
                        ExchnageExcludeProductsAmtTotal = ExchnageExcludeProductsAmtTotal + invoice.GiftCardTotal;
                    }
                    if (invoice.DiscoutType == "ordervalue" && invoice.GiftCardTotal > 0 && !invoice.TaxInclusive)
                    {
                        ExchnageExcludeEffectiveTotal = ExchnageExcludeEffectiveTotal + invoice.GiftCardTotal;
                    }
                    //end #94424 By Pratik

                    //Ticket end: #66249
                    if (!invoice.DiscountIsAsPercentage)
                    {
                        //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                        discountPercentValue = (invoice.DiscountValue * 100) / (ExcTotalEffectiveamount - ExchnageExcludeEffectiveTotal);
                        //discountPercentValue = (invoice.DiscountValue * 100) / ExcTotalEffectiveamount;
                        //End #92641 by Pratik
                    }                  

                    //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                    var subTotalDiscount = GetValuefromPercent((ExcexcludingAmmountTotal - ExchnageExcludeProductsAmtTotal), discountPercentValue);
                    if (!invoice.TaxInclusive)
                        subTotalDiscount = GetValuefromPercent((ExcTotalEffectiveamount - ExchnageExcludeEffectiveTotal), discountPercentValue);
                    // var subTotalDiscount = GetValuefromPercent(ExcexcludingAmmountTotal, discountPercentValue);
                    // if (!invoice.TaxInclusive)
                    //     subTotalDiscount = GetValuefromPercent(ExcTotalEffectiveamount, discountPercentValue);
                    //End #92641 by Pratik


                    decimal taxDiscount = 0;
                    if (ExctaxamountTotal != 0)
                    {
                        //Start #92641 iOS: correct "Exclude this product from any and all discount offers" same as web By Pratik
                        taxDiscount = GetValuefromPercent((ExctaxamountTotal - ExcExcludetaxamountTotal), discountPercentValue);
                        //taxDiscount = GetValuefromPercent(ExctaxamountTotal, discountPercentValue);
                        //End #92641 by Pratik
                    }
                    //End Ticket #74344 By pratik

                    if (invoice.TaxInclusive)
                    {
                        invoice.SubTotal += Math.Abs(subTotalDiscount);
                        if (invoice.ApplyTaxAfterDiscount)
                            invoice.Tax += Math.Abs(taxDiscount);

                        invoice.TotalDiscount = Math.Abs(subTotalDiscount + taxDiscount);
                    }
                    else
                    {
                        if (invoice.ApplyTaxAfterDiscount)
                            invoice.Tax -= taxDiscount;
                        //Ticket #9269 Start:Negative discount value. By Nikhil.
                        invoice.TotalDiscount = Math.Abs(subTotalDiscount);
                        //Ticket #9269 End:By Nikhil. 
                    }

                    invoice.TotalDiscount = -invoice.TotalDiscount;

                }
                //Ticket #1998 End:Negative discount value. By rupesh.
                else
                {
                    //start #94424 FR Need to delete Gift cards from discount offer By Pratik
                    if (invoice.DiscoutType == "ordervalue" && invoice.GiftCardTotal > 0)
                    {
                        ExcludeDiscountAmt = ExcludeDiscountAmt + invoice.GiftCardTotal;
                    }
                    //end #94424 By Pratik

                    //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                    var subTotalDiscount = GetValuefromPercent(invoice.SubTotal - ExcludeDiscountAmt, discountPercentValue);
                    //var subTotalDiscount = GetValuefromPercent(invoice.SubTotal, discountPercentValue);
                    //End #92641 by Pratik

                    decimal taxDiscount = 0;
                    if (invoice.Tax != 0)
                    {
                        //Start #92641 iOS: correct "Exclude this product from any and all discount offers"  same as web By Pratik
                        taxDiscount = GetValuefromPercent(invoice.Tax - ExcludeDiscountTax, discountPercentValue);
                        //taxDiscount = GetValuefromPercent(invoice.Tax, discountPercentValue);
                        //End #92641 by Pratik
                    }

                    if (invoice.TaxInclusive)
                    {

                        if (invoice.Status == InvoiceStatus.Refunded || invoice.Status == InvoiceStatus.Exchange)
                        {
                            invoice.SubTotal += Math.Abs(subTotalDiscount);
                            if (invoice.ApplyTaxAfterDiscount)
                                invoice.Tax += Math.Abs(taxDiscount);
                        }
                        else
                        {
                            invoice.SubTotal -= subTotalDiscount;
                            if (invoice.ApplyTaxAfterDiscount)
                                invoice.Tax -= taxDiscount;
                        }
                        //Ticket #9269 Start:Negative discount value. By Nikhil.
                        invoice.TotalDiscount = Math.Abs(subTotalDiscount + taxDiscount);
                        //Ticket #9269 End:By Nikhil.
                    }
                    else
                    {
                        if (invoice.ApplyTaxAfterDiscount)
                            invoice.Tax -= taxDiscount;
                        //Ticket #9269 Start:Negative discount value. By Nikhil.
                        invoice.TotalDiscount = Math.Abs(subTotalDiscount);
                        //Ticket #9269 End:By Nikhil. 
                    }

                    //Ticket #1998 Start:Negative discount value. By rupesh.
                    if (invoice.Status == InvoiceStatus.Refunded || invoice.Status == InvoiceStatus.Exchange)
                    {
                        invoice.TotalDiscount = -invoice.TotalDiscount;
                    }
                    //Ticket #1998 End:Negative discount value. By rupesh.
                }

                invoice.DiscountPercentValue = discountPercentValue;
                //invoice.Taxgroup = GetTaxgroup(invoice, discountPercentValue);

                var texLabel = "Tax";
                if (Settings.CurrentRegister != null && Settings.CurrentRegister.ReceiptTemplate != null)
                    texLabel = Settings.CurrentRegister.ReceiptTemplate.TaxLable;
                var taxes = new ObservableCollection<LineItemTaxDto>();

                if (invoice.Taxgroup != null && invoice.Taxgroup.Any())
                {
                    // Ticket Start #63111 iOS: Separate line items for Tax Group format need to match with web By: Pratik
                    foreach (var item in invoice.Taxgroup.OrderBy(a => a.IsGroupTax))
                    // Ticket end #63111 By: Pratik
                    {
                        if (item.SubTaxes != null && item.SubTaxes.Count > 0)
                        {
                            item.TaxName = texLabel + "(" + item.TaxName + ")";
                            taxes.Add(item);
                            foreach (var item2 in item.SubTaxes)
                            {
                                taxes.Add(item2);
                            }
                        }
                        else
                        {
                            item.TaxName = texLabel + "(" + item.TaxName + ")";
                            taxes.Add(item);
                        }
                    }
                }

                invoice.ReceiptTaxList = taxes;


                // tip
                //Ticket start:#33563 iPad: surcharge amount should not be getting during exchange time.by rupesh
                if (invoice.Status == InvoiceStatus.Refunded || invoice.Status == InvoiceStatus.Exchange)
                {
                    invoice.TotalTip = 0;
                    invoice.TipTaxAmount = 0;
                }
                //Ticket end:#33563 .by rupesh
                else
                {
                    //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
                    if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.ApplySurchargeOnTaxInclusiveTotal)
                    {

                        if (invoice.TipIsAsPercentage)
                        {

                            invoice.TotalTip = GetValuefromPercent((invoice.SubTotal + invoice.Tax), invoice.TipValue);


                        }
                        //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
                        else
                        {
                            invoice.TotalTip = invoice.TipValue;
                        }
                    }
                    else
                    {
                        if (invoice.TipIsAsPercentage)
                            invoice.TotalTip = GetValuefromPercent(invoice.SubTotal, invoice.TipValue);
                        else
                            invoice.TotalTip = invoice.TipValue;
                    }
                }

                if (invoice.TaxInclusive)
                    invoice.NetAmount = (invoice.SubTotal + invoice.Tax + invoice.TotalTip);
                else
                    invoice.NetAmount = ((invoice.SubTotal - invoice.TotalDiscount) + invoice.Tax + invoice.TotalTip);

                if (invoice.Status == InvoiceStatus.Refunded)
                {
                    var tapToPayments = invoice.InvoiceRefundPayments.Where(x => x.PaymentOptionType == PaymentOptionType.TyroTapToPay);
                    decimal surchargeValue = 0;
                    foreach (var payment in tapToPayments)
                    {
                        try
                        {
                            var transactionOutCome = JsonConvert.DeserializeObject<HikePOS.Models.Payment.TyroTapToPayPaymentResponse>(payment.InvoicePaymentDetails?.FirstOrDefault().Value);
                            double surcharge = 0;
                            double.TryParse(transactionOutCome.surcharge, out surcharge);
                            surchargeValue += (decimal)(surcharge / 100.0);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                     tapToPayments = invoice.InvoiceRefundPayments.Where(x => x.PaymentOptionType == PaymentOptionType.HikePayTapToPay || x.PaymentOptionType == PaymentOptionType.HikePay);
                     foreach (var payment in tapToPayments)
                    {
                        try
                        {
                            var transactionOutCome = JsonConvert.DeserializeObject<HikePOS.Models.Payment.NadaPayTransactionDto>(payment.InvoicePaymentDetails.FirstOrDefault(x => x.Key == InvoicePaymentKey.HikePaySaleResponseData).Value);
                            decimal surcharge = transactionOutCome?.SurchargeAmount ?? 0;
                            surchargeValue += surcharge;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    invoice.NetAmount = invoice.NetAmount + surchargeValue.ToNegative();
                }
                // else if (invoice.Status == InvoiceStatus.Exchange)
                // {

                //     var tapToPayments = invoice.InvoicePayments.Where(x => x.PaymentOptionType == PaymentOptionType.TyroTapToPay);
                //     decimal surchargeValuePayment = 0;
                //     foreach (var payment in tapToPayments)
                //     {
                //         try
                //         {
                //             var transactionOutCome = JsonConvert.DeserializeObject<HikePOS.Models.Payment.TyroTapToPayPaymentResponse>(payment.InvoicePaymentDetails?.FirstOrDefault().Value);
                //             double surcharge = 0;
                //             double.TryParse(transactionOutCome.surcharge, out surcharge);
                //             surchargeValuePayment += (decimal)(surcharge / 100.0);
                //         }
                //         catch (Exception ex)
                //         {

                //         }
                //     }
                //     invoice.NetAmount = invoice.NetAmount + surchargeValuePayment;
                    
                // }

                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                //#75269 start.Total Amount Incorrect for Selected WooCommerce Order.by rupesh
                //if(invoice.Status != InvoiceStatus.Refunded && invoice.Status != InvoiceStatus.Exchange)
                if (invoice.InvoicePayments != null && invoice.InvoicePayments.Count > 0)
                    invoice.NetAmount = invoice.NetAmount + invoice.TotalShippingCost + invoice.TotalPaymentSurcharge;
                else
                    invoice.NetAmount = invoice.NetAmount + invoice.TotalShippingCost;
                //#75269 end.by rupesh
                //END Ticket #73190 By: Pratik


                //Ticket start:#33476 Parked items showing pending paymetn.by rupesh
                //Ticket start:#34928 iPad: amount is not deducting on check out page for Japanese language.by rupesh
                if (Settings.StoreCulture == "ja-jp")
                    invoice.NetAmount = Math.Round(invoice.NetAmount, 0, MidpointRounding.AwayFromZero);
                else
                    //Ticket start:#41610 iPad: total value showing wrong in Kuwait(English).by rupesh
                    //Ticket start:#44350 Small difference in Net sale amount.by rupesh
                    //Start Ticket #75526 Myob rounding issue By Pratik
                    invoice.NetAmount = Math.Round(invoice.NetAmount, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                //End Ticket #75526 By Pratik
                //Ticket end:#44350 .by rupesh
                //Ticket end:#41610 .by rupesh
                //Ticket end:#33476 .by rupesh
                //Ticket start:#35310 iPad: rounding not calculating properly in refunded sale.by rupesh
                /* if (invoice.Status == InvoiceStatus.Refunded || invoice.Status == InvoiceStatus.Exchange)
                 {
                     //Ticket start:#33975 iPad: Rounding issue during refund sale for 0.10 and 1.00 round off.by rupesh
                     if (invoice.RoundingAmount > 0)
                         invoice.RoundingAmount = -(invoice.RoundingAmount);
                     //Ticket end:#33975 .by rupesh

                 }*/
                //Ticket end:#35310 .by rupesh

                invoice.TotalPay = invoice.NetAmount - invoice.TotalPaid;

                //Done to resolve 7290 as tender amount showing -0.01
                if (invoice.TotalPay < 0 && invoice.TotalPay > (decimal)(-0.01))
                {
                    invoice.TotalPay = 0;
                }

                //Ticket start:#41610 iPad: total value showing wrong in Kuwait(English).by rupesh
                invoice.TotalPay = Math.Round(invoice.TotalPay, Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero);
                //Ticket end:#41610 .by rupesh
                Debug.WriteLine("invoice : " + Newtonsoft.Json.JsonConvert.SerializeObject(invoice).ToString());

                invoice.TenderAmount = invoice.TotalPay;

                invoice = GetLoyalty(invoice, invoice.CustomerDetail);

                //if (Settings.StoreGeneralRule.RoundUptoFiveCent)
                //    invoice.TotalPay = Math.Ceiling(invoice.TotalPay * 20) / 20;
                //else
                //    invoice.TotalPay = Math.Round(invoice.TotalPay, 2, MidpointRounding.AwayFromZero);


                //Below condition for Ticket 20924
                //Ticket start:#24138,#26946 iOS - Wrong Logic When Removing All Products from The Cart.by 
                // #94565
                if (invoice.InvoiceFloorTable?.TableId != null && invoice.InvoiceFloorTable?.TableId > 0 &&
                    invoice.Status != InvoiceStatus.Refunded && invoice.Status != InvoiceStatus.OnGoing &&
                    invoice.Status != InvoiceStatus.Exchange && invoice.Status != InvoiceStatus.BackOrder &&
                    invoice.Status != InvoiceStatus.OnAccount && invoice.Status != InvoiceStatus.Parked &&
                    invoice.Status != InvoiceStatus.LayBy && invoice.Status != InvoiceStatus.Quote)
                {
                    Task.Run(async () =>
                    {
                        await saleService.UpdateLocalInvoice(invoice, LocalInvoiceStatus.Pending);
                    });
                }
                else if (invoice.Status != InvoiceStatus.Refunded && invoice.Status != InvoiceStatus.OnGoing &&
                   invoice.Status != InvoiceStatus.Exchange && invoice.Status != InvoiceStatus.BackOrder &&
                   invoice.Status != InvoiceStatus.OnAccount && invoice.Status != InvoiceStatus.Parked &&
                   invoice.Status != InvoiceStatus.LayBy && invoice.Status != InvoiceStatus.Quote)
                {
                    _debounceTimer?.Stop();
                    _debounceTimer = new System.Timers.Timer(2000); // 500 ms debounce
                    _debounceTimer.Elapsed += (s, e) =>
                    {
                        _debounceTimer.Stop();
                        Task.Run(async () =>
                        {
                            await saleService.UpdateLocalInvoice(invoice, LocalInvoiceStatus.Pending);
                        });
                    };
                    _debounceTimer.Start();
                }
                // #94565
                //Ticket end:#24138,#26946 .by rupesh

                return invoice;
            }
            catch (Exception ex)
            {
                //Logger.ExceptionLogger(ex);
                Debug.WriteLine(ex.Message);
            }

            return invoice;
        }
        //End Remove Code "try catch" when app live
        private static System.Timers.Timer _debounceTimer;
 
        public static async Task<ObservableCollection<LineItemTaxDto>> CalculateTax(InvoiceDto invoice)
        {
            await Task.Delay(1);
            return GetTaxgroup(invoice, invoice.DiscountPercentValue);
        }

        public static InvoiceDto GetLoyalty(InvoiceDto invoice, CustomerDto_POS customer)
        {
            try
            {
                invoice.LoyaltyPoints = 0;
                invoice.LoyaltyPointsValue = 0;

                var totalPaid = invoice.TotalPaid;

                foreach (var item in invoice.InvoicePayments)
                {
                    //Ticket start:#62551 IOS loyalty points not properly updated in print receipt.by rupesh
                    if (item.PaymentOptionType == PaymentOptionType.Loyalty)
                    //|| item.PaymentOptionType == PaymentOptionType.Credit
                    //|| item.PaymentOptionType == PaymentOptionType.GiftCard)
                    //Ticket end:#62551.by rupesh
                    {
                        totalPaid -= item.Amount;
                    }
                }

                var enableLoyalty = false;
                decimal earnAmountOnPurchgeByLoyalty = 0;
                int loyaltyPointsValue = 0;

                if (Settings.StoreGeneralRule != null)
                {
                    enableLoyalty = Settings.StoreGeneralRule.EnableLoyalty;
                    earnAmountOnPurchgeByLoyalty = Settings.StoreGeneralRule.EarnAmountOnPurchgeByLoyalty;
                    loyaltyPointsValue = Settings.StoreGeneralRule.LoyaltyPointsValue;
                }

                if (enableLoyalty && customer != null && customer.AllowLoyalty)
                {

                    var overrideamountOnLoyaltyPurchase = 0; //totalPaid > 0 ? invoice.InvoiceLineItems.Sum(x => x.LoyaltyPoints > 0 ? x.EffectiveAmount : 0) : 0;

                    var amountTocalculateLoyalty = totalPaid - overrideamountOnLoyaltyPurchase;

                    decimal balance = 0;

                    if (earnAmountOnPurchgeByLoyalty > 0)
                    {
                        //Ticket:start:#71296 iPad - Feature: Product Loyalty point exclusion.by rupesh
                        decimal itemAmountNotToCalculateLoyalty = 0;
                        foreach (var item in invoice.InvoiceLineItems)
                        {
                            if (item.DisableAdditionalLoyalty)
                            {
                                itemAmountNotToCalculateLoyalty += item.TotalAmount;
                            }
                        }
                        if (amountTocalculateLoyalty > 0)
                        {
                            amountTocalculateLoyalty -= itemAmountNotToCalculateLoyalty;
                        }
                        //Ticket:end:#71296 .by rupesh

                        balance = amountTocalculateLoyalty / earnAmountOnPurchgeByLoyalty;

                        decimal itemLoyalty = 0;
                        // Item loyalty points                         
                        foreach (var item in invoice.InvoiceLineItems)
                        {
                            if (item.AdditionalLoyalty > 0)
                            {
                                itemLoyalty += (item.AdditionalLoyalty * item.Quantity);
                            }
                        }

                        balance += itemLoyalty;


                        decimal CustomerGroupLoyaltyPoints = 0;
                        foreach (var item in invoice.InvoiceLineItems)
                        {
                            CustomerGroupLoyaltyPoints += item.CustomerGroupLoyaltyPoints;
                        }
                        invoice.LoyaltyPoints = Math.Round(balance, 2);
                        invoice.LoyaltyPointsValue = invoice.LoyaltyPoints / loyaltyPointsValue;

                        invoice.LoyaltyPointsValue = Math.Round(invoice.LoyaltyPointsValue, 2);

                        //Start #91233 ios : Create on ac sale at that time loyalty point not proper display. by Pratik
                        if (invoice.Status == InvoiceStatus.OnAccount && invoice.TotalPaid <= 0)
                        {
                            invoice.LoyaltyPoints = 0;
                            invoice.LoyaltyPointsValue = 0;
                        }
                        //End #91233 by Pratik

                        //Ticket #12736 Loyalty Points Calculated Wrongly on Invoices. By Nikhil
                        //  bool isParkedSaleCompleted = invoice.InvoiceHistories.Any(x => x.Status == InvoiceStatus.Parked);
                        //Ticket #13078 Wrong Opening Balance of Loyalty Points on Receipts.By Nikhil
                        //if (!isParkedSaleCompleted && invoice.CustomerCurrentLoyaltyPoints == 0)
                        //Ticket start:#33944 iOS - Loyalty Details Printed Wrongly for Refunds.by rupesh
                        //  if (!isParkedSaleCompleted)
                        //  {
                        //Ticket end:#33944 .by rupesh
                        //Ticket #13078 End. By Nikhil 
                        // invoice.CustomerCurrentLoyaltyPoints = customer.CurrentLoyaltyBalance;
                        //  }
                        //Ticket #12736 End. By Nikhil 
                    }
                    else
                    {
                        invoice.LoyaltyPoints = 0;
                        invoice.LoyaltyPointsValue = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return invoice;
        }

        //Start Ticket #73665 iOS: Group Taxes not matched in iOS for Prints from POS Screen and Same print from Sales history by pratik
        public static InvoiceDto LineItemTaxCalaculation(InvoiceDto invoice)
        {

            if (invoice != null && invoice.DiscountIsAsPercentage && invoice.DiscountValue > 0 && invoice.InvoiceLineItems != null)
            {
                foreach (var item in invoice.InvoiceLineItems.Where(a => a.LineItemTaxes != null && a.LineItemTaxes.Count > 0))
                {
                    if (invoice.Status != InvoiceStatus.Exchange ||
                        (invoice.Status == InvoiceStatus.Exchange && item.IsExchangedProduct))
                    {
                        var taxamt = (item.TaxAmount - InvoiceCalculations.GetValuefromPercent(item.TaxAmount, invoice.DiscountValue));
                        foreach (var lineitem in item.LineItemTaxes)
                        {
                            if(item.TaxRate > 0)
                                lineitem.TaxAmount = (taxamt * lineitem.TaxRate) / item.TaxRate;
                        }
                    }
                }
            }
            return invoice;
        }

        public static ObservableCollection<LineItemTaxDto> GetTaxgroupForprint(InvoiceDto invoice, decimal discountPercentValue)
        {
            invoice.Taxgroup = new ObservableCollection<LineItemTaxDto>();
            try
            {
                if (invoice.InvoiceLineItems != null)
                {
                    var TotalEffectiveamount = invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
                    foreach (var InvoiceLineItems in invoice.InvoiceLineItems)
                    {
                        var item = InvoiceLineItems.Copy();

                        if ((item.TaxId > 1 || (item.TaxId == 1 && item.TaxRate > 0)) && item.InvoiceItemType != InvoiceItemType.Discount)
                        {
                            var taxamount = item.TaxAmount;

                            //var taxamount = invoice.TotalTax;

                            Debug.WriteLine("taxamount : " + taxamount.ToString());

                            //Pratik 20Apr2013
                            var discountOfferTax = item.DiscountOfferTax.HasValue ? item.DiscountOfferTax.Value : 0;
                            if (invoice.Status == InvoiceStatus.Refunded)
                            {
                                taxamount += Math.Abs(discountOfferTax);
                            }
                            else if (invoice.Status == InvoiceStatus.Exchange)
                            {
                                if (item.Quantity < 0)
                                {
                                    taxamount += Math.Abs(discountOfferTax);
                                }
                                else
                                {
                                    taxamount += discountOfferTax;
                                }
                            }
                            else
                            {
                                taxamount += discountOfferTax;
                            }


                            if (invoice.ApplyTaxAfterDiscount && (invoice.Status != InvoiceStatus.Exchange ||
                            (invoice.Status == InvoiceStatus.Exchange && item.IsExchangedProduct)))
                            {
                                taxamount = (taxamount - InvoiceCalculations.GetValuefromPercent(taxamount, discountPercentValue));
                            }

                            // Changes by Jigar  ticket no 8335
                            var hasTax = invoice.Taxgroup.FirstOrDefault(x => x.TaxId == item.TaxId);
                            if (hasTax != null)
                            {
                                hasTax.TaxRate += item.TaxRate;
                                hasTax.TaxAmount += taxamount;
                                //Ticket start:#22390 Group tax break up is not displayed in the receipt when the discount offer product is added in the sale.by rupesh
                                if (item.LineItemTaxes != null && item.LineItemTaxes.Count > 0 && (hasTax.SubTaxes == null || hasTax.SubTaxes?.Count == 0))
                                {
                                    hasTax.SubTaxes = item.LineItemTaxes;
                                }
                                //Ticket end:#22390

                                foreach (var subvalue in item.LineItemTaxes)
                                {
                                    var hasSubTaxTax = hasTax.SubTaxes?.FirstOrDefault(x => x.TaxId == subvalue.TaxId);
                                    if (hasSubTaxTax != null)
                                    {
                                        if (invoice.DiscountIsAsPercentage)
                                        {
                                            hasSubTaxTax.TaxAmount = hasSubTaxTax.TaxAmount + subvalue.TaxAmount;
                                        }
                                        else
                                        {
                                            hasSubTaxTax.TaxAmount = subvalue.TaxAmount;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var tax = new LineItemTaxDto()
                                {
                                    TaxId = item.TaxId,
                                    TaxName = item.TaxName,
                                    TaxRate = item.TaxRate,
                                    TaxAmount = taxamount,
                                    SubTaxes = item.LineItemTaxes,
                                };
                                invoice.Taxgroup.Add(tax);
                            }

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
            return invoice.Taxgroup;

        }
        //End Ticket #73665 by pratik

        //Ticket start:#60810 iOS Tax issue.by rupesh
        public static ObservableCollection<LineItemTaxDto> GetTaxgroup(InvoiceDto invoice, decimal discountPercentValue)
        {
            invoice.Taxgroup = new ObservableCollection<LineItemTaxDto>();
            try
            {
                if (invoice.InvoiceLineItems != null)
                {
                    var TotalEffectiveamount = invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
                    foreach (var InvoiceLineItems in invoice.InvoiceLineItems)
                    {
                        var item = InvoiceLineItems.Copy();

                        if ((item.TaxId > 1 || (item.TaxId == 1 && item.TaxRate > 0)) && item.InvoiceItemType != InvoiceItemType.Discount)
                        {
                            var taxamount = item.TaxAmount;

                            //var taxamount = invoice.TotalTax;

                            Debug.WriteLine("taxamount : " + taxamount.ToString());

                            
                            var discountOfferTax = item.DiscountOfferTax.HasValue ? item.DiscountOfferTax.Value : 0;
                            if (invoice.Status == InvoiceStatus.Refunded)
                            {
                                taxamount += Math.Abs(discountOfferTax);
                            }
                            else if (invoice.Status == InvoiceStatus.Exchange)
                            {
                                if (item.Quantity < 0)
                                {
                                    taxamount += Math.Abs(discountOfferTax);
                                }
                                else
                                {
                                    taxamount += discountOfferTax;
                                }
                            }
                            else
                            {
                                taxamount += discountOfferTax;
                            }

                            // Changes by Jigar  ticket no 8335

                            //Start Ticket #73665 iOS: Group Taxes not matched in iOS for Prints from POS Screen and Same print from Sales history by pratik
                            if (invoice.Status != InvoiceStatus.Exchange || (invoice.Status == InvoiceStatus.Exchange && item.IsExchangedProduct)) //add Condition for #73665 
                            {
                                if (invoice.ApplyTaxAfterDiscount)
                                {
                                    taxamount = (taxamount - InvoiceCalculations.GetValuefromPercent(taxamount, discountPercentValue));
                                }

                                // Changes by Jigar  ticket no 8335
                                Debug.WriteLine("taxamount : " + taxamount.ToString());

                                if (item.LineItemTaxes != null)
                                {
                                    foreach (var subvalue in item.LineItemTaxes)
                                    {
                                        if (invoice.DiscountIsAsPercentage)
                                        {
                                            if (invoice.ApplyTaxAfterDiscount)
                                                subvalue.TaxAmount = (subvalue.TaxAmount - InvoiceCalculations.GetValuefromPercent(subvalue.TaxAmount, discountPercentValue));
                                        }
                                        else
                                        {
                                            if (invoice.ApplyTaxAfterDiscount)
                                            {
                                                decimal tempamount = invoice.SubTotal;
                                                if (!invoice.TaxInclusive)
                                                    tempamount = invoice.SubTotal - invoice.TotalDiscount;
                                                subvalue.TaxAmount = tempamount * subvalue.TaxRate / 100;
                                            }
                                            else
                                                subvalue.TaxAmount = subvalue.TaxAmount; // - discountPercentValue;
                                        }

                                        Debug.WriteLine("subvalue : " + subvalue.TaxAmount.ToString());
                                    }
                                }
                            }
                            //End Ticket #73665 by pratik

                            var hasTax = invoice.Taxgroup?.FirstOrDefault(x => x.TaxId == item.TaxId);
                            if (hasTax != null)
                            {
                                hasTax.TaxRate += item.TaxRate;
                                hasTax.TaxAmount += taxamount;
                                //Ticket start:#22390 Group tax break up is not displayed in the receipt when the discount offer product is added in the sale.by rupesh
                                if (item.LineItemTaxes != null && item.LineItemTaxes.Count > 0 && (hasTax.SubTaxes == null || hasTax.SubTaxes?.Count == 0))
                                {
                                    hasTax.SubTaxes = item.LineItemTaxes;
                                }
                                //Ticket end:#22390

                                if (item.LineItemTaxes != null)
                                {
                                    foreach (var subvalue in item.LineItemTaxes)
                                    {
                                        var hasSubTaxTax = hasTax.SubTaxes?.FirstOrDefault(x => x.TaxId == subvalue.TaxId);
                                        if (hasSubTaxTax != null)
                                        {
                                            if (invoice.DiscountIsAsPercentage)
                                            {
                                                hasSubTaxTax.TaxAmount = hasSubTaxTax.TaxAmount + subvalue.TaxAmount;
                                            }
                                            else
                                            {
                                                hasSubTaxTax.TaxAmount = subvalue.TaxAmount;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if(invoice.Taxgroup == null)
                                    invoice.Taxgroup = new ObservableCollection<LineItemTaxDto>();
                                var tax = new LineItemTaxDto()
                                {
                                    TaxId = item.TaxId,
                                    TaxName = item.TaxName,
                                    TaxRate = item.TaxRate,
                                    TaxAmount = taxamount,
                                    SubTaxes = item.LineItemTaxes,
                                };

                                invoice.Taxgroup.Add(tax);
                            }

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
            return invoice.Taxgroup;

        }
        //Ticket end:#60810.by rupesh
        public static InvoiceDto GetOrderValueOffer(InvoiceDto Invoice, ObservableCollection<OfferDto> offers, decimal orderValue, ProductServices productServices)
        {
            if (offers == null)
            {
                offers = new ObservableCollection<OfferDto>();
            }

            Orderrequest request = new Orderrequest
            {
                Price = orderValue,
                CustomerGroupId = Invoice.CustomerGroupId,
                OutletId = Invoice.OutletId,
                Quantity = 0
            };

            OfferDto productoffer = null;

            //Start Ticket #8819 :- Order Value Discount By Nikhil on 12/19/19.
            var sortedOffers = offers.OrderByDescending(x => x.OfferValue);
            foreach (var offer in sortedOffers)
            //End Ticket #8819 on 12/19/19.
            {
                //temp Comment
                if (offer.OfferType == OfferType.OnSale && Invoice.Status != InvoiceStatus.Refunded)
                    productoffer = CheckofferValidation(Invoice, offer, request, productServices);
                if (productoffer != null)
                {
                    break;
                }
            }

            if (productoffer != null)
            {
                Invoice.DiscountIsAsPercentage = productoffer.IsPercentage;
                Invoice.DiscountValue = Invoice.DiscountValue == 0 ? productoffer.OfferValue : Invoice.DiscountValue;
                Invoice.DiscoutType = "ordervalue";
                //Start Ticket #8819 :- Order Value Discount By Nikhil on 12/19/19.
                Invoice.Note = "Total sale discount offer applied: " + productoffer.Name;
                //End Ticket #8819 on 12/19/19.
            }
            else if (Invoice.DiscoutType != null && Invoice.DiscoutType == "ordervalue")
            {
                Invoice.DiscountValue = 0;
            }

            return Invoice;
        }

        public static OfferDto CheckofferValidation(InvoiceDto invoice, OfferDto offer, Orderrequest request, ProductServices productService)
        {

            var poffer = new OfferDto()
            {
                Id = offer.Id,
                Priority = offer.Priority,
                Description = offer.Description,
                OfferImage = offer.OfferImage,
                OfferType = offer.OfferType,
                BuyX = offer.BuyX,
                GetX = offer.GetX,
                IsPercentage = offer.IsPercentage,
                OfferValue = offer.OfferValue,
                OfferAmount = offer.OfferAmount,
                Name = offer.Name,
                OfferItems = offer.OfferItems,
                IsActive = offer.IsActive,
                IsOfferOnAllCustomer = offer.IsOfferOnAllCustomer,
                OfferCustomerGroups = offer.OfferCustomerGroups,
                OfferId = offer.Id,
                Include = offer.Include //Start #91264 By Pratik
            };
            ProductDto_POS product = productService.GetLocalProductSync(request.ProductId);

            if (invoice != null && invoice.InvoiceLineItems != null && product != null && product.ParentId != null && offer.OfferItems != null)
            {
                //Start #91264 iOS: FR Discount Offers: Create and EXCLUDED Brands offer By Pratik
                if(offer.Include)
                {
                    if (offer.OfferItems.Where(x => x.OfferOn == OfferOn.Product && x.OfferOnId == product.ParentId).Any())
                    {
                        foreach (var item in invoice.InvoiceLineItems)
                        {
                            if (offer.BuyX <= invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == product.ParentId).Count())
                            {
                                item.TotalQuantity = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == product.ParentId).Count();
                            }
                        }
                    }
                    else if (offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Brand) != null)
                    {
                        var mainproduct = Products.FirstOrDefault(x => x.Id == product.ParentId);
                        if (mainproduct != null && offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Brand && x.OfferOnId == mainproduct.BrandId) != null)
                        {
                            foreach (var item in invoice.InvoiceLineItems)
                            {
                                if (offer.BuyX <= invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id))
                                {
                                    item.TotalQuantity = invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id);
                                }
                            }
                        }
                    }
                    else if (offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Season) != null)
                    {
                        var mainproduct = Products.FirstOrDefault(x => x.Id == product.ParentId);
                        if (mainproduct != null && offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Season && x.OfferOnId == mainproduct.SeasonId) != null)
                        {

                            foreach (var item in invoice.InvoiceLineItems)
                            {
                                if (offer.BuyX <= invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id))
                                {
                                    item.TotalQuantity = invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id);
                                }
                            }
                        }
                    }
                    else if (offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Tags) != null)
                    {
                        
                        var mainproduct = Products.FirstOrDefault(x => x.Id == product.ParentId);
                        if (mainproduct != null && invoice?.CustomerDetail?.APICustomerTags != null && offer.OfferItems.Any(x => x.OfferOn == OfferOn.Tags && invoice.CustomerDetail.APICustomerTags.Contains(x.OfferOnId)))
                        {
                            foreach (var item in invoice.InvoiceLineItems)
                            {
                                if (offer.BuyX <= invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id))
                                {
                                    item.TotalQuantity = invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id);
                                }
                            }
                        }
                    }
                    //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                    else if (offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.ProductTag) != null)
                    {
                        var mainproduct = Products.FirstOrDefault(x => x.Id == product.ParentId);
                        if (mainproduct != null && offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.ProductTag && x.OfferOnId == mainproduct.TagId) != null)
                        {

                            foreach (var item in invoice.InvoiceLineItems)
                            {
                                if (offer.BuyX <= invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id))
                                {
                                    item.TotalQuantity = invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id);
                                }
                            }
                        }
                    }
                    //End #84438 by Pratik
                }
                else
                {
                    if (offer.OfferItems.Where(x => x.OfferOn == OfferOn.Product && x.OfferOnId != product.ParentId).Any())
                    {
                        foreach (var item in invoice.InvoiceLineItems)
                        {
                            if (offer.BuyX <= invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == product.ParentId).Count())
                            {
                                item.TotalQuantity = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == product.ParentId).Count();
                            }
                        }
                    }
                    else if (offer.OfferItems.FirstOrDefault(x => x.OfferOn != OfferOn.Brand) != null)
                    {
                        var mainproduct = Products.FirstOrDefault(x => x.Id == product.ParentId);
                        if (mainproduct != null && offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Brand && x.OfferOnId != mainproduct.BrandId) != null)
                        {
                            foreach (var item in invoice.InvoiceLineItems)
                            {
                                if (offer.BuyX <= invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id))
                                {
                                    item.TotalQuantity = invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id);
                                }
                            }
                        }
                    }
                    else if (offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Season) != null)
                    {
                        var mainproduct = Products.FirstOrDefault(x => x.Id == product.ParentId);
                        if (mainproduct != null && offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Season && x.OfferOnId == mainproduct.SeasonId) != null)
                        {

                            foreach (var item in invoice.InvoiceLineItems)
                            {
                                if (offer.BuyX <= invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id))
                                {
                                    item.TotalQuantity = invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id);
                                }
                            }
                        }
                    }
                    else if (offer.OfferItems.FirstOrDefault(x => x.OfferOn != OfferOn.Tags) != null)
                    {
                        var mainproduct = Products.FirstOrDefault(x => x.Id == product.ParentId);
                        if (mainproduct != null && invoice?.CustomerDetail?.APICustomerTags != null && offer.OfferItems.Any(x => x.OfferOn == OfferOn.Tags && invoice.CustomerDetail.APICustomerTags.Contains(x.OfferOnId)))
                        {

                            foreach (var item in invoice.InvoiceLineItems)
                            {
                                if (offer.BuyX <= invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id))
                                {
                                    item.TotalQuantity = invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id);
                                }
                            }
                        }
                    }
                    //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                    else if (offer.OfferItems.FirstOrDefault(x => x.OfferOn != OfferOn.ProductTag) != null)
                    {
                        var mainproduct = Products.FirstOrDefault(x => x.Id == product.ParentId);
                        if (mainproduct != null && offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.ProductTag && x.OfferOnId != mainproduct.TagId) != null)
                        {

                            foreach (var item in invoice.InvoiceLineItems)
                            {
                                if (offer.BuyX <= invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id))
                                {
                                    item.TotalQuantity = invoice.InvoiceLineItems.Count(x => x.InvoiceItemValueParent == mainproduct.Id);
                                }
                            }
                        }
                    }
                    //End #84438 by Pratik
                }
                //End #91264 By Pratik
            }

            var isvalid = true;
            if (offer.ValidFrom != null && offer.ValidTo != null)
            {
                isvalid = offer.ValidFrom.Value.ToLocalTime().Date <= DateTime.Now.Date && offer.ValidTo.Value.ToLocalTime().Date >= DateTime.Now.Date;
            }

            //#94419
            if (isvalid && !string.IsNullOrEmpty(offer.FromTime) && !string.IsNullOrEmpty(offer.ToTime))
            {
                TimeSpan fromTime = TimeSpan.Parse(offer.FromTime);
                TimeSpan toTime = TimeSpan.Parse(offer.ToTime);
                TimeSpan now = DateTime.Now.TimeOfDay;
                isvalid = now >= fromTime && now <= toTime;
                if (!isvalid && invoice.InvoiceLineItems != null && invoice.Status != InvoiceStatus.Exchange && invoice.Status != InvoiceStatus.Refunded)
                {
                    for (int i = 0; i < invoice.InvoiceLineItems.Count; i++)
                    {
                        var item = invoice.InvoiceLineItems[i];
                        if (item.OfferId.HasValue && item.OfferId > 0 && offer.Id == item.OfferId && item.InvoiceItemType != InvoiceItemType.Discount)
                        {
                            for (int j = invoice.InvoiceLineItems.Count - 1; j >= 0; j--)
                            {
                                var discountItem = invoice.InvoiceLineItems[j];
                                if (discountItem.InvoiceItemValue == item.OfferId && discountItem.InvoiceItemType == InvoiceItemType.Discount)
                                {
                                    invoice.InvoiceLineItems.RemoveAt(j);
                                }
                            }
                            item.OfferId = null;
                            item.OfferDiscountPercent = null;
                            item.OffersNote = string.Empty;
                            item.DiscountOfferTax = 0;
                        }
                    }
                }
             
            }
            //#94419

            if (!offer.IsOfferOnAllCustomer && offer.OfferCustomerGroups.Any())
            {
                var hascustomergroup = offer.OfferCustomerGroups.FirstOrDefault(x => x.CustomerGroupId == request.CustomerGroupId);
                if (hascustomergroup == null)
                    isvalid = false;
            }

            if (!offer.IsOfferOnAllOutlet)
            {
                var hasoutlet = offer.OfferOutlets?.FirstOrDefault(x => x.OutletId == request.OutletId);
                if (hasoutlet == null)
                    isvalid = false;
            }


            if (offer.OfferType == OfferType.buyxgety)
            {
                if (!request.OnlycheckAvailability && !(request.Quantity >= offer.BuyX + offer.GetX))
                    isvalid = false;
            }

            if (offer.OfferType == OfferType.OnSale)
            {
                if (!(request.Price >= offer.OfferAmount))
                    isvalid = false;
            }

            if (offer.OfferType == OfferType.buyxgetPercent)
            {
                if (!request.OnlycheckAvailability && !(request.Quantity >= offer.BuyX))
                    isvalid = false;
            }

            if (isvalid)
            {
                return poffer;
            }
            else
            {
                return null;
            }
        }

        public async static Task<InvoiceDto> AddItemToSell(InvoiceDto Invoice, ObservableCollection<OfferDto> offers, ProductDto_POS product, decimal quantity, decimal? backOrderQty, ProductServices productService, TaxServices taxServices, int? extraItemValueParent, int sequence)
        {

            //Exchange Issue ticket 7987, 7657
            //Ticket start:#22898 Composite sale not working properly.by rupesh.//addedInvoiceItemType == InvoiceItemType.CompositeProduct
           //Start #85370 Product showing two entries in a sale By Pratik
           //var hasInvoiceItem = Invoice.InvoiceLineItems.FirstOrDefault(x => (((x.InvoiceItemType == InvoiceItemType.Standard || x.InvoiceItemType == InvoiceItemType.CompositeProduct) && !product.IsUnitOfMeasure) || (x.InvoiceItemType == InvoiceItemType.UnityOfMeasure && product.IsUnitOfMeasure)) && x.InvoiceItemValue == product.Id && x.InvoiceExtraItemValueParent == extraItemValueParent && x.Quantity >= 0 && (!x.EnableSerialNumber));
            InvoiceLineItemDto hasInvoiceItem = null;
            if(Invoice.Status == InvoiceStatus.Parked)
                hasInvoiceItem = Invoice.InvoiceLineItems.FirstOrDefault(x => (((x.InvoiceItemType == InvoiceItemType.Standard || x.InvoiceItemType == InvoiceItemType.CompositeProduct) && !product.IsUnitOfMeasure) || (x.InvoiceItemType == InvoiceItemType.UnityOfMeasure && product.IsUnitOfMeasure)) && x.InvoiceItemValue == product.Id && x.InvoiceExtraItemValueParent == extraItemValueParent && x.Quantity >= 0 && (!x.EnableSerialNumber));
            else
                hasInvoiceItem = Invoice.InvoiceLineItems.FirstOrDefault(x => (((x.InvoiceItemType == InvoiceItemType.Standard || x.InvoiceItemType == InvoiceItemType.CompositeProduct) && !product.IsUnitOfMeasure) || (x.InvoiceItemType == InvoiceItemType.UnityOfMeasure && product.IsUnitOfMeasure)) && x.RegisterClosureId == Settings.CurrentRegister.Registerclosure.Id && x.InvoiceItemValue == product.Id && x.InvoiceExtraItemValueParent == extraItemValueParent && x.Quantity >= 0 && (!x.EnableSerialNumber));
            //End #85370 By Pratik

            // Ticket end:#22898.by rupesh
            //Ticket #13213 : Display individually as separate line items not working.By Nikhil 
            if (hasInvoiceItem != null && Invoice.Status != InvoiceStatus.Exchange && product.ProductExtras.Count == 0 && !Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct)
            {
                //Ticket #13213 End.By Nikhil 
                hasInvoiceItem.BackOrderQty = backOrderQty;
                if (backOrderQty > 0)
                {
                    hasInvoiceItem.Quantity = quantity;
                    hasInvoiceItem.Description = backOrderQty + " Quantity in back order";
                }
                else
                    hasInvoiceItem.Quantity += quantity;

                if (hasInvoiceItem.Quantity > 0)
                {
                    //Ticket start:#74632 iOS: Add Flat Markup% to Customer Group Pricing (FR).by rupesh
                    hasInvoiceItem = CustomerMarkupDiscount(Invoice, hasInvoiceItem, Invoice.CustomerDetail);
                    //Ticket end:#74632.by rupesh
                    hasInvoiceItem = CustomerPricebookDiscount(Invoice, hasInvoiceItem, Invoice.CustomerDetail);
                    Invoice = await AddofferTolineItem(Invoice, offers, product, hasInvoiceItem, productService, false);
                }
                hasInvoiceItem = CalculateLineItemTotal(hasInvoiceItem, Invoice,false);
                Invoice = CalculateInvoiceTotal(Invoice, offers, productService);

                //TODO : Will add localdatabase call


                //await saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.Pending);

                return Invoice;
            }
            else if (hasInvoiceItem != null && Invoice.Status == InvoiceStatus.Exchange && hasInvoiceItem.Quantity > 0)
            {
                var exchangeinvoice = Invoice.Copy();

                //Ticket start:#22898 Composite sale not working properly.by rupesh.//addedInvoiceItemType == InvoiceItemType.CompositeProduct
                var existingExchangeInvoiceItem = exchangeinvoice.InvoiceLineItems.FirstOrDefault(x => ((x.InvoiceItemType == InvoiceItemType.Standard || x.InvoiceItemType == InvoiceItemType.CompositeProduct) && !product.IsUnitOfMeasure) || (x.InvoiceItemType == InvoiceItemType.UnityOfMeasure && product.IsUnitOfMeasure) && x.InvoiceItemValue == product.Id && x.InvoiceExtraItemValueParent == extraItemValueParent);
                //Ticket end:#22898.by rupesh

                //Ticket #13213 : Display individually as separate line items not working.By Nikhil 
                if (existingExchangeInvoiceItem != null && !Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct)
                {
                    //Ticket #13213 End. By Nikhil
                    hasInvoiceItem.BackOrderQty = backOrderQty;
                    if (backOrderQty > 0)
                    {
                        hasInvoiceItem.Quantity = quantity;
                    }
                    else
                        hasInvoiceItem.Quantity += quantity;

                    if (hasInvoiceItem.Quantity > 0)
                    {
                        //Ticket start:#74632 iOS: Add Flat Markup% to Customer Group Pricing (FR).by rupesh
                        hasInvoiceItem = CustomerMarkupDiscount(Invoice, hasInvoiceItem, Invoice.CustomerDetail);
                        //Ticket end:#74632.by rupesh
                        hasInvoiceItem = CustomerPricebookDiscount(Invoice, hasInvoiceItem, Invoice.CustomerDetail);
                        Invoice = await AddofferTolineItem(Invoice, offers, product, hasInvoiceItem, productService, false);
                    }
                    hasInvoiceItem = CalculateLineItemTotal(hasInvoiceItem, Invoice);
                    Invoice = CalculateInvoiceTotal(Invoice, offers, productService);

                    //await saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.Pending);
                    return Invoice;
                }
                else if (existingExchangeInvoiceItem == null && hasInvoiceItem != null)
                {
                    hasInvoiceItem.BackOrderQty = backOrderQty;
                    if (backOrderQty > 0)
                    {
                        hasInvoiceItem.Quantity = quantity;
                    }
                    else
                        hasInvoiceItem.Quantity += quantity;

                    if (hasInvoiceItem.Quantity > 0)
                    {
                        //Ticket start:#74632 iOS: Add Flat Markup% to Customer Group Pricing (FR).by rupesh
                        hasInvoiceItem = CustomerMarkupDiscount(Invoice, hasInvoiceItem, Invoice.CustomerDetail);
                        //Ticket end:#74632.by rupesh
                        hasInvoiceItem = CustomerPricebookDiscount(Invoice, hasInvoiceItem, Invoice.CustomerDetail);
                        Invoice = await AddofferTolineItem(Invoice, offers, product, hasInvoiceItem, productService, false);
                    }
                    hasInvoiceItem = CalculateLineItemTotal(hasInvoiceItem, Invoice);
                    Invoice = CalculateInvoiceTotal(Invoice, offers, productService);


                    //await saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.Pending);
                    return Invoice;
                }

            }
            InvoiceLineItemDto invoiceItem = new InvoiceLineItemDto();

            invoiceItem.CustomerGroupDiscountPercent = 0;
            invoiceItem.InvoiceItemType = InvoiceItemType.Standard;
            invoiceItem.InvoiceItemValue = product.Id;
            
            invoiceItem.InvoiceItemImage = product.ItemImage.ToString();
            invoiceItem.InvoiceItemValueParent = product.ParentId;
            //invoiceItem.invoiceItemTitleParent = product.parentName;
            invoiceItem.InvoiceExtraItemValueParent = extraItemValueParent;
            invoiceItem.HasExtraproduct = product.ProductExtras.Count() > 0 ? true : false;
            //Ticket #9624 Start: Extra product line item not showing on web issue. By Nikhil.
            invoiceItem.Sequence = sequence;// Invoice.InvoiceLineItems.Count() + 1; 
                                            //Ticket #9624 End:By Nikhil. 
            invoiceItem.Title = product.Name;
            invoiceItem.Sku = product.Sku;
            //Ticket #9624 Start: Extra product line item not showing on web issue. By Nikhil.
            invoiceItem.Barcode = product.BarCode;
            //Ticket #9624 Start: Extra product line item not showing on web issue. By Nikhil.
            invoiceItem.EnableSerialNumber = product.EnableSerialNumber;
            invoiceItem.SerialNumber = string.Empty;

            //Ticket start:#21684 Quantities sold for some categories not showing.by rupesh
            invoiceItem.Category = JsonConvert.SerializeObject(product.ProductCategories, Newtonsoft.Json.Formatting.Indented,
                                                   new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            //Ticket start:#45382 iPad: FR - Non-Discountable items.by rupesh
            if (!product.DisableDiscountIndividually)
            {
                //maui
                List<CategoryDto> catagoriesdata = new List<CategoryDto>();
                if (product.ProductCategories != null && product.ProductCategories.Count > 0)
                {
                    catagoriesdata = productService.GetLocalCategoriesByIds(product.ProductCategories.ToList());
                }
                if (catagoriesdata != null)
                {
                    invoiceItem.CategoryDtos = JsonConvert.SerializeObject(catagoriesdata, Newtonsoft.Json.Formatting.Indented,
                                            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                }
                // End maui

                // invoiceItem.CategoryDtos = JsonConvert.SerializeObject(product.ProductCategoryDtos, Newtonsoft.Json.Formatting.Indented,
                //                                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }
            //Ticket start:#68994,#73763 Discounts not working on iPad.by rupesh
            invoiceItem.DisableDiscountIndividually = product.DisableDiscountIndividually;
            //Ticket end:#68994,#73763.by rupesh

            //Ticket end:#45382 .by rupesh
            //Ticket end:#21684 .by rupesh
            invoiceItem.Brand = product.BrandId?.ToString();
            invoiceItem.Tags = JsonConvert.SerializeObject(product.ProductTags, Newtonsoft.Json.Formatting.Indented,
                                                   new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            invoiceItem.Season = product.SeasonId?.ToString();

            invoiceItem.TotalQuantity = 0;
            invoiceItem.Quantity = quantity;
            invoiceItem.BackOrderQty = backOrderQty;
            //Ticket:start:#71296 iPad - Feature: Product Loyalty point exclusion.by rupesh
            invoiceItem.DisableAdditionalLoyalty = product.DisableAdditionalLoyalty;
            if (!product.DisableAdditionalLoyalty)
            {
                invoiceItem.AdditionalLoyalty = product.Loyalty;
            }
            //Ticket:end:#71296 .by rupesh
            invoiceItem.CustomerGroupLoyaltyPoints = 0;

            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                if (currentRegister.Registerclosure != null)
                {
                    invoiceItem.RegisterClosureId = currentRegister.Registerclosure.Id;
                }
            }
            if (product.ItemImage != null)
                invoiceItem.InvoiceItemImage = product.ItemImageUrl != null ? product.ItemImageUrl.ToString() : string.Empty;
            //Start #90941 iOS:FR: Custom filed on product detailpage By Pratik
            invoiceItem.CustomField = product.CustomField;
            if (product.ParentId != null && product.ParentId > 0)
            {
                var proddto = productService.GetLocalProductDB(product.ParentId.Value);
                if(proddto != null && !string.IsNullOrEmpty(proddto.CustomField))
                {
                    invoiceItem.CustomField = proddto.CustomField;
                }
                if (product.ItemImage == null && proddto != null)
                {
                    invoiceItem.InvoiceItemImage = proddto.ItemImageUrl != null ? proddto.ItemImageUrl.ToString() : string.Empty;
                }
            }

            if (backOrderQty > 0)
                invoiceItem.Description = backOrderQty + " Quantity in back order";
            else
                invoiceItem.Description = "";
            invoiceItem.ItemCost = product.ProductOutlet.CostPrice;
            invoiceItem.AvgCostPrice = product.ProductOutlet.AvgCostPrice;

            if (Invoice.TaxInclusive)
                invoiceItem.RetailPrice = product.ProductOutlet.SellingPrice;
            else
                invoiceItem.RetailPrice = product.ProductOutlet.PriceExcludingTax;

            invoiceItem.DiscountIsAsPercentage = true;
            invoiceItem.DiscountValue = 0;

            invoiceItem.SalesCode = product.SalesCode;
            invoiceItem.ActionType = ActionType.Sell;


            //Ticket #9624 Start: Extra product line item not showing on web issue.
            //Following code comented.By Nikhil.
            //if (product.ProductExtras != null && product.ProductExtras.Count > 0)
            //{
            //    foreach (var item in product.ProductExtras)
            //    {
            //        var extraitem = Invoice.InvoiceLineItems.Where(a => a.InvoiceItemValue == item.ExtraItemId).FirstOrDefault();
            //        if (extraitem != null)
            //            extraitem.InvoiceExtraItemValueParent = product.Id;
            //    }
            //}
            //Ticket #9624 End:By Nikhil.

            //Ticket start:#22898 Composite sale not working properly.by rupesh
            if (product.ProductType == ProductType.Composite && product.ProductCompositeItems != null)
            {
                invoiceItem.InvoiceItemType = InvoiceItemType.CompositeProduct;
                invoiceItem.InvoiceLineSubItems = new ObservableCollection<InvoiceLineSubItemDto>();
                foreach (var temp in product.ProductCompositeItems)
                {

                    var subProduct =  productService.GetLocalProductDB(temp.CompositeProductId);

                    var subitem = new InvoiceLineSubItemDto
                    {
                        ItemId = subProduct.Id,
                        ItemName = subProduct.Name,
                        Quantity = temp.Qty ?? 0,
                        //Ticket start:#84291 iOS:FR: composite change. by rupesh
                        Discount = temp.Discount ?? 0,
                        IndividualPrice = temp.IndividualPrice ?? 0,
                        Barcode = subProduct.BarCode,
                        Sku = subProduct.Sku,
                        //Ticket end:#84291 . by rupesh

                    };
                    invoiceItem.InvoiceLineSubItems.Add(subitem);
                }


            }
            //Ticket end:#22898 .by rupesh
            
            //Ticket start:#20064 Unit of measurement feature for iPad app..by rupesh
            if (product.IsUnitOfMeasure)
            {
                invoiceItem.InvoiceItemType = InvoiceItemType.UnityOfMeasure;
                invoiceItem.ItemCost = product.ProductOutlet.CostPrice * product.ProductUnitOfMeasureDto.Qty;
                invoiceItem.AvgCostPrice = product.ProductOutlet.AvgCostPrice * product.ProductUnitOfMeasureDto.Qty;
                decimal soldPrice = 0;
                if (Invoice.TaxInclusive)
                {
                    soldPrice = product.ProductOutlet.SellingPrice * product.ProductUnitOfMeasureDto.Qty;
                }
                else
                {
                    soldPrice = product.ProductOutlet.PriceExcludingTax * product.ProductUnitOfMeasureDto.Qty;
                }
                soldPrice = Math.Round(soldPrice,Settings.StoreDecimalDigit, MidpointRounding.AwayFromZero); //#96092
                var discountedValue = product.ProductUnitOfMeasureDto.DiscountValue;
                if (product.ProductUnitOfMeasureDto.DiscountIsAsPercentage)
                    discountedValue = GetValuefromPercent(soldPrice, product.ProductUnitOfMeasureDto.DiscountValue);
                invoiceItem.RetailPrice = soldPrice - discountedValue;

                var measureProduct = productService.GetLocalProduct(product.ProductUnitOfMeasureDto.MeasureProductId);
                invoiceItem.InvoiceLineSubItems = new ObservableCollection<InvoiceLineSubItemDto>();
                var subitem = new InvoiceLineSubItemDto
                {
                    ItemId = measureProduct.Id,
                    ItemName = measureProduct.Name,
                    Quantity = product.ProductUnitOfMeasureDto.Qty,
                    Stock = measureProduct.Stock,
                    Barcode = measureProduct.BarCode,
                    Sku = measureProduct.Sku,
                    TrackInventory = measureProduct.TrackInventory
                };
                invoiceItem.InvoiceLineSubItems.Add(subitem);

                //Ticket start:#26599 iPad: Discount offer should not be applied to UOM product.by rupesh
                invoiceItem.CategoryDtos = null;
                //Ticket end:#26599 .by rupesh

            }
            //Ticket end:#20064 .by rupesh

            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
           // MainThread.BeginInvokeOnMainThread(()=>{
                if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    Invoice.InvoiceLineItems.Add(invoiceItem);
                else
                    Invoice.InvoiceLineItems.Insert(0, invoiceItem);
            //});
         
            //End:#45375 .by rupesh

            //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil 
            var customer = Invoice.CustomerDetail;
            if (customer != null && customer.Id > 0 && customer.ToAllowForTaxExempt)
            {
                invoiceItem.RetailPrice = product.ProductOutlet.PriceExcludingTax;
                var noTax = Extensions.GetNoTaxRecord(taxServices);
                AddNoTaxToInvoiceLineItem(invoiceItem, noTax);
            }
            else
            {
                SetLineItemTaxes(invoiceItem, product, taxServices);
            }
            //Ticket #10921 End. By Nikhil 

            if (invoiceItem.Quantity > 0)
            {
                //Ticket start:#74632 iOS: Add Flat Markup% to Customer Group Pricing (FR).by rupesh
                invoiceItem = CustomerMarkupDiscount(Invoice, invoiceItem, Invoice.CustomerDetail);
                //Ticket end:#74632.by rupesh
                invoiceItem = CustomerPricebookDiscount(Invoice, invoiceItem, Invoice.CustomerDetail);
                if(offers != null && offers.Count > 0)
                    Invoice = await AddofferTolineItem(Invoice, offers, product, invoiceItem, productService, false);
            }

            invoiceItem = CalculateLineItemTotal(invoiceItem, Invoice, false);
            Invoice = CalculateInvoiceTotal(Invoice, offers, productService);
            //await saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.Pending);
            return Invoice;
        }

        //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil	 
        static InvoiceLineItemDto SetLineItemTaxes(InvoiceLineItemDto invoiceItem, ProductDto_POS product, TaxServices taxServices)
        {
            try
            {
                //Ticket start:#60810 iOS Tax issue.by pratik
                if (invoiceItem.TaxId <= 0)
                {
                    invoiceItem.TaxId = product.ProductOutlet.TaxID ?? 1;
                    invoiceItem.TaxName = string.IsNullOrEmpty(product.ProductOutlet.TaxName) ? "No Tax" : product.ProductOutlet.TaxName;
                    invoiceItem.TaxRate = product.ProductOutlet.TaxRate ?? 0;
                }
                //Ticket end:#60810.by pratik

                // add line item taxes
                invoiceItem.LineItemTaxes = new ObservableCollection<LineItemTaxDto>();

                if (product.ProductOutlet.TaxID != null)
                {
                    // Ticket Start #63111 iOS: Separate line items for Tax Group format need to match with web By: Pratik
                    var SubTaxList = taxServices.GetLocalTaxById(invoiceItem.TaxId)?.SubTaxes;
                    // Ticket End #63111 By: Pratik
                    if (SubTaxList != null && SubTaxList.Count > 0)
                    {
                        foreach (var item in SubTaxList)
                        {
                            var subtax = new LineItemTaxDto
                            {
                                TaxId = item.TaxId,
                                TaxRate = item.Rate,
                                TaxName = item.Name
                            };

                            invoiceItem.LineItemTaxes.Add(subtax);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in SetLineItemTaxes: " + e.Message + "\n" + e.StackTrace);
            }
            return invoiceItem;
        }

        static void AddNoTaxToInvoiceLineItem(InvoiceLineItemDto invoiceLineItem, TaxDto noTax)
        {
            try
            {
                invoiceLineItem.TaxId = noTax.Id;
                invoiceLineItem.TaxName = noTax.Name;
                invoiceLineItem.TaxRate = noTax.Rate;
                invoiceLineItem.TaxAmount = 0;

                var lineItemNoTax = new LineItemTaxDto
                {
                    TaxId = noTax.Id,
                    TaxRate = noTax.Rate,
                    TaxName = noTax.Name
                };

                invoiceLineItem.LineItemTaxes.Clear();
                invoiceLineItem.LineItemTaxes.Add(lineItemNoTax);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in AddNoTaxToInvoiceLineItem: " + e.Message + "\n" + e.StackTrace);
            }
        }
        //Ticket #10921 End. By Nikhil

        public static async Task<InvoiceDto> CheckstockAndaddToCart(InvoiceDto Invoice, ObservableCollection<OfferDto> offers, ProductDto_POS product, decimal qunatity, ProductServices productService, TaxServices taxServices, int? invoiceExtraItemValueParent, int sequence)
        {
            try
            {
                //WEB CODE
                var hasInvoiceItems = Invoice.InvoiceLineItems.Where(x => (((x.InvoiceItemType == InvoiceItemType.Standard || x.InvoiceItemType == InvoiceItemType.CompositeProduct) && !product.IsUnitOfMeasure) || (x.InvoiceItemType == InvoiceItemType.UnityOfMeasure && product.IsUnitOfMeasure)) && x.InvoiceItemValue == product.Id && x.InvoiceExtraItemValueParent == invoiceExtraItemValueParent && x.ActionType == ActionType.Sell);
                //Start Ticket #65352 iOS: 1 QTY product add multiple time in line items By Pratik
                //if (hasInvoiceItem != null && !Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct)
                if (hasInvoiceItems != null && hasInvoiceItems.Count() > 0)
                //End Ticket #65352 Pratik
                {
                    var hasInvoiceItem = hasInvoiceItems.FirstOrDefault();
                    //Ticket #13213 End.By Nikhil 
                    var checkvalidateQty = hasInvoiceItems.Sum(a => a.Quantity) + qunatity;
                    //Start Ticket #65352 iOS: 1 QTY product add multiple time in line items By Pratik
                    //var result = await CheckstockValidation(product, checkvalidateQty, hasInvoiceItem.BackOrderQty, Invoice);
                    decimal QtnforSameProductMultipleLine = 0;
                    if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct)
                    {
                        QtnforSameProductMultipleLine = qunatity;
                    }
                    //Start #85662 iOS :Back order Not working while 1 line item setting is true By Pratik
                    //var result = await CheckstockValidation(product, checkvalidateQty, hasInvoiceItem.BackOrderQty, Invoice, true, null, QtnforSameProductMultipleLine);
                    var result = await CheckstockValidation(product, checkvalidateQty, Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct ? 0 : hasInvoiceItem.BackOrderQty, Invoice, true, null, QtnforSameProductMultipleLine);
                    //End #85662 By Pratik
                    //End Ticket #65352 Pratik
                    if (result.IsValid)
                    {
                        if (result.Validatedstock <= 0 && result.BackOrderQty > 0)
                            qunatity = 0;
                        else if (result.BackOrderQty > 0)
                            qunatity = result.Quantity;
                        //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time. by rupesh
                        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                        else if ((Invoice.IsEditBackOrderFromSaleHistory && !Settings.IsBackorderSaleSelected) && ((result.Validatedstock > 0 && product.TrackInventory) || product.ProductType == ProductType.Composite))
                        {
                            await App.Alert.ShowAlert("", "Selected item is already in stock. It cannot be added to a backorder", "Ok");
                            return Invoice;
                        }
                        //Ticket end:#92764.by rupesh
                        //Ticket end:#84289 . by rupesh

                        Invoice = await AddItemToSell(Invoice, offers, product, qunatity, result.BackOrderQty, productService, taxServices, invoiceExtraItemValueParent, sequence);
                    }

                }
                else
                {
                    //Ticket start:#101177 Incorrect Fraction Quantity Display for Weight-Based Products on iPad .by rupesh
                    if (qunatity == 1 && product.ProductOutlet.OnHandstock > 0 && product.ProductOutlet.OnHandstock < 1 && product.TrackInventory)
                    {
                        //Ticket end:#101177.by rupesh
                        qunatity = product.ProductOutlet.OnHandstock;
                    }
                    var result = await CheckstockValidation(product, qunatity, 0, Invoice);
                    if (result.IsValid)
                    {
                        if (result.Validatedstock <= 0 && result.BackOrderQty > 0)
                            qunatity = 0;
                        else if (result.BackOrderQty > 0)
                            qunatity = result.Quantity;
                        //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time. by rupesh
                        //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                        else if ((Invoice.IsEditBackOrderFromSaleHistory && !Settings.IsBackorderSaleSelected) && ((result.Validatedstock > 0 && product.TrackInventory) || product.ProductType == ProductType.Composite))
                        {
                            await App.Alert.ShowAlert("", "Selected item is already in stock. It cannot be added to a backorder", "Ok");
                            return Invoice;
                        }
                        //Ticket end:#92764.by rupesh
                        //Ticket end:#84289 . by rupesh

                        Invoice = await AddItemToSell(Invoice, offers, product, qunatity, result.BackOrderQty, productService, taxServices, invoiceExtraItemValueParent, sequence);
                    }
                }
            }
            catch (Exception exc)
            {
                exc.Track();
            }
            return Invoice;
        }

        // Ticket#11964 (previous #10039) Issues About BO added bool isBackorderConfirmation = true  by rupesh
        //Ticket start:#63960 iPad: Product sold on iPad even after 0 stock.by rupesh

        //Start Ticket #65352 iOS: 1 QTY product add multiple time in line items By Pratik
        //public static async Task<BackorderResult> CheckstockValidation(ProductDto_POS product, decimal quantity, decimal? backOrderQty, InvoiceDto Invoice, bool isBackorderConfirmation = true,InvoiceLineItemDto invoiceLineItem = null)
        public static async Task<BackorderResult> CheckstockValidation(ProductDto_POS product, decimal quantity, decimal? backOrderQty, InvoiceDto Invoice, bool isBackorderConfirmation = true, InvoiceLineItemDto invoiceLineItem = null, decimal QtnforSameProductMultipleLine = 0)
        //END Ticket #65352 By Pratik
        {
            //Ticket end:#63960.by rupesh
            //if(Settings.GrantedPermissionNames)

            BackorderResult result = new BackorderResult
            {
                IsValid = true,
                BackOrderQty = 0,
                Validatedstock = 0,
                Quantity = quantity
            };

            try
            {
                //Start #85662 iOS :Back order Not working while 1 line item setting is true By Pratik
                //Start Ticket #65352 iOS: 1 QTY product add multiple time in line items By Pratik
                //if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct && !product.EnableSerialNumber)
                //    backOrderQty = 0;
                //End Ticket #65352 By Pratik
                //End #85662 By Pratik
                var committedStock = product.ProductOutlet.Committedstock;
                if (backOrderQty == null)
                    backOrderQty = 0;
                else
                    quantity += backOrderQty.Value;


                var validatedstock = product.ProductOutlet.OnHandstock - committedStock;
                result.Validatedstock = validatedstock;
                
                if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.AllowSellingOutofStock)
                {
                    return result;
                }

                //Ticket start:#22898 Composite sale not working properly.by rupesh
                 //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                if (!Settings.IsQuoteSale && !Settings.IsBackorderSaleSelected && product.ProductType == ProductType.Composite && (!product.AllowOutOfStock || Settings.StoreGeneralRule.AllowSellingOutofStock))
                {
                     //Ticket end:#92764.by rupesh 
                    if (product.ProductCompositeItems == null)
                    {
                        return result;

                    }
                    //Ticket start:#63960 iPad: Product sold on iPad even after 0 stock.by rupes
                    if (Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct)
                    {
                        if (invoiceLineItem != null)
                            quantity = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == product.Id && x.Sequence != invoiceLineItem.Sequence).Sum(x => x.Quantity) + quantity;
                        //else
                        //    quantity = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == product.Id).Sum(x => x.Quantity) + quantity;
                    }
                    //Ticket end:#63960 .by rupesh
                    var productNameList = "";
                    foreach (var item in product.ProductCompositeItems)
                    {
                        //Ticket start:#63960 iPad: Product sold on iPad even after 0 stock.by rupesh
                        if (productService == null)
                            productService = new ProductServices(productApiService);
                        var subProduct = productService.GetLocalProduct(item.CompositeProductId);
                        //var subProduct = Products.Where(x=>x.Id == item.CompositeProductId).FirstOrDefault();
                        //Ticket end:#63960 .by rupesh
                        var checkvalidateQty = quantity * item.Qty;
                        //Ticket start #65351:iOS : Composite product Issue. by rupesh
                        checkvalidateQty = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == subProduct.Id).Sum(x => x.Quantity) + checkvalidateQty;
                        //Ticket end #65351. by rupesh
                        //Ticket start:#26911 iOS - Wrong Validation for Composite Product Stock.by rupesh
                        if (subProduct.ProductOutlet.Stock < checkvalidateQty && subProduct.TrackInventory && !subProduct.AllowOutOfStock)
                        {
                            if (string.IsNullOrEmpty(productNameList))
                                productNameList = subProduct.Name;
                            else
                                productNameList = productNameList + "," + subProduct.Name;

                        }

                    }
                    if (string.IsNullOrEmpty(productNameList))
                        result.IsValid = true;
                    else
                    {
                        await App.Alert.ShowAlert("Out of stock", "One or more items used to create this composite product are out of stock. Please restock following items: " + productNameList + " to continue selling this composite product.", "Ok");
                        result.IsValid = false;
                        //Ticket end:#26911 .by rupesh

                    }
                    return result;
                }
                //Ticket end:#22898 Composite sale not working properly.by rupesh

                //Ticket start:#22406 Quote sale.by rupesh
                //Ticket start:#92764 iOS:FR Need to manage display product stock.by rupesh
                if (!Settings.IsQuoteSale && !Settings.IsBackorderSaleSelected && product.TrackInventory && (!product.AllowOutOfStock || Settings.StoreGeneralRule.AllowSellingOutofStock))
                {
                    //Ticket end:#92764.by rupesh
                    //Ticket end:#22406 Quote sale.by rupesh

                    //Start Ticket #65352 iOS: 1 QTY product add multiple time in line items By Pratik
                    decimal otherprodqty = 0;
                    if (Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct)
                    {
                        if (invoiceLineItem != null)
                        {
                            otherprodqty = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == product.Id && x.Sequence != invoiceLineItem.Sequence).Sum(x => x.Quantity);
                            quantity = otherprodqty + quantity;
                        }
                    }
                    //End Ticket #65352 By Pratik

                    //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                    var productId = product.IsUnitOfMeasure ? product.ProductUnitOfMeasureDto.MeasureProductId : product.Id;
                    decimal totalQuantity = 0;
                    var hasCompositeInvoiceItems = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.CompositeProduct && x.InvoiceLineSubItems.Any(x => x.ItemId == productId) && x.ActionType == ActionType.Sell);
                    foreach (var item in hasCompositeInvoiceItems)
                    {
                        totalQuantity = item.Quantity * item.InvoiceLineSubItems.Where(x => x.ItemId == productId).Sum(x => x.Quantity) + totalQuantity;
                    }
                    quantity = quantity + totalQuantity;

                    //Ticket start:#84289 IOS-Feature:-Ability to re-open Backorders and apply partial payments at any time. by rupesh
                    if (validatedstock < quantity && !Invoice.IsEditBackOrderFromSaleHistory)
                    {
                        //start:#94565.by rupesh
                        if (Settings.IsRestaurantPOS)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("OutofStockMessage"), Colors.Red, Colors.White);
                            result.IsValid = false;
                            result.BackOrderQty = 0;
                            return result;

                        }
                        //end:#94565.

                        //Ticket end:#84289.by rupesh
                        decimal backOrderQuantity = 0;

                        if (validatedstock <= 0)
                            backOrderQuantity = quantity;
                        else
                            backOrderQuantity = quantity - validatedstock;

                        //var stockresult = await Application.Current.MainPage.DisplayAlert("", "Do you have to create backorder for this product?", "Yes", "Cancel");
                        //result.isValid = stockresult;
                        //result.backOrderQty = backOrderQuantity;

                        bool isPermissionBackOrder = false;
                        if (Settings.GrantedPermissionNames != null)
                        {
                            isPermissionBackOrder = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.BackOrder"));
                        }

                        if (!isPermissionBackOrder)
                        {
                            //await Application.Current.MainPage.DisplayAlert("Error", "Out of Stock ", "Ok");
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("YouDoNotHaveEnoughPermissionToBackorder"), Colors.Red, Colors.White);
                            result.IsValid = false;
                            result.BackOrderQty = 0;
                        }
                        else if (Invoice.Status == InvoiceStatus.Exchange || Invoice.Status == InvoiceStatus.Refunded)
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("YouCanNotCreateBackOrderInExchangeOrRefund"), Colors.Red, Colors.White);
                            result.IsValid = false;
                            result.BackOrderQty = 0;
                        }
                        else
                        {
                            // Ticket #10039 Issues About BO On iPad by rupesh
                            if (backOrderQuantity > 0)
                            {
                                // Ticket #11964 Issues About BO On iPad by rupesh
                                var stockresult = true;
                                if (isBackorderConfirmation)
                                {
                                    stockresult = await App.Alert.ShowAlert("Out of stock", "Create a backorder for out of stock quantity?", "Yes", "Cancel");

                                }

                                result.IsValid = stockresult;
                                result.BackOrderQty = backOrderQuantity;

                                if (result.IsValid)
                                {
                                    // Ticket #10039 Issues About BO On iPad by rupesh
                                    //Start Ticket #65352 iOS: 1 QTY product add multiple time in line items By Pratik
                                    if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct && QtnforSameProductMultipleLine > 0)
                                    {
                                        quantity = QtnforSameProductMultipleLine;
                                    }
                                    result.Quantity = (quantity - otherprodqty) - backOrderQuantity;
                                    //END Ticket #65352 By Pratik
                                    string number = Convert.ToString(result.Quantity);
                                    if (number.Contains("."))
                                    {
                                        while (number.Substring(number.Length - 1) == "0")
                                        {
                                            number = number.Substring(0, number.Length - 1);
                                        }
                                    }
                                    result.Quantity = Convert.ToDecimal(number);
                                }
                            }
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                    return result;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return result;
        }

        public static async Task<BackorderResult> CheckCompositestockValidation(ProductDto_POS product, decimal quantity, decimal? backOrderQty)
        {
            quantity += backOrderQty == null ? 0 : backOrderQty.Value;
            BackorderResult result = new BackorderResult
            {
                IsValid = true,
                BackOrderQty = 0,
                Validatedstock = 0
            };


            var validatedstock = product.ProductOutlet.Stock;
            result.Validatedstock = validatedstock;

            if (product.TrackInventory && !product.AllowOutOfStock)
                if (validatedstock < quantity)
                {
                    decimal backOrderQuantity = 0;

                    if (validatedstock < 0)
                        backOrderQuantity = quantity;
                    else
                        backOrderQuantity = quantity - validatedstock;

                    result.IsValid = false;
                    result.BackOrderQty = backOrderQuantity;
                }

            return result;
        }



        #region OfferLogic

        public static async Task<InvoiceDto> AddofferTolineItem(InvoiceDto Invoice, ObservableCollection<OfferDto> offers, ProductDto_POS product, InvoiceLineItemDto invoiceLineItem, ProductServices productServices, bool calltotal = true)
        {
            try
            {
                invoiceLineItem.IsOfferAdded = false;

                var request = new Orderrequest
                {
                    OnlycheckAvailability = true,
                    ProductId = product.Id,
                    Quantity = invoiceLineItem.Quantity,
                    Price = invoiceLineItem.RetailPrice,
                    CustomerGroupId = Invoice.CustomerGroupId,
                    OutletId = Invoice.OutletId
                };

                //Ticket start:#30959 iPad - New feature request :: Rule for discount offers when there are more than one offers applicable.by rupesh
                OfferDto productoffer = null;
                if (Settings.StoreGeneralRule.ConflictOffer)
                {
                    var invoice = Invoice.Copy();
                    var productoffers = GetoffersByProduct(invoice, offers, product, request, invoiceLineItem, productServices);
                    productoffer = productoffers.FirstOrDefault();
                    if (Settings.StoreGeneralRule.ConflictOfferHighest == ConflictOfferType.HighestDiscount)
                    {
                        productoffer = productoffers.LastOrDefault();
                        //Start #95236 Discount drop down menu By PR
                        var offerids = productoffers.Select(a => a.OfferId);
                        for (int i = 0; i < Invoice.InvoiceLineItems.Count; i++)
                        {
                            var item = Invoice.InvoiceLineItems[i];
                            if (item.OfferId.HasValue && item.OfferId > 0 && offerids.Contains(item.OfferId.Value) && item.InvoiceItemType != InvoiceItemType.Discount)
                            {
                                for (int j = Invoice.InvoiceLineItems.Count - 1; j >= 0; j--)
                                {
                                    var discountItem = Invoice.InvoiceLineItems[j];
                                    if (discountItem.InvoiceItemValue == item.OfferId && discountItem.InvoiceItemType == InvoiceItemType.Discount)
                                    {
                                        Invoice.InvoiceLineItems.RemoveAt(j);
                                    }
                                }

                                item.OfferId = null;
                                item.OfferDiscountPercent = null;
                                item.OffersNote = string.Empty;
                                item.DiscountOfferTax = 0;
                            }
                        }
                        //End #95236 By PR
                    }
                    else if (Settings.StoreGeneralRule.ConflictOfferHighest == ConflictOfferType.LowestDiscount)
                    {
                        productoffer = productoffers.Where(x => x.TotalDiscount > 0).FirstOrDefault();
                        if (productoffer == null)
                            productoffer = productoffers.FirstOrDefault();
                        //Start #95236 Discount drop down menu By PR
                        var offerids = productoffers.Select(a => a.OfferId);
                        for (int i = 0; i < Invoice.InvoiceLineItems.Count; i++)
                        {
                            var item = Invoice.InvoiceLineItems[i];
                            if (item.OfferId.HasValue && item.OfferId > 0 && offerids.Contains(item.OfferId.Value) && item.InvoiceItemType != InvoiceItemType.Discount)
                            {
                                for (int j = Invoice.InvoiceLineItems.Count - 1; j >= 0; j--)
                                {
                                    var discountItem = Invoice.InvoiceLineItems[j];
                                    if (discountItem.InvoiceItemValue == item.OfferId && discountItem.InvoiceItemType == InvoiceItemType.Discount)
                                    {
                                        Invoice.InvoiceLineItems.RemoveAt(j);
                                    }
                                }

                                item.OfferId = null;
                                item.OfferDiscountPercent = null;
                                item.OffersNote = string.Empty;
                                item.DiscountOfferTax = 0;
                            }
                        }
                        //End #95236 By PR
                    }
                    else
                    {
                        var offersApplied = productoffers.Where(x => x.TotalDiscount > 0).ToList();
                        var selectedOffer = OffersToSelectManually?.FirstOrDefault(x => x.IsSelected);

                        //Start #91264 By Pratik
                        bool openpopup = false;
                        if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct && invoice.InvoiceLineItems != null)
                        {

                            openpopup = invoice.InvoiceLineItems.Where(a=> a.OfferId.HasValue && a.InvoiceItemValue == invoiceLineItem.InvoiceItemValue).Count() >= 1;
                            //Start #95236 Discount drop down menu By PR
                            //    if(openpopup)
                            //    {
                            //         var id =  invoice.InvoiceLineItems.FirstOrDefault(a=> a.OfferId.HasValue && a.InvoiceItemValue == invoiceLineItem.InvoiceItemValue).OfferId;
                            //         selectedOffer = productoffers.FirstOrDefault(a=>a.Id == id);
                            //    }
                            //End #95236 By PR
                        }
                        //End #91264 By Pratik

                        if (OffersToSelectManually?.Count != offersApplied.Count || (PreviousInvoiceLineItem?.InvoiceItemValue != invoiceLineItem.InvoiceItemValue && (!openpopup || selectedOffer == null))) //Start #91264 By Pratik
                        {
                            var OffersToSelectManuallycopy = OffersToSelectManually;
                            OffersToSelectManually = new ObservableCollection<OfferDto>(offersApplied);
                            var offerToSelect = OffersToSelectManually.FirstOrDefault(x => x.Id == (selectedOffer != null ? selectedOffer.Id : offersApplied.FirstOrDefault().Id));
                            if (offerToSelect != null)
                                offerToSelect.IsSelected = true;
                            else if (OffersToSelectManually.Count > 0)
                                OffersToSelectManually.First().IsSelected = true;
                            if (OffersToSelectManually.Count > 1)
                            {
                                selectedOffer = await OpenOfferSelectPage(OffersToSelectManually);
                                //Start #95236 Discount drop down menu By PR
                                var offerids = OffersToSelectManually.Select(a => a.OfferId);
                                for (int i = 0; i < Invoice.InvoiceLineItems.Count; i++)
                                {
                                    var item = Invoice.InvoiceLineItems[i];
                                    if (item.OfferId.HasValue && item.OfferId > 0 && offerids.Contains(item.OfferId.Value) && item.InvoiceItemType != InvoiceItemType.Discount)
                                    {
                                        for (int j = Invoice.InvoiceLineItems.Count - 1; j >= 0; j--)
                                        {
                                            var discountItem = Invoice.InvoiceLineItems[j];
                                            if (discountItem.InvoiceItemValue == item.OfferId && discountItem.InvoiceItemType == InvoiceItemType.Discount)
                                            {
                                                Invoice.InvoiceLineItems.RemoveAt(j); // Safely remove while iterating backward
                                            }
                                        }
                                        item.OfferId = null;
                                        item.OfferDiscountPercent = null;
                                        item.OffersNote = string.Empty;
                                        item.DiscountOfferTax = 0;
                                    }
                                }
                                //End #95236 By PR
                            }
                            else
                            {
                                selectedOffer = OffersToSelectManually.FirstOrDefault();
                                //Start #95236 Discount drop down menu By PR
                                if (invoiceLineItem != null && invoiceLineItem.OfferId.HasValue && invoiceLineItem.OfferId > 0)
                                {
                                    var offerids = OffersToSelectManuallycopy.Select(a => a.OfferId);
                                    for (int i = 0; i < Invoice.InvoiceLineItems.Count; i++)
                                    {
                                        var item = Invoice.InvoiceLineItems[i];
                                        if (item.OfferId.HasValue && item.OfferId > 0 && offerids.Contains(item.OfferId.Value) && item.InvoiceItemType != InvoiceItemType.Discount)
                                        {
                                            for (int j = Invoice.InvoiceLineItems.Count - 1; j >= 0; j--)
                                            {
                                                var discountItem = Invoice.InvoiceLineItems[j];
                                                if (discountItem.InvoiceItemValue == item.OfferId && discountItem.InvoiceItemType == InvoiceItemType.Discount)
                                                {
                                                    Invoice.InvoiceLineItems.RemoveAt(j);
                                                }
                                            }

                                            item.OfferId = null;
                                            item.OfferDiscountPercent = null;
                                            item.OffersNote = string.Empty;
                                            item.DiscountOfferTax = 0;
                                        }
                                    }
                                }
                                //End #95236 By PR
                            }
                        }
                        if (selectedOffer != null)
                        {
                            productoffer = selectedOffer;
                        }
                    }

                    var hasDiscountedItem = Invoice.InvoiceLineItems.Where(x => productoffers.Where(y => (y.TotalDiscount > 0 || y.TotalDiscount == 0) && y.OfferId == x.InvoiceItemValue).Any() &&  productoffer.OfferId == x.InvoiceItemValue && x.InvoiceItemType == InvoiceItemType.Discount && x.TotalAmount < 0).FirstOrDefault();
                    //Ticket start:#56383 iPad: discount issue.by rupesh
                    if (hasDiscountedItem != null && invoiceLineItem.CategoryDtos != null)
                    //Ticket end:#56383 .by rupesh
                    {
                        var preProductOffer = productoffers.Where(x => x.OfferId == hasDiscountedItem.InvoiceItemValue).FirstOrDefault();
                        var portionLineItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == hasDiscountedItem.InvoiceItemValue);
                        hasDiscountedItem.TotalAmount = portionLineItem?.TotalAmount ?? 0 + preProductOffer.TotalDiscount.Value;
                        if (hasDiscountedItem.TotalAmount <= 0)
                            Invoice.InvoiceLineItems.Remove(hasDiscountedItem);
                    }

                    PreviousInvoiceLineItem = invoiceLineItem;
                    if (productoffer != null)
                        Invoice.InvoiceLineItems.ForEach(c => { if (c.OfferId == productoffer.Id) c.IsOfferAdded = false; });
                    //Ticket end:#30959 .by rupesh
                }
                else
                {
                    productoffer = GetofferByProduct(Invoice, offers, product, request, productServices);
                }
                if (productoffer != null)
                {
                    if (productoffer.OfferType == OfferType.Simple)
                    {
                        //invoiceLineItem.DiscountIsAsPercentage = productoffer.IsPercentage;
                        //invoiceLineItem.DiscountValue = productoffer.OfferValue;
                        invoiceLineItem = ApplyPercentOff(Invoice, productoffer, invoiceLineItem, Invoice.CustomerGroupId, productServices);
                    }
                    else if (productoffer.OfferType == OfferType.Bin)
                    {
                        invoiceLineItem = ApplyBinOffer(Invoice, productoffer, invoiceLineItem, Invoice.CustomerGroupId, productServices);
                    }
                    else if (productoffer.OfferType == OfferType.buyxgety)
                    {
                        if (productoffer.BuyX == null)
                        {
                            productoffer.BuyX = 0;
                        }

                        if (productoffer.GetX == null)
                        {
                            productoffer.GetX = 0;
                        }
                        //#78749 Pratik add false 27May
                        Invoice = ApplyBuyXGetXOffer(Invoice, offers, productoffer, invoiceLineItem, productServices, Invoice.CustomerGroupId,false);
                    }
                    else if (productoffer.OfferType == OfferType.buyxgetPercent)
                    {
                        //#78749 Pratik add false 27May
                        Invoice = ApplyBuyXGetPercentOffer(Invoice, offers, productoffer, invoiceLineItem, Invoice.CustomerGroupId, productServices,false);
                    }
                    else if (productoffer.OfferType == OfferType.buyxgetPercentoffonbuyY)
                    {
                        //#78749 Pratik add false 27May
                        Invoice = ApplyBuyXGetPercentOffOnBuyY(Invoice, offers, productoffer, invoiceLineItem, productServices, Invoice.CustomerGroupId,false);
                    }
                    else if (productoffer.OfferType == OfferType.buyxgetValueOff)
                    {
                        //#78749 Pratik add false 27May
                        Invoice = ApplyBuyXGetValueOffer(Invoice, offers, productoffer, invoiceLineItem, Invoice.CustomerGroupId,false);
                    }
                    else if (productoffer.OfferType == OfferType.buyxgetYfree)
                    {
                        Invoice = ApplyBuyXGetYFreeOffer(Invoice, offers, productoffer, invoiceLineItem, productServices, Invoice.CustomerGroupId);
                    }
                    //Ticket start:#14435 Discount not applying in iPad Hike App by rupesh
                    else if (productoffer.OfferType == OfferType.buyxormoregetValueOff)
                    {
                        //#78749 Pratik add false 27May
                        Invoice = ApplyBuyXOrMoreGetValueOffer(Invoice, offers, productoffer, invoiceLineItem, Invoice.CustomerGroupId,false);
                    }
                    //Ticket end:#14435 Discount not applying in iPad Hike App by rupesh


                    invoiceLineItem.OfferId = productoffer.OfferId;
                    //Ticket start:#68994 Discounts not working on iPad.by rupesh
                    invoiceLineItem.OffersNote = invoiceLineItem.CategoryDtos != null ? productoffer.Description : "";
                    //Ticket end:#68994. by rupesh
                }
                else
                {
                    var hasInvoiceItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == product.Id && x.InvoiceItemType == InvoiceItemType.Discount && x.TotalAmount < 0);

                    invoiceLineItem.OfferId = null;
                    invoiceLineItem.OffersNote = "";
                    //Start #77843 INCORRECT INVOICE TOTAL By Pratik
                    invoiceLineItem.OfferDiscountPercent = 0;
                    //End #77843 By Pratik


                    while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                    {
                        //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                        Invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
                    }

                }

                if (calltotal)
                {
                        //#78749 Pratik add false 27May
                    invoiceLineItem = CalculateLineItemTotal(invoiceLineItem, Invoice,false);
                    Invoice = CalculateInvoiceTotal(Invoice, offers, productServices);
                }

                //Start #91264 By Pratik
                if(productoffer!=null)
                    invoiceLineItem.IsOfferAdded = invoiceLineItem.OfferId == productoffer.Id ? false : Settings.StoreGeneralRule.ConflictOffer;
                else
                    invoiceLineItem.IsOfferAdded = Settings.StoreGeneralRule.ConflictOffer;
                //End #91264 By Pratik
            }
            catch (Exception ex)
            {
                ex.Track();
            }

            return Invoice;
        }

        public static async Task<InvoiceDto> UpdatelineItem(InvoiceDto Invoice, ObservableCollection<OfferDto> offers, ProductDto_POS product, InvoiceLineItemDto invoiceItem, ProductServices productServices, int action)
        {
            //if (app.utils.isNullOrEmpty(invoiceItem.Quantity) && action != 1)
            //{
            //    abp.notify.error(app.localize('LineitemQuantityshouldByminMax', vm.touchSpinQtyOptions.min, vm.touchSpinQtyOptions.max));
            //    return false;
            //}
            var updatedInvoice = Invoice.Copy();


            //Ticket start:#22898 Composite sale not working properly.by rupesh.//addedInvoiceItemType == InvoiceItemType.CompositeProduct
            //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
            if (invoiceItem.InvoiceItemType == InvoiceItemType.Standard || invoiceItem.InvoiceItemType == InvoiceItemType.CompositeProduct || invoiceItem.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
            {
                //Ticket end:#20064.by rupesh
                //Ticket end:#22898.by rupesh
                var totalQty = invoiceItem.Quantity + invoiceItem.BackOrderQty;
                // Ticket #11964 Issues About BO added bool isBackorderConfirmation = false On iPad by rupesh
                //Ticket start:#63960 iPad: Product sold on iPad even after 0 stock.by rupesh
                var result = await CheckstockValidation(product, invoiceItem.Quantity, invoiceItem.BackOrderQty, Invoice, false, invoiceItem);
                //Ticket end:#63960.by rupesh
                //CheckstockValidation(vm.products[invoiceItem.invoiceItemValue], invoiceItem.Quantity, invoiceItem.BackOrderQty).then(function(result) {
                if (result.IsValid)
                {

                    invoiceItem.BackOrderQty = result.BackOrderQty;
                    if (result.BackOrderQty > 0)
                    {

                        if (result.Validatedstock <= 0)
                            invoiceItem.Quantity = 0;
                        else
                        {
                            invoiceItem.Quantity = result.Quantity;
                        }
                        //invoiceItem.Description = (result.BackOrderQty.ToString().IndexOf(".") > 0 ? parseFloat(result.BackOrderQty).toFixed(1) : result.BackOrderQty) + " " + app.localize('QuantityInBackOrder');
                    }
                    //else
                    //{
                    //    if (Invoice.Status != InvoiceStatus.Refunded && string.IsNullOrEmpty(invoiceItem.Description))
                    //        invoiceItem.Description = "";
                    //}

                    //Exchange Issue ticket 7987, 7657
                    foreach (var item in Invoice.InvoiceLineItems)
                    {
                        if (item.TotalAmount < 0)
                        {
                            if (item.InvoiceItemType == InvoiceItemType.Discount && Invoice.Status != InvoiceStatus.Refunded)
                            {

                                decimal discountedPrice = 0;
                                decimal discountedValue = 0;

                                //Ticket #12876 - Discount offer not working as expected. By Nikhil	
                                if (item.OfferDiscountPercent != null)
                                {

                                    var retailPrice = item.RetailPrice;
                                    if (item.DiscountValue > 0)
                                    {
                                        discountedValue = GetValuefromPercent(item.RetailPrice, item.DiscountValue);
                                        retailPrice = item.RetailPrice - discountedValue;
                                    }

                                    discountedPrice = GetValuefromPercent((retailPrice), Math.Abs(item.OfferDiscountPercent ?? 0));
                                    item.TotalAmount = item.TotalAmount - discountedPrice;
                                    item.EffectiveAmount = item.EffectiveAmount - discountedPrice;
                                    item.TaxExclusiveTotalAmount = item.TaxExclusiveTotalAmount - discountedPrice;
                                }
                                //Ticket #12876 End. By Nikhil
                            }
                        }
                    }
                    //Ticket start:#74632 iOS: Add Flat Markup% to Customer Group Pricing (FR).by rupesh
                    invoiceItem = CustomerMarkupDiscount(Invoice, invoiceItem, Invoice.CustomerDetail);
                    //Ticket end:#74632.by rupesh
                    invoiceItem = CustomerPricebookDiscount(Invoice, invoiceItem, Invoice.CustomerDetail);
                    if (Invoice.Status != InvoiceStatus.Refunded && invoiceItem.Quantity > 0)
                    {
                        await AddofferTolineItem(Invoice, offers, product, invoiceItem, productServices,false);
                    }
                    else
                    { //for refund process..

                        //Ticket start:#22898 Composite sale not working properly.by rupesh.//addedInvoiceItemType == InvoiceItemType.CompositeProduct
                        //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                        var itemCount = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.Standard || x.InvoiceItemType == InvoiceItemType.CompositeProduct || x.InvoiceItemType == InvoiceItemType.UnityOfMeasure);
                        //Ticket end:#20064 .by rupesh.
                        //Ticket end:#22898 .by rupesh.

                        if (itemCount == null && itemCount.Count() == 0)
                        {
                            var offerLineItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.Discount);
                            if (offerLineItem != null)
                            {
                                while (offerLineItem != null && offerLineItem.Count() > 0)
                                {
                                    var indexExtra = Invoice.InvoiceLineItems.IndexOf(offerLineItem.FirstOrDefault());
                                    Invoice.InvoiceLineItems.Remove(offerLineItem.FirstOrDefault());
                                }
                            }
                        }
                        else
                        {
                            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                            IEnumerable<InvoiceLineItemDto> discountedLineItem = null;
                            if (Invoice.Status == InvoiceStatus.Exchange)
                                discountedLineItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.Discount && x.InvoiceItemValue == invoiceItem.OfferId && x.IsExchangedProduct);
                            else
                                discountedLineItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.Discount && x.InvoiceItemValue == invoiceItem.OfferId);
                            //End Ticket #74344 By pratik
                            if (discountedLineItem != null && discountedLineItem.Count() > 0)
                            {
                                var discountedinvoiceItem = discountedLineItem.FirstOrDefault();
                                //Exchange Issue ticket 7987, 7657
                                decimal effectiveAmount = 0;
                                decimal soldPrice = 0;
                                decimal retailPrice = 0;
                                decimal ExlcudingtotalAmount = 0;
                                decimal totalAmount = 0;

                                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                                //foreach (var uValue in updatedInvoice.InvoiceLineItems)
                                foreach (var uValue in updatedInvoice.InvoiceLineItems.Where(x => Invoice.Status == InvoiceStatus.Exchange ? x.IsExchangedProduct : (x.IsExchangedProduct || !x.IsExchangedProduct)))
                                {

                                    if (uValue.InvoiceItemValue != invoiceItem.InvoiceItemValue && uValue.OfferId == discountedinvoiceItem.InvoiceItemValue && uValue.InvoiceItemType == InvoiceItemType.Discount)
                                    {

                                        decimal discountedValue = 0;
                                        if ((uValue.DiscountValue != 0 || uValue.CustomerGroupDiscountPercent != 0) && uValue.SoldPrice != uValue.RetailPrice)
                                            discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                        else if (uValue.SoldPrice > uValue.RetailPrice)
                                            discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                        else
                                            discountedValue = GetValuefromPercent(uValue.RetailPrice, uValue.OfferDiscountPercent);
                                        //if (uValue.TotalAmount >= 0)
                                        //{
                                        //decimal discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                        decimal amount = 0;
                                        //if (uValue.refundedQuantity == 0)
                                        amount = discountedValue * Math.Abs(uValue.Quantity);
                                        //else
                                        //    amount = discountedValue * uValue.refundedQuantity;

                                        effectiveAmount += amount;
                                        soldPrice += amount;
                                        retailPrice += amount;
                                        ExlcudingtotalAmount += amount;
                                        totalAmount += amount;
                                        //}
                                    }
                                    else
                                    {
                                        //if (action == 0)
                                        if (action == 0 && uValue.OfferId == discountedinvoiceItem.InvoiceItemValue)
                                        {
                                            decimal discountedValue = 0;
                                            if (uValue.SoldPrice != uValue.RetailPrice)
                                            {
                                                discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                            }
                                            else
                                                discountedValue = GetValuefromPercent(uValue.RetailPrice, uValue.OfferDiscountPercent);
                                            //var discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                            decimal amount = 0;
                                            amount = discountedValue * Math.Abs(uValue.Quantity);

                                            effectiveAmount += amount;
                                            soldPrice += amount;
                                            retailPrice += amount;
                                            ExlcudingtotalAmount += amount;
                                            totalAmount += amount;
                                        }
                                    }
                                }


                                decimal effectiveAmount_ex = 0;
                                decimal soldPrice_ex = 0;
                                decimal retailPrice_ex = 0;
                                decimal ExlcudingtotalAmount_ex = 0;
                                decimal totalAmount_ex = 0;
                                // Start Update or Remove For Exchange Discount Itemline
                                //foreach (var uValue in updatedInvoice.InvoiceLineItems)
                                foreach (var uValue in updatedInvoice.InvoiceLineItems.Where(x => Invoice.Status == InvoiceStatus.Exchange ? (x.IsExchangedProduct) : (x.IsExchangedProduct || !x.IsExchangedProduct)))
                                {
                                    if (uValue.InvoiceItemValue != invoiceItem.InvoiceItemValue && uValue.OfferId == discountedinvoiceItem.InvoiceItemValue && uValue.InvoiceItemType != InvoiceItemType.Discount)
                                    {
                                        //decimal discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                        decimal discountedValue = 0;
                                        if ((uValue.DiscountValue != 0 || uValue.CustomerGroupDiscountPercent != 0) && uValue.SoldPrice != uValue.RetailPrice)
                                            discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                        else if (uValue.SoldPrice > uValue.RetailPrice)
                                            discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                        else
                                            discountedValue = GetValuefromPercent(uValue.RetailPrice, uValue.OfferDiscountPercent);
                                        decimal amount = 0;
                                        //if (uValue.refundedQuantity == 0)
                                        amount = discountedValue * Math.Abs(uValue.Quantity);
                                        //else
                                        //    amount = discountedValue * uValue.refundedQuantity;

                                        effectiveAmount_ex += amount;
                                        soldPrice_ex += amount;
                                        retailPrice_ex += amount;
                                        ExlcudingtotalAmount_ex += amount;
                                        totalAmount_ex += amount;

                                        foreach (var item in uValue.LineItemTaxes)
                                        {
                                            //Subtax calculation changes
                                            if (updatedInvoice.TaxInclusive)
                                            {
                                                item.TaxAmount = GetValuefromPercent(uValue.TaxExclusiveTotalAmount - (GetValuefromPercent((Math.Abs(uValue.SoldPrice) - Math.Abs(uValue.TaxAmount)), uValue.OfferDiscountPercent) * uValue.Quantity), item.TaxRate);
                                            }
                                            else
                                            {
                                                item.TaxAmount = GetValuefromPercent(uValue.TaxExclusiveTotalAmount - (GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent) * uValue.Quantity), item.TaxRate);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        //if (action == 0)
                                        if (action == 0 && uValue.OfferId == discountedinvoiceItem.InvoiceItemValue)
                                        {
                                            //var discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                            decimal discountedValue = 0;
                                            if (uValue.SoldPrice != uValue.RetailPrice)
                                            {
                                                discountedValue = GetValuefromPercent(uValue.SoldPrice, uValue.OfferDiscountPercent);
                                            }
                                            else
                                                discountedValue = GetValuefromPercent(uValue.RetailPrice, uValue.OfferDiscountPercent);
                                            decimal amount = 0;
                                            amount = discountedValue * Math.Abs(uValue.Quantity);

                                            effectiveAmount_ex += amount;
                                            soldPrice_ex += amount;
                                            retailPrice_ex += amount;
                                            ExlcudingtotalAmount_ex += amount;
                                            totalAmount_ex += amount;
                                        }
                                    }
                                }
                                //End Ticket #74344 By Pratik


                                if (effectiveAmount > 0)
                                {
                                    discountedinvoiceItem.EffectiveAmount = effectiveAmount;
                                    discountedinvoiceItem.SoldPrice = soldPrice;
                                    discountedinvoiceItem.RetailPrice = retailPrice;
                                    discountedinvoiceItem.TaxExclusiveTotalAmount = ExlcudingtotalAmount;
                                    discountedinvoiceItem.TotalAmount = totalAmount;
                                }
                                else
                                {
                                    var offerLineItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.Discount && x.InvoiceItemValue == invoiceItem.OfferId && x.RetailPrice <= 0);

                                    if (offerLineItem != null)
                                    {
                                        while (offerLineItem != null && offerLineItem.Count() > 0)
                                        {
                                            //Ticket start: #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted by Pratik
                                            var firstoffer = offerLineItem.FirstOrDefault();
                                            var lineItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount && (x.OfferId == null || x.OfferId == firstoffer.InvoiceItemValue) && x.IsExchangedProduct == false);
                                            lineItem?.ForEach(a => a.OfferDiscountPercent = null);

                                            Invoice.InvoiceLineItems.Remove(firstoffer);
                                            //Ticket end: #65929 by Pratik

                                            // Invoice.InvoiceLineItems.Remove(offerLineItem.FirstOrDefault());
                                        }
                                    }
                                }

                                // Update or Remove For Exchange Discount Itemline
                                if (effectiveAmount_ex > 0)
                                {
                                    discountedinvoiceItem.EffectiveAmount = effectiveAmount_ex;
                                    discountedinvoiceItem.SoldPrice = soldPrice_ex;
                                    discountedinvoiceItem.RetailPrice = retailPrice_ex;
                                    discountedinvoiceItem.TaxExclusiveTotalAmount = ExlcudingtotalAmount_ex;
                                    discountedinvoiceItem.TotalAmount = totalAmount_ex;
                                    //Start Ticket #74344 iOS and WEB :: Discount Issue By Pratik
                                    //discountedinvoiceItem.TaxAmount = GetValuefromPercent(totalAmount_ex, discountedinvoiceItem.TaxRate);
                                    decimal taxAmount = 0;
                                    taxAmount = CalculateTaxInclusive(discountedinvoiceItem.TaxExclusiveTotalAmount, discountedinvoiceItem.TaxRate);

                                    //Ticket start:#14508 Exchange&Refund issue in buyxgetyvalueoffer by rupesh
                                    if (Invoice.TaxInclusive)
                                    {
                                        discountedinvoiceItem.TaxExclusiveTotalAmount = discountedinvoiceItem.TotalAmount - taxAmount;
                                        discountedinvoiceItem.EffectiveAmount = discountedinvoiceItem.TaxExclusiveTotalAmount + taxAmount;
                                    }
                                    //Ticket end:#14508 by rupesh

                                    discountedinvoiceItem.TaxAmount = GetValuefromPercent(discountedinvoiceItem.TaxExclusiveTotalAmount, discountedinvoiceItem.TaxRate);
                                    //End Ticket #74344 By Pratik


                                }
                                else
                                {
                                    var offerLineItem = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.Discount && x.InvoiceItemValue == invoiceItem.OfferId && x.RetailPrice >= 0);
                                    if (invoiceItem.actualqty < 0)
                                    {
                                        if (offerLineItem != null)
                                        {
                                            while (offerLineItem != null && offerLineItem.Count() > 0)
                                            {
                                                //Ticket start: #65926 Discounts are not working properly  by Pratik
                                                var firstoffer = offerLineItem.FirstOrDefault();
                                                var applyofferLineItem = Invoice.InvoiceLineItems.Where(x => (x.OfferId == null || x.OfferId == firstoffer.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount);
                                                applyofferLineItem?.ForEach(a => a.OfferDiscountPercent = null);
                                                //Ticket end: #65926  by Pratik

                                                Invoice.InvoiceLineItems.Remove(offerLineItem.FirstOrDefault());
                                            }
                                        }
                                    }
                                }
                                // End Update or Remove For Exchange Discount Itemline

                            }
                        }
                    }

                    CalculateLineItemTotal(invoiceItem, Invoice,false);
                }
            }
            else
            {
                if (invoiceItem.InvoiceItemType == InvoiceItemType.Composite)
                {

                    var haveStock = true;
                    foreach (var item in invoiceItem.InvoiceLineSubItems)
                    {
                        if (haveStock)
                        {
                            var productnew =  productServices.GetLocalProduct(item.ItemId);
                            var checkvalidateQty = invoiceItem.Quantity * item.Quantity;
                            var result = await CheckCompositestockValidation(productnew, checkvalidateQty, 0);
                            if (!result.IsValid)
                                haveStock = false;
                        }
                    }

                    if (!haveStock)
                    {
                        //abp.libs.sweetAlert.config.confirm.confirmButtonText = app.localize('Yes');
                        //app.confirmMessage(app.localize('CompositeOutOfStockValidationMessage'), app.localize('OutOfStock'), "sweet-alert-info", function(stockresult)
                        //{

                        //}
                    }
                    else
                    {
                        CalculateLineItemTotal(invoiceItem, Invoice);
                        //vm.cartBackdrop = false;
                    }
                }
                else
                {
                    CalculateLineItemTotal(invoiceItem, Invoice);
                }
                foreach (var item in Invoice.InvoiceLineItems)
                {
                    if (item.InvoiceItemType == InvoiceItemType.Discount && Invoice.Status != InvoiceStatus.Refunded)
                    {
                        decimal discountedPrice = 0;
                        decimal discountedValue = 0;

                        if (invoiceItem.OfferDiscountPercent != null)
                        {

                            var retailPrice = invoiceItem.RetailPrice;
                            if (invoiceItem.DiscountValue > 0)
                            {
                                discountedValue = GetValuefromPercent(invoiceItem.RetailPrice, invoiceItem.DiscountValue);
                                retailPrice = invoiceItem.RetailPrice - discountedValue;
                            }

                            discountedPrice = GetValuefromPercent((retailPrice), Math.Abs(invoiceItem.OfferDiscountPercent ?? 0));
                            item.TotalAmount = item.TotalAmount - discountedPrice;
                            item.EffectiveAmount = item.EffectiveAmount - discountedPrice;
                            item.TaxExclusiveTotalAmount = item.TaxExclusiveTotalAmount - discountedPrice;
                        }
                        // vm.calculateLineItemTotal(invoiceItem, vm.invoice);
                    }
                }

            }
            return Invoice;
        }

        //START Ticket #74344 iOS and WEB :: Discount Issue : FOR CustomerPricebookDiscount By pratik
        public static InvoiceLineItemDto CustomerPricebookDiscount(InvoiceDto invoice, InvoiceLineItemDto invoiceItem, CustomerDto_POS customer)
        {
            if (invoiceItem != null && invoiceItem.Quantity < 0 && (invoice.Status == InvoiceStatus.Exchange || invoice.Status == InvoiceStatus.Refunded))
            {
                return invoiceItem;
            }
            if (invoice.CustomerGroupDiscountType)
            {
                decimal maxQty = 0;
                if (invoice.CustomerGroupId != null)
                {
                    var flag = false;
                    if (customer != null && customer.CustomerGroupId != null)
                    {
                        var customerGroup = customerService.GetLocalCustomerGroupById(customer.CustomerGroupId.Value);

                        if (customerGroup != null && customerGroup.PriceBookLists != null)
                        {
                            var totalProdQuantity = invoice.InvoiceLineItems.Where(x => x.Sku == invoiceItem.Sku && x.Quantity > 0 && x.IsExchangedProduct == false).Sum(x => x.Quantity);
                            var totalMinProdQuantity = invoice.InvoiceLineItems.Where(x => x.Sequence != invoiceItem.Sequence && x.Sku == invoiceItem.Sku && x.Quantity > 0 && x.IsExchangedProduct == false && x.Quantity != x.isDiscountAdded).Sum(x => (x.Quantity - x.isDiscountAdded));
                            var totalProd_Cnt = invoice.InvoiceLineItems.Count(x => x.Sku == invoiceItem.Sku && x.Quantity > 0 && x.IsExchangedProduct == false);
                            var otherProdQuantity = invoice.InvoiceLineItems.Where(x => x.Sku == invoiceItem.Sku && x.isDiscountAdded > 0 && x.Quantity > 0 && x.IsExchangedProduct == false).Sum(a => a.isDiscountAdded);
                            var otherSeqProdQuantity = invoice.InvoiceLineItems.Where(x => x.Sequence != invoiceItem.Sequence && x.Sku == invoiceItem.Sku && x.isDiscountAdded > 0 && x.Quantity > 0 && x.IsExchangedProduct == false).Sum(a => a.isDiscountAdded);
                            var woProdQuantity = totalProdQuantity - otherSeqProdQuantity;

                            foreach (var item in customerGroup.PriceBookLists)
                            {
                                var hasoutlet = item.PriceBookOutlets.FirstOrDefault(x => x.OutletId == invoice.OutletId);
                                if (hasoutlet != null)
                                {
                                    var hasTemplate = item.PricebookTemplates.Where(x => x.PriceBookId == hasoutlet.PriceBookId);
                                    if (flag == false)
                                    {
                                        foreach (var item1 in hasTemplate)
                                        {
                                            if (flag == false)
                                            {
                                                if (item1.SKU == invoiceItem.Sku && invoiceItem.InvoiceItemType != InvoiceItemType.Discount && !invoiceItem.DisableDiscountIndividually)
                                                {
                                                    if (Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct)
                                                    {
                                                        if (invoice.Status != InvoiceStatus.Refunded)
                                                        {
                                                            if (item1.MinUnits != null && item1.MaxUnits == null && Math.Abs(totalProdQuantity) >= item1.MinUnits)
                                                            {
                                                                maxQty = invoiceItem.Quantity - ((invoiceItem.Quantity - invoiceItem.isDiscountAdded) - 1);

                                                                if (totalProd_Cnt == 1 && invoiceItem.Quantity < (item1.MinUnits.Value - 1))
                                                                {
                                                                    maxQty = 0;
                                                                }
                                                                else if (totalProd_Cnt == 1 && invoiceItem.Quantity >= (item1.MinUnits.Value - 1))
                                                                {
                                                                    maxQty = invoiceItem.Quantity - (item1.MinUnits.Value - 1);
                                                                }
                                                                else if (woProdQuantity < (item1.MinUnits - 1) && invoiceItem.Quantity > (((item1.MinUnits.Value - 1) - woProdQuantity)))
                                                                {
                                                                    maxQty = invoiceItem.Quantity - ((item1.MinUnits.Value - 1) - woProdQuantity);
                                                                }
                                                                else if (woProdQuantity >= (item1.MinUnits - 1) && invoiceItem.isDiscountAdded > 0 && totalMinProdQuantity >= (item1.MinUnits - 1))
                                                                {
                                                                    maxQty = invoiceItem.Quantity;
                                                                }
                                                                else if (woProdQuantity >= (item1.MinUnits - 1) && totalMinProdQuantity < item1.MinUnits && invoiceItem.Quantity >= (((item1.MinUnits.Value) - totalMinProdQuantity)))
                                                                {
                                                                    if (invoiceItem.isDiscountAdded > 0)
                                                                        maxQty = invoiceItem.Quantity - ((invoiceItem.Quantity - invoiceItem.isDiscountAdded));
                                                                    else if (totalMinProdQuantity == (item1.MinUnits.Value - 1))
                                                                        maxQty = invoiceItem.Quantity;
                                                                    else
                                                                        maxQty = invoiceItem.Quantity - ((item1.MinUnits.Value - 1) - totalMinProdQuantity);
                                                                }
                                                                else if (woProdQuantity >= (item1.MinUnits - 1) && totalMinProdQuantity < item1.MinUnits && invoiceItem.Quantity <= totalMinProdQuantity)
                                                                {
                                                                    maxQty = 0;
                                                                }

                                                                var Qty = invoiceItem.Quantity - maxQty;
                                                                if (Qty >= 0)
                                                                    invoiceItem = CalculateCustomerDiscount(maxQty, Qty, item1, invoiceItem, ref invoice);
                                                            }
                                                            else if ((item1.MinUnits == null && item1.MaxUnits == null)
                                                                || (item1.MinUnits == null && item1.MaxUnits != null && Math.Abs(totalProdQuantity) <= item1.MaxUnits)
                                                                || (item1.MinUnits != null && item1.MaxUnits != null && Math.Abs(totalProdQuantity) >= item1.MinUnits && Math.Abs(totalProdQuantity) <= item1.MaxUnits))
                                                            {
                                                                if (item1.MinUnits != null && item1.MaxUnits != null && Math.Abs(totalProdQuantity) >= item1.MinUnits && Math.Abs(totalProdQuantity) <= item1.MaxUnits && invoiceItem.Quantity > 1 && (invoiceItem.isDiscountAdded <= 0 || invoiceItem.isDiscountAdded > 1))
                                                                {
                                                                    var Qty = 1;
                                                                    if (otherProdQuantity < (item1.MaxUnits.Value - (item1.MinUnits.Value - 1)))
                                                                    {
                                                                        maxQty = invoiceItem.Quantity;
                                                                        Qty = 0;
                                                                    }
                                                                    else
                                                                        maxQty = (invoiceItem.Quantity - 1);
                                                                    invoiceItem = CalculateCustomerDiscount(maxQty, Qty, item1, invoiceItem, ref invoice);
                                                                }
                                                                else
                                                                {
                                                                    invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                                    invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (invoiceItem.Quantity);
                                                                    invoiceItem.isDiscountAdded = invoiceItem.Quantity;
                                                                    invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + item1.RetailPrice.ToString("C");
                                                                }
                                                                flag = true;
                                                            }
                                                            else if (!item1.MinUnits.HasValue && item1.MaxUnits.HasValue && totalProdQuantity > item1.MaxUnits)
                                                            {

                                                                maxQty = (totalProdQuantity + 1) - otherProdQuantity;
                                                                var Qty = invoiceItem.Quantity - maxQty;
                                                                if (Qty <= 0)
                                                                {
                                                                    if (totalProd_Cnt == 1 || invoiceItem.isDiscountAdded == otherProdQuantity || (otherProdQuantity <= 0 && invoiceItem.isDiscountAdded <= 0))
                                                                    {
                                                                        maxQty = Math.Min(invoiceItem.Quantity, item1.MaxUnits.Value);
                                                                    }
                                                                    else if (invoiceItem.isDiscountAdded > 0)
                                                                        maxQty = Math.Min(invoiceItem.Quantity, invoiceItem.isDiscountAdded);
                                                                    else
                                                                    {
                                                                        var total = (item1.MaxUnits.Value - otherProdQuantity);
                                                                        if (total > 0)
                                                                        {
                                                                            maxQty = Math.Min(invoiceItem.Quantity, total);
                                                                        }
                                                                        else
                                                                            maxQty = 0;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (otherProdQuantity >= item1.MaxUnits.Value)
                                                                        maxQty = invoiceItem.isDiscountAdded;
                                                                }
                                                                Qty = invoiceItem.Quantity - maxQty;
                                                                invoiceItem = CalculateCustomerDiscount(maxQty, Qty, item1, invoiceItem, ref invoice);
                                                                flag = true;
                                                            }
                                                            else if (item1.MinUnits.HasValue && totalProdQuantity > item1.MinUnits && item1.MaxUnits.HasValue && totalProdQuantity > item1.MaxUnits)
                                                            {
                                                                maxQty = (totalProdQuantity - (item1.MinUnits.Value - 1)) - otherProdQuantity;
                                                                var Qty = invoiceItem.Quantity - maxQty;
                                                                if (Qty <= 0)
                                                                {
                                                                    if (totalProd_Cnt == 1 || invoiceItem.isDiscountAdded == otherProdQuantity || (otherProdQuantity <= 0 && invoiceItem.isDiscountAdded <= 0))
                                                                    {
                                                                        maxQty = Math.Min(invoiceItem.Quantity, (item1.MaxUnits.Value - (item1.MinUnits.Value - 1)));
                                                                    }
                                                                    else if (invoiceItem.isDiscountAdded > 0)
                                                                        maxQty = Math.Min(invoiceItem.Quantity, invoiceItem.isDiscountAdded);
                                                                    else
                                                                    {
                                                                        var total = (item1.MaxUnits.Value - (item1.MinUnits.Value - 1)) - otherProdQuantity;
                                                                        if (total > 0)
                                                                        {
                                                                            maxQty = Math.Min(invoiceItem.Quantity, total);
                                                                        }
                                                                        else
                                                                            maxQty = 0;
                                                                    }
                                                                    Qty = invoiceItem.Quantity - maxQty;
                                                                }

                                                                invoiceItem = CalculateCustomerDiscount(maxQty, Qty, item1, invoiceItem, ref invoice);
                                                                flag = true;
                                                            }
                                                        }
                                                    }
                                                    //Min and Max null
                                                    else if (item1.MinUnits == null && item1.MaxUnits == null && invoice.Status != InvoiceStatus.Refunded)
                                                    {
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (invoiceItem.Quantity);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item Price" + " " + item1.RetailPrice.ToString("C");
                                                        flag = true;
                                                    }
                                                    else if (item1.MinUnits == null && item1.MaxUnits == null && invoice.Status == InvoiceStatus.Refunded)
                                                    {
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (invoiceItem.Quantity);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item Price" + " " + item1.RetailPrice.ToString("C");
                                                    }
                                                    //Min not null and Max is null
                                                    else if (item1.MinUnits != null && item1.MaxUnits == null && invoiceItem.Quantity >= item1.MinUnits && invoice.Status != InvoiceStatus.Refunded)
                                                    {
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (invoiceItem.Quantity);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item Price" + " " + item1.RetailPrice.ToString("C");
                                                        flag = true;
                                                    }
                                                    else if (item1.MinUnits != null && item1.MaxUnits == null && Math.Abs(invoiceItem.Quantity) >= item1.MinUnits && invoice.Status == InvoiceStatus.Refunded)
                                                    {
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (invoiceItem.Quantity);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item Price" + " " + item1.RetailPrice.ToString("C");
                                                        flag = true;
                                                    }
                                                    //Min is null and Max is not null
                                                    else if (item1.MinUnits == null && item1.MaxUnits != null && invoiceItem.Quantity <= item1.MaxUnits && invoice.Status != InvoiceStatus.Refunded)
                                                    {
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (invoiceItem.Quantity);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + item1.RetailPrice.ToString("C");
                                                        flag = true;
                                                    }
                                                    else if (item1.MinUnits == null && item1.MaxUnits != null && Math.Abs(invoiceItem.Quantity) <= item1.MaxUnits && invoice.Status == InvoiceStatus.Refunded)
                                                    {
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (invoiceItem.Quantity);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + item1.RetailPrice.ToString("C");
                                                        flag = true;
                                                    }
                                                    //min is not null and max is not null and quantity is between unit
                                                    else if (item1.MinUnits != null && item1.MaxUnits != null && invoiceItem.Quantity >= item1.MinUnits && invoiceItem.Quantity <= item1.MaxUnits && invoice.Status != InvoiceStatus.Refunded)
                                                    {
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (invoiceItem.Quantity);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + item1.RetailPrice.ToString("C");
                                                        flag = true;
                                                    }
                                                    else if (item1.MinUnits != null && item1.MaxUnits != null && Math.Abs(invoiceItem.Quantity) >= item1.MinUnits && Math.Abs(invoiceItem.Quantity) <= item1.MaxUnits && invoice.Status == InvoiceStatus.Refunded)
                                                    {
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (invoiceItem.Quantity);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + item1.RetailPrice.ToString("C");
                                                        flag = true;
                                                    }
                                                    //min is not null and max is not null and Quantity is greater than max qty
                                                    else if (item1.MinUnits != null && item1.MaxUnits != null && invoiceItem.Quantity > item1.MaxUnits && invoice.Status != InvoiceStatus.Refunded)
                                                    {
                                                        maxQty = item1.MaxUnits ?? 0;
                                                        var Qty = invoiceItem.Quantity - item1.MaxUnits;
                                                        var perDiscount = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        var ddiscountperItem = GetValuefromPercent(invoiceItem.RetailPrice, perDiscount);
                                                        var mretailPrice = invoiceItem.RetailPrice - ddiscountperItem;
                                                        var finalPrice = (mretailPrice * maxQty) + (Qty * invoiceItem.RetailPrice);
                                                        var orgPrice = invoiceItem.RetailPrice * invoiceItem.Quantity;
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(orgPrice, finalPrice ?? 0);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (maxQty);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + finalPrice?.ToString("C");
                                                        flag = true;
                                                    }
                                                    else if (item1.MinUnits != null && item1.MaxUnits != null && Math.Abs(invoiceItem.Quantity) > item1.MaxUnits && invoice.Status == InvoiceStatus.Refunded)
                                                    {
                                                        maxQty = item1.MaxUnits ?? 0;

                                                        var Qty = Math.Abs(invoiceItem.Quantity) - item1.MaxUnits;
                                                        var perDiscount = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);

                                                        var ddiscountperItem = GetValuefromPercent(invoiceItem.RetailPrice, perDiscount);
                                                        var mretailPrice = invoiceItem.RetailPrice - ddiscountperItem;
                                                        var finalPrice = (mretailPrice * maxQty) + (Qty * invoiceItem.RetailPrice);
                                                        var orgPrice = invoiceItem.RetailPrice * Math.Abs(invoiceItem.Quantity);

                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(orgPrice, finalPrice ?? 0);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (maxQty);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + item1.RetailPrice.ToString("C");
                                                        flag = true;
                                                    }
                                                    //Min is null and Max is not null and Quantity greater than Max Qty
                                                    else if (item1.MinUnits == null && item1.MaxUnits != null && invoiceItem.Quantity > item1.MaxUnits && invoice.Status != InvoiceStatus.Refunded)
                                                    {
                                                        maxQty = item1.MaxUnits ?? 0;
                                                        var Qty = invoiceItem.Quantity - item1.MaxUnits;
                                                        var perDiscount = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        var ddiscountperItem = GetValuefromPercent(invoiceItem.RetailPrice, perDiscount);
                                                        var mretailPrice = invoiceItem.RetailPrice - ddiscountperItem;
                                                        var finalPrice = (mretailPrice * maxQty) + (Qty * invoiceItem.RetailPrice);
                                                        var orgPrice = invoiceItem.RetailPrice * invoiceItem.Quantity;
                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(orgPrice, finalPrice ?? 0);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (maxQty);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + finalPrice?.ToString("C");
                                                        flag = true;
                                                    }
                                                    else if (item1.MinUnits == null && item1.MaxUnits != null && Math.Abs(invoiceItem.Quantity) > item1.MaxUnits && invoice.Status == InvoiceStatus.Refunded)
                                                    {
                                                        maxQty = item1.MaxUnits ?? 0;

                                                        var Qty = Math.Abs(invoiceItem.Quantity) - item1.MaxUnits;
                                                        var perDiscount = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);

                                                        var ddiscountperItem = GetValuefromPercent(invoiceItem.RetailPrice, perDiscount);
                                                        var mretailPrice = invoiceItem.RetailPrice - ddiscountperItem;
                                                        var finalPrice = (mretailPrice * maxQty) + (Qty * invoiceItem.RetailPrice);
                                                        var orgPrice = invoiceItem.RetailPrice * Math.Abs(invoiceItem.Quantity);

                                                        invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(orgPrice, finalPrice ?? 0);
                                                        invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (maxQty);
                                                        invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + item1.RetailPrice.ToString("C");
                                                        flag = true;
                                                    }
                                                    else
                                                    {
                                                        if (invoice.Status == InvoiceStatus.Refunded && invoiceItem.CustomerGroupDiscountPercent > 0)
                                                        {
                                                            invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
                                                        }
                                                        else
                                                        {
                                                            invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + item1.RetailPrice.ToString("C");
                                                            invoiceItem.CustomerGroupDiscountPercent = 0;
                                                        }
                                                    }
                                                }
                                                else
                                                    invoice.CustomerGroupDiscountNoteInside = "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (invoice.CustomerGroupDiscount != 0 && !invoiceItem.DisableDiscountIndividually)
                invoiceItem.CustomerGroupDiscountPercent = invoice.CustomerGroupDiscount;
            return invoiceItem;
        }

        private static InvoiceLineItemDto CalculateCustomerDiscount(decimal maxQty, decimal Qty, GroupPriceTemplateDto item1, InvoiceLineItemDto invoiceItem, ref InvoiceDto invoice)
        {
            var perDiscount = GetDiscountPercentfromValue(invoiceItem.RetailPrice, item1.RetailPrice);
            var ddiscountperItem = GetValuefromPercent(invoiceItem.RetailPrice, perDiscount);
            var mretailPrice = invoiceItem.RetailPrice - ddiscountperItem;
            var finalPrice1 = (mretailPrice * maxQty) + (Qty * invoiceItem.RetailPrice);
            var orgPrice = invoiceItem.RetailPrice * invoiceItem.Quantity;
            invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(orgPrice, finalPrice1);
            invoiceItem.CustomerGroupLoyaltyPoints = (item1.LoyaltyValue ?? 0) * (maxQty);
            invoiceItem.isDiscountAdded = maxQty;
            invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + invoice.CustomerGroupName + ", " + "Item discount" + " " + finalPrice1.ToString("C");
            return invoiceItem;
        }
        //END Ticket #74344 By pratik

        //Ticket start:#74632 iOS: Add Flat Markup% to Customer Group Pricing (FR).by rupesh
        public static InvoiceLineItemDto CustomerMarkupDiscount(InvoiceDto invoice, InvoiceLineItemDto invoiceItem, CustomerDto_POS customer)
        {
            if (invoice.CustomerGroupId != null)
            {
                if (customer != null && customer.CustomerGroupId != null)
                {
                    var customerGroup = customerService.GetLocalCustomerGroupById(customer.CustomerGroupId.Value);
                    if (customerGroup.CustomerGroupDiscountType == 3)
                    {
                        if (invoiceItem.InvoiceItemType == InvoiceItemType.Standard || invoiceItem.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
                        {
                            decimal taxAmount = 0;
                            if (invoice.TaxInclusive)
                                taxAmount = GetValuefromPercent(invoiceItem.ItemCost, invoiceItem.TaxRate);
                            var finalPrice = invoiceItem.ItemCost + taxAmount;
                            var customerMarkup = GetValuefromPercent(finalPrice, customerGroup.DiscountPercent);
                            finalPrice = finalPrice + customerMarkup;
                            invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, finalPrice);

                        }
                        else if (invoiceItem.InvoiceItemType == InvoiceItemType.Custom)
                        {
                            var finalPrice = invoiceItem.RetailPrice;
                            var customerMarkup = GetValuefromPercent(finalPrice, customerGroup.DiscountPercent);
                            finalPrice = finalPrice + customerMarkup;
                            invoiceItem.CustomerGroupDiscountPercent = GetDiscountPercentfromValue(invoiceItem.RetailPrice, finalPrice);
                        }

                    }

                }
            }
            return invoiceItem;

        }
        //Ticket end:#74632.by rupesh

        //Percentage Offer..
        static InvoiceLineItemDto ApplyPercentOff(InvoiceDto invoice, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, int? customerGroupId, ProductServices productServices)
        {
            try
            {
                ObservableCollection<InvoiceLineItemDto> toaddOffer = new ObservableCollection<InvoiceLineItemDto>();
                List<OfferItemDto> offerItem = null;

                ObservableCollection<OfferItemDto> offerItemObject = productoffer.OfferItems;
                toaddOffer = ApplyPercentageOffOffer(invoice, offerItemObject, offerItem, toaddOffer, productoffer, invoiceLineItem, productServices);
                //Ticket start:#26866,#26599 Getting different total amount in cart in iPad.(No discount for UOM product).by rupesh
                toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
                //Ticket end:#26866,,#26599 .by rupesh

                //var offerName = "Discount" + "/ " + productoffer.Name;
                var lineItemTitle = "";
                if (productoffer.Description == null)
                    lineItemTitle = "Discount" + "/ " + productoffer.Name;
                else
                    lineItemTitle = "Discount" + "/ " + productoffer.Description;

                decimal finalDiscountValue = 0;
                decimal sumQty = 0;
                decimal finalDiscount = 0;
                foreach (var item in toaddOffer)
                {
                    if (item.IsOfferAdded)
                        continue;
                    if ((item.Quantity - item.BackOrderQty ?? 0) <= item.Quantity)
                    {
                        sumQty += item.Quantity;

                        //customer group discount..
                        if (customerGroupId != null)
                        {
                            var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent == null ? 0 : item.CustomerGroupDiscountPercent);
                            item.SoldPrice = item.RetailPrice - discount;
                        }
                        else if (customerGroupId == null && item.DiscountValue == 0)
                        {
                            item.SoldPrice = item.RetailPrice;
                        }
                        else if (customerGroupId == null)
                        {
                            item.SoldPrice = item.RetailPrice;
                        }
                        //customer group discount..

                        //line item Discount..
                        if (item.DiscountValue > 0)
                        {
                            var discountedValue = GetValuefromPercent(item.SoldPrice, item.DiscountValue);
                            item.SoldPrice = item.SoldPrice - discountedValue;
                        }
                        //line item Discount..

                        finalDiscountValue = GetValuefromPercent(item.SoldPrice, productoffer.OfferValue) * item.Quantity;

                        var discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), (item.SoldPrice * item.Quantity) - finalDiscountValue);

                        item.OfferDiscountPercent = discountPrice;
                        //#104346
                        if(discountPrice > 0)
                        {
                            item.OfferId = productoffer.OfferId;
                            item.OffersNote = item.CategoryDtos != null ? productoffer.Description : "";
                        }
                        //#104346

                        if (invoiceLineItem.DiscountIsAsPercentage)
                        {
                            InvoiceLineItemDto hasInvoiceItem = null;
                            //Start #84438 Exchange type discount issue 
                            //hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                            if (invoice.Status == InvoiceStatus.Exchange && invoice.InvoiceLineItems.Count(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount) > 1)
                                hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.EffectiveAmount <= 0 && x.InvoiceItemType == InvoiceItemType.Discount);
                            else
                                hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                            //End #84438 Exchange type discount issue 

                            decimal discountedPrice = 0;
                            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                            var hasRecentlyAddedInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);
                            if (hasInvoiceItem == null || (invoice.Status == InvoiceStatus.Exchange && hasRecentlyAddedInvoiceItem == null && hasInvoiceItem.TaxAmount >= 0 && hasInvoiceItem.EffectiveAmount >= 0))
                            {
                                AddInvoiceItemLineApplyPercentOff(invoice, productoffer, invoiceLineItem, lineItemTitle, out finalDiscountValue, item, out discountedPrice);
                                finalDiscount = finalDiscountValue;
                            }
                            else
                            {
                                //Ticket start.#15502 Discount offers by rupesh
                                //Ticket #13213 : Display individually as separate line items not working.By Nikhil  
                                decimal qty = item.Quantity;
                                /* if (Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct)
									 qty = invoice.InvoiceLineItems.Count(x => x.InvoiceItemType == InvoiceItemType.Standard && x.InvoiceItemValue == item.InvoiceItemValue && x.Quantity > 0);*/
                                //Ticket #13213 End.By Nikhil
                                //Ticket end.#15502. by rupesh

                                //Start Inclusive Tax Pratik
                                finalDiscountValue = GetValuefromPercent(item.SoldPrice, productoffer.OfferValue) * qty;

                                if (invoice.TaxInclusive)
                                {
                                    var taxAmount1 = CalculateTaxInclusive(finalDiscount, hasInvoiceItem.TaxRate);
                                    var taxAmount2 = CalculateTaxInclusive(finalDiscountValue, item.TaxRate);
                                    var taxAmount = taxAmount1 + taxAmount2;

                                    finalDiscount = finalDiscount + finalDiscountValue;
                                    if (finalDiscount - taxAmount != 0)
                                        hasInvoiceItem.TaxRate = taxAmount * 100 / (finalDiscount - taxAmount);
                                    //Ticket start:#24889,#25034 .by rupesh
                                }
                                else
                                {
                                    var taxAmount1 = GetValuefromPercent(finalDiscount, hasInvoiceItem.TaxRate);
                                    var taxAmount2 = GetValuefromPercent(finalDiscountValue, item.TaxRate);
                                    var taxAmount = taxAmount1 + taxAmount2;

                                    finalDiscount = finalDiscount + finalDiscountValue;
                                    if (finalDiscount - taxAmount != 0)
                                        hasInvoiceItem.TaxRate = (taxAmount * 100) / finalDiscount;
                                }
                                //End Inclusive Tax Pratik

                                discountedPrice = -finalDiscount;
                                UpdateInvoiceItemLineApplyPercentOff(invoice, productoffer, item, hasInvoiceItem, discountedPrice);
                                // UpdateInvoiceItemLineApplyPercentOff(invoice, productoffer, out finalDiscountValue, finalDiscount, item, hasInvoiceItem, out discountedPrice);

                            }
                            //End Ticket #74344 By pratik 
                        }
                    }
                }
                if (toaddOffer.Count() == 0)
                {
                    //var offerInvItems = abpCommonHelpers.$filter("where")(Invoice.invoiceLineItems, { offerId: productoffer.offerId, invoiceItemType: app.consts.invoiceItemType.standard, invoiceItemValue: invoiceLineItem.invoiceItemValue });
                    var offerInvItems = invoice.InvoiceLineItems.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && a.InvoiceItemValue != invoiceLineItem.InvoiceItemValue && a.OfferId == productoffer.OfferId);
                    if (offerInvItems.Count() == 0)
                    {
                        var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                        while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                        {
                            //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                            invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
                        }
                    }
                }

                return invoiceLineItem;
            }
            catch (Exception ex)
            {
                ex.Track();
                return invoiceLineItem;
            }
        }
        //Exchange Issue ticket 7987, 7657
        private static void AddInvoiceItemLineApplyPercentOff(InvoiceDto invoice, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, string lineItemTitle, out decimal finalDiscountValue, InvoiceLineItemDto item, out decimal discountedPrice)
        {
            InvoiceLineItemDto invoiceItem = new InvoiceLineItemDto();

            if (invoiceLineItem != null && invoiceLineItem.TaxId != 0)
            {
                invoiceItem.TaxId = invoiceLineItem.TaxId;
                invoiceItem.TaxName = invoiceLineItem.TaxName;
                invoiceItem.TaxRate = invoiceLineItem.TaxRate;
            }
            else
            {
                invoiceItem.TaxId = 1;
                invoiceItem.TaxName = "NoTax";
                invoiceItem.TaxRate = 0;
            }

            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                if (currentRegister.Registerclosure != null)
                    invoiceItem.RegisterClosureId = currentRegister.Registerclosure.Id;
                else
                    invoiceItem.RegisterClosureId = null;
            }
            invoiceItem.InvoiceItemValue = productoffer.OfferId ?? 0;
            invoiceItem.InvoiceItemValueParent = item.InvoiceItemValue;

            //Ticket #9247 Start:Following code commented to solve ticket. By Nikhil.
            //if (item.InvoiceItemValueParent != null)
            //    invoiceItem.InvoiceItemValueParent = item.InvoiceItemValueParent;
            //else
            //    invoiceItem.InvoiceItemValueParent = item.InvoiceItemValue; 
            //Ticket #9247 End:By Nikhil.

            invoiceItem.InvoiceItemType = InvoiceItemType.Discount;
            invoiceItem.Sequence = invoice.InvoiceLineItems.Count() + 1;

            finalDiscountValue = GetValuefromPercent(item.SoldPrice, productoffer.OfferValue) * item.Quantity;


            invoiceItem.Quantity = 1;
            invoiceItem.TotalAmount = -finalDiscountValue;
            invoiceItem.Title = lineItemTitle;
            //invoiceItem.OffersNote = offerName;
            invoiceItem.TotalDiscount = 0;
            invoiceItem.OfferId = 0;
            invoiceItem.TaxAmount = 0;
            discountedPrice = -finalDiscountValue;
            invoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            invoiceItem.EffectiveAmount = discountedPrice;
            invoiceItem.SoldPrice = discountedPrice;
            invoiceItem.RetailPrice = discountedPrice;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount - taxAmount;

            invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoiceLineItem.Quantity > 0)
            {
                
                //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
                MainThread.BeginInvokeOnMainThread(()=>{
                    if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                        invoice.InvoiceLineItems.Add(invoiceItem);
                    else
                        invoice.InvoiceLineItems.Insert(0, invoiceItem);
                });
                //End:#45375 .by rupesh
            }
        }
        //Exchange Issue ticket 7987, 7657
        //private static void UpdateInvoiceItemLineApplyPercentOff(InvoiceDto invoice, OfferDto productoffer, out decimal finalDiscountValue, decimal finalDiscount, InvoiceLineItemDto item, InvoiceLineItemDto hasInvoiceItem, out decimal discountedPrice)
        //{
        private static void UpdateInvoiceItemLineApplyPercentOff(InvoiceDto invoice, OfferDto productoffer, InvoiceLineItemDto item, InvoiceLineItemDto hasInvoiceItem, decimal discountedPrice)
        {
            //finalDiscountValue = GetValuefromPercent(item.SoldPrice, productoffer.OfferValue) * item.Quantity;
            //finalDiscount += finalDiscountValue;
            //discountedPrice = -finalDiscount;
            hasInvoiceItem.Quantity = 1;
            hasInvoiceItem.TotalAmount = discountedPrice;
            hasInvoiceItem.TotalDiscount = 0;
            hasInvoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            hasInvoiceItem.TaxAmount = 0;
            hasInvoiceItem.TaxId = item.TaxId;
            hasInvoiceItem.TaxName = item.TaxName;
            //Ticket start:#24889,#25034 IPad : Discount offter is not working.by rupesh
            //hasInvoiceItem.TaxRate = item.TaxRate;
            //Ticket end:#24889,#25034 .by rupesh

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                hasInvoiceItem.TaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount - taxAmount;

            hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            hasInvoiceItem.EffectiveAmount = discountedPrice;
            hasInvoiceItem.SoldPrice = discountedPrice;
            hasInvoiceItem.RetailPrice = discountedPrice;
        }



        //Buy X Get Y value off..
        static InvoiceDto ApplyBuyXGetValueOffer(InvoiceDto invoice, ObservableCollection<OfferDto> offers, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, int? customerGroupId, bool calltotal = true)
        {
            ObservableCollection<InvoiceLineItemDto> toaddOffer = new ObservableCollection<InvoiceLineItemDto>();

            // var offerItem = null;
            var offerItemObject = productoffer.OfferItems;
            var offerName = productoffer.Name;
            var offerOn = productoffer.OfferItems.FirstOrDefault()?.OfferOn;
            var offerOnId = productoffer.OfferItems.FirstOrDefault()?.OfferOnId;


            if (offerOn == OfferOn.Product)
            {
                foreach (var item in invoice.InvoiceLineItems)
                {
                    var productId = productoffer.OfferItems.Where(a => (a.OfferOnId == item.InvoiceItemValue || a.OfferOnId == item.InvoiceItemValueParent) && item.InvoiceItemType == InvoiceItemType.Standard).FirstOrDefault();
                    if (productId != null && !item.DisableDiscountIndividually && (item.InvoiceItemType == InvoiceItemType.Standard && (item.InvoiceItemValue == productId.OfferOnId || item.InvoiceItemValueParent == productId.OfferOnId) || item.TotalQuantity > 0)) //Start #92641 By Pratik
                    {
                        toaddOffer.Add(item);
                    }
                }
            }

            //Start Ticket #73624 iOS Discount: Buy X units of brand A and get Y units of brand B for free : Not working (All Discount with Exchange) By Pratik
            //Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
            if (invoice.Status == InvoiceStatus.Exchange)
                toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.Quantity > 0 && (x.OfferId == null || x.OfferId == productoffer.OfferId)));
            else
                toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end: #65929 by Pratik


            //Ticket start:#26866,#26599 Getting different total amount in cart in iPad.(No discount for UOM product).by rupesh
            //toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end:#26866,#26599 .by rupesh
            //End Ticket #73624 (All Discount with Exchange)

            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;

                if (item.BackOrderQty <= item.Quantity)
                {
                    //customer group discount..
                    if (customerGroupId != null)
                    {
                        var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                        item.SoldPrice = item.RetailPrice - discount;
                    }
                    else if (customerGroupId == null && item.DiscountValue == 0)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    else if (customerGroupId == null)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    //customer group discount..


                    //line item Discount..
                    if (item.DiscountValue > 0)
                    {
                        var discountedValue = GetValuefromPercent(item.SoldPrice, item.DiscountValue);
                        item.SoldPrice = item.SoldPrice - discountedValue;
                    }
                    //line item Discount..

                    //Ticket start:#65241 iOS: Discount offer not Applying on line Items while add one by one.by Pratik
                    var sumQty = toaddOffer.Where(a => a.InvoiceItemValue == item.InvoiceItemValue).Sum(x => x.Quantity);
                    //Ticket end: #65241 by Pratik

                    var productId = item.InvoiceItemValue;
                    if (item.InvoiceItemValueParent != null)
                    {
                        //Ticket start:#73980 Sock discount issue.by rupesh
                        var offer = productoffer.OfferItems.Where(a => (a.OfferOnId == item.InvoiceItemValueParent) && item.InvoiceItemType == InvoiceItemType.Standard).FirstOrDefault();
                        if (offer != null)
                        {
                            productId = item.InvoiceItemValueParent.Value;
                            var obj = toaddOffer.Where(a => a.InvoiceItemValue == productId || a.InvoiceItemValueParent == productId);
                            if (obj != null)
                            {
                                sumQty = obj.Sum(x => x.Quantity);
                            }
                        }
                        //Ticket end:#73980 .by rupesh

                    }

                    var lineItemTitle = "";
                    if (productoffer.Description == null)
                        lineItemTitle = "Discount" + "/ " + productoffer.Name;
                    else
                        lineItemTitle = "Discount" + "/ " + productoffer.Description;

                    var hasOffer = offerItemObject.FirstOrDefault(x => x.OfferOnId == item.InvoiceItemValue || x.OfferOnId == item.InvoiceItemValueParent);
                    if (hasOffer != null)
                    {
                        if (Math.Abs(sumQty) % hasOffer.CompositeQty == 0)
                        {
                            //Ticket start:#64732 tax difference while selling the product in with discount offers.by rupesh
                            decimal discountedQty = Math.Round((Math.Abs(sumQty) / hasOffer.CompositeQty ?? 1), 0);
                            //Ticket end:#64732 .by rupesh
                            var discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), (item.SoldPrice * item.Quantity) - (((hasOffer.FixedPrice ?? 0) / (hasOffer.CompositeQty ?? 1)) * item.Quantity));
                            item.OfferDiscountPercent = discountPrice;
                            //#104346
                            if(discountPrice > 0)
                            {
                                item.OfferId = productoffer.OfferId;
                                item.OffersNote = item.CategoryDtos != null ? productoffer.Description : "";
                            }
                            //#104346
                            //Exchange Issue ticket 7987, 7657
                            //Start #95236 Discount drop down menu By PR
                            var hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount);
                            //var hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount);
                            //End #95236 By PR

                            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                            var hasRecentlyAddedInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);
                            if (hasInvoiceItem == null || ((invoice.Status == InvoiceStatus.Exchange && hasRecentlyAddedInvoiceItem == null && hasInvoiceItem.TaxAmount >= 0 && hasInvoiceItem.EffectiveAmount >= 0)))
                            {
                                AddInvoiceItemLine_HasOffer_ApplyBuyXGetValueOffer(invoice, invoiceLineItem, sumQty, productId, lineItemTitle, hasOffer);
                            }
                            else
                            {
                                UpdateInvoiceItemLine_HasOffer_ApplyBuyXGetValueOffer(invoice, item, sumQty, hasOffer, hasInvoiceItem);
                            }
                            //End Ticket #74344 By pratik
                        }
                        else
                        {
                            if ((Math.Abs(sumQty)) > hasOffer.CompositeQty)
                            {
                                 //Start #95236 Discount drop down menu By PR
                                var hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount);
                                //var hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount);
                                //End #95236 By PR
                            
                                //Ticket start:#64732 tax difference while selling the product in with discount offers.by rupesh
                                //Ticket start: #65926 Discounts are not working properly  by Rupesh
                                decimal discountedQty = Math.Truncate((Math.Abs(sumQty) / hasOffer.CompositeQty ?? 1));
                                //Ticket end: #65926 .  by Rupesh
                                //var discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), (item.SoldPrice * item.Quantity) - (((hasOffer.FixedPrice ?? 0) / (hasOffer.CompositeQty ?? 1)) * discountedQty));
                                var discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), (item.SoldPrice * item.Quantity) - (((hasOffer.FixedPrice ?? 0) * discountedQty / sumQty) * item.Quantity));
                                //Ticket end:#64732 .by rupesh
                                item.OfferDiscountPercent = discountPrice;
                                //#104346
                                if(discountPrice > 0)
                                {
                                    item.OfferId = productoffer.OfferId;
                                    item.OffersNote = item.CategoryDtos != null ? productoffer.Description : "";
                                }
                                //#104346

                                //Exchange Issue ticket 7987, 7657
                                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                                var hasRecentlyAddedInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);
                                if (hasInvoiceItem == null || ((invoice.Status == InvoiceStatus.Exchange && hasRecentlyAddedInvoiceItem == null && hasInvoiceItem.TaxAmount >= 0 && hasInvoiceItem.EffectiveAmount >= 0)))
                                {
                                    AddInvoiceItemLine_HasOffer_CompositeQty_ApplyBuyXGetValueOffer(invoice, invoiceLineItem, sumQty, productId, lineItemTitle, hasOffer);
                                }
                                else
                                {
                                    UpdateInvoiceItemLine_HasOffer_CompositeQty_ApplyBuyXGetValueOffer(invoice, item, sumQty, hasOffer, hasInvoiceItem);
                                }
                                //END Ticket #74344 By pratik
                            }

                            if ((Math.Abs(sumQty)) < hasOffer.CompositeQty)
                            {
                                var offerLineItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount);
                                //Ticket start:#14508 Exchange&Refund issue in buyxgetyvalueoffer by rupesh
                                if (invoice.Status != InvoiceStatus.Exchange)
                                {
                                    while (offerLineItem != null && offerLineItem.Count() > 0)
                                    {
                                        //Ticket start: #65926 Discounts are not working properly  by Pratik
                                        var firstoffer = offerLineItem.FirstOrDefault();
                                        var applyofferLineItem = toaddOffer.Where(x => (x.OfferId == null || x.OfferId == firstoffer.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount && x.IsExchangedProduct == false);
                                        applyofferLineItem?.ForEach(a => a.OfferDiscountPercent = null);
                                        //Ticket end: #65926  by Pratik

                                        //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                                        invoice.InvoiceLineItems.Remove(offerLineItem.FirstOrDefault());
                                    }
                                }
                                //Ticket end:#14508 by rupesh
                            }
                        }
                        
                        CalculateLineItemTotal(item, invoice,calltotal);
                    }
                }
            }
            //Exchange Issue ticket 7987, 7657
            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;

                if (item.TotalAmount > 0)
                {
                    var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == item.InvoiceItemValue && x.InvoiceItemType == InvoiceItemType.Discount);
                    if (hasInvoiceItem.Count() > 0)
                    {
                        var retailPrice = item.RetailPrice;
                        foreach (var dvalue in hasInvoiceItem)
                        {
                            if (dvalue.TotalAmount < 0)
                            {
                                //Ticket start: #65926 Discounts are not working properly  by Pratik
                                var sumquantity = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == dvalue.InvoiceItemValueParent && (x.OfferId == null || x.OfferId == dvalue.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount && x.IsExchangedProduct == false).Sum(a => a.Quantity);
                                var discountedLineItem = sumquantity != 0 ? dvalue.TotalAmount / sumquantity : 0;
                                //Ticket end: #65926  by Pratik

                                if (customerGroupId != null)
                                {
                                    var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                                    retailPrice = item.RetailPrice - discount;
                                }

                                if (retailPrice < Math.Abs(discountedLineItem))
                                {
                                    item.OfferDiscountPercent = 100;
                                    discountedLineItem = discountedLineItem - item.RetailPrice;
                                }
                                else
                                {
                                    var discountPrice = GetDiscountPercentfromValue((retailPrice), (retailPrice - discountedLineItem));
                                    item.OfferDiscountPercent = Math.Abs(discountPrice);
                                }
                            }
                        }
                    }
                }
            }
            if (toaddOffer.Count == 0)
            {
                var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                {
                    //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                    invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
                }
            }
            return invoice;

        }

        //Buy X or more Get Y value off..

        //Ticket start:#14435 Discount not applying in iPad Hike App by rupesh
        static InvoiceDto ApplyBuyXOrMoreGetValueOffer(InvoiceDto invoice, ObservableCollection<OfferDto> offers, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, int? customerGroupId, bool calltotal = true)
        {
            ObservableCollection<InvoiceLineItemDto> toaddOffer = new ObservableCollection<InvoiceLineItemDto>();

            // var offerItem = null;
            var offerItemObject = productoffer.OfferItems;
            var offerName = productoffer.Name;
            var offerOn = productoffer.OfferItems.FirstOrDefault()?.OfferOn;
            var offerOnId = productoffer.OfferItems.FirstOrDefault()?.OfferOnId;


            if (offerOn == OfferOn.Product)
            {
                foreach (var item in invoice.InvoiceLineItems)
                {
                    var productId = productoffer.OfferItems.Where(a => (a.OfferOnId == item.InvoiceItemValue || a.OfferOnId == item.InvoiceItemValueParent) && (item.InvoiceItemType == InvoiceItemType.Standard || item.InvoiceItemType == InvoiceItemType.CompositeProduct)).FirstOrDefault();
                    if (productId != null  && !item.DisableDiscountIndividually && ((item.InvoiceItemType == InvoiceItemType.Standard || item.InvoiceItemType == InvoiceItemType.CompositeProduct) && (item.InvoiceItemValue == productId.OfferOnId || item.InvoiceItemValueParent == productId.OfferOnId) || item.TotalQuantity > 0)) //Start #92641 By Pratik
                    {
                        toaddOffer.Add(item);
                    }
                }
            }

            //Start Ticket #73624 iOS Discount: Buy X units of brand A and get Y units of brand B for free : Not working (All Discount with Exchange) By Pratik
            // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
            //if (invoice.Status == InvoiceStatus.Exchange)
            //    toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.IsExchangedProduct == false && (x.OfferId == null || x.OfferId == productoffer.OfferId)));
            //else
            //    toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end: #65929 by Pratik


            //Ticket start:#26866,#26599 Getting different total amount in cart in iPad.(No discount for UOM product).by rupesh
            toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end:#26866,#26599 .by rupesh
            //End Ticket #73624 (All Discount with Exchange)

            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;

                //Ticket start:#30470 iPad: discount offer is not applied for separate line item.by rupesh
                if (item.BackOrderQty.GetValueOrDefault() <= item.Quantity)
                {
                    //Ticket end:#30470 .by rupesh
                    //customer group discount..
                    if (customerGroupId != null)
                    {
                        var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                        item.SoldPrice = item.RetailPrice - discount;
                    }
                    else if (customerGroupId == null && item.DiscountValue == 0)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    else if (customerGroupId == null)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    //customer group discount..

                    //line item Discount..
                    if (item.DiscountValue > 0)
                    {
                        var discountedValue = GetValuefromPercent(item.SoldPrice, item.DiscountValue);
                        item.SoldPrice = item.SoldPrice - discountedValue;
                    }
                    //line item Discount..


                    //var sumQty = item.Quantity;
                    //Ticket start:#30470 iPad: discount offer is not applied for separate line item.by rupesh
                    var sumQty = toaddOffer.Where(a => a.InvoiceItemValue == item.InvoiceItemValue).Sum(x => x.Quantity);
                    //Ticket end:#30470 .by rupesh
                    var productId = item.InvoiceItemValue;
                    if (item.InvoiceItemValueParent != null)
                    {
                        //Ticket start: #65926 Discounts are not working properly  by Pratik
                        productId = item.InvoiceItemValueParent.Value;
                        //Ticket end: #65926 by Pratik
                        var obj = toaddOffer.Where(a => a.InvoiceItemValue == productId || a.InvoiceItemValueParent == productId);
                        if (obj != null)
                        {
                            sumQty = obj.Sum(x => x.Quantity);
                        }
                    }

                    var lineItemTitle = "";
                    if (productoffer.Description == null)
                        lineItemTitle = "Discount" + "/ " + productoffer.Name;
                    else
                        lineItemTitle = "Discount" + "/ " + productoffer.Description;

                    var hasOffer = offerItemObject.FirstOrDefault(x => x.OfferOnId == item.InvoiceItemValue || x.OfferOnId == item.InvoiceItemValueParent);
                    if (hasOffer != null)
                    {

                        if ((Math.Abs(sumQty)) >= hasOffer.CompositeQty)
                        {
                            var hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount);
                            //Ticket start:#64732 tax difference while selling the product in with discount offers.by rupesh
                            //Ticket start: #65926 Discounts are not working properly  by Rupesh
                            decimal discountedQty = Math.Truncate((Math.Abs(sumQty) / hasOffer.CompositeQty ?? 1));
                            //var discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), ((item.SoldPrice * 1) - (((hasOffer.FixedPrice ?? 0) * sumQty / (hasOffer.CompositeQty ?? 1)) * discountedQty)) * item.Quantity);
                            var discountPrice = GetDiscountPercentfromValue(item.SoldPrice, item.SoldPrice - (hasOffer.FixedPrice ?? 0));
                            //Ticket end: #65926 .  by Rupesh
                            //Ticket end:#64732 .by rupesh
                            item.OfferDiscountPercent = discountPrice;
                            //Exchange Issue ticket 7987, 7657
                            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                            var hasRecentlyAddedInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);
                            if (hasInvoiceItem == null || ((invoice.Status == InvoiceStatus.Exchange && hasRecentlyAddedInvoiceItem == null && hasInvoiceItem.TaxAmount >= 0 && hasInvoiceItem.EffectiveAmount >= 0)))
                            {
                                AddInvoiceItemLine_HasOffer_CompositeQty_ApplyBuyXOrMoreGetValueOffer(invoice, invoiceLineItem, sumQty, productId, lineItemTitle, hasOffer);
                            }
                            else
                            {
                                UpdateInvoiceItemLine_HasOffer_CompositeQty_ApplyBuyXOrMoreGetValueOffer(invoice, item, sumQty, hasOffer, hasInvoiceItem);
                            }
                            //END Ticket #74344 By pratik
                        }

                        if ((Math.Abs(sumQty)) < hasOffer.CompositeQty)
                        {
                            var offerLineItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == productId && x.InvoiceItemType == InvoiceItemType.Discount);

                            //Ticket start:#14508 Exchange&Refund issue in buyxgetyvalueoffer by rupesh
                            if (invoice.Status != InvoiceStatus.Exchange)
                            {

                                while (offerLineItem != null && offerLineItem.Count() > 0)
                                {
                                    //Ticket start: #65926 Discounts are not working properly  by Pratik
                                    var firstoffer = offerLineItem.FirstOrDefault();
                                    var applyofferLineItem = toaddOffer.Where(x => (x.OfferId == null || x.OfferId == firstoffer.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount && x.IsExchangedProduct == false);
                                    applyofferLineItem?.ForEach(a => a.OfferDiscountPercent = null);
                                    //Ticket end: #65926  by Pratik
                                    //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                                    invoice.InvoiceLineItems.Remove(offerLineItem.FirstOrDefault());
                                }
                            }
                            //Ticket end:#14508 by rupesh
                        }

                        CalculateLineItemTotal(item, invoice, calltotal);

                    }
                }
            }
            //Exchange Issue ticket 7987, 7657
            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;

                if (item.TotalAmount > 0)
                {
                    var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValueParent == item.InvoiceItemValue && x.InvoiceItemType == InvoiceItemType.Discount);


                    if (hasInvoiceItem.Count() > 0)
                    {
                        var retailPrice = item.RetailPrice;
                        foreach (var dvalue in hasInvoiceItem)
                        {
                            if (dvalue.TotalAmount < 0)
                            {
                                //Ticket start: #65926 Discounts are not working properly  by Pratik
                                var sumquantity = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == dvalue.InvoiceItemValueParent && (x.OfferId == null || x.OfferId == dvalue.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount && x.IsExchangedProduct == false).Sum(a => a.Quantity);
                                var discountedLineItem = sumquantity != 0 ? dvalue.TotalAmount / sumquantity : 0;
                                //Ticket end: #65926  by Pratik

                                if (customerGroupId != null)
                                {
                                    var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                                    retailPrice = item.RetailPrice - discount;
                                }

                                if (retailPrice < Math.Abs(discountedLineItem))
                                {
                                    item.OfferDiscountPercent = 100;
                                    discountedLineItem = discountedLineItem - item.RetailPrice;
                                }
                                else
                                {
                                    var discountPrice = GetDiscountPercentfromValue((retailPrice), (retailPrice - discountedLineItem));
                                    item.OfferDiscountPercent = Math.Abs(discountPrice);
                                }
                            }
                        }
                    }
                }
            }
            if (toaddOffer.Count == 0)
            {
                var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                {
                    //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                    invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
                }
            }
            return invoice;

        }
        //Ticket emd:#14435

        //Exchange Issue ticket 7987, 7657
        private static void AddInvoiceItemLine_HasOffer_CompositeQty_ApplyBuyXGetValueOffer(InvoiceDto invoice, InvoiceLineItemDto invoiceLineItem, decimal sumQty, int productId, string lineItemTitle, OfferItemDto hasOffer)
        {
            var invoiceItem = new InvoiceLineItemDto();
            if (invoiceLineItem != null && invoiceLineItem.TaxId != 0)
            {
                invoiceItem.TaxId = invoiceLineItem.TaxId;
                invoiceItem.TaxName = invoiceLineItem.TaxName;
                invoiceItem.TaxRate = invoiceLineItem.TaxRate;
            }
            else
            {
                invoiceItem.TaxId = 1;
                invoiceItem.TaxName = "NoTax";
                invoiceItem.TaxRate = 0;
            }
            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                invoiceItem.RegisterClosureId = (currentRegister.Registerclosure != null) ? currentRegister.Registerclosure.Id : 0;
            }
            invoiceItem.InvoiceItemValue = hasOffer.OfferId;
            invoiceItem.InvoiceItemValueParent = productId;
            invoiceItem.InvoiceItemType = InvoiceItemType.Discount;
            invoiceItem.Sequence = invoice.InvoiceLineItems.Count() + 1;

            decimal finalQty = 0;
            //finalQty = Math.Round(((sumQty) / hasOffer.CompositeQty.Value), 0);

            finalQty = Math.Floor((Math.Abs(sumQty)) / hasOffer.CompositeQty.Value);

            invoiceItem.Quantity = invoiceItem.Quantity + finalQty;
            invoiceItem.TotalAmount = finalQty * -hasOffer.FixedPrice.Value;  //- hasOffer.fixedPrice;
            invoiceItem.Title = lineItemTitle;
            invoiceItem.TotalDiscount = 0;

            invoiceItem.TaxAmount = 0;
            decimal discountedPrice = finalQty * -hasOffer.FixedPrice.Value;
            invoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            invoiceItem.EffectiveAmount = discountedPrice;
            invoiceItem.SoldPrice = -hasOffer.FixedPrice ?? 0;
            invoiceItem.RetailPrice = -hasOffer.FixedPrice ?? 0;

            var productsLength = invoice.InvoiceLineItems.Count();
            invoiceItem.Sequence = productsLength + 1;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount - taxAmount;

            invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            //invoiceItem.class = 'sell-item-offer';
            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
            MainThread.BeginInvokeOnMainThread(()=>{
                    if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                        invoice.InvoiceLineItems.Add(invoiceItem);
                    else
                        invoice.InvoiceLineItems.Insert(0, invoiceItem);
            });
            //End:#45375 .by rupesh
        }
        //Exchange Issue ticket 7987, 7657
        private static void UpdateInvoiceItemLine_HasOffer_CompositeQty_ApplyBuyXGetValueOffer(InvoiceDto invoice, InvoiceLineItemDto item, decimal sumQty, OfferItemDto hasOffer, InvoiceLineItemDto hasInvoiceItem)
        {
            decimal finalQty = 0;
            //Ticket start:#65241 iOS: Discount offer not Applying on line Items while add one by one.by Pratik
            //if (item.InvoiceItemValueParent != null)
            finalQty = Math.Floor(((Math.Abs(sumQty)) / hasOffer.CompositeQty.Value));
            //else
            //	finalQty = Math.Floor(((item.Quantity) / hasOffer.CompositeQty.Value));
            //End:#65241 .by Pratik

            decimal discountedPrice = finalQty * -hasOffer.FixedPrice.Value;
            hasInvoiceItem.Quantity = finalQty;
            hasInvoiceItem.TotalAmount = discountedPrice;
            hasInvoiceItem.TotalDiscount = 0;
            hasInvoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            hasInvoiceItem.TaxId = item.TaxId;
            hasInvoiceItem.TaxName = item.TaxName;
            hasInvoiceItem.TaxRate = item.TaxRate;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                hasInvoiceItem.TaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount - taxAmount;

            hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            hasInvoiceItem.EffectiveAmount = discountedPrice;
            hasInvoiceItem.SoldPrice = -hasOffer.FixedPrice ?? 0;
            hasInvoiceItem.RetailPrice = -hasOffer.FixedPrice ?? 0;
        }
        //Exchange Issue ticket 7987, 7657
        private static void AddInvoiceItemLine_HasOffer_ApplyBuyXGetValueOffer(InvoiceDto invoice, InvoiceLineItemDto invoiceLineItem, decimal sumQty, int productId, string lineItemTitle, OfferItemDto hasOffer)
        {
            var invoiceItem = new InvoiceLineItemDto();
            if (invoiceLineItem != null && invoiceLineItem.TaxId != 0)
            {
                invoiceItem.TaxId = invoiceLineItem.TaxId;
                invoiceItem.TaxName = invoiceLineItem.TaxName;
                invoiceItem.TaxRate = invoiceLineItem.TaxRate;
            }
            else
            {
                invoiceItem.TaxId = 1;
                invoiceItem.TaxName = "NoTax";
                invoiceItem.TaxRate = 0;
            }
            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                invoiceItem.RegisterClosureId = (currentRegister.Registerclosure != null) ? currentRegister.Registerclosure.Id : 0;
            }
            invoiceItem.InvoiceItemValue = hasOffer.OfferId;
            invoiceItem.InvoiceItemValueParent = productId;
            invoiceItem.InvoiceItemType = InvoiceItemType.Discount;
            invoiceItem.Sequence = invoice.InvoiceLineItems.Count() + 1;

            decimal finalQty = 0;
            finalQty = Math.Round(((Math.Abs(sumQty)) / hasOffer.CompositeQty.Value), 0);


            invoiceItem.Quantity = invoiceItem.Quantity + finalQty;
            invoiceItem.TotalAmount = finalQty * -hasOffer.FixedPrice.Value;  //- hasOffer.fixedPrice;
            invoiceItem.Title = lineItemTitle;
            invoiceItem.TotalDiscount = 0;

            invoiceItem.TaxAmount = 0;
            decimal discountedPrice = finalQty * -hasOffer.FixedPrice.Value;
            invoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            invoiceItem.EffectiveAmount = discountedPrice;

            
            invoiceItem.SoldPrice = -hasOffer.FixedPrice ?? 0;
            invoiceItem.RetailPrice = -hasOffer.FixedPrice ?? 0;

            var productsLength = invoice.InvoiceLineItems.Count();
            invoiceItem.Sequence = productsLength + 1;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount - taxAmount;

            invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);


            //invoiceItem.class = 'sell-item-offer';
            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
            MainThread.BeginInvokeOnMainThread(()=>{
                if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    invoice.InvoiceLineItems.Add(invoiceItem);
                else
                    invoice.InvoiceLineItems.Insert(0, invoiceItem);
            });
            //End:#45375 .by rupesh

        }
        //Exchange Issue ticket 7987, 7657
        private static void UpdateInvoiceItemLine_HasOffer_ApplyBuyXGetValueOffer(InvoiceDto invoice, InvoiceLineItemDto item, decimal sumQty, OfferItemDto hasOffer, InvoiceLineItemDto hasInvoiceItem)
        {
            decimal finalQty = 0;

            //Ticket start:#65241 iOS: Discount offer not Applying on line Items while add one by one.by Pratik
            //if (item.InvoiceItemValueParent != null)
            finalQty = Math.Floor(((Math.Abs(sumQty)) / hasOffer.CompositeQty.Value));
            //else
            //	finalQty = Math.Round(((item.Quantity) / hasOffer.CompositeQty.Value), 0);
            //End:#65241 .by Pratik

            decimal discountedPrice = finalQty * -hasOffer.FixedPrice.Value;
            hasInvoiceItem.Quantity = finalQty;
            hasInvoiceItem.TotalAmount = discountedPrice;
            hasInvoiceItem.TotalDiscount = 0;
            hasInvoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            hasInvoiceItem.TaxId = item.TaxId;
            hasInvoiceItem.TaxName = item.TaxName;
            hasInvoiceItem.TaxRate = item.TaxRate;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                hasInvoiceItem.TaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount - taxAmount;

            hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            hasInvoiceItem.EffectiveAmount = discountedPrice;
            hasInvoiceItem.SoldPrice = -hasOffer.FixedPrice ?? 0;
            hasInvoiceItem.RetailPrice = -hasOffer.FixedPrice ?? 0;
        }

        //Ticket start:#14435 Discount not applying in iPad Hike App by rupesh
        private static void AddInvoiceItemLine_HasOffer_CompositeQty_ApplyBuyXOrMoreGetValueOffer(InvoiceDto invoice, InvoiceLineItemDto invoiceLineItem, decimal sumQty, int productId, string lineItemTitle, OfferItemDto hasOffer)
        {
            var invoiceItem = new InvoiceLineItemDto();
            if (invoiceLineItem != null && invoiceLineItem.TaxId != 0)
            {
                invoiceItem.TaxId = invoiceLineItem.TaxId;
                invoiceItem.TaxName = invoiceLineItem.TaxName;
                invoiceItem.TaxRate = invoiceLineItem.TaxRate;
            }
            else
            {
                invoiceItem.TaxId = 1;
                invoiceItem.TaxName = "NoTax";
                invoiceItem.TaxRate = 0;
            }
            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                invoiceItem.RegisterClosureId = (currentRegister.Registerclosure != null) ? currentRegister.Registerclosure.Id : 0;
            }
            invoiceItem.InvoiceItemValue = hasOffer.OfferId;
            invoiceItem.InvoiceItemValueParent = productId;
            invoiceItem.InvoiceItemType = InvoiceItemType.Discount;
            invoiceItem.Sequence = invoice.InvoiceLineItems.Count() + 1;

            decimal finalQty = 0;
            //finalQty = Math.Round(((sumQty) / hasOffer.CompositeQty.Value), 0);

            finalQty = Math.Floor(Math.Abs(sumQty));

            //Ticket start:#15313 Android - Buy X and More Get $Y off Not Working Fine.by rupesh
            invoiceItem.Quantity = 1;// invoiceItem.Quantity + finalQty;
                                     //Ticket end:#15313.by rupesh
            invoiceItem.TotalAmount = finalQty * -hasOffer.FixedPrice.Value;  //- hasOffer.fixedPrice;
            invoiceItem.Title = lineItemTitle;
            invoiceItem.TotalDiscount = 0;

            invoiceItem.TaxAmount = 0;
            decimal discountedPrice = finalQty * -hasOffer.FixedPrice.Value;
            invoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            invoiceItem.EffectiveAmount = discountedPrice;
            //Ticket start:#14508 Exchange&Refund issue in buyxgetyvalueoffer. by rupesh
            invoiceItem.SoldPrice = -hasOffer.FixedPrice * finalQty ?? 0;
            invoiceItem.RetailPrice = -hasOffer.FixedPrice * finalQty ?? 0;
            //Ticket end:#14508. by rupesh

            var productsLength = invoice.InvoiceLineItems.Count();
            invoiceItem.Sequence = productsLength + 1;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount - taxAmount;

            invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);


            //invoiceItem.class = 'sell-item-offer';
            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
            MainThread.BeginInvokeOnMainThread(()=>{
                if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    invoice.InvoiceLineItems.Add(invoiceItem);
                else
                    invoice.InvoiceLineItems.Insert(0, invoiceItem);
            });
            //End:#45375 .by rupesh
        }

        private static void UpdateInvoiceItemLine_HasOffer_CompositeQty_ApplyBuyXOrMoreGetValueOffer(InvoiceDto invoice, InvoiceLineItemDto item, decimal sumQty, OfferItemDto hasOffer, InvoiceLineItemDto hasInvoiceItem)
        {
            //Ticket start:#30470 iPad: discount offer is not applied for separate line item.by rupesh
            decimal finalQty = 0;
            // if (item.InvoiceItemValueParent != null)
            //Ticket start: #65926 Discounts are not working properly .by Rupesh
            finalQty = Math.Abs(sumQty);
            //Ticket end: #65926 .by Rupesh
            //else
            //    finalQty = Math.Floor(item.Quantity);

            //Ticket end:#30470 .by rupesh
            decimal discountedPrice = finalQty * -hasOffer.FixedPrice.Value;
            //Ticket start:#14435 Discount not applying in iPad Hike App.by rupesh
            hasInvoiceItem.Quantity = 1;// finalQty;
                                        //Ticket end:#14435 .by rupesh
            hasInvoiceItem.TotalAmount = discountedPrice;
            hasInvoiceItem.TotalDiscount = 0;
            hasInvoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            hasInvoiceItem.TaxId = item.TaxId;
            hasInvoiceItem.TaxName = item.TaxName;
            hasInvoiceItem.TaxRate = item.TaxRate;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                hasInvoiceItem.TaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount - taxAmount;

            hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            hasInvoiceItem.EffectiveAmount = discountedPrice;
            //Ticket start:#14508 Exchange&Refund issue in buyxgetyvalueoffer. by rupesh
            hasInvoiceItem.SoldPrice = -hasOffer.FixedPrice * finalQty ?? 0;
            hasInvoiceItem.RetailPrice = -hasOffer.FixedPrice * finalQty ?? 0;
            //Ticket end:#14508 . by rupesh
        }
        //Ticket end:#14435 by rupesh

        //Bin discount
        static InvoiceLineItemDto ApplyBinOffer(InvoiceDto invoice, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, int? customerGroupId, ProductServices productServices)
        {
            var retailPrice = invoiceLineItem.RetailPrice;
            if (invoice.CustomerGroupDiscount != null && invoiceLineItem.InvoiceItemType != InvoiceItemType.GiftCard)
            {
                var discountperItem = GetValuefromPercent(invoiceLineItem.RetailPrice, invoice.CustomerGroupDiscount);
                retailPrice -= discountperItem;
            }

            ObservableCollection<InvoiceLineItemDto> toaddOffer = new ObservableCollection<InvoiceLineItemDto>();
            List<OfferItemDto> offerItem = null;

            var offerItemObject = productoffer.OfferItems;
            toaddOffer = ApplyPercentageOffOffer(invoice, offerItemObject, offerItem, toaddOffer, productoffer, invoiceLineItem, productServices);
            //Ticket start:#26866,#26599 Getting different total amount in cart in iPad.(No discount for UOM product).by rupesh
            toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end:#26866,#26599 .by rupesh
            var lineItemTitle = "";
            if (productoffer.Description == null)
                lineItemTitle = "Discount" + "/ " + productoffer.Name;
            else
                lineItemTitle = "Discount" + "/ " + productoffer.Description;

            decimal finalDiscountedQuantity = 0;
            //Exchange Issue ticket 7987, 7657
            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;

                if (item.EffectiveAmount > 0)
                {
                    finalDiscountedQuantity = finalDiscountedQuantity + item.Quantity;
                }
            }


            decimal finalDiscountValue = 0;
            decimal finalDiscount = 0;
            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
            decimal finalTaxExclusiveTotalAmount = 0;
            decimal finalTax = 0;
            decimal finalTaxRate = 0;
            //End Ticket #74344 By pratik

            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;

                if ((item.Quantity - item.BackOrderQty ?? 0) <= item.Quantity)
                {
                    if (item.SoldPrice == 0)
                        item.SoldPrice = item.RetailPrice;
                    //customer group discount..
                    if (customerGroupId != null)
                    {
                        var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                        item.SoldPrice = item.RetailPrice - discount;
                    }
                    else if (customerGroupId == null && item.DiscountValue == 0)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    else if (customerGroupId == null)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    //customer group discount..

                    //line item Discount..
                    if (item.DiscountValue > 0)
                    {
                        var discountedValue = GetValuefromPercent(item.SoldPrice, item.DiscountValue);
                        item.SoldPrice = item.SoldPrice - discountedValue;
                    }
                    //line item Discount..


                    decimal discountPrice = 0;
                    if (productoffer.OfferValue > item.SoldPrice)
                    {
                        discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), item.SoldPrice);
                    }
                    else
                    {
                        discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), (productoffer.OfferValue * item.Quantity));
                    }

                    //var discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), productoffer.OfferValue);
                    item.OfferDiscountPercent = discountPrice;
                    //#104346
                    if(discountPrice > 0)
                    {
                        item.OfferId = productoffer.OfferId;
                        item.OffersNote = item.CategoryDtos != null ? productoffer.Description : "";
                    }
                    //#104346

                    if (item.SoldPrice >= productoffer.OfferValue)
                    {
                        //Exchange Issue ticket 7987, 7657
                        InvoiceLineItemDto hasInvoiceItem = null;
                        //Start #84438 Exchange type discount issue 
                        //hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                        if (invoice.Status == InvoiceStatus.Exchange && invoice.InvoiceLineItems.Count(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount) > 1)
                            hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.EffectiveAmount <= 0 && x.InvoiceItemType == InvoiceItemType.Discount);
                        else
                            hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                        //End #84438 Exchange type discount issue

                        //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                        var hasRecentlyAddedInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);
                        if (hasInvoiceItem == null || (invoice.Status == InvoiceStatus.Exchange && hasRecentlyAddedInvoiceItem == null))
                        {
                            AddInvoiceItemLineApplyBinOffer(invoice, productoffer, invoiceLineItem, lineItemTitle, out finalDiscountValue, out finalDiscount, item);
                        }
                        else
                        {
                            UpdateInvoiceItemLineApplyBinOffer(invoice, productoffer, ref finalDiscountValue, ref finalDiscount, ref finalTax, ref finalTaxExclusiveTotalAmount, ref finalTaxRate, item, hasInvoiceItem);
                        }
                        //End Ticket #74344 By pratik
                    }
                }
            }
            if (toaddOffer.Count == 0)
            {
                var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                //var tmphasInvoiceItem = hasInvoiceItem.Copy();
                while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                {
                    //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                    invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
                }
            }


            var discountedLineItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount && x.TotalAmount == 0);
            while (discountedLineItem != null && discountedLineItem.Count() > 0)
            {
                //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                invoice.InvoiceLineItems.Remove(discountedLineItem.FirstOrDefault());
            }

            return invoiceLineItem;
        }
        //Exchange Issue ticket 7987, 7657
        private static void AddInvoiceItemLineApplyBinOffer(InvoiceDto invoice, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, string lineItemTitle, out decimal finalDiscountValue, out decimal finalDiscount, InvoiceLineItemDto item)
        {
            var invoiceItem = new InvoiceLineItemDto();

            if (invoiceLineItem != null && invoiceLineItem.TaxId != 0)
            {
                invoiceItem.TaxId = invoiceLineItem.TaxId;
                invoiceItem.TaxName = invoiceLineItem.TaxName;
                invoiceItem.TaxRate = invoiceLineItem.TaxRate;
            }
            else
            {
                invoiceItem.TaxId = 1;
                invoiceItem.TaxName = "NoTax";
                invoiceItem.TaxRate = 0;
            }

            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                invoiceItem.RegisterClosureId = (currentRegister.Registerclosure != null) ? currentRegister.Registerclosure.Id : 0;
            }

            invoiceItem.InvoiceItemValue = productoffer.OfferId ?? 0;

            if (item.InvoiceItemValueParent != null)
                invoiceItem.InvoiceItemValueParent = item.InvoiceItemValueParent;
            else
                invoiceItem.InvoiceItemValueParent = item.InvoiceItemValue;
            invoiceItem.InvoiceItemValueParent = item.InvoiceItemValue;
            invoiceItem.InvoiceItemType = InvoiceItemType.Discount;
            invoiceItem.Sequence = invoice.InvoiceLineItems.Count() + 1;


            if (item.RetailPrice < productoffer.OfferValue)
                finalDiscountValue = item.SoldPrice * item.Quantity;
            else
                finalDiscountValue = (item.SoldPrice - productoffer.OfferValue) * item.Quantity;
            finalDiscount = finalDiscountValue;
            invoiceItem.Quantity = 1;
            invoiceItem.TotalAmount = -finalDiscountValue;
            invoiceItem.Title = lineItemTitle;
            //invoiceItem.OffersNote = offerName;
            invoiceItem.TotalDiscount = 0;

            invoiceItem.TaxAmount = 0;
            var discountedPrice = -finalDiscountValue;
            invoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            invoiceItem.EffectiveAmount = discountedPrice;
            invoiceItem.SoldPrice = discountedPrice;
            invoiceItem.RetailPrice = discountedPrice;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount - taxAmount;

            invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);


            //invoiceItem.class = 'sell-item-offer';
            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
             MainThread.BeginInvokeOnMainThread(()=>{
                if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    invoice.InvoiceLineItems.Add(invoiceItem);
                else
                    invoice.InvoiceLineItems.Insert(0, invoiceItem);
            });
            //End:#45375 .by rupesh
        }
        //Exchange Issue ticket 7987, 7657

        //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
        private static void UpdateInvoiceItemLineApplyBinOffer(InvoiceDto invoice, OfferDto productoffer, ref decimal finalDiscountValue, ref decimal finalDiscount, ref decimal finalTax, ref decimal finalTaxExclusiveTotalAmount, ref decimal finalTaxRate, InvoiceLineItemDto item, InvoiceLineItemDto hasInvoiceItem)
        {
            if (item.RetailPrice < productoffer.OfferValue)
            {
                finalDiscountValue = item.SoldPrice * item.Quantity;
                finalDiscount += finalDiscountValue;
            }
            else
            {
                if (item.SoldPrice <= productoffer.OfferValue)
                {
                    var lineItemCount = invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.Standard);
                    if (lineItemCount.Count() == 1)
                    {
                        var hasInvoiceItem1 = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);

                        while (hasInvoiceItem1 != null && hasInvoiceItem1.Count() > 0)
                        {
                            //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                            invoice.InvoiceLineItems.Remove(hasInvoiceItem1.FirstOrDefault());
                        }
                    }
                    else
                    {
                        finalDiscountValue = (item.SoldPrice - productoffer.OfferValue) * item.Quantity;
                    }
                }
                else
                {
                    finalDiscountValue = (item.SoldPrice - productoffer.OfferValue) * item.Quantity;
                    finalDiscount += finalDiscountValue;
                }
            }
            //finalDiscountValue = (item.SoldPrice - productoffer.OfferValue) * item.Quantity;



            //finalDiscount += finalDiscountValue;
            var discountedPrice = -finalDiscount;
            hasInvoiceItem.Quantity = 1;
            hasInvoiceItem.TotalAmount = discountedPrice;
            hasInvoiceItem.TotalDiscount = 0;
            hasInvoiceItem.TaxExclusiveTotalAmount = -finalDiscountValue;
            hasInvoiceItem.TaxId = item.TaxId;
            hasInvoiceItem.TaxName = item.TaxName;
            hasInvoiceItem.TaxRate = item.TaxRate;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                hasInvoiceItem.TaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount - taxAmount;

            hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            finalTaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount + finalTaxExclusiveTotalAmount;

            hasInvoiceItem.TaxExclusiveTotalAmount = finalTaxExclusiveTotalAmount;

            hasInvoiceItem.TaxAmount = finalTax + hasInvoiceItem.TaxAmount;

            finalTax = hasInvoiceItem.TaxAmount;

            // hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            hasInvoiceItem.EffectiveAmount = discountedPrice;
            hasInvoiceItem.SoldPrice = discountedPrice;
            hasInvoiceItem.RetailPrice = discountedPrice;
        }
        //End Ticket #74344 By pratik

        //Buy X Get Y % off..
        static InvoiceDto ApplyBuyXGetPercentOffer(InvoiceDto invoice, ObservableCollection<OfferDto> offers, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, int? customerGroupId, ProductServices productServices,bool callTotal = true)
        {
            ObservableCollection<InvoiceLineItemDto> toaddOffer = new ObservableCollection<InvoiceLineItemDto>();
            var offerItemObject = productoffer.OfferItems;
            List<OfferItemDto> offerItem = null;

            toaddOffer = ApplyPercentageOffOffer(invoice, offerItemObject, offerItem, toaddOffer, productoffer, invoiceLineItem, productServices);

            //Start Ticket #73624 iOS Discount: Buy X units of brand A and get Y units of brand B for free : Not working (All Discount with Exchange) By Pratik
            //START Ticket #74344 iOS and WEB :: Discount Issue : FOR CustomerPricebookDiscount By pratik
            if (invoice.Status == InvoiceStatus.Exchange)
                toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.Quantity > 0));
            else
                toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end: #65929 by Pratik
            //Ticket start:#26866,#26599 Getting different total amount in cart in iPad.(No discount for UOM product).by rupesh
            // toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end:#26866,#26599 .by rupesh
            //End Ticket #73624 (All Discount with Exchange)


            var sumQty = toaddOffer.Where(x => x.Quantity > 0).Sum(a => a.Quantity);
            var discountedQty = GetDiscountQtyWhenBuyx(productoffer.BuyX.Value, sumQty);

            toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.OrderBy(i => i.RetailPrice));

            // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
            //         foreach (var item in toaddOffer)
            //{
            //	if (item.IsOfferAdded)
            //		continue;

            //	if (item.DiscountedQty > 0)
            //	{
            //		item.DiscountedQty = 0;
            //		item.DiscountValue = 0;
            //		CalculateLineItemTotal(item, invoice);
            //	}
            //}

            // Ticket End #65929 By Pratik

            var lineItemTitle = "";
            if (productoffer.Description == null)
                lineItemTitle = "Discount" + "/ " + productoffer.Name;
            else
                lineItemTitle = "Discount" + "/ " + productoffer.Description;

            decimal finalDiscountedQuantity = 0;
            decimal totalRetailPrice = 0;

            // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
            //Exchange Issue ticket 7987, 7657
            //         foreach (var item in toaddOffer)
            //{
            //	if (item.IsOfferAdded)
            //		continue;

            //	if (item.EffectiveAmount > 0)
            //	{
            //		finalDiscountedQuantity = finalDiscountedQuantity + item.Quantity;
            //		totalRetailPrice = totalRetailPrice + (item.RetailPrice * item.Quantity);
            //	}
            //}
            // Ticket End #65929 By Pratik

            decimal finalDiscountValue = 0;
            decimal finalDiscount = 0;
            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;
                
                // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
                if (item.DiscountedQty > 0)
                {
                    item.DiscountedQty = 0;
                    item.DiscountValue = 0;
                }


                CalculateLineItemTotal(item, invoice, callTotal);

                if (item.EffectiveAmount > 0)
                {
                    finalDiscountedQuantity = finalDiscountedQuantity + item.Quantity;
                    totalRetailPrice = totalRetailPrice + (item.RetailPrice * item.Quantity);
                }
                // Ticket End #65929 By Pratik

                if ((item.Quantity - item.BackOrderQty ?? 0) <= item.Quantity)
                {
                    if (item.SoldPrice == 0)
                        item.SoldPrice = item.RetailPrice;
                    //customer group discount..
                    if (customerGroupId != null)
                    {
                        var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                        item.SoldPrice = item.RetailPrice - discount;
                        totalRetailPrice = 0;
                    }
                    else if (customerGroupId == null && item.DiscountValue == 0)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    else if (customerGroupId == null)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    //customer group discount..

                    //line item Discount..
                    if (item.DiscountValue > 0)
                    {
                        var discountedValue = GetValuefromPercent(item.SoldPrice, item.DiscountValue);
                        item.SoldPrice = item.SoldPrice - discountedValue;
                    }
                    //line item Discount.

                    if (discountedQty > 0)
                    {
                        if (sumQty >= productoffer.BuyX)
                        {
                            //#94421
                            if (productoffer.IsPercentage)
                                finalDiscountValue = GetValuefromPercent(item.SoldPrice, productoffer.OfferValue) * item.Quantity;
                            else
                                finalDiscountValue = productoffer.OfferValue * item.Quantity;
                            //#94421

                            finalDiscount += finalDiscountValue;

                            var discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), (item.SoldPrice * item.Quantity) - finalDiscountValue);
                            item.OfferDiscountPercent = discountPrice;
                            
                            //#104346
                            if(discountPrice > 0)
                            {
                                item.OfferId = productoffer.OfferId;
                                item.OffersNote = item.CategoryDtos != null ? productoffer.Description : "";
                            }
                            //#104346

                            //Exchange Issue ticket 7987, 7657
                            InvoiceLineItemDto hasInvoiceItem = null;
                            //Start #84438 Exchange type discount issue 
                            //hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                            if (invoice.Status == InvoiceStatus.Exchange && invoice.InvoiceLineItems.Count(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount) > 1)
                                hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.EffectiveAmount <= 0 && x.InvoiceItemType == InvoiceItemType.Discount);
                            else
                                hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                            //End #84438 Exchange type discount issue

                            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                            var hasRecentlyAddedInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);
                            if (hasInvoiceItem == null || ((invoice.Status == InvoiceStatus.Exchange && hasRecentlyAddedInvoiceItem == null && hasInvoiceItem.EffectiveAmount >= 0)))
                            {
                                AddInvoiceItemLineApplyBuyXGetPercentOffer(invoice, productoffer, invoiceLineItem, lineItemTitle, finalDiscountValue, item);
                            }
                            else
                            {
                                UpdateInvoiceItemLineApplyBuyXGetPercentOffer(invoice, finalDiscount, item, hasInvoiceItem);
                            }
                            //END Ticket #74344 By pratik
                        }
                    }
                    else
                    {
                        if (discountedQty <= 0)
                        {
                            if (invoice.Status != InvoiceStatus.Exchange)
                            {
                                var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);

                                while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                                {
                                    // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
                                    var firstoffer = hasInvoiceItem.FirstOrDefault();
                                    var applyofferLineItem = toaddOffer.Where(x => (x.OfferId == null || x.OfferId == firstoffer.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount && x.IsExchangedProduct == false);
                                    applyofferLineItem?.ForEach(a => a.OfferDiscountPercent = null);
                                    //Ticket end: #65929 by Pratik

                                    //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                                    invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());

                                }
                            }
                        }
                    }
                }
            }
            //totalRetailPrice = 0;

            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;

                var retailPrice = item.RetailPrice;
                if (customerGroupId != null)
                {
                    var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                    retailPrice = item.RetailPrice - discount;
                    //Start ticket #75521 VAT and total amount calculated on the invoice is incorrect by pratik
                    totalRetailPrice = totalRetailPrice + (retailPrice * item.Quantity);
                    //End ticket #75521 by pratik
                }
            }
            //Exchange Issue ticket 7987, 7657
            var hasInvoiceItem1 = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
            foreach (var dvalue in hasInvoiceItem1)
            {
                if (dvalue.TotalAmount < 0)
                {
                    decimal discountedLineItem = 0;
                    if (finalDiscountedQuantity != 0)
                        discountedLineItem = dvalue.TotalAmount / finalDiscountedQuantity;
                    foreach (var item in toaddOffer)
                    {
                        if (item.IsOfferAdded)
                            continue;

                        if (item.TotalAmount > 0)
                        {
                            if (totalRetailPrice != 0)
                            {
                                var retailPrice = item.RetailPrice;
                                if (customerGroupId != null)
                                {
                                    var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                                    retailPrice = item.RetailPrice - discount;
                                }

                                var finalAmount = ((retailPrice * item.Quantity) * (dvalue.TotalAmount)) / totalRetailPrice;
                                var discountPrice = GetDiscountPercentfromValue((retailPrice * item.Quantity), ((retailPrice * item.Quantity) - finalAmount));
                                item.OfferDiscountPercent = Math.Abs(discountPrice);
                            }
                        }
                        //if (retailPrice < Math.Abs(discountedLineItem))
                        //{
                        //    item.OfferDiscountPercent = 100;
                        //    finalDiscountedQuantity = finalDiscountedQuantity - item.Quantity;
                        //    discountedLineItem = (dvalue.TotalAmount + (retailPrice * item.Quantity)) / finalDiscountedQuantity;
                        //}
                        //else
                        //{
                        //    var discountPrice = GetDiscountPercentfromValue((retailPrice), (retailPrice - discountedLineItem));
                        //    item.OfferDiscountPercent = Math.Abs(discountPrice);
                        //}
                        //}
                    }
                }
            }

            if (toaddOffer.Count() == 0)
            {
                var HasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                //var hasInvoiceItem = abpCommonHelpers.$filter("where")(Invoice.invoiceLineItems, { title: lineItemTitle, invoiceItemType: app.consts.invoiceItemType.discount });
                while (HasInvoiceItem != null && HasInvoiceItem.Count() > 0)
                {
                    //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                    invoice.InvoiceLineItems.Remove(HasInvoiceItem.FirstOrDefault());
                }
            }
            return invoice;
        }
        //Exchange Issue ticket 7987, 7657
        private static void AddInvoiceItemLineApplyBuyXGetPercentOffer(InvoiceDto invoice, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, string lineItemTitle, decimal finalDiscountValue, InvoiceLineItemDto item)
        {
            var invoiceItem = new InvoiceLineItemDto();

            if (invoiceLineItem != null && invoiceLineItem.TaxId != 0)
            {
                invoiceItem.TaxId = invoiceLineItem.TaxId;
                invoiceItem.TaxName = invoiceLineItem.TaxName;
                invoiceItem.TaxRate = invoiceLineItem.TaxRate;
            }
            else
            {
                invoiceItem.TaxId = 1;
                invoiceItem.TaxName = "NoTax";
                invoiceItem.TaxRate = 0;
            }

            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                invoiceItem.RegisterClosureId = (currentRegister.Registerclosure != null) ? currentRegister.Registerclosure.Id : 0;
            }
            invoiceItem.InvoiceItemValue = productoffer.OfferId ?? 0;
            invoiceItem.InvoiceItemValueParent = item.InvoiceItemValue;
            invoiceItem.InvoiceItemType = InvoiceItemType.Discount;
            invoiceItem.Sequence = invoice.InvoiceLineItems.Count() + 1;

            invoiceItem.Quantity = invoiceItem.Quantity + 1;
            invoiceItem.TotalAmount = -finalDiscountValue;  //- hasOffer.fixedPrice;
            invoiceItem.Title = lineItemTitle;
            invoiceItem.TotalDiscount = 0;

            invoiceItem.TaxAmount = 0;
            var discountedPrice = -finalDiscountValue;
            invoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            invoiceItem.EffectiveAmount = discountedPrice;
            invoiceItem.SoldPrice = discountedPrice;
            invoiceItem.RetailPrice = discountedPrice;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount - taxAmount;

            invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);


            //invoiceItem.class = 'sell-item-offer';
            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
             MainThread.BeginInvokeOnMainThread(()=>{
                if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    invoice.InvoiceLineItems.Add(invoiceItem);
                else
                    invoice.InvoiceLineItems.Insert(0, invoiceItem);
            });
            //End:#45375 .by rupesh
        }
        //Exchange Issue ticket 7987, 7657
        private static void UpdateInvoiceItemLineApplyBuyXGetPercentOffer(InvoiceDto invoice, decimal finalDiscount, InvoiceLineItemDto item, InvoiceLineItemDto hasInvoiceItem)
        {
            decimal discountedPrice = finalDiscount * (-1);
            hasInvoiceItem.TotalAmount = discountedPrice;
            hasInvoiceItem.TotalDiscount = 0;
            hasInvoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            hasInvoiceItem.TaxId = item.TaxId;
            hasInvoiceItem.TaxName = item.TaxName;
            hasInvoiceItem.TaxRate = item.TaxRate;

            decimal taxAmount = 0;
            //taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);
            taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);


            if (invoice.TaxInclusive)
                hasInvoiceItem.TaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount - taxAmount;

            //hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);
            hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

            hasInvoiceItem.EffectiveAmount = discountedPrice;
            hasInvoiceItem.SoldPrice = discountedPrice;
            hasInvoiceItem.RetailPrice = discountedPrice;
        }


        //Buy X Get Percent Off On Buy Y..
        static InvoiceDto ApplyBuyXGetPercentOffOnBuyY(InvoiceDto invoice, ObservableCollection<OfferDto> offers, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, ProductServices productServices, int? customerGroupId, bool calltotal = true)
        {
            ObservableCollection<InvoiceLineItemDto> toaddOffer = new ObservableCollection<InvoiceLineItemDto>();
            ObservableCollection<InvoiceLineItemDto> tobuyXoffer = new ObservableCollection<InvoiceLineItemDto>();
            ObservableCollection<InvoiceLineItemDto> tobuyYoffer = new ObservableCollection<InvoiceLineItemDto>();
            ObservableCollection<InvoiceLineItemDto> toDuplicateXoffer = new ObservableCollection<InvoiceLineItemDto>();

            var buyYofferItems = productoffer.OfferItems.Where(x => x.BuyAndGetType == 1);

            var offerOn = buyYofferItems.FirstOrDefault()?.OfferOn;
            var offerOnId = buyYofferItems.FirstOrDefault()?.OfferOnId;

            if (offerOn == OfferOn.Category)
            {
                var categoryId = offerOnId;
                foreach (var item in invoice.InvoiceLineItems)
                {
                    //Ticket start:#21684 Quantities sold for some categories not showing.by rupesh
                    if (item != null && item.CategoryDtos != null && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
                        var productCategories = JsonConvert.DeserializeObject<List<CategoryDto>>(item.CategoryDtos);
                        //Ticket end:#21684 .by rupesh
                        if (productCategories != null)
                        {
                            var isTypeAvailable = false;
                            var productTypes = productServices.GetLocalCategoriesSync();
                            foreach (var item2 in productTypes)
                            {
                                var typeAvailable = productCategories.Where(x => x.Id == item2.Id);
                                if (typeAvailable != null && typeAvailable.Count() > 0)
                                {
                                    if (item2.IsActive)
                                        isTypeAvailable = true;
                                }
                            }
                            if (isTypeAvailable)
                            {
                                var categoryofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Category && productCategories.FirstOrDefault(y => y.Id == x.OfferOnId || y.ParentId == x.OfferOnId) != null).ToList();

                                if (categoryofferitems != null && categoryofferitems.Count > 0)
                                {
                                    toaddOffer.Add(item);

                                    var existBuyXOffeItem = categoryofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
                                    if (existBuyXOffeItem != null)
                                        tobuyXoffer.Add(item);

                                    var existBuyYOffeItem = categoryofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
                                    if (existBuyYOffeItem != null)
                                        tobuyYoffer.Add(item);
                                }
                            }
                        }
                    }
                }
            }

            else if (offerOn == OfferOn.Tags)
            {
                var tagId = offerOnId;
                foreach (var item in invoice.InvoiceLineItems)
                {
                    if (item != null && !string.IsNullOrEmpty(item.Tags) && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
                        var productTags = JsonConvert.DeserializeObject<List<ProductTagDto>>(item.Tags);
                        if (productTags != null)
                        {
                            var tagofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Tags && productTags.FirstOrDefault(y => y.Id == x.OfferOnId.ToString()) != null).ToList();

                            if (tagofferitems != null && tagofferitems.Count > 0)
                            {
                                toaddOffer.Add(item);

                                var existBuyXOffeItem = tagofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
                                if (existBuyXOffeItem != null)
                                    tobuyXoffer.Add(item);

                                var existBuyYOffeItem = tagofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
                                if (existBuyYOffeItem != null)
                                    tobuyYoffer.Add(item);

                            }
                        }
                    }
                }
            }

            //Start #84438 iOS : FR :add discount offers on product tag by Pratik
            else if (offerOn == OfferOn.ProductTag)
            {
                var tagId = offerOnId;
                foreach (var item in invoice.InvoiceLineItems)
                {
                    if (item != null && !string.IsNullOrEmpty(item.Tags) && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
                        var productTags = item.Tags.TagsJsonToDto(productServices);
                        if (productTags != null)
                        {
                            var tagofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.ProductTag && productTags.FirstOrDefault(y => y.Id == x.OfferOnId.ToString()) != null).ToList();
                            if (tagofferitems != null && tagofferitems.Count > 0)
                            {
                                toaddOffer.Add(item);

                                var existBuyXOffeItem = tagofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
                                if (existBuyXOffeItem != null)
                                    tobuyXoffer.Add(item);

                                var existBuyYOffeItem = tagofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
                                if (existBuyYOffeItem != null)
                                    tobuyYoffer.Add(item);

                            }
                        }
                    }
                }
            }
            //End #84438 by Pratik

            else if (offerOn == OfferOn.Brand)
            {
                var brandId = offerOnId;
                foreach (var item in invoice.InvoiceLineItems)
                {
                    var brandofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Brand && x.OfferOnId.ToString() == item.Brand);
                    if (brandofferitems != null && brandofferitems.Count() > 0 && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
                        toaddOffer.Add(item);

                        var existBuyXOffeItem = brandofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
                        if (existBuyXOffeItem != null)
                            tobuyXoffer.Add(item);

                        var existBuyYOffeItem = brandofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
                        if (existBuyYOffeItem != null)
                            tobuyYoffer.Add(item);
                    }
                }
            }

            else if (offerOn == OfferOn.Season)
            {
                var seasonId = offerOnId;
                foreach (var item in invoice.InvoiceLineItems)
                {
                    var seasonofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Season && x.OfferOnId.ToString() == item.Season);
                    if (seasonofferitems != null && seasonofferitems.Count() > 0 && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
                        toaddOffer.Add(item);
                        var existBuyXOffeItem = seasonofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
                        if (existBuyXOffeItem != null)
                            tobuyXoffer.Add(item);

                        var existBuyYOffeItem = seasonofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
                        if (existBuyYOffeItem != null)
                            tobuyYoffer.Add(item);

                    }
                }
            }

            else if (offerOn == OfferOn.Product)
            {
                var productId = offerOnId;
                foreach (var item in invoice.InvoiceLineItems)
                {
                    if (item.InvoiceItemType == InvoiceItemType.Standard && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
                        var productofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Product && (x.OfferOnId == item.InvoiceItemValue || x.OfferOnId == item.InvoiceItemValueParent));
                        if (productofferitems != null && productofferitems.Count() > 0)
                        {
                            toaddOffer.Add(item);
                            var existBuyXOffeItem = productofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
                            if (existBuyXOffeItem != null)
                                tobuyXoffer.Add(item);

                            var existBuyYOffeItem = productofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
                            if (existBuyYOffeItem != null)
                                tobuyYoffer.Add(item);

                        }
                    }
                }
            }

            //Start Ticket #73624 iOS Discount: Buy X units of brand A and get Y units of brand B for free : Not working (All Discount with Exchange) By Pratik
            // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
            //Ticket start:#26866,#26599 Getting different total amount in cart in iPad.(No discount for UOM product).by rupesh
            toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            tobuyXoffer = new ObservableCollection<InvoiceLineItemDto>(tobuyXoffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            tobuyYoffer = new ObservableCollection<InvoiceLineItemDto>(tobuyYoffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end: #65929 by Pratik
            //End Ticket #73624 (All Discount with Exchange) By Pratik


            if (toaddOffer.Count == 0)
            {
                var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);

                while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                {
                    invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
                }
                return invoice;
            }

            //Exchange Issue ticket 7987, 7657
            var sumQty = tobuyXoffer.Where(x => x.Quantity > 0).Sum(x => x.Quantity);
            var sumBuyYQty = tobuyYoffer.Where(x => x.Quantity > 0).Sum(x => x.Quantity);

            ObservableCollection<InvoiceLineItemDto> toNotDuplicateXoffer = new ObservableCollection<InvoiceLineItemDto>();
            ObservableCollection<InvoiceLineItemDto> toNotDuplicateYoffer = new ObservableCollection<InvoiceLineItemDto>();

            foreach (var item in tobuyXoffer)
            {
                var isDuplicate = false;
                foreach (var itemY in tobuyYoffer)
                {
                    if (itemY.InvoiceItemType == item.InvoiceItemType && itemY.InvoiceItemValue == item.InvoiceItemValue && (itemY.InvoiceExtraItemValueParent == item.InvoiceExtraItemValueParent || itemY.InvoiceExtraItemValueParent == null))
                    {
                        isDuplicate = true;
                    }
                    else
                    {
                        var obj = toNotDuplicateYoffer.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && a.InvoiceItemValue == itemY.InvoiceItemValue && (a.InvoiceItemValueParent == itemY.InvoiceItemValueParent || a.InvoiceItemValueParent == null));
                        if (obj == null || obj.Count() == 0)
                        {
                            //toaddOffer.push(value);
                            toNotDuplicateYoffer.Add(itemY);
                        }
                        //toNotDuplicateYoffer.Add(itemY);
                    }
                }

                if (isDuplicate)
                    toDuplicateXoffer.Add(item);
                else
                    toNotDuplicateXoffer.Add(item);

            }
            //Exchange Issue ticket 7987, 7657
            var sumQty1 = toNotDuplicateXoffer.Where(x => x.Quantity > 0).Sum(x => x.Quantity);
            var sumQty2 = toNotDuplicateYoffer.Where(x => x.Quantity > 0).Sum(x => x.Quantity);

            foreach (var item in toDuplicateXoffer)
            {
                var isloop = true;
                var remainQty = item.Quantity;
                var xqty = 0;
                var yqty = 0;
                while (isloop)
                {

                    remainQty = remainQty - productoffer.BuyX.Value;
                    if (remainQty >= 0)
                    {
                        xqty++;
                    }
                    else
                        isloop = false;

                    if (isloop)
                    {
                        remainQty = remainQty - productoffer.GetX.Value;
                        if (remainQty >= 0)
                        {
                            yqty++;
                        }
                        else
                            isloop = false;
                    }

                }
                sumQty1 += (xqty * productoffer.BuyX.Value);
                sumQty2 += (yqty * productoffer.GetX.Value);
            }

            decimal discountedQty = 0;
            if (invoice.Status == InvoiceStatus.Refunded)
            {
                discountedQty = GetdiscountqtyfromBuyxToUptoGetY(productoffer.BuyX, productoffer.GetX, Math.Abs(sumQty1), Math.Abs(sumQty2));
            }
            else
            {
                discountedQty = GetdiscountqtyfromBuyxToUptoGetY(productoffer.BuyX, productoffer.GetX, sumQty1, sumQty2);
            }

            //var discountedQty = getdiscountqtyfromBuyxToGetY(productoffer.BuyX, productoffer.GetX, sumQty1, sumQty2);

            tobuyYoffer = new ObservableCollection<InvoiceLineItemDto>(tobuyYoffer.OrderBy(i => i.RetailPrice));

            var lineItemTitle = "";
            if (productoffer.Description == null)
                lineItemTitle = "Discount" + "/ " + productoffer.Name;
            else
                lineItemTitle = "Discount" + "/ " + productoffer.Description;

            decimal finalDiscountedQuantity = 0;
            decimal finalDiscountedQuantityAmount = 0;
            decimal totalRetailPrice = 0;
            foreach (var item in tobuyYoffer)
            {
                // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
                CalculateLineItemTotal(item, invoice,calltotal);
                //Ticket end: #65929 by Pratik

                if (item.EffectiveAmount > 0)
                {
                    finalDiscountedQuantity = finalDiscountedQuantity + item.Quantity;
                    finalDiscountedQuantityAmount = finalDiscountedQuantityAmount + item.RetailPrice;
                    totalRetailPrice = totalRetailPrice + item.RetailPrice;
                }
            }

            decimal finalDiscountValue = 0;
            decimal finalDiscount = 0;
            decimal remainingQty = ((productoffer.GetX ?? 0) * discountedQty);
            if (discountedQty > 0)
            {
                foreach (var item in tobuyYoffer)
                {
                    if ((item.Quantity - item.BackOrderQty ?? 0) <= item.Quantity)
                    {
                        if (remainingQty > 0)
                        {
                            decimal toRemoveQty = 0;
                            if (Math.Abs(item.Quantity) >= remainingQty)
                                toRemoveQty = remainingQty;
                            else if (Math.Abs(item.Quantity) < remainingQty)
                                toRemoveQty = Math.Abs(item.Quantity);

                            //customer group discount..
                            if (customerGroupId != null)
                            {
                                var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                                item.SoldPrice = item.RetailPrice - discount;
                                totalRetailPrice = 0;
                            }
                            else if (customerGroupId == null && item.DiscountValue == 0)
                            {
                                item.SoldPrice = item.RetailPrice;
                            }
                            else if (customerGroupId == null)
                            {
                                item.SoldPrice = item.RetailPrice;
                            }
                            //customer group discount..

                            //line item Discount..
                            if (item.DiscountValue > 0)
                            {
                                var discountedValue = GetValuefromPercent(item.SoldPrice, item.DiscountValue);
                                item.SoldPrice = item.SoldPrice - discountedValue;
                            }
                            //line item Discount..
                            item.OfferDiscountPercent = productoffer.OfferValue;

                            var fromPrice = item.SoldPrice * toRemoveQty;
                            finalDiscountValue = GetValuefromPercent(fromPrice, productoffer.OfferValue);
                            finalDiscount += finalDiscountValue;
                            remainingQty = remainingQty - toRemoveQty;

                            var productId = item.InvoiceItemValue;
                            if (item.DiscountIsAsPercentage)
                            {
                                InvoiceLineItemDto hasInvoiceItem = null;
                                //Start #84438 Exchange type discount issue 
                                //hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                                if (invoice.Status == InvoiceStatus.Exchange && invoice.InvoiceLineItems.Count(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount) > 1)
                                    hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.EffectiveAmount <= 0 && x.InvoiceItemType == InvoiceItemType.Discount);
                                else
                                    hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                                //End #84438 Exchange type discount issue
                                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                                var hasRecentlyAddedInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);
                                if (hasInvoiceItem == null || ((invoice.Status == InvoiceStatus.Exchange && hasRecentlyAddedInvoiceItem == null && hasInvoiceItem.TaxAmount >= 0 && hasInvoiceItem.EffectiveAmount >= 0)))
                                {
                                    AddInvoiceItemLineApplyBuyXGetPercentOffOnBuyY(invoice, productoffer, invoiceLineItem, lineItemTitle, finalDiscountValue, item);
                                }
                                else
                                {
                                    UpdateInvoiceItemLineApplyBuyXGetPercentOffOnBuyY(invoice, productoffer, discountedQty, finalDiscount, toRemoveQty, hasInvoiceItem, item);
                                }
                                //End Ticket #74344 By pratik
                            }
                        }
                        else
                        {
                            if (discountedQty <= 0)
                            {
                                if (invoice.Status != InvoiceStatus.Exchange)
                                {
                                    var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);

                                    while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                                    {
                                        // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
                                        var firstoffer = hasInvoiceItem.FirstOrDefault();
                                        var applyofferLineItem = tobuyYoffer.Where(x => (x.OfferId == null || x.OfferId == firstoffer.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount && x.IsExchangedProduct == false);
                                        applyofferLineItem?.ForEach(a => a.OfferDiscountPercent = null);
                                        //Ticket end: #65929 by Pratik

                                        //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                                        invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var item in tobuyYoffer)
                {
                    var retailPrice = item.RetailPrice;
                    if (customerGroupId != null)
                    {
                        var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                        retailPrice = item.RetailPrice - discount;
                        totalRetailPrice = totalRetailPrice + retailPrice;
                    }

                }
                //Exchange Issue ticket 7987, 7657
                var hasInvoiceItem1 = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);

                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                decimal totalTaxPrice = 0;
                foreach (var dvalue in hasInvoiceItem1)
                {
                    var discountedLineItem = dvalue.TotalAmount / discountedQty;
                    foreach (var value in tobuyYoffer)
                    {
                        var invoiceItem_taxExclusiveTotalAmount = discountedLineItem;

                        decimal invoiceItem_taxAmount = 0;
                        decimal taxAmount = 0;
                        taxAmount = CalculateTaxInclusive(invoiceItem_taxExclusiveTotalAmount, value.TaxRate);

                        if (invoice.TaxInclusive)
                            invoiceItem_taxExclusiveTotalAmount = invoiceItem_taxExclusiveTotalAmount - taxAmount;
                        invoiceItem_taxAmount = GetValuefromPercent(invoiceItem_taxExclusiveTotalAmount, value.TaxRate);
                        totalTaxPrice += Math.Abs(invoiceItem_taxAmount);
                        value.taxAmountNew = Math.Abs(invoiceItem_taxAmount);
                    }
                }
                //End Ticket #74344 By pratik
                //Ticket start:#83531 Tax amount mismatch in sales history and receipt. by rupesh
                tobuyYoffer.ForEach(x => x.DiscountOfferTax = 0);
                //Ticket end:#83531.by rupesh
                foreach (var dvalue in hasInvoiceItem1)
                {
                    if (dvalue.TotalAmount < 0)
                    {
                        decimal discountedLineItem = 0;
                        if (finalDiscountedQuantity != 0)
                            discountedLineItem = dvalue.TotalAmount / finalDiscountedQuantity;
                        foreach (var item in tobuyYoffer)
                        {
                            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                            if (totalTaxPrice > 0)
                                item.DiscountOfferTax = (item.taxAmountNew * dvalue.TaxAmount) / totalTaxPrice;
                            else
                                item.DiscountOfferTax = 0;
                            //End Ticket #74344 By pratik
                            if (item.TotalAmount > 0)
                            {
                                var retailPrice = item.RetailPrice;
                                if (customerGroupId != null)
                                {
                                    var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                                    retailPrice = item.RetailPrice - discount;
                                }


                                var finalAmount = (retailPrice * dvalue.TotalAmount) / totalRetailPrice;
                                var discountPrice = GetDiscountPercentfromValue((retailPrice * item.Quantity), ((retailPrice * item.Quantity) - finalAmount));
                                item.OfferDiscountPercent = Math.Abs(discountPrice);

                                //if (retailPrice < Math.Abs(discountedLineItem))
                                //{
                                //    item.OfferDiscountPercent = 100;
                                //    finalDiscountedQuantity = finalDiscountedQuantity - item.Quantity;
                                //    discountedLineItem = (dvalue.TotalAmount + retailPrice) / finalDiscountedQuantity;
                                //}
                                //else
                                //{
                                //    var discountPrice = GetDiscountPercentfromValue((retailPrice), (retailPrice - discountedLineItem));
                                //    item.OfferDiscountPercent = Math.Abs(discountPrice);
                                //}
                            }
                        }
                    }
                }
            }
            else
            {
                if (discountedQty <= 0)
                {
                    //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                    var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount && !x.IsExchangedProduct);
                    while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                    {
                        // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
                        var firstoffer = hasInvoiceItem.FirstOrDefault();
                        var applyofferLineItem = tobuyYoffer.Where(x => (x.OfferId == null || x.OfferId == firstoffer.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount && x.IsExchangedProduct == false);
                        applyofferLineItem?.ForEach(a => a.OfferDiscountPercent = null);
                        //Ticket end: #65929 by Pratik
                        //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                        invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
                    }
                    //End Ticket #74344 By pratik
                }
            }
            return invoice;
        }
        //Exchange Issue ticket 7987, 7657
        private static void AddInvoiceItemLineApplyBuyXGetPercentOffOnBuyY(InvoiceDto invoice, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, string lineItemTitle, decimal finalDiscountValue, InvoiceLineItemDto item)
        {
            var invoiceItem = new InvoiceLineItemDto();

            if (invoiceLineItem != null && invoiceLineItem.TaxId != 0)
            {
                invoiceItem.TaxId = invoiceLineItem.TaxId;
                invoiceItem.TaxName = invoiceLineItem.TaxName;
                invoiceItem.TaxRate = invoiceLineItem.TaxRate;
            }
            else
            {
                invoiceItem.TaxId = 1;
                invoiceItem.TaxName = "NoTax";
                invoiceItem.TaxRate = 0;
            }

            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                invoiceItem.RegisterClosureId = (currentRegister.Registerclosure != null) ? currentRegister.Registerclosure.Id : 0;
            }
            invoiceItem.InvoiceItemValue = productoffer.OfferId ?? 0;
            //invoiceItem.InvoiceItemValueParent = item.InvoiceItemValue;

            if (item.InvoiceItemValueParent != null)
                invoiceItem.InvoiceItemValueParent = item.InvoiceItemValueParent;
            else
                invoiceItem.InvoiceItemValueParent = item.InvoiceItemValue;

            invoiceItem.InvoiceItemType = InvoiceItemType.Discount;
            invoiceItem.Sequence = invoice.InvoiceLineItems.Count() + 1;

            invoiceItem.Quantity = invoiceItem.Quantity + 1;
            invoiceItem.OfferId = 0;
            invoiceItem.TotalAmount = -finalDiscountValue;  //- hasOffer.fixedPrice;
            invoiceItem.Title = lineItemTitle;
            invoiceItem.TotalDiscount = 0;

            invoiceItem.TaxAmount = 0;
            var discountedPrice = -finalDiscountValue;
            invoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            invoiceItem.EffectiveAmount = discountedPrice;
            invoiceItem.SoldPrice = discountedPrice;
            invoiceItem.RetailPrice = discountedPrice;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount - taxAmount;

            invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);


            //invoiceItem.class = 'sell-item-offer';
            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
              MainThread.BeginInvokeOnMainThread(()=>{
                if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    invoice.InvoiceLineItems.Add(invoiceItem);
                else
                    invoice.InvoiceLineItems.Insert(0, invoiceItem);
            });
            //End:#45375 .by rupesh
        }

        private static void UpdateInvoiceItemLineApplyBuyXGetPercentOffOnBuyY(InvoiceDto invoice, OfferDto productoffer, decimal discountedQty, decimal finalDiscount, decimal toRemoveQty, InvoiceLineItemDto hasInvoiceItem, InvoiceLineItemDto item)
        {
            if ((productoffer.GetX * discountedQty) >= toRemoveQty)
            {
                //finalDiscount += finalDiscountValue;
                decimal discountedPrice = 0;
                if (invoice.Status == InvoiceStatus.Refunded)
                {
                    discountedPrice = finalDiscount;
                }
                else
                {
                    discountedPrice = -finalDiscount;
                }

                hasInvoiceItem.Quantity = 1;
                hasInvoiceItem.TotalAmount = discountedPrice;
                hasInvoiceItem.TotalDiscount = 0;
                hasInvoiceItem.TaxExclusiveTotalAmount = discountedPrice;
                hasInvoiceItem.Id = item.Id;
                hasInvoiceItem.TaxRate = item.TaxRate;
                hasInvoiceItem.TaxName = item.TaxName;
                //START Ticket #74344 iOS and WEB :: Discount Issue : By pratik
                hasInvoiceItem.TaxId = item.TaxId;
                //end Ticket #74344 By pratik
                decimal taxAmount = 0;
                taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

                if (invoice.TaxInclusive)
                    hasInvoiceItem.TaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount - taxAmount;

                hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

                hasInvoiceItem.EffectiveAmount = discountedPrice;
                hasInvoiceItem.SoldPrice = discountedPrice;
                hasInvoiceItem.RetailPrice = discountedPrice;
            }
        }


        //Buy X Get Y Free..
        static InvoiceDto ApplyBuyXGetYFreeOffer(InvoiceDto invoice, ObservableCollection<OfferDto> offers, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, ProductServices productServices, int? customerGroupId)
		{
			ObservableCollection<InvoiceLineItemDto> toaddOffer = new ObservableCollection<InvoiceLineItemDto>();
			ObservableCollection<InvoiceLineItemDto> tobuyXoffer = new ObservableCollection<InvoiceLineItemDto>();
			ObservableCollection<InvoiceLineItemDto> tobuyYoffer = new ObservableCollection<InvoiceLineItemDto>();
			ObservableCollection<InvoiceLineItemDto> toDuplicateXoffer = new ObservableCollection<InvoiceLineItemDto>();

			var buyYofferItems = productoffer.OfferItems.Where(x => x.BuyAndGetType == 1);

			var offerOn = buyYofferItems.FirstOrDefault()?.OfferOn;
			var offerOnId = buyYofferItems.FirstOrDefault()?.OfferOnId;

			if (offerOn == OfferOn.Category)
			{
				var categoryId = offerOnId;
				foreach (var item in invoice.InvoiceLineItems)
				{
					//Ticket start:#21684 Quantities sold for some categories not showing.by rupesh
					if (item != null && item.CategoryDtos != null && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
						var productCategories = JsonConvert.DeserializeObject<List<CategoryDto>>(item.CategoryDtos);
						//Ticket end:#21684 .by rupesh
						if (productCategories != null)
						{
							var isTypeAvailable = false;
							var productTypes = productServices.GetLocalCategoriesSync();
							foreach (var item2 in productTypes)
							{
								var typeAvailable = productCategories.Where(x => x.Id == item2.Id);
								if (typeAvailable != null && typeAvailable.Count() > 0)
								{
									if (item2.IsActive)
										isTypeAvailable = true;
								}
							}
							if (isTypeAvailable)
							{
								var categoryofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Category && productCategories.FirstOrDefault(y => y.Id == x.OfferOnId || y.ParentId == x.OfferOnId) != null).ToList();

								if (categoryofferitems != null && categoryofferitems.Count > 0)
								{
									toaddOffer.Add(item);

									var existBuyXOffeItem = categoryofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
									if (existBuyXOffeItem != null)
										tobuyXoffer.Add(item);

									var existBuyYOffeItem = categoryofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
									if (existBuyYOffeItem != null)
										tobuyYoffer.Add(item);
								}
							}
						}
					}
				}
			}

			else if (offerOn == OfferOn.Tags)
			{
				var tagId = offerOnId;
				foreach (var item in invoice.InvoiceLineItems)
				{
					if (item != null && !string.IsNullOrEmpty(item.Tags) && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
						var productTags = JsonConvert.DeserializeObject<List<ProductTagDto>>(item.Tags);
						if (productTags != null)
						{
							var tagofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Tags && productTags.FirstOrDefault(y => y.Id == x.OfferOnId.ToString()) != null).ToList();

							if (tagofferitems != null && tagofferitems.Count > 0)
							{
								toaddOffer.Add(item);

								var existBuyXOffeItem = tagofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
								if (existBuyXOffeItem != null)
									tobuyXoffer.Add(item);

								var existBuyYOffeItem = tagofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
								if (existBuyYOffeItem != null)
									tobuyYoffer.Add(item);

							}
						}
					}
				}
			}

            //Start #84438 iOS : FR :add discount offers on product tag by Pratik
            else if (offerOn == OfferOn.ProductTag)
            {
                var tagId = offerOnId;
                foreach (var item in invoice.InvoiceLineItems)
                {
                    if (item != null && !string.IsNullOrEmpty(item.Tags) && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
                        var productTags = item.Tags.TagsJsonToDto(productServices);
                        if (productTags != null)
                        {
                            var tagofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.ProductTag && productTags.FirstOrDefault(y => y.Id == x.OfferOnId.ToString()) != null).ToList();

                            if (tagofferitems != null && tagofferitems.Count > 0)
                            {
                                toaddOffer.Add(item);

                                var existBuyXOffeItem = tagofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
                                if (existBuyXOffeItem != null)
                                    tobuyXoffer.Add(item);

                                var existBuyYOffeItem = tagofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
                                if (existBuyYOffeItem != null)
                                    tobuyYoffer.Add(item);

                            }
                        }
                    }
                }
            }
            //End #84438 by Pratik

            else if (offerOn == OfferOn.Brand)
			{
				var brandId = offerOnId;
				foreach (var item in invoice.InvoiceLineItems)
				{
					var brandofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Brand && x.OfferOnId.ToString() == item.Brand);
					if (brandofferitems != null && brandofferitems.Count() > 0 && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
						toaddOffer.Add(item);

						var existBuyXOffeItem = brandofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
						if (existBuyXOffeItem != null)
							tobuyXoffer.Add(item);

						var existBuyYOffeItem = brandofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
						if (existBuyYOffeItem != null)
							tobuyYoffer.Add(item);
					}
				}
			}

			else if (offerOn == OfferOn.Season)
			{
				var seasonId = offerOnId;
				foreach (var item in invoice.InvoiceLineItems)
				{
					var seasonofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Season && x.OfferOnId.ToString() == item.Season);
					if (seasonofferitems != null && seasonofferitems.Count() > 0 && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
						toaddOffer.Add(item);
						var existBuyXOffeItem = seasonofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
						if (existBuyXOffeItem != null)
							tobuyXoffer.Add(item);

						var existBuyYOffeItem = seasonofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
						if (existBuyYOffeItem != null)
							tobuyYoffer.Add(item);

					}
				}
			}

			else if (offerOn == OfferOn.Product)
			{
				var productId = offerOnId;
				foreach (var item in invoice.InvoiceLineItems)
				{
					if (item.InvoiceItemType == InvoiceItemType.Standard && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                    {
						var productofferitems = productoffer.OfferItems.Where(x => x.OfferOn == OfferOn.Product && (x.OfferOnId == item.InvoiceItemValue || x.OfferOnId == item.InvoiceItemValueParent));
						if (productofferitems != null && productofferitems.Count() > 0)
						{
							toaddOffer.Add(item);
							var existBuyXOffeItem = productofferitems.FirstOrDefault(x => x.BuyAndGetType == 0);
							if (existBuyXOffeItem != null)
								tobuyXoffer.Add(item);

							var existBuyYOffeItem = productofferitems.FirstOrDefault(x => x.BuyAndGetType == 1);
							if (existBuyYOffeItem != null)
								tobuyYoffer.Add(item);

						}
					}
				}
			}

			//Ticket start:#26866,#26599 Getting different total amount in cart in iPad.(No discount for UOM product).by rupesh
			toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
			tobuyXoffer = new ObservableCollection<InvoiceLineItemDto>(tobuyXoffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
			tobuyYoffer = new ObservableCollection<InvoiceLineItemDto>(tobuyYoffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
			//Ticket end:#26866,#26599 .by rupesh

			if (toaddOffer.Count == 0)
			{
				var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);

				while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
				{
					invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
				}
				return invoice;
			}
			//Exchange Issue ticket 7987, 7657
			var sumQty = tobuyXoffer.Where(x => x.Quantity > 0).Sum(x => x.Quantity);
			var sumBuyYQty = tobuyYoffer.Where(x => x.Quantity > 0).Sum(x => x.Quantity);

			ObservableCollection<InvoiceLineItemDto> toNotDuplicateXoffer = new ObservableCollection<InvoiceLineItemDto>();
			ObservableCollection<InvoiceLineItemDto> toNotDuplicateYoffer = new ObservableCollection<InvoiceLineItemDto>();

			foreach (var item in tobuyXoffer)
			{
				var isDuplicate = false;
				foreach (var itemY in tobuyYoffer)
				{
					if (itemY.InvoiceItemType == item.InvoiceItemType && itemY.InvoiceItemValue == item.InvoiceItemValue && itemY.InvoiceExtraItemValueParent == item.InvoiceExtraItemValueParent)
					{
						isDuplicate = true;
					}
				}

				if (isDuplicate)
					toDuplicateXoffer.Add(item);
				else
					toNotDuplicateXoffer.Add(item);

			}

            foreach (var item in tobuyYoffer)
			{
				var isDuplicate = false;
				foreach (var itemY in tobuyXoffer)
				{
					if (itemY.InvoiceItemType == item.InvoiceItemType && itemY.InvoiceItemValue == item.InvoiceItemValue && itemY.InvoiceExtraItemValueParent == item.InvoiceExtraItemValueParent)
					{
						isDuplicate = true;
					}
				}

				if (!isDuplicate)
					toNotDuplicateYoffer.Add(item);
			}
            
            
			//Exchange Issue ticket 7987, 7657
            var sumQty1 = toNotDuplicateXoffer.Where(x => x.Quantity > 0).Sum(x => x.Quantity);
			var sumQty2 = toNotDuplicateYoffer.Where(x => x.Quantity > 0).Sum(x => x.Quantity);

			foreach (var item in toDuplicateXoffer)
			{
				var isloop = true;
				var remainQty = item.Quantity;
				var xqty = 0;
				var yqty = 0;
				while (isloop)
				{
					remainQty = remainQty - productoffer.BuyX.Value;
					if (remainQty >= 0)
					{
						xqty++;
					}
					else
						isloop = false;

					if (isloop)
					{
						remainQty = remainQty - productoffer.GetX.Value;
						if (remainQty >= 0)
						{
							yqty++;
						}
						else
							isloop = false;
					}

				}
				sumQty1 += (xqty * productoffer.BuyX.Value);
				sumQty2 += (yqty * productoffer.GetX.Value);
			}

			decimal discountedQty = 0;
			if (invoice.Status == InvoiceStatus.Refunded)
			{
				discountedQty = GetdiscountqtyfromBuyxToGetY(productoffer.BuyX, productoffer.GetX, Math.Abs(sumQty1), Math.Abs(sumQty2));
			}
			else
			{
				discountedQty = GetdiscountqtyfromBuyxToGetY(productoffer.BuyX, productoffer.GetX, sumQty1, sumQty2);
			}
			tobuyYoffer = new ObservableCollection<InvoiceLineItemDto>(tobuyYoffer.OrderBy(i => i.RetailPrice));

			var lineItemTitle = "";
			if (productoffer.Description == null)
				lineItemTitle = "Discount" + "/ " + productoffer.Name;
			else
				lineItemTitle = "Discount" + "/ " + productoffer.Description;

			decimal finalDiscountedQuantity = 0;
			decimal totalRetailPrice = 0;
			foreach (var item in tobuyYoffer)
			{
				finalDiscountedQuantity = finalDiscountedQuantity + item.Quantity;
                totalRetailPrice = totalRetailPrice + item.RetailPrice;
            }

            decimal finalDiscountValue = 0;
			decimal finalDiscount = 0;
			decimal remainingQty = ((productoffer.GetX ?? 0) * discountedQty);
			if (discountedQty > 0)
			{
				foreach (var item in tobuyYoffer)
				{
					if ((item.Quantity - item.BackOrderQty ?? 0) <= item.Quantity)
					{
						if (remainingQty > 0)
						{
							decimal toRemoveQty = 0;
							if (Math.Abs(item.Quantity) >= remainingQty)
								toRemoveQty = remainingQty;
							else if (Math.Abs(item.Quantity) < remainingQty)
								toRemoveQty = Math.Abs(item.Quantity);

							//customer group discount..
							if (customerGroupId != null)
							{
								var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
								item.SoldPrice = item.RetailPrice - discount;
								totalRetailPrice = 0;
							}
							else if (customerGroupId == null && item.DiscountValue == 0)
							{
								item.SoldPrice = item.RetailPrice;
							}
							else if (customerGroupId == null)
							{
								item.SoldPrice = item.RetailPrice;
							}
							//customer group discount..

							//line item Discount..
							if (item.DiscountValue > 0)
							{
								var discountedValue = GetValuefromPercent(item.SoldPrice, item.DiscountValue);
								item.SoldPrice = item.SoldPrice - discountedValue;
							}
							//line item Discount..

							item.OfferDiscountPercent = 100;

							var fromPrice = item.SoldPrice * toRemoveQty;

							finalDiscountValue = GetValuefromPercent(fromPrice, 100);
							finalDiscount += finalDiscountValue;
							remainingQty = remainingQty - toRemoveQty;

							var productId = item.InvoiceItemValue;
							if (item.DiscountIsAsPercentage)
							{
								//Exchange Issue ticket 7987, 7657
								InvoiceLineItemDto hasInvoiceItem = null;
                                //Start #84438 Exchange type discount issue 
                                //hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                                if (invoice.Status == InvoiceStatus.Exchange && invoice.InvoiceLineItems.Count(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount) > 1)
                                    hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.EffectiveAmount <= 0 && x.InvoiceItemType == InvoiceItemType.Discount);
                                else
                                    hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                                //End #84438 Exchange type discount issue
                                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                                var hasRecentlyAddedInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);
                                if (hasInvoiceItem == null || ((invoice.Status == InvoiceStatus.Exchange && hasRecentlyAddedInvoiceItem == null && hasInvoiceItem.TaxAmount >= 0 && hasInvoiceItem.EffectiveAmount >= 0)))
                                {
                                    AddInvoiceItemLineApplyBuyXGetYFreeOffer(invoice, productoffer, invoiceLineItem, lineItemTitle, finalDiscountValue, item);
                                }
                                else
                                {
                                    UpdateInvoiceItemLineApplyBuyXGetYFreeOffer(invoice, productoffer, discountedQty, finalDiscount, item, toRemoveQty, hasInvoiceItem);
                                }
                                //End Ticket #74344 By pratik
                            }
                        }
						else
						{
							if (discountedQty <= 0)
							{
								if (invoice.Status != InvoiceStatus.Exchange)
								{
									var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);

									while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
									{
										//var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
										invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
									}
								}
							}
						}
					}
				}

				// totalRetailPrice = 0;
				foreach (var item in tobuyYoffer)
				{
					if (item.TotalAmount > 0)
					{
						var retailPrice = item.RetailPrice;
						if (customerGroupId != null)
						{
							var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
							retailPrice = item.RetailPrice - discount;
                            totalRetailPrice = totalRetailPrice + retailPrice;
                        }

                    }
				}
				//Exchange Issue ticket 7987, 7657
				var hasInvoiceItem1 = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);

                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                decimal totalTaxPrice = 0;
                foreach (var dvalue in hasInvoiceItem1)
                {
                    var discountedLineItem = dvalue.TotalAmount / discountedQty;
                    foreach (var value in tobuyYoffer)
                    {
                        var invoiceItem_taxExclusiveTotalAmount = discountedLineItem;

                        decimal invoiceItem_taxAmount = 0;
                        decimal taxAmount = 0;
                        taxAmount = CalculateTaxInclusive(invoiceItem_taxExclusiveTotalAmount, value.TaxRate);

                        if (invoice.TaxInclusive)
                            invoiceItem_taxExclusiveTotalAmount = invoiceItem_taxExclusiveTotalAmount - taxAmount;
                        invoiceItem_taxAmount = GetValuefromPercent(invoiceItem_taxExclusiveTotalAmount, value.TaxRate);
                        totalTaxPrice += Math.Abs(invoiceItem_taxAmount);
                        value.taxAmountNew = Math.Abs(invoiceItem_taxAmount);
                    }
                }
                //End Ticket #74344 By pratik
                //Ticket start:#83531 Tax amount mismatch in sales history and receipt. by rupesh
                tobuyYoffer.ForEach(x => x.DiscountOfferTax = 0);
                //Ticket end:#83531.by rupesh
                foreach (var dvalue in hasInvoiceItem1)
				{
					if (dvalue.TotalAmount < 0)
					{
						decimal discountedLineItem = 0;
						if (finalDiscountedQuantity != 0)
							discountedLineItem = dvalue.TotalAmount / finalDiscountedQuantity;
						foreach (var item in tobuyYoffer)
						{
                            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
							if(totalTaxPrice > 0)
								item.DiscountOfferTax = (item.taxAmountNew * dvalue.TaxAmount) / totalTaxPrice;
                            else
                                item.DiscountOfferTax = 0;
                            //End Ticket #74344 By pratik
                            if (item.TotalAmount > 0)
							{
								var retailPrice = item.RetailPrice;
								if (customerGroupId != null)
								{
									var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
									retailPrice = item.RetailPrice - discount;
								}

                                var finalAmount = (retailPrice * dvalue.TotalAmount) / totalRetailPrice;

                                var discountPrice = GetDiscountPercentfromValue((retailPrice * item.Quantity), ((retailPrice * item.Quantity) - finalAmount));
								item.OfferDiscountPercent = Math.Abs(discountPrice);

								//if (retailPrice < Math.Abs(discountedLineItem))
								//{
								//    item.OfferDiscountPercent = 100;
								//    finalDiscountedQuantity = finalDiscountedQuantity - item.Quantity;
								//    discountedLineItem = (dvalue.TotalAmount + retailPrice) / finalDiscountedQuantity;
								//}
								//else
								//{
								//    var discountPrice = GetDiscountPercentfromValue((retailPrice), (retailPrice - discountedLineItem));
								//    item.OfferDiscountPercent = Math.Abs(discountPrice);
								//}
							}
						}
					}
				}

			}
			else
			{
				if (discountedQty <= 0)
				{
					if (invoice.Status != InvoiceStatus.Exchange)
					{
						var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);

						while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
						{
							//var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
							invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
						}
					}
				}
			}
			return invoice;
		}
		
        //Exchange Issue ticket 7987, 7657
        private static void AddInvoiceItemLineApplyBuyXGetYFreeOffer(InvoiceDto invoice, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, string lineItemTitle, decimal finalDiscountValue, InvoiceLineItemDto item)
        {
            var invoiceItem = new InvoiceLineItemDto();

            if (invoiceLineItem != null && invoiceLineItem.TaxId != 0)
            {
                invoiceItem.TaxId = invoiceLineItem.TaxId;
                invoiceItem.TaxName = invoiceLineItem.TaxName;
                invoiceItem.TaxRate = invoiceLineItem.TaxRate;
            }
            else
            {
                invoiceItem.TaxId = 1;
                invoiceItem.TaxName = "NoTax";
                invoiceItem.TaxRate = 0;
            }

            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                invoiceItem.RegisterClosureId = (currentRegister.Registerclosure != null) ? currentRegister.Registerclosure.Id : 0;
            }
            invoiceItem.InvoiceItemValue = productoffer.OfferId ?? 0;

            if (item.InvoiceItemValueParent != null)
                invoiceItem.InvoiceItemValueParent = item.InvoiceItemValueParent;
            else
                invoiceItem.InvoiceItemValueParent = item.InvoiceItemValue;

            invoiceItem.InvoiceItemType = InvoiceItemType.Discount;
            invoiceItem.Sequence = invoice.InvoiceLineItems.Count() + 1;

            invoiceItem.Quantity = invoiceItem.Quantity + 1;
            invoiceItem.OfferId = 0;
            invoiceItem.TotalAmount = -finalDiscountValue;  //- hasOffer.fixedPrice;

            invoiceItem.Title = lineItemTitle;
            invoiceItem.TotalDiscount = 0;

            invoiceItem.TaxAmount = 0;
            var discountedPrice = -finalDiscountValue;
            invoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            invoiceItem.EffectiveAmount = discountedPrice;
            invoiceItem.SoldPrice = discountedPrice;
            invoiceItem.RetailPrice = discountedPrice;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount - taxAmount;

            invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            //invoiceItem.class = 'sell-item-offer';
            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
              MainThread.BeginInvokeOnMainThread(()=>{
                if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    invoice.InvoiceLineItems.Add(invoiceItem);
                else
                    invoice.InvoiceLineItems.Insert(0, invoiceItem);
            });
            //End:#45375 .by rupesh
        }
        //Exchange Issue ticket 7987, 7657
        private static void UpdateInvoiceItemLineApplyBuyXGetYFreeOffer(InvoiceDto invoice, OfferDto productoffer, decimal discountedQty, decimal finalDiscount, InvoiceLineItemDto item, decimal toRemoveQty, InvoiceLineItemDto hasInvoiceItem)
        {
            if ((productoffer.GetX * discountedQty) >= toRemoveQty)
            {
                //finalDiscount += finalDiscountValue;
                decimal discountedPrice = 0;
                if (invoice.Status == InvoiceStatus.Refunded)
                {
                    discountedPrice = finalDiscount;
                }
                else
                {
                    discountedPrice = -finalDiscount;
                }
                hasInvoiceItem.Quantity = 1;
                hasInvoiceItem.TotalAmount = discountedPrice;
                hasInvoiceItem.TotalDiscount = 0;
                hasInvoiceItem.TaxExclusiveTotalAmount = discountedPrice;
                hasInvoiceItem.TaxId = item.TaxId;
                hasInvoiceItem.TaxName = item.TaxName;
                hasInvoiceItem.TaxRate = item.TaxRate;

                decimal taxAmount = 0;
                taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

                if (invoice.TaxInclusive)
                    hasInvoiceItem.TaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount - taxAmount;

                hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

                hasInvoiceItem.EffectiveAmount = discountedPrice;
                hasInvoiceItem.SoldPrice = discountedPrice;
                hasInvoiceItem.RetailPrice = discountedPrice;
            }
        }

        static decimal GetdiscountqtyfromBuyxToGetY(decimal? buyx, decimal? getx, decimal actualQty, decimal getYQty)
        {
            if (getYQty < getx)
                return 0;

            if (buyx <= actualQty)
            {
                var loopLength = actualQty / buyx;
                var remainGetQty = getYQty;
                var returnGetYQty = 0;
                for (var i = 1; i <= loopLength; i++)
                {
                    remainGetQty = remainGetQty - getx ?? 0;
                    if (remainGetQty >= 0)
                        returnGetYQty++;

                }

                return returnGetYQty;

            }
            return 0;
        }

        static decimal GetdiscountqtyfromBuyxToUptoGetY(decimal? buyx, decimal? getx, decimal actualQty, decimal getYQty)
        {
            if (buyx <= actualQty)
            {
                var loopLength = actualQty / buyx;
                var remainGetQty = getYQty;
                var returnGetYQty = 0;
                for (var i = 1; i <= loopLength.Value; i++)
                {
                    if (remainGetQty <= getx)
                        remainGetQty = getx.Value - remainGetQty;
                    else if (remainGetQty > getx)
                        remainGetQty = getx.Value;
                    //remainGetQty = remainGetQty - getx;
                    if (remainGetQty >= 0)
                        returnGetYQty++;
                }
                return returnGetYQty;
            }
            return 0;

        }



        static OfferDto CheckAvailabilityOfferByProduct(InvoiceDto Invoice, ObservableCollection<OfferDto> offers, ProductDto_POS product, InvoiceLineItemDto invoiceitem, ProductServices productServices)
        {
            var request = new Orderrequest
            {
                ProductId = product.Id,
                Quantity = 0,
                Price = 0,
                CustomerGroupId = Invoice.CustomerGroupId,
                OutletId = Invoice.OutletId
            };

            return GetofferByProduct(Invoice, offers, product, request, productServices);
        }

        static InvoiceDto ApplyBuyXGetXOffer(InvoiceDto invoice, ObservableCollection<OfferDto> offers, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, ProductServices productServices, int? customerGroupId, bool calltotal = true)
        {
            ObservableCollection<InvoiceLineItemDto> toaddOffer = new ObservableCollection<InvoiceLineItemDto>();

            List<OfferItemDto> offerItem = null;
            var offerItemObject = productoffer.OfferItems;

            toaddOffer = ApplyPercentageOffOffer(invoice, offerItemObject, offerItem, toaddOffer, productoffer, invoiceLineItem, productServices);
            //Start Ticket #73624 iOS Discount: Buy X units of brand A and get Y units of brand B for free : Not working (All Discount with Exchange) By Pratik
            // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
            //if (invoice.Status == InvoiceStatus.Exchange)
            //    toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.IsExchangedProduct == false && (x.OfferId == null || x.OfferId == productoffer.OfferId)));
            //else
            //    toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end: #65929 by Pratik


            //Ticket start:#26866,#26599 Getting different total amount in cart in iPad.(No discount for UOM product).by rupesh
            toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.Where(x => x.InvoiceItemType != InvoiceItemType.UnityOfMeasure));
            //Ticket end:#26866,#26599 .by rupesh
            //End Ticket #73624 (All Discount with Exchange)
            decimal discountedQty = 0;
            //Exchange Issue ticket 7987, 7657
            if (toaddOffer.Count() > 0)
            {
                var sumQty = toaddOffer.Where(x => x.Quantity > 0).Sum(x => x.Quantity);
                var sumRetailPrice = toaddOffer.Sum(x => x.RetailPrice);
                discountedQty = GetdiscountqtyfromBuyxGetx(productoffer.BuyX.Value, productoffer.GetX.Value, Math.Abs(sumQty));

                var minValuelineitem = toaddOffer.Min(x => x.RetailPrice);
                toaddOffer = new ObservableCollection<InvoiceLineItemDto>(toaddOffer.OrderBy(i => i.RetailPrice));
            }

            //toaddOffer.sort(function(a, b) { return a.retailPrice - b.retailPrice });

            // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
            //         foreach (var item in toaddOffer)
            //{
            //	if (item.IsOfferAdded)
            //		continue;

            //	if (item.DiscountedQty > 0)
            //	{
            //		item.DiscountedQty = 0;
            //		item.DiscountValue = 0;
            //		CalculateLineItemTotal(item, invoice);
            //	}
            //}
            //Ticket end: #65929 by Pratik

            var lineItemTitle = "";
            if (productoffer.Description == null)
                lineItemTitle = "Discount" + "/ " + productoffer.Name;
            else
                lineItemTitle = "Discount" + "/ " + productoffer.Description;

            decimal finalDiscountedQuantity = 0;
            decimal totalRetailPrice = 0;

            // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
            //Exchange Issue ticket 7987, 7657
            //         foreach (var item in toaddOffer)
            //{
            //	if (item.IsOfferAdded)
            //		continue;

            //	if (item.EffectiveAmount > 0)
            //	{
            //		finalDiscountedQuantity = finalDiscountedQuantity + item.Quantity;
            //		totalRetailPrice = totalRetailPrice + (item.RetailPrice * item.Quantity);
            //	}
            //}
            //Ticket end: #65929 by Pratik

            decimal finalDiscountValue = 0;
            decimal finalDiscount = 0;
            var remainingQty = Math.Abs(discountedQty);
            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;

                // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
                if (item.DiscountedQty > 0)
                {
                    item.DiscountedQty = 0;
                    item.DiscountValue = 0;
                }
                CalculateLineItemTotal(item, invoice,calltotal);
                if (item.EffectiveAmount > 0)
                {
                    finalDiscountedQuantity = finalDiscountedQuantity + item.Quantity;
                    totalRetailPrice = totalRetailPrice + (item.RetailPrice * item.Quantity);
                }
                //Ticket end: #65929 by Pratik

                if ((item.Quantity - item.BackOrderQty ?? 0) <= item.Quantity)
                {
                    //customer group discount..
                    if (customerGroupId != null)
                    {
                        var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                        item.SoldPrice = item.RetailPrice - discount;
                        //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                        totalRetailPrice = 0;
                        //End Ticket #74344 By pratik
                    }
                    else if (customerGroupId == null && item.DiscountValue == 0)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    else if (customerGroupId == null)
                    {
                        item.SoldPrice = item.RetailPrice;
                    }
                    //customer group discount..

                    //line item Discount..
                    if (item.DiscountValue > 0)
                    {
                        var discountedValue = GetValuefromPercent(item.SoldPrice, item.DiscountValue);
                        item.SoldPrice = item.SoldPrice - discountedValue;
                    }
                    //line item Discount..

                    if (discountedQty > 0)
                    {
                        //var remainingQty = value.quantity - Math.abs(discountedQty);
                        if (remainingQty > 0)
                        {
                            decimal toRemoveQty = 0;
                            if (item.Quantity >= remainingQty)
                                toRemoveQty = remainingQty;
                            else if (item.Quantity < remainingQty)
                                toRemoveQty = item.Quantity;

                            var fromPrice = item.SoldPrice * toRemoveQty;
                            finalDiscountValue = item.SoldPrice * (toRemoveQty);
                            finalDiscount += finalDiscountValue;
                            remainingQty = remainingQty - toRemoveQty;

                            var discountPrice = GetDiscountPercentfromValue((item.SoldPrice * item.Quantity), (item.SoldPrice * item.Quantity) - fromPrice);
                            item.OfferDiscountPercent = discountPrice;

                            var productId = item.InvoiceItemValue;
                            if (item.DiscountIsAsPercentage)
                            {
                                //Exchange Issue ticket 7987, 7657
                                //var hasInvoiceItem = abpCommonHelpers.$filter("where")(Invoice.invoiceLineItems, { title: lineItemTitle, invoiceItemType: app.consts.invoiceItemType.discount })[0];
                                var hasInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                                //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                                var hasRecentlyAddedInvoiceItem = invoice.InvoiceLineItems.FirstOrDefault(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount && x.IsExchangedProduct == false);
                                if (hasInvoiceItem == null || ((invoice.Status == InvoiceStatus.Exchange && hasRecentlyAddedInvoiceItem == null && hasInvoiceItem.TaxAmount >= 0 && hasInvoiceItem.EffectiveAmount >= 0)))
                                {
                                    AddInvoiceItemLineApplyBuyXGetXOffer(invoice, productoffer, invoiceLineItem, lineItemTitle, finalDiscountValue, item);
                                }
                                else
                                {
                                    UpdateInvoiceItemLineApplyBuyXGetXOffer(invoice, productoffer, discountedQty, finalDiscount, toRemoveQty, hasInvoiceItem, item);
                                }
                                //End Ticket #74344 By pratik
                            }
                        }
                    }
                    else
                    {
                        if (discountedQty <= 0)
                        {
                            if (invoice.Status != InvoiceStatus.Exchange)
                            {
                                var hasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                                while (hasInvoiceItem != null && hasInvoiceItem.Count() > 0)
                                {
                                    // Ticket Start #65929 iOS : Discount not working :" Buy X units of Product A and get Y units of Product B for free" and taxes wrongly counted By Pratik
                                    var firstoffer = hasInvoiceItem.FirstOrDefault();
                                    var applyofferLineItem = toaddOffer.Where(x => (x.OfferId == null || x.OfferId == firstoffer.InvoiceItemValue) && x.InvoiceItemType != InvoiceItemType.Discount && x.IsExchangedProduct == false);
                                    applyofferLineItem?.ForEach(a => a.OfferDiscountPercent = null);
                                    //Ticket end: #65929 by Pratik

                                    //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                                    invoice.InvoiceLineItems.Remove(hasInvoiceItem.FirstOrDefault());
                                }
                            }
                        }
                    }
                }
            }


            //  totalRetailPrice = 0;
            foreach (var item in toaddOffer)
            {
                if (item.IsOfferAdded)
                    continue;

                var retailPrice = item.RetailPrice;
                if (customerGroupId != null)
                {
                    var discount = GetValuefromPercent(item.RetailPrice, item.CustomerGroupDiscountPercent);
                    retailPrice = item.RetailPrice - discount;
                    totalRetailPrice = totalRetailPrice + retailPrice;
                }
            }
            //Exchange Issue ticket 7987, 7657
            var hasInvoiceItem1 = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
            //Ticket start:#83531 Tax amount mismatch in sales history and receipt. by rupesh
            toaddOffer.ForEach(x => x.DiscountOfferTax = 0);
            //Ticket end:#83531.by rupesh
            foreach (var dvalue in hasInvoiceItem1)
            {
                if (dvalue.TotalAmount < 0)
                {
                    decimal discountedLineItem = 0;
                    if (finalDiscountedQuantity != 0)
                        discountedLineItem = dvalue.TotalAmount / finalDiscountedQuantity;
                    foreach (var value in toaddOffer)
                    {
                        if (value.IsOfferAdded)
                            continue;

                        if (value.TotalAmount > 0)
                        {
                            var retailPrice = value.RetailPrice;
                            if (customerGroupId != null)
                            {
                                var discount = GetValuefromPercent(value.RetailPrice, value.CustomerGroupDiscountPercent);
                                retailPrice = value.RetailPrice - discount;
                            }

                            var finalAmount = (retailPrice * dvalue.TotalAmount) / totalRetailPrice;

                            // Note : uncomment below line and comment second line to
                            // resolved Zoho ticket: 8024

                            var discountPrice = GetDiscountPercentfromValue((retailPrice), ((retailPrice) - finalAmount));
                            // var discountPrice = GetDiscountPercentfromValue((retailPrice * value.Quantity), ((retailPrice * value.Quantity) - finalAmount));

                            value.OfferDiscountPercent = Math.Abs(discountPrice);

                            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                            if (value.InvoiceItemValue == dvalue.InvoiceItemValueParent && Settings.StoreGeneralRule.DisplayMutipleQuantitiesOfSameProduct)
                            {
                                var totalQuantity = toaddOffer.Where(a => a.InvoiceItemValue == dvalue.InvoiceItemValueParent && a.InvoiceItemType != InvoiceItemType.Discount && !a.IsExchangedProduct).Sum(a => a.Quantity);
                                if (totalQuantity > 0)
                                {
                                    value.DiscountOfferTax = dvalue.TaxAmount / totalQuantity;
                                }
                                else
                                    value.DiscountOfferTax = 0;
                            }
                            //End Ticket #74344 By pratik
                        }
                    }
                }
            }
            if (toaddOffer.Count == 0)
            {
                var HasInvoiceItem = invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == productoffer.OfferId && x.InvoiceItemType == InvoiceItemType.Discount);
                while (HasInvoiceItem != null && HasInvoiceItem.Count() > 0)
                {
                    //var indexExtra = invoice.InvoiceLineItems.IndexOf(item);
                    invoice.InvoiceLineItems.Remove(HasInvoiceItem.FirstOrDefault());
                }
            }
            return invoice;
        }
        //Exchange Issue ticket 7987, 7657
        private static void AddInvoiceItemLineApplyBuyXGetXOffer(InvoiceDto invoice, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, string lineItemTitle, decimal finalDiscountValue, InvoiceLineItemDto item)
        {
            var invoiceItem = new InvoiceLineItemDto();

            if (invoiceLineItem != null && invoiceLineItem.TaxId != 0)
            {
                invoiceItem.TaxId = invoiceLineItem.TaxId;
                invoiceItem.TaxName = invoiceLineItem.TaxName;
                invoiceItem.TaxRate = invoiceLineItem.TaxRate;
            }
            else
            {
                invoiceItem.TaxId = 1;
                invoiceItem.TaxName = "NoTax";
                invoiceItem.TaxRate = 0;
            }


            var currentRegister = Settings.CurrentRegister;
            if (currentRegister != null)
            {
                invoiceItem.RegisterId = currentRegister.Id;
                invoiceItem.RegisterName = currentRegister.Name;
                invoiceItem.RegisterClosureId = (currentRegister.Registerclosure != null) ? currentRegister.Registerclosure.Id : 0;
            }

            invoiceItem.InvoiceItemValue = productoffer.OfferId ?? 0;

            if (item.InvoiceItemValueParent != null)
                invoiceItem.InvoiceItemValueParent = item.InvoiceItemValueParent;
            else
                invoiceItem.InvoiceItemValueParent = item.InvoiceItemValue;

            invoiceItem.InvoiceItemType = InvoiceItemType.Discount;
            invoiceItem.Sequence = invoice.InvoiceLineItems.Count + 1;

            invoiceItem.Quantity = 1;
            invoiceItem.TotalAmount = -finalDiscountValue;
            invoiceItem.Title = lineItemTitle;
            invoiceItem.TotalDiscount = 0;
            invoiceItem.OfferId = 0;
            invoiceItem.TaxAmount = 0;
            var discountedPrice = -finalDiscountValue;
            invoiceItem.TaxExclusiveTotalAmount = discountedPrice;
            invoiceItem.EffectiveAmount = discountedPrice;
            invoiceItem.SoldPrice = discountedPrice;
            invoiceItem.RetailPrice = discountedPrice;

            decimal taxAmount = 0;
            taxAmount = CalculateTaxInclusive(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            if (invoice.TaxInclusive)
                invoiceItem.TaxExclusiveTotalAmount = invoiceItem.TaxExclusiveTotalAmount - taxAmount;

            invoiceItem.TaxAmount = GetValuefromPercent(invoiceItem.TaxExclusiveTotalAmount, invoiceItem.TaxRate);

            //invoiceItem.class = 'sell-item-offer';
            //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
              MainThread.BeginInvokeOnMainThread(()=>{
                if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    invoice.InvoiceLineItems.Add(invoiceItem);
                else
                    invoice.InvoiceLineItems.Insert(0, invoiceItem);
            });
            //End:#45375 .by rupesh
        }
        //Exchange Issue ticket 7987, 7657
        private static void UpdateInvoiceItemLineApplyBuyXGetXOffer(InvoiceDto invoice, OfferDto productoffer, decimal discountedQty, decimal finalDiscount, decimal toRemoveQty, InvoiceLineItemDto hasInvoiceItem, InvoiceLineItemDto item)
        {
            if ((productoffer.GetX * discountedQty) >= toRemoveQty)
            {
                decimal discountedPrice = 0;
                if (invoice.Status == InvoiceStatus.Refunded)
                {
                    discountedPrice = finalDiscount;
                }
                else
                {
                    discountedPrice = -finalDiscount;
                }
                hasInvoiceItem.Quantity = 1;
                hasInvoiceItem.TotalAmount = discountedPrice;
                hasInvoiceItem.TotalDiscount = 0;
                hasInvoiceItem.TaxExclusiveTotalAmount = discountedPrice;
                hasInvoiceItem.TaxId = item.TaxId;
                hasInvoiceItem.TaxName = item.TaxName;
                hasInvoiceItem.TaxRate = item.TaxRate;

                decimal taxAmount = 0;
                taxAmount = CalculateTaxInclusive(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

                if (invoice.TaxInclusive)
                    hasInvoiceItem.TaxExclusiveTotalAmount = hasInvoiceItem.TaxExclusiveTotalAmount - taxAmount;

                hasInvoiceItem.TaxAmount = GetValuefromPercent(hasInvoiceItem.TaxExclusiveTotalAmount, hasInvoiceItem.TaxRate);

                hasInvoiceItem.EffectiveAmount = discountedPrice;
                hasInvoiceItem.SoldPrice = discountedPrice;
                hasInvoiceItem.RetailPrice = discountedPrice;
            }
        }

        static decimal GetDiscountQtyFromBuyxGetx(decimal buyx, decimal getx, decimal actualQty)
        {
            if ((buyx + getx) > actualQty)
                return 0;
            if ((buyx + getx) == actualQty)
                return getx;

            if ((buyx + getx) < actualQty)
            {
                var loopLength = actualQty / (buyx + getx);
                var remainQty = actualQty;
                var freeQty = 0;
                for (var i = 1; i <= loopLength; i++)
                {
                    remainQty = remainQty - (buyx + getx);
                    if (remainQty >= 0)
                        freeQty++;
                }
                return freeQty * getx;
            }
            return 0;
        }

        static decimal GetDiscountQtyWhenBuyx(decimal buyx, decimal actualQty)
        {

            if (buyx > actualQty)
                return 0;
            if (buyx == actualQty)
                return 1;

            if (buyx < actualQty)
            {
                var loopLength = actualQty / buyx;
                var remainQty = actualQty;
                var freeQty = 0;
                for (var i = 1; i <= loopLength; i++)
                {
                    remainQty = remainQty - buyx;
                    if (remainQty >= 0)
                        freeQty++;
                }
                return freeQty * 1;
            }
            return 0;
        }

        public static OfferDto GetofferByProduct(InvoiceDto invoice, ObservableCollection<OfferDto> offers, ProductDto_POS product, Orderrequest request, ProductServices productService)
        {
            OfferDto productoffer = null;

            if (offers == null)
            {
                return productoffer;
            }

            List<CategoryDto> catagoriesdata = new List<CategoryDto>();
            //maui
            if (product.ProductCategories != null && product.ProductCategories.Count > 0)
            {
                catagoriesdata = productService.GetLocalCategoriesByIds(product.ProductCategories.ToList());
            }
            //

            foreach (var offer in offers)
            {
                if (!offer.IsActive)
                    continue;

                if (offer.OfferType == OfferType.OnSale || offer.OfferType == OfferType.Composite)
                    continue;

                //Ticket start: #100117
                if (offer.OfferDays != null && offer.OfferDays.Any() && !offer.OfferDays.Contains((int)OfferWeekDays.AllDays))
                {
                    var weekDay = (int)DateTime.Now.DayOfWeek;
                    if (!offer.OfferDays.Contains(weekDay))
                        continue;
                }
                //Ticket end:#100117

                if (productoffer != null)
                    break;

                
                
                if(offer.OfferItems != null && offer.OfferItems.Any(x => x.OfferOn == OfferOn.Brand)) //Start #91264 By Pratik
                {
                    var brandofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Brand && x.OfferOnId == product.BrandId);
                    if ((!offer.Include && brandofferitems == null) || (offer.Include && brandofferitems != null)) //Start #91264 By Pratik
                    {
                        productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null)
                        { break; }
                    }
                }

                if(offer.OfferItems != null && offer.OfferItems.Any(x => x.OfferOn == OfferOn.Season)) //Start #91264 By Pratik
                {
                    var seasonofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Season && x.OfferOnId == product.SeasonId);
                    if (seasonofferitems != null)
                    {
                        productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null)
                            break;
                    }
                }

                if(offer.OfferItems != null && offer.OfferItems.Any(x => x.OfferOn == OfferOn.Product)) //Start #91264 By Pratik
                {
                    var productofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Product && (x.OfferOnId == product.Id || x.OfferOnId == product.ParentId));
                    if ((!offer.Include && productofferitems == null) || (offer.Include && productofferitems != null)) //Start #91264 By Pratik
                    {
                        productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null)
                            break;
                    }
                }


                //maui
                if (catagoriesdata != null && catagoriesdata.Count > 0 && offer.OfferItems != null && offer.OfferItems.Any(x => x.OfferOn == OfferOn.Category)) //Start #91264 By Pratik
                {
                    var categoryofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Category && catagoriesdata.Any(c => c.Id == x.OfferOnId || c.ParentId == x.OfferOnId));
                    if ((!offer.Include && categoryofferitems == null) || (offer.Include && categoryofferitems != null)) //Start #91264 By Pratik
                    {
                        productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null)
                            break;
                    }
                }
                //end Maui


                ////Xam
                //var categoryofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Category && product.ProductCategoryDtos.Any(c => c.Id == x.OfferOnId || c.ParentId == x.OfferOnId));

                //if (categoryofferitems != null)
                //{
                //    productoffer = CheckofferValidation(invoice, offer, request, productService);
                //    if (productoffer != null)
                //        break;
                //}
                ///End Xam

                //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                if(offer.OfferItems != null && offer.OfferItems.Any(x => x.OfferOn == OfferOn.ProductTag)) //Start #91264 By Pratik
                {
                    var producttagofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.ProductTag && product.ProductTags != null && product.ProductTags.Any(c => c == x.OfferOnId));
                    if ((!offer.Include && producttagofferitems == null) || (offer.Include && producttagofferitems != null)) //Start #91264 By Pratik
                    {
                        productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null)
                            break;
                    }
                }

                //End #84438 by Pratik

                //Ticket start:#54908 iPad: FR: Customer Tag Discount. by rupesh
                if(offer.OfferItems != null && offer.OfferItems.Any(x => x.OfferOn == OfferOn.Tags)) //Start #91264 By Pratik
                {
                    // var tagofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Tags && product.ProductTags.Any(c => c == x.OfferOnId));
                    // if ((!offer.Include && tagofferitems == null) || (offer.Include && tagofferitems != null)) //Start #91264 By Pratik
                    // {
                    //     productoffer = CheckofferValidation(invoice, offer, request, productService);
                    //     if (productoffer != null)
                    //         break;
                    // }
                    var tagofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Tags && invoice.CustomerDetail?.APICustomerTags != null && invoice.CustomerDetail.APICustomerTags.Any(c => c == x.OfferOnId));
                    if ((!offer.Include && invoice?.CustomerDetail != null && invoice?.CustomerDetail.Id > 0  && tagofferitems == null) || (offer.Include && tagofferitems != null)) //Start #91264 By Pratik
                    {
                        productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null)
                            break;
                    }
                }
                //Ticket end:#54908 . by rupesh

            }
            return productoffer;

        }

        //Ticket start:#30959 iPad - New feature request :: Rule for discount offers when there are more than one offers applicable.by rupesh
        public static List<OfferDto> GetoffersByProduct(InvoiceDto invoice, ObservableCollection<OfferDto> offers, ProductDto_POS product, Orderrequest request, InvoiceLineItemDto invoiceLineItem, ProductServices productService)
        {
            invoice.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(invoice.InvoiceLineItems);
            // invoice.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>(invoice.InvoiceLineItems.Where(x => x.InvoiceItemValue == invoiceLineItem.InvoiceItemValue));
            invoice.InvoiceLineItems.ForEach(x => x.IsOfferAdded = false);
            List<OfferDto> productoffers = new List<OfferDto>();
            List<CategoryDto> catagoriesdata = new List<CategoryDto>();

            if (offers == null)
            {
                return productoffers;
            }

            //maui
            if (product.ProductCategories != null && product.ProductCategories.Count > 0)
            {
                catagoriesdata = productService.GetLocalCategoriesByIds(product.ProductCategories.ToList());
            }
            //

            foreach (var offer in offers)
            {
                if (!offer.IsActive)
                    continue;

                if (offer.OfferType == OfferType.OnSale || offer.OfferType == OfferType.Composite)
                    continue;

                //Ticket start:#100117
                if (offer.OfferDays != null && offer.OfferDays.Any() && !offer.OfferDays.Contains((int)OfferWeekDays.AllDays))
                {
                    var weekDay = (int)DateTime.Now.DayOfWeek;
                    if (!offer.OfferDays.Contains(weekDay))
                        continue;
                }
                //Ticket end:#100117 


                
                //Tickket start:#56383 iPad: discount issue.by rupesh
                if(offer.OfferItems != null && offer.OfferItems.Any(x=> x.OfferOn == OfferOn.Brand)) //Start #91264 By Pratik
                {
                    var brandofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Brand && x.OfferOnId == product.BrandId);
                    if ((!offer.Include && brandofferitems == null) || (offer.Include && brandofferitems != null)) //Start #91264 By Pratik
                    {
                        var productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null && !productoffers.Any(a=>a.Id == productoffer.Id))
                        {
                            productoffers.Add(productoffer);
                        }
                    }
                }

                if(offer.OfferItems != null && offer.OfferItems.Any(x=> x.OfferOn == OfferOn.Season)) //Start #91264 By Pratik
                {
                    var seasonofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Season && x.OfferOnId == product.SeasonId);
                    if (seasonofferitems != null)
                    {
                        var productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null)
                            productoffers.Add(productoffer);
                    }
                }

                if(offer.OfferItems != null && offer.OfferItems.Any(x=> x.OfferOn == OfferOn.Product)) //Start #91264 By Pratik
                {
                    var productofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Product && (x.OfferOnId == product.Id || x.OfferOnId == product.ParentId));
                    if ((!offer.Include && productofferitems == null) || (offer.Include && productofferitems != null)) //Start #91264 By Pratik
                    {
                        var productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null && !productoffers.Any(a=> a.Id == productoffer.Id))
                            productoffers.Add(productoffer);
                    }
                }

                //maui
                if (catagoriesdata != null && catagoriesdata.Count > 0 && offer.OfferItems != null && offer.OfferItems.Any(x=> x.OfferOn == OfferOn.Category)) //Start #91264 By Pratik
                {
                    var categoryofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Category && (catagoriesdata.Any(c => c.Id == x.OfferOnId || c.ParentId == x.OfferOnId)));
                    if ((!offer.Include && categoryofferitems == null) || (offer.Include && categoryofferitems != null)) //Start #91264 By Pratik
                    {
                        var productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null && !productoffers.Any(a=>a.Id == productoffer.Id))
                            productoffers.Add(productoffer);
                    }
                }
                //end Maui

                //var categoryofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Category && (product.ProductCategoryDtos.Any(c => c.Id == x.OfferOnId || c.ParentId == x.OfferOnId)));
                //if (categoryofferitems != null)
                //{
                //    var productoffer = CheckofferValidation(invoice, offer, request, productService);
                //    if (productoffer != null)
                //        productoffers.Add(productoffer);
                //}

                //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                if(offer.OfferItems != null && offer.OfferItems.Any(x=> x.OfferOn == OfferOn.ProductTag)) //Start #91264 By Pratik
                {
                    var producttagofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.ProductTag && product.ProductTags != null && product.ProductTags.Any(c => c == x.OfferOnId));
                    if ((!offer.Include && producttagofferitems == null) || (offer.Include && producttagofferitems != null)) //Start #91264 By Pratik
                    {
                        var productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null && !productoffers.Any(a=>a.Id == productoffer.Id))
                            productoffers.Add(productoffer);
                    }
                }
                //End #84438 by Pratik

                if(offer.OfferItems != null && offer.OfferItems.Any(x=> x.OfferOn == OfferOn.Tags)) //Start #91264 By Pratik
                {
                    // var tagofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Tags && (product.ProductTags.Any(c => c == x.OfferOnId)));
                    // if ((!offer.Include && tagofferitems == null) || (offer.Include && tagofferitems != null)) //Start #91264 By Pratik
                    // {
                    //     var productoffer = CheckofferValidation(invoice, offer, request, productService);
                    //     if (productoffer != null && !productoffers.Any(a=>a.Id == productoffer.Id))
                    //         productoffers.Add(productoffer);
                    // }
                    //Ticket start:#54908 iPad: FR: Customer Tag Discount. by rupesh
                    var tagofferitems = offer.OfferItems.FirstOrDefault(x => x.OfferOn == OfferOn.Tags && invoice.CustomerDetail?.APICustomerTags != null && (invoice.CustomerDetail.APICustomerTags.Any(c => c == x.OfferOnId)));
                    if ((!offer.Include && invoice?.CustomerDetail != null && invoice?.CustomerDetail.Id > 0 && tagofferitems == null) || (offer.Include && tagofferitems != null)) //Start #91264 By Pratik
                    {
                        var productoffer = CheckofferValidation(invoice, offer, request, productService);
                        if (productoffer != null && !productoffers.Any(a=>a.Id == productoffer.Id))
                            productoffers.Add(productoffer);
                    }
                }
                //Ticket end:#54908 . by rupesh
                //Tickket end:#56383 .by rupesh
            }
            productoffers.Sort(delegate (OfferDto x, OfferDto y)
            {
                decimal xValue = 0;
                decimal yValue = 0;
                if (x != null)
                {
                    
                    if (x.OfferType == OfferType.Simple)
                    {
                        invoiceLineItem = ApplyPercentOff(invoice, x, invoiceLineItem, invoice.CustomerGroupId, productService);
                    }
                    else if (x.OfferType == OfferType.Bin)
                    {
                        invoiceLineItem = ApplyBinOffer(invoice, x, invoiceLineItem, invoice.CustomerGroupId, productService);
                    }
                    else if (x.OfferType == OfferType.buyxgety)
                    {
                        if (x.BuyX == null)
                        {
                            x.BuyX = 0;
                        }

                        if (x.GetX == null)
                        {
                            x.GetX = 0;
                        }

                        invoice = ApplyBuyXGetXOffer(invoice, offers, x, invoiceLineItem, productService, invoice.CustomerGroupId);
                    }
                    else if (x.OfferType == OfferType.buyxgetPercent)
                    {
                        invoice = ApplyBuyXGetPercentOffer(invoice, offers, x, invoiceLineItem, invoice.CustomerGroupId, productService);
                    }
                    else if (x.OfferType == OfferType.buyxgetPercentoffonbuyY)
                    {
                        invoice = ApplyBuyXGetPercentOffOnBuyY(invoice, offers, x, invoiceLineItem, productService, invoice.CustomerGroupId);
                    }
                    else if (x.OfferType == OfferType.buyxgetValueOff)
                    {
                        invoice = ApplyBuyXGetValueOffer(invoice, offers, x, invoiceLineItem, invoice.CustomerGroupId);
                    }
                    else if (x.OfferType == OfferType.buyxgetYfree)
                    {
                        invoice = ApplyBuyXGetYFreeOffer(invoice, offers, x, invoiceLineItem, productService, invoice.CustomerGroupId);
                    }
                    else if (x.OfferType == OfferType.buyxormoregetValueOff)
                    {
                        invoice = ApplyBuyXOrMoreGetValueOffer(invoice, offers, x, invoiceLineItem, invoice.CustomerGroupId);
                    }
                    //invoiceLineItem.OfferId = x.OfferId;
                    //invoiceLineItem.OffersNote = x.Description;
                    var xLineItem = invoice.InvoiceLineItems.FirstOrDefault(s => s.InvoiceItemType == InvoiceItemType.Discount && s.InvoiceItemValue == x.Id);
                    if (xLineItem != null)
                    {
                        xValue = xLineItem.TotalAmount.ToPositive(); //Start #95236 Discount drop down menu By PR
                        //invoice.InvoiceLineItems.Remove(xLineItem);
                    }

                }
                if (y != null)
                {
                    if (y.OfferType == OfferType.Simple)
                    {
                        invoiceLineItem = ApplyPercentOff(invoice, y, invoiceLineItem, invoice.CustomerGroupId, productService);
                    }
                    else if (y.OfferType == OfferType.Bin)
                    {
                        invoiceLineItem = ApplyBinOffer(invoice, y, invoiceLineItem, invoice.CustomerGroupId, productService);
                    }
                    else if (y.OfferType == OfferType.buyxgety)
                    {
                        if (y.BuyX == null)
                        {
                            y.BuyX = 0;
                        }

                        if (y.GetX == null)
                        {
                            y.GetX = 0;
                        }

                        invoice = ApplyBuyXGetXOffer(invoice, offers, y, invoiceLineItem, productService, invoice.CustomerGroupId);
                    }
                    else if (y.OfferType == OfferType.buyxgetPercent)
                    {
                        invoice = ApplyBuyXGetPercentOffer(invoice, offers, y, invoiceLineItem, invoice.CustomerGroupId, productService);
                    }
                    else if (y.OfferType == OfferType.buyxgetPercentoffonbuyY)
                    {
                        invoice = ApplyBuyXGetPercentOffOnBuyY(invoice, offers, y, invoiceLineItem, productService, invoice.CustomerGroupId);
                    }
                    else if (y.OfferType == OfferType.buyxgetValueOff)
                    {
                        invoice = ApplyBuyXGetValueOffer(invoice, offers, y, invoiceLineItem, invoice.CustomerGroupId);
                    }
                    else if (y.OfferType == OfferType.buyxgetYfree)
                    {
                        invoice = ApplyBuyXGetYFreeOffer(invoice, offers, y, invoiceLineItem, productService, invoice.CustomerGroupId);
                    }
                    else if (y.OfferType == OfferType.buyxormoregetValueOff)
                    {
                        invoice = ApplyBuyXOrMoreGetValueOffer(invoice, offers, y, invoiceLineItem, invoice.CustomerGroupId);
                    }
                    //invoiceLineItem.OfferId = x.OfferId;
                    //invoiceLineItem.OffersNote = x.Description;
                    var yLineItem = invoice.InvoiceLineItems.FirstOrDefault(s => s.InvoiceItemType == InvoiceItemType.Discount && s.InvoiceItemValue == y.Id);
                    if (yLineItem != null)
                    {
                        yValue = yLineItem.TotalAmount.ToPositive(); //Start #95236 Discount drop down menu By PR
                        // invoice.InvoiceLineItems.Remove(yLineItem);
                    }

                }
                x.TotalDiscount = xValue;
                y.TotalDiscount = yValue;
                return xValue.CompareTo(yValue);


            });
            return productoffers;

        }
        //Ticket end:#30959 .by rupesh

        //Start #91264 iOS: FR Discount Offers: Create and EXCLUDED Brands offer By Pratik
        public static bool CheckSameOfferId(OfferDto productoffer, InvoiceLineItemDto item, OfferOn offerOn = OfferOn.Product)
        {
           if(productoffer.OfferType != OfferType.Simple)
               return true;
            return (productoffer.Id == item.OfferId || !item.OfferId.HasValue || (item.OfferId.HasValue && item.OfferId.Value == 0));
        }
        //End #91264 By Pratik

        //check offer on type..
        public static ObservableCollection<InvoiceLineItemDto> ApplyPercentageOffOffer(InvoiceDto invoice, ObservableCollection<OfferItemDto> offerItemObject, List<OfferItemDto> offerItem, ObservableCollection<InvoiceLineItemDto> toaddOffer, OfferDto productoffer, InvoiceLineItemDto invoiceLineItem, ProductServices productServices)
        {
            var offerOnIds = new ObservableCollection<int>();
            OfferOn offerOn = OfferOn.Product;

            //Start #91264 iOS: FR Discount Offers: Create and EXCLUDED Brands offer By Pratik
            if(productoffer.Include)
            {
                foreach (var item in offerItemObject)
                {
                    //Ticket start:#21684 Quantities sold for some categories not showing.by rupesh
                    if (item.OfferOn == OfferOn.Category && invoiceLineItem.CategoryDtos != null)
                    {
                        var cateObject = JsonConvert.DeserializeObject<List<CategoryDto>>(invoiceLineItem.CategoryDtos);
                        //Ticket end:#21684 .by rupesh
                        for (var i = 0; i < cateObject.Count(); i++)
                        {
                            offerItem = productoffer.OfferItems.Where(a => a.OfferOnId == cateObject[i].Id).ToList();
                            if (offerItem.Count() > 0)
                            {
                                offerOnIds.Add(item.OfferOnId);
                                offerOn = item.OfferOn;
                            }
                            else
                            {
                                offerItem = productoffer.OfferItems.Where(a => a.OfferOnId == cateObject[i].ParentId).ToList();
                                if (offerItem.Count() > 0)
                                    offerOnIds.Add(item.OfferOnId);
                                offerOn = item.OfferOn;
                            }
                        }

                        if (offerOnIds.Count() <= 0)
                        {
                            //Ticket start:#21684 Quantities sold for some categories not showing.by rupesh
                            var productCategoryList = JsonConvert.DeserializeObject<List<ProductCategoryDto>>(invoiceLineItem.CategoryDtos);
                            //Ticket end:#21684 .by rupesh
                            for (var i = 0; i < productCategoryList.Count(); i++)
                            {
                                offerItem = productoffer.OfferItems.Where(a => a.OfferOnId == productCategoryList[i].CategoryId).ToList();

                                if (offerItem.Count() > 0)
                                {
                                    offerOnIds.Add(item.OfferOnId);
                                    offerOn = item.OfferOn;
                                }
                                else
                                {
                                    offerItem = productoffer.OfferItems.Where(a => a.OfferOnId == cateObject[i].ParentId).ToList();
                                    if (offerItem.Count() > 0)
                                        offerOnIds.Add(item.OfferOnId);
                                    offerOn = item.OfferOn;
                                }
                            }
                        }
                    }
                    else if (item.OfferOn == OfferOn.Brand)
                    {
                        offerItem = productoffer.OfferItems.Where(a => a.OfferOnId.ToString() == invoiceLineItem.Brand).ToList();
                        if (offerItem.Count() > 0)
                        {
                            offerOnIds.Add(item.OfferOnId);
                            offerOn = item.OfferOn;
                        }
                    }
                    //Ticket start:#54908 iPad: FR: Customer Tag Discount. by rupesh
                    else if (item.OfferOn == OfferOn.Tags)
                    {
                        if (invoice.CustomerDetail?.APICustomerTags != null && invoice.CustomerDetail?.APICustomerTags.Count > 0)
                        {
                            var tagObject = invoice.CustomerDetail.APICustomerTags;
                            for (var i = 0; i < tagObject.Count(); i++)
                            {
                                offerOn = item.OfferOn;
                                offerItem = productoffer.OfferItems.Where(a => a.OfferOnId == tagObject[i]).ToList();

                                if (offerItem.Count() > 0)
                                {
                                    offerOnIds.Add(item.OfferOnId);
                                }
                            }
                        }
                        // else if (invoiceLineItem.Tags != null)
                        // {
                        //     var tagObject = JsonConvert.DeserializeObject<List<ProductTagDto>>(invoiceLineItem.Tags);
                        //     for (var i = 0; i < tagObject.Count(); i++)
                        //     {
                        //         offerOn = item.OfferOn;
                        //         offerItem = productoffer.OfferItems.Where(a => a.OfferOnId.ToString() == tagObject[i].Id).ToList();
                        //         if (offerItem.Count() > 0)
                        //         {
                        //             offerOnIds.Add(item.OfferOnId);
                        //         }
                        //     }
                        // }


                    }
                    //Ticket end:#54908 . by rupesh
                    //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                    else if (item.OfferOn == OfferOn.ProductTag)
                    {
                        if (!string.IsNullOrEmpty(invoiceLineItem.Tags))
                        {
                            var tagObject = invoiceLineItem.Tags.TagsJsonToDto(productServices);
                            for (var i = 0; i < tagObject.Count(); i++)
                            {
                                offerOn = item.OfferOn;
                                offerItem = productoffer.OfferItems.Where(a => a.OfferOnId.ToString() == tagObject[i].Id).ToList();
                                if (offerItem.Count() > 0)
                                {
                                    offerOnIds.Add(item.OfferOnId);
                                }
                            }
                        }
                    }
                    //End #84438 by Pratik
                    else if (item.OfferOn == OfferOn.Season)
                    {
                        offerItem = productoffer.OfferItems.Where(a => a.OfferOnId.ToString() == invoiceLineItem.Season).ToList();
                        if (offerItem.Count() > 0)
                        {
                            offerOnIds.Add(item.OfferOnId);
                        }
                        offerOn = item.OfferOn;
                    }
                    //Ticket:start #45382 iPad: FR - Non-Discountable items.by rupesh
                    else if (invoiceLineItem.CategoryDtos != null)
                    //Ticket:end #45382.by rupesh
                    {
                        offerOn = item.OfferOn;                       
                        offerItem = productoffer.OfferItems.Where(a => a.OfferOnId == invoiceLineItem.InvoiceItemValue || a.OfferOnId == invoiceLineItem.InvoiceItemValueParent).ToList();
                        if (offerItem.Count() > 0)
                        {
                                offerOnIds.Add(item.OfferOnId);
                        }
                    }
                }
            }
            else
            {
                var item = productoffer.OfferItems.First();
                //Ticket start:#21684 Quantities sold for some categories not showing.by rupesh
                if (item.OfferOn == OfferOn.Category && invoiceLineItem.CategoryDtos != null)
                {
                    var cateObject = JsonConvert.DeserializeObject<List<CategoryDto>>(invoiceLineItem.CategoryDtos);
                    //Ticket end:#21684 .by rupesh
                    for (var i = 0; i < cateObject.Count(); i++)
                    {
                        offerItem = productoffer.OfferItems.Where(a => a.OfferOnId != cateObject[i].Id).ToList();
                        if (offerItem.Count() > 0)
                        {
                            offerOnIds.Add(item.OfferOnId);
                            offerOn = item.OfferOn;
                        }
                        else
                        {
                            offerItem = productoffer.OfferItems.Where(a => a.OfferOnId != cateObject[i].ParentId).ToList();
                            if (offerItem.Count() > 0)
                                offerOnIds.Add(item.OfferOnId);
                            offerOn = item.OfferOn;
                        }
                    }

                    if (offerOnIds.Count() <= 0)
                    {
                        //Ticket start:#21684 Quantities sold for some categories not showing.by rupesh
                        var productCategoryList = JsonConvert.DeserializeObject<List<ProductCategoryDto>>(invoiceLineItem.CategoryDtos);
                        //Ticket end:#21684 .by rupesh
                        for (var i = 0; i < productCategoryList.Count(); i++)
                        {
                            offerItem = productoffer.OfferItems.Where(a => a.OfferOnId != productCategoryList[i].CategoryId).ToList();

                            if (offerItem.Count() > 0)
                            {
                                offerOnIds.Add(item.OfferOnId);
                                offerOn = item.OfferOn;
                            }
                            else
                            {
                                offerItem = productoffer.OfferItems.Where(a => a.OfferOnId != cateObject[i].ParentId).ToList();
                                if (offerItem.Count() > 0)
                                    offerOnIds.Add(item.OfferOnId);
                                offerOn = item.OfferOn;
                            }
                        }
                    }
                }
                else if (item.OfferOn == OfferOn.Brand)
                {
                    offerItem = productoffer.OfferItems.Where(a => a.OfferOnId.ToString() != invoiceLineItem.Brand).ToList();
                    if (offerItem.Count() > 0)
                    {
                        offerOnIds.Add(item.OfferOnId);
                        offerOn = item.OfferOn;
                    }
                }
                //Ticket start:#54908 iPad: FR: Customer Tag Discount. by rupesh
                else if (item.OfferOn == OfferOn.Tags)
                {
                    if (invoice.CustomerDetail?.APICustomerTags != null && invoice.CustomerDetail?.APICustomerTags.Count > 0)
                    {
                        var tagObject = invoice.CustomerDetail.APICustomerTags;
                        for (var i = 0; i < tagObject.Count(); i++)
                        {
                            offerOn = item.OfferOn;
                            offerItem = productoffer.OfferItems.Where(a => a.OfferOnId != tagObject[i]).ToList();
                            if (offerItem.Count() > 0)
                            {
                                offerOnIds.Add(item.OfferOnId);
                            }
                        }
                    }
                    // else if (invoiceLineItem.Tags != null)
                    // {
                    //     var tagObject = JsonConvert.DeserializeObject<List<ProductTagDto>>(invoiceLineItem.Tags);
                    //     for (var i = 0; i < tagObject.Count(); i++)
                    //     {
                    //         offerOn = item.OfferOn;
                    //         offerItem = productoffer.OfferItems.Where(a => a.OfferOnId.ToString() != tagObject[i].Id).ToList();                          

                    //         if (offerItem.Count() > 0)
                    //         {
                    //             offerOnIds.Add(item.OfferOnId);
                    //         }
                    //     }
                    // }


                }
                //Ticket end:#54908 . by rupesh
                //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                else if (item.OfferOn == OfferOn.ProductTag)
                {
                    if (!string.IsNullOrEmpty(invoiceLineItem.Tags))
                    {
                        var tagObject = invoiceLineItem.Tags.TagsJsonToDto(productServices);
                        for (var i = 0; i < tagObject.Count(); i++)
                        {
                            offerOn = item.OfferOn;
                            offerItem = productoffer.OfferItems.Where(a => a.OfferOnId.ToString() != tagObject[i].Id).ToList();
                            if (offerItem.Count() > 0)
                            {
                                offerOnIds.Add(item.OfferOnId);
                            }
                        }
                    }
                }
                //End #84438 by Pratik
                else if (item.OfferOn == OfferOn.Season)
                {
                    offerItem = productoffer.OfferItems.Where(a => a.OfferOnId.ToString() == invoiceLineItem.Season).ToList();
                    if (offerItem.Count() > 0)
                    {
                        offerOnIds.Add(item.OfferOnId);
                    }
                    offerOn = item.OfferOn;
                }
                //Ticket:start #45382 iPad: FR - Non-Discountable items.by rupesh
                else if (invoiceLineItem.CategoryDtos != null)
                //Ticket:end #45382.by rupesh
                {
                    offerOn = item.OfferOn;
                    offerItem = productoffer.OfferItems.Where(a =>  a.OfferOnId != invoiceLineItem.InvoiceItemValue && a.OfferOnId != invoiceLineItem.InvoiceItemValueParent).ToList();
                    if (offerItem.Count > 0)
                    {
                        offerOnIds.Add(item.OfferOnId);
                    }                   

                }
            }
            //End #91264 By Pratik
            
            //Start #84438 iOS : FR :add discount offers on product tag by Pratik
            if (offerServices == null)
                offerServices = new OfferServices(offerApiService);
            var offerlst = invoice.InvoiceLineItems.Where(a => a.InvoiceItemType == InvoiceItemType.Discount).ToList();
            foreach (var item in offerlst)
            {
                var offerdata = offerServices.GetLocalOffer(item.InvoiceItemValue);
                if ((invoice.CustomerDetail?.APICustomerTags == null || (invoice.CustomerDetail?.APICustomerTags != null && invoice.CustomerDetail.APICustomerTags.Count <= 0))
                    && offerdata?.OfferItems != null && offerdata.OfferItems.Any(a => a.IsActive && a.OfferOn == OfferOn.Tags))
                {
                    invoice.InvoiceLineItems.Remove(item);
                    break;
                }
            }
            //End #84438 by Pratik

            foreach (var valueOffer in offerOnIds)
            {
                 
                var isTypeAvailable = false;
                //Start #91264 iOS: FR Discount Offers: Create and EXCLUDED Brands offer By Pratik
                if(productoffer.Include)
                {   
                    //End #91264 By Pratik

                    if (offerOn == OfferOn.Category)
                    {
                        ObservableCollection<CategoryDto> productTypes = null;
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            var cateObject = JsonConvert.DeserializeObject<List<CategoryDto>>(invoiceLineItem.CategoryDtos);
                            if (item != null && item.CategoryDtos != null && !item.DisableDiscountIndividually && item.Quantity > 0 && CheckSameOfferId(productoffer, item)) //Start #92641 By Pratik
                            {
                                var productCategories = JsonConvert.DeserializeObject<List<CategoryDto>>(item.CategoryDtos);
                                if (productCategories != null)
                                {
                                    productTypes = productServices.GetLocalCategoriesSync();
                                    isTypeAvailable = productTypes.Where(pt => pt.IsActive).Any(pt => productCategories.Any(pc => pc.Id == pt.Id || pc.ParentId == pt.Id));

                                    if (isTypeAvailable)
                                    {
                                        var categoryofferitems = productCategories.Where(x => x.Id == valueOffer || x.ParentId == valueOffer);
                                        if (categoryofferitems != null && categoryofferitems.Count() > 0)
                                        {
                                            var temptoaddOffer = toaddOffer.ToList();
                                            foreach (var toOfferValue in temptoaddOffer)
                                            {
                                                if (toOfferValue != null && toOfferValue.CategoryDtos != null)
                                                {
                                                    var toofferCategories = JsonConvert.DeserializeObject<List<CategoryDto>>(toOfferValue.CategoryDtos);
                                                    if (toofferCategories != null)
                                                    {
                                                        if (!toaddOffer.Contains(item))
                                                            toaddOffer.Add(item);
                                                    }
                                                }
                                            }
                                            if (toaddOffer.Count() == 0)
                                            {
                                                toaddOffer.Add(item);
                                            }
                                        }
                                    }
                                }

                                if (toaddOffer.Count() <= 0)
                                {
                                    //Ticket start:#21684 Quantities sold for some categories not showing.by rupesh
                                    var categories = JsonConvert.DeserializeObject<List<ProductCategoryDto>>(item.CategoryDtos);
                                    //Ticket end:#21684 .by rupesh
                                    if (categories != null)
                                    {
                                        if (productTypes == null || (productTypes != null && productTypes.Count <= 0))
                                            productTypes = productServices.GetLocalCategoriesSync();
                                        isTypeAvailable = productTypes.Where(pt => pt.IsActive).Any(pt => categories.Any(cat => cat.CategoryId == pt.Id || cat.ParentCategoryId == pt.Id));
                                        // foreach (var ptValue in productTypes)
                                        // {
                                        //     var typeAvailable = categories.Where(x => x.CategoryId == ptValue.Id || x.ParentCategoryId == ptValue.Id);
                                        //     if (typeAvailable != null && typeAvailable.Count() > 0)
                                        //     {
                                        //         if (ptValue.IsActive)
                                        //             isTypeAvailable = true;
                                        //     }
                                        // }

                                        if (isTypeAvailable)
                                        {
                                            var categoryofferitems = categories.Where(x => x.CategoryId == valueOffer || x.ParentCategoryId == valueOffer);
                                            if (categoryofferitems != null && categoryofferitems.Count() > 0)
                                            {
                                                //End #91264 By Pratik
                                                var temptoaddOffer = toaddOffer.ToList();
                                                foreach (var toOfferValue in temptoaddOffer)
                                                {
                                                    if (toOfferValue != null && toOfferValue.CategoryDtos != null)
                                                    {
                                                        var toofferCategories = JsonConvert.DeserializeObject<List<ProductCategoryDto>>(toOfferValue.CategoryDtos);
                                                        if (toofferCategories != null)
                                                        {
                                                            if (!toaddOffer.Contains(item))
                                                                toaddOffer.Add(item);
                                                        }
                                                    }
                                                }
                                                if (toaddOffer.Count() == 0)
                                                {
                                                    toaddOffer.Add(item);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (offerOn == OfferOn.Tags)
                    {
                        var tagId = valueOffer;
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                            //Ticket start:#54908 iPad: FR: Customer Tag Discount. by rupesh
                            if (item != null && invoice.CustomerDetail?.APICustomerTags != null && !item.DisableDiscountIndividually && item.IsExchangedProduct == false && invoice.CustomerDetail?.APICustomerTags.Count > 0 && CheckSameOfferId(productoffer,item)) //Start #92641 By Pratik
                            //End Ticket #74344 By pratik
                            {
                                var productTags = invoice.CustomerDetail?.APICustomerTags;
                                if (item.InvoiceItemType == InvoiceItemType.Discount)
                                {
                                    productTags = null;
                                    invoice.InvoiceLineItems.Remove(item);

                                }
                                //var productTags = app.utils.parJson(value.tags);
                                if (productTags != null)
                                {
                                    var tagofferitems = productTags.Where(x => x == valueOffer);
                                    if (tagofferitems != null && tagofferitems.Count() > 0)
                                    {

                                        //Ticket start: #23294 iOS - Discount Applied Wrongly.by rupesh
                                        var temptoaddOffer = toaddOffer.ToList();
                                        //Ticket end: #23294 .by rupesh
                                        foreach (var toOfferValue in temptoaddOffer)
                                        {
                                            if (toOfferValue != null && invoice.CustomerDetail?.APICustomerTags != null)
                                            {
                                                var toofferTags = invoice.CustomerDetail?.APICustomerTags;
                                                if (toofferTags != null)
                                                {
                                                    var hasInvoiceItem = toofferTags.Where(x => x == valueOffer);
                                                    if (hasInvoiceItem.Count() == 0)
                                                    {
                                                        var obj = toaddOffer.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && (a.InvoiceItemValue == item.InvoiceItemValue && a.InvoiceItemValueParent == item.InvoiceItemValueParent));
                                                        if (obj.Count() == 0)
                                                        {
                                                            toaddOffer.Add(item);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var obj = toaddOffer.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && (a.InvoiceItemValue == item.InvoiceItemValue && a.InvoiceItemValueParent == item.InvoiceItemValueParent));
                                                        if (obj.Count() == 0)
                                                        {
                                                            toaddOffer.Add(item);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (toaddOffer.Count() == 0)
                                            toaddOffer.Add(item);
                                    }
                                }
                            }
                            //Ticket end:#54908 . by rupesh

                            // //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
                            // else if (item != null && item.IsExchangedProduct == false && !string.IsNullOrEmpty(item.Tags) && CheckSameOfferId(productoffer,item))
                            // //End Ticket #74344 By pratik
                            // {
                            //     var productTags = JsonConvert.DeserializeObject<List<ProductTagDto>>(item.Tags);
                            //     //var productTags = app.utils.parJson(value.tags);
                            //     if (productTags != null)
                            //     {
                            //         var tagofferitems = productTags.Where(x => x.Id == valueOffer.ToString());
                            //         if (tagofferitems != null && tagofferitems.Count() > 0)
                            //         {
                            //             //Ticket start: #23294 iOS - Discount Applied Wrongly.by rupesh
                            //             var temptoaddOffer = toaddOffer.ToList();
                            //             //Ticket end: #23294 .by rupesh
                            //             foreach (var toOfferValue in temptoaddOffer)
                            //             {
                            //                 if (toOfferValue != null && !string.IsNullOrEmpty(toOfferValue.Tags))
                            //                 {
                            //                     var toofferTags = JsonConvert.DeserializeObject<List<ProductTagDto>>(toOfferValue.Tags);
                            //                     if (toofferTags != null)
                            //                     {
                            //                         var hasInvoiceItem = toofferTags.Where(x => x.Id == valueOffer.ToString());
                            //                         if (hasInvoiceItem.Count() == 0)
                            //                         {
                            //                             var obj = toaddOffer.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && (a.InvoiceItemValue == item.InvoiceItemValue && a.InvoiceItemValueParent == item.InvoiceItemValueParent));
                            //                             if (obj.Count() == 0)
                            //                             {
                            //                                 toaddOffer.Add(item);
                            //                             }
                            //                         }
                            //                         else
                            //                         {
                            //                             var obj = toaddOffer.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && (a.InvoiceItemValue == item.InvoiceItemValue && a.InvoiceItemValueParent == item.InvoiceItemValueParent));
                            //                             if (obj.Count() == 0)
                            //                             {
                            //                                 toaddOffer.Add(item);
                            //                             }
                            //                         }
                            //                     }
                            //                 }
                            //             }
                            //             if (toaddOffer.Count() == 0)
                            //                 toaddOffer.Add(item);
                            //         }
                            //     }
                            // }
                        }
                    }
                    //Start #84438 iOS : FR :add discount offers on product tag by Pratik
                    else if (offerOn == OfferOn.ProductTag)
                    {
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            if (item != null && item.IsExchangedProduct == false && !item.DisableDiscountIndividually && !string.IsNullOrEmpty(item.Tags) && CheckSameOfferId(productoffer,item)) //Start #92641 By Pratik
                            {

                                var  productTags = item.Tags.TagsJsonToDto(productServices);
                                if (productTags != null)
                                {
                                    var tagofferitems = productTags.Where(x => x.Id == valueOffer.ToString());
                                    if (tagofferitems != null && tagofferitems.Count() > 0)
                                    {
                                        var temptoaddOffer = toaddOffer.ToList();
                                        foreach (var toOfferValue in temptoaddOffer)
                                        {
                                            if (toOfferValue != null && !string.IsNullOrEmpty(toOfferValue.Tags))
                                            {
                                                var toofferTags = toOfferValue.Tags.TagsJsonToDto(productServices);
                                                if (toofferTags != null)
                                                {
                                                    if (!toaddOffer.Contains(item))
                                                        toaddOffer.Add(item);
                                                }
                                            }
                                        }
                                        if (toaddOffer.Count() == 0)
                                            toaddOffer.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    //End #84438 by Pratik
                    else if (offerOn == OfferOn.Brand)
                    {
                        var brandId = valueOffer;
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            if (item.Brand == brandId.ToString() && !item.DisableDiscountIndividually && CheckSameOfferId(productoffer,item)) //Start #92641 By Pratik
                            {
                                //Ticket start:#24889 IPad : Discount offter is not working.by rupesh
                                if (!toaddOffer.Contains(item))
                                    toaddOffer.Add(item);
                                //Ticket end:#24889 .by rupesh
                            }
                        }
                    }
                    else if (offerOn == OfferOn.Season)
                    {
                        var seasonId = valueOffer;
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            if (item.InvoiceItemType == InvoiceItemType.Standard && item.Season == seasonId.ToString() && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                            {
                                toaddOffer.Add(item);
                            }
                        }
                    }
                    else if (offerOn == OfferOn.Product)
                    {
                        var productId = valueOffer;
                        
                        foreach (var item in invoice.InvoiceLineItems)//#96028
                        {
                            //Start Ticket #73532 iOS: Inclusve Discount products get removed with discount on POS screen while removing any other product with exclusive discount offer setting applied By Pratik
                            //if (item.InvoiceItemType == InvoiceItemType.Standard && (item.InvoiceItemValue == productId || item.InvoiceItemValueParent == productId))
                            //{
                            //    toaddOffer.Add(item);
                            //}
                            if (item.InvoiceItemType == InvoiceItemType.Standard && !item.DisableDiscountIndividually && (item.InvoiceItemValue == productId || item.InvoiceItemValueParent == productId) && CheckSameOfferId(productoffer,item)) //Start #92641 By Pratik
                            {
                                toaddOffer.Add(item);
                            }
                            //End Ticket #73532 By Pratik

                        }
                    }
               
                //Start #91264 iOS: FR Discount Offers: Create and EXCLUDED Brands offer By Pratik
                }
                else
                {
                    if (offerOn == OfferOn.Category)
                    {
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            var cateObject = JsonConvert.DeserializeObject<List<CategoryDto>>(invoiceLineItem.CategoryDtos);
                            if (item != null && item.CategoryDtos != null && !item.DisableDiscountIndividually && item.Quantity > 0 && CheckSameOfferId(productoffer,item)) //Start #92641 By Pratik
                            {
                                var productCategories = JsonConvert.DeserializeObject<List<CategoryDto>>(item.CategoryDtos);
                                if (productCategories != null)
                                {
                                    var productTypes = productServices.GetLocalCategoriesSync();
                                    foreach (var ptValue in productTypes)
                                    {
                                        var typeAvailable = productCategories.Where(x => x.Id == ptValue.Id || x.ParentId == ptValue.Id);
                                        if (typeAvailable != null && typeAvailable.Count() > 0)
                                        {
                                            if (ptValue.IsActive)
                                                isTypeAvailable = true;
                                        }
                                    }

                                    if (isTypeAvailable)
                                    {
                                        var categoryofferitems = productCategories.Where(x => x.Id == valueOffer || x.ParentId == valueOffer).ToList();
                                        if (categoryofferitems == null || (categoryofferitems != null && categoryofferitems.Count <= 0))
                                        {
                                            var temptoaddOffer = toaddOffer.ToList();
                                            foreach (var toOfferValue in temptoaddOffer)
                                            {
                                                if (toOfferValue != null && toOfferValue.CategoryDtos != null)
                                                {
                                                    var toofferCategories = JsonConvert.DeserializeObject<List<CategoryDto>>(toOfferValue.CategoryDtos);
                                                    if (toofferCategories != null)
                                                    {
                                                        if (!toaddOffer.Contains(item))
                                                            toaddOffer.Add(item);
                                                    }
                                                }
                                            }
                                            if (toaddOffer.Count() == 0)
                                            {
                                                toaddOffer.Add(item);
                                            }
                                        }
                                    }
                                
                                    if (!item.OfferId.HasValue && item.InvoiceItemType != InvoiceItemType.Discount)
                                    {
                                        foreach(var cat in productCategories)
                                        {
                                            if(productoffer.OfferItems.Select(a=>a.OfferOnId).Contains(cat.Id))
                                            {
                                                if (toaddOffer.Contains(item))
                                                    toaddOffer.Remove(item);
                                            }
                                        }
                                    }
                                }

                                if (toaddOffer.Count() <= 0)
                                {
                                    var categories = JsonConvert.DeserializeObject<List<ProductCategoryDto>>(item.CategoryDtos);
                                    if (categories != null)
                                    {
                                        var productTypes = productServices.GetLocalCategoriesSync();
                                        foreach (var ptValue in productTypes)
                                        {
                                            var typeAvailable = categories.Where(x => x.CategoryId == ptValue.Id && x.ParentCategoryId == ptValue.Id);
                                            if (typeAvailable != null && typeAvailable.Count() > 0)
                                            {
                                                if (ptValue.IsActive)
                                                    isTypeAvailable = true;
                                            }
                                        }

                                        if (isTypeAvailable)
                                        {
                                          
                                            var categoryofferitems = categories.Where(x => x.CategoryId == valueOffer && x.ParentCategoryId == valueOffer).ToList();
                                            if (categoryofferitems == null || (categoryofferitems != null && categoryofferitems.Count <= 0))
                                            {
                                                var temptoaddOffer = toaddOffer.ToList();
                                                foreach (var toOfferValue in temptoaddOffer)
                                                {
                                                    if (toOfferValue != null && toOfferValue.CategoryDtos != null)
                                                    {
                                                        var toofferCategories = JsonConvert.DeserializeObject<List<ProductCategoryDto>>(toOfferValue.CategoryDtos);
                                                        if (toofferCategories != null)
                                                        {
                                                            if (!toaddOffer.Contains(item))
                                                                toaddOffer.Add(item);
                                                        }
                                                    }
                                                }
                                                if (toaddOffer.Count() == 0)
                                                {
                                                    toaddOffer.Add(item);
                                                }
                                            }
                                        }
                                    
                                        if (!item.OfferId.HasValue && item.InvoiceItemType != InvoiceItemType.Discount)
                                        {
                                            foreach(var cat in categories)
                                            {
                                                if(productoffer.OfferItems.Select(a=>a.OfferOnId).Contains(cat.CategoryId))
                                                {
                                                    if (toaddOffer.Contains(item))
                                                        toaddOffer.Remove(item);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (offerOn == OfferOn.Tags)
                    {
                        var tagId = valueOffer;
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            if (item != null && invoice.CustomerDetail?.APICustomerTags != null && !item.DisableDiscountIndividually && item.IsExchangedProduct == false && invoice.CustomerDetail?.APICustomerTags.Count > 0 && CheckSameOfferId(productoffer,item)) //Start #92641 By Pratik
                            {
                                var productTags = invoice.CustomerDetail?.APICustomerTags;
                                if (item.InvoiceItemType == InvoiceItemType.Discount)
                                {
                                    productTags = null;
                                    invoice.InvoiceLineItems.Remove(item);

                                }
                                if (productTags != null)
                                {
                                    var tagofferitems = productTags.Where(x => x == valueOffer).ToList();
                                    if (tagofferitems == null || (tagofferitems != null && tagofferitems.Count <= 0))
                                    {
                                        //End #91264 By Pratik

                                        //Ticket start: #23294 iOS - Discount Applied Wrongly.by rupesh
                                        var temptoaddOffer = toaddOffer.ToList();
                                        //Ticket end: #23294 .by rupesh
                                        foreach (var toOfferValue in temptoaddOffer)
                                        {
                                            if (toOfferValue != null && invoice.CustomerDetail?.APICustomerTags != null)
                                            {
                                                var toofferTags = invoice.CustomerDetail?.APICustomerTags;
                                                if (toofferTags != null)
                                                {
                                                    var hasInvoiceItem = toofferTags.Where(x => x == valueOffer);
                                                    if (hasInvoiceItem.Count() == 0)
                                                    {
                                                        var obj = toaddOffer.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && (a.InvoiceItemValue == item.InvoiceItemValue && a.InvoiceItemValueParent == item.InvoiceItemValueParent));
                                                        if (obj.Count() == 0)
                                                        {
                                                            toaddOffer.Add(item);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var obj = toaddOffer.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && (a.InvoiceItemValue == item.InvoiceItemValue && a.InvoiceItemValueParent == item.InvoiceItemValueParent));
                                                        if (obj.Count() == 0)
                                                        {
                                                            toaddOffer.Add(item);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (toaddOffer.Count() == 0)
                                            toaddOffer.Add(item);
                                    }
                                }
                            }
                            // else if (item != null && item.IsExchangedProduct == false && !string.IsNullOrEmpty(item.Tags) && CheckSameOfferId(productoffer,item))
                            // {
                            //     var productTags = JsonConvert.DeserializeObject<List<ProductTagDto>>(item.Tags);
                            //     if (productTags != null)
                            //     {
                            //         var tagofferitems = productTags.Where(x => x.Id == valueOffer.ToString()).ToList();
                            //         if (tagofferitems == null || (tagofferitems != null && tagofferitems.Count <= 0))
                            //         {
                            //             var temptoaddOffer = toaddOffer.ToList();
                            //             foreach (var toOfferValue in temptoaddOffer)
                            //             {
                            //                 if (toOfferValue != null && !string.IsNullOrEmpty(toOfferValue.Tags))
                            //                 {
                            //                     var toofferTags = JsonConvert.DeserializeObject<List<ProductTagDto>>(toOfferValue.Tags);
                            //                     if (toofferTags != null)
                            //                     {
                            //                         var hasInvoiceItem = toofferTags.Where(x => x.Id == valueOffer.ToString());
                            //                         if (hasInvoiceItem.Count() == 0)
                            //                         {
                            //                             var obj = toaddOffer.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && (a.InvoiceItemValue == item.InvoiceItemValue && a.InvoiceItemValueParent == item.InvoiceItemValueParent));
                            //                             if (obj.Count() == 0)
                            //                             {
                            //                                 toaddOffer.Add(item);
                            //                             }
                            //                         }
                            //                         else
                            //                         {
                            //                             var obj = toaddOffer.Where(a => a.InvoiceItemType == InvoiceItemType.Standard && (a.InvoiceItemValue == item.InvoiceItemValue && a.InvoiceItemValueParent == item.InvoiceItemValueParent));
                            //                             if (obj.Count() == 0)
                            //                             {
                            //                                 toaddOffer.Add(item);
                            //                             }
                            //                         }
                            //                     }
                            //                 }
                            //             }
                            //             if (toaddOffer.Count() == 0)
                            //                 toaddOffer.Add(item);
                            //         }
                            //     }
                            // }

                            if (!item.OfferId.HasValue && item.InvoiceItemType != InvoiceItemType.Discount && item.IsExchangedProduct == false && invoice.CustomerDetail?.APICustomerTags != null && invoice.CustomerDetail.APICustomerTags.Count > 0)
                            {
                                foreach(var cat in invoice.CustomerDetail.APICustomerTags)
                                {
                                    if(productoffer.OfferItems.Select(a=>a.OfferOnId).Contains(cat))
                                    {
                                        if (toaddOffer.Contains(item))
                                            toaddOffer.Remove(item);
                                    }
                                }
                            }
                        }
                    }
                    else if (offerOn == OfferOn.ProductTag)
                    {
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            if (item != null && item.IsExchangedProduct == false && !item.DisableDiscountIndividually && !string.IsNullOrEmpty(item.Tags) && CheckSameOfferId(productoffer,item)) //Start #92641 By Pratik
                            {

                                var  productTags = item.Tags.TagsJsonToDto(productServices);
                                if (productTags != null)
                                {
                                    var tagofferitems = productTags.Where(x => x.Id == valueOffer.ToString()).ToList();
                                    if (tagofferitems == null || (tagofferitems != null && tagofferitems.Count <= 0))
                                    {
                                        var temptoaddOffer = toaddOffer.ToList();
                                        foreach (var toOfferValue in temptoaddOffer)
                                        {
                                            if (toOfferValue != null && !string.IsNullOrEmpty(toOfferValue.Tags))
                                            {
                                                var toofferTags = toOfferValue.Tags.TagsJsonToDto(productServices);
                                                if (toofferTags != null)
                                                {
                                                    if (!toaddOffer.Contains(item))
                                                        toaddOffer.Add(item);
                                                }
                                            }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
                                        }
                                        if (toaddOffer.Count() == 0)
                                            toaddOffer.Add(item);
                                    }
                                }

                                if (!item.OfferId.HasValue && item.InvoiceItemType != InvoiceItemType.Discount && productTags != null && productTags.Count > 0)
                                {
                                    foreach(var cat in productTags)
                                    {
                                        if(productoffer.OfferItems.Select(a=>a.OfferOnId.ToString()).Contains(cat.Id))
                                        {
                                            if (toaddOffer.Contains(item))
                                                toaddOffer.Remove(item);
                                        }
                                    }
                                }
                            }
                           
                        }
                    }
                    else if (offerOn == OfferOn.Brand)
                    {
                        var brandId = valueOffer;
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            if (item.Brand != brandId.ToString() && !item.DisableDiscountIndividually && CheckSameOfferId(productoffer,item)) //Start #92641 By Pratik
                            {
                                if (item.InvoiceItemType == InvoiceItemType.Standard && !toaddOffer.Contains(item) && (item.OfferId.HasValue || (!item.OfferId.HasValue && !productoffer.OfferItems.Select(a=>a.OfferOnId.ToString()).Contains(item.Brand))))
                                    toaddOffer.Add(item);
                            }
                        }
                    }
                    else if (offerOn == OfferOn.Season)
                    {
                        var seasonId = valueOffer;
                        foreach (var item in invoice.InvoiceLineItems.Where(a=> a.InvoiceItemType != InvoiceItemType.CompositeProduct))//#96028
                        {
                            if (item.InvoiceItemType == InvoiceItemType.Standard && item.Season == seasonId.ToString() && !item.DisableDiscountIndividually) //Start #92641 By Pratik
                            {
                                toaddOffer.Add(item);
                            }
                        }
                    }
                    else if (offerOn == OfferOn.Product)
                    {
                        var productId = valueOffer;
                        foreach (var item in invoice.InvoiceLineItems) //#96028
                        {
                            if (item.InvoiceItemType == InvoiceItemType.Standard && !item.DisableDiscountIndividually && item.InvoiceItemValue != productId && item.InvoiceItemValueParent != productId
                             && CheckSameOfferId(productoffer,item))  //Start #92641 By Pratik
                            {
                                if(item.OfferId.HasValue || (!item.OfferId.HasValue && !productoffer.OfferItems.Select(a=>a.OfferOnId).Contains(item.InvoiceItemValue) && !productoffer.OfferItems.Select(a=>a.OfferOnId).Contains(item.InvoiceItemValueParent ?? 0)))
                                    toaddOffer.Add(item); 
                            }
                        }
                    }
                }
                //End #91264 By Pratik
            }                        
            return toaddOffer;
        }

        #endregion


        public static async Task<InvoiceDto> FinaliseOrder(InvoiceDto Invoice, ObservableCollection<OfferDto> offers, SaleServices saleService, OutletServices outletService, ProductServices productService, string invoiceStatus = null)
        {
            // Changes by jigar Ticket 8360
            InvoiceDto InvoiceBackOrder = null;
            // End 8360
            try
            {
                Debug.WriteLine("Invoice before updatelocal 0 : " + Newtonsoft.Json.JsonConvert.SerializeObject(Invoice).ToString());

                if (string.IsNullOrEmpty(Invoice.CustomerTempId))
                {
                    if (Invoice.CustomerDetail != null)
                    {
                        Invoice.CustomerTempId = Invoice.CustomerDetail.TempId;
                    }
                }

                //Start Ticket #73665 iOS: Group Taxes not matched in iOS for Prints from POS Screen and Same print from Sales history by pratik
                Invoice = LineItemTaxCalaculation(Invoice);
                //End Ticket #73665 by pratik

                if (Invoice.CustomerId == null || (Invoice.CustomerId == 0 && string.IsNullOrEmpty(Invoice.CustomerTempId)) || Invoice.CustomerName == "Select customer")
                {
                    Invoice.CustomerId = null;
                    Invoice.CustomerName = "Walk in";
                    Invoice.CustomerTempId = "";
                    Invoice.CustomerDetail = new CustomerDto_POS();
                }

                //Start #81875 #83886  Discrepancy in payment received and net sales. By Pratik
                if (Invoice.InvoiceHistories != null && Invoice.InvoiceHistories.Any()
                    && (Invoice.InvoiceHistories.LastOrDefault(x=>x.Status != InvoiceStatus.EmailSent).Status == InvoiceStatus.Parked
                    || Invoice.InvoiceHistories.LastOrDefault(x=>x.Status != InvoiceStatus.EmailSent).Status == InvoiceStatus.Quote
                    || Invoice.InvoiceHistories.LastOrDefault(x=>x.Status != InvoiceStatus.EmailSent).Status == InvoiceStatus.LayBy
                    || Invoice.InvoiceHistories.LastOrDefault(x=>x.Status != InvoiceStatus.EmailSent).Status == InvoiceStatus.OnAccount)
                    && Settings.CurrentRegister?.Registerclosure != null)
                {
                    Invoice.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id;
                    Invoice.RegisterId = Settings.CurrentRegister.Id;
                    Invoice.RegisterName = Settings.CurrentRegister.Name;
                    Invoice.InvoiceLineItems.ForEach(a => { a.RegisterId = Settings.CurrentRegister.Id; a.RegisterName = Settings.CurrentRegister.Name; a.RegisterClosureId = Settings.CurrentRegister.Registerclosure.Id; });
                }
                //End #81875 #83886 By Pratik


                //Ticket start:#48657 Empty Invoice Number.by rupesh
                //Ticket start:#70574.by rupesh
                if (string.IsNullOrEmpty(Invoice.Number) || Invoice.Number.StartsWith("-InProc"))
                {
                    //Ticket end:#70574.by rupesh
                    //Ticket end:#48657 .by rupesh
                    var Register = Settings.CurrentRegister;
                    //Ticket start:#22406 Quote sale by rupesh
                    if (Settings.IsQuoteSale)
                        Invoice.Number = Register.QuotePrefix + (Register.QuoteReceiptNumber + 1) + Register.QuoteSuffix;
                    else
                        Invoice.Number = Register.Prefix + (Register.ReceiptNumber + 1) + Register.Suffix;
                    //Ticket end:#22406 Quote sale by rupesh
                    Invoice.Barcode = Settings.CurrentRegister.Id.ToString() + Invoice.Number; //"#" + Invoice.Number;
                    Invoice.TransactionDate = Extensions.moment();
                    Invoice.FinalizeDate = Invoice.TransactionDate;

                    //update register invoice number 
                    outletService.UpdateLocalRegisterReceiptNumber(Settings.CurrentRegister.Id, Settings.SelectedOutletId);
                }
                else
                {
                    if (Invoice.LocalInvoiceStatus == LocalInvoiceStatus.Pending)
                    {
                        outletService.UpdateLocalRegisterReceiptNumber(Settings.CurrentRegister.Id, Settings.SelectedOutletId);
                        Invoice.LocalInvoiceStatus = LocalInvoiceStatus.Processing;

                    }

                    Invoice.FinalizeDate = Extensions.moment();
                    //Zoho Ticket: 8030

                    // Changes by jigar Ticket 8451
                    //if (invoiceStatus == null)
                    //    Invoice.TransactionDate = Extensions.moment();
                    // End Ticket 8451

                    // Start #73186 iPad  :iPad - Lay-by completion date option: Same as parked sale By Pratik
                    //Start #84330 On account Invoice created date got changed to completed date By Pratik
                    if (invoiceStatus == null && (Invoice.InvoiceHistories != null && Invoice.InvoiceHistories.Any()
                        && Invoice.InvoiceHistories.LastOrDefault(x => x.Status != InvoiceStatus.EmailSent).Status != InvoiceStatus.OnAccount))
                    {
                        //End #84330 By Pratik
                        Invoice.TransactionDate = Extensions.moment();
                    }
                    else if (invoiceStatus == Convert.ToString(InvoiceStatus.Parked))
                    {
                        if (Settings.StoreGeneralRule.HandleDateOfParkedSales)
                            Invoice.TransactionDate = Extensions.moment(Invoice.TransactionDate);
                        else
                            Invoice.TransactionDate = Extensions.moment();
                    }
                    else if (invoiceStatus == Convert.ToString(InvoiceStatus.LayBy))
                    {
                        if (Settings.StoreGeneralRule.HandleDateOfLayBySales)
                            Invoice.TransactionDate = Extensions.moment(Invoice.TransactionDate);
                        else
                            Invoice.TransactionDate = Extensions.moment();
                    }
                    //End Ticket 73186 By Pratik

                    //Ticket 14465 Date mismatch issue on receipt
                    if (Invoice.Status == InvoiceStatus.OnAccount)
                    {
                        if (Invoice != null && Invoice.InvoiceHistories != null)
                        {

                            var tempHistory = Invoice.InvoiceHistories.FirstOrDefault();

                            // Invoice.Status = InvoiceStatus.OnAccount;
                            if (tempHistory != null)
                            {
                                if (tempHistory.Status == InvoiceStatus.OnAccount)
                                {
                                    Invoice.TransactionDate = (DateTime)tempHistory.CreationTime;
                                }
                            }

                        }
                    }

                    //End Ticket 14465 

                }

                //Start #84985 iOS: onaccount outstanding not working properly on payment page by Pratik
                if (Invoice.InvoiceHistories != null && Invoice.InvoiceHistories.Any() &&
                    Invoice.InvoiceHistories.LastOrDefault(x=>x.Status != InvoiceStatus.EmailSent).Status == InvoiceStatus.OnAccount
                    && Invoice.Status == InvoiceStatus.Completed && Invoice?.CustomerDetail?.OutStandingBalance != null)
                {
                    Invoice.CustomerDetail.OutStandingBalance = Invoice.CustomerDetail.OutStandingBalance - Invoice.TotalPay;
                    customerService.UpdateLocalCustomer(Invoice.CustomerDetail);
                }
                //End #84985 by Pratik

                //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                if (Invoice.Status == InvoiceStatus.OnAccount && Invoice?.CustomerDetail?.OutStandingBalance != null)
                {
                    var val = Invoice.CustomerDetail.OutStandingBalance.Value;
                    Invoice.InvoiceOutstanding = new InvoiceOutstanding()
                    {
                        previousOutstanding = val - Invoice.OutstandingAmount,
                        currentSale = Invoice.OutstandingAmount,
                        currentOutstanding = val
                    };
                }
                //End Ticket #63876 by Pratik

                //Start ticket #76208 IOS:FR:Terms of payments by Pratik
                if (Invoice.Status == InvoiceStatus.OnAccount && Invoice?.CustomerDetail != null && Invoice?.CustomerDetail.InvoicesDueDays > 0 && !Invoice.InvoiceDueDate.HasValue)
                {
                    switch (Invoice.CustomerDetail.InvoicesDueType)
                    {
                        //Start #87922 Invoice missing processed from iPad. By Pratik
                        case InvoicesDueType.OfTheFollowingMonth:
                        case InvoicesDueType.DaysAfterTheEndOfTheInvoiceMonth:
                            var month = Invoice.TransactionDate.Month == 12 ? 1 : (Invoice.TransactionDate.Month + 1);
                            var year = Invoice.TransactionDate.Year;
                            if (Invoice.TransactionDate.Date.Month == 12)
                                year = year + 1;
                            Invoice.InvoiceDueDate = new DateTime(year, month, 1).AddDays(Invoice.CustomerDetail.InvoicesDueDays.Value - 1).ToStoreTime().ToStoreUTCTime();
                            break;
                        case InvoicesDueType.DaysAfterTheInvoiceDate:
                            Invoice.InvoiceDueDate = Invoice.TransactionDate.AddDays(Invoice.CustomerDetail.InvoicesDueDays.Value).ToStoreTime().ToStoreUTCTime();
                            break;
                        case InvoicesDueType.OfTheCurrentMonth:
                            var date1 = new DateTime(Invoice.TransactionDate.Year, Invoice.TransactionDate.Month, 1).AddDays(Invoice.CustomerDetail.InvoicesDueDays.Value - 1).ToStoreTime().ToStoreUTCTime();
                            Invoice.InvoiceDueDate =  date1;
                            break;
                        default:
                            break;
                        //End #87922 By Pratik
                    }
                }
                else if (Invoice.InvoiceDueDate.HasValue && Invoice.InvoiceDueDate.Value.Kind == DateTimeKind.Local)
                {
                    Invoice.InvoiceDueDate = Invoice.InvoiceDueDate.Value.ToStoreTime().ToStoreUTCTime();
                }
                //End ticket #76208 by Pratik

                //if (Invoice.Status != InvoiceStatus.LayBy || Invoice.Status != InvoiceStatus.Parked)
                //{
                //    Invoice.FinalizeDate = Extensions.moment();
                //}
               
                if (EnterSalePage.ServedBy == null)
                {
                    EnterSalePage.ServedBy = Settings.CurrentUser;

                }
                Invoice.ServedBy = EnterSalePage.ServedBy.Id;
                Invoice.ServedByName = EnterSalePage.ServedBy.FullName;

                //#97493 
                if (Invoice.Status == InvoiceStatus.Exchange)
                {
                    Invoice.IsExchangeSale = true;
                }
                //#97493 

                //Ticket #9972 Start : Exchange related issues. By Nikhil
                //decimal quantity = Invoice.InvoiceLineItems.Where(x => x.InvoiceItemType != InvoiceItemType.Discount).Sum(y => y.Quantity);
                //if(quantity==0)
                bool isCompleteBackOrder = Invoice.InvoiceLineItems.All(x => x.InvoiceItemType != InvoiceItemType.Discount && x.BackOrderQty > 0 && x.Quantity == 0);
                if (isCompleteBackOrder)
                {
                    //Ticket #9972 End : By Nikhil
                    //    isBackorder = true;
                    // Changes by jigar Ticket 8360
                    InvoiceBackOrder = new InvoiceDto();
                    var tmpinvoice = Invoice.Copy();
                    //var tmpinv = Invoice.MemberwiseClone());
                    var backorder = CreateBackOrder(tmpinvoice, offers, productService);
                    InvoiceBackOrder = backorder;
                    // End 8360

                    if (backorder != null)
                    {
                        await saleService.UpdateLocalInvoice(backorder);
                        ObservableCollection<InvoiceDto> backorders = new ObservableCollection<InvoiceDto>() { backorder };
                        var backorderresult = await saleService.UpdateLocalInvoiceThenSendItToServer(backorders);
                    }
                    else
                    {
                        Invoice.isSync = false;
                        ObservableCollection<InvoiceDto> Invoices = new ObservableCollection<InvoiceDto>() { Invoice };
                        await saleService.UpdateLocalInvoiceThenSendItToServer(Invoices);
                    }
                    //Ticket #9976 Start : Inventory Count On POS not updating for Back Order. By Nikhil
                    //Ticket start:#22406 Quote sale by rupesh
                    if (!Settings.IsQuoteSale)
                        await Task.Run(()=> MainThread.BeginInvokeOnMainThread(()=> { UpdateStock(Invoice, productService); }));
                    //Ticket end:#22406 by rupesh
                    //Ticket #9976 End : By Nikhil
                }
                else
                {

                    if (Invoice.InvoiceLineItems.Any(x => x.BackOrderQty != null && x.BackOrderQty.Value > 0))
                    {
                        var tmpinvoice = Invoice.Copy();
                        tmpinvoice.ReferenceTempInvoiceId = Invoice.InvoiceTempId;
                        tmpinvoice.InvoiceTempId = null;//Ticket:20064.There's something wrong with the display of the orders, when an order is split into a complete order and a back order.
                        var backorder = CreateBackOrder(tmpinvoice, offers, productService);
                        if (backorder != null)
                        {
                            Invoice.isSync = false;
                            Invoice.IsCustomerChange = false;
                            ObservableCollection<InvoiceDto> backorders = new ObservableCollection<InvoiceDto>() { Invoice, backorder };
                            await saleService.UpdateLocalInvoiceThenSendItToServer(backorders);

                        }
                        else
                        {
                            Invoice.isSync = false;
                            Invoice.IsCustomerChange = false;
                            ObservableCollection<InvoiceDto> Invoices = new ObservableCollection<InvoiceDto>() { Invoice };

                            Debug.WriteLine("Invoice before updatelocal 1 : " + Newtonsoft.Json.JsonConvert.SerializeObject(Invoice).ToString());

                            await saleService.UpdateLocalInvoiceThenSendItToServer(Invoices);
                        }
                        
                    }
                    else
                    {
                        Invoice.isSync = false;
                        Invoice.IsCustomerChange = false;
                        ObservableCollection<InvoiceDto> Invoices = new ObservableCollection<InvoiceDto>() { Invoice };

                        Debug.WriteLine("Invoice before updatelocal 2 : " + Newtonsoft.Json.JsonConvert.SerializeObject(Invoice).ToString());



                        await saleService.UpdateLocalInvoiceThenSendItToServer(Invoices);
                    }

                    //Ticket #9976 Start : Inventory Count On POS not updating for Back Order. By Nikhil
                    //Ticket start:#22406 Quote sale by rupesh
                    if (!Settings.IsQuoteSale)
                      await Task.Run(()=> MainThread.BeginInvokeOnMainThread(()=> { UpdateStock(Invoice, productService); }));
                    //Ticket end:#22406 by rupesh
                    //Ticket #9976 End : By Nikhil
                }

                if (Settings.CurrentRegister != null && Invoice.Id < 1 && Invoice.Status != InvoiceStatus.Refunded)// && Invoice.Status != InvoiceStatus.BackOrder)
                {

                    if (Invoice.Status != InvoiceStatus.BackOrder)
                    {
                        if (Settings.PrintCustomerEndingNumber > 0 && Settings.PrintCustomerEndingNumber != Settings.PrintCustomerStartingNumber)
                        {
                            if (Settings.PrintCustomerCurrentNumber == Settings.PrintCustomerEndingNumber)
                                Settings.PrintCustomerCurrentNumber = Settings.PrintCustomerStartingNumber;
                            else
                                Settings.PrintCustomerCurrentNumber = Settings.PrintCustomerCurrentNumber + 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                Logger.SaleLogger("Exception Msg while sending to server - " + ex.StackTrace);
            }

            if (Settings.IsQuoteSale)
            {
                Invoice.IscallFromPayment = true;
            }

            // Changes by jigar Ticket 8360
            if (InvoiceBackOrder != null)
            {
                return InvoiceBackOrder;
            }
            else
            {
                return Invoice;
            }
            // End 8360

        }


        public static async Task<InvoiceDto> UpdateCustomerParkedSaleOrder(InvoiceDto Invoice, SaleServices saleService, OutletServices outletService, ProductServices productService)
        {
            try
            {
                if (Invoice.CustomerId == null || (Invoice.CustomerId == 0 && string.IsNullOrEmpty(Invoice.CustomerTempId)) || Invoice.CustomerName == "Select customer")
                {
                    Invoice.CustomerId = null;
                    Invoice.CustomerName = "Walk in";
                    Invoice.CustomerTempId = "";
                    Invoice.CustomerDetail = new CustomerDto_POS();
                }
                if (Invoice.isSync)
                    Invoice.IsCustomerChange = true;

                ObservableCollection<InvoiceDto> Invoices = new ObservableCollection<InvoiceDto>() { Invoice };
                await saleService.UpdateLocalInvoiceThenSendItToServer(Invoices);
                //Invoice = await saleService.UpdateLocalInvoiceThenSendItToServer(Invoice);
            }
            catch (Exception ex)
            {
                ex.Track();
                //return Invoice;
            }
            return Invoice;
        }

        static void UpdateStock(InvoiceDto invoice, ProductServices productService)
        {
            try
            {
                if (invoice != null && invoice.InvoiceLineItems != null)
                {

                    var invoiceitems = invoice.InvoiceLineItems.Where(x => x.InvoiceItemType == InvoiceItemType.Standard || x.InvoiceItemType == InvoiceItemType.CompositeProduct || (x.InvoiceItemType == InvoiceItemType.Composite && x.InvoiceLineSubItems.Any()) || x.InvoiceItemType == InvoiceItemType.UnityOfMeasure).ToList();
                    foreach (var item in invoiceitems)
                    {
                        if (item.InvoiceItemType == InvoiceItemType.Standard)
                        {
                            if (invoice.Status != InvoiceStatus.Voided && item.IsReopenFromSaleHistory && item.ReopenQuantity == item.Quantity)
                                return;
                            else
                                item.IsReopenFromSaleHistory = false;

                            decimal qty = 0;
                            if (invoice.Status != InvoiceStatus.Voided && item.IsReopenFromSaleHistory)
                                qty = (item.Quantity - item.ReopenQuantity);
                            else
                                qty = item.Quantity;
                            if (qty == 0 && item.BackOrderQty > 0)
                                qty = item.BackOrderQty.Value;
                            updateProductStock(invoice, item.InvoiceItemValue, qty, productService);
                        }
                        else if ((item.InvoiceItemType == InvoiceItemType.CompositeProduct || item.InvoiceItemType == InvoiceItemType.Composite) && item.InvoiceLineSubItems.Any())
                        {
                            foreach (var productitem in item.InvoiceLineSubItems)
                            {
                                var checkvalidateQty = (productitem.Quantity * item.Quantity);
                                updateProductStock(invoice, productitem.ItemId, checkvalidateQty, productService);
                            }
                        }
                        else if (item.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
                        {
                            var qty = item.Quantity * item.InvoiceLineSubItems.FirstOrDefault().Quantity;
                            if (qty == 0 && item.BackOrderQty > 0)
                                qty = item.BackOrderQty.Value * item.InvoiceLineSubItems.FirstOrDefault().Quantity;
                            updateProductStock(invoice, item.InvoiceLineSubItems.FirstOrDefault().ItemId, qty, productService);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        static void updateProductStock(InvoiceDto invoice, int productId, decimal Quantity, ProductServices productService)
        {
            try
            {
                var product = productService.GetLocalProduct(productId);
                var mainVariantProductId = product.ParentId;
                if (product != null && product.TrackInventory)
                {
                    //&& invoice.Status != InvoiceStatus.Exchange added by rupesh for Ticket #9972
                    if ((invoice.Status != InvoiceStatus.Refunded && invoice.Status != InvoiceStatus.Exchange) || invoice.IsReStockWhenRefund || Quantity > 0)
                    {
                        //Ticket #9471 Start: Stock Issue while completing Layby sale. By Nikhil.
                        //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
                        //Ticket start:  #35108 iPad: back order quantity should be deducting after completing sale.by rupesh
                        bool doNotUpdateStock = invoice.InvoiceHistories.Any(x =>
                        {
                            return x.Status == InvoiceStatus.LayBy;
                            //|| x.Status == InvoiceStatus.BackOrder;
                        });
                        //Ticket end:#35108 iPad: back order quantity should be deducting after completing sale.by rupesh
                        //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales


                        //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
                        if (invoice.Status == InvoiceStatus.Voided)
                            doNotUpdateStock = false;
                        //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales


                        if (doNotUpdateStock)
                            return;
                        //Ticket #9471 End:By Nikhil.  
                        //using var realm = RealmService.GetRealm();
                        if (mainVariantProductId != null && mainVariantProductId > 0)
                        {
                            var tempVariantProduct =  productService.GetLocalProduct(mainVariantProductId.Value);
                            //if (tempVariantProduct != null && tempVariantProduct.ProductVarients != null && tempVariantProduct.HasVarients && tempVariantProduct.ProductVarients.Count > 0)
                            if (tempVariantProduct != null  && tempVariantProduct.HasVarients )

                                {
                                //    tempVariantProduct.ProductVarients.All(x =>
                                //{
                                //    if (x.ProductVarientId == product.Id)
                                //    {
                                //        if (x.VariantOutlet != null)
                                //        {
                                //           // realm.Write(() =>
                                //           // {
                                //                //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
                                //                if (invoice.Status == InvoiceStatus.Refunded || invoice.Status == InvoiceStatus.Voided)
                                //                    x.VariantOutlet.OnHandstock = x.VariantOutlet.OnHandstock + Quantity.ToPositive();
                                //                //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
                                //                else
                                //                    x.VariantOutlet.OnHandstock = x.VariantOutlet.OnHandstock - Quantity;
                                //            //});

                                //        }
                                //    }
                                //    return true;
                                //});
                                if (invoice.Status == InvoiceStatus.Refunded || invoice.Status == InvoiceStatus.Voided)
                                    tempVariantProduct.ProductOutlet.OnHandstock = tempVariantProduct.ProductOutlet.OnHandstock + Quantity.ToPositive();
                                //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
                                else
                                    tempVariantProduct.ProductOutlet.OnHandstock = tempVariantProduct.ProductOutlet.OnHandstock - Quantity;

                                productService.UpdateLocalProduct(tempVariantProduct);
                                productService.UpdateLocalProductOutlet(tempVariantProduct.ProductOutlet);
                                WeakReferenceMessenger.Default.Send(new Messenger.ProductStockChangeMessenger(tempVariantProduct));
                            }
                        }
                        //realm.Write(() =>
                       // {

                            //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
                            if (invoice.Status == InvoiceStatus.Refunded || invoice.Status == InvoiceStatus.Voided)
                                product.ProductOutlet.OnHandstock = product.ProductOutlet.OnHandstock + Quantity.ToPositive();
                            //#33951 iOS - Stock Not Deducted on POS Screen for Parked Sales
                            else
                                product.ProductOutlet.OnHandstock = product.ProductOutlet.OnHandstock - Quantity;
                       // });
                        productService.UpdateLocalProduct(product);
                        productService.UpdateLocalProductOutlet(product.ProductOutlet);
                        WeakReferenceMessenger.Default.Send(new Messenger.ProductStockChangeMessenger(product));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in updateProductStock : " + ex.Message + " : " + ex.StackTrace);
            }

        }


        public static bool CheckHasBackOrder(InvoiceDto invoice)
        {
            try
            {
                var backOrderinvoice = invoice.Copy();
                backOrderinvoice.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
                var isBackorder = false;

                foreach (var item in invoice.InvoiceLineItems)
                {
                    if (item.BackOrderQty != null && item.BackOrderQty > 0)
                    {
                        var invoiceItem = item.Copy();
                        invoiceItem.Quantity = invoiceItem.BackOrderQty.Value;
                        invoiceItem.BackOrderQty = 0;
                        var backOrderQuantity = invoiceItem.Quantity;
                        var res = "";
                        if (!string.IsNullOrEmpty(invoiceItem.Description))
                        {
                            var start = invoiceItem.Description.IndexOf("QuantityInBackOrder");
                            var end = invoiceItem.Description.IndexOf(".");

                            if (start < 0)
                                res = invoiceItem.Description;
                            else
                                res = invoiceItem.Description.Substring(end + 1, invoiceItem.Description.Length);
                        }

                        invoiceItem.Description = res;



                        backOrderinvoice.CustomerGroupDiscount = 0;
                        invoiceItem.CustomerGroupDiscountPercent = 0;

                        backOrderinvoice.InvoiceLineItems.Add(invoiceItem);
                        invoiceItem = CalculateLineItemTotal(invoiceItem, backOrderinvoice);
                        //Ticket start:#34436 iOS - Wrong Item Qty in BO.by rupesh
                        invoiceItem.Quantity = 0;
                        invoiceItem.BackOrderQty = backOrderQuantity;
                        //Ticket end:#34436 .by rupesh


                        isBackorder = true;
                    }
                }

                //if (invoice!=null && invoice.InvoiceLineItems!=null)
                //{
                //    isBackorder = invoice.InvoiceLineItems.Any(x=>x.BackOrderQty!=null && x.BackOrderQty > 0);     
                //}

                if (invoice.BackOrdertotalPaid == null)
                    invoice.BackOrdertotalPaid = 0;

                if (isBackorder)
                    invoice.BackOrdertotal = backOrderinvoice.NetAmount - invoice.BackOrdertotalPaid.Value;
                else
                    invoice.BackOrdertotal = 0;

                return isBackorder;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        static InvoiceDto CreateBackOrder(InvoiceDto invoice, ObservableCollection<OfferDto> offers, ProductServices productServices)
        {
            if (invoice.Status == InvoiceStatus.Refunded)
            {
                return null;
            }

            InvoiceDto BackOrderinvoice = new InvoiceDto();
            var isBackorderAsMainOrder = false;


            var isBackorder = true;

            foreach (var value in invoice.InvoiceLineItems)
            {
                if (value.Quantity == 0 && value.BackOrderQty != null && value.BackOrderQty > 0)
                {

                }
                else
                {
                    isBackorder = false;
                    break;
                }
            }


            if (isBackorder)
            {
                BackOrderinvoice = invoice.Copy();
                isBackorderAsMainOrder = true;
            }
            else
            {
                BackOrderinvoice = invoice.Copy();
                BackOrderinvoice.Note = "";
            }

            BackOrderinvoice.Status = InvoiceStatus.BackOrder;


            if (invoice.Id != 0)
                BackOrderinvoice.ReferenceInvoiceId = invoice.Id;


            BackOrderinvoice.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();
            if (!isBackorderAsMainOrder)
            {

                BackOrderinvoice.Id = 0;
                BackOrderinvoice.Number += "BO";
                BackOrderinvoice.Barcode = Settings.CurrentRegister.Id.ToString() + BackOrderinvoice.Number; // "#" + BackOrderinvoice.Number;

                BackOrderinvoice.RoundingAmount = 0;
                BackOrderinvoice.NetAmount = 0;
                BackOrderinvoice.TotalTender = 0;
                BackOrderinvoice.ChangeAmount = 0;
                BackOrderinvoice.TotalPaid = 0;
                BackOrderinvoice.CustomerGroupDiscount = 0;
                BackOrderinvoice.DiscountValue = 0;

                foreach (var value in invoice.InvoiceLineItems)
                {
                    if (value.BackOrderQty != null && value.BackOrderQty > 0)
                    {
                        isBackorder = true;
                        var invoiceItem = value.Copy();
                        invoiceItem.Quantity = value.BackOrderQty ?? 0;
                        invoiceItem.BackOrderQty = 0;
                        invoiceItem.Description = "";
                        invoiceItem.CustomerGroupDiscountPercent = 0;

                        var res = "";
                        if (!string.IsNullOrEmpty(invoiceItem.Description))
                        {
                            var start = invoiceItem.Description.IndexOf("QuantityInBackOrder");
                            var end = invoiceItem.Description.IndexOf(".");

                            if (start < 0)
                                res = invoiceItem.Description;
                            else
                                res = invoiceItem.Description.Substring(end + 1, invoiceItem.Description.Length);
                        }
                        invoiceItem.Description = res;


                        BackOrderinvoice.InvoiceLineItems.Add(invoiceItem);
                        CalculateLineItemTotal(invoiceItem, BackOrderinvoice);
                    }
                }

                BackOrderinvoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                foreach (var value in invoice.InvoicePayments)
                {
                    if (value.BackorderPayment != null && value.BackorderPayment > 0)
                    {

                        var invoicePayment = value.Copy();
                        invoicePayment.Amount = invoicePayment.BackorderPayment.Value;
                        BackOrderinvoice.InvoicePayments.Add(invoicePayment);
                    }
                }

                BackOrderinvoice.TotalTender = BackOrderinvoice.InvoicePayments.Sum(x => x.Amount);
                BackOrderinvoice.ChangeAmount = 0;
                BackOrderinvoice.TotalPaid = BackOrderinvoice.TotalTender;

            }
            else
            {
                BackOrderinvoice.CustomerGroupDiscount = 0;
                foreach (var value in invoice.InvoiceLineItems)
                {
                    if (value.BackOrderQty != null && value.BackOrderQty > 0)
                    {
                        value.CustomerGroupDiscountPercent = 0;
                        value.Quantity = value.BackOrderQty ?? 0;
                        value.BackOrderQty = 0;
                        value.Description = "";
                        BackOrderinvoice.InvoiceLineItems.Add(value);
                        CalculateLineItemTotal(value, BackOrderinvoice);
                    }
                }

                foreach (var value in BackOrderinvoice.InvoicePayments)
                {
                    if (value.BackorderPayment != null && value.BackorderPayment > 0)
                    {
                        value.Amount = value.BackorderPayment.Value;
                    }
                }

                BackOrderinvoice.TotalTender = BackOrderinvoice.InvoicePayments.Sum(x => x.Amount);
                BackOrderinvoice.ChangeAmount = 0;
                BackOrderinvoice.TotalPaid = BackOrderinvoice.TotalTender;

            }
            BackOrderinvoice = CalculateInvoiceTotal(BackOrderinvoice, offers, productServices);


            if (isBackorder)
                return BackOrderinvoice;
            else
                return null;
        }

        #region search/select customer methods
        public static async Task<InvoiceDto> CustomerOnSelectAsync(CustomerDto_POS customer, InvoiceDto Invoice, ObservableCollection<OfferDto> offers, ProductServices productService
            , TaxServices taxServices)
        {
            try
            {
                //Customer removed case
                //Ticket #13801 Wrong Opening Balance for Loyalty Points When Changing Customers.By Nikhil
                if (customer.Id == 0 && string.IsNullOrEmpty(customer.TempId))
                {
                    Invoice.CustomerCurrentLoyaltyPoints = 0;
                    Invoice.DeliveryAddressId = null;
                    Invoice.DeliveryAddress = null;
                }
                //Ticket #13801 End.By Nikhil

                //START Ticket #74344 iOS and WEB :: Discount Issue : FOR CustomerPricebookDiscount By pratik
                ////Ticket start:#54908 iPad: FR: Customer Tag Discount. by rupesh
                //if (Invoice.CustomerDetail?.APICustomerTags != null && Invoice.CustomerDetail?.APICustomerTags.Count > 0 && customer.Id == 0)
                //{
                //	foreach(var item in Invoice.InvoiceLineItems.ToList())
                //	{
                //		if(item.InvoiceItemType == InvoiceItemType.Discount)
                //		{
                //			Invoice.InvoiceLineItems.Remove(item);
                //		}
                //	}
                //}
                ////Ticket start:#54908 . by rupesh
                //End Ticket #74344 By pratik

                Invoice.CustomerId = customer.Id;
                Invoice.CustomerTempId = customer.TempId;

                Invoice.CustomerName = (string.IsNullOrEmpty(customer.FirstName) ? "" : (customer.FirstName.ToUppercaseFirstCharacter() + " ")) + (string.IsNullOrEmpty(customer.LastName) ? "" : customer.LastName.ToUppercaseFirstCharacter());
                //Invoice.companyName = app.utils.isNulltoString($model.companyName);
                //Ticket start:#29657 Seach customer using phone number.by rupesh
                Invoice.CustomerPhone = customer.Phone;
                //Ticket end:#29657 .by rupesh

                Invoice.CustomerDetail = customer;

                //if (Invoice.Status != InvoiceStatus.Refunded)
                //{
                Invoice.CustomerGroupId = null;
                Invoice.CustomerGroupName = "";
                Invoice.CustomerGroupDiscount = null;
                Invoice.CustomerGroupDiscountNote = "";

                Invoice.CustomerGroupDiscountNoteInside = "";
                Invoice.CustomerGroupDiscountNoteInsidePrice = 0;

                Invoice.CustomerGroupDiscountType = false;
                //} 

                Invoice.PriceListCustomerCurrentLoyaltyPoints = 0;

                //Ticket #9748 Start: Issue with back order deposit By Nikhil.
                var hasbackorder = CheckHasBackOrder(Invoice);
                if (hasbackorder && (Invoice.CustomerId == null
                    || Invoice.CustomerId == 0))
                {
                    Invoice.StrBackorderDeposite = string.Empty;
                }
                //Ticket #9748 End:By Nikhil. 
                
                if (customer.Id > 0 && customer.ToAllowForTaxExempt)
                {
                    var noTax = Extensions.GetNoTaxRecord(taxServices);


                    Invoice.InvoiceLineItems.ForEach(
                       x =>
                       {
                           if (x.InvoiceItemValue > 0)
                           {
                               if (x.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
                               {
                                   var product = productService.GetLocalUnitOfMeasureProduct(x.InvoiceItemValue);
                                   var soldPrice = product.ProductOutlet.PriceExcludingTax * product.ProductUnitOfMeasureDto.Qty;
                                   var discountedValue = product.ProductUnitOfMeasureDto.DiscountValue;
                                   if (product.ProductUnitOfMeasureDto.DiscountIsAsPercentage)
                                       discountedValue = GetValuefromPercent(soldPrice, product.ProductUnitOfMeasureDto.DiscountValue);

                                   x.RetailPrice = soldPrice - discountedValue;
                               }
                               else if (x.InvoiceItemType != InvoiceItemType.Discount)
                               {
                                   var product =  productService.GetLocalProductDB(x.InvoiceItemValue);
                                   x.RetailPrice = product.ProductOutlet.PriceExcludingTax;
                               }
                           }
                           else
                           {
                               x.RetailPrice = x.TaxExclusiveTotalAmount;
                           }
                           AddNoTaxToInvoiceLineItem(x, noTax);
                           x = CalculateLineItemTotal(x, Invoice,false);
                       });
                }
                else
                {
                    Invoice.InvoiceLineItems.ForEach(
                         x =>
                         {
                             if (x.InvoiceItemValue > 0)
                             {
                                 //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                                 if (x.InvoiceItemType == InvoiceItemType.UnityOfMeasure)
                                 {
                                     var product = productService.GetLocalUnitOfMeasureProduct(x.InvoiceItemValue);
                                     decimal soldPrice;
                                     if (Invoice.TaxInclusive)
                                     {
                                         soldPrice = product.ProductOutlet.SellingPrice * x.InvoiceLineSubItems.FirstOrDefault().Quantity;
                                     }
                                     else
                                     {
                                         soldPrice = product.ProductOutlet.PriceExcludingTax * x.InvoiceLineSubItems.FirstOrDefault().Quantity;
                                     }

                                     var discountedValue = product.ProductUnitOfMeasureDto.DiscountValue;
                                     if (product.ProductUnitOfMeasureDto.DiscountIsAsPercentage)
                                         discountedValue = GetValuefromPercent(soldPrice, product.ProductUnitOfMeasureDto.DiscountValue);
                                     x.RetailPrice = soldPrice - discountedValue;

                                     SetLineItemTaxes(x, product, taxServices);
                                 }
                                 //Ticket end:#20064 .by rupesh
                                 //Ticket start:#25376 iPad app crashes when adding customer.by rupesh
                                 else if (x.InvoiceItemType != InvoiceItemType.Discount)
                                 //Ticket end:#25376 .by rupesh
                                 {
                                     var product = productService.GetLocalProduct(x.InvoiceItemValue);
                                     if (Invoice.TaxInclusive)
                                         x.RetailPrice = product.ProductOutlet.SellingPrice;
                                     else
                                         x.RetailPrice = product.ProductOutlet.PriceExcludingTax;

                                     SetLineItemTaxes(x, product, taxServices);

                                 }
                             }
                             else if (x.InvoiceItemType != InvoiceItemType.GiftCard)
                             {
                                 var defaultTax = Extensions.GetDefaultTaxRecord(taxServices);
                                 x.TaxId = defaultTax.Id;
                                 x.TaxName = defaultTax.Name;
                                 x.TaxRate = defaultTax.Rate;
                                 if (x.CustomSaleRetailPrice != 0)
                                     x.RetailPrice = x.CustomSaleRetailPrice;
                                 foreach (var item in defaultTax.SubTaxes)
                                 {
                                     var subtax = new LineItemTaxDto
                                     {
                                         TaxId = item.Id,
                                         TaxRate = item.Rate,
                                         TaxName = item.Name
                                     };

                                     x.LineItemTaxes.Add(subtax);
                                 }
                             }
                             x = CalculateLineItemTotal(x, Invoice,false);
                         });
                }

                if (customer.CustomerGroupId != null && Invoice.Status != InvoiceStatus.Refunded)
                {
                    var customerGroup = customerService.GetLocalCustomerGroupById(customer.CustomerGroupId.Value);
                    if (customerGroup != null && customerGroup.Id != 0)
                    {
                        Invoice.CustomerGroupId = customerGroup.Id;

                        if (customerGroup.CustomerGroupDiscountType == 2)
                        {
                            Invoice.CustomerGroupDiscountNote = customerGroup.Name;
                            Invoice.CustomerGroupName = customerGroup.Name;
                            Invoice.CustomerGroupDiscountType = true;
                            Invoice.CustomerGroupDiscount = customerGroup.DiscountPercent;
                        }
                        //Ticket start:#74632 iOS: Add Flat Markup% to Customer Group Pricing (FR).by rupesh
                        else if (customerGroup.CustomerGroupDiscountType == 3)
                        {
                            Invoice.CustomerGroupDiscountType = false;
                            Invoice.CustomerGroupDiscount = 0;
                            Invoice.CustomerGroupDiscountNote = customerGroup.Name + ":" + customerGroup.DiscountPercent + "% " + "markup";
                            Invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + customerGroup.Name + ", " + "Item markup" + " " + customerGroup.DiscountPercent + "% ";
                            Invoice.CustomerGroupName = customerGroup.Name;
                        }
                        //Ticket end:#74632.by rupesh
                        else
                        {
                            Invoice.CustomerGroupDiscountType = false;
                            Invoice.CustomerGroupDiscount = customerGroup.DiscountPercent;
                            Invoice.CustomerGroupDiscountNote = customerGroup.Name + ":" + customerGroup.DiscountPercent + "% " + "off";
                            Invoice.CustomerGroupDiscountNoteInside = "Group" + " : " + customerGroup.Name + ", " + "Item discount" + " " + customerGroup.DiscountPercent + "% ";
                            Invoice.CustomerGroupName = customerGroup.Name;
                        }
                        if (Invoice.InvoiceLineItems != null && Invoice.Status != InvoiceStatus.Refunded)
                        {
                            
                            for (int i = 0; i < Invoice.InvoiceLineItems.Count(); i++)
                            {
                                Invoice.InvoiceLineItems[i].CustomerGroupDiscountPercent = Invoice.CustomerGroupDiscount;
                                //Ticket start:#74632 iOS: Add Flat Markup% to Customer Group Pricing (FR).by rupesh
                                CustomerMarkupDiscount(Invoice, Invoice.InvoiceLineItems[i], Invoice.CustomerDetail);
                                //Ticket end:#74632.by rupesh
                                //Ticket start:#22898 Composite sale not working properly.by rupesh.//addedInvoiceItemType == InvoiceItemType.CompositeProduct
                                //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
                                if (Invoice.CustomerGroupId != null && (Invoice.InvoiceLineItems[i].InvoiceItemType == InvoiceItemType.Standard || Invoice.InvoiceLineItems[i].InvoiceItemType == InvoiceItemType.CompositeProduct || Invoice.InvoiceLineItems[i].InvoiceItemType == InvoiceItemType.UnityOfMeasure))
                                //Ticket end:#20064.by rupesh
                                //Ticket end:#22898.by rupesh
                                {
                                    var hasProduct = productService.GetLocalProduct(Invoice.InvoiceLineItems[i].InvoiceItemValue);
                                    CustomerPricebookDiscount(Invoice, Invoice.InvoiceLineItems[i], Invoice.CustomerDetail);
                                    if (Invoice.Status != InvoiceStatus.Refunded)
                                    {
                                        //Ticket #13216 Discount Icon Shall Not Show When Creating BO.By Nikhil
                                        var lineItem = Invoice.InvoiceLineItems[i];
                                        if (lineItem.Quantity > 0)
                                            //Ticket #13216 End.By Nikhil

                                            //Pass false in Maui
                                            _= AddofferTolineItem(Invoice, offers, hasProduct, lineItem, productService, false);
                                    }
                                }
                                if (Invoice.InvoiceLineItems[i].InvoiceItemType != InvoiceItemType.Discount)
                                {
                                    CalculateLineItemTotal(Invoice.InvoiceLineItems[i], Invoice, false);
                                    //Comment CalculateInvoiceTotal in Maui
                                    //CalculateInvoiceTotal(Invoice, offers, productService);
                                }
                            }
                            //Add CalculateInvoiceTotal in Maui 
                            CalculateInvoiceTotal(Invoice, offers, productService);
                        }
                    }
                    else if(Invoice.InvoiceLineItems != null)
                    {
                        CalculateInvoiceTotal(Invoice, offers, productService);
                    }
                }
                else
                {
                    //Ticket start:#74344 iOS and WEB :: Discount Issue.by rupesh
                    if (Invoice.InvoiceLineItems != null && Invoice.Status != InvoiceStatus.Refunded && Invoice.Status != InvoiceStatus.Exchange)
                    {
                        //Ticket end:#74344 .by rupesh
                        var temp = Invoice.InvoiceLineItems.Where(a => a.InvoiceItemType != InvoiceItemType.Discount).Copy();

                        temp.ForEach(async item =>

                        {
                            InvoiceLineItemDto tempItem;
                            if (item.Id == 0)
                                tempItem = Invoice.InvoiceLineItems.First(x => x.InvoiceItemValue == item.InvoiceItemValue && x.InvoiceItemValueParent == item.InvoiceItemValueParent && x.Sequence == item.Sequence);
                            else
                                tempItem = Invoice.InvoiceLineItems.First(x => x.Id == item.Id);

                            if (tempItem != null)
                            {
                                tempItem.isDiscountAdded = 0;
                                tempItem.CustomerGroupDiscountPercent = Invoice.CustomerGroupDiscount;
                                var hasProduct = productService.GetLocalProduct(tempItem.InvoiceItemValue);
                                if (hasProduct != null)
                                {
                                    if (Invoice.Status != InvoiceStatus.Refunded && tempItem.InvoiceItemType != InvoiceItemType.Discount)
                                    {
                                        //Ticket #13216 Discount Icon Shall Not Show When Creating BO.By Nikhil
                                        if (tempItem.Quantity > 0)
                                        {
                                            tempItem.OfferDiscountPercent = null;
                                            //Ticket #13216 End.By Nikhil
                                            //Pass false in Maui
                                            _= AddofferTolineItem(Invoice, offers, hasProduct, tempItem, productService, false);
                                        }
                                        tempItem = CalculateLineItemTotal(tempItem, Invoice, false);
                                    }
                                }
                                else
                                {
                                    if (Invoice.Status != InvoiceStatus.Refunded && tempItem.InvoiceItemType != InvoiceItemType.Discount)
                                    {
                                        tempItem = CalculateLineItemTotal(tempItem, Invoice);
                                        //Comment CalculateInvoiceTotal in Maui
                                        //Invoice = CalculateInvoiceTotal(Invoice, offers, productService);
                                    }
                                }
                            }
                        });
                        //Add CalculateInvoiceTotal in Maui 
                        CalculateInvoiceTotal(Invoice, offers, productService);
                    }
                    else if(Invoice.InvoiceLineItems != null)
                    {
                        CalculateInvoiceTotal(Invoice, offers, productService);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return Invoice;

        }
        #endregion

        //Ticket start:#30959 iPad - New feature request :: Rule for discount offers when there are more than one offers applicable.by rupesh
        private static async Task<OfferDto> OpenOfferSelectPage(ObservableCollection<OfferDto> offers)
        {
            var tcs = new TaskCompletionSource<OfferDto>();
            var offerSelectPage = new OfferSelectPage();
            offerSelectPage.OfferSelected += (object sender, OfferDto e) =>
            {
                tcs.SetResult(e);
            };
            offerSelectPage.ViewModel.Offers = offers;
            offerSelectPage.ViewModel.SelectedOffer = offers.FirstOrDefault(x => x.IsSelected);
            await _navigationService.GetCurrentPage.Navigation.PushModalAsync(offerSelectPage);
            return await tcs.Task;
        }
        //Ticket end:#30959 .by rupesh

        public class Orderrequest
        {
            public decimal Price { get; set; }
            public int? CustomerGroupId { get; set; }
            public int OutletId { get; set; }
            public decimal Quantity { get; set; }
            public int ProductId { get; set; }
            public bool OnlycheckAvailability { get; set; }
        }
    }
}
