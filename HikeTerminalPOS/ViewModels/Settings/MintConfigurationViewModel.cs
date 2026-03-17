using System;
using System.Windows.Input;
using HikePOS.Services;
using HikePOS.Models;
using HikePOS.Models.Payment;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
    public class MintConfigurationViewModel : BaseViewModel
    {
        #region Properties
        string _Username { get; set; }
        public string Username { get { return _Username; } set { _Username = value;  SetPropertyChanged(nameof(Username));} }
        string _Userpin { get; set; }
		public string Userpin { get { return _Userpin; } set { _Userpin = value; SetPropertyChanged(nameof(Userpin)); } }

		string _DeviceStatus { get; set; }
		public string DeviceStatus { get { return _DeviceStatus; } set { _DeviceStatus = value; SetPropertyChanged(nameof(DeviceStatus)); } }

        public event EventHandler<string> ConfigurationSuccessed;
        MintResetPinPage mintResetPinPage;
        #endregion

        #region Command
        public ICommand LoginCommand { get; set; }
        public ICommand ForgotCommand { get; set; }
        #endregion

        #region Life Cycle
        public MintConfigurationViewModel(){
            LoginCommand = new Command(MintLogin);
            ForgotCommand = new Command(ForgotPin);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.MintReaderStatusMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.MintReaderStatusMessenger>(this, (sender, arg) =>
                {
                    if (arg.Value != null)
                    {
                        DeviceStatus = arg.Value.Text;
                        if (arg.Value.Type == Enums.MessageType.Failed)
                        {
                            App.Instance.Hud.Dismiss();
                        }
                        else if (arg.Value.Type == Enums.MessageType.Success && arg.Value.Result != null)
                        {
                            DeviceStatus = "Connected";
                            ConfigurationSuccessed?.Invoke(this, arg.Value.Result.AccessToken);
                            App.Instance.Hud.Dismiss();
                        }
                    }
                });
            }
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            WeakReferenceMessenger.Default.Unregister<Messenger.MintReaderStatusMessenger>(this);
        }
        #endregion

        #region Command Execution
        void MintLogin()
        {
            //using(new Busy(this,true))
            //{
                try
                {
                    if(string.IsNullOrEmpty(Username) || string.IsNullOrWhiteSpace(Username))
                    {
                        return;
                    }

					if (string.IsNullOrEmpty(Userpin) || string.IsNullOrWhiteSpace(Userpin))
					{
						return;
					}

                    App.Instance.Hud.DisplayProgress("Please wait...");
					IMintPayment mintPayment = DependencyService.Get<IMintPayment>();
                    mintPayment.Login(Username, Userpin);
                }
                catch(Exception ex)
                {
                    ex.Track();
                }
            //}

        }
    
        void ForgotPin(){
            if (mintResetPinPage == null)
            {
                mintResetPinPage = new MintResetPinPage();
            }

            if (NavigationService!=null && NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
            {
                NavigationService.PopModalAsync();
                NavigationService.PushModalAsync(mintResetPinPage);
            }
        }
        #endregion






    }
}
