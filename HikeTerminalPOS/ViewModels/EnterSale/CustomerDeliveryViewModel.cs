using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Customer;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS.ViewModels
{
    public class CustomerDeliveryViewModel : BaseViewModel
    {
         private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;

        ObservableCollection<CustomerAddressDto> _DeliveryAddresseList { get; set; }
        public ObservableCollection<CustomerAddressDto> DeliveryAddressList { get { return _DeliveryAddresseList; } set { _DeliveryAddresseList = value; SetPropertyChanged(nameof(DeliveryAddressList)); } }

        #region Properties
        public event EventHandler<CustomerAddressDto> DeliverAddressAdded;
        public event EventHandler<CustomerAddressDto> DeliveryAddressSelectionClosed;

        public CustomerAddressDto _DeliveryAddress { get; set; }
        public CustomerAddressDto DeliveryAddress { get { return _DeliveryAddress; } set { _DeliveryAddress = value; SetPropertyChanged(nameof(DeliveryAddress)); } }

        ObservableCollection<CountriesDto> _CountryList { get; set; }
        public ObservableCollection<CountriesDto> CountryList { get { return _CountryList; } set { _CountryList = value; SetPropertyChanged(nameof(CountryList)); } }

        CountriesDto _SelectedCountry { get; set; }
        public CountriesDto SelectedCountry { get { return _SelectedCountry; } set { _SelectedCountry = value; SetPropertyChanged(nameof(SelectedCountry)); } }

        bool _IsDeliveryAddressExist { get; set; } = true;
        public bool IsDeliveryAddressExist { get { return _IsDeliveryAddressExist; } set { _IsDeliveryAddressExist = value; SetPropertyChanged(nameof(IsDeliveryAddressExist)); } }

        bool _IsEditDeliveryAddress { get; set; } = false;
        public bool IsEditDeliveryAddress { get { return _IsEditDeliveryAddress; } set { _IsEditDeliveryAddress = value; SetPropertyChanged(nameof(IsEditDeliveryAddress)); } }

        string _Title { get; set; } = "Delivery address";
        public string Title { get { return _Title; } set { _Title = value; SetPropertyChanged(nameof(Title)); } }


        public CustomerDto_POS NewCustomer;
        #endregion

        #region LifeCycle

        public CustomerDeliveryViewModel()
        {
            customerService = new CustomerServices(customerApiService);
            DeliveryAddressList = new ObservableCollection<CustomerAddressDto>();

        }
        public async override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                if (NewCustomer == null)
                    return;

                var deliveryAddress = new CustomerAddressDto();
                var tempTitle = "Delivery address" + " - " + NewCustomer.FullName;
                if (!string.IsNullOrEmpty(NewCustomer.CompanyName))
                    tempTitle = tempTitle + "(" + NewCustomer.CompanyName + ")";
                Title = tempTitle;
                IsEditDeliveryAddress = false;
                GetLocalCustomerAddresses();

                if (DeliveryAddressList == null)
                     DeliveryAddressList = new ObservableCollection<CustomerAddressDto>();

                if (DeliveryAddressList.Count > 0)
                {
                    IsDeliveryAddressExist = true;
                }
                else
                {
                    IsDeliveryAddressExist = await IsExistCustomerAddress();
                    if (IsDeliveryAddressExist)
                        await GetCustomerAddresses();

                }
                GetCountries();

                if (_navigationService.RootPage is BaseContentPage<EnterSaleViewModel>)
                {
                    var invoice = ((BaseContentPage<EnterSaleViewModel>)_navigationService.RootPage).ViewModel.invoicemodel?.Invoice;
                    if (invoice?.DeliveryAddressId != null && invoice?.DeliveryAddressId > 0)
                    {
                        var fisrtdata = DeliveryAddressList.FirstOrDefault(x => x.Id == invoice?.DeliveryAddressId);
                        if(fisrtdata!=null)
                            fisrtdata.IsSelected = true;
                    }

                }
                if (!IsDeliveryAddressExist)
                {
                    if (string.IsNullOrEmpty(deliveryAddress.ReceiverName) && !string.IsNullOrEmpty(NewCustomer.FullName))
                        deliveryAddress.ReceiverName = NewCustomer.FullName;
                    if (string.IsNullOrEmpty(deliveryAddress.ReceiverCompanyName) && !string.IsNullOrEmpty(NewCustomer.CompanyName))
                        deliveryAddress.ReceiverCompanyName = NewCustomer.CompanyName;
                    if (string.IsNullOrEmpty(deliveryAddress.ReceiverPhone) && !string.IsNullOrEmpty(NewCustomer.Phone))
                        deliveryAddress.ReceiverPhone = NewCustomer.Phone;

                }
                DeliveryAddress = deliveryAddress;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            

        }

        #endregion

        #region Command
        public ICommand SaveCommand => new Command(SaveTapped);
        public ICommand CloseCommand => new Command(CloseTapped);
        public ICommand SaveSelectCommand => new Command(SaveSelectTapped);
        public ICommand DeleteCommand => new Command(DeleteTapped);
        public ICommand AddNewCommand => new Command(AddNewAddress);
        public ICommand EditAddressCommand => new Command<CustomerAddressDto>(EditAddressTapped);
        public ICommand DelivereHereCommand => new Command<CustomerAddressDto>(DelivereHereTapped);
        #endregion

        #region Command Execution

        public async void SaveTapped()
        {
            try
            {
                await SaveAddress();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void CloseTapped()
        {
            ClosePopupTapped();
            //Ticket start:#28369 iPad: delivery address should be added from the sales history.by rupesh
            DeliveryAddressSelectionClosed.Invoke(this, null);
            //Ticket end:#28369 .by rupesh
        }

        async void DeleteTapped()
        {
            try
            {
                await DeleteAddress();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);        
            }
        }

        async void SaveSelectTapped()
        {
            try
            {
                var result = await SaveAddress();
                if (result)
                {
                    DeliverHere(DeliveryAddress);
                    DeliverAddressAdded.Invoke(this, DeliveryAddress);
                    ClosePopupTapped();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void AddNewAddress()
        {
            try
            {
                IsDeliveryAddressExist = false;
                IsEditDeliveryAddress = false;
                var deliveryAddress = new CustomerAddressDto();
                if (string.IsNullOrEmpty(deliveryAddress.ReceiverName) && !string.IsNullOrEmpty(NewCustomer.FullName))
                    deliveryAddress.ReceiverName = NewCustomer.FullName;
                if (string.IsNullOrEmpty(deliveryAddress.ReceiverCompanyName) && !string.IsNullOrEmpty(NewCustomer.CompanyName))
                    deliveryAddress.ReceiverCompanyName = NewCustomer.CompanyName;
                if (string.IsNullOrEmpty(deliveryAddress.ReceiverPhone) && !string.IsNullOrEmpty(NewCustomer.Phone))
                    deliveryAddress.ReceiverPhone = NewCustomer.Phone;
                DeliveryAddress = deliveryAddress;
            }
            catch (Exception ex)
            {
                 Debug.WriteLine(ex.Message);
            }
        }

        void EditAddressTapped(CustomerAddressDto deliverAddress)
        {
            try
            {
                EditAddress(deliverAddress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        void DelivereHereTapped(CustomerAddressDto deliverAddress)
        {
            try
            {
                DeliverAddressAdded.Invoke(this, deliverAddress);
                ClosePopupTapped();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Methods
        public async Task<bool> IsExistCustomerAddress()
        {

            try
            {

                using (new Busy(this, true))
                {
                    var result = await customerService.IsExistCustomerAddress(Fusillade.Priority.UserInitiated, true, NewCustomer.Id);
                    return result;

                };
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

        }
        public void GetLocalCustomerAddresses()
        {
            try
            {
                using (new Busy(this, true))
                {
                    DeliveryAddressList = customerService.GetLocalDeliveryAddresses(NewCustomer.Id);
                };
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public async Task GetCustomerAddresses()
        {
            try
            {

                using (new Busy(this, true))
                {
                    DeliveryAddressList = await customerService.GetRemoteCustomerAddresses(Fusillade.Priority.UserInitiated, true, NewCustomer.Id);
                };
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        public async Task<bool> SaveAddress()
        {
            try
            {
                DeliveryAddress.CustomerId = NewCustomer.Id;
                if (string.IsNullOrEmpty(DeliveryAddress.ReceiverName))
                {
                    App.Instance.Hud.DisplayToast("Please add ReceiverName");
                    return false;
                }
                else if (string.IsNullOrEmpty(DeliveryAddress.Address1) || String.IsNullOrWhiteSpace(DeliveryAddress.Address1))
                {
                    App.Instance.Hud.DisplayToast("Please add street address");
                    return false;
                }
                else if (string.IsNullOrEmpty(DeliveryAddress.City) || String.IsNullOrWhiteSpace(DeliveryAddress.City))
                {
                    App.Instance.Hud.DisplayToast("Please add city");
                    return false;
                }

                else if (string.IsNullOrEmpty(DeliveryAddress.State) || String.IsNullOrWhiteSpace(DeliveryAddress.State))
                {
                    App.Instance.Hud.DisplayToast("Please add state");
                    return false;
                }
                else if (string.IsNullOrEmpty(DeliveryAddress.PostCode) || String.IsNullOrWhiteSpace(DeliveryAddress.PostCode))
                {
                    App.Instance.Hud.DisplayToast("Please add postcode");
                    return false;
                }
                else if (string.IsNullOrEmpty(SelectedCountry?.value) || String.IsNullOrWhiteSpace(SelectedCountry?.value))
                {
                    App.Instance.Hud.DisplayToast("Please add country");
                    return false;
                }

                DeliveryAddress.Country = SelectedCountry.value;

                using (new Busy(this, true))
                {

                    var deliveryAddress = await customerService.CreateOrUpdateRemoteCustomerAddress(Fusillade.Priority.UserInitiated, true, DeliveryAddress);
                    DeliveryAddress = deliveryAddress;
                    if(deliveryAddress != null)
                    {
                        IsDeliveryAddressExist = true;
                        IsEditDeliveryAddress = false;
                        var found = DeliveryAddressList.FirstOrDefault(x => x.Id == deliveryAddress.Id);
                        if(found != null)
                        {
                            int i = DeliveryAddressList.IndexOf(found);
                            DeliveryAddressList[i] = deliveryAddress;

                        }
                        else
                        {
                            DeliveryAddressList.Add(deliveryAddress);

                        }
                        return true;

                    }
                    return false;

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
       
        public async Task DeleteAddress()
        {
            try
            {
                var result = await App.Alert.ShowAlert("Delete this delivery address?", "Click 'Yes' to permanently delete this delivery address.?", "Yes", "No");
                if(result)
                {
                    using (new Busy(this, true))
                    {

                        var isDeleted = await customerService.DeleteRemoteDeliveryAddress(Fusillade.Priority.UserInitiated, true, DeliveryAddress.Id);
                        if (isDeleted)
                        {
                            IsDeliveryAddressExist = true;
                            IsEditDeliveryAddress = false;
                            DeliveryAddressList.Remove(DeliveryAddress);
                            if(DeliveryAddressList.Count == 0)
                            AddNewAddress();
                            
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

        }

        public void EditAddress(CustomerAddressDto deliveryAddress)
        {
            IsDeliveryAddressExist = false;
            //Ticket start:#28371 iPad: edit time delete button should not be showing delivery address.by rupesh
            IsEditDeliveryAddress = !deliveryAddress.IsSelected;
            //Ticket end:#28371 .by rupesh
            if (string.IsNullOrEmpty(deliveryAddress.ReceiverName) && !string.IsNullOrEmpty(NewCustomer.FullName))
                deliveryAddress.ReceiverName = NewCustomer.FullName;
            else if (string.IsNullOrEmpty(deliveryAddress.ReceiverCompanyName) && !string.IsNullOrEmpty(NewCustomer.CompanyName))
                deliveryAddress.ReceiverCompanyName = NewCustomer.CompanyName;
            else if (string.IsNullOrEmpty(deliveryAddress.ReceiverPhone) && !string.IsNullOrEmpty(NewCustomer.Phone))
                deliveryAddress.ReceiverPhone = NewCustomer.Phone;
            DeliveryAddress = deliveryAddress;
            GetCountries();
        }
        public void DeliverHere(CustomerAddressDto customerAddress)
        {
            customerAddress.IsSelected = true;
            DeliveryAddress = customerAddress;
        }


        public void GetCountries()
        {
            try
            {
                if (Settings.AllCountries != null)
                {
                    CountryList = Settings.AllCountries;

                    var objCountry = CountryList.FirstOrDefault(x => DeliveryAddress != null && DeliveryAddress.Address1 != null && DeliveryAddress.Country != null && x.value == DeliveryAddress.Country);
                    if (objCountry != null)
                        SelectedCountry = objCountry;
                    else
                        SelectedCountry = GetStoreCountry();//Store country if customer country not available
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        CountriesDto GetStoreCountry()
        {

            CountriesDto country = null;
            try
            {
                var countryCode = Settings.StoreCountryCode;
                if (!string.IsNullOrEmpty(countryCode))
                {
                    country = CountryList.FirstOrDefault(x => string.Equals(x.value, countryCode, StringComparison.InvariantCultureIgnoreCase));
                }
                
            }
            catch (Exception e)
            {
                e.Track();
                Debug.WriteLine("GetStoreCountry in GetStoreCountry : " + e.Message + e.StackTrace);
                country = null;
            }

            return country;
        }

        #endregion
    }
}

