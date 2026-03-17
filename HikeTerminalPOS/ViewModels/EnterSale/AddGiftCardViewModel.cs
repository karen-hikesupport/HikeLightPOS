using System;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Models;
using HikePOS.Resources;
using HikePOS.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
	public class AddGiftCardViewModel : BaseViewModel
	{
		ApiService<IGiftCardApi> giftCardApiService = new ApiService<IGiftCardApi>();
		GiftCardServices giftCardService;
		public CustomerDto_POS CustomerDetail;

        #region Properties
        public event EventHandler<GiftCardDto> GiftCardAdded;

        //#28721 Email for Sending Gift Cards
        string _GiftFromName { get; set; } = "";
		public string GiftFromName { get { return _GiftFromName; } set { _GiftFromName = value; SetPropertyChanged(nameof(GiftFromName)); } }

		string _GiftFromEmail { get; set; } = "";
		public string GiftFromEmail { get { return _GiftFromEmail; } set { _GiftFromEmail = value; SetPropertyChanged(nameof(GiftFromEmail)); } }

		string _GiftReceiptEmail { get; set; } = "";
		public string GiftReceiptEmail { get { return _GiftReceiptEmail; } set { _GiftReceiptEmail = value; SetPropertyChanged(nameof(GiftReceiptEmail)); } }

		string _GiftReceiptName { get; set; } = "";
		public string GiftReceiptName { get { return _GiftReceiptName; } set { _GiftReceiptName = value; SetPropertyChanged(nameof(GiftReceiptName)); } }

		string _GiftReceiptMessage { get; set; } = "";
		public string GiftReceiptMessage { get { return _GiftReceiptMessage; } set { _GiftReceiptMessage = value; SetPropertyChanged(nameof(GiftReceiptMessage)); } }


		bool _IsGiftFirstTime { get; set; } = false;
		public bool IsGiftFirstTime { get { return _IsGiftFirstTime; } set { _IsGiftFirstTime = value; SetPropertyChanged(nameof(IsGiftFirstTime)); } }

		//#28721 Email for Sending Gift Cards


		string _GiftCardNumber { get; set; } = "";
		public string GiftCardNumber { get { return _GiftCardNumber; } set { _GiftCardNumber = value; SetPropertyChanged(nameof(GiftCardNumber)); } }

		public string _GiftCardAmount { get; set; }
		public string GiftCardAmount { get { return _GiftCardAmount; } set { _GiftCardAmount = value; SetPropertyChanged(nameof(GiftCardAmount)); } }

        //Start #90944 iOS:FR Gift cards expiry date by Pratik
        private string _giftCardExpireMsg;
		public string GiftCardExpireMsg { get { return _giftCardExpireMsg; } set { _giftCardExpireMsg = value; SetPropertyChanged(nameof(GiftCardExpireMsg)); } }
        //End #90944 by Pratik

        public decimal _GiftCardBalance { get; set; }
		public decimal GiftCardBalance { get { return _GiftCardBalance; } set { _GiftCardBalance = value; SetPropertyChanged(nameof(GiftCardBalance)); } }

		bool _IsVerified { get; set; } = false;
		public bool IsVerified { get { return _IsVerified; } set { _IsVerified = value; SetPropertyChanged(nameof(IsVerified)); } }


		bool _IsTopupGiftCard { get; set; } = false;
		public bool IsTopupGiftCard { get { return _IsTopupGiftCard; } set { _IsTopupGiftCard = value; SetPropertyChanged(nameof(IsTopupGiftCard)); } }

		bool _IsGiftCardBalance { get; set; } = false;
		public bool IsGiftCardBalance { get { return _IsGiftCardBalance; } set { _IsGiftCardBalance = value; SetPropertyChanged(nameof(IsGiftCardBalance)); } }

        #endregion

        #region Constructor/OnAppearing
        public AddGiftCardViewModel()
        {
            Title = "Add gift card page";
            giftCardService = new GiftCardServices(giftCardApiService);
            IsTopupGiftCard = false;
        }

        public override void OnAppearing()
		{
			base.OnAppearing();
			try
			{
				//Ticket start:#39787 iPad - Issuing and actioning gift cards using the barcode is not working on iPad, but working on the web.by rupesh
                if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.BarcodeMessenger>(this))
                {
                    WeakReferenceMessenger.Default.Register<Messenger.BarcodeMessenger>(this, (sender, arg) =>
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            GiftCardNumber = arg.Value;
                            await VerifyGiftCard();
                        });
                    });
                }
                //Ticket end:#39787 .by rupesh
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
				//Ticket start:#39787 iPad - Issuing and actioning gift cards using the barcode is not working on iPad, but working on the web.by rupesh
                WeakReferenceMessenger.Default.Unregister<Messenger.BarcodeMessenger>(this);
                //Ticket end:#39787 .by rupesh

			}
			catch (Exception ex)
			{
				ex.Track();
			}

		}

        #endregion

        #region Command
        public ICommand VerifyGiftCardCommand => new Command(VerifyGiftCardTapped);
        public ICommand AddToCartCommand => new Command(AddToCartTapped);
        public ICommand BackToGiftCardCommand => new Command(BackToGiftCardTapped);
        #endregion

        #region Command Execution

        public void VerifyGiftCardTapped()
        {
           _= VerifyGiftCard();
        }

        void BackToGiftCardTapped()
        {
            IsGiftCardBalance = false;
        }

        void AddToCartTapped()
        {
            try
            {
                decimal amount = Convert.ToDecimal(GiftCardAmount);

                if (string.IsNullOrEmpty(GiftCardAmount) || amount == 0)
                {
                    //Application.Current.MainPage.DisplayAlert("Authentication error", "Please enter valid gift card Amount.", "Ok");
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("GiftCardAmountValidationMessage"));
                    return;
                }
                else if (!string.IsNullOrEmpty(GiftReceiptName) && string.IsNullOrEmpty(GiftFromName))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("GiftCardFromName"));
                    return;
                }
                else if (!string.IsNullOrEmpty(GiftReceiptEmail) && string.IsNullOrEmpty(GiftFromEmail))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("GiftCardFromEmail"));
                    return;
                }

                GiftCardAdded?.Invoke(this, new GiftCardDto()
                {
                    Amount = amount,
                    SpentAmount = 0,
                    Number = GiftCardNumber,
                    IsTopupGiftCard = IsTopupGiftCard,
                    FromEmail = GiftFromEmail,
                    RecipientEmail = GiftReceiptEmail,
                    FromName = GiftFromName,
                    RecipientName = GiftReceiptName,
                    RecipientMessage = GiftReceiptMessage
                });
            }
            catch (Exception ex)
            {
                ex.Track();
                //Application.Current.MainPage.DisplayAlert("Authentication error", "Please enter valid gift card Amount.", "Ok");
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("GiftCardAmountValidationMessage"));
            }
        }

        #endregion

        public async Task VerifyGiftCard()
        {
            
            GiftCardExpireMsg = string.Empty; // #90944 by Pratik
            if (string.IsNullOrEmpty(GiftCardNumber))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("GiftCardNumberValidationMessage"));
                return;
            }
            else if (GiftCardNumber.Length < 4)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("GiftCardNumberMinimumValidationMessage"));
                return;
            }
            using (new Busy(this, true))
            {
                //Ticket start:#30620 iPad: the balance of gift card should be update.by rupesh
                var GiftResponse = await giftCardService.ValidateGiftCard(Fusillade.Priority.UserInitiated, false, GiftCardNumber);
                //Ticket end:#30620 .by rupesh
                if (GiftResponse != null && GiftResponse.Id == 0)
                {
                    IsVerified = true;
                    IsGiftCardBalance = false;
                    IsTopupGiftCard = false;
                    IsGiftFirstTime = true;

                    GiftCardExpireMsg = Extensions.GetGiftCardExpiredMsg(); // #90944 by Pratik
                }
                else if (GiftResponse != null && GiftResponse.Id != 0)
                {
                    IsGiftCardBalance = true;
                    IsVerified = true;
                    GiftCardBalance = GiftResponse.Amount - GiftResponse.SpentAmount;
                    IsTopupGiftCard = true;
                    IsGiftFirstTime = false;

                    GiftCardExpireMsg = GiftResponse.ExpiryMsg; // #90944 by Pratik

                    return;
                }
            };
            GiftFromName = CustomerDetail?.FullName;
            GiftFromEmail = CustomerDetail?.Email;
        }

    }
}
