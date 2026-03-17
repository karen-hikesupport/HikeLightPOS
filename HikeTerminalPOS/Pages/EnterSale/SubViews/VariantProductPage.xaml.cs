using System;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.UserControls;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class VariantProductPage : PopupBasePage<VariantProductModel>, IDisposable
	{
		public VariantProductPage()
		{
			InitializeComponent();
		}

        public void Dispose()
        {
        }
    }
}
