using System;
using System.Windows.Input;
using HikePOS.Services;
using HikePOS.Models;
using HikePOS.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HikePOS.Helpers;
using System.Linq;
using System.Collections.Generic;
using HikePOS.Enums;
using System.Diagnostics;

namespace HikePOS
{
	public class CustomerViewModel : BaseViewModel
	{

		//ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
		CustomerServices customerService;
		SaleServices saleService;

		private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();

		#region Declare select customer popup commands
		public ICommand OpenSearchCustomerCommand { get; }
		public ICommand SearchCustomerAddCommand { get; }
		public ICommand SearchCustomerEditCommand { get; }
		public ICommand RemoveCustomerCommand { get; }
		public ICommand EditCustomerCommand { get; }
		public ICommand DeliveryCustomerCommand { get; }

		#endregion

		#region Customer properties
		public SearchCustomerView SearchCustomerView { get; set; }  //#97186 By PR
		
		ObservableCollection<CustomerDto_POS> _CustomerList { get; set; }
		public ObservableCollection<CustomerDto_POS> CustomerList { get { return _CustomerList; } set { _CustomerList = value; SetPropertyChanged(nameof(CustomerList)); } }

        CustomerDto_POS _selectedCustomer { get; set; }
		public CustomerDto_POS SelectedCustomer { get { return _selectedCustomer; } set { _selectedCustomer = value; ResetCustomer?.Invoke(this, value); SetPropertyChanged(nameof(SelectedCustomer)); } }
         

        public static ObservableCollection<CustomerDto_POS> AllCustomer { get; set; }

		int _IsOpenSearchCustomerPopUp { get; set; } = 0;
		public int IsOpenSearchCustomerPopUp
		{
			get { return _IsOpenSearchCustomerPopUp; }
			set
			{
				_IsOpenSearchCustomerPopUp = value;
				SetPropertyChanged(nameof(IsOpenSearchCustomerPopUp));
				try
				{
					if (!IsFromSalesHistoryPage && _navigationService?.NavigatedPage != null)
					{
						if (_navigationService.NavigatedPage is BaseContentPage<CheckOutViewModel>)
						{
							((BaseContentPage<CheckOutViewModel>)_navigationService.NavigatedPage).ViewModel.IsOpenBackground = value;
						}
						else if (_navigationService.NavigatedPage is BaseContentPage<PaymentViewModel>)
						{
							((BaseContentPage<PaymentViewModel>)_navigationService.NavigatedPage).ViewModel.IsOpenBackground = value;
						}
					}
					else if (_navigationService?.NavigatedPage != null && (_navigationService?.NavigatedPage as BaseContentPage<ParkSaleViewModel>).ViewModel != null
						&& ParkSaleViewModel.detailpage != null && ParkSaleViewModel.detailpage.ViewModel != null)
					{
						ParkSaleViewModel.detailpage.ViewModel.IsOpenBackground = value;
					}
				}
				catch (Exception ex)
				{
					ex.Track();
				}
			}
		}


		bool _isHavingAddCustomerPermission { get; set; }
		public bool IsHavingAddCustomerPermission
		{
			get { return _isHavingAddCustomerPermission; }
			set { _isHavingAddCustomerPermission = value; SetPropertyChanged(nameof(IsHavingAddCustomerPermission)); }
		}

		#endregion

		public event EventHandler<CustomerDto_POS> CustomerChanged;
		public event EventHandler<CustomerDto_POS> ResetCustomer;

		public event EventHandler<CustomerAddressDto> DeliveryAddressChanged;
		public event EventHandler<CustomerAddressDto> DeliveryAddressSelectionClosed;

		AddCustomerPage addcustomerpage;
		CustomerDetailPage customerdetailpage;
		CustomerDeliveryPage customerdeliverypage;
		//HowDidHearAboutPage howDidHearAboutPage;

