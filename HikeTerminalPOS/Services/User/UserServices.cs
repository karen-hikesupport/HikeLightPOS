using System;
using HikePOS.Models;
using Refit;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using HikePOS.Helpers;
using Fusillade;
using Polly;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using Realms;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;

namespace HikePOS.Services
{
    public class UserServices
    {
        private readonly IApiService<IUserApi> _apiService;
        private readonly IApiService<IAccountApi> accountApiService;
        private readonly AccountServices accountService;

        public UserServices(IApiService<IUserApi> apiService)
        {
            _apiService = apiService;
            accountApiService = new ApiService<IAccountApi>();
            accountService = new AccountServices(accountApiService);
        }

        public ObservableCollection<UserListDto> GetLocalUsers()
        {
            // var users = await CommonQueries.GetAllLocals<UserListDto>();
            using var realm = RealmService.GetRealm();
            var users = realm.All<UserListDB>().ToList();
            var result = new ObservableCollection<UserListDto>(users.Where(x => x.IsActive == true).Select(a => UserListDto.FromModel(a)));
            using var realm1 = RealmService.GetRealm();
            var roles = realm1.All<RoleListDto>().ToList();
            foreach (var role in roles)
            {
                result.SelectMany(x => x.Roles.Where(k => k.RoleId == role.Id)).All(s => { s.RoleName = role.Name; return true; });
            }
            return result;
        }

        public async Task<UserListDto> GetRemoteUserByEmail(Priority priority, string EmailId, bool syncLocal)
        {
            ResponseModel<UserListDto> userResponse = null;

            Task<ResponseModel<UserListDto>> userTask;
        Retry:
            switch (priority)
            {
                case Priority.Background:
                    userTask = _apiService.Background.GetByEmail(Settings.AccessToken, EmailId);
                    break;
                case Priority.UserInitiated:
                    userTask = _apiService.UserInitiated.GetByEmail(Settings.AccessToken, EmailId);
                    break;
                case Priority.Speculative:
                    userTask = _apiService.Speculative.GetByEmail(Settings.AccessToken, EmailId);
                    break;
                default:
                    userTask = _apiService.UserInitiated.GetByEmail(Settings.AccessToken, EmailId);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    userResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await userTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    userResponse = await ex.GetContentAsAsync<ResponseModel<UserListDto>>();
                    if (userResponse != null && userResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                    if (priority != Priority.Background)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                return null;
                            }
                        }
                        else
                        {
                            Extensions.SomethingWentWrong("Getting user by email.", ex);
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (priority != Priority.Background)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
                return null;
            }

            if (userResponse != null && userResponse.success)
            {
                if (syncLocal && userResponse.result != null)
                {
                    UpdateLocalUser(userResponse.result);
                }

                if (userResponse.result != null)
                {
                    if (Settings.CurrentUser != null && userResponse.result.EmailAddress == Settings.CurrentUser.EmailAddress)
                    {
                        Settings.CurrentUser = userResponse.result;
                        if (userResponse.result.GrantedPermissionNames != null && userResponse.result.GrantedPermissionNames.Count > 0)
                            Settings.GrantedPermissionNames = userResponse.result.GrantedPermissionNames;
                    }
                }
                return userResponse.result;
            }
            else
            {
                if (priority != Priority.Background && userResponse != null && userResponse.error != null && userResponse.error.message != null)
                {
                    Extensions.ServerMessage(userResponse.error.message);
                }
                return null;
            }

        }

        //Start #45654 iPad: FR - Bypassing the Who is Selling screen By Pratik
        public UserListDto GetLocalUserByPin(string pin)
        {
            //var users = await BlobCache.LocalMachine.GetAllObjects<UserListDto>();
            using var realm = RealmService.GetRealm();
            var users = realm.All<UserListDB>().ToList();
            var user = UserListDto.FromModel(users.FirstOrDefault(x => x.UserPin == pin));
            if (user != null && user.Id > 0)
            {
                var roles = GetLocalRoles();
                foreach (var role in roles)
                {
                    user.Roles.Where(k => k.RoleId == role.Id).All(s => { s.RoleName = role.Name; return true; });
                }
            }
            return user;
        }
        //End #45654 iPad: FR - Bypassing the Who is Selling screen

