using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{

    public partial class PaymentPage : BaseContentPage<PaymentViewModel>
    {
        public ScrollView _Rootview
        {
            get
            {
                return rootview;
            }
        }

        public SearchCustomerView _CustomerPopup
        {
            get
            {
                return CustomerPopup;
            }
        }

        public PaymentPage()
        {
            try
            {
                InitializeComponent();
                NavigationPage.SetHasNavigationBar(this, false);
                customernamelbl.PropertyChanged += ViewModel.CustomerNameLableChanged;
                var height = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
                if (height <= 750)
                {
                    SuccessPaymentView.RowSpacing = 25;
                    SuccessPaymentView.Padding = new Thickness(100, 25, 100, 0);
                }
                else
                {
                    SuccessPaymentView.RowSpacing = 30;
                    SuccessPaymentView.Padding = new Thickness(100, 50, 100, 0);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            Title = "Payment";


        }

        private void InvoiceItemList_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (ViewModel.Invoice.InvoiceLineItems.Count > ViewModel.InvoiceLineItems.Count)
            {
                if (((InvoiceLineItemDto)e.Item) == ViewModel.InvoiceLineItems[15])
                {
                    ViewModel.InvoiceLineItems = ViewModel.Invoice.InvoiceLineItems;
                }
            }
        }
    }
}