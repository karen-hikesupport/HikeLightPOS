using System;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class ChangeUserPage : PopupBasePage<ChangeUserViewModel>
    {
		public ChangeUserPage()
		{

            try
            {
                InitializeComponent();
                NavigationPage.SetHasNavigationBar(this, false);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
		}
    }
}