		bool IsFromSalesHistoryPage = false;
		public CustomerViewModel(CustomerServices _customerService, SaleServices _saleService, bool isFromSalesHistoryPage = false)
		{

            customerService = _customerService;
			saleService = _saleService;
            IsFromSalesHistoryPage = isFromSalesHistoryPage;
			#region Assign select customer popup commands
			OpenSearchCustomerCommand = new Command<CustomerDto_POS>(OpenSearchCustomer);
			SearchCustomerAddCommand = new Command(SearchCustomerAdd);
			SearchCustomerEditCommand = new Command<CustomerDto_POS>(SearchCustomerEdit);
            RemoveCustomerCommand = new Command<CustomerDto_POS>(removeSelectedCustomer);
			EditCustomerCommand = new Command<CustomerDto_POS>(EditCustomer);
			DeliveryCustomerCommand = new Command<CustomerDto_POS>(DeliveryCustomer);
			#endregion

			if (Settings.GrantedPermissionNames != null)
			{
				IsHavingAddCustomerPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.Customers.Customer.Create" || s == "Pages.Tenant.POS.EnterSale"));
			}
		}

		public async Task SelectedCustomerChanged(CustomerDto_POS customer)
		{
			try
			{

				if (customer == null)
				{
                    removeSelectedCustomer(customer);
				}
				else
				{
					//Ticket start:#21749 On account sale in offline mode in iPad and Android.by rupesh
					//offline customer selection was not working.
					if (customer.Id > 0)
                        customer = customerService.GetLocalCustomerById(customer.Id);
					else
						customer = customerService.GetLocalCustomerByTempId(customer.TempId);
					//Ticket end:#21749.by rupesh

					SelectedCustomer = customer;
					CustomerChanged?.Invoke(this, customer);
				}
			}
			catch (Exception ex)
			{
				ex.Track();
			}

			CloseSearchCustomer();
            
		}

