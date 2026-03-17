using HikePOS.Services;
using HikePOS.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
	public class SettingUpOutletViewModel : BaseViewModel
	{
		private readonly static INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		string _ProcessTitle { get; set; } = LanguageExtension.Localize("Progress1_Text");
		public string ProcessTitle
		{
			get { return _ProcessTitle; }
			set { _ProcessTitle = value; SetPropertyChanged(nameof(ProcessTitle)); }
		}

		public async void SettingUpOutlet()
		{
			Title = "Setting Up outlet page";
			using (new Busy(this,false))
			{
				var objOutletSync = new OutletSync();
				bool shopResponse = await objOutletSync.SetupOutlet();
				EnterSalePage.DataUpdated = true;
				if (shopResponse)
				{
					// if (MainPage.entersalepage != null)
					// 	MainPage.entersalepage = null;
					
					// if(MainPage.parksalepage != null)
					// 	MainPage.parksalepage = null;

					// if (MainPage.settingpage != null)
					// 	MainPage.settingpage = null;

					// if (MainPage.adminpage != null)
					// 	MainPage.adminpage = null;

					// if (MainPage.cashRegisterpage != null)
					// 	MainPage.cashRegisterpage = null;

					App.Instance.MainPage = new AppShell();
				}
				else
				{
					if (_navigationService.GetCurrentPage is NavigationPage && ((NavigationPage)_navigationService.GetCurrentPage).Navigation.NavigationStack.Count > 1)
					{
						await ((NavigationPage)_navigationService.GetCurrentPage).PopAsync();
					}
					else
					{
						App.Instance.MainPage = new NavigationPage(new LoginUserPage());
					}
				}
			}
		}

		public override void OnAppearing()
		{
			base.OnAppearing();
			if (!HasInitialized)
			{
				var productStep = 0;
				var total = 0;
				HasInitialized = true;
				SettingUpOutlet();
				Random random = new Random();
                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.ProgressStatusMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.ProgressStatusMessenger>(this, (sender, arg) =>
                    {
                        ProcessTitle = arg.Value;
                    });
                }

                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.ProductProgressMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.ProductProgressMessenger>(this, (sender, arg) =>
                    {
                        if (total == 0)
                        {
                            total = arg.Value;
                        }
                        else
                        {

                            productStep += arg.Value + random.Next(20);
                            productStep = Math.Min(productStep, total);
                        }

                        ProcessTitle = "Loading Products\n" + productStep.ToString() + "/" + total.ToString();
                    });
                }

            }
		}

		public override void OnDisappearing()
		{
			base.OnDisappearing();
			WeakReferenceMessenger.Default.Unregister<Messenger.ProgressStatusMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.ProductProgressMessenger>(this);

		}

	}
}
