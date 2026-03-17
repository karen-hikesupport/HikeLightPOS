using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{

    public partial class AddCustomerPage : PopupBasePage<AddCustomerViewModel>
    {

        public AddCustomerPage()
        {
            InitializeComponent();
            //START ticket #76208 IOS:FR:Terms of payments by Pratik
            if (Settings.Subscription.Edition.PlanType != null && (Settings.Subscription.Edition.PlanType == PlanType.Plus || Settings.Subscription.Edition.PlanType == PlanType.Trial))
            {
                lblInvoicesDue.IsVisible = true;
                frmInvoicesDue.IsVisible = true;
            }
            //End ticket #76208 by Pratik
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.LoadData();
            //START ticket #76208 IOS:FR:Terms of payments by Pratik
            ViewModel.InvoicesDueDays = null;
            if (ViewModel.NewCustomer != null && ViewModel.NewCustomer.InvoicesDueType.HasValue)
            {
                ViewModel.SelectedInvoicesDueType = ViewModel.GetInvoicesDueTypeStr(ViewModel.NewCustomer.InvoicesDueType.Value);
                ViewModel.InvoicesDueDays = ViewModel.NewCustomer.InvoicesDueDays?.ToString();
                lblInvoicesDueDay.Unfocus();
            }
            //End ticket #76208 by Pratik
        }

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            PickerOverDueDate.Focus();
        }
        //End ticket #76208 by Pratik

    }
}
