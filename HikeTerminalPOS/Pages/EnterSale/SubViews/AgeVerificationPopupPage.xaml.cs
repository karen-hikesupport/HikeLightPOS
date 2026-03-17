using System;
using System.Threading.Tasks;
using HikePOS.Resources;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class AgeVerificationPopupPage : PopupBasePage<BaseViewModel>
    {
        public event EventHandler<AgeVerificationEventArgs> Confirmed;

        private bool IsClick;
        private string Base64 = null;
        public int Age;
        private int AgeLimit
        {
            get
            {
                return Age == 1 ? 21 : 18;
            }
        }
        private DateTime? DateOfBirth;
        private bool IsAgeVerfied;
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(AgeVerificationPopupPage), string.Empty, BindingMode.TwoWay, propertyChanged: HandleBindingPropertyChangedDelegate);

        static void HandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            var popup = bindable as AgeVerificationPopupPage;
        }



        public AgeVerificationPopupPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
        //Ticket End:#14391.by rupesh

        async void SaveHandle_Clicked(object sender, System.EventArgs e)
        {
            if (Base64 == null && !IsAgeVerfied)
                return;

            if (IsClick)
                return;
            IsClick = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? Helpers.AppFeatures.AndroidMSecond : Helpers.AppFeatures.IOSMSecond).Wait();
                IsClick = false;
            });
            await Close();
            Confirmed?.Invoke(this, new AgeVerificationEventArgs(Base64, AgeLimit, DateOfBirth));
        }

        async void CancelHandle_Clicked(object sender, System.EventArgs e)
        {
            if (IsClick)
                return;
            IsClick = true;
            _ = Task.Run(() =>
            {
                Task.Delay(DeviceInfo.Platform == DevicePlatform.Android ? Helpers.AppFeatures.AndroidMSecond : Helpers.AppFeatures.IOSMSecond).Wait();
                IsClick = false;
            });
            await Close();
            Confirmed?.Invoke(this, null);

        }

        public async Task Close()
        {
            try
            {
                if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
                    await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private async void ChooseImage_Clicked(object sender, EventArgs e)
        {
            var file = await MediaPicker.Default.PickPhotoAsync();
            if (file != null)
            {
                byte[] bytes = File.ReadAllBytes(file.FullPath);
                // Get length of file in bytes
                long fileSizeInBytes = bytes.Length;
                var sizeInBytes = 4 * Math.Ceiling(Convert.ToDouble(fileSizeInBytes / 3)) * 0.5624896334383812;
                var sizeInKb = sizeInBytes / 1000;
                if (sizeInKb <= 5000)
                {

                    Base64 = Convert.ToBase64String(bytes);
                    ImagePreview.IsVisible = true;
                    Image.Source = file.FullPath;
                    SizeErrorLabel.IsVisible = false;

                }
                else
                {
                    SizeErrorLabel.IsVisible = true;
                }
            }
        }

        private void CancelImage_Clicked(object sender, EventArgs e)
        {
            Image.Source = null;
            ImagePreview.IsVisible = false;
            Base64 = null;
        }
        private void BirthDatePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            DateTime birthDate = e.NewDate;
            int age = CalculateAge(birthDate);
            if (age < AgeLimit)
            {
                AgeErrorLabel.Text = $"\uf00d; Age Verification Failed – Customer must be {AgeLimit} or older";
                AgeErrorLabel.TextColor = AppColors.RedButtonBackgroundColor;
                IsAgeVerfied = false;
            }
            else
            {
                AgeErrorLabel.Text = "\uf058; Age Verified – Customer is eligible";
                AgeErrorLabel.TextColor = AppColors.HikeColor;
                IsAgeVerfied = true;

            }
            AgeErrorLabel.IsVisible = true;
        }

        private int CalculateAge(DateTime birthDate)
        {
            DateOfBirth = birthDate;
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            // Adjust if the birthday hasn't occurred yet this year
            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }

    }
    public class AgeVerificationEventArgs : EventArgs
    {
        public string Base64 { get; }
        public int Age { get; }
        public DateTime? DateOfBirth { get; }

        public AgeVerificationEventArgs(string base64, int age, DateTime? dob)
        {
            Base64 = base64;
            Age = age;
            DateOfBirth = dob;
        }
    }

}