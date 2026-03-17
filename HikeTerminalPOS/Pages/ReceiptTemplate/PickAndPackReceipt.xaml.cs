using System;
using System.Collections;
using System.Collections.Generic;
using HikePOS.Models;
using HikePOS.ViewModels;
using HikePOS.Helpers;
using System.Diagnostics;

namespace HikePOS
{
    public partial class PickAndPackReceipt : ScrollView
    {
        public PickAndPackReceipt()
        {
            InitializeComponent();
        }

        public void UpdateHtmlHeaderFooter(ReceiptTemplateDto ReceiptTemplate, Printer printer)
        {
            try
            {
                if (ReceiptTemplate != null)
                {
                    //Ticket start:#43113 The big gap is shown in the delivery address print receipt.by rupesh
                    lblHeader.Text = "";
                    lblFooter.Text = "";
                    lbldumy.Text = "";
                    //Ticket end:#43113 .by rupesh
                    lblHeader.Text = CommonMethods.GetHtmlData(lblHeader, ReceiptTemplate.HeaderText);
                    lblFooter.Text = CommonMethods.GetHtmlData(lblFooter, ReceiptTemplate.FooterText);
                    lbldumy.Text = "";
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
        //Ticket start:#36540 iPad: Feature request : Larger log size.by rupesh
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
        //Ticket end:#36540 .by rupesh

    }
}
