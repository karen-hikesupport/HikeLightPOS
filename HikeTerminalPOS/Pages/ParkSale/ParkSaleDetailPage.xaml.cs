using System;
using System.Linq;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;
using HikePOS.Enums;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Collections.Generic;
using HikePOS.Models.Payment;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace HikePOS
{

    public partial class ParkSaleDetailPage : PopupBasePage<ParkSaleDetailViewModel>
    {

        public Entry _InvoiceNote { get; private set; }
        public Entry _txtEmail { get; private set; }
        public SearchCustomerView _CustomerPopup { get; private set; }

        public ParkSaleDetailPage()
        {
            InitializeComponent();

            Microsoft.Maui.Handlers.LabelHandler.Mapper.AppendToMapping("MyCustomizationLabel", (handler, view) =>
            {
                if (handler?.PlatformView != null && view is HikePOS.UserControls.AutoFitLabel)
                {
#if IOS
                    handler.PlatformView.AdjustsFontSizeToFitWidth = true;
#endif
                }
            });
            //Start TICKET #76698 iPAD FR: Next Sale by Pratik
            if (Settings.Subscription?.Edition?.PlanType != null && (Settings.Subscription.Edition.PlanType == PlanType.Plus || Settings.Subscription.Edition.PlanType == PlanType.Trial))
            {
                btnPrevious.IsVisible = true;
                btnNext.IsVisible = true;
                btnPreviousphon.IsVisible = true;
                btnNextphon.IsVisible = true;
            }
            //End TICKET #76698 by Pratik
            _CustomerPopup = CustomerPopup;
            _InvoiceNote = InvoiceNote;
            _txtEmail = txtEmail;
            
        }

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        
        void DatePicker_DateSelected(System.Object sender, DateChangedEventArgs e)
        {
            ViewModel.olddate = e.OldDate;
            ViewModel.newDate = e.NewDate;
        }

        async void CustomDatePicker_Unfocused(System.Object sender, FocusEventArgs e)
        {
            if (ViewModel.olddate.HasValue && ViewModel.newDate.HasValue)
            {
                if (ViewModel.olddate.Value.Date != ViewModel.newDate.Value.Date)
                    await ViewModel.UpdateInvoiceDueDate();
                else if (ViewModel.olddate.Value.Date == ViewModel.newDate.Value.Date && ViewModel.MinInvoiceDueDate > ViewModel.newDate.Value)
                {
                    ViewModel.InvoiceDueDate = ViewModel.MinInvoiceDueDate;
                    await ViewModel.UpdateInvoiceDueDate();
                }
            }

        }

        void DueDateTapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            if (ViewModel.EnableInvoiceDueDate)
            {
                duedatepicker.Date = ViewModel.InvoiceDueDate;
                duedatepicker.Focus();
            }
        }
        //End ticket #76208 by Pratik
        void txtEmail_Focused(System.Object sender, Microsoft.Maui.Controls.FocusEventArgs e)
        {
            ViewModel.txtEmail_TextChanged();
        }

        void txtEmail_Completed(System.Object sender, System.EventArgs e)
        {
            if (sender is Entry)
            {
                ((Entry)sender).Unfocus();
                if(!ViewModel.IsEmailWithPayLink || !ViewModel.IsEmailWithPayLinkVisible)
                    ViewModel.txtEmail_Unfocused((Entry)sender);
            }

        }

        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
        }
    }
}







