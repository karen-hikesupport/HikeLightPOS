using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Services;
using HikePOS.Models;
using System.Security.AccessControl;

namespace HikePOS.ViewModels
{
    public class CustomSaleViewModel : BaseViewModel
    {
      
        ApiService<ITaxApi> taxApiService = new ApiService<ITaxApi>();
        TaxServices taxServices;
        public event EventHandler<InvoiceLineItemDto> CustomsaleAdded;

        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        ApiService<IUserApi> userApiService = new ApiService<IUserApi>();
        UserServices userService;

        ObservableCollection<UserListDto> _Users { get; set; }
        public ObservableCollection<UserListDto> Users { get { return _Users; } set { _Users = value; SetPropertyChanged(nameof(Users)); } }

        UserListDto _selectedUser;
        public UserListDto SelectedUser { get { return _selectedUser; } set { _selectedUser = value; SetPropertyChanged(nameof(SelectedUser)); } }

        bool _isServedBy;
        public bool IsServedBy { get { return _isServedBy; } set { _isServedBy = value; SetPropertyChanged(nameof(IsServedBy)); } }
        //end #84287 .by Pratik

        InvoiceLineItemDto _CustomSale { get; set; } = new InvoiceLineItemDto();
        public InvoiceLineItemDto CustomSale { get { return _CustomSale; } set { _CustomSale = value; SetPropertyChanged(nameof(CustomSale)); } }

        ObservableCollection<TaxDto> _taxList { get; set; }
        public ObservableCollection<TaxDto> TaxList { get { return _taxList; } set { _taxList = value; SetPropertyChanged(nameof(TaxList)); } }

        TaxDto _selectedTax { get; set; }
        public TaxDto SelectedTax
        {
            get
            {
                return _selectedTax;
            }
            set
            {
                _selectedTax = value;
                SetPropertyChanged(nameof(SelectedTax));
                PickerTaxListSelectedChanged();
            }
        }

        //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil
        InvoiceDto _invoice { get; set; }
        public InvoiceDto Invoice { get { return _invoice; } set { _invoice = value; } }

        string _quantity { get; set; }
        public string Quantity { get { return _quantity; } set { _quantity = value; SetPropertyChanged(nameof(Quantity)); } }

        bool _canChangeTax { get; set; } = true;
        public bool CanChangeTax { get { return _canChangeTax; } set { _canChangeTax = value; SetPropertyChanged(nameof(CanChangeTax)); } }
        //Ticket #10921 End. By Nikhil

        public CustomSaleViewModel()
        {
            Title = "Custom sale page";
            taxServices = new TaxServices(taxApiService);
            userService = new UserServices(userApiService);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            if (CustomSale.Quantity == 0)
                CustomSale.Quantity = 1;
            if (string.IsNullOrEmpty(CustomSale.Title))
                CustomSale.Title = LanguageExtension.Localize("CustomSale_TitleText");

            //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
            if (Settings.StoreGeneralRule.ServedByLineItem)
            {
                LoadUserData();
            }
            else
            {
                IsServedBy = false;
            }
            //end #84287 .by Pratik

            //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil	 
            TaxList = taxServices.GetLocalTaxes();
            if (TaxList != null)
            {
                if (Invoice != null && Invoice.CustomerDetail != null && Invoice.CustomerDetail.ToAllowForTaxExempt)
                {
                    SelectedTax = TaxList.FirstOrDefault(x => x.Id == 1);//1 is fixed id for NoTax
                }
                else
                {
                    var Register = Settings.CurrentRegister;
                    if (Register != null && Register.DefaultTax != null)
                    {
                        SelectedTax = TaxList.FirstOrDefault(x => x.Id == Register.DefaultTax);
                        if (SelectedTax == null)
                            SelectedTax = TaxList.FirstOrDefault();
                    }
                    else
                    {
                        SelectedTax = TaxList.FirstOrDefault();
                    }
                }
            }

            Quantity = CustomSale.Quantity.ToString();
            //Ticket #10921 End. By Nikhil  
        }

        #region Command
        public ICommand SaveCommand => new Command(SaveTapped);
        public ICommand DecreaseHandleCommand => new Command(DecreaseTapped);
        public ICommand IncreaseHandleCommand => new Command(IncreaseTapped);
        public ICommand QuantityUnfocusedCommand => new Command(QuantityUnfocused);

        #endregion

        #region Command Execution

