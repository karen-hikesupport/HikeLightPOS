using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
	public class AutoLockViewModel : BaseViewModel
	{
		ApiService<IUserApi> UserApiService = new ApiService<IUserApi>();
		UserServices userService;

		ApiService<IAccountApi> AccountApiService = new ApiService<IAccountApi>();
		AccountServices accountService;

		UserListDto _CurrentUser { get; set; }
		public UserListDto CurrentUser { get { return _CurrentUser;} set { _CurrentUser = value; SetPropertyChanged(nameof(CurrentUser)); } }

		public AutoLockViewModel()
		{
			userService = new UserServices(UserApiService);
			accountService = new AccountServices(AccountApiService);
		}

		public bool CheckPin(string pin, string emailid) {
			using (new Busy(this, true))
			{
				//Start #45654 iPad: FR - Bypassing the Who is Selling screen By Pratik
				if (CurrentUser == null && Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EachUserHasAUniquePinCode)
				{
					var res = userService.GetLocalUserByPin(pin);
					if (res != null)
					{
						CurrentUser = res;
						return true;
					}

				}
				else
				{
					var res = userService.GetLocalUser(emailid);
					if (res != null && res.UserPin == pin)
					{
						return true;
					}

				}
				return false;
				//End #45654 By Pratik
			}
		}

		public async Task<bool> ChangeUser_GetNewAccessToken(string userId) {
			using (new Busy(this, true))
			{
				try
				{
					return await accountService.GetRenewAccessToken(Fusillade.Priority.UserInitiated, userId);
				}
				catch (Exception ex)
				{
					ex.Track();
					return false;
				}
			};
		}
	}
}
