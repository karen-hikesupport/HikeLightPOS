using System;
using System.Windows.Input;
using Fusillade;
using HikePOS.Models.Customer;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class AddCustomerStoreCreditViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;

        public event EventHandler<CreditBalanceHistoryDto> CreditAdded;

        decimal _Amount { get; set; }
        public decimal Amount
        {
            get
            {
                return _Amount;
            }
            set
            {
                _Amount = value;
                SetPropertyChanged(nameof(Amount));
            }
        }

        string _StrAmount { get; set; } = "";
        public string StrAmount
        {
            get
            {
                return _StrAmount;
            }
            set
            {
                _StrAmount = value;
                SetPropertyChanged(nameof(StrAmount));
            }
        }

        string _Notes { get; set; }
        public string Notes
        {
            get
            {
                return _Notes;
            }
            set
            {
                _Notes = value;
                SetPropertyChanged(nameof(Notes));
            }
        }

        public int CustomerId { get; set; }

        public AddCustomerStoreCreditViewModel()
        {
        }

        #region Command
        public ICommand SaveCommand => new Command(SaveTapped);
        #endregion

        #region Command Execution

        public async void SaveTapped()
        {
            try
            {
                AmountHandle();
                using (new Busy(this, true))
                {
                    if (customerService == null)
                    {
                        customerService = new CustomerServices(customerApiService);
                    }
                    if (App.Instance.IsInternetConnected)
                    {
                        var result = await customerService.UpdateRemoteCustomerCredits(Fusillade.Priority.UserInitiated, true, new Models.Customer.CreditBalanceHistoryDto()
                        {
                            CustomerId = CustomerId,
                            Note = Notes,
                            Credit = Amount
                        });

                        if (result != null && result.Id > 0)
                        {
                            //Ticket #12562 Start : Store credits not applying to a customer. By Nikhil	 
                           var customer = await customerService.GetRemoteCustomerDetail(Priority.Background, CustomerId); 
                            //Ticket #12562 End. By Nikhil

                            Notes = string.Empty;
                            Amount = 0;
                            StrAmount = "";
                            CustomerId = 0;
                            CreditAdded?.Invoke(this, result);
                            if (_navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel> || _navigationService.NavigatedPage is BaseContentPage<PaymentViewModel>)
                            {
                                var invoice = ((BaseContentPage<EnterSaleViewModel>)_navigationService.RootPage).ViewModel.invoicemodel?.Invoice;
                                if (invoice != null)
                                    invoice.CustomerDetail = customer?.result;

                            }

                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }

        void AmountHandle()
        {
            try
            {
                String newText = String.Empty;
                decimal intparseresult;
                if (decimal.TryParse(StrAmount, out intparseresult))
                {
                    Amount = Math.Round(intparseresult, 2);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        #endregion
    }
}
