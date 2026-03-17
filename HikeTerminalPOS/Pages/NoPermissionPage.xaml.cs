using System;
using System.Collections.Generic;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{
	public partial class NoPermissionPage : BaseContentPage<BaseViewModel>
    {
		//private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		public NoPermissionPage(string title)
		{
			InitializeComponent();
			lbltitle.Text = title;
			NavigationPage.SetHasNavigationBar(this, false);
		}

		private void ImageButton_Clicked(object sender, EventArgs e)
		{
			App.Current.MainPage = new AppShell();
			//_navigationService.CurrentPage.Navigation.PopModalAsync();
        }
	}
}