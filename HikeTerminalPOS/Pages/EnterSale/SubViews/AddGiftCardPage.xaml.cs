using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{
	
	public partial class AddGiftCardPage : PopupBasePage<AddGiftCardViewModel>
    {

		public AddGiftCardPage()
		{
			InitializeComponent();
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

        //Start #90944 iOS:FR Gift cards expiry date by Pratik
        private void CustomEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.IsGiftCardBalance = false;
            ViewModel.IsGiftFirstTime= false;
            ViewModel.IsVerified = false;
            ViewModel.IsTopupGiftCard = false;
            ViewModel.GiftCardExpireMsg = string.Empty;
        }
        //End #90944 by Pratik
	}
}