        public async Task<UserListDto> GetRemoteUserByUserNameOrEmail(Priority priority, string UserNameOrEmail, bool syncLocal)
        {
            ResponseModel<UserListDto> userResponse = null;

            Task<ResponseModel<UserListDto>> userTask;
        Retry:
            switch (priority)
            {
                case Priority.Background:
                    userTask = _apiService.Background.GetUserByUserNameOrEmail(Settings.AccessToken, UserNameOrEmail);
                    break;
                case Priority.UserInitiated:
                    userTask = _apiService.UserInitiated.GetUserByUserNameOrEmail(Settings.AccessToken, UserNameOrEmail);
                    break;
                case Priority.Speculative:
                    userTask = _apiService.Speculative.GetUserByUserNameOrEmail(Settings.AccessToken, UserNameOrEmail);
                    break;
                default:
                    userTask = _apiService.UserInitiated.GetUserByUserNameOrEmail(Settings.AccessToken, UserNameOrEmail);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    userResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await userTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    userResponse = await ex.GetContentAsAsync<ResponseModel<UserListDto>>();
                    if (userResponse != null && userResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                    if (priority != Priority.Background)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                return null;
                            }
                        }
                        else
                        {
                            Extensions.SomethingWentWrong("Getting user by name or email.", ex);
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (priority != Priority.Background)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
                return null;
            }

            if (userResponse != null && userResponse.success)
            {
                if (syncLocal && userResponse.result != null)
                {
                    UpdateLocalUser(userResponse.result);
                }

                if (userResponse.result != null)
                {
                    if (Settings.CurrentUser != null)
                    {
                        Settings.CurrentUser = userResponse.result;
                        if (userResponse.result.GrantedPermissionNames != null && userResponse.result.GrantedPermissionNames.Count > 0)
                            Settings.GrantedPermissionNames = userResponse.result.GrantedPermissionNames;
                    }
                }
                return userResponse.result;
            }
            else
            {
                if (priority != Priority.Background && userResponse != null && userResponse.error != null && userResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(userResponse.error.message, Colors.Red, Colors.White);
                }
                return null;
            }
        }
        public async Task<UserListDto> GetRemoteUserByUserPin(Priority priority, string userPin, bool syncLocal)
        {
            ResponseModel<UserListDto> userResponse = null;

            Task<ResponseModel<UserListDto>> userTask;
        Retry:
            switch (priority)
            {
                case Priority.Background:
                    userTask = _apiService.Background.GetByUserPin(Settings.AccessToken, userPin);
                    break;
                case Priority.UserInitiated:
                    userTask = _apiService.UserInitiated.GetByUserPin(Settings.AccessToken, userPin);
                    break;
                case Priority.Speculative:
                    userTask = _apiService.Speculative.GetByUserPin(Settings.AccessToken, userPin);
                    break;
                default:
                    userTask = _apiService.UserInitiated.GetByUserPin(Settings.AccessToken, userPin);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    userResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await userTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    userResponse = await ex.GetContentAsAsync<ResponseModel<UserListDto>>();
                    if (userResponse != null && userResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                    if (priority != Priority.Background)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                return null;
                            }
                        }
                        else
                        {
                            Extensions.SomethingWentWrong("Getting user by userPin.", ex);
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (priority != Priority.Background)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
                return null;
            }

            if (userResponse != null && userResponse.success)
            {
                if (syncLocal && userResponse.result != null)
                {
                    UpdateLocalUser(userResponse.result);
                }

                if (userResponse.result != null)
                {
                    if (Settings.CurrentUser != null)
                    {
                        Settings.CurrentUser = userResponse.result;
                        if (userResponse.result.GrantedPermissionNames != null && userResponse.result.GrantedPermissionNames.Count > 0)
                            Settings.GrantedPermissionNames = userResponse.result.GrantedPermissionNames;
                    }
                }
                return userResponse.result;
            }
            else
            {
                if (priority != Priority.Background && userResponse != null && userResponse.error != null && userResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(userResponse.error.message, Colors.Red, Colors.White);
                }
                return null;
            }
        }


        public bool UpdateLocalUser(UserListDto user)
        {
            try
            {
                // await BlobCache.LocalMachine.InsertObject(nameof(UserListDto) + "_" + user.Id.ToString(), user, DateTimeOffset.Now.AddYears(2));
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(user.ToModel(), update: true);
                });

                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public UserListDto GetLocalUser(string emailAddress)
        {
            //var users = await BlobCache.LocalMachine.GetAllObjects<UserListDto>();
            using var realm = RealmService.GetRealm();
            var users = realm.All<UserListDB>().ToList();
            var user = UserListDto.FromModel(users.FirstOrDefault(x => x.EmailAddress == emailAddress));
            if (user != null && user.Id > 0)
            {
                var roles = GetLocalRoles();
                foreach (var role in roles)
                {
                    user.Roles.Where(k => k.RoleId == role.Id).All(s => { s.RoleName = role.Name; return true; });
                }
            }
            return user;
        }

