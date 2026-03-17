using System;
using System.Windows.Input;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class MintResetPinViewModel : BaseViewModel
    {
        public ICommand ResetPINCommand { get; set; }

        string _UserId { get; set; }
        public string UserId { get { return _UserId; } set { _UserId = value; SetPropertyChanged(nameof(UserId));}}

        public MintResetPinViewModel()
        {
            ResetPINCommand = new Command(ResetPIN);
        }

        void ResetPIN()
        {
            try
            {
                if (string.IsNullOrEmpty(UserId) || string.IsNullOrWhiteSpace(UserId))
                {
                    return;
                }

                //App.Instance.Hud.DisplayProgress("Please wait...");
                IMintPayment mintPayment = DependencyService.Get<IMintPayment>();
                mintPayment.ResetPin(UserId);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
    }
}

