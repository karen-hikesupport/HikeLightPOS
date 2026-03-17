using System;
using System.Windows.Input;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class BaseViewModel : BaseNotify
    {
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        bool _isBusy;
        public event EventHandler IsBusyChanged;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (SetPropertyChanged(ref _isBusy, value))
                {
                    if (IsBusyChanged != null)
                        IsBusyChanged(this, new EventArgs());
                }
            }
        }

        bool _isLoad;
        public bool IsLoad
        {
            get
            {
                return _isLoad;
            }
            set
            {
                SetPropertyChanged(ref _isLoad, value);
            }
        }

        string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                SetPropertyChanged(ref _title, value);
            }
        }

        public bool HasInitialized
        {
            get;
            set;
        }

        public bool EventCallRunning { get; set; }

        public bool IsClosePopup
        {
            get;
            set;
        }

        public bool IsOpenPopup
        {
            get;
            set;
        }

        INavigation _navigationservice;
        public INavigation NavigationService
        {
            get
            {
                return _navigationservice;
            }
            set
            {
                SetPropertyChanged(ref _navigationservice, value);
            }
        }

        public virtual void OnAppearing() { }

        public virtual void OnDisappearing() { }
        public virtual void OnAppeared() { }


        #region Command

        public ICommand ClosePopupCommand => new Command(ClosePopupTapped);

        #endregion

        #region Command Execution

        public async Task ClosePopupTapped_Task()
        {
            try
            {
                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                {
                    if (_navigationService.IsFlyoutPage)
                    {
                        var lastpage = _navigationService.NavigatedPage;
                        if (lastpage != null && lastpage is BaseContentPage<EnterSaleViewModel> baseContentPage && NavigationService.ModalStack.Count == 1)//Ticket:#97721.by rupesh
                        {
                            baseContentPage.ViewModel.IsClosePopup = true;
                        }
                        else if (lastpage != null && lastpage is BaseContentPage<CashRegisterViewModel> baseContentPage1)
                        {
                            baseContentPage1.ViewModel.IsClosePopup = true;
                        }
                    }
                    await NavigationService.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async void ClosePopupTapped()
        {
            await ClosePopupTapped_Task();
        }

        public async Task Close()
        {
            try
            {
                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                {
                    await NavigationService.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        #endregion

        public class Busy : IDisposable
        {
            readonly object _sync = new object();
            readonly BaseViewModel _viewModel;
            readonly bool _showProgressView;
            public Busy(BaseViewModel viewModel, bool showProgressView, string displayMessage = null)
            {
                try
                {
                    if (viewModel != null)
                    {
                        _viewModel = viewModel;
                        lock (_sync)
                        {
                            _viewModel.IsBusy = true;
                            _showProgressView = showProgressView;
                            if (_showProgressView)
                            {
                                if (string.IsNullOrEmpty(displayMessage))
                                {
                                    displayMessage = LanguageExtension.Localize("Progress0_Text");
                                }
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    App.Instance?.Hud?.DisplayProgress(displayMessage);
                                });
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }

            public void Dispose()
            {
                try
                {
                    lock (_sync)
                    {
                        _viewModel.IsBusy = false;
                        if (_showProgressView)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                App.Instance?.Hud?.Dismiss();
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
        }
    }
}

