using System;
using System.Collections.Generic;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{
	public partial class UpdateAppPage : BaseContentPage<BaseViewModel>
    {
		public UpdateAppPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}

		private void ImageButton_Clicked(object sender, EventArgs e)
		{
			App.Current.MainPage = new AppShell();
			//_navigationService.CurrentPage.Navigation.PopModalAsync();
        }
	}
}