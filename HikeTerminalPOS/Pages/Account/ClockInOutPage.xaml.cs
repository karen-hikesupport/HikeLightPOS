using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class ClockInOutPage : PopupBasePage<AllUserViewModel>
    {
        public ClockInOutPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ViewModel.IsAllUserPage = false;
        }
    }
}
