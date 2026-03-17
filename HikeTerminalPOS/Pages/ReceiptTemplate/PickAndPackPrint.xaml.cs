using System;
using System.Collections;
using System.Collections.Generic;
using HikePOS.Models;
using HikePOS.ViewModels;
using HikePOS.Helpers;
using System.Diagnostics;

namespace HikePOS
{
    //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
    public partial class PickAndPackPrint : ScrollView
    {
        public PickAndPackPrint()
        {
            InitializeComponent();
        }

        public void UpdateHtmlHeaderFooter(ReceiptTemplateDto ReceiptTemplate,InvoiceFulfillmentDto invoiceFulfillment, Printer printer)
        {
            try
            {
                txtissuedate.Text = "Issue date : " + invoiceFulfillment.CreationTime.ToStoreTime().ToString("dd MMM, yyyy - hh:mmtt");
                BindableLayout.SetItemsSource(ProductList, invoiceFulfillment.InvoiceLineItems);
                if (ReceiptTemplate != null)
                {
                    lblHeader.Text = "";
                    lblFooter.Text = "";
                    lblHeader.Text = CommonMethods.GetHtmlData(lblHeader, ReceiptTemplate.HeaderText);
                    lblFooter.Text = CommonMethods.GetHtmlData(lblFooter, ReceiptTemplate.FooterText);
                    boxFooter.HeightRequest = string.IsNullOrEmpty(lblFooter.Text) ? 5 : 10;
                    ChildStack.WidthRequest = printer.width;
                    UpdateLogoSize(ReceiptTemplate);
                }
            }
            catch (Exception ex)
            { 
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateLogoSize(ReceiptTemplateDto CurrentReceiptTemplate)
        {

            if (CurrentReceiptTemplate.ReceipStyleFormat?.LogoSize == LogoSize.Large)
            {
                Logo.WidthRequest = Logo.HeightRequest = 380;
            }
            else if (CurrentReceiptTemplate.ReceipStyleFormat?.LogoSize == LogoSize.Small)
            {
                Logo.WidthRequest = Logo.HeightRequest = 180;
            }
            else
            {
                Logo.WidthRequest = Logo.HeightRequest = 280;
            }

        }

    }
    //End #84293 by Pratik
}
