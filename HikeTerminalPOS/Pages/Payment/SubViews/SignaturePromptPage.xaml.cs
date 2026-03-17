using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using HikePOS.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace HikePOS
{
    public partial class SignaturePromptPage : PopupBasePage<BaseViewModel>
    {
        public event EventHandler<ImageSource> Saved;

        public event EventHandler<bool> Cancel;

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly BindableProperty DescriptionProperty =
            BindableProperty.Create(nameof(Description),typeof(string),typeof(SignaturePromptPage), string.Empty, BindingMode.TwoWay, propertyChanged: HandleDescriptionBindingPropertyChangedDelegate);

        static void HandleDescriptionBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            var popup = bindable as SignaturePromptPage;
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


        public string YesButtonText
        {
            get { return (string)GetValue(YesButtonTextProperty); }
            set { SetValue(YesButtonTextProperty, value); }
        }

        public static readonly BindableProperty YesButtonTextProperty =
            BindableProperty.Create(nameof(YesButtonText), typeof(string), typeof(SignaturePromptPage), string.Empty, BindingMode.TwoWay, propertyChanged: HandleYesButtonTextBindingPropertyChangedDelegate);

        static void HandleYesButtonTextBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            if (!string.IsNullOrEmpty(newValue?.ToString()) && !string.IsNullOrWhiteSpace(newValue?.ToString()))
            {
                var popup = bindable as SignaturePromptPage;
                popup.YesButton.Text = newValue?.ToString();
            }
        }

        public string NoButtonText
        {
            get { return (string)GetValue(NoButtonTextProperty); }
            set { SetValue(NoButtonTextProperty, value); }
        }

        public static readonly BindableProperty NoButtonTextProperty =
            BindableProperty.Create(nameof(NoButtonText), typeof(string), typeof(SignaturePromptPage), string.Empty, BindingMode.TwoWay, propertyChanged: HandleNoButtonTextBindingPropertyChangedDelegate);

        static void HandleNoButtonTextBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            if (!string.IsNullOrEmpty(newValue?.ToString()) && !string.IsNullOrWhiteSpace(newValue?.ToString()))
            {
                var popup = bindable as SignaturePromptPage;
                popup.NoButton.Text = newValue?.ToString();
            }
        }


        DrawingView signatureView;
        public SignaturePromptPage()
        {
            InitializeComponent();
            setSignaturePadView();
        }

        void setSignaturePadView()
        {
            signatureView = new DrawingView();
            signatureView.BackgroundColor = Colors.White;
            signatureView.Lines = new ObservableCollection<IDrawingLine>();
            signatureView.IsMultiLineModeEnabled = true;
            signatureView.ShouldClearOnFinish = true;
            signatureView.LineWidth = 2;
            signatureView.LineColor = Colors.Black;
            frameSignaturePad.Content = signatureView; 
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        async void SaveHandle_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                //var iostream = this.signatureView.GetImage(Acr.XamForms.SignaturePad.ImageFormatType.Jpg);
                var iostream = await signatureView.GetImageStream(200, 200);
                ImageSource imagesource = ImageSource.FromStream(() => iostream);
                if (imagesource != null)
                {
                    Saved?.Invoke(this, imagesource);
                    await Close();
                }
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent("1 SignaturePromptPage.xaml.cs SaveHandle_Clicked() ");
                ex.Track();
            }
        }

        async void CancelHandle_Clicked(object sender, System.EventArgs e)
        {
            Cancel?.Invoke(this, true);
            await Close();
        }

        public async Task Close()
        {
            try
            {

                if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
                {
                    await Navigation.PopModalAsync();
                    Saved?.Invoke(this, null);
                }
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent("1 SignaturePromptPage.xaml.cs Close() ");
                ex.Track();
            }
        }
    }
}
