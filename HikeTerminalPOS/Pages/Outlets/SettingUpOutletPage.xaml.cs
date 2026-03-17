using System;
using System.Collections.Generic;
using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class SettingUpOutletPage : BaseContentPage<SettingUpOutletViewModel>
    {
		public SettingUpOutletPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}
	}
}
