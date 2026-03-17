using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using HikePOS.Models;
using HikePOS.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
    public class PeerCommunicatorViewModel : BaseViewModel
    {
        private IPeerComunicator PeerComunicator;

        public event EventHandler<CustomerDto_POS> CustomerAdded;

        CustomerViewModel customerViewModel;



        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;


        SaleServices saleService;

      
        private string _deviceInfoLabel;
        public string DeviceInfoLabel
        {
            get { return _deviceInfoLabel; }
            set
            {
                _deviceInfoLabel = value;
                SetPropertyChanged(nameof(DeviceInfoLabel));
            }
        }


        private string _message;
        

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                SetPropertyChanged(nameof(Message));
            }
        }

        ObservableCollection<OfferDto> _offers { get; set; }
        public ObservableCollection<OfferDto> Offers { get { return _offers; } set { _offers = value; SetPropertyChanged(nameof(Offers)); } }

        public InvoiceDto Displayinvoice { get; private set; }

        public PeerCommunicatorViewModel(SaleServices _saleService)
        {
            saleService = _saleService;
          
            Debug.WriteLine("PeerCommunicatorViewModel constructor......");

            // StartPeerConnection();


       

        }


        public override void OnAppearing()
        {
            Debug.WriteLine("HikePeer_OnAppearing......");

            base.OnAppearing();
        }


        public void SendPeerNotification(InvoiceDto invoice, ObservableCollection<OfferDto> offers)
        {
            Offers = offers;
            HikePeer_SendMessage(invoice);
        }

        public async Task<bool> CreateCustomer(CustomerDto_POS NewCustomer)
        {
            try
            {
                customerService = new CustomerServices(customerApiService);
                NewCustomer.Gender = -1;
                NewCustomer.CreationTime = DateTime.UtcNow;

                if (NewCustomer.BirthDate != null)
                    NewCustomer.BirthDate = NewCustomer.BirthDate.Value;

                //Ticket #9557 Start:Newly created customer not showing issue. By Nikhil.
                if (NewCustomer.TempId == null)
                {
                    string docId = nameof(CustomerDto_POS) + "_" + Guid.NewGuid().ToString();
                    NewCustomer.TempId = docId;
                }
                //Ticket #9557 End:By Nikhil. 

                // CustomerAdded?.Invoke(this, NewCustomer);

                var customer = await CreateOrUpdateCustomer(NewCustomer);

               
                if (customer == null)
                {
                    return false;
                }
                else
                {
                    NewCustomer = customer;
                   // CustomerAdded?.Invoke(this, NewCustomer);
                    Displayinvoice.CustomerId = customer.Id;
                    WeakReferenceMessenger.Default.Send(new Messenger.BackgroundInvoiceUpdatedMessenger(Displayinvoice));
                    return true;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
        }

        public async Task<CustomerDto_POS> CreateOrUpdateCustomer(CustomerDto_POS _NewCustomer)
        {

            try
            {

                using (new Busy(this, true))
                {
                  

                    var customer = await customerService.UpdateRemoteCustomer(Fusillade.Priority.Background, true, _NewCustomer);
                    if (customer != null)
                    {

                        

                        return customer;
                    }


                    return null;
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;

        }

       
        public void HikePeer_MessageReceived(PeerResponse response)
        {

            Debug.WriteLine("HikePeer_MessageReceived..");

            if (response != null)
            {
                if (response.IsMessage)
                {
                    Message = response.Data;
                    if (response != null)
                        ConvertPeerResponse(response);

                }
                else
                {
                    DeviceInfoLabel = response.Data;
                    
                }
            }
        }


        private async void ConvertPeerResponse(PeerResponse response)
        {
            try
            {

                Debug.WriteLine("Received Data : " + response.Data);

                if (response == null || (response != null && response.Data == "Empty user input"))
                {
                    return;
                }
                
                //Displayinvoice
                Displayinvoice = Newtonsoft.Json.JsonConvert.DeserializeObject<InvoiceDto>(response.Data);

                if (Displayinvoice == null)
                    return;

                if (Displayinvoice.InvoiceUpdateFrom == Models.Enum.InvoiceUpdateFrom.POS)
                    return;


                //var tempObject  = Newtonsoft.Json.JsonConvert.DeserializeObject<Object>(response.Data);

                //if (tempObject is InvoiceDto)
                //{
                //    Displayinvoice = (InvoiceDto)tempObject;
                //}
                //else
                //{
                //    if (customerViewModel == null)
                //        customerViewModel = new CustomerViewModel(customerService, saleService);

                //    await customerViewModel.SearchCustomer(tempObject.ToString());
                //}

                await CreateCustomer(Displayinvoice.CustomerDetail);

                // await InvoiceCalculations.CustomerOnSelectAsync(invoice.CustomerDetail, invoice, offers, productService, taxServices);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Conversion issue : " + ex.ToString());

                

                if (customerViewModel == null)
                    customerViewModel = new CustomerViewModel(customerService, saleService);

                customerViewModel.SearchCustomer(response.Data.ToString());



                var tmpCustomerList = new ObservableCollection<CustomerDto_POS>();
                if (CustomerViewModel.AllCustomer != null)// && customers.Any(x => (x.FirstName + " " + x.LastName).ToLower().Contains(keyword.ToLower()) || (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CompanyName) && x.CompanyName.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.Phone) && x.Phone.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CustomerCode) && x.CustomerCode.ToLower().Contains(keyword.ToLower()))))
                {
                    tmpCustomerList = new ObservableCollection<CustomerDto_POS>(CustomerViewModel.AllCustomer.Where(x => (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(response.Data.ToLower()))));
                }
                else
                {
                    CustomerViewModel.AllCustomer = customerService.GetLocalCustomers();
                    if (CustomerViewModel.AllCustomer != null)
                        tmpCustomerList = new ObservableCollection<CustomerDto_POS>(CustomerViewModel.AllCustomer.Where(x => (x.FirstName + " " + x.LastName).ToLower().Contains(response.Data.ToLower()) || (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(response.Data.ToLower())) || (!string.IsNullOrEmpty(x.CompanyName) && x.CompanyName.ToLower().Contains(response.Data.ToLower())) || (!string.IsNullOrEmpty(x.Phone) && x.Phone.ToLower().Contains(response.Data.ToLower())) || (!string.IsNullOrEmpty(x.CustomerCode) && x.CustomerCode.ToLower().Contains(response.Data.ToLower()))));
                }

                CustomerDto_POS cust = tmpCustomerList.FirstOrDefault();

                if (Displayinvoice == null)
                    return;

                if (Displayinvoice.InvoiceUpdateFrom == Models.Enum.InvoiceUpdateFrom.POS)
                    return;


                if (cust != null)
                {
                    Displayinvoice.CustomerId = cust.Id;
                     Displayinvoice.CustomerDetail = cust;
                    Displayinvoice.CustomerName = cust.FirstName + " " + cust.LastName;
                    //Displayinvoice.CustomerName = "";
                    HikePeer_SendMessage(Displayinvoice);


                    WeakReferenceMessenger.Default.Send(new Messenger.BackgroundInvoiceUpdatedMessenger(Displayinvoice));
                }
                //InvoiceDto
                //if (CustomerModel.SelectedCustomer = cust;)

                
            }
           
        }

        public void HikePeer_SendMessage(InvoiceDto invoice)
        {
            try
            {
                InvoiceStatus invoiceStatus = invoice == null ? InvoiceStatus.Pending : invoice.Status;
                //Start #89499 Customer display not working By Pratik
                if (invoice != null)
                {
                    if (Settings.IsQuoteSale)
                        invoice.Status = InvoiceStatus.Quote;
                    else if (Settings.IsBackorderSaleSelected)
                        invoice.Status = InvoiceStatus.BackOrder;


                    invoice.TanentId = Settings.TenantId;
                    invoice.RegisterId = Settings.CurrentRegister == null ? invoice.RegisterId : Settings.CurrentRegister.Id;
                }
                //End #89499
                Displayinvoice = invoice;
                string message = string.Empty;
                if (invoice != null)
                {
                    invoice.InvoiceUpdateFrom = Models.Enum.InvoiceUpdateFrom.POS;
                    message = Newtonsoft.Json.JsonConvert.SerializeObject(invoice).ToString();
                    invoice.Status = invoiceStatus;
                }
                if (string.IsNullOrEmpty(message))
                    message = "Empty user input";

                PeerComunicator = DependencyService.Get<IPeerComunicator>();
            
                PeerComunicator.SendInvoiceMessage(message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        
        public void DisconnectCustomerDisplay()
        {
            try
            {
                var message = "DisconnectCustomerDisplay";
                PeerComunicator = DependencyService.Get<IPeerComunicator>();
                PeerComunicator.SendInvoiceMessage(message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        
        //start #93611
        public void UpdateAdvertise()
        {
            try
            {
                var message = "UpdateAdvertise-" + Settings.CustomerAppPin;
                PeerComunicator = DependencyService.Get<IPeerComunicator>();
                PeerComunicator.SendInvoiceMessage(message);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        //end #93611

        public void StartPeerConnection()
        {

            try
            {
                PeerComunicator = DependencyService.Get<IPeerComunicator>();
                PeerComunicator.StartPeerConnection(Settings.TenantId.ToString(), Settings.CurrentUser.DisplayName, Settings.StoreName);
                PeerComunicator.ReceivePeerData(HikePeer_MessageReceived);
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }
    }
}
