using System;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class AllUserPage : PopupBasePage<AllUserViewModel>
    {
        public AllUserPage()
		{
            InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
            ViewModel.IsAllUserPage = true; 
        }
	}
}
