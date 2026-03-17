using System;
using HikePOS.Resources;
using HikePOS.ViewModels;
#if ANDROID
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
#endif

using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

using NavigationPage = Microsoft.Maui.Controls.NavigationPage;


namespace HikePOS
{
    public class PopupBasePage<T> : MainPopupBasePage where T : BaseViewModel, new()
    {
        protected T _viewModel;

        public T ViewModel
        {
            get
            {
                return _viewModel ?? (_viewModel = new T());
            }
        }

        ~PopupBasePage()
        {
            _viewModel = null;
        }

        public PopupBasePage()
        {
            BindingContext = ViewModel;
            ViewModel.NavigationService = Navigation;
        }

        protected override void OnAppearing()
        {
            ViewModel.OnAppearing();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            ViewModel.OnDisappearing();
            base.OnDisappearing();
        }

    }

    public enum PopupShowDirection
    {
        TopToBottom,
        BottomToTop,
        RightToLeft,
        LeftToRight
    }

    public class MainPopupBasePage : ContentPage
    {
        public PopupShowDirection ShowDirection { get; set; } = PopupShowDirection.RightToLeft;
        public bool IsFullScreen { get; set; } = true;

        public Color BarTextColor
        {
            get;
            set;
        }

        public Color BarBackgroundColor
        {
            get;
            set;
        }

        public MainPopupBasePage()
        {
            HideSoftInputOnTapped = true;
            BarBackgroundColor = Colors.White;
            BarTextColor = AppColors.NavigationBarBackgroundColor;
            if(DeviceInfo.Platform == DevicePlatform.iOS)
                BackgroundColor = Colors.Transparent;

#if ANDROID
            this.Behaviors.Add(new StatusBarBehavior
            {
                StatusBarColor = Colors.White,
                StatusBarStyle = StatusBarStyle.DarkContent
            });
#endif

            On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetUseSafeArea(false);

            if (Parent is NavigationPage)
            {
                var nav = (NavigationPage)Parent;
                nav.BarBackgroundColor = BarBackgroundColor;
                nav.BarTextColor = BarTextColor;
            }
        }

        public bool HasInitialized
        {
            get;
            private set;
        }

        protected virtual void OnLoaded() { }

        protected virtual void Initialize() { }


        protected override void OnAppearing()
        {
            if (!HasInitialized)
            {
                HasInitialized = true;
                OnLoaded();
            }
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

    }
}

