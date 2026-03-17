using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class AddCustomerStoreCreditPage : PopupBasePage<AddCustomerStoreCreditViewModel>
    {
        public AddCustomerStoreCreditPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            txtNote.Text = string.Empty;
        }
    }
}
