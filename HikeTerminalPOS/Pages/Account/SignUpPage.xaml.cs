using HikePOS.ViewModels;

namespace HikePOS
{

	public partial class SignUpPage : BaseContentPage<SignUpViewModel>
    {
		public SignUpPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}

		void EntryKeyboardHandle_Focused(object sender, FocusEventArgs e)
		{
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				var height = MainGrid.Height - 120;
				await MainScroll.ScrollToAsync(0, height, true);
			});
		}
	}
}
