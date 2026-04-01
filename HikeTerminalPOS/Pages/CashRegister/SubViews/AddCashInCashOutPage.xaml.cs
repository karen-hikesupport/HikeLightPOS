using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core.Platform;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{

    public partial class AddCashInCashOutPage : PopupBasePage<AddCashInCashOutViewModel>
    {
		public AddCashInCashOutPage()
		{
			InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
			txtAmount.Focus();
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            if(DeviceInfo.Platform == DevicePlatform.Android)
            {
                if(txtNote.IsFocused)
                {
                    txtNote.IsEnabled = false;
                    txtNote.IsEnabled = true;
                }
                else if(txtAmount.IsFocused)
                {
                    txtAmount.IsEnabled = false;
                    txtAmount.IsEnabled = true;
                }
            }
        }
	}
}
