using System;
using HikePOS.ViewModels;
using System.Linq;
using HikePOS.Models;
using System.Text.RegularExpressions;

namespace HikePOS
{

	public partial class ChangePasswordPage : PopupBasePage<ChangePasswordViewModel>
    {

		public ChangePasswordPage()
		{
			InitializeComponent();
		}
	}
}
