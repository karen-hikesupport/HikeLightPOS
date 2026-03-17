using System;
using System.Collections.Generic;
using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class TwoFactorPage : BaseContentPage<TwoFactorViewModel>
    {
		public TwoFactorPage(string storeWebAddress,string email,string token)
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
            ViewModel.StoreWebAddress = storeWebAddress;
            ViewModel.Email = email;
            ViewModel.Token = token;
		}

        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            
        }

        private void TapGestureRecognizer_Tapped1(object sender, TappedEventArgs e)
        {
            ViewModel.IsRemember = !ViewModel.IsRemember;
        }
    }
}
