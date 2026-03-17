using System;
using System.Threading.Tasks;
using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class OpenRegisterPage : PopupBasePage<OpenRegisterViewModel>
    {
		public OpenRegisterPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			txtFloatingAmount.Focus();
		}
	}
}
