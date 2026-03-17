using System;
using System.Threading.Tasks;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class PromptPopupPage : PopupBasePage<BaseViewModel>
    {
        public event EventHandler<string> Saved;
        //Start ticket #60456 By Praik 
        public event EventHandler<string> SavedAndPrint;
        //End ticket #60456 By Praik 
        public event EventHandler<bool> Cancel;
        private bool IsClick;

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(PromptPopupPage), string.Empty, BindingMode.TwoWay, propertyChanged: HandleBindingPropertyChangedDelegate);

        static void HandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            var popup = bindable as PromptPopupPage;
            popup.inputText.Placeholder = newValue?.ToString();
        }


        //public string InputTextValue
        //{
        //	get { return (string)GetValue(InputTextValueProperty); }
        //	set { SetValue(InputTextValueProperty, value); }
        //}

        //public static readonly BindableProperty InputTextValueProperty =
        //         BindableProperty.Create(nameof(InputTextValue), typeof(string), typeof(PromptPopupPage), null, BindingMode.TwoWay, null, HandleBindingPropertyChangedDelegate);

        //     static void HandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        //     {
        //var popup = bindable as PromptPopupPage;
        //popup.inputText.Text = (string)newValue;
        //}

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly BindableProperty DescriptionProperty =
            BindableProperty.Create(nameof(Description), typeof(string), typeof(PromptPopupPage), string.Empty, BindingMode.TwoWay, propertyChanged: HandleDescriptionBindingPropertyChangedDelegate);

        static void HandleDescriptionBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            var popup = bindable as PromptPopupPage;
            if (string.IsNullOrEmpty(newValue?.ToString()) || string.IsNullOrWhiteSpace(newValue?.ToString()))
            {
                popup.DescriptionLabel.IsVisible = false;
            }
            else
            {
                popup.DescriptionLabel.IsVisible = true;
                popup.DescriptionLabel.Text = newValue?.ToString();
            }
        }

        //Start ticket #60456 By Praik 
        public string SaveAndPrintButtonText
        {
            get { return (string)GetValue(SaveAndPrintButtonTextProperty); }
            set { SetValue(SaveAndPrintButtonTextProperty, value); }
            
        }

        public static readonly BindableProperty SaveAndPrintButtonTextProperty =
            BindableProperty.Create(nameof(SaveAndPrintButtonText), typeof(string), typeof(PromptPopupPage), string.Empty, BindingMode.TwoWay, propertyChanged: HandleSaveeAndPrintButtonTextBindingPropertyChangedDelegate);

        static void HandleSaveeAndPrintButtonTextBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            if (!string.IsNullOrEmpty(newValue?.ToString()) && !string.IsNullOrWhiteSpace(newValue?.ToString()))
            {
                var popup = bindable as PromptPopupPage;
                popup.YesPrintButton.Text = newValue?.ToString();
                popup.YesPrintButton.IsVisible = true;
                popup.YesPrintButton.WidthRequest = 150;
            }
            else
            {
                var popup = bindable as PromptPopupPage;
                popup.YesPrintButton.IsVisible = false;
            }
        }

        async void SaveAndPrintHandle_Clicked(object sender, System.EventArgs e)
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
            SavedAndPrint?.Invoke(this, inputText.Text);
        }
        //End ticket #60456 By Praik 

        public string YesButtonText
        {
            get { return (string)GetValue(YesButtonTextProperty); }
            set { SetValue(YesButtonTextProperty, value); }
        }

        public static readonly BindableProperty YesButtonTextProperty =
            BindableProperty.Create(nameof(YesButtonText), typeof(string), typeof(PromptPopupPage), string.Empty, BindingMode.TwoWay, propertyChanged: HandleYesButtonTextBindingPropertyChangedDelegate);

        static void HandleYesButtonTextBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            if (!string.IsNullOrEmpty(newValue?.ToString()) && !string.IsNullOrWhiteSpace(newValue?.ToString()))
            {
                var popup = bindable as PromptPopupPage;
                popup.YesButton.Text = newValue?.ToString();
            }
        }

        public string NoButtonText
        {
            get { return (string)GetValue(NoButtonTextProperty); }
            set { SetValue(NoButtonTextProperty, value); }
        }

        public static readonly BindableProperty NoButtonTextProperty =
            BindableProperty.Create(nameof(NoButtonText), typeof(string), typeof(PromptPopupPage), string.Empty, BindingMode.TwoWay, propertyChanged: HandleNoButtonTextBindingPropertyChangedDelegate);

        static void HandleNoButtonTextBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            if (!string.IsNullOrEmpty(newValue?.ToString()) && !string.IsNullOrWhiteSpace(newValue?.ToString()))
            {
                var popup = bindable as PromptPopupPage;
                popup.NoButton.Text = newValue?.ToString();
            }
        }


        public PromptPopupPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            inputText.Text = string.Empty;

            //Ticket start:#14391 iOS - Keyboard Pop Up.by rupesh
            inputText.IsEnabled = true;

        }
        protected override void OnDisappearing()
        {
            inputText.IsEnabled = false;
            base.OnDisappearing();
        }
        //Ticket End:#14391.by rupesh

        async void SaveHandle_Clicked(object sender, System.EventArgs e)
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
            Saved?.Invoke(this, inputText.Text);
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
            Cancel?.Invoke(this, false);
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
    }
}