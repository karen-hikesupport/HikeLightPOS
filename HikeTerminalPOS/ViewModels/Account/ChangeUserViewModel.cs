using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
	public class ChangeUserViewModel : BaseViewModel
	{
        #region Properties
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        AutoLockPage autolockpage;
        public EventHandler<bool> ChangeUserSuccessed;
        static UserListDto _CurrentUser { get; set; } = Settings.CurrentUser;
		public UserListDto CurrentUser { get { return _CurrentUser; } set { _CurrentUser = value; SetPropertyChanged(nameof(CurrentUser)); } }

		static ObservableCollection<UserListDto> _Users { get; set; }
		public ObservableCollection<UserListDto> Users { get { return _Users; } set { _Users = value; SetPropertyChanged(nameof(Users)); } }

        UserListDto _selectedUser { get; set; }
        public UserListDto SelectedUserDto { get { return _selectedUser; } set { _selectedUser = value; SetPropertyChanged(nameof(SelectedUserDto)); } }

        ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
		UserServices userService;
        #endregion

        #region Life Cycle
        public ChangeUserViewModel()
		{
            Title = LanguageExtension.Localize("ChangeUserPageTitle");
            userService = new UserServices(userApiService);
			Users = new ObservableCollection<UserListDto>();

			if (CurrentUser == null)
			{
				CurrentUser = new UserListDto();
			}
		}

        public override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                LoadData();

                if (autolockpage == null)
                {
                    autolockpage = new AutoLockPage();
                }
                autolockpage.HasBackButtton = true;
                autolockpage.HasSwitchButtton = false;
                autolockpage.AuthennticationSuccessed += Autolockpage_AuthennticationSuccessed;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            try
            {
                if (autolockpage != null)
                {
                    autolockpage.AuthennticationSuccessed -= Autolockpage_AuthennticationSuccessed;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        #endregion

        #region Command

        public ICommand LogoutCommand => new Command(Logout);
        public ICommand UserSelectedCommand => new Command(UserSelected);

        #endregion



        #region Command Exe. / Methods
        async void UserSelected()
        {
            try
            {
                if (SelectedUserDto != null)
                {
                    if (autolockpage == null)
                    {
                        autolockpage = new AutoLockPage();
                    }
                    autolockpage.ViewModel.CurrentUser = SelectedUserDto;
                    autolockpage.HasBackButtton = true;
                    autolockpage.HasSwitchButtton = false;
                    autolockpage.AuthennticationSuccessed += Autolockpage_AuthennticationSuccessed;

                    await NavigationService.PushModalAsync(autolockpage);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        async void Logout()
        {
            var res = await ((AppShell)_navigationService.GetCurrentPage).LogoutUser();
            if (res)
                await Close();
        }

        public void LoadData() {

			using (new Busy(this, true))
			{
				try
				{
					CurrentUser = Settings.CurrentUser;

					var res = userService.GetLocalUsers();
					if (res != null)
					{
                        var result = res.Where(x => x.EmailAddress.ToLower() != Settings.CurrentUser.EmailAddress.ToLower() && x.Outlets != null && x.Roles != null && x.Roles.Any() && x.Outlets.Any(s => s.OutletId == Settings.SelectedOutletId)).Select(x => { x.Roles = new ObservableCollection<UserListRoleDto>() { x.Roles.FirstOrDefault() }; return x; });
						if (result != null)
							Users = new ObservableCollection<UserListDto>(result);
					}

                    if (Users == null)
                    {
                        Users = new ObservableCollection<UserListDto>();
                    }

				}
				catch (Exception ex)
				{
					ex.Track();
					Users = new ObservableCollection<UserListDto>();
				}
			}
		}

        async void Autolockpage_AuthennticationSuccessed(object sender, bool e)
        {
            try
            {
                if (e && sender != null && sender is AutoLockPage)
                {
                    var Autolockpage = (AutoLockPage)sender;
                    await Autolockpage.Close();
                    Settings.CurrentUser = Autolockpage.ViewModel.CurrentUser;
                    Settings.CurrentUserEmail = Autolockpage.ViewModel.CurrentUser.EmailAddress;
                    Settings.GrantedPermissionNames = Autolockpage.ViewModel.CurrentUser.GrantedPermissionNames;
                    EnterSalePage.ServedBy = Autolockpage.ViewModel.CurrentUser;
                    WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("All-Data"));
                    WeakReferenceMessenger.Default.Send(new Messenger.UpdateShopDataMessenger(true));
                    //Ticket no #10041 fixed by rupesh
                    // var mainpage = (MainPage)_navigationService.MainPage;
                    // mainpage.ChangePage("EnterSalePage");
                    //  if (mainpage.Detail is NavigationPage np)
                    // {
                    //    if (np.CurrentPage is EnterSalePage salePage)
                    //        salePage.ViewModel.SetExpandHeight();
                    // }

                    Shell.Current.CurrentItem = Shell.Current.Items[0];

                    //Ticket start:#33790 iPad :: Feature request :: User specific language.by rupesh
                    ////Start ticket #76209 iOS:FR: User Permission : Add a permission for allowing 'Issue a Quote' By Pratik
                    //if (mainpage.Detail is NavigationPage np)
                    //{
                    //    if (np.CurrentPage is EnterSalePage salePage)
                    //        salePage.ViewModel.OnAppearing();
                    //}
                    ////End ticket #76209 By Pratik
                    if (Settings.CurrentUser.AllowOverrideLangugeSettingOverGeneralSetting)
                    {
                        Settings.StoreCulture = Settings.CurrentUser.Language;
                        var objGetTimeZoneService = DependencyService.Get<HikePOS.Services.IGetTimeZoneService>();
                        Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(Settings.CurrentUser.IANATimeZone);
                        Settings.StoreTimeZoneInfoId = Settings.CurrentUser.IANATimeZone;
                        if (Settings.StoreCulture.ToLower() == "ar" || Settings.StoreCulture.ToLower() == "ar-kw")
                        {
                            Settings.SymbolForDecimalSeperatorForNonDot = ",";
                        }
                        Extensions.SetCulture(Settings.StoreCulture.ToLower());

                    }
                    else
                    {
                        Settings.StoreCulture = Settings.StoreZoneAndFormatDetail.Language;
                        var objGetTimeZoneService = DependencyService.Get<HikePOS.Services.IGetTimeZoneService>();
                        Extensions.storeTimeZoneInfo = objGetTimeZoneService.getTimeZoneInfo(Settings.StoreZoneAndFormatDetail.IanaTimeZone);
                        Settings.StoreTimeZoneInfoId = Settings.StoreZoneAndFormatDetail.IanaTimeZone;
                        Extensions.SetCulture(Settings.StoreCulture.ToLower());

                    }
                    //Ticket end:#33790 .by rupesh

                }
                await Close();
                ChangeUserSuccessed?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }
        #endregion

    }
}
