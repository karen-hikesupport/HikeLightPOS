using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class PickAndPackPage : PopupBasePage<PickAndPackViewModel>
    {
        public PickAndPackReceipt _PickAndPackReceiptView { get; private set; }

        public PickAndPackPage()
        {
            InitializeComponent();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                PickAndPackReceiptView.Content = new PickAndPackReceipt();
                _PickAndPackReceiptView = (PickAndPackReceipt)PickAndPackReceiptView.Content;
            });
            ViewModel.PickAndPackPage = this;
        }
        //Ticket start:#71299 iPad - Feature: Adding the stock items manually in Pick and Pack.by rupesh

        void Quntity_Entry_Completed(System.Object sender, System.EventArgs e)
        {
            var entry = sender as Entry;
            ViewModel.UpdateQuantity(entry);
        }

        void Quntity_Entry_Unfocused(System.Object sender, System.EventArgs e)
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                var entry = sender as Entry;
                ViewModel.UpdateQuantity(entry);
            }
        }

        void Quntity_Entry_TextChanged(System.Object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
        {
            ViewModel.CheckQuantity();
        }

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        public void SelelectedPicker()
        {
            outletspicker.SelectedIndex = (ViewModel.Outlets == null || ViewModel.SelectedOutlet == null) ? -1 : ViewModel.Outlets.IndexOf(ViewModel.Outlets.First(a => a.Id == ViewModel.SelectedOutlet.Id));
        }
        //End #84293 by Pratik
    }
}
