using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
	public class ChangePasswordViewModel : BaseViewModel
	{


        ApiService<IProfileApi> ProfileApiService = new ApiService<IProfileApi>();
        ProfileServices profileService;

        #region Properties
        string _CurrentPassword { get; set; }
        public string CurrentPassword { get { return _CurrentPassword; } set { _CurrentPassword = value; SetPropertyChanged(nameof(CurrentPassword)); } }

        string _NewPassword { get; set; }
        public string NewPassword { get { return _NewPassword; } set { _NewPassword = value; SetPropertyChanged(nameof(NewPassword)); } }

        string _repeatNewPassword { get; set; }
        public string RepeatNewPassword { get { return _repeatNewPassword; } set { _repeatNewPassword = value; SetPropertyChanged(nameof(RepeatNewPassword)); } }
        #endregion

        #region Life Cycle
        public ChangePasswordViewModel()
		{
			profileService = new ProfileServices(ProfileApiService);
		}

        public override void OnDisappearing()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            RepeatNewPassword = string.Empty;
            base.OnDisappearing();
        }
        #endregion

        #region Command
        public ICommand SaveCommand => new Command(SaveTapped);
		#endregion

		#region Command Exe / Methods
		async void SaveTapped()
		{
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(RepeatNewPassword))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("AllValidationMessage"));
            }
            else if (!IsValidPassword(CurrentPassword) || !IsValidPassword(NewPassword) || !IsValidPassword(RepeatNewPassword))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("UserPassword_InValidMessage"));
            }
            else if (NewPassword != RepeatNewPassword)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NewPasswordAndRepeatDidntMatchValidationMessage"));
            }
            else
            {
                var result = await SaveChangePassword();
                if (result)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ChangePasswordSuccessMessage"), Colors.Green, Colors.White);
                    Close();
                }
            }
        }

        bool IsValidPassword(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            string Regx = @"^(?=.*\d)[A-Za-z\d$@$!%*#?&]{6,}$";
            return (Regex.IsMatch(str, Regx, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }

        public async Task<bool> SaveChangePassword()
		{
			using (new Busy(this, true))
			{
                var ChangePassword = new ChangePasswordModel();
                ChangePassword.CurrentPassword = CurrentPassword;
                ChangePassword.NewPassword = NewPassword;
                ChangePassword.RepeatNewPassword = RepeatNewPassword;
                var Result = await profileService.UpdatePassword(Fusillade.Priority.UserInitiated, true, ChangePassword);
				if (Result != null)
				{
					return Result.success;
				}
				return false;
			};

		}
        #endregion
    }
}