        async void removeSelectedCustomer(CustomerDto_POS CurrentCustomer)
		{
			try
			{
                if (CurrentCustomer != null && (CurrentCustomer.Id != 0 || !string.IsNullOrEmpty(CurrentCustomer.TempId)))
                {
                    var decline = await App.Alert.ShowAlert("Remove customer?", LanguageExtension.Localize("RemoveCustomerText"), "Yes", "No");
                    if (!decline)
                        return;
                }

				SelectedCustomer = new CustomerDto_POS();
				CustomerChanged?.Invoke(this, SelectedCustomer);
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

		async void EditCustomer(CustomerDto_POS CurrentCustomer)
		{
			using (new Busy(this, true))
			{
				try
				{
					if (CurrentCustomer != null && (CurrentCustomer.Id !=0 || !string.IsNullOrEmpty(CurrentCustomer.TempId)))
					{
						if (addcustomerpage == null)
						{
                            addcustomerpage = new AddCustomerPage();
                            addcustomerpage.ViewModel.CustomerAdded += Addcustomerpage_CustomerAddedAsync;
						}
						addcustomerpage.ViewModel.IsPriaryInfoActive = true;
						if (Settings.StoreGeneralRule.EnableCustomerMultiOutlet && CurrentCustomer.CustomerOutlets?.Count == 0)
						{
								var customer = await customerService.GetRemoteCustomerDetail(Fusillade.Priority.UserInitiated, CurrentCustomer.Id);
								CurrentCustomer = customer?.result ?? CurrentCustomer;
								if (_navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel> CurrentPage)
						        {
						            CurrentPage.ViewModel.invoicemodel.Invoice.CustomerDetail = CurrentCustomer;
						        }
						} //start #95313 Customer notes can't be accessed from the app by PR
						else  
						{
							CurrentCustomer = customerService.GetLocalCustomerById(CurrentCustomer.Id);
							if (CurrentCustomer.TotalPurchase == null || CurrentCustomer.TotalPurchase == 0)
							{
								var result = await customerService.GetRemoteCustomerDetail(Fusillade.Priority.UserInitiated, CurrentCustomer.Id);
								CurrentCustomer = result?.result ?? CurrentCustomer;
							}
						}
						//End #95313 by PR
						addcustomerpage.ViewModel.NewCustomer = CurrentCustomer;
                        if (_navigationService?.NavigatedPage is BaseContentPage<EnterSaleViewModel> enterSalePage)
                            enterSalePage.ViewModel.IsOpenPopup = true;
                        await _navigationService.GetCurrentPage.Navigation.PushModalAsync(addcustomerpage);
					}
				}
				catch (Exception ex)
				{
					ex.Track();
				}
			}
		}

		//Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
		async void DeliveryCustomer(CustomerDto_POS CurrentCustomer)
		{
			using (new Busy(this, true))
			{
				try
				{
					{
						if (customerdeliverypage == null)
						{
							customerdeliverypage = new CustomerDeliveryPage();
							customerdeliverypage.ViewModel.DeliverAddressAdded += DeliveryAddress_Added;
							//Ticket start:#28369 iPad: delivery address should be added from the sales history.by rupesh
							customerdeliverypage.ViewModel.DeliveryAddressSelectionClosed += DeliveryAddressSelection_Closed;
							//Ticket end:#28369 .by rupesh

						}

						customerdeliverypage.ViewModel.NewCustomer = CurrentCustomer;
                        if (_navigationService?.NavigatedPage is BaseContentPage<EnterSaleViewModel> enterSalePage)
                            enterSalePage.ViewModel.IsOpenPopup = true;
                        await _navigationService.GetCurrentPage.Navigation.PushModalAsync(customerdeliverypage);
					}
				}
				catch (Exception ex)
				{
					ex.Track();
				}
			}
		}
		//Ticket end:#26664 .by rupesh

		async void OpenSearchCustomer(CustomerDto_POS CurrentCustomer)
		{
			using (new Busy(this, true))
			{
				try
				{
					ObservableCollection<InvoiceDto> lstCustomerInvoices = new ObservableCollection<InvoiceDto>();
					if (CurrentCustomer != null && (CurrentCustomer.Id != 0 || !string.IsNullOrEmpty(CurrentCustomer.TempId)))
					{
                        if (customerdetailpage == null)
                        {
                           	customerdetailpage = new CustomerDetailPage();
							customerdetailpage.ViewModel.EditCustomer += (object sender, CustomerDto_POS e) =>
							{
								EditCustomer(e);
							};
                        }
                        customerdetailpage.ViewModel.IsPriaryInfoActive = true;
                        if (_navigationService?.NavigatedPage is BaseContentPage<EnterSaleViewModel> enterSalePage)
                            enterSalePage.ViewModel.IsOpenPopup = true;
                        await _navigationService.GetCurrentPage.Navigation.PushModalAsync(customerdetailpage);

                        if (CurrentCustomer.Id != 0)
						{
							CurrentCustomer = customerService.GetLocalCustomerById(CurrentCustomer.Id);
                            
							//Ticket #1555 and #589 Start.customer details by rupesh
							//Ticket start :#20998 feature request for IPAD (#20096) by rupesh
							if (CurrentCustomer.TotalPurchase == null || CurrentCustomer.TotalPurchase == 0)
                            {
                                var customer = await customerService.GetRemoteCustomerDetail(Fusillade.Priority.UserInitiated, CurrentCustomer.Id);
                                CurrentCustomer = customer.result;
                            }
							//Ticket end :#20998
							//Ticket #1555 and #589 End  by rupesh
                            if (_navigationService.NavigatedPage is BaseContentPage<EnterSaleViewModel> CurrentPage)
						    {
						        CurrentPage.ViewModel.invoicemodel.Invoice.CustomerDetail = CurrentCustomer;
						    }
						}

                        if (CurrentCustomer == null)
						{
							if(_navigationService.GetCurrentPage.Navigation.ModalStack.Count > 0)
								await _navigationService.GetCurrentPage.Navigation.PopModalAsync();
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CustomerNotFound"), Colors.Red, Colors.White);
							return;
						}
						else
						{
                            customerdetailpage.ViewModel.NewCustomer = CurrentCustomer;
                            _= Task.Run(() =>
							{
								MainThread.BeginInvokeOnMainThread(async()=> 
                        		{ 
									lstCustomerInvoices = saleService.GetLocalInvoicesByCustomerId(CurrentCustomer.Id);
									// Changes by Jigar  ticket no 8342
									if (lstCustomerInvoices.Count == 0)
									{
										List<InvoiceStatus> status = new List<InvoiceStatus>() { InvoiceStatus.Pending, InvoiceStatus.Completed, InvoiceStatus.Parked, InvoiceStatus.OnAccount, InvoiceStatus.Refunded, InvoiceStatus.BackOrder, InvoiceStatus.Voided, InvoiceStatus.LayBy, InvoiceStatus.Exchange, InvoiceStatus.Quote };
										string CustomerName = (string.IsNullOrEmpty(CurrentCustomer.FirstName) ? "" : (CurrentCustomer.FirstName.ToUppercaseFirstCharacter() + " ")) + (string.IsNullOrEmpty(CurrentCustomer.LastName) ? "" : CurrentCustomer.LastName.ToUppercaseFirstCharacter());
										lstCustomerInvoices = await saleService.GetRemoteInvoices(Fusillade.Priority.UserInitiated, true, Settings.SelectedOutletId, null, null, status, CustomerName);
									}
									customerdetailpage.ViewModel.customerInvoices = lstCustomerInvoices;
									customerdetailpage.ViewModel.GetCustomerGroups();
									// Changes by Jigar  ticket no 8342
								});
                            });
                        }

					}
					else // (CurrentCustomer == null || CurrentCustomer.FirstName=="Select" || CurrentCustomer.FirstName == "Guest" || CurrentCustomer.FirstName == "Cash")
					{
						CustomerList = new ObservableCollection<CustomerDto_POS>();

                        if (Settings.GrantedPermissionNames != null)
                        {
                            IsHavingAddCustomerPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.Customers.Customer.Create" || s == "Pages.Tenant.POS.EnterSale"));
                        }

						if (IsOpenSearchCustomerPopUp == 1)
						{
							IsOpenSearchCustomerPopUp = 0;
							//CloseBackgroundShadow();
						}
						else
						{
							IsOpenSearchCustomerPopUp = 1;
							//OpenBackgroundShadow();
						}
					}
				}
				catch (Exception ex)
				{
					ex.Track();
				}
			}
		}

		public void CloseSearchCustomer()
		{
			try
			{
				if (IsOpenSearchCustomerPopUp == 1)
				{
					IsOpenSearchCustomerPopUp = 0;
					//CloseBackgroundShadow();
				}
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

		public void SearchCustomer(string keyword)
		{
			try
			{
				CustomerList = new ObservableCollection<CustomerDto_POS>();
				if (string.IsNullOrEmpty(keyword))
					return;
				
				using (new Busy(this, true))
				{
                    //var tmpCustomerList = await customerService.GetLocalCustomerByKeyword(keyword);
                    var tmpCustomerList = new ObservableCollection<CustomerDto_POS>();
                    if (AllCustomer != null)// && customers.Any(x => (x.FirstName + " " + x.LastName).ToLower().Contains(keyword.ToLower()) || (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CompanyName) && x.CompanyName.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.Phone) && x.Phone.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CustomerCode) && x.CustomerCode.ToLower().Contains(keyword.ToLower()))))
                    {
                        tmpCustomerList = new ObservableCollection<CustomerDto_POS>(AllCustomer.Where(x => !string.IsNullOrWhiteSpace(x.FullName) && ((x.FirstName + " " + x.LastName).ToLower().Contains(keyword.ToLower()) || (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CompanyName) && x.CompanyName.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.Phone) && x.Phone.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CustomerCode) && x.CustomerCode.ToLower().Contains(keyword.ToLower())))));
                    }
                    else
                    {
                        AllCustomer = customerService.GetLocalCustomers();
                        if(AllCustomer !=null)
                            tmpCustomerList = new ObservableCollection<CustomerDto_POS>(AllCustomer.Where(x => !string.IsNullOrWhiteSpace(x.FullName) && ((x.FirstName + " " + x.LastName).ToLower().Contains(keyword.ToLower()) || (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CompanyName) && x.CompanyName.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.Phone) && x.Phone.ToLower().Contains(keyword.ToLower())) || (!string.IsNullOrEmpty(x.CustomerCode) && x.CustomerCode.ToLower().Contains(keyword.ToLower())))));
                    }

					if (tmpCustomerList != null && tmpCustomerList.Any())
					{
						if (Settings.GrantedPermissionNames != null)
						{
                            //Start #77581 Edit field should be disable if User is not granted with Permission as per web by Pratik
                            bool isEditPermission = (Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.Customers.Customer.Edit"));
                            //End #77581 by Pratik
                            if (isEditPermission)
							{
								tmpCustomerList.ForEach(c => c.IsAbletoEdit = true);
							}
							else
							{
								tmpCustomerList.ForEach(c => c.IsAbletoEdit = false);
							}
						}

						CustomerList = new ObservableCollection<CustomerDto_POS>(tmpCustomerList.OrderBy(x => x.FullName).ToList());
					}
				}
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

		async void SearchCustomerAdd()
		{
			try
			{
				if (addcustomerpage == null)
				{
					addcustomerpage = new AddCustomerPage();
                    addcustomerpage.ViewModel.CustomerAdded += Addcustomerpage_CustomerAddedAsync;
				}

				addcustomerpage.ViewModel.IsPriaryInfoActive = true;
				addcustomerpage.ViewModel.NewCustomer = new CustomerDto_POS();
                addcustomerpage.ViewModel.SelectedInvoicesDueType = "None";
                if (_navigationService?.NavigatedPage is BaseContentPage<EnterSaleViewModel> enterSalePage)
                    enterSalePage.ViewModel.IsOpenPopup = true;
                await _navigationService.GetCurrentPage.Navigation.PushModalAsync(addcustomerpage);
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

		async void SearchCustomerEdit(CustomerDto_POS customer)
		{
			try
			{
				//start #95313 Customer notes can't be accessed from the app by PR
				using (new Busy(this, true))
				{
					if (addcustomerpage == null)
					{
						addcustomerpage = new AddCustomerPage();
						addcustomerpage.ViewModel.CustomerAdded += Addcustomerpage_CustomerAddedAsync;
					}
					addcustomerpage.ViewModel.IsPriaryInfoActive = true;
					if (customer.Id != 0)
					{
						customer = customerService.GetLocalCustomerById(customer.Id);
						if (customer.TotalPurchase == null || customer.TotalPurchase == 0)
						{
							var result = await customerService.GetRemoteCustomerDetail(Fusillade.Priority.UserInitiated, customer.Id);
							customer = result?.result ?? customer;
						}
					}
					addcustomerpage.ViewModel.NewCustomer = customer;
					//_navigationService.GetCurrentPage.Navigation.PushModalAsync(addcustomerpage);
					if (_navigationService?.NavigatedPage is BaseContentPage<EnterSaleViewModel> enterSalePage)
						enterSalePage.ViewModel.IsOpenPopup = true;

					IsOpenSearchCustomerPopUp = 0;
					SearchCustomerView.Instant.ClearSearchCustomerEntry();
					await _navigationService.GetCurrentPage.Navigation.PushModalAsync(addcustomerpage);
				}
				//End #95313 by PR
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}

        async void Addcustomerpage_CustomerAddedAsync(object sender, Models.CustomerDto_POS e)
        {
			try
			{
				await addcustomerpage.ViewModel.Close();
				SelectedCustomerChanged(e);
				addcustomerpage.ViewModel.NewCustomer = new CustomerDto_POS();
				if (customerdetailpage != null && customerdetailpage.ViewModel != null && e != null)
				{
					customerdetailpage.ViewModel.NewCustomer = e;
				}
                //Start #77336 POS ipad cash register crashing regularly By Pratik
                if (e != null && e.Id == 0)
                // if (e.Id == 0)
                {
                    if (!AllCustomer.Any(x => x.TempId == e.TempId))
                    {
                        AllCustomer.Add(e);
                        SetPropertyChanged(nameof(AllCustomer));
                    }
                }
                else if (e != null && !AllCustomer.Any(x => x.Id == e.Id))
                //else if (!AllCustomer.Any(x => x.Id == e.Id))
                {
                    if (!AllCustomer.Any(x => x.TempId == e.TempId))
                    {
                        AllCustomer.Add(e);
                        SetPropertyChanged(nameof(AllCustomer));
                    }
                }
                //End #77336 By Pratik

            }
            catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
        }

		//Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
		void DeliveryAddress_Added(object sender, CustomerAddressDto e)
        {
			DeliveryAddressChanged.Invoke(this, e);
        }
		//Ticket end:#26664 .by rupesh
		//Ticket start:#28369 iPad: delivery address should be added from the sales history.by rupesh
		void DeliveryAddressSelection_Closed(object sender, CustomerAddressDto e)
		{
			DeliveryAddressSelectionClosed?.Invoke(this,e);
		}
		//Ticket end:#28369 .by rupesh

	}
}

