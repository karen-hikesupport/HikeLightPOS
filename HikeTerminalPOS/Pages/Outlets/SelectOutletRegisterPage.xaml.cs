using HikePOS.ViewModels;
using System.Linq;
using HikePOS.Models;
using HikePOS.Helpers;
using System;
using System.Threading.Tasks;

namespace HikePOS
{

	public partial class SelectOutletRegisterPage : PopupBasePage<SelectOutletRegisterViewModel>
    {

		public SelectOutletRegisterPage(bool IsClose = false)
		{
			InitializeComponent();
            CloseStk.IsVisible = IsClose;
            ViewModel.IsClose = IsClose;

        }
    }
}
