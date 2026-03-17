using System;
using HikePOS.Resources;
using HikePOS.ViewModels;
using Microsoft.Maui.Controls;
//#if IOS
//using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
//#endif
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
#if ANDROID
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
#endif

#if IOS
using Microsoft.Maui.Platform;
using UIKit;
using Foundation;
#endif

namespace HikePOS
{

    public class BaseContentPage<T> : MainBaseContentPage where T : BaseViewModel, new()
    {
        protected T _viewModel;
        public T ViewModel
        {
            get
            {
                return _viewModel ?? (_viewModel = new T());
            }
        }



        ~BaseContentPage()
        {
            _viewModel = null;
        }

        public BaseContentPage()
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
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            ViewModel.OnAppeared();
        }

    }

    public class MainBaseContentPage : ContentPage
    {
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

        public MainBaseContentPage()
        {
            HideSoftInputOnTapped = true;
            BarBackgroundColor = Colors.White;
            BarTextColor = AppColors.NavigationBarBackgroundColor;
            BackgroundColor = Colors.White;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() =>
                {
                    Padding = new Thickness(0, 0, 0, -App.Instance.Hud.GetSafeareaHeight());
                });
            }
#if ANDROID
            this.Behaviors.Add(new StatusBarBehavior
            {
                StatusBarColor = Colors.White,
                StatusBarStyle = StatusBarStyle.DarkContent
            });
#endif
            // #if IOS

            //On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetUseSafeArea(false);
            // #endif
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
        public bool HasPushedModally
        {
            get;
            private set;
        }


        protected virtual void OnLoaded()
        {
        }

        protected virtual void Initialize()
        {
        }


        protected override void OnAppearing()
        {
            if(HasPushedModally)
            {
                HasPushedModally = false;
            }
            if (!HasInitialized)
            {
                HasInitialized = true;
                OnLoaded();
            }
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            if (Navigation.ModalStack.Count > 0)
                HasPushedModally = true;
            else
                HasPushedModally = false;
            base.OnDisappearing();
        }
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
        }
    }


}

