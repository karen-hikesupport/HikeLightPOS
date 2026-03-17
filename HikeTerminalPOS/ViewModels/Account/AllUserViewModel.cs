using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class AllUserViewModel : BaseViewModel
    {
        #region Properties
        AutoLockPage autolockpage;
        public EventHandler<UserListDto> SelectedUser;
        static ObservableCollection<UserListDto> _Users { get; set; }
        public ObservableCollection<UserListDto> Users { get { return _Users; } set { _Users = value; SetPropertyChanged(nameof(Users)); } }

        UserListDto _selectedUser { get; set; }
        public UserListDto SelectedUserDto { get { return _selectedUser; } set { _selectedUser = value; SetPropertyChanged(nameof(SelectedUserDto)); } }

        static ObservableCollection<UserClockActivityDto> _ClockInOutUsers { get; set; }
        public ObservableCollection<UserClockActivityDto> ClockInOutUsers { get { return _ClockInOutUsers; } set { _ClockInOutUsers = value; SetPropertyChanged(nameof(ClockInOutUsers)); } }

        UserClockActivityDto _selectedClockInOutUser { get; set; }
        public UserClockActivityDto SelectedClockInOutUser { get { return _selectedClockInOutUser; } set { _selectedClockInOutUser = value; SetPropertyChanged(nameof(SelectedClockInOutUser)); } }

        ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
        UserServices userService;

        public bool IsAllUserPage { get; set; }
        #endregion

        #region Life Cycle
        public AllUserViewModel()
        {
            userService = new UserServices(userApiService);
            Users = new ObservableCollection<UserListDto>();
            ClockInOutUsers = new ObservableCollection<UserClockActivityDto>();
            UpdateClockInOutCommand = new Command<UserListDto>(UpdateClockinOutData);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();           
            if (IsAllUserPage)
            {
                LoadData();
            }
            else
            {
                LoadClockinOutData();
            }
        }

        async Task NavigationToSetAutoLockPage()
        {
            if (autolockpage == null)
            {
                autolockpage = new AutoLockPage();
                autolockpage.RequiredUpdateAccessToken &= Settings.StoreGeneralRule.SwitchUserAfterEachSale != false;
                autolockpage.AuthennticationSuccessed += Autolockpage_AllUserAuthennticationSuccessed;
            }
            autolockpage.HasBackButtton = true;
            autolockpage.HasSwitchButtton = false;
            autolockpage.ViewModel.CurrentUser = SelectedUserDto;
            await NavigationService.PushModalAsync(autolockpage);
        }

        #endregion

        #region Command
        public ICommand UserSelectedCommand => new Command(UserSelected);
        public ICommand UserClockInOutSelectedCommand => new Command(ClockInOutUserSelected);
        public ICommand UpdateClockInOutCommand { get; }
        #endregion

        #region Command Exe / Methods
        async void UserSelected()
        {
            try
            {
                if (SelectedUserDto != null)
                {
                    await NavigationToSetAutoLockPage();
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void ClockInOutUserSelected()
        {
            try
            {
                if (SelectedClockInOutUser?.User != null)
                {
                    UpdateClockinOutData(SelectedClockInOutUser.User);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
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
                        var result = res.Where(x => x.Outlets != null && x.Roles != null && x.Roles.Any() && x.Outlets.Any(s => s.OutletId == Settings.SelectedOutletId)).Select(x => { x.Roles = new ObservableCollection<UserListRoleDto>() { x.Roles.FirstOrDefault() }; return x; });
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

        public async void LoadClockinOutData()
        {
            using (new Busy(this, true))
            {
                try
                {
                    var result2 = await userService.GetAllClockInOutUsers(Fusillade.Priority.UserInitiated, true);
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

                    await NavigationService.PushModalAsync(autolockpage);
                }
                catch (Exception ex)
                {
                    ex.Track();
                    ClockInOutUsers = new ObservableCollection<UserClockActivityDto>();
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
        }

        void Autolockpage_AllUserAuthennticationSuccessed(object sender, bool e1)
        {
            MainThread.BeginInvokeOnMainThread(async ()=>
            {
                try
                {
                    await autolockpage.Close();
                    if (e1)
                    {
                        if (e1 && autolockpage != null)
                        {
                            var Autolockpage = autolockpage;
                            Settings.CurrentUser = Autolockpage.ViewModel.CurrentUser;
                            Settings.CurrentUserEmail = Autolockpage.ViewModel.CurrentUser.EmailAddress;
                            Settings.GrantedPermissionNames = Autolockpage.ViewModel.CurrentUser.GrantedPermissionNames;
                            EnterSalePage.ServedBy = Autolockpage.ViewModel.CurrentUser;
                            WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("All"));
                            WeakReferenceMessenger.Default.Send(new Messenger.UpdateShopDataMessenger(true));
                        }
                        SelectedUser?.Invoke(this, autolockpage.ViewModel.CurrentUser);
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            });
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
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
        }
        #endregion

    }
}
