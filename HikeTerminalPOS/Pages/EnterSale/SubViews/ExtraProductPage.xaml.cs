using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class ExtraProductPage : PopupBasePage<ExtraProductViewModel>
    {

		public event EventHandler<IEnumerable<ProductDto_POS>> ExtraProductAdded;

		public ExtraProductPage()
		{
			InitializeComponent();
			Title = "Extra product";
		}
		
		void SelectProductHandle_ItemTapped(System.Object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
		{
			if (e?.CurrentSelection?.FirstOrDefault() != null && e.CurrentSelection.FirstOrDefault() is ProductDto_POS product)
			{
				decimal stk = 0;
                //Ticket start: #96644 Extra Product slider is not working on iPad. by rupesh
				if (!string.IsNullOrEmpty(product.Stock) && decimal.TryParse(product.Stock, out stk) && stk <= 0 && product.TrackInventory && !product.AllowOutOfStock)
				{
					//Ticket end: #96644 . by rupesh
					App.Instance.Hud.DisplayToast("This product is currently out of stock or has zero inventory.", Colors.Red, Colors.White);
					lstViewProductList.SelectedItem = null;
					return;
				}
				product.isSelected = !product.isSelected;
                //Start:#45375 iPad: FR - Change how products listed on receipt based on order to scan.by rupesh
                product.SelectionDateTime = DateTime.Now;
                //Start:#45375 .by rupesh
            }
        }


		void AddHandle_Clicked(object sender, System.EventArgs e)
		{
            //Start:#63489 IOS: Items Not sequence wise in print receipt.by rupesh
            var selectedProducts = ViewModel.Products.Where(x => x.isSelected).OrderBy(x=>x.SelectionDateTime);
            //Start:#63489 .by rupesh
            //if (selectedProducts == null || selectedProducts.Count() < 1)
            //{
            //	//Application.Current.MainPage.DisplayAlert("Alert", "Please select any product", "Ok");
            //	App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ExtraProductSelectionValidationMessage"));
            //}

            ExtraProductAdded?.Invoke(this, selectedProducts);
		}

		async void CloseHandle_Clicked(object sender, System.EventArgs e)
		{
            await Close();
		}

        public async Task Close()
        {
			try
			{
                if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
                    await Navigation.PopModalAsync();
			}
			catch (Exception ex)
			{
				ex.Track();
			}
        }
	}
}
