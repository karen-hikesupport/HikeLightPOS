using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
namespace HikePOS.ViewModels
{

    public class OpenCashCalculatorViewModel : BaseViewModel
    {


        ApiService<IOutletApi> OutletApiService = new ApiService<IOutletApi>();
        OutletServices outletServices;

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        SaleServices saleService;

        ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
        CustomerServices customerService;

        #region Properties
        public EventHandler<RegisterclosuresTallyDto> Saved;

        public EventHandler Closed;


        RegisterclosuresTallyDto _RegisterclosuresTally { get; set; }
        public RegisterclosuresTallyDto RegisterclosuresTally { get { return _RegisterclosuresTally; } set { _RegisterclosuresTally = value; SetPropertyChanged(nameof(RegisterclosuresTally)); } }

     
        EditDenominationPage editDenominationPage;

        public bool isUpdated = false;

        #endregion

        #region LifeCycle
        public OpenCashCalculatorViewModel()
        {
            outletServices = new OutletServices(OutletApiService);
            saleService = new SaleServices(saleApiService);
            customerService = new CustomerServices(customerApiService);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            isUpdated = false;
            QuatityTextChanged();
        }
        #endregion

        #region Command
        public ICommand CloseCommand => new Command(CloseTapped);
        public ICommand SaveCommand => new Command(SaveTapped);
        public ICommand EditCommand => new Command(EditTapped);
        public ICommand FocusedCommand => new Command<Entry>(QuantityFocused);
        public ICommand UnfocusedCommand => new Command<Entry>(QuantityUnfocused);
        public ICommand QuatityTextChangedCommand => new Command(QuatityTextChanged);

        #endregion

        #region Command Execution

        public async void CloseTapped()
        {
            try
            {
                if (isUpdated)
                    Saved?.Invoke(this, RegisterclosuresTally);
                else
                    Closed?.Invoke(this,null);

                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                    await NavigationService.PopModalAsync();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        async void SaveTapped()
        {
             try
            {
                if (App.Instance.IsInternetConnected)
                {
                    Saved?.Invoke(this, RegisterclosuresTally);
                    if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                        await NavigationService.PopModalAsync();
                }
                else
                {
                    App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        void EditTapped()
        {
            if (App.Instance.IsInternetConnected)
            {
                EditDenomination();
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
        }

        //Ticket #12945 Denominations Calculator freezing.By Nikhil	 
        void QuantityFocused(Entry textBox)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBox.Text) && textBox.Text == "0")
                {
                    textBox.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        //Ticket #12945 End.By Nikhil

        void QuantityUnfocused(Entry textBox)
        {
            try
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = "0";
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }
        #endregion

        #region Mothods
        public async void EditDenomination()
        {
            if (App.Instance.IsInternetConnected)
            {
                try
                {
                    using (new Busy(this, true))
                    {
                        if (editDenominationPage == null)
                        {
                            editDenominationPage = new EditDenominationPage();
                            editDenominationPage.ViewModel.Closed += (sender, e) =>
                            {
                                if (e)
                                {
                                    LoadDenomination();
                                    QuatityTextChanged();
                                }

                            };
                        }


                        await NavigationService.PushModalAsync(editDenominationPage);
                    }
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
        }

        public void LoadDenomination(bool fromOpenReg =false)
        {
            try
            {
                    
                using (new Busy(this, true))
                {
                    if(RegisterclosuresTally != null && !fromOpenReg)
                        RegisterclosuresTally.RegisterClosureTallyDenominations =  new ObservableCollection<RegisterClosureTallyDenominationDto>();
                    //Ticket start:#30955 iOS - Denomination Entering When Opening Registers.by rupesh
                    if (Settings.CurrentRegister != null && Settings.CurrentRegister.Registerclosure != null&& Settings.CurrentRegister.Registerclosure.StartDateTime != null && Settings.CurrentRegister.Registerclosure.EndDateTime == null)
                        RegisterclosuresTally.RegisterClosureTallyDenominations = new ObservableCollection<RegisterClosureTallyDenominationDto>(Settings.CurrentRegister.Registerclosure.RegisterclosuresTallys.FirstOrDefault(x => x.Id == RegisterclosuresTally.Id).RegisterClosureTallyDenominations.OrderBy(x => x.DenominationValue).ToList());
                    else
                    {
                        var denominations = outletServices.GetLocalDenomination();
                        var registerClosureTallyDenominations = new ObservableCollection<RegisterClosureTallyDenominationDto>();
                        foreach (var denomination in denominations)
                        {
                            var existingDenomination = RegisterclosuresTally.RegisterClosureTallyDenominations.FirstOrDefault(x => x.DenominationId == denomination.Id);
                            var registerClosureDenomination = new RegisterClosureTallyDenominationDto
                            {
                                DenominationId = denomination.Id,
                                DenominationValue = denomination.Value,
                                IsActive = denomination.IsEditable,
                                Quantity = existingDenomination != null? existingDenomination.Quantity : 0

                            };
                            registerClosureTallyDenominations.Add(registerClosureDenomination);
                        }
                        RegisterclosuresTally.RegisterClosureTallyDenominations = registerClosureTallyDenominations;
                    }
                    //Ticket end:#30955 .by rupesh
                    isUpdated = true;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        void QuatityTextChanged()
        {
            if (RegisterclosuresTally != null && RegisterclosuresTally.RegisterClosureTallyDenominations != null)
                RegisterclosuresTally.CashCalculatorTotal = RegisterclosuresTally.RegisterClosureTallyDenominations.Sum(x => x.Total);
            else
                RegisterclosuresTally.CashCalculatorTotal = 0;
        }

        #endregion
    }
}
