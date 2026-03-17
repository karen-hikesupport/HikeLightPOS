using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.UserControls;
using HikePOS.ViewModels;

namespace HikePOS
{

    public partial class ParkSalePagePhone : BaseContentPage<ParkSaleViewModel>
    {
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        public static readonly BindableProperty StatusButtonTextProperty =
                BindableProperty.Create("StatusButtonText", typeof(string), typeof(ParkSalePage), "All");

        //public string StatusButtonText
        //{
        //  get { return (string)GetValue(StatusButtonTextProperty); }
        //}

        public string StatusButtonText
        {
            get { return (string)GetValue(StatusButtonTextProperty); }
            set { SetValue(StatusButtonTextProperty, value); }
        }

        public Grid _IncludeDiscardedSaleView
        {
            get
            {
                return IncludeDiscardedSaleView;
            }
        }
        public Grid _SearchSaleView
        {
            get
            {
                return SearchSaleView;
            }
        }

        public BorderLessEntry _SearchSaleEntry
        {
            get
            {
                return SearchSaleEntry;
            }
        }

        public ParkSalePagePhone()
        {
			try
			{
                InitializeComponent();
			}
			catch (Exception ex)
			{
				ex.Track();
			}
            Title = "Sales history";
            NavigationPage.SetHasNavigationBar(this, false);
            //SearchSaleView.TranslateTo(0, -64, 0);
        }

        protected override void OnAppearing()
        {
            if(HasPushedModally)
            {
                return;
            }
            base.OnAppearing();

            Task.Run(() => {
                MainThread.BeginInvokeOnMainThread(()=>
                {
                    try
                    {
                        if (!ViewModel.HasInitialized)
                        {
                            // _ = ParkSaleFilterOutletMenuView.LoadOutlets();
                            ViewModel.HasInitialized = true;
                        }
                        //Ticket start:#22083 iPad : Key board open and closed automatically in sales history.by rupesh
                        SearchSaleEntry.IsEnabled = true;
                        ViewModel.OnAppearingCall();
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                });
             });
             
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (HasPushedModally)
            {
                return;
            }

            Task.Run(() =>
            {
                MainThread.BeginInvokeOnMainThread(()=>
                {
                    try
                    {
                        SearchSaleEntry.IsEnabled = false;
                        //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
                        if (ViewModel.SelectedInvoiceToUpdateCustomer != null)
                            ViewModel.CloseUpdateCustomerName(ViewModel.SelectedInvoiceToUpdateCustomer);
                        //Ticket end:#45648 .by rupesh

                        ViewModel.OnDisappearingCall();
                    }
                    catch (Exception ex)
                    {
                        ex.Track();
                    }
                });
            });

        }
        //Ticket end:#22083 .by rupesh

        protected override void OnLoaded()
        {
            base.OnLoaded();
            SearchSaleEntry.Unfocused += ViewModel.SearchSaleEntry_Unfocused;

        }

        private async void ParkedListView_RemainingItemsThresholdReached(object sender, EventArgs e)
        {
            var items = ((CollectionView)sender).ItemsSource as IList;

            if (isLoading || items.Count == 0)
                return;

            //hit bottom!
            if (items != null)
            {
                isLoading = true;
                await ViewModel.LoadMoreSale();
                isLoading = false;
            }
        }

       /* private async void ParkedListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e?.CurrentSelection != null && e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is InvoiceDto)
            {
                await ViewModel.OpenInvoiceDetailView((InvoiceDto)e.CurrentSelection[0]);
                //ParkedListView.SelectedItem = null;
            }
            ParkedListView.SelectedItem = null;
            //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
            if (ViewModel.SelectedInvoiceToUpdateCustomer != null)
                ViewModel.CloseUpdateCustomerName(ViewModel.SelectedInvoiceToUpdateCustomer);
            //Ticket end:#45648 .by rupesh
        }*/

        void SliderMenuHandle_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
                //_navigationService.MainPage.IsPresented = !_navigationService.MainPage.IsPresented;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        bool isLoading = false;
        async void ParkedListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            var items = ((ListView)sender).ItemsSource as IList;

            if (isLoading || items.Count == 0)
                return;

            //hit bottom!
            if (items != null && e.Item == items[items.Count - 1])
            {
                isLoading = true;
                await ViewModel.LoadMoreSale();
                isLoading = false;
            }
        }

        public void ScrollFirst()
        {
            ParkedListView.ScrollTo(0);
        }

        async void FilterOptionHandle_ItemChangedAsync(object sender, string e)
        {
            if (!string.IsNullOrEmpty(e))
            {
                //START ticket #76208 IOS:FR:Terms of payments by Pratik
                ViewModel.IsOverdue = false;
                ViewModel.IsOverdueDisplay = false;
                if (e == "OnAccount")
                {
                    ViewModel.IsOverdueDisplay = true;
                    ViewModel.IsOpenIncludeDiscardPopUp = true;
                    await IncludeDiscardedSaleView.TranslateTo(0, 0, 200);
                }
                //End ticket #76208 Pratik

                await ViewModel.SelectFilterMenu(e);

                // FilterStatusButton.Text = ViewModel.SelectedMenu;
            }
        }

        void FilterDateRangeHandle_ItemChanged(object sender, string e)
		{
            if (!string.IsNullOrEmpty(e))
			{
				ViewModel.SelectFilterDateRange(e);
			}
        }

        //Added by rupesh to filter by outlet
        void FilterOutletHandle_ItemChanged(object sender, OutletDto_POS e)
        {
            if (e != null)
            {
                ViewModel.FilterByOutlet(e);
            }
        }


   
    }
}
