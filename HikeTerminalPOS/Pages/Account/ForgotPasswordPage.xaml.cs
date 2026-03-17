using System;
using System.Collections.Generic;
using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class ForgotPasswordPage : BaseContentPage<ForgotPasswordViewModel>
    {
		public ForgotPasswordPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}

        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            if (txtStoreWebAddress.IsSoftInputShowing())
                txtStoreWebAddress.HideSoftInputAsync(System.Threading.CancellationToken.None);
            else if (txtEmail.IsSoftInputShowing())
                txtEmail.HideSoftInputAsync(System.Threading.CancellationToken.None);
        }
    }
}
