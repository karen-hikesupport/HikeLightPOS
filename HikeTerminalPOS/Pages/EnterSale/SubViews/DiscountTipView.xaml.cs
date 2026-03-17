using System;
using System.Collections.Generic;
using HikePOS.UserControls;
using HikePOS.ViewModels;
using HikePOS.Helpers;
using HikePOS.Models;
using System.Diagnostics;

namespace HikePOS
{
	public partial class DiscountTipView : Border
    {

		public static readonly BindableProperty PopupTitleProperty =
			BindableProperty.Create("PopupTitle", typeof(string), typeof(Border), string.Empty);

		public string PopupTitle
		{
			get { return (string)GetValue(PopupTitleProperty); }
			set { SetValue(PopupTitleProperty, value); }
		}

		public DiscountTipView()
		{
			InitializeComponent();
			//Ticket start:#33985 iPad: User specific language issued for European country.by rupesh
			DotButton.Text = Resx.AppResources.Culture?.NumberFormat?.CurrencyDecimalSeparator;
			//Ticket end:#33985 .by rupesh
		}


		void Button_Clicked(object sender, System.EventArgs e)
		{
            var DiscountValue = txtDiscountValues.Text;

			var btn = (Button)sender;
			if (btn.Text == "<")
			{
				if (DiscountValue.Length == 1)
				{
					DiscountValue = string.Empty;
				}
				else if (DiscountValue.Length > 1)
				{
					DiscountValue = DiscountValue.Remove(DiscountValue.Length - 1);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(DiscountValue))
				{
					DiscountValue = btn.Text;
				}
				else
				{
					DiscountValue = DiscountValue.Insert(DiscountValue.Length, btn.Text);
				}
			}
			if (!string.IsNullOrEmpty(DiscountValue) && DiscountValue.Length >= 2)
			{
				if (DiscountValue.Substring(0, 1) == "0" && DiscountValue.Substring(1, 1) != ".")
					DiscountValue = DiscountValue.Substring(1);
			}
			if (!string.IsNullOrEmpty(DiscountValue) && DiscountValue.Length == 1 && DiscountValue.Substring(0, 1) == ".")
			{
				DiscountValue = "0.";
			}
			txtDiscountValues.Text = DiscountValue;
        }

		//void PlaceHolder()
		//{ 
		//	var ViewModel = (InvoiceViewModel)this.BindingContext;
		//	if (ViewModel != null)
		//	{
		//		ViewModel.AmountValue = string.Empty;
			
		//		string DiscountTipSymbol = "";
		//		if (ViewModel.DiscountType == "Amount")
		//		{
		//			DiscountTipSymbol = Settings.StoreCurrencySymbol;
		//		}
		//		else
		//		{
		//			DiscountTipSymbol = "%";
		//		}


		//		if (ViewModel.isDiscount)
		//		{
		//			txtDiscountValues.Placeholder = "Discount " + DiscountTipSymbol;
		//		}
		//		else
		//		{
		//			txtDiscountValues.Placeholder = "Tip " + DiscountTipSymbol;
		//		}
		//	}
		//}

		void PerecentageButtonHandle_Clicked(object sender, System.EventArgs e)
		{
			var viewmodel = (InvoiceViewModel)this.BindingContext;
			if (viewmodel != null)
			{
				viewmodel.ChangeDiscountType("Percentage");
				//PlaceHolder();
			}
		}

		void AmountButtonHandle_Clicked(object sender, System.EventArgs e)
		{
			var viewmodel = (InvoiceViewModel)this.BindingContext;
			if (viewmodel != null)
			{
				viewmodel.ChangeDiscountType("Amount");
				//PlaceHolder();
			}
		}

    }
}
