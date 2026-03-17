using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class ApproveAdminViewModel : BaseViewModel
	{
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		static ObservableCollection<UserListDto> _Users { get; set; }
		public ObservableCollection<UserListDto> Users { get { return _Users; } set { _Users = value; SetPropertyChanged(nameof(Users)); } }

        static ObservableCollection<UserClockActivityDto> _ClockInOutUsers { get; set; }
        public ObservableCollection<UserClockActivityDto> ClockInOutUsers { get { return _ClockInOutUsers; } set { _ClockInOutUsers = value; SetPropertyChanged(nameof(ClockInOutUsers)); } }

        //Ticket start:#91263 iOS:FR Redeeming Points.by rupesh 
		private bool _isDescriptionNotShown { get; set; }
		public bool IsDescriptionNotShown { get { return _isDescriptionNotShown; } set { _isDescriptionNotShown = value; SetPropertyChanged(nameof(IsDescriptionNotShown)); } }
        //Ticket end:#91263 .by rupesh 

		ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
		UserServices userService;

        public ICommand UpdateClockInOutCommand { get; }

        AutoLockPage autolockpage;

		public ApproveAdminViewModel()
		{
			userService = new UserServices(userApiService);
			Users = new ObservableCollection<UserListDto>();
            ClockInOutUsers = new ObservableCollection<UserClockActivityDto>();
            UpdateClockInOutCommand = new Command<UserListDto>(UpdateClockinOutData);
		}
		public void LoadData()
		{

			using (new Busy(this, true))
			{
				try
				{
					var res = userService.GetLocalUsers();
                    if (res != null)
					{
						var result = res.Where(x => x.Outlets != null && x.Roles != null && x.Roles.Any(s => s.RoleName == "Admin" ||  s.RoleName == "Manager") && x.Outlets.Any(s => s.OutletId == Settings.SelectedOutletId)).Select(x => { x.Roles = new ObservableCollection<UserListRoleDto>() { x.Roles.FirstOrDefault() }; return x; });
						if (result != null)
							Users = new ObservableCollection<UserListDto>(result);
					}

                    if(Users == null)
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

        public async void LoadClockinOutData()
        {
            using (new Busy(this, true))
            {
                try
                {
                    //var result1 = await userService.GetRemoteUsers(Fusillade.Priority.UserInitiated, true);
                    var result2 = await userService.GetAllClockInOutUsers(Fusillade.Priority.UserInitiated, true);
                    //if (result1 != null && result2 != null)
                    if (result2 != null)
                    {
                        ClockInOutUsers = result2;
                    }

                    if (ClockInOutUsers == null)
                    {
                        ClockInOutUsers = new ObservableCollection<UserClockActivityDto>();
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                    ClockInOutUsers = new ObservableCollection<UserClockActivityDto>();
                }
            }
        }
        public async void UpdateClockinOutData(UserListDto userListDto)
        {
            if (userListDto == null)
                return;
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                using (new Busy(this, true))
                {
                    try
                    {
                        if (autolockpage == null)
                        {
                            autolockpage = new AutoLockPage();
                            autolockpage.AuthennticationSuccessed += Autolockpage_AuthennticationSuccessed;
                            
                        }
                        userListDto = userService.GetLocalUserById(userListDto.Id);
                        autolockpage.ViewModel.CurrentUser = userListDto;
                        autolockpage.HasBackButtton = true;
                        autolockpage.HasSwitchButtton = false;
                        autolockpage.RequiredUpdateAccessToken = false;

                        await _navigationService.GetCurrentPage.Navigation.PushModalAsync(autolockpage);
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                        ClockInOutUsers = new ObservableCollection<UserClockActivityDto>();
                    }
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
        }
        async void Autolockpage_AuthennticationSuccessed(object sender, bool e)
        {
            using (new Busy(this, true))
            {
                try
                {
                    var Autolockpage = (AutoLockPage)sender;
                    await Autolockpage.Close();
                    UserClockActivityInputDto userClockActivityModel = new UserClockActivityInputDto() { Id = Autolockpage.ViewModel.CurrentUser.Id };
                    var res = await userService.CreateOrUpdateUserClockActivity(Fusillade.Priority.UserInitiated, true, userClockActivityModel);
                    if (res != null)
                    {
                        LoadClockinOutData();
                    }
                    if (ClockInOutUsers == null)
                    {
                        ClockInOutUsers = new ObservableCollection<UserClockActivityDto>();
                    }
                }
                catch(Exception ex)
                {
                    ex.Track();
                }
            }
        }



	}
}
