using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class CustomerDetailPage : PopupBasePage<AddCustomerViewModel>
    {

 		private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		public CustomerDetailPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			//Ticket start:#25325 Getting "Invoice not of valid status for modification" error while syncing invoice to Xero.by rupesh
			try
			{
                //Start #77581 Edit field should be disable if User is not granted with Permission as per web by Pratik
                ViewModel.EditCustomerVisible = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.Customers.Customer.Edit");
                //End #77581 by Pratik
                if (_navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel> || _navigationService.NavigatedPage is BaseContentPage<PaymentViewModel>)
				{
					var invoice = ((BaseContentPage<EnterSaleViewModel>)_navigationService.RootPage).ViewModel.invoicemodel?.Invoice;
					if (invoice?.Status == InvoiceStatus.OnAccount)
					{
						ViewModel.EditCustomerVisible = false;
					}

				}

                //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik
                bool result = false;
                var myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeCreditNotePrintFeature");
                bool tempResult = Boolean.TryParse(Convert.ToString(myPropInfo.GetValue(Settings.ShopFeatures)), out result);
                btnPrintCustomer.IsVisible = result;
                //End Ticket #74631 by pratik
            }
            catch (Exception ex)
			{
				ex.Track();
			}
			//Ticket end:#25325 .by rupesh

		}


		void FilterOptionHandle_ItemChanged(object sender, string e)
		{
			if (!string.IsNullOrEmpty(e))
			{
				ViewModel.SelectFilterMenu(e);
			}
		}

        //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik
        async void PrintHandle_Clicked(object sender, System.EventArgs e)
        {
            using (new BaseViewModel.Busy(this.ViewModel, true))
            {
                await ViewModel.PrintInvoice(CreditNoteView);
            };
        }
        //End Ticket #74631 by pratik
    }
}
