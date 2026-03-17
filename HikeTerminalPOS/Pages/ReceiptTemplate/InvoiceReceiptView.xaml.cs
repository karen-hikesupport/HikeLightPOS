using System;
using HikePOS.Models;
using HikePOS.ViewModels;
using System.Collections.ObjectModel;
using HikePOS.Helpers;
using System.Diagnostics;
using HikePOS.Enums;
using Newtonsoft.Json;
using HikePOS.Models.Payment;

namespace HikePOS
{
    public partial class InvoiceReceiptView : ScrollView
    {

        public string CustomerAddress { get; set; }
        public string CustomerFullAddress { get; set; }
        // double SaleListHeight;

        public InvoiceReceiptView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void UpdateDataFromSaleHistory(ReceiptTemplateDto receiptTemplate, CustomerDto_POS customer)
        {
            try
            {
                var ViewModel = ((ParkSaleDetailViewModel)this.BindingContext);

                //Start #84295 iOS - Feature:- Receipt Template Change Invoice Header Titles by Pratik
                if (!string.IsNullOrEmpty(ViewModel.CurrentReceiptTemplate.ItemTitleLabel))
                {
                    lblItem.Text = ViewModel.CurrentReceiptTemplate.ItemTitleLabel;
                }
                if (!string.IsNullOrEmpty(ViewModel.CurrentReceiptTemplate.PriceTitleLabel))
                {
                    lblPrice.Text = ViewModel.CurrentReceiptTemplate.PriceTitleLabel;
                }
                //End #84295 by Pratik

                //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                BoxOutstanding.IsVisible = false;
                GridOutstanding.IsVisible = false;
                if (ViewModel.CurrentReceiptTemplate.ShowOnAccountOutStadningOnReciept && !string.IsNullOrEmpty(ViewModel.CurrentReceiptTemplate.ToPayLable) && ViewModel.CurrentReceiptTemplate.ToPayLable.ToLower().Contains("outstanding"))
                {
                    CustomFieldsResponce result = null;
                    result = ViewModel.Invoice.CustomFields != null ? JsonConvert.DeserializeObject<CustomFieldsResponce>(ViewModel.Invoice.CustomFields) : null;
                    if (ViewModel.Invoice.Status == InvoiceStatus.OnAccount && result?.invoiceOutstanding != null || ViewModel.Invoice.InvoiceOutstanding != null)
                    {
                        var data = result?.invoiceOutstanding != null ? result.invoiceOutstanding : ViewModel.Invoice.InvoiceOutstanding;
                        BoxOutstanding.IsVisible = true;
                        GridOutstanding.IsVisible = true;
                        CurrentSale.Text = "Current Sale : " + (data.currentSale.HasValue ? data.currentSale.Value : decimal.Zero).ToString("C");
                        PreviousOutstanding.Text = "Previous Outstanding : " + (data.previousOutstanding.HasValue ? data.previousOutstanding.Value : decimal.Zero).ToString("C");
                        CurrentOutstanding.Text = "Current Outstanding : " + (data.currentOutstanding.HasValue ? data.currentOutstanding.Value : decimal.Zero).ToString("C");
                    }
                }
                //End Ticket #63876 by Pratik

                //start #63163 Voided Sale water mark by pratik
                imgvoidsale.IsVisible = ViewModel.Invoice.Status == InvoiceStatus.Voided;
                //end #63163 by pratik

                //Start ticket #76208 IOS:FR:Terms of payments by Pratik
                lblduedate.IsVisible = false;
                if (ViewModel.Invoice.Status == InvoiceStatus.OnAccount && ViewModel.Invoice.InvoiceDueDate.HasValue && receiptTemplate.ShowInvoiceDueDateOnReciept)
                {
                    lblduedate.IsVisible = true;
                    lblduedate.Text = "Due on: " + ViewModel.Invoice.InvoiceDueDate.Value.ToStoreTime().ToString("dd MMM yyyy");
                }
                //End ticket #76208 by Pratik

                // OutletEmail.Text = "";Added by rupesh
                if (ViewModel.CurrentReceiptTemplate.ShowOutletAddress)
                {
                    if (!string.IsNullOrEmpty(ViewModel.CurrentOutlet.Email))
                    {
                        OutletEmail.Text = "Email: " + ViewModel.CurrentOutlet.Email;
                        OutletEmail.IsVisible = true;
                    }
                    else
                    {
                        OutletEmail.Text = "";
                        OutletEmail.IsVisible = false;
                    }
                    if (!string.IsNullOrEmpty(ViewModel.CurrentOutlet.Phone))
                    {
                        OutletPhone.Text = "Phone: " + ViewModel.CurrentOutlet.Phone;
                        OutletPhone.IsVisible = true;
                    }
                    else
                    {
                        OutletPhone.Text = "";
                        OutletPhone.IsVisible = false;
                    }
                }


                //Start Ticket #84441 iOS: FR :add discount on receipt (Sale Invoice Receipt Upgradation) by Pratik
                lblTotalDiscount.IsVisible = false;
                if (ViewModel.CurrentReceiptTemplate.ShowTotalDiscountOnReciept)
                {
                    var totaldis = (ViewModel.Invoice.TotalDiscount + ViewModel.Invoice.InvoiceLineItems.Sum(a => a.TotalDiscount));
                    if (totaldis > 0)
                    {
                        lblTotalDiscount.IsVisible = true;
                        lblTotalDiscount.Text = "(" + LanguageExtension.Localize("TotalSaleDiscount") + " " + totaldis.ToString("C") + ")";
                    }
                }
                //End Ticket #84441 by Pratik



                ViewModel.Invoice.InvoicePayments1 = new ObservableCollection<InvoicePaymentDto>();
                if (ViewModel.Invoice.InvoicePayments == null)
                {
                    ViewModel.Invoice.InvoicePayments = new ObservableCollection<InvoicePaymentDto>();
                }
                foreach (var item in ViewModel.Invoice.InvoicePayments)
                {
                    string eConduitORAfterPayRefID = string.Empty; //item.InvoicePaymentDetails[0].Value;

                    if (item.PaymentOptionType == PaymentOptionType.Afterpay)
                    {
                        int ordernumber;
                        eConduitORAfterPayRefID = item.InvoicePaymentDetails[0].Value;

                        bool result1 = int.TryParse(eConduitORAfterPayRefID, out ordernumber);
                        if (!result1)
                        {
                            var afterPayvalue1 = JsonConvert.DeserializeObject<AfterPayResponseRootObject>(item.InvoicePaymentDetails[0].Value);
                            eConduitORAfterPayRefID = afterPayvalue1.Result.OrderId.ToString();
                            eConduitORAfterPayRefID = "C#" + eConduitORAfterPayRefID + ")";
                        }
                    }
                    //Added for zip same as reference Id
                    else if (item.PaymentOptionType == PaymentOptionType.Zip)
                    {
                        int ordernumber;
                        eConduitORAfterPayRefID = item.InvoicePaymentDetails[0].Value;

                        bool result1 = int.TryParse(eConduitORAfterPayRefID, out ordernumber);
                        if (!result1)
                        {
                            var zipPayvalue1 = JsonConvert.DeserializeObject<ZipResponeObject>(item.InvoicePaymentDetails[0].Value);
                            eConduitORAfterPayRefID = zipPayvalue1.Id.ToString();
                            eConduitORAfterPayRefID = "C#" + eConduitORAfterPayRefID + ")";
                        }
                    }


                    item.PrintPaymentOptionName = item.PaymentOptionName + eConduitORAfterPayRefID;


                    Debug.WriteLine("PrintPaymentOptionName : " + item.PrintPaymentOptionName);
                    ViewModel.Invoice.InvoicePayments1.Add(item);
                }


                //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                // PaymentListView.ItemsSource = ViewModel.Invoice.InvoicePayments1;
                BindableLayout.SetItemsSource(PaymentListView, ViewModel.Invoice.InvoicePayments1);
                /*if (PaymentListView.ItemsSource != null)
                {


                    //Changes By Jigar Ticket 8346
                    PaymentListViewHeight(ViewModel.Invoice.InvoicePayments);
                    //End 8346
                }*/
                //Ticket end: #62808 .by rupesh


                // Zoho 7241
                if (Settings.StoreShopDto.LogoImagePath != null && ViewModel.CurrentReceiptTemplate.Displaylogo)
                {
                    StoreNameLable.FontSize = 30;
                }
                else
                {
                    StoreNameLable.FontSize = 50;
                }

                //Ticket #11975 start:Receipts Printed from Sales History Not Printing Full Customer Info by rupesh
                //if (customer != null && ViewModel.Invoice.CustomerId != null && ViewModel.Invoice.CustomerId != 0 && ViewModel.ShopGeneralRule.EnableLoyalty && ViewModel.CurrentRegister.ReceiptTemplate.ShowLoyaltyPointsOnReciept)
                //{

                //if (ViewModel.CurrentRegister.ReceiptTemplate.ShowCustomerAddress)
                //Ticket start:#22406 Quote sale.by rupesh
                // if (customer != null && customer.Id != 0 && ViewModel.CurrentRegister.ReceiptTemplate.ShowCustomerAddress)
                //Ticket start:#67140 Customer Name Printout is Wrong on iPad .by Pratik
                LblCustomerName.Text = ViewModel.Invoice.CustomerName != "Walk in" ? ViewModel.Invoice.CustomerName : "";
                //Ticket end:#67140 .by Pratik
                if (customer != null && customer.Id != 0 && ViewModel.CurrentReceiptTemplate.ShowCustomerAddress)
                //Ticket end:#22406 .by rupesh
                {
                    //Ticket #10303 Start : In the invoice, company name should be displayed first and customer name below it. By Nikhil
                    if (!string.IsNullOrEmpty(customer.CompanyName)) 
                    {
                        if (receiptTemplate != null)
                            if (receiptTemplate.ShowCompanyNameInBillingInvoice)
                            {
                                //#32363 iPad :: Feature request :: Hide Customer Name or Company Name in Receipt
                                if (receiptTemplate.ShowCompanyName)
                                    LblCustomerName.Text = customer.CompanyName + "\n" + customer.FullName;
                                else
                                    LblCustomerName.Text = customer.FullName;
                            }
                            else
                            {
                                //#32363 iPad :: Feature request :: Hide Customer Name or Company Name in Receipt
                                if (receiptTemplate.ShowCompanyName)
                                    LblCustomerName.Text = customer.FullName + "\n" + customer.CompanyName;
                                else
                                    LblCustomerName.Text = customer.FullName;

                            }
                    }
                    else
                    {
                        //Ticket #11870 Added to resolve previous customer display issue by rupesh
                        LblCustomerName.Text = customer.FullName;
                    }

                    //Ticket #10303 End. By Nikhil

                    if (customer.Address != null && !string.IsNullOrEmpty(customer.Address.Address1))
                    {
                        LblCustomerAddress1.Text = customer.Address.Address1;
                        LblCustomerAddress1.IsVisible = true;
                    }
                    else
                    {
                        //Ticket #11870 Added to resolve previous customer display issue by rupesh
                        LblCustomerAddress1.Text = "";
                        LblCustomerAddress1.IsVisible = false;
                    }
                    if (customer.Address != null && !string.IsNullOrEmpty(customer.Address.FullAddress))
                    {
                        LblCustomerFullAddress.Text = customer.Address.FullAddress;
                        LblCustomerFullAddress.IsVisible = true;
                    }
                    else
                    {
                        LblCustomerFullAddress.Text = "";
                        LblCustomerFullAddress.IsVisible = false;
                    }
                    if (!string.IsNullOrEmpty(customer.Phone))
                    {
                        LblCustomerPhone.Text = "Phone: " + customer.Phone;
                        LblCustomerPhone.IsVisible = true;
                    }
                    else
                    {
                        LblCustomerPhone.Text = "";
                        LblCustomerPhone.IsVisible = false;
                    }
                    if (!string.IsNullOrEmpty(customer.Email))
                    {
                        LblCustomerEmail.Text = "Email: " + customer.Email;
                        LblCustomerEmail.IsVisible = true;
                    }
                    else
                    {
                        LblCustomerEmail.Text = "";
                        LblCustomerEmail.IsVisible = false;
                    }
                }
                else
                {
                    //Ticket start:#47822 iPad: Existing customer showing in print receipt.by rupesh
                    if (customer != null && customer.Id != 0 && ViewModel.CurrentReceiptTemplate.ShowCompanyName)
                    {
                        if (!string.IsNullOrEmpty(customer.CompanyName))
                        {
                            LblCustomerName.Text = customer.FullName + "\n" + customer.CompanyName;
                        }
                        //Ticket start:#67140 Customer Name Printout is Wrong on iPad .by Pratik
                        else
                        {
                            LblCustomerName.Text = customer.FullName;
                        }
                        //Ticket end:#67140 .by Pratik
                    }
                    //Ticket end:#47822 .by rupesh
                    //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
                    //Ticket start:#67140 Customer Name Printout is Wrong on iPad .by Pratik
                    //else
                    //{
                    //   LblCustomerName.Text = ViewModel.Invoice.CustomerName != "Walk in" ? ViewModel.Invoice.CustomerName : ""; 
                    //}
                    //Ticket end:#67140 .by Pratik
                    //Ticket end:#45648 .by rupesh
                    LblCustomerAddress1.IsVisible = false;
                    LblCustomerFullAddress.IsVisible = false;
                    LblCustomerPhone.IsVisible = false;
                    LblCustomerEmail.IsVisible = false;
                }

                //Ticket start:#22406 Quote sale.by rupesh
                // if (customer != null && ViewModel.Invoice.CustomerId != null && ViewModel.Invoice.CustomerId != 0 && ViewModel.ShopGeneralRule.EnableLoyalty && ViewModel.CurrentRegister.ReceiptTemplate.ShowLoyaltyPointsOnReciept)
                if (customer != null && ViewModel.Invoice.CustomerId != null && ViewModel.Invoice.CustomerId != 0 && ViewModel.ShopGeneralRule.EnableLoyalty && ViewModel.CurrentReceiptTemplate.ShowLoyaltyPointsOnReciept)
                //Ticket end:#22406 .by rupesh
                {
                    //Ticket #11975 End by rupesh

                    LoayltyBalance.Text = "Balance : " + ViewModel.Invoice.CustomerCurrentLoyaltyPoints.ToString();
                    if (ViewModel.Invoice.InvoiceLineItems != null)
                    {
                        LoyaltyPointsEarned.Text = "This visit - Earned : " +  (ViewModel.Invoice.LoyaltyPoints + ViewModel.Invoice.InvoiceLineItems.Sum(x => x.CustomerGroupLoyaltyPoints)).ToString();
                    }

                    decimal LoyaltyRedeemed = 0;
                    if (ViewModel.Invoice.InvoicePayments != null && ViewModel.Invoice.InvoicePayments.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty) && Settings.StoreGeneralRule != null)
                    {
                        LoyaltyRedeemed = ViewModel.Invoice.InvoicePayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty).Sum(x => x.Amount) * Settings.StoreGeneralRule.LoyaltyPointsValue;
                    }

                    txtLoyaltyRedeemed.Text = "This visit - Redeemed : " + "-" + LoyaltyRedeemed.ToString();
                    ClosingBalance.Text = "Closing balance : " + (ViewModel.Invoice.CustomerCurrentLoyaltyPoints + ViewModel.Invoice.LoyaltyPoints - LoyaltyRedeemed).ToString();

                    if (customer.AllowLoyalty)
                    {
                        BoxLoyalty.IsVisible = true;
                        GridLoyalty.IsVisible = true;
                    }
                    else
                    {
                        BoxLoyalty.IsVisible = false;
                        GridLoyalty.IsVisible = false;
                    }
                }
                else
                {
                    BoxLoyalty.IsVisible = false;
                    GridLoyalty.IsVisible = false;
                }
                //Ticket start:#58412 Print Store Credit on Receipt.by rupesh    
                if (customer != null && ViewModel.Invoice.CustomerId != null && ViewModel.Invoice.CustomerId != 0 && ViewModel.CurrentReceiptTemplate.ShowStoreCreditOnReciept)
                {

                    if (ViewModel.Invoice.InvoicePayments != null && ViewModel.Invoice.InvoicePayments.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Credit))
                    {
                        var storeCreditPayments = ViewModel.Invoice.InvoicePayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Credit);
                        var storeCreditUsed = storeCreditPayments.Sum(x => x.Amount);
                        StoreCreditUsed.Text = "This visit - Used : " + string.Format("{0:C}", storeCreditUsed);
                        if (storeCreditPayments.Last().InvoicePaymentDetails.Any())
                        {
                            StoreCreditOpeningBalance.Text = "Opening Balance : " + string.Format("{0:C}", Convert.ToDecimal(storeCreditPayments.First().InvoicePaymentDetails?.First().Value));
                            StoreCreditClosingBalance.Text = "Closing balance : " + string.Format("{0:C}", Convert.ToDecimal(storeCreditPayments.Last().InvoicePaymentDetails?.Last().Value));
                        }
                        BoxStoreCredit.IsVisible = true;
                        GridStoreCredit.IsVisible = true;
                    }
                    else
                    {
                        BoxStoreCredit.IsVisible = false;
                        GridStoreCredit.IsVisible = false;
                    }
                }
                else
                {
                    BoxStoreCredit.IsVisible = false;
                    GridStoreCredit.IsVisible = false;
                }
                //Ticket end:#58412 .by rupesh
                //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
                if (ViewModel.CurrentReceiptTemplate.ShowGiftCardOnReciept)
                    ShowUsedGiftCardBalanceTally(ViewModel.Invoice);
                else
                    GiftCardListView.IsVisible = false;
                //Ticket end:#94420.by rupesh
                //
                /*if (SaleItemListView.ItemsSource != null)
                {
                    var tmp = (ObservableCollection<InvoiceLineItemDto>)(SaleItemListView.ItemsSource);

                    int offercount = 0;
                    int descriptioncount = 0;

                    offercount = tmp.Count(x => !string.IsNullOrEmpty(x.OffersNote));
                    descriptioncount = tmp.Count(x => !string.IsNullOrEmpty(x.Description));

                    if (offercount >= 1 || descriptioncount >= 1)
                        SaleListHeight = ((tmp.Count * 110) + (offercount * 40) + (descriptioncount * 40));
                    else
                        SaleListHeight = ((tmp.Count * 110) + (10) + (10));
                }*/
                //Ticket start:#42157 Feature Request - iPad: show % value for invoice level Discount in print receipt.by rupesh
                if (Settings.StoreGeneralRule.ShowInvoiceLevelDiscountInPercentage)
                {
                    var disCountAsPercent = InvoiceCalculations.GetPercentfromValue(ViewModel.Invoice.TotalDiscount, ViewModel.Invoice.NetAmount + ViewModel.Invoice.TotalDiscount);
                    disCountAsPercent = Math.Round(disCountAsPercent, 2, MidpointRounding.AwayFromZero);
                    TotalDiscountAsPercent.Text = $"({disCountAsPercent}%)";
                    TotalDiscountAsPercent.IsVisible = true;

                }
                //Ticket end:#42157 .by rupesh
                // Changes by Jigar  ticket no 8335
                if (ViewModel.Invoice.TaxInclusive == true)
                {
                    SubTotalLable.SetValue(Grid.RowProperty, 1);
                    SubTotalValue.SetValue(Grid.RowProperty, 1);
                    DiscountLable.SetValue(Grid.RowProperty, 0);
                    TotalDiscount.SetValue(Grid.RowProperty, 0);
                }

                //Ticket start:#22406 Quote sale.by rupesh
                //Start #78676 by pratik
                if (ViewModel.Invoice.Status == InvoiceStatus.Quote || ViewModel.Invoice.Status == InvoiceStatus.Voided)
                {
                    //End #78676 by pratik
                    ToPayLable.IsVisible = false;
                    OutstandingAmountLable.IsVisible = false;
                }
                else
                {
                    //#33839 iOS - Outstanding Line Not Printed
                    ToPayLable.IsVisible = true;
                    OutstandingAmountLable.IsVisible = true;
                    //#33839 iOS - Outstanding Line Not Printed
                }
                //Ticket end:#22406 .by rupesh

                //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
                var texLabel = "Tax";
                if (receiptTemplate != null)
                    texLabel = receiptTemplate.TaxLable;
                // Ticket Start #63111 iOS: Separate line items for Tax Group format need to match with web By: Pratik
                var taxes = new ObservableCollection<LineItemTaxDto>((ViewModel.Invoice.ReceiptTaxList != null && ViewModel.Invoice.ReceiptTaxList.Count > 0) ? ViewModel.Invoice.ReceiptTaxList : ViewModel.Invoice.Taxgroup);
                var taxes1 = taxes?.Copy();
                if (taxes1 != null)
                    taxes = taxes1;
                //Ticket end: #63111 by Pratik
                if (ViewModel.Invoice.TotalTip.ToPositive() > 0)
                {
                    var hasTax = taxes?.FirstOrDefault(x => x.TaxId == ViewModel.Invoice.TipTaxId);
                    if (hasTax != null)
                    {
                        hasTax.TaxRate += ViewModel.Invoice.TipTaxRate.Value;
                        hasTax.TaxAmount += ViewModel.Invoice.TipTaxAmount.RoundingUptoTwoDecimal();
                    }
                    else
                    {
                        //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                        if (taxes!= null && !taxes.Any(a=>a.TaxId == ViewModel.Invoice.TipTaxId) && ViewModel.Invoice.TipTaxId != null && ViewModel.Invoice.TipTaxRate != null)
                        {
                            var tipTax = new LineItemTaxDto()
                            {
                                TaxId = ViewModel.Invoice.TipTaxId.Value,
                                TaxName = texLabel + "(" + ViewModel.Invoice.TipTaxName + ")",
                                TaxRate = ViewModel.Invoice.TipTaxRate.Value,
                                TaxAmount = ViewModel.Invoice.TipTaxAmount,
                                SubTaxes = new ObservableCollection<LineItemTaxDto>()
                            };
                            taxes.Add(tipTax);
                        }
                        //End ticket #73190  By Pratik

                    }
                }

                //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Rupesh
                if (ViewModel.Invoice.TipTaxId == null || ViewModel.Invoice.TipTaxId == 0)
                {
                    tipsLable.Text = ViewModel.CurrentReceiptTemplate.tipsLable + "(Inc.Tax)";
                }
                else
                {
                    tipsLable.Text = ViewModel.CurrentReceiptTemplate.tipsLable + "(Ex.Tax)";

                }
                //End Ticket #73190 By: Rupesh
                //ViewModel.Invoice.TotalTip = ViewModel.Invoice.TotalTip - ViewModel.Invoice.TipTaxAmount;
                //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total

                //Ticket start:#33812 iPad: New Feature Request :: Shipping charge showing Tax Exclusive in sales history page.by rupesh
                if (ViewModel.Invoice.TotalShippingCost.ToPositive() > 0)
                {
                    var hasTax = taxes?.FirstOrDefault(x => x.TaxId == ViewModel.Invoice.shippingTaxId.Value);
                    if (hasTax != null)
                    {
                        hasTax.TaxRate += ViewModel.Invoice.ShippingTaxRate.Value;
                        hasTax.TaxAmount += ViewModel.Invoice.ShippingTaxAmount.Value.RoundingUptoTwoDecimal();
                    }
                    else
                    {
                        if (ViewModel.Invoice.shippingTaxId.HasValue && taxes != null && !taxes.Any(a => a.TaxId == ViewModel.Invoice.shippingTaxId.Value))
                        {
                            var shipingTax = new LineItemTaxDto()
                            {
                                TaxId = ViewModel.Invoice.shippingTaxId.Value,
                                TaxName = texLabel + "(" + ViewModel.Invoice.ShippingTaxName + ")",
                                TaxRate = ViewModel.Invoice.ShippingTaxRate.Value,
                                TaxAmount = ViewModel.Invoice.ShippingTaxAmount.Value,
                                SubTaxes = new ObservableCollection<LineItemTaxDto>()
                            };
                            taxes.Add(shipingTax);
                        }
                        else if (ViewModel.Invoice.shippingTaxId.HasValue && taxes != null && taxes.Any(a => a.TaxId == ViewModel.Invoice.shippingTaxId.Value))
                        {
                            var taxdata = taxes.First(a => a.TaxId == ViewModel.Invoice.shippingTaxId.Value);
                            taxdata.TaxRate += ViewModel.Invoice.ShippingTaxRate.Value;
                            taxdata.TaxAmount += ViewModel.Invoice.ShippingTaxAmount.Value.RoundingUptoTwoDecimal();
                        }
                    }
                }
                if (ViewModel.Invoice.Taxgroup != null && ViewModel.Invoice.Taxgroup.Count > 0 && taxes!= null && taxes.Any())
                {
                    foreach (var item in ViewModel.Invoice.Taxgroup)
                    {
                        if (item.SubTaxes != null && item.SubTaxes.Count > 0 && taxes.FirstOrDefault(x => x.TaxId == item.TaxId) != null)
                        {
                            var existtax1 = taxes.FirstOrDefault(x => x.TaxId == item.TaxId);
                            existtax1.SubTaxes = item.SubTaxes;
                            var totalrate = item.SubTaxes.Sum(a => a.TaxRate);
                            foreach (var subitem in item.SubTaxes)
                            {
                                var existtax = taxes.FirstOrDefault(x => x.TaxId == subitem.TaxId && x.TaxName == subitem.TaxName);
                                if (existtax != null)
                                {
                                    existtax.TaxAmount = ((subitem.TaxRate * existtax1.TaxAmount) / totalrate).RoundingUptoTwoDecimal();
                                }
                            }
                        }
                    }
                }
                //Ticket end:#33812 .by rupesh

                //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                BindableLayout.SetItemsSource(TaxListView, taxes);
                //Ticket end: #62808 .by rupesh
                ServedByStackLayout.IsVisible= ViewModel.CurrentReceiptTemplate.showServedByOnReciept;
                UpdateLogoSize(receiptTemplate);

            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        private void PaymentListViewHeight(ObservableCollection<InvoicePaymentDto> InvoicePayments)
        {
            List<Printer> AvailablePrinter = Settings.GetCachePrinters.Where(x => x.PrimaryReceiptPrint == true).ToList();
            //Ticket starts #70775:The client wants to connect  usb scanner to mc3 print in ipad.by rupesh
            var mPOPPrinterConfigure = AvailablePrinter != null && AvailablePrinter.Any(x => (!string.IsNullOrEmpty(x.ModelName) && x.ModelName.Contains("POP")) || x.EnableUSBScanner);
            //var mPOPPrinterConfigure = AvailablePrinter != null && AvailablePrinter.Any();
            //Ticket end #70775.by rupesh
            double PaymentListHeight = 0;
            if (InvoicePayments != null)
            {
                foreach (var item in InvoicePayments)
                {
                    if (mPOPPrinterConfigure)
                    {
                        if (item.PaymentOptionType == Enums.PaymentOptionType.PayJunction
                        || item.PaymentOptionType == Enums.PaymentOptionType.EVOPayment
                        || item.PaymentOptionType == Enums.PaymentOptionType.VerifonePaymark
                        || item.PaymentOptionType == Enums.PaymentOptionType.eConduit
                        || item.PaymentOptionType == Enums.PaymentOptionType.NorthAmericanBankcard)
                        {
                            if (item.PaymentOptionName.Length > 40)
                            {
                                double PHeight = Convert.ToDouble(item.PaymentOptionName.Length) / 40;
                                PaymentListHeight += PHeight * 35;
                            }
                            else
                            {
                                PaymentListHeight += 70;
                            }
                        }
                        else
                        {
                            if (item.PaymentOptionName.Length > 18)
                            {
                                double PHeight = Convert.ToDouble(item.PaymentOptionName.Length) / 20;
                                PaymentListHeight += PHeight * 35;
                            }
                            else
                            {
                                PaymentListHeight += 40;
                            }
                        }
                    }
                    else if (!mPOPPrinterConfigure)
                    {
                        if (item.PaymentOptionType == Enums.PaymentOptionType.PayJunction
                        || item.PaymentOptionType == Enums.PaymentOptionType.EVOPayment
                        || item.PaymentOptionType == Enums.PaymentOptionType.VerifonePaymark
                        || item.PaymentOptionType == Enums.PaymentOptionType.eConduit
                        || item.PaymentOptionType == Enums.PaymentOptionType.NorthAmericanBankcard)
                        {
                            if (item.PaymentOptionName.Length > 80)
                            {
                                double PHeight = Convert.ToDouble(item.PaymentOptionName.Length) / 70;
                                PaymentListHeight += PHeight * 35;
                            }
                            else
                            {
                                PaymentListHeight += 70;
                            }
                        }
                        else
                        {
                            if (item.PaymentOptionName.Length > 35)
                            {
                                double PHeight = Convert.ToDouble(item.PaymentOptionName.Length) / 40;
                                PaymentListHeight += PHeight * 35;
                            }
                            else
                            {
                                PaymentListHeight += 40;
                            }
                        }
                    }
                }
            }
            PaymentListView.HeightRequest = PaymentListHeight + 10;
        }

        public void UpdateHtmlHeaderFooter(ReceiptTemplateDto ReceiptTemplate, Printer printer)
        {
            try
            {
                if (ReceiptTemplate != null)
                {
                    //Ticket start:#90943 iOS:FR Display Barcode or SKU instead of Product Name .by Pratik
                    if (this.BindingContext is PaymentViewModel paymentView)
                    {
                        BindableLayout.SetItemsSource(SaleItemListView, Extensions.SetDisplayTitle(paymentView.Invoice.InvoiceLineItems, paymentView.CurrentReceiptTemplate));
                    }
                    else if (this.BindingContext is ParkSaleDetailViewModel parkSaleDetailView)
                    {
                        BindableLayout.SetItemsSource(SaleItemListView, Extensions.SetDisplayTitle(parkSaleDetailView.Invoice.InvoiceLineItems, parkSaleDetailView.CurrentReceiptTemplate));
                    }
                    //Ticket end:#90943 by Pratik

                    //Ticket start:#43113 The big gap is shown in the delivery address print receipt.by rupesh
                    lblHeader.Text = "";
                    lblFooter.Text = "";
                    lbldumy.Text = "";
                    //Ticket end:#43113 .by rupesh
                    lblHeader.Text = CommonMethods.GetHtmlData(lblHeader, ReceiptTemplate.HeaderText);
                    lblFooter.Text = CommonMethods.GetHtmlData(lblFooter, ReceiptTemplate.FooterText);
                    lbldumy.Text = "<p>TEST</p>";
                    ChildStack.WidthRequest = printer.width;
                    UpdateLogoSize(ReceiptTemplate);

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        public async Task<bool> UpdateData(ImageSource imagesource, string invoiceNumber, ObservableCollection<InvoicePaymentDto> InvoicePayments, CustomerDto_POS customer, ReceiptTemplateDto CurrentReceiptTemplate, InvoiceDto invoice)
        {
            var tcs = new TaskCompletionSource<bool>();
           // BarcodeImage.Source = imagesource;
            InvoiceNumberLabel.Text = invoiceNumber;
            //Ticket start:#23536 iOS - MPOP Scanner Not Scanning Invoice Barcode Correctly.by rupesh
            BarcodeInvoiceNumberLabel.Text = invoice.Barcode;
            //Ticket end:#23536 .by rupesh

            //Start ticket #76208 IOS:FR:Terms of payments by Pratik
            lblduedate.IsVisible = false;
            if (invoice.Status == InvoiceStatus.OnAccount && invoice.InvoiceDueDate.HasValue && CurrentReceiptTemplate.ShowInvoiceDueDateOnReciept)
            {
                lblduedate.IsVisible = true;
                lblduedate.Text = "Due on: " + invoice.InvoiceDueDate.Value.ToStoreTime().ToString("dd MMM yyyy");

            }
            //End ticket by Pratik

            tcs.SetResult(true);
            ////Ticket start:#18615 iOS - Store Logo Not Printed. By rupesh
            ////Ticket start:#22068  image logo loading issue in offline.By rupesh
            //if (!Displaylogo.Source.IsEmpty && Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            ////Ticket end:#22068.by rupesh
            //{
            //    var img = await Displaylogo.GetImageAsPngAsync();
            //    if (img == null)
            //    {
            //        Displaylogo.Finish += async (sender, e) =>
            //        {

            //            img = await Displaylogo.GetImageAsPngAsync();
            //            if ((e.ScheduledWork.IsCompleted || e.ScheduledWork.IsCancelled) && !tcs.Task.IsCompleted)
            //                tcs?.SetResult(true);
            //        };
            //    }
            //    else
            //    {
            //        tcs.SetResult(true);

            //    }

            //}
            //else
            //{
            //    tcs.SetResult(true);

            //}
            ////Ticket end:#18615 . By rupesh

            var ViewModel = ((PaymentViewModel)this.BindingContext);

            if (ViewModel != null)
            {
                try
                {
                    //start #63163 Voided Sale water mark by pratik
                    imgvoidsale.IsVisible = ViewModel.Invoice.Status == InvoiceStatus.Voided;
                    //end #63163 by pratik
                    ViewModel.Invoice = invoice;
                    this.BindingContext = ViewModel;

                    //Start Ticket #84441 iOS: FR :add discount on receipt (Sale Invoice Receipt Upgradation) by Pratik
                    lblTotalDiscount.IsVisible = false;
                    if (ViewModel.CurrentReceiptTemplate.ShowTotalDiscountOnReciept)
                    {
                        var totaldis = (ViewModel.Invoice.TotalDiscount + ViewModel.Invoice.InvoiceLineItems.Sum(a => a.TotalDiscount));
                        if (totaldis > 0)
                        {
                            lblTotalDiscount.IsVisible = true;
                            lblTotalDiscount.Text = "("+ LanguageExtension.Localize("TotalSaleDiscount") + " " + totaldis.ToString("C") + ")";
                        }
                    }
                    //End Ticket #84441 by Pratik

                    //Start #84295 iOS - Feature:- Receipt Template Change Invoice Header Titles by Pratik
                    if (!string.IsNullOrEmpty(ViewModel.CurrentReceiptTemplate.ItemTitleLabel))
                    {
                        lblItem.Text = ViewModel.CurrentReceiptTemplate.ItemTitleLabel;
                    }
                    if (!string.IsNullOrEmpty(ViewModel.CurrentReceiptTemplate.PriceTitleLabel))
                    {
                        lblPrice.Text = ViewModel.CurrentReceiptTemplate.PriceTitleLabel;
                    }
                    //End #84295 by Pratik

                    //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                    BoxOutstanding.IsVisible = false;
                    GridOutstanding.IsVisible = false;

                    if (ViewModel.CurrentReceiptTemplate.ShowOnAccountOutStadningOnReciept && !string.IsNullOrEmpty(CurrentReceiptTemplate.ToPayLable) && CurrentReceiptTemplate.ToPayLable.ToLower().Contains("outstanding"))
                    {
                        CustomFieldsResponce result = null;
                        result = ViewModel.Invoice.CustomFields != null ? JsonConvert.DeserializeObject<CustomFieldsResponce>(ViewModel.Invoice.CustomFields) : null;
                        if (ViewModel.Invoice.OutstandingAmount > 0 && ViewModel.Invoice.Status == InvoiceStatus.OnAccount && result?.invoiceOutstanding != null || ViewModel.Invoice.InvoiceOutstanding != null)
                        {
                            var data = ViewModel.Invoice.InvoiceOutstanding != null ? ViewModel.Invoice.InvoiceOutstanding : result.invoiceOutstanding;
                            BoxOutstanding.IsVisible = true;
                            GridOutstanding.IsVisible = true;
                            CurrentSale.Text = "Current Sale : " + (data.currentSale.HasValue ? data.currentSale.Value : decimal.Zero).ToString("C");
                            PreviousOutstanding.Text = "Previous Outstanding : " + (data.previousOutstanding.HasValue ? data.previousOutstanding.Value : decimal.Zero).ToString("C");
                            CurrentOutstanding.Text = "Current Outstanding : " + (data.currentOutstanding.HasValue ? data.currentOutstanding.Value : decimal.Zero).ToString("C");
                        }
                    }
                    //End Ticket #63876 by Pratik

                    //Ticket #10865 Start : Improper Receipt Layout From Auto-print. By Nikhil
                    //Following line commented to solve issue.
                    //InitializeComponent(); 
                    //Ticket #10865 End : By Nikhil 

                    //Moved from android.By Rupesh.
                    if (ViewModel.CurrentReceiptTemplate.ShowOutletAddress)
                    {
                        if (!string.IsNullOrEmpty(ViewModel.CurrentOutlet.Email))
                        {
                            OutletEmail.Text = "Email: " + ViewModel.CurrentOutlet.Email;
                            OutletEmail.IsVisible = true;
                        }
                        else
                        {
                            OutletEmail.Text = "";
                            OutletEmail.IsVisible = false;
                        }
                        if (!string.IsNullOrEmpty(ViewModel.CurrentOutlet.Phone))
                        {
                            OutletPhone.Text = "Phone: " + ViewModel.CurrentOutlet.Phone;
                            OutletPhone.IsVisible = true;
                        }
                        else
                        {
                            OutletPhone.Text = "";
                            OutletPhone.IsVisible = false;
                        }
                    }
                    //End.by Rupesh.

                    //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                    //PaymentListView.ItemsSource = invoice.InvoicePayments;
                    BindableLayout.SetItemsSource(TaxListView, invoice.InvoicePayments);
                    /* if (PaymentListView.ItemsSource != null)
                     {
                         PaymentListViewHeight(invoice.InvoicePayments);
                     }*/
                    //Ticket end: #62808 .by rupesh

                    var texLabel = "Tax";
                    if (CurrentReceiptTemplate != null)
                        texLabel = CurrentReceiptTemplate.TaxLable;
                    var taxes = new ObservableCollection<LineItemTaxDto>();

                    // Changes by Jigar  ticket no 8335
                    var TotalEffectiveamount = invoice.InvoiceLineItems.Sum(x => x.EffectiveAmount);
                    decimal discountPercentValue = 0;
                    if (!invoice.DiscountIsAsPercentage)
                    {
                        var total = (invoice.Status == InvoiceStatus.Refunded ? TotalEffectiveamount.ToPositive() : TotalEffectiveamount);
                        if (total != 0)
                            discountPercentValue = (invoice.DiscountValue * 100) / total;
                    }
                    else
                    {
                        discountPercentValue = invoice.DiscountValue;
                    }

                    //Start Ticket #73665 iOS: Group Taxes not matched in iOS for Prints from POS Screen and Same print from Sales history by pratik
                    //invoice.Taxgroup = InvoiceCalculations.GetTaxgroup(invoice, discountPercentValue);
                    invoice.Taxgroup = InvoiceCalculations.GetTaxgroupForprint(invoice, discountPercentValue);
                    //End Ticket #73665 by pratik

                    // Changes by Jigar  ticket no 8335


                    // Ticket Start #63111 iOS: Separate line items for Tax Group format need to match with web By: Pratik
                    foreach (var item in invoice.Taxgroup.OrderBy(a => a.IsGroupTax))
                    // Ticket end #63111 By: Pratik
                    {

                        if (item.SubTaxes.Count > 0)
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
                            //item.TaxAmount = invoice.TotalTax;
                            taxes.Add(item);
                        }
                    }

                    var taxes1 = taxes.Copy();
                    if (taxes1 != null)
                        taxes = taxes1;

                    //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
                    // invoice.TotalTip = invoice.TotalTip - invoice.TipTaxAmount;
                    //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total

                    //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
                    if (invoice.TotalTip.ToPositive() > 0)
                    {
                        var hasTax = taxes.FirstOrDefault(x => x.TaxId == invoice.TipTaxId);
                        if (hasTax != null)
                        {
                            hasTax.TaxRate += invoice.TipTaxRate.Value;
                            hasTax.TaxAmount += invoice.TipTaxAmount.RoundingUptoTwoDecimal();
                        }
                        else
                        {
                            //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Pratik
                            if (taxes != null && !taxes.Any(a => a.TaxId == ViewModel.Invoice.TipTaxId) && invoice.TipTaxId != null && invoice.TipTaxRate != null)
                            {
                                var tipTax = new LineItemTaxDto()
                                {
                                    TaxId = invoice.TipTaxId.Value,
                                    TaxName = texLabel + "(" + invoice.TipTaxName + ")",
                                    TaxRate = invoice.TipTaxRate.Value,
                                    TaxAmount = invoice.TipTaxAmount,
                                    SubTaxes = new ObservableCollection<LineItemTaxDto>()
                                };
                                taxes.Add(tipTax);
                            }
                            //End ticket #73190 By Pratik
                        }
                    }
                    //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total

                    //Ticket start:#33812 iPad: New Feature Request :: Shipping charge showing Tax Exclusive in sales history page.by rupesh
                    if (invoice.TotalShippingCost.ToPositive() > 0)
                    {
                        var hasTax = taxes.FirstOrDefault(x => x.TaxId == invoice.shippingTaxId.Value);
                        if (hasTax != null)
                        {
                            hasTax.TaxRate += invoice.ShippingTaxRate.Value;
                            hasTax.TaxAmount += invoice.ShippingTaxAmount.Value.RoundingUptoTwoDecimal();
                        }
                        else
                        {
                            if (ViewModel.Invoice.shippingTaxId.HasValue && taxes != null && !taxes.Any(a => a.TaxId == ViewModel.Invoice.shippingTaxId.Value))
                            {
                                var shipingTax = new LineItemTaxDto()
                                {
                                    TaxId = invoice.shippingTaxId.Value,
                                    TaxName = texLabel + "(" + invoice.ShippingTaxName + ")",
                                    TaxRate = invoice.ShippingTaxRate.Value,
                                    TaxAmount = invoice.ShippingTaxAmount.Value,
                                    SubTaxes = new ObservableCollection<LineItemTaxDto>()
                                };
                                taxes.Add(shipingTax);
                            }
                            else if (ViewModel.Invoice.shippingTaxId.HasValue && taxes != null && taxes.Any(a => a.TaxId == ViewModel.Invoice.shippingTaxId.Value))
                            {
                                var taxdata = taxes.First(a => a.TaxId == ViewModel.Invoice.shippingTaxId.Value);
                                taxdata.TaxRate += ViewModel.Invoice.ShippingTaxRate.Value;
                                taxdata.TaxAmount += ViewModel.Invoice.ShippingTaxAmount.Value.RoundingUptoTwoDecimal();
                            }
                        }
                    }
                    //Ticket end:#33812 .by rupesh

                    if (invoice.Taxgroup != null && invoice.Taxgroup.Count > 0 && taxes.Any())
                    {
                        foreach (var item in ViewModel.Invoice.Taxgroup)
                        {
                            if (item.SubTaxes != null && item.SubTaxes.Count > 0 && taxes.FirstOrDefault(x => x.TaxId == item.TaxId) != null)
                            {
                                var existtax1 = taxes.FirstOrDefault(x => x.TaxId == item.TaxId);
                                existtax1.SubTaxes = item.SubTaxes;
                                var totalrate = item.SubTaxes.Sum(a => a.TaxRate);
                                foreach (var subitem in item.SubTaxes)
                                {
                                    var existtax = taxes.FirstOrDefault(x => x.TaxId == subitem.TaxId && x.TaxName == subitem.TaxName);
                                    if (existtax != null)
                                    {
                                        existtax.TaxAmount = ((subitem.TaxRate * existtax1.TaxAmount) / totalrate).RoundingUptoTwoDecimal();
                                    }
                                }
                            }
                        }
                    }

                    //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                    //TaxListView.ItemsSource = taxes;
                    BindableLayout.SetItemsSource(TaxListView, taxes);
                    /*if (TaxListView.ItemsSource != null)
                    {
                        var tmp = (IList)TaxListView.ItemsSource;
                        TaxListView.HeightRequest = tmp.Count * (TaxListView.RowHeight+15);
                    }*/
                    //Ticket end: #62808.by rupesh

                    //Ticket start:#45367 iPad: FR - Group Products by Category on POS.by rupesh
                    //   SaleItemListView.ItemsSource = invoice.InvoiceLineItems;
                    //Ticket end:#45367 .by rupesh

                    //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                    //if (SaleItemListView.ItemsSource != null)
                    //{
                    //    ViewModel.calculateInvoiceHeight();

                    //}
                    //Ticket end: #62808 .by rupesh

                    //Ticket start:#22406 Quote sale.by rupesh
                    //if (customer != null && customer.Id != 0 && ViewModel.CurrentRegister.ReceiptTemplate.ShowCustomerAddress)
                    if (customer != null && customer.Id != 0 && ViewModel.CurrentReceiptTemplate.ShowCustomerAddress)
                    //Ticket end:#22406 Quote sale.by rupesh
                    {
                        //Ticket #10303 Start : In the invoice, company name should be displayed first and customer name below it. By Nikhil
                        if (!string.IsNullOrEmpty(customer.CompanyName))
                        {
                            if (CurrentReceiptTemplate != null)

                                if (CurrentReceiptTemplate.ShowCompanyNameInBillingInvoice)
                                {
                                    //#32363 iPad :: Feature request :: Hide Customer Name or Company Name in Receipt
                                    if (CurrentReceiptTemplate.ShowCompanyName)
                                        LblCustomerName.Text = customer.CompanyName + "\n" + customer.FullName;
                                    else
                                        LblCustomerName.Text = customer.FullName;
                                }
                                else
                                {
                                    //#32363 iPad :: Feature request :: Hide Customer Name or Company Name in Receipt
                                    if (CurrentReceiptTemplate.ShowCompanyName)
                                        LblCustomerName.Text = customer.FullName + "\n" + customer.CompanyName;
                                    else
                                        LblCustomerName.Text = customer.FullName;

                                }
                        }
                        else
                        {
                            LblCustomerName.Text = customer.FullName;

                        }
                        //Ticket #10303 End. By Nikhil

                        if (customer.Address != null && !string.IsNullOrEmpty(customer.Address.Address1))
                        {
                            LblCustomerAddress1.Text = customer.Address.Address1;
                            LblCustomerAddress1.IsVisible = true;
                        }
                        //Moved from android . By rupesh.
                        else
                        {
                            LblCustomerAddress1.Text = "";
                            LblCustomerAddress1.IsVisible = false;
                        }

                        if (customer.Address != null && !string.IsNullOrEmpty(customer.Address.FullAddress))
                        {
                            LblCustomerFullAddress.Text = customer.Address.FullAddress;
                            LblCustomerFullAddress.IsVisible = true;
                        }
                        else
                        {
                            LblCustomerFullAddress.Text = "";
                            LblCustomerFullAddress.IsVisible = false;
                        }
                        if (!string.IsNullOrEmpty(customer.Phone))
                        {
                            LblCustomerPhone.Text = "Phone: " + customer.Phone;
                            LblCustomerPhone.IsVisible = true;
                        }
                        else
                        {
                            LblCustomerPhone.Text = "";
                            LblCustomerPhone.IsVisible = false;
                        }
                        if (!string.IsNullOrEmpty(customer.Email))
                        {
                            LblCustomerEmail.Text = "Email: " + customer.Email;
                            LblCustomerEmail.IsVisible = true;
                        }
                        else
                        {
                            LblCustomerEmail.Text = "";
                            LblCustomerEmail.IsVisible = false;
                        }
                        //Moved from android.By rupesh.
                    }
                    else
                    {
                        //Ticket start:#47822 iPad: Existing customer showing in print receipt.by rupesh
                        if (customer != null && customer.Id != 0 && ViewModel.CurrentReceiptTemplate.ShowCompanyName)
                        {
                            if (!string.IsNullOrEmpty(customer.CompanyName))
                            {
                                LblCustomerName.Text = customer.FullName + "\n" + customer.CompanyName;
                            }
                            //Ticket start:#56178 Receipts are getting printed with the customer name of previous sale.by rupesh
                            else
                            {
                                LblCustomerName.Text = customer.FullName;
                            }
                            //Ticket end:#56178 .by rupesh
                        }
                        //Ticket end:#47822 .by rupesh
                        //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
                        else
                        {
                            LblCustomerName.Text = ViewModel.Invoice.CustomerName != "Walk in" ? ViewModel.Invoice.CustomerName : "";
                        }
                        //Ticket end:#45648 .by rupesh

                        LblCustomerAddress1.IsVisible = false;
                        LblCustomerFullAddress.IsVisible = false;
                        LblCustomerPhone.IsVisible = false;
                        LblCustomerEmail.IsVisible = false;
                    }

                    //Ticket start:#22406 Quote sale.by rupesh
                    // if (customer != null && customer.AllowLoyalty && ViewModel.Invoice.CustomerId != null && ViewModel.Invoice.CustomerId != 0 && ViewModel.ShopGeneralRule.EnableLoyalty && ViewModel.CurrentRegister.ReceiptTemplate.ShowLoyaltyPointsOnReciept)
                    if (customer != null && customer.AllowLoyalty && ViewModel.Invoice.CustomerId != null && ViewModel.Invoice.CustomerId != 0 && ViewModel.ShopGeneralRule.EnableLoyalty && ViewModel.CurrentReceiptTemplate.ShowLoyaltyPointsOnReciept)
                    //Ticket end:#22406 .by rupesh
                    {
                        //Start #85382 iOS: Loyalty point print calculation issue By Pratik
                        var loyaltybal = customer.CurrentLoyaltyBalance > ViewModel.Invoice.CustomerCurrentLoyaltyPoints ? customer.CurrentLoyaltyBalance : ViewModel.Invoice.CustomerCurrentLoyaltyPoints;
                        LoayltyBalance.Text = "Balance : " + loyaltybal.ToString();
                       
                        if (ViewModel.Invoice.InvoiceLineItems != null)
                        {
                            LoyaltyPointsEarned.Text = "This visit - Earned : " + (ViewModel.Invoice.LoyaltyPoints + ViewModel.Invoice.InvoiceLineItems.Sum(x => x.CustomerGroupLoyaltyPoints)).ToString();
                        }

                        decimal LoyaltyRedeemed = 0;
                        if (ViewModel.Invoice.InvoicePayments != null && ViewModel.Invoice.InvoicePayments.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty))
                        {
                            LoyaltyRedeemed = ViewModel.Invoice.InvoicePayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Loyalty).Sum(x => x.Amount) * Settings.StoreGeneralRule.LoyaltyPointsValue;
                        }

                        txtLoyaltyRedeemed.Text = "This visit - Redeemed : " + "-" + LoyaltyRedeemed;
                        ClosingBalance.Text = "Closing balance : " + (loyaltybal + ViewModel.Invoice.LoyaltyPoints - LoyaltyRedeemed).ToString();

                        BoxLoyalty.IsVisible = true;
                        GridLoyalty.IsVisible = true;
                        //End #85382 By Pratik
                    }
                    else
                    {
                        BoxLoyalty.IsVisible = false;
                        GridLoyalty.IsVisible = false;
                    }

                    //Ticket start:#58412 Print Store Credit on Receipt.by rupesh    
                    if (customer != null && ViewModel.Invoice.CustomerId != null && ViewModel.Invoice.CustomerId != 0 && ViewModel.CurrentReceiptTemplate.ShowStoreCreditOnReciept)
                    {

                        if (ViewModel.Invoice.InvoicePayments != null && ViewModel.Invoice.InvoicePayments.Any(x => x.PaymentOptionType == Enums.PaymentOptionType.Credit))
                        {
                            var storeCreditPayments = ViewModel.Invoice.InvoicePayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Credit);
                            var storeCreditUsed = storeCreditPayments.Sum(x => x.Amount);
                            StoreCreditUsed.Text = "This visit - Used : " + string.Format("{0:C}", storeCreditUsed);
                            if (storeCreditPayments.Last().InvoicePaymentDetails.Any())
                            {
                                StoreCreditOpeningBalance.Text = "Opening Balance : " + string.Format("{0:C}", Convert.ToDecimal(storeCreditPayments.First().InvoicePaymentDetails?.First().Value));
                                StoreCreditClosingBalance.Text = "Closing balance : " + string.Format("{0:C}", Convert.ToDecimal(storeCreditPayments.Last().InvoicePaymentDetails?.Last().Value));
                            }
                            BoxStoreCredit.IsVisible = true;
                            GridStoreCredit.IsVisible = true;

                        }
                        else
                        {
                            BoxStoreCredit.IsVisible = false;
                            GridStoreCredit.IsVisible = false;
                        }
                    }
                    else
                    {
                        BoxStoreCredit.IsVisible = false;
                        GridStoreCredit.IsVisible = false;
                    }
                    //Ticket end:#58412 .by rupesh    
                    //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
                    if (ViewModel.CurrentReceiptTemplate.ShowGiftCardOnReciept)
                        ShowUsedGiftCardBalanceTally(ViewModel.Invoice);
                    else
                        GiftCardListView.IsVisible = false;
                    //Ticket edn:#94420.by rupesh
                    //Ticket start:#42157 Feature Request - iPad: show % value for invoice level Discount in print receipt.by rupesh
                    if (Settings.StoreGeneralRule.ShowInvoiceLevelDiscountInPercentage)
                    {
                        var disCountAsPercent = InvoiceCalculations.GetPercentfromValue(ViewModel.Invoice.TotalDiscount, ViewModel.Invoice.NetAmount + ViewModel.Invoice.TotalDiscount);
                        disCountAsPercent = Math.Round(disCountAsPercent, 2, MidpointRounding.AwayFromZero);
                        TotalDiscountAsPercent.Text = $"({disCountAsPercent}%)";
                        TotalDiscountAsPercent.IsVisible = true;

                    }
                    //Ticket end:#42157 .by rupesh
                    // Changes by Jigar  ticket no 8335
                    if (ViewModel.Invoice.TaxInclusive == true)
                    {
                        SubTotalLable.SetValue(Grid.RowProperty, 1);
                        SubTotalValue.SetValue(Grid.RowProperty, 1);
                        DiscountLable.SetValue(Grid.RowProperty, 0);
                        TotalDiscount.SetValue(Grid.RowProperty, 0);
                    }
                    if (ViewModel.Invoice.BackOrdertotal > 0 && ViewModel.Invoice.InvoiceLineItems.Count == 1)
                    {
                        ViewModel.Invoice.NetAmount = invoice.NetAmount;
                        ViewModel.Invoice.TotalPaid = invoice.TotalTender;
                    }


                    ViewModel.Invoice.InvoicePayments1 = new ObservableCollection<InvoicePaymentDto>();
                    foreach (var item in ViewModel.Invoice.InvoicePayments)
                    {
                        string eConduitORAfterPayRefID = string.Empty; //item.InvoicePaymentDetails[0].Value;

                        if (item.PaymentOptionType == PaymentOptionType.Afterpay)
                        {
                            int ordernumber;
                            eConduitORAfterPayRefID = item.InvoicePaymentDetails[0].Value;

                            bool result1 = int.TryParse(eConduitORAfterPayRefID, out ordernumber);
                            if (!result1)
                            {
                                var afterPayvalue1 = JsonConvert.DeserializeObject<AfterPayResponseRootObject>(item.InvoicePaymentDetails[0].Value);
                                eConduitORAfterPayRefID = afterPayvalue1.Result.OrderId.ToString();
                                eConduitORAfterPayRefID = "C#" + eConduitORAfterPayRefID + ")";
                            }
                        }
                        //Added for zip same as reference Id
                        else if (item.PaymentOptionType == PaymentOptionType.Zip)
                        {
                            int ordernumber;
                            eConduitORAfterPayRefID = item.InvoicePaymentDetails[0].Value;

                            bool result1 = int.TryParse(eConduitORAfterPayRefID, out ordernumber);
                            if (!result1)
                            {
                                var zipPayvalue1 = JsonConvert.DeserializeObject<ZipResponeObject>(item.InvoicePaymentDetails[0].Value);
                                eConduitORAfterPayRefID = zipPayvalue1.Id.ToString();
                                eConduitORAfterPayRefID = "C#" + eConduitORAfterPayRefID + ")";
                            }
                        }
                        if (item.PaymentOptionType != PaymentOptionType.Linkly)
                        {
                            item.PrintPaymentOptionName = item.PaymentOptionName + eConduitORAfterPayRefID;
                        }


                        Debug.WriteLine("PrintPaymentOptionName : " + item.PrintPaymentOptionName);
                        ViewModel.Invoice.InvoicePayments1.Add(item);
                    }


                    //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                    //PaymentListView.ItemsSource = ViewModel.Invoice.InvoicePayments1;
                    BindableLayout.SetItemsSource(PaymentListView, ViewModel.Invoice.InvoicePayments1);
                    //Ticket end: #62808.by rupesh

                    UpdateTemplateValue(CurrentReceiptTemplate, ViewModel.Invoice);

                    //Ticket start:#22406 Quote sale.by rupesh
                    //Start #78676 by pratik
                    if (ViewModel.Invoice.Status == InvoiceStatus.Quote || ViewModel.Invoice.Status == InvoiceStatus.Voided )
                    {
                        //End #78676 by pratik
                        ToPayLable.IsVisible = false;
                        OutstandingAmountLable.IsVisible = false;
                    }
                    else
                    {
                        //#33839 iOS - Outstanding Line Not Printed
                        ToPayLable.IsVisible = true;
                        OutstandingAmountLable.IsVisible = true;
                        //#33839 iOS - Outstanding Line Not Printed

                    }
                    //Ticket end:#22406 .by rupesh
                    UpdateLogoSize(CurrentReceiptTemplate);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
            return await tcs.Task;
        }

        public void UpdateTemplateValue(ReceiptTemplateDto CurrentReceiptTemplate, InvoiceDto invoiceDto)
        {
            if (Settings.StoreShopDto.LogoImagePath != null && CurrentReceiptTemplate.Displaylogo)
            {
                Displaylogo.IsVisible = CurrentReceiptTemplate.Displaylogo;
                StoreNameLable.FontSize = 30;
            }
            else
            {
                Displaylogo.IsVisible = false;
                StoreNameLable.FontSize = 50;
            }
            StoreNameLable.Text = CurrentReceiptTemplate.StoreNameLable;
            OutletName.IsVisible = CurrentReceiptTemplate.ShowOutletAddress;
            OutletAddress1.IsVisible = CurrentReceiptTemplate.ShowOutletAddress;
            OutletFullAddress.IsVisible = CurrentReceiptTemplate.ShowOutletAddress;
            //Moved from android.By Rupesh.
            OutletEmail.IsVisible = CurrentReceiptTemplate.ShowOutletAddress && !string.IsNullOrEmpty(OutletEmail.Text); // reamining
            OutletPhone.IsVisible = CurrentReceiptTemplate.ShowOutletAddress && !string.IsNullOrEmpty(OutletPhone.Text); // reamining                                                                                                                       //End.By Rupesh.
            InvoiceHeading.Text = CurrentReceiptTemplate.InvoiceHeading;
            InvoiceNoPrefix.Text = CurrentReceiptTemplate.InvoiceNoPrefix;

            //Ticket #11435 Start : Customer address printed incorrectly. By Nikhil
            var customer = invoiceDto.CustomerDetail;
            bool isCustomerAvailable = (customer != null && customer.Id != 0);
            //Moved from android.By Rupesh.
            LblCustomerAddress1.IsVisible = isCustomerAvailable && CurrentReceiptTemplate.ShowCustomerAddress && !string.IsNullOrEmpty(LblCustomerAddress1.Text);
            LblCustomerFullAddress.IsVisible = isCustomerAvailable && CurrentReceiptTemplate.ShowCustomerAddress && !string.IsNullOrEmpty(LblCustomerFullAddress.Text);
            LblCustomerEmail.IsVisible = isCustomerAvailable &&
               CurrentReceiptTemplate.ShowCustomerAddress && !string.IsNullOrEmpty(LblCustomerEmail.Text);
            LblCustomerPhone.IsVisible = isCustomerAvailable &&
                CurrentReceiptTemplate.ShowCustomerAddress && !string.IsNullOrEmpty(LblCustomerPhone.Text);
            //End.By Rupesh.
            //Ticket #11435 End : By Nikhil

            SubTotalLable.Text = CurrentReceiptTemplate.SubTotalLable;
            DiscountLable.Text = CurrentReceiptTemplate.DiscountLable;

            if (invoiceDto.DiscountValue == 0 && CurrentReceiptTemplate.HideDiscountLineOnReceipt)
            {
                DiscountLable.IsVisible = false;
                TotalDiscount.IsVisible = false;
            }
            else
            {
                DiscountLable.IsVisible = true;
                TotalDiscount.IsVisible = true;
            }

            //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total
            //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Rupesh
            if (invoiceDto.TipTaxId == null || invoiceDto.TipTaxId == 0)
            {
                tipsLable.Text = CurrentReceiptTemplate.tipsLable + "(Inc.Tax)";
            }
            else
            {
                tipsLable.Text = CurrentReceiptTemplate.tipsLable + "(Ex.Tax)";

            }
            //End Ticket #73190 By: Rupesh
            //#33590 iPad :: Feature request :: Option To Calculate Surcharge Based on Tax-inclusive Total


            TotalLable.Text = CurrentReceiptTemplate.TotalLable;

            ToPayLable.Text = CurrentReceiptTemplate.ToPayLable;
            BarcodeImage.IsVisible = CurrentReceiptTemplate.PrintReceiptBarcode;
            ServedByLable.Text = CurrentReceiptTemplate.ServedByLable;

            //#40623 iPad: Feature request Don't display SoldBy
            ServedByStackLayout.IsVisible= CurrentReceiptTemplate.showServedByOnReciept;

        }

        //Ticket start:#36540 iPad: Feature request : Larger log size.by rupesh
        private void UpdateLogoSize(ReceiptTemplateDto CurrentReceiptTemplate)
        {

            if (CurrentReceiptTemplate.ReceipStyleFormat?.LogoSize == LogoSize.Large)
            {
                Displaylogo.WidthRequest = 380;
                Displaylogo.HeightRequest = 300;
            }
            else if (CurrentReceiptTemplate.ReceipStyleFormat?.LogoSize == LogoSize.Small)
            {
                Displaylogo.WidthRequest = 180;
                Displaylogo.HeightRequest = 150;
            }
            else
            {
                Displaylogo.WidthRequest = 280;
                Displaylogo.HeightRequest = 240;
            }
        }
        //Ticket end:#36540 .by rupesh
        //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
        private void ShowUsedGiftCardBalanceTally(InvoiceDto invoice)
        {
            GiftCardListView.IsVisible = false;
            var giftCardPaymentDetails = invoice.InvoicePayments
                .Where(x => x.PaymentOptionType == PaymentOptionType.GiftCard && x.InvoicePaymentDetails != null)
                .Select(x => x.InvoicePaymentDetails)
                .ToList();

            if (giftCardPaymentDetails.Any())
            {
                var giftCardDetails = new List<GiftCardDetail>();
                foreach (var paymentDetailList in giftCardPaymentDetails)
                {
                    var number = paymentDetailList
                        .FirstOrDefault(x => x.Key == InvoicePaymentKey.GiftCardNumber)?.Value?.Trim();

                    var openingStr = paymentDetailList
                        .FirstOrDefault(x => x.Key == InvoicePaymentKey.GiftCardOpeningBalance)?.Value?.Trim();

                    var closingStr = paymentDetailList
                        .FirstOrDefault(x => x.Key == InvoicePaymentKey.GiftCardClosingBalance)?.Value?.Trim();

                    decimal.TryParse(openingStr, out decimal opening);
                    decimal.TryParse(closingStr, out decimal closing);

                    var used = opening - closing;

                    giftCardDetails.Add(new GiftCardDetail
                    {
                        Number = number,
                        OpeningBalance = opening,
                        UsedBalance = used,
                        ClosingBalance = closing
                    });
                }
                GiftCardListView.IsVisible = true;
                BindableLayout.SetItemsSource(GiftCardListView, giftCardDetails);
            }
        }
        //Ticket end:#94420.by rupesh

    }
}