        public async Task<ObservableCollection<UserListDto>> GetRemoteUsers(Priority priority, bool syncLocal)
        {
            ListResponseModel<UserListDto> userResponse = null;

            Task<ListResponseModel<UserListDto>> userTask;
        Retry1:
            switch (priority)
            {
                case Priority.Background:
                    userTask = _apiService.Background.GetAll(Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    userTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    userTask = _apiService.Speculative.GetAll(Settings.AccessToken);
                    break;
                default:
                    userTask = _apiService.UserInitiated.GetAll(Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    userResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await userTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    userResponse = await ex.GetContentAsAsync<ListResponseModel<UserListDto>>();

                    if (userResponse != null && userResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                    if (priority != Priority.Background)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                return null;
                            }
                        }
                        else
                        {
                            Extensions.SomethingWentWrong("Getting users.", ex);
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (priority != Priority.Background)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
                return null;
            }

            if (userResponse != null && userResponse.success)
            {
                if (syncLocal && userResponse.result != null && userResponse.result.items.Count > 0)
                {
                    UpdateLocalUsers(userResponse.result.items);
                }

                return userResponse.result.items;
            }
            else
            {
                if (priority != Priority.Background && userResponse != null && userResponse.error != null && userResponse.error.message != null)
                {
                    Extensions.ServerMessage(userResponse.error.message);
                    return null;
                }
                return null;
            }
        }

        public bool UpdateLocalUsers(ObservableCollection<UserListDto> users)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(users.Select(a => a.ToModel()).ToList(), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public async Task<ObservableCollection<RoleListDto>> GetRemoteRoles(Priority priority, bool syncLocal, RoleRequestModel roleRequest)
        {
            ListResponseModel<RoleListDto> roleResponse = null;

            Task<ListResponseModel<RoleListDto>> roleTask;
        Retry2:
            switch (priority)
            {
                case Priority.Background:
                    roleTask = _apiService.Background.GetRoles(Settings.AccessToken, roleRequest);
                    break;
                case Priority.UserInitiated:
                    roleTask = _apiService.UserInitiated.GetRoles(Settings.AccessToken, roleRequest);
                    break;
                case Priority.Speculative:
                    roleTask = _apiService.Speculative.GetRoles(Settings.AccessToken, roleRequest);
                    break;
                default:
                    roleTask = _apiService.UserInitiated.GetRoles(Settings.AccessToken, roleRequest);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    roleResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await roleTask);
                }
                catch (ApiException ex)
                {
                    roleResponse = await ex.GetContentAsAsync<ListResponseModel<RoleListDto>>();
                    if (roleResponse != null && roleResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry2;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                    if (priority != Priority.Background)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                            }
                        }
                        else
                        {
                            Extensions.SomethingWentWrong("Getting roles.", ex);
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (priority != Priority.Background)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
                return null;
            }

            if (roleResponse.success)
            {
                if (syncLocal && roleResponse.result != null && roleResponse.result.items.Count > 0)
                {
                    UpdateLocalRoles(roleResponse.result.items);
                }
                return roleResponse.result.items;
            }
            else
            {
                if (priority != Priority.Background && roleResponse != null && roleResponse.error != null && roleResponse.error.message != null)
                {
                    Extensions.ServerMessage(roleResponse.error.message);
                }
                return null;
            }
        }

        public ObservableCollection<RoleListDto> GetLocalRoles()
        {
            // return await CommonQueries.GetAllLocals<RoleListDto>();
            var realm = RealmService.GetRealm();
            return new ObservableCollection<RoleListDto>(realm.All<RoleListDto>().ToList());
        }

        public bool UpdateLocalRoles(ObservableCollection<RoleListDto> outlets)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(outlets.ToList(), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public UserListDto GetLocalUserById(int id)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var user = realm.All<UserListDB>().ToList().FirstOrDefault(a => a.Id == id);
                return UserListDto.FromModel(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }


        public async Task<ObservableCollection<UserClockActivityDto>> GetAllClockInOutUsers(Priority priority, bool syncLocal)
        {
            ResponseListModel<UserClockActivityDto> userResponse = null;

            Task<ResponseListModel<UserClockActivityDto>> userTask;
        Retry1:
            switch (priority)
            {
                case Priority.Background:
                    userTask = _apiService.Background.GetClockUserActivities(Settings.AccessToken);
                    break;
                case Priority.UserInitiated:
                    userTask = _apiService.UserInitiated.GetClockUserActivities(Settings.AccessToken);
                    break;
                case Priority.Speculative:
                    userTask = _apiService.Speculative.GetClockUserActivities(Settings.AccessToken);
                    break;
                default:
                    userTask = _apiService.UserInitiated.GetClockUserActivities(Settings.AccessToken);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    userResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await userTask);
                }
                catch (ApiException ex)
                {
                    //Get Exception content
                    userResponse = await ex.GetContentAsAsync<ResponseListModel<UserClockActivityDto>>();

                    if (userResponse != null && userResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                    if (priority != Priority.Background)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                                return null;
                            }
                        }
                        else
                        {
                            Extensions.SomethingWentWrong("Getting clock in out users.", ex);
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (priority != Priority.Background)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
                return null;
            }

            if (userResponse != null && userResponse.success)
            {
                if (syncLocal && userResponse.result != null && userResponse.result.Count > 0)
                {
                    UpdateClockInOutUsers(userResponse.result);
                    WeakReferenceMessenger.Default.Send(new Messenger.MenuDataUpdatedMessenger("User"));
                }
                return userResponse.result;
            }
            else
            {
                if (priority != Priority.Background && userResponse != null && userResponse.error != null && userResponse.error.message != null)
                {
                    Extensions.ServerMessage(userResponse.error.message);
                    return null;
                }
                return null;
            }
        }

        public async Task<ResponseListModel<UserClockActivityDto>> CreateOrUpdateUserClockActivity(Priority priority, bool syncLocal, UserClockActivityInputDto userClockActivity)
        {
            ResponseListModel<UserClockActivityDto> UserClockActivityResponse = null;

            Task<ResponseListModel<UserClockActivityDto>> UserClockActivityTask;
        Retry2:
            switch (priority)
            {
                case Priority.Background:
                    UserClockActivityTask = _apiService.Background.UpdateUserClockActivity(Settings.AccessToken, userClockActivity);
                    break;
                case Priority.UserInitiated:
                    UserClockActivityTask = _apiService.UserInitiated.UpdateUserClockActivity(Settings.AccessToken, userClockActivity);
                    break;
                case Priority.Speculative:
                    UserClockActivityTask = _apiService.Speculative.UpdateUserClockActivity(Settings.AccessToken, userClockActivity);
                    break;
                default:
                    UserClockActivityTask = _apiService.UserInitiated.UpdateUserClockActivity(Settings.AccessToken, userClockActivity);
                    break;
            }

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    UserClockActivityResponse = await Policy
                        .Handle<Exception>()
                        .RetryAsync(retryCount: ServiceConfiguration.retryCount)
                        .WrapAsync(Policy.TimeoutAsync(ServiceConfiguration.ServiceTimeoutSeconds))
                        .ExecuteAsync(async () => await UserClockActivityTask);
                }
                catch (ApiException ex)
                {
                    UserClockActivityResponse = await ex.GetContentAsAsync<ResponseListModel<UserClockActivityDto>>();
                    if (UserClockActivityResponse != null && UserClockActivityResponse.unAuthorizedRequest && ex != null && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool res = await accountService.GetRenewAccessToken(priority);
                        if (res)
                        {
                            goto Retry2;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                    if (priority != Priority.Background)
                    {
                        if (ex.Message == "An error occurred while sending the request")
                        {
                            bool isReachable = await CommonMethods.ReachableCheck(_apiService.ApiBaseAddress);
                            if (!isReachable)
                            {
                                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                            }
                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (priority != Priority.Background)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
                return null;
            }

            if (UserClockActivityResponse.success)
            {
                return UserClockActivityResponse;
            }
            else
            {
                if (priority != Priority.Background && UserClockActivityResponse != null && UserClockActivityResponse.error != null && UserClockActivityResponse.error.message != null)
                {
                    App.Instance.Hud.DisplayToast(UserClockActivityResponse.error.message, Colors.Red, Colors.White);
                }
                return null;
            }
        }


        public bool UpdateClockInOutUsers(ObservableCollection<UserClockActivityDto> ClockInOutUsers)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                realm.Write(() =>
                {
                    realm.Add(ClockInOutUsers.Select(a => a.ToModel()).ToList(), update: true);
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.Track();
                return false;
            }
        }

        public UserClockActivityDto GetClockInOutUsersById(int Userid)
        {
            try
            {
                using var realm = RealmService.GetRealm();
                var result = realm.Find<UserClockActivityDB>(Userid);
                var resultdata = UserClockActivityDto.FromModel(result);
                using var realm1 = RealmService.GetRealm();
                var user = realm1.Find<UserListDB>(Userid);
                resultdata.User = UserListDto.FromModel(user);
                return resultdata;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }

        }
        
        //#95241
        public UserListDto GetUserListDtoById(int Userid)
        {
            try
            {
                using var realm1 = RealmService.GetRealm();
                var user = realm1.Find<UserListDB>(Userid);
                return UserListDto.FromModel(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }

        }
        //#95241

    }
}
