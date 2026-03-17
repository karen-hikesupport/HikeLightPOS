using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class LoginPage : BaseContentPage<LoginViewModel>
    {
		public LoginPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}
        protected override void OnAppearing()
        {
            base.OnAppearing();
            #if IOS
            // Workaround for iPhone 14: https://developer.apple.com/forums/thread/715417

            if (UIKit.UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                // var window = UIKit.UIApplication.SharedApplication.Windows.FirstOrDefault();
                var window =  UIKit.UIApplication.SharedApplication.ConnectedScenes.OfType<UIKit.UIWindowScene>().SelectMany(s => s.Windows).FirstOrDefault();
                double sheight = window?.WindowScene?.StatusBarManager?.StatusBarFrame.Height ?? 0;
                if(sheight <= 0)
                    sheight = 30;
                LoginBG.Margin = new Thickness(0,(sheight * -1),0,0);
                var topPadding = window?.SafeAreaInsets.Top ?? 0;
                var bottomPadding = window?.SafeAreaInsets.Bottom ?? 0;
                this.Padding = new Thickness(0,-topPadding,0,-bottomPadding);
            }
            #endif

        }
        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
			if(txtStoreWebAddress.IsSoftInputShowing())
                txtStoreWebAddress.HideSoftInputAsync(System.Threading.CancellationToken.None);
            else if (txtEmail.IsSoftInputShowing())
                txtEmail.HideSoftInputAsync(System.Threading.CancellationToken.None);
            else if (txtPassword.IsSoftInputShowing())
                txtPassword.HideSoftInputAsync(System.Threading.CancellationToken.None);
        }
    }
}
