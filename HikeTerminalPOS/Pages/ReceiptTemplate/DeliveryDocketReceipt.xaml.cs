using System;
using System.Collections;
using HikePOS.Models;
using HikePOS.ViewModels;
using HikePOS.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace HikePOS
{
    public partial class DeliveryDocketReceipt : ScrollView
    {
        public DeliveryDocketReceipt()
        {
            InitializeComponent();
        }

        //Ticket #10303 Start : In the invoice, company name should be displayed first and customer name below it. By Nikhil
        public void UpdateData(ReceiptTemplateDto receiptTemplate, InvoiceDto invoice)
        {
            CustomerDto_POS customer = invoice.CustomerDetail;
            if (customer != null && invoice.CustomerId != null &&invoice.CustomerId != 0)
            {
                if (receiptTemplate.ShowCustomerAddress)
                {
                    if (!string.IsNullOrEmpty(customer.CompanyName))
                    {
                        if (receiptTemplate != null)
                            if (receiptTemplate.ShowCompanyNameInBillingInvoice)
                                LblCustomerName.Text = customer.CompanyName + "\n" + customer.FullName;
                            else LblCustomerName.Text = customer.FullName + "\n" + customer.CompanyName;
                    }
                    else
                    {
                        //Ticket #11818 Start : Previous customer name showing if no companyname
                        LblCustomerName.Text = customer.FullName;
                        //Ticket End
                    }

                }
            }
        }
        //Ticket #10303 End. By Nikhil

       
        public void UpdateHtmlHeaderFooter(ReceiptTemplateDto ReceiptTemplate, Printer printer)
        {
            try
            {
                if (ReceiptTemplate != null)
                {
                    //Ticket start:#90943 iOS:FR Display Barcode or SKU instead of Product Name .by Pratik
                    if (this.BindingContext is PaymentViewModel paymentView)
                    {
                        BindableLayout.SetItemsSource(ProductList, Extensions.SetDisplayTitle(paymentView.Invoice.InvoiceLineItems, paymentView.CurrentRegister.DocketReceiptTemplate, false));
                    }
                    else if (this.BindingContext is ParkSaleDetailViewModel parkSaleDetailView)
                    {
                        BindableLayout.SetItemsSource(ProductList, Extensions.SetDisplayTitle(parkSaleDetailView.Invoice.InvoiceLineItems, parkSaleDetailView.CurrentRegister.DocketReceiptTemplate, false));
                    }
                    //Ticket end:#90943 by Pratik

                    //Start #84295 iOS - Feature:- Receipt Template Change Invoice Header Titles by Pratik
                    if (!string.IsNullOrEmpty(ReceiptTemplate.ItemTitleLabel))
                    {
                        lblItem.Text = ReceiptTemplate.ItemTitleLabel;
                    }
                    if (!string.IsNullOrEmpty(ReceiptTemplate.QuantityTitleLabel))
                    {
                        lblQuantity.Text = ReceiptTemplate.QuantityTitleLabel;
                    }
                    //End #84295 by Pratik

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

        public StackLayout Container { get { return ChildStack; } }

        //Ticket start:#36540 iPad: Feature request : Larger log size.by rupesh
        private void UpdateLogoSize(ReceiptTemplateDto CurrentReceiptTemplate)
        {

            if (CurrentReceiptTemplate.ReceipStyleFormat?.LogoSize == LogoSize.Large)
            {
                Logo.WidthRequest  = 380;
                Logo.HeightRequest = 300;
            }
            else if (CurrentReceiptTemplate.ReceipStyleFormat?.LogoSize == LogoSize.Small)
            {
                Logo.WidthRequest  = 180;
                Logo.HeightRequest = 150;

            }
            else
            {
                Logo.WidthRequest  = 280;
                Logo.HeightRequest = 240;
            }

        }
        //Ticket end:#36540 .by rupesh

    }
}
