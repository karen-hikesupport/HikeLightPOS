using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Resources;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{

    public partial class CashRegisterPagePhone : BaseContentPage<CashRegisterViewModel>
    {
        public CashRegisterPagePhone()
        {
            try
            {
                InitializeComponent();
                ViewModel.cashRegisterPage = this;
                ViewModel.CloseRegisterReceiptView = closeRegisterReceipt;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private void CustomEntry_Unfocused(object sender, FocusEventArgs e)
        {
            ViewModel.UnfocusedCommand.Execute(((UserControls.CustomEntry)sender).ItemId);
        }

        private void CustomEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.RegisteredTotalCommand.Execute(((UserControls.CustomEntry)sender).ItemId);
        }
    }
}
