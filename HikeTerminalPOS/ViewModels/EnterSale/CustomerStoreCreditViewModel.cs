using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using HikePOS.Models.Customer;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class CustomerStoreCreditViewModel : BaseViewModel
    {
        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;
        AddCustomerStoreCreditPage page;

        //Start #77145 iPAD: Store credit visibility by Pratik
        public event EventHandler<CreditBalanceHistoryDto> CreditAdded;
        //End #77145 by Pratik
        ObservableCollection<CreditBalanceHistoryDto> _HistoryList { get; set; }
        public ObservableCollection<CreditBalanceHistoryDto> HistoryList
        {
            get{
                return _HistoryList;
            }
            set{
                _HistoryList = value;
                SetPropertyChanged(nameof(HistoryList));
            }
        }
        public int CustomerId { get; set; }

        #region Constructor/OnAppearing
        public CustomerStoreCreditViewModel()
        {
            page = new AddCustomerStoreCreditPage();
            page.ViewModel.CreditAdded += ViewModel_CreditAdded;
           
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            LoadData();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
        }
        #endregion


        #region Command
        public ICommand StoreCreditCommand => new Command(StoreCreditTapped);
        #endregion

        #region Command Execution

        public async void StoreCreditTapped()
        {
            try
            {
                page.ViewModel.CustomerId = CustomerId;
                await NavigationService.PushModalAsync(page);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        #endregion

        #region Methods

        async void ViewModel_CreditAdded(object sender, Models.Customer.CreditBalanceHistoryDto e)
        {
            AddData(e);
            //Start #77145 iPAD: Store credit visibility by Pratik
            CreditAdded?.Invoke(this,e);
            //End #77145 by Pratik
            await Close();

        }

        public async void LoadData()
        {
            using (new Busy(this, true))
            {
                try
                {
                    HistoryList = new ObservableCollection<CreditBalanceHistoryDto>();
                    if (customerService == null)
                    {
                        customerService = new CustomerServices(customerApiService);
                    }

                    var tmpHistoryList = new ObservableCollection<CreditBalanceHistoryDto>();

                    if (App.Instance.IsInternetConnected)
                    {
                        tmpHistoryList = await customerService.GetRemoteCustomerCreditBalance(Fusillade.Priority.UserInitiated, true, "", CustomerId);
                    }
                    else
                    {
                        tmpHistoryList = customerService.GetLocalCustomerCreditBalance(CustomerId);
                    }

                    //Ticket #12562 Start : Store credits not applying to a customer. By Nikhil	 
                    if (tmpHistoryList != null)
                        HistoryList = new ObservableCollection<CreditBalanceHistoryDto>(tmpHistoryList.OrderByDescending(x => x.CreationTime));
                    else HistoryList = new ObservableCollection<CreditBalanceHistoryDto>();
                   //Ticket #12562 End. By Nikhil

                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
        }

        public void AddData(CreditBalanceHistoryDto data)
        {
            try
            {
                if (data != null)
                {
                    if (HistoryList == null)
                    {
                        HistoryList = new ObservableCollection<CreditBalanceHistoryDto>();
                    }
                    HistoryList.Insert(0, data);
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }

        #endregion
    }
}
