using System;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using HikePOS.Enums;

namespace HikePOS
{
    public partial class ProductDetailPage : PopupBasePage<ProductDetailViewModel>
    {

		public ProductDetailPage()
		{
			InitializeComponent();
            // var size = ((DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) / 4) - 55;
            // ProductImage.HeightRequest = size;
            // ProductImage.WidthRequest = size;

            
            //Ticket start:#71297 iPad- Feature: A lock the sale price.by rupesh
            // bool isUnlockSalePrice = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.EnterSale.UnlockSalePrice"));
            // txtPrice.IsEnabled = isUnlockSalePrice;
            // txtDiscount.IsEnabled = isUnlockSalePrice;
            //Ticket end:#71297 .by rupesh
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("MyCustomizationEditorHandler", (handler, view) =>
            {
                if (handler != null && view is Editor)
                {
#if IOS
                    handler.PlatformView.Layer.BorderWidth = 0;
#elif ANDROID
                    handler.PlatformView.Background = null;
#endif
                }
            });
        }

        void RetailPriceHandle_Focused(object sender, FocusEventArgs e)
		{
			((Entry)sender).Text = "";
		}

		void DiscountValueHandle_Focused(object sender, FocusEventArgs e)
		{
			((Entry)sender).Text = "";
		}

        //Ticket start:#32571 iOS - A few optimisation on qty editing in sales cart.by rupesh
        void QuantityHandle_Focused(object sender, FocusEventArgs e)
        {
            ((Entry)sender).Text = "";

        }

        //Ticket end:#32571 .by rupesh

        //Start #71297 iPad- Feature: A lock the sale price  By pratik
        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            if(DeviceInfo.Platform == DevicePlatform.Android)
            {
                if (txtDiscount.IsFocused)
                {
                    txtDiscount.IsEnabled = false;
                    txtDiscount.IsEnabled = true;
                }
                else if (txtPrice.IsFocused)
                {
                    txtPrice.IsEnabled = false;
                    txtPrice.IsEnabled = true;
                }
                else if (VariantProductQuantity.IsFocused)
                {
                    VariantProductQuantity.IsEnabled = false;
                    VariantProductQuantity.IsEnabled = true;
                }
                else if (txtnote.IsFocused)
                {
                    txtnote.IsEnabled = false;
                    txtnote.IsEnabled = true;
                }
                else if (PickerTaxList.IsFocused)
                {
                    PickerTaxList.IsEnabled = false;
                    PickerTaxList.IsEnabled = true;
                }
            }
            else
            {
                if (txtDiscount.IsFocused)
                    txtDiscount.Unfocus();
                else if (txtPrice.IsFocused)
                    txtPrice.Unfocus();
                else if (VariantProductQuantity.IsFocused)
                    VariantProductQuantity.Unfocus();
                else if (txtnote.IsFocused)
                    txtnote.Unfocus();
                else if (PickerTaxList.IsFocused)
                    PickerTaxList.Unfocus();
            }
        }

        void SaveButton_Clicked(System.Object sender, System.EventArgs e)
        {
            if (txtDiscount.IsFocused)
            { 
                txtDiscount.Unfocus();
            }
            else if (txtPrice.IsFocused)
            { 
                txtPrice.Unfocus();
            }
            else if (VariantProductQuantity.IsFocused)
            { 
                VariantProductQuantity.Unfocus();
            }
            ViewModel.SaveCommand.Execute(null);
        }
        //End #71297 By pratik
    }
}
