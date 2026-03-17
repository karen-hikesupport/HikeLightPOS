using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{

    public partial class OpenCashCalculatorPage : PopupBasePage<OpenCashCalculatorViewModel>
    {
        public OpenCashCalculatorPage()
        {
            InitializeComponent();
        }

        private void textEntry_Unfocused(object sender, FocusEventArgs e)
        {
            ViewModel.UnfocusedCommand.Execute((Entry)sender);
        }

        private void textEntry_Focused(object sender, FocusEventArgs e)
        {
            ViewModel.FocusedCommand.Execute((Entry)sender);
        }

        private void textEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.QuatityTextChangedCommand.Execute(null);
        }
    }
}
