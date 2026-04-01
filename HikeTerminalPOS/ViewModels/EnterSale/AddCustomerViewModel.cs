
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Customer;
using HikePOS.Resources;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class AddCustomerViewModel : BaseViewModel
    {

        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;

        ApiService<ICommonLookupAPI> lookupAPIService = new ApiService<ICommonLookupAPI>();
        CommonLookupServices commonLookupServices;
        CustomerStoreCreditPage customerStoreCreditPage;


        #region Properties
        AddCustomFeildPage addCustomFeildPage;
        public event EventHandler<CustomerDto_POS> CustomerAdded;
        public event EventHandler<CustomerDto_POS> EditCustomer;

        ObservableCollection<CustomField> _CustomFeildList { get; set; }
        public ObservableCollection<CustomField> CustomFeildList
        {
            get
            {
                return _CustomFeildList;
            }
            set
            {
                _CustomFeildList = value;
                SetPropertyChanged(nameof(CustomFeildList));
            }
        }


        Color _customFielTextColor { get; set; }

        //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik
        GeneralRuleDto _shopGeneralRule { get; set; }
        public GeneralRuleDto GeneralShopDto { get { return _shopGeneralRule; } set { _shopGeneralRule = value; SetPropertyChanged(nameof(GeneralShopDto)); } }

        #region register properties
        RegisterDto _currentRegister { get; set; }
        public RegisterDto CurrentRegister { get { return _currentRegister; } set { _currentRegister = value; SetPropertyChanged(nameof(CurrentRegister)); } }
        #endregion

        SubscriptionDto _subscription { get; set; }
        public SubscriptionDto Subscription { get { return _subscription; } set { _subscription = value; SetPropertyChanged(nameof(Subscription)); } }

        //End Ticket #74631 by pratik

        public Color CustomFielButtonTextColor { get { return _customFielTextColor; } set { _customFielTextColor = value; SetPropertyChanged(nameof(CustomFielButtonTextColor)); } }

        //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
        bool _isSecondaryEmailVisible { get; set; }
        public bool IsSecondaryEmailVisible { get { return _isSecondaryEmailVisible; } set { _isSecondaryEmailVisible = value; SetPropertyChanged(nameof(IsSecondaryEmailVisible)); } }
        //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.

        bool _isCustomFieldAlllowed { get; set; }
        public bool IsCustomFieldAlllowed { get { return _isCustomFieldAlllowed; } set { _isCustomFieldAlllowed = value; SetPropertyChanged(nameof(IsCustomFieldAlllowed)); } }

        bool _customFeild1Visible { get; set; }
        public bool CustomFeild1Visible { get { return _customFeild1Visible; } set { _customFeild1Visible = value; SetPropertyChanged(nameof(CustomFeild1Visible)); } }
        bool _customFeild2Visible { get; set; }
        public bool CustomFeild2Visible { get { return _customFeild2Visible; } set { _customFeild2Visible = value; SetPropertyChanged(nameof(CustomFeild2Visible)); } }
        bool _customFeild3Visible { get; set; }
        public bool CustomFeild3Visible { get { return _customFeild3Visible; } set { _customFeild3Visible = value; SetPropertyChanged(nameof(CustomFeild3Visible)); } }
        bool _customFeild4Visible { get; set; }
        public bool CustomFeild4Visible { get { return _customFeild4Visible; } set { _customFeild4Visible = value; SetPropertyChanged(nameof(CustomFeild4Visible)); } }



        string _customFeild1Text { get; set; }
        public string CustomFeild1Text { get { return _customFeild1Text; } set { _customFeild1Text = value; SetPropertyChanged(nameof(CustomFeild1Text)); } }
        string _customFeild2Text { get; set; }
        public string CustomFeild2Text { get { return _customFeild2Text; } set { _customFeild2Text = value; SetPropertyChanged(nameof(CustomFeild2Text)); } }
        string _customFeild3Text { get; set; }
        public string CustomFeild3Text { get { return _customFeild3Text; } set { _customFeild3Text = value; SetPropertyChanged(nameof(CustomFeild3Text)); } }
        string _customFeild4Text { get; set; }
        public string CustomFeild4Text { get { return _customFeild4Text; } set { _customFeild4Text = value; SetPropertyChanged(nameof(CustomFeild4Text)); } }



        #region Create Customer properties
        CustomerDto_POS _NewCustomer { get; set; }
        public CustomerDto_POS NewCustomer
        {
            get { return _NewCustomer; }
            set
            {
                _NewCustomer = value;
                if (_NewCustomer.Address == null) _NewCustomer.Address = new AddressDto();
                SetPropertyChanged(nameof(NewCustomer));
            }
        }

        ObservableCollection<InvoiceDto> _customerAllInvoices { get; set; }
        public ObservableCollection<InvoiceDto> customerAllInvoices { get { return _customerAllInvoices; } set { _customerAllInvoices = value; SetPropertyChanged(nameof(customerAllInvoices)); } }

        ObservableCollection<InvoiceDto> _customerInvoices { get; set; }
        public ObservableCollection<InvoiceDto> customerInvoices { get { return _customerInvoices; } set { _customerInvoices = value; SetPropertyChanged(nameof(customerInvoices)); } }

        public HearAboutDto _selectedHearAboutUs { get; set; }
        public HearAboutDto SelectedHearAboutUs { get { return _selectedHearAboutUs; } set { _selectedHearAboutUs = value; SetPropertyChanged(nameof(SelectedHearAboutUs)); } }
        ObservableCollection<HearAboutDto> _HearAboutUsList { get; set; }
        public ObservableCollection<HearAboutDto> HearAboutUsList { get { return _HearAboutUsList; } set { _HearAboutUsList = value; SetPropertyChanged(nameof(HearAboutUsList)); } }

        public CustomerGroupDto _selectedCustomerGroup { get; set; }
        public CustomerGroupDto SelectedCustomerGroup { get { return _selectedCustomerGroup; } set { _selectedCustomerGroup = value; SetPropertyChanged(nameof(SelectedCustomerGroup)); } }

        String _selectedCustomerGroupName { get; set; }
        public String SelectedCustomerGroupName { get { return _selectedCustomerGroupName; } set { _selectedCustomerGroupName = value; SetPropertyChanged(nameof(SelectedCustomerGroupName)); } }

        //Ticket #9611 Start: Permission for user "assign a discount group to a customer profile" not working in iPad issue. By Nikhil.
        bool _canSelectGroup { get; set; }
        public bool CanSelectGroup
        {
            get { return _canSelectGroup; }
            set
            {
                _canSelectGroup = value;
                SetPropertyChanged(nameof(CanSelectGroup));
            }
        }
        //Ticket #9611 End:By Nikhil.

        ObservableCollection<CustomerGroupDto> _CustomerGroupList { get; set; }
        public ObservableCollection<CustomerGroupDto> CustomerGroupList { get { return _CustomerGroupList; } set { _CustomerGroupList = value; SetPropertyChanged(nameof(CustomerGroupList)); } }

        //Ticket #9042 Start : Customer country field added. By Nikhil.
        ObservableCollection<CountriesDto> _CountryList { get; set; }
        public ObservableCollection<CountriesDto> CountryList { get { return _CountryList; } set { _CountryList = value; SetPropertyChanged(nameof(CountryList)); } }

        CountriesDto _SelectedCountry { get; set; }
        public CountriesDto SelectedCountry { get { return _SelectedCountry; } set { _SelectedCountry = value; SetPropertyChanged(nameof(SelectedCountry)); } }
        //Ticket #9042 End : By Nikhil.

        bool _IsPriaryInfoActive { get; set; } = true;
        public bool IsPriaryInfoActive { get { return _IsPriaryInfoActive; } set { _IsPriaryInfoActive = value; SetPropertyChanged(nameof(IsPriaryInfoActive)); } }

        int _IsOpenFilterOptionPopUp { get; set; } = 0;
        public int IsOpenFilterOptionPopUp { get { return _IsOpenFilterOptionPopUp; } set { _IsOpenFilterOptionPopUp = value; SetPropertyChanged(nameof(IsOpenFilterOptionPopUp)); } }

        string _SelectedMenu { get; set; } = "All";
        public string SelectedMenu { get { return _SelectedMenu; } set { _SelectedMenu = value; SetPropertyChanged(nameof(SelectedMenu)); } }

        public bool _HasStoreCreditPermission { get; set; }
        public bool HasStoreCreditPermission { get { return _HasStoreCreditPermission; } set { _HasStoreCreditPermission = value; SetPropertyChanged(nameof(HasStoreCreditPermission)); } }

        #endregion

        //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil 
        public ICommand ChangeTaxExemptCommand { get; }
        //Ticket #10921 End. By Nikhil

        ObservableCollection<OutletDto_POS> _Outlets { get; set; }
        public ObservableCollection<OutletDto_POS> Outlets { get { return _Outlets; } set { _Outlets = value; SetPropertyChanged(nameof(Outlets)); } }
        public bool _EnableCustomerMultiOutlet { get; set; }
        public bool EnableCustomerMultiOutlet { get { return _EnableCustomerMultiOutlet; } set { _EnableCustomerMultiOutlet = value; SetPropertyChanged(nameof(EnableCustomerMultiOutlet)); } }

        //Ticket start:#23573 iOS - Gender Options.by rupesh
        public string _SelectedGender { get; set; }
        public string SelectedGender { get { return _SelectedGender; } set { _SelectedGender = value; SetPropertyChanged(nameof(SelectedGender)); } }
        ObservableCollection<string> _GenderList { get; set; }
        public ObservableCollection<string> GenderList { get { return _GenderList; } set { _GenderList = value; SetPropertyChanged(nameof(GenderList)); } }
        //Ticket end:#23573 .by rupesh
        //Start #45386 iPad: Please ignore birthday "year" when add new customer .by rupesh        
        public bool IsDoNotAskForTheYearInTheBirthDateOfTheCustomers { get { return Settings.StoreGeneralRule.DoNotAskForTheYearInTheBirthDateOfTheCustomers; } }
        //end #45386  .by rupesh

        private bool _loyaltySectionVisible;
        public bool LoyaltySectionVisible { get { return _loyaltySectionVisible; } set { _loyaltySectionVisible = value; SetPropertyChanged(nameof(LoyaltySectionVisible)); } }

        private string _birthDateFormat = "yyyy-MM-dd";
        public string BirthDateFormat { get { return _birthDateFormat; } set { _birthDateFormat = value; SetPropertyChanged(nameof(BirthDateFormat)); } }

        public string _strCreditLimit { get; set; }
        public string StrCreditLimit
        {
            get
            {
                return _strCreditLimit;
            }
            set
            {
                _strCreditLimit = value;
                SetPropertyChanged(nameof(StrCreditLimit));
            }
        }

        private bool _editCustomerVisible;
        public bool EditCustomerVisible { get { return _editCustomerVisible; } set { _editCustomerVisible = value; SetPropertyChanged(nameof(EditCustomerVisible)); } }

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public string _invoicesDueDays;
        public string InvoicesDueDays
        {
            get { return _invoicesDueDays; }
            set
            {
                _invoicesDueDays = value;
                SetPropertyChanged(nameof(InvoicesDueDays));
                if (!string.IsNullOrEmpty(value) && SelectedInvoicesDueType != null)
                {
                    var regg = new Regex(@"^(0?[1-9]|[12][0-9]|3[01])$");
                    string msg = "Enter invoice due days between 1 to 31";
                    switch (SelectedInvoicesDueType)
                    {
                        case "Day(s) after the invoice date":
                            regg = new Regex(@"^([1-9]|[1-9][0-9]|[1-9][0-9][0-9])$");
                            msg = "Enter invoice due days between 1 to 999";
                            break;
                        case "Day(s) after the end of the invoice month":
                            regg = new Regex(@"^([1-9]|[1-9][0-9])$");
                            msg = "Enter invoice due days between 1 to 99";
                            break;
                        case "None":
                            msg = "Please select one of the payment validation options";
                            break;
                    }
                    if (!regg.IsMatch(value) || SelectedInvoicesDueType == "None")
                    {
                        InvoicesDueDays = InvoicesDueDays.Remove(InvoicesDueDays.Length - 1);
                        App.Instance.Hud.DisplayToast(msg, Colors.Red, Colors.White, TimeSpan.FromMilliseconds(3000));
                    }
                }
            }
        }

        public string _selectedInvoicesDueType = "None";
        public string SelectedInvoicesDueType
        {
            get { return _selectedInvoicesDueType; }
            set
            {
                _selectedInvoicesDueType = value;
                SetPropertyChanged(nameof(SelectedInvoicesDueType));
                if (!string.IsNullOrWhiteSpace(InvoicesDueDays))
                {
                    switch (value)
                    {
                        case "Day(s) after the invoice date":
                            int vval1 = 0;
                            if (int.TryParse(InvoicesDueDays, out vval1) && vval1 > 999)
                                InvoicesDueDays = "";
                            break;
                        case "Day(s) after the end of the invoice month":
                            int vval = 0;
                            if (int.TryParse(InvoicesDueDays, out vval) && vval > 99)
                                InvoicesDueDays = "";
                            break;
                        case "None":
                            InvoicesDueDays = "";
                            break;
                    }
                }
            }
        }
        public ObservableCollection<string> InvoicesDueTypeList
        {
            get
            {
                return new ObservableCollection<string>() { "None", "Of The Following Month", "Day(s) after the invoice date", "Day(s) after the end of the invoice month", "Of the current month" };
            }
        }
        //End ticket #76208 by Pratik
        ObservableCollection<CustomerOutletDto> CustomerOutlets { get; set; }

        #endregion

        #region Constructor/OnAppearing
        public AddCustomerViewModel()
        {
            Title = "Add Customer";
            NewCustomer = new CustomerDto_POS();
            IsSecondaryEmailVisible = true;
            customerService = new CustomerServices(customerApiService);
            commonLookupServices = new CommonLookupServices(lookupAPIService);
            //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil
            ChangeTaxExemptCommand = new Command<String>(CustomerTaxExemptChange);
            //Ticket #10921 End. By Nikhil
            CustomFielButtonTextColor = AppColors.NavigationBarBackgroundColor;

            //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik
            GeneralShopDto = Settings.StoreGeneralRule;
            //End Ticket #74631 by pratik

        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            IsCustomFieldAlllowed = true;
            //Start #77581 Edit field should be disable if User is not granted with Permission as per web by Pratik
            EditCustomerVisible = Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.Customers.Customer.Edit");
            //End #77581 by Pratik
            //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik
            Subscription = Settings.Subscription;
            //End Ticket #74631 by pratik

            //Ticket start:#20998 feature request for IPAD (#20096).by rupesh
            EnableCustomerMultiOutlet = Settings.StoreGeneralRule.EnableCustomerMultiOutlet;
            if (EnableCustomerMultiOutlet)
            {
                var realm = RealmService.GetRealm();
                var list = realm.All<OutletDB_POS>().ToList().Select(a=>OutletDto_POS.FromModel(a)).ToList();// await CommonQueries.GetAllLocals<OutletDto_POS>();
                if (list != null)
                {
                    var outlets = new ObservableCollection<OutletDto_POS>(list.Where(x => Settings.CurrentUser.Outlets.Select(s => s.OutletId).Contains(x.Id)).OrderBy(x => x.Title));
                    outlets.Insert(0, new OutletDto_POS { Title = "All outlets" });
                    if(NewCustomer.CustomerOutlets != null)
                    {
                        var outletsToUpdate = outlets.Where(x => NewCustomer.CustomerOutlets.Any(y => y.OutletId == x.Id)).ToList();
                        outletsToUpdate.ForEach(c => c.IsSelected = true);
                        if(outletsToUpdate.Count + 1 == outlets.Count)
                        {
                            outlets.FirstOrDefault().IsSelected = true;
                        }
                    }
                    else
                    {
                        NewCustomer.CustomerOutlets = new ObservableCollection<CustomerOutletDto>();
                        var outletToUpdate = outlets.FirstOrDefault(x => x.Id == Settings.SelectedOutletId);
                        outletToUpdate.IsSelected = true;
                        var customerOutletDto = new CustomerOutletDto { OutletId = outletToUpdate.Id };
                        NewCustomer.CustomerOutlets.Add(customerOutletDto);
                    }
                    Outlets = outlets;

                }
                CustomerOutlets = new ObservableCollection<CustomerOutletDto>(NewCustomer.CustomerOutlets);
            }
            //Ticket end:#20998 feature request for IPAD (#20096).by rupesh


            //Ticket #9611 Start: Permission for user "assign a discount group to a customer profile" not working in iPad issue. By Nikhil.
            CanSelectGroup = Settings.GrantedPermissionNames.Contains("Pages.Tenant.Customers.Customer.CanableToSelectGroup");
            //Ticket #9611 End:By Nikhil. 

            HasStoreCreditPermission = Settings.GrantedPermissionNames.Contains("Pages.Tenant.POS.EnterSale.ToGiveStoreCredit");
            UpdateCustomFields();


        }
        #endregion

        #region Command

        public ICommand SaveCommand => new Command(SaveTapped);
        public ICommand YesLoyaltyRewordCommand => new Command(YesSelectRoyalty);
        public ICommand NoLoyaltyRewordCommand => new Command(NoSelectRoyalty);
        public ICommand YesMarketingCommand => new Command(YesSelectMarketing);
        public ICommand NoMarketingCommand => new Command(NoSelectMarketing);
        public ICommand PrimaryInfoCommand => new Command<string>(PrimaryInfo);
        public ICommand MoreDetailsCommand => new Command(MoreDetails);
        public ICommand CustomeFieldCommand => new Command(CustomeFieldTapped);
        public ICommand CreditLimitCommand => new Command(CreditLimitTapped);
        public ICommand OutletSelectedCommand => new Command<OutletDto_POS>(OutletSelected);

        public ICommand EditCommand => new Command(EditTapped);
        public ICommand StoreCreditCommand => new Command(StoreCreditTapped);
        public ICommand FilterOptionCommand => new Command(FilterOptionTapped);
        #endregion

        #region Command Execution

        public void EditTapped()
        {
            EditCustomer?.Invoke(this, NewCustomer);
        }

        public void FilterOptionTapped()
        {
            if (IsOpenFilterOptionPopUp == 0)
            {
                IsOpenFilterOptionPopUp = 1;
            }
            else
            {
                IsOpenFilterOptionPopUp = 0;
            }
        }

        public async void StoreCreditTapped()
        {
            //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.

            try
            {

                System.Reflection.PropertyInfo myPropInfo;
                bool result = false;

                myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeCreditNotFeature");

                bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out result);

                if (!result)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreFeatureNotAvailable"), Colors.Red, Colors.White);

                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.

            if (customerStoreCreditPage == null)
            {
                customerStoreCreditPage = new CustomerStoreCreditPage();
            }
            customerStoreCreditPage.ViewModel.CustomerId = NewCustomer.Id;
            //Start #77145 iPAD: Store credit visibility by Pratik
            customerStoreCreditPage.ViewModel.CreditAdded += ViewModel_CreditAdded;
            //End #77145 by Pratik
            await NavigationService.PushModalAsync(customerStoreCreditPage);
        }

        //Start #77145 iPAD: Store credit visibility by Pratik
        private void ViewModel_CreditAdded(object sender, CreditBalanceHistoryDto e)
        {
            customerStoreCreditPage.ViewModel.CreditAdded -= ViewModel_CreditAdded;
            if (NewCustomer.CreditBalance == null)
                NewCustomer.CreditBalance = e.CreditAmount;
            else
                NewCustomer.CreditBalance = NewCustomer.CreditBalance.Value + e.CreditAmount;
        }
        //End #77145 by Pratik

        public async void SaveTapped()
        {
            try
            {
                CreditLimitTapped();
                var result = await CreateCustomer();
                if (result)
                {
                    
                    ClosePopupTapped();
                    CustomerAdded?.Invoke(this, NewCustomer);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void YesSelectRoyalty()
        {
            NewCustomer.AllowLoyalty = true;
        }

        public void NoSelectRoyalty()
        {
            NewCustomer.AllowLoyalty = false;
        }

        public void YesSelectMarketing()
        {
            NewCustomer.ToAllowForMarketing = true;
        }

        public void NoSelectMarketing()
        {
            NewCustomer.ToAllowForMarketing = false;
        }

        public void PrimaryInfo(string dto)
        {
            IsPriaryInfoActive = true;
            if(dto == "custDetail")
                IsOpenFilterOptionPopUp = 0;
        }

        public void MoreDetails()
        {
            IsPriaryInfoActive = false;
        }

        public void CustomeFieldTapped()
        {
            //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.
            //if (sender != null)
            //{
            //    Button button = sender as Button;
            //    if (!Extensions.IsFeatureAccessible(button.TextColor))    //HikeCustomFieldsFeature
            //        return;
            //}

            try
            {

                System.Reflection.PropertyInfo myPropInfo;
                bool StorePermissionResult = false;

                myPropInfo = Settings.ShopFeatures.GetType().GetProperty("HikeCustomFieldsFeature");


                bool tempResult = Boolean.TryParse((myPropInfo.GetValue(Settings.ShopFeatures).ToString()), out StorePermissionResult);

                if (!StorePermissionResult)
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("StoreFeatureNotAvailable"), Colors.Red, Colors.White);
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            //#34963 iPad: Feature Request: on account and store credit option shouldn't be shown in an Essential plan.


            if (App.Instance.IsInternetConnected)
            {
                AddCustomFeild();
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
            //throw new NotImplementedException();

        }

        public void CreditLimitTapped()
        {
            var val = NewCustomer.StrCreditLimit;
            decimal result = 0;
            if (decimal.TryParse(val, out result))
            {
                NewCustomer.CreditLimit = result;
            }
            else if(string.IsNullOrEmpty(val))
            {
                NewCustomer.CreditLimit = 0;
            }
            else
            {
                NewCustomer.CreditLimit = NewCustomer.CreditLimit;

            }
        }

        void OutletSelected(OutletDto_POS outlet)
        {
            outlet.IsSelected = !outlet.IsSelected;
            if (CustomerOutlets == null)
            {
                CustomerOutlets = new ObservableCollection<CustomerOutletDto>();
            }

            if (outlet.Id == 0)
            {
                CustomerOutlets.Clear();
                foreach (var l in Outlets)
                {
                    if (l.Id == 0)
                        continue;

                    l.IsSelected = outlet.IsSelected;
                    if (l.IsSelected)
                    {
                        var customerOutletDto = new CustomerOutletDto { OutletId = l.Id };
                        CustomerOutlets.Add(customerOutletDto);
                    }
                    else
                    {
                        var removeOutlet = CustomerOutlets.Where(x => x.OutletId == l.Id).FirstOrDefault();
                        CustomerOutlets.Remove(removeOutlet);

                    }

                }

            }
            else
            {
                Outlets.FirstOrDefault().IsSelected = false;
                if (outlet.IsSelected)
                {
                    var customerOutletDto = new CustomerOutletDto { OutletId = outlet.Id };
                    CustomerOutlets.Add(customerOutletDto);
                }
                else
                {
                    var removeOutlet = CustomerOutlets.Where(x => x.OutletId == outlet.Id).FirstOrDefault();
                    CustomerOutlets.Remove(removeOutlet);
                }
            }
        }
        #endregion

        #region Methods

        public void LoadData()
        {
            IsSecondaryEmailVisible = true;

            GetCustomerGroups();
            GetCountries();
            GetAllHearAboutus();
            //Ticket start:#23573 iOS - Gender Options.by rupesh
            GetAllGenderList();
            //Ticket end:#23573 .by rupesh
            if (Settings.StoreGeneralRule != null)
            {
                LoyaltySectionVisible = Settings.StoreGeneralRule.EnableLoyalty;
            }
            else
            {
                LoyaltySectionVisible = false;
            }

            if (NewCustomer != null)
            {
                if (NewCustomer.Id < 1)
                {
                    NewCustomer.AllowLoyalty = LoyaltySectionVisible;
                    NewCustomer.BirthDate = null;
                }
            }
            else
            {
                //Ticket #9042 Start : Customer country field added. By Nikhil.
                SelectedCountry = null;
                //Ticket #9042 End :By Nikhil.
                SelectedCustomerGroup = null;
                SelectedHearAboutUs = null;
                NewCustomer.BirthDate = null;
            }
            //Start #45386 iPad: Please ignore birthday "year" when add new customer         
            if (Settings.StoreGeneralRule.DoNotAskForTheYearInTheBirthDateOfTheCustomers)
            {
                BirthDateFormat = "MM-dd";
            }
            else
            {
                BirthDateFormat = "yyyy-MM-dd";
            }
            //#45386 iPad: End by nutan
        }

        public void UpdateCustomFields()
        {
            CustomFeildList = customerService.GetAllLocalCustomerCustomFields();

            if (CustomFeildList.Count > 0)
            {
                CustomFeild1Visible = true;
                CustomFeild1Text = CustomFeildList[0].FieldName;
            }
            if (CustomFeildList.Count > 1)
            {
                CustomFeild2Visible = true;
                CustomFeild2Text = CustomFeildList[1].FieldName;
            }
            if (CustomFeildList.Count > 2)
            {
                CustomFeild3Visible = true;
                CustomFeild3Text = CustomFeildList[2].FieldName;
            }
            if (CustomFeildList.Count > 3)
            {
                CustomFeild4Visible = true;
                CustomFeild4Text = CustomFeildList[3].FieldName;
            }
        }

        public async Task<bool> CreateCustomer(CustomerDto_POS DisplayCustomer = null, bool isFromDisplayApp = false)
        {

            if (!isFromDisplayApp)
            {
                if (string.IsNullOrEmpty(NewCustomer.FirstName) || String.IsNullOrWhiteSpace(NewCustomer.FirstName))
                {
                    //await Application.Current.MainPage.DisplayAlert("Warning!!", "Please enter First name!", "Ok");
                    NewCustomer.FirstName = String.Empty;
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("FirstNameRequiredValidationMessage"));
                    return false;
                }

                //Ticket start:#50721 Customer email created with a space.by rupesh
                if (!string.IsNullOrEmpty(NewCustomer.Email) && !Regex.IsMatch(NewCustomer.Email, RegxValues.EmailRegx, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_InValidMessage"));
                    return false;
                }
                if (!string.IsNullOrEmpty(NewCustomer.SecondaryEmail1) && !Regex.IsMatch(NewCustomer.SecondaryEmail1, RegxValues.EmailRegx, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                {
                    App.Instance.Hud.DisplayToast("Please enter a valid secondary email 1");
                    return false;
                }
                if (!string.IsNullOrEmpty(NewCustomer.SecondaryEmail2) && !Regex.IsMatch(NewCustomer.SecondaryEmail2, RegxValues.EmailRegx, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                {
                    App.Instance.Hud.DisplayToast("Please enter a valid secondary email 2");
                    return false;
                }
                //Ticket end:#50721.by rupesh

                //START ticket #76208 IOS:FR:Terms of payments by Pratik
                if (Settings.Subscription.Edition.PlanType != null && (Settings.Subscription.Edition.PlanType == PlanType.Plus || Settings.Subscription.Edition.PlanType == PlanType.Trial))
                {
                    if (!string.IsNullOrEmpty(SelectedInvoicesDueType) && SelectedInvoicesDueType != "None" && string.IsNullOrWhiteSpace(InvoicesDueDays))
                    {
                        App.Instance.Hud.DisplayToast(DueDateErrorMsg(), Colors.Red, Colors.White, TimeSpan.FromMilliseconds(3000));
                        return false;
                    }
                    NewCustomer.InvoicesDueDays = null;
                    NewCustomer.InvoicesDueType = null;
                    if (!string.IsNullOrEmpty(SelectedInvoicesDueType) && SelectedInvoicesDueType != "None" && !string.IsNullOrEmpty(InvoicesDueDays))
                    {
                        NewCustomer.InvoicesDueDays = Convert.ToInt32(InvoicesDueDays);
                        NewCustomer.InvoicesDueType = GetInvoicesDueType();
                    }
                }
                //End ticket #76208 by Pratik

                if (Settings.StoreGeneralRule.EnableCustomerMultiOutlet && CustomerOutlets.Count == 0)
                {
                    App.Instance.Hud.DisplayToast("Please select atleast one Outlet");
                    return false;
                }
                //Ticket start:#55401 iPad: Duplicate Customer issue.by rupesh
                if (SelectedCustomerGroup != null && CustomerGroupList != null && CustomerGroupList.Count > 0)
                {
                    NewCustomer.CustomerGroupId = SelectedCustomerGroup.Id;
                   //Ticket #589 CustomerGroupName added by rupesh
                    NewCustomer.CustomerGroupName = SelectedCustomerGroup.Name;
                }
                //Ticket end:#55401 .by rupesh
                if (SelectedHearAboutUs != null)
                {
                    NewCustomer.HowDidYouHearAboutus = SelectedHearAboutUs.Id;
                }

                //Ticket start:#23573 iOS - Gender Options.by rupesh
                if (SelectedGender != null)
                    NewCustomer.Gender = GenderList.IndexOf(SelectedGender);
                else
                    NewCustomer.Gender = -1;
                //Ticket end:#23573 .by rupesh

                //Ticket #9042 Start : Customer country field added. By Nikhil.
                if (SelectedCountry != null && NewCustomer.Address != null)
                {
                    NewCustomer.Address.Country = SelectedCountry.value;
                }
                //Ticket #9042 End :By Nikhil.

                if (!string.IsNullOrEmpty(NewCustomer.Email))
                {
                    bool existcustomer = customerService.CheckLocalCustomerIsExistByEmail(NewCustomer.Email, NewCustomer.Id);
                    if (existcustomer)
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CustomerEmailAlreadyExist"));
                        return false;
                    }
                }
            }
            else
            {
                NewCustomer = DisplayCustomer;
               isFromDisplayApp = false;
            }

            //if(NewCustomer.PickerDate != null)
            //{
            //    NewCustomer.BirthDate = NewCustomer.PickerDate;
            //}

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

             NewCustomer.CustomerOutlets = CustomerOutlets;
            var customer = await CreateCustomer(NewCustomer);
            if (customer == null)
            {
                return false;
            }
            else
            {
                NewCustomer = customer;
                return true;
            }


        }

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        InvoicesDueType GetInvoicesDueType()
        {
            switch (SelectedInvoicesDueType)
            {
                case "Of The Following Month":
                    return InvoicesDueType.OfTheFollowingMonth;
                case "Day(s) after the invoice date":
                    return InvoicesDueType.DaysAfterTheInvoiceDate;
                case "Day(s) after the end of the invoice month":
                    return InvoicesDueType.DaysAfterTheEndOfTheInvoiceMonth;
                default:
                    return InvoicesDueType.OfTheCurrentMonth;
            }
        }

        public string GetInvoicesDueTypeStr(InvoicesDueType invoicesDueType)
        {
            switch (invoicesDueType)
            {
                case InvoicesDueType.OfTheFollowingMonth:
                    return "Of The Following Month";
                case InvoicesDueType.DaysAfterTheInvoiceDate:
                    return "Day(s) after the invoice date";
                case InvoicesDueType.DaysAfterTheEndOfTheInvoiceMonth:
                    return "Day(s) after the end of the invoice month";
                case InvoicesDueType.OfTheCurrentMonth:
                    return "Of the current month";
                default:
                    return "None";
            }
        }

        string DueDateErrorMsg()
        {
            string msg = "Enter invoice due days between 1 to 31";
            switch (SelectedInvoicesDueType)
            {
                case "Day(s) after the invoice date":
                    msg = "Enter invoice due days between 1 to 999";
                    break;
                case "Day(s) after the end of the invoice month":
                    msg = "Enter invoice due days between 1 to 99";
                    break;
            }
            return msg;
        }
        //End ticket #76208 by Pratik

        public async Task<CustomerDto_POS> CreateCustomer(CustomerDto_POS _NewCustomer)
        {
            using (new Busy(this, true))
            {
                var customer = await customerService.UpdateRemoteCustomer(Fusillade.Priority.UserInitiated, true, _NewCustomer);
                if (customer != null)
                {
                    return customer;
                }

                return null;
            };

        }

        //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil 
        public void CustomerTaxExemptChange(string parameter)
        {
            bool allowTaxExempt = !string.IsNullOrEmpty(parameter) && parameter.Equals("Yes");
            NewCustomer.ToAllowForTaxExempt = allowTaxExempt;
        }
        //Ticket #10921 End. By Nikhil

        #region Customer group methods
        public bool GetCustomerGroups()
        {

            var CustomerGroup = new ObservableCollection<CustomerGroupDto>();
            using (new Busy(this, true))
            {
                try
                {
                    CustomerGroup = customerService.GetLocalCustomerGroups();
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
                if (CustomerGroup == null)
                {
                    CustomerGroup = new ObservableCollection<CustomerGroupDto>();
                }
            };
            CustomerGroupList = CustomerGroup;

            //Ticket #589 Change in query added by rupesh
            if (CustomerGroupList.Count > 0)
                SelectedCustomerGroup = CustomerGroupList.FirstOrDefault(x => NewCustomer != null && NewCustomer.CustomerGroupId != null && x.Id == NewCustomer.CustomerGroupId);
            //#42404 start:iPad: Feature request - customer group - default.by rupesh
             if (CustomerGroupList.Count > 0 && NewCustomer != null && NewCustomer.CustomerGroupId == null)
                SelectedCustomerGroup = CustomerGroupList.FirstOrDefault(x => x.Name == "Retail (default)");
            //#42404 end:.by rupesh
            if (SelectedCustomerGroup != null)
                SelectedCustomerGroupName = SelectedCustomerGroup.Name;
            else SelectedCustomerGroupName = "-";

            return true;
        }
        #endregion

        //Ticket #9042 Start : Customer country field added. By Nikhil.
        public bool GetCountries()
        {
            try
            {
                if (Settings.AllCountries != null)
                {
                    CountryList = new ObservableCollection<CountriesDto>(Settings.AllCountries.OrderBy(a=>a.displayText)); //#96190

                    var objCountry = CountryList.FirstOrDefault(x => NewCustomer != null && NewCustomer.Address != null && NewCustomer.Address.Country != null && x.value == NewCustomer.Address.Country);
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

            return true;
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
        //Ticket #9042 End : By Nikhil.

        public bool GetAllHearAboutus()
        {
            try
            {
                if (Settings.AllHearAbout != null)
                {
                    HearAboutUsList = Settings.AllHearAbout;

                    var objHearAboutUs = Settings.AllHearAbout.FirstOrDefault(x => NewCustomer != null && x.Id == NewCustomer.HowDidYouHearAboutus);
                    if (objHearAboutUs != null)
                        SelectedHearAboutUs = HearAboutUsList.First(x => x.Id == objHearAboutUs.Id);
                    else
                        SelectedHearAboutUs = null;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

            return true;
        }

        //Ticket start:#23573 iOS - Gender Options.by rupesh
        public bool GetAllGenderList()
        {
            try
            {
                if (GenderList == null)
                {
                    GenderList = new ObservableCollection<string> { "Male", "Female", "Prefer not to say" };
                }
                SelectedGender = (NewCustomer.Gender >= 0 && NewCustomer.Gender < GenderList.Count) ? GenderList[NewCustomer.Gender] : null;

            }
            catch (Exception ex)
            {
                ex.Track();
            }

            return true;
        }
        //Ticket end:#23573 .by rupesh

        public void SelectFilterMenu(string selectedMenu)
        {
            try
            {
                if (SelectedMenu != selectedMenu)
                {
                    SelectedMenu = selectedMenu;
                    //ParkSales?.Clear();
                    using (new Busy(this, true))
                    {
                        if (customerAllInvoices != null && customerAllInvoices.Any())
                        {
                            if (selectedMenu != "All")
                            {
                                //customerInvoices = new ObservableCollection<InvoiceDto>(customerAllInvoices.Where(x => x.Status.ToString() == selectedMenu));
                                customerInvoices = customerAllInvoices;
                            }
                            else
                            {
                                customerInvoices = customerAllInvoices;
                            }
                        }
                    }
                }
                IsOpenFilterOptionPopUp = 0;
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public async void AddCustomFeild()
        {
            if (App.Instance.IsInternetConnected)
            {
                try
                {
                    using (new Busy(this, true))
                    {
                        if (addCustomFeildPage == null)
                        {
                            addCustomFeildPage = new AddCustomFeildPage();
                            addCustomFeildPage.ViewModel.CustomFeildList = await customerService.GetRemoteCustomerCustomFields(Fusillade.Priority.UserInitiated, true);
                            addCustomFeildPage.ViewModel.Saved += async (sender, e) =>
                            {
                                if (e != null)
                                {
                                    addCustomFeildPage.ViewModel.CustomFeildList = await customerService.UpdateRemoteCustomFields(Fusillade.Priority.UserInitiated, true, e);
                                    CustomFeildList = e;
                                    UpdateCustomFields();
                                }
                            };
                        }


                        await NavigationService.PushModalAsync(addCustomFeildPage);
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
        }

    

        #endregion
    }
}