        public void SaveTapped()
        {
            try
            {

                if (string.IsNullOrEmpty(CustomSale.Title))
                {
                    //Application.Current.MainPage.DisplayAlert("Authentication error", "Please enter custom sale name", "Ok");
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CustomSaleNameValidationMessage"));

                    return;
                }

                // Ticket 6994 : Added below validation because we were facing issue in print receipt.

                var charsInTitle = CustomSale.Title.Length;

                if (charsInTitle > 110)
                {
                    App.Instance.Hud.DisplayToast("The name should not be more than 110 characters long.");

                    return;
                }

                if (string.IsNullOrEmpty(CustomSale.StrRetailPrice))
                {
                    //Start Ticket #66674 iOS: Discount option not available in custom sale edit & custom sale cost price always take 0 (without price) by Pratik
                    if (!string.IsNullOrEmpty(CustomSale.StrItemCost))
                       CustomSale.RetailPrice = Convert.ToDecimal(CustomSale.StrItemCost);
                    else
                        CustomSale.RetailPrice = 0;
                    //end Ticket #66674 by Pratik
                }
                else
                    CustomSale.RetailPrice = Convert.ToDecimal(CustomSale.StrRetailPrice);

                //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil 
                CustomSale.CustomSaleRetailPrice = CustomSale.RetailPrice;
                //Ticket #10921 End. By Nikhil

                if (string.IsNullOrEmpty(CustomSale.StrItemCost))
                    CustomSale.ItemCost = 0;
                else
                    CustomSale.ItemCost = Convert.ToDecimal(CustomSale.StrItemCost);

                //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
                if (IsServedBy)
                {
                    CustomSale.ServedByName = SelectedUser.FullName;
                    CustomSale.CreatorUserId = SelectedUser.Id;
                }
                else
                {
                    CustomSale.ServedByName = Settings.CurrentUser.FullName;
                    CustomSale.CreatorUserId = Settings.CurrentUser.Id;
                }
                //end #84287 .by Pratik

                CustomsaleAdded?.Invoke(this, CustomSale);

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void DecreaseTapped()
        {
            if (CustomSale.Quantity != 1)
            {
                CustomSale.Quantity--;
            }
            else
            {
                CustomSale.Quantity = -1;
            }
            Quantity = CustomSale.Quantity.ToString();
        }

        void IncreaseTapped()
        {
            if (CustomSale.Quantity != -1)
            {
                CustomSale.Quantity++;
            }
            else
            {
                CustomSale.Quantity = 1;
            }
            Quantity = CustomSale.Quantity.ToString();
        }

        void QuantityUnfocused()
        {
            try
            {
                decimal decimalparseresult;
                if (decimal.TryParse(Quantity, out decimalparseresult))
                {
                    CustomSale.Quantity = decimalparseresult;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        #endregion

        void PickerTaxListSelectedChanged() 
        {

            CustomSale.LineItemTaxes = new ObservableCollection<LineItemTaxDto>();
            if (SelectedTax != null && SelectedTax.SubTaxes != null)
            {
                CustomSale.TaxId = SelectedTax.Id;
                CustomSale.TaxName = SelectedTax.Name;
                CustomSale.TaxRate = SelectedTax.Rate;

                foreach (var item in SelectedTax.SubTaxes)
                {
                    var subtax = new LineItemTaxDto
                    {
                        TaxId = item.Id,
                        TaxRate = item.Rate,
                        TaxName = item.Name
                    };

                    CustomSale.LineItemTaxes.Add(subtax);
                }
            }


        }

        //start #84287 IOS- Feature:-Allow an option to add 'Sold by' user name on line items in the cart By Pratik
        public async void LoadUserData()
        {
            try
            {
                IsServedBy = true;
                Users = new ObservableCollection<UserListDto>();
                var res = userService.GetLocalUsers();
                if (res != null)
                {
                    var result = res.Where(x => x.Outlets != null && x.Roles != null && x.Roles.Any() && x.Outlets.Any(s => s.OutletId == Settings.SelectedOutletId)).Select(x => { x.Roles = new ObservableCollection<UserListRoleDto>() { x.Roles.FirstOrDefault() }; return x; });
                    if (result != null)
                        Users = new ObservableCollection<UserListDto>(result);

                   
                }
                else
                {
                    res = await userService.GetRemoteUsers(Fusillade.Priority.UserInitiated, false);
                    if (res != null)
                    {
                        var result = res.Where(x => x.Outlets != null && x.Roles != null && x.Roles.Any() && x.Outlets.Any(s => s.OutletId == Settings.SelectedOutletId)).Select(x => { x.Roles = new ObservableCollection<UserListRoleDto>() { x.Roles.FirstOrDefault() }; return x; });
                        if (result != null)
                            Users = new ObservableCollection<UserListDto>(result);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            finally
            {
                if (Users != null && Users.Count <= 0)
                {
                    IsServedBy = false;
                }
                else
                {
                    if(Users.Any(a=>a.Id == Settings.CurrentUser.Id))
                        SelectedUser = Users.First(a => a.Id == Settings.CurrentUser.Id);
                }
            }
        }
        //end #84287 .by Pratik

    }
}