using System;
using HikePOS.Models;
using HikePOS.ViewModels;
using System.Threading.Tasks;
using System.Threading;

namespace HikePOS
{

    public partial class SearchProductPage : PopupBasePage<SearchProductViewModel>
    {
        public SearchProductPage()
        {
            InitializeComponent();          
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Phone && App.Instance.Hud.HasSafeArea())
                {
                    Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() =>
                    {
                        mainGrd.Margin = new Thickness(0, -20, 0, 0);
                    });
                }
                if (ViewModel != null)
                {
                    ViewModel.SearchProducts = new System.Collections.ObjectModel.ObservableCollection<EnterSaleItemDto>();
                }
                //#97186 By PR
                Dispatcher.Dispatch(async () =>
                {
                    await Task.Delay(100);
                    SearchEntry.Focus();
                });
                //#97186 By PR

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.CloseTapped();
            }
        }

    }
}
