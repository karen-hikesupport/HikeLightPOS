using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{
	public partial class DocketReceipt : ScrollView
	{
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		public DocketReceipt()
        {
            InitializeComponent();
        }

        public void UpdateDocket(double width)
        {
            try
            {
                lblDatetime.Text = "";
                lblServedBy.Text = "";
                if (_navigationService.IsFlyoutPage && _navigationService.NavigatedPage is BaseContentPage<PaymentViewModel>)
                {
                    var objViewModel = (PaymentViewModel)this.BindingContext;
                    if (objViewModel != null)
                    {
                        lblDatetime.Text = objViewModel.Invoice.FinalizeDateStoreDate.ToString("dd-MM-yyyy h:mm:ss");
                        lblServedBy.Text = "Served by " + objViewModel.Invoice.ServedByName + ',' + objViewModel.Invoice.RegisterName;
                    }
                }
                MainStack.WidthRequest = width;
                this.ForceLayout();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
    }
}
