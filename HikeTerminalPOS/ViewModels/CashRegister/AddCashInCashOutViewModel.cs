using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
	public class AddCashInCashOutViewModel : BaseViewModel
	{
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
		ApiService<IOutletApi> OutletApiService = new ApiService<IOutletApi>();
		OutletServices outletServices;

        #region Properties
        public EventHandler<bool> CashInCashOutAdded;

        public CashInOutReceipt CashInOutReceiptView { get; set; }

        private string _note { get; set; }
        public string Note { get { return _note; } set { _note = value; SetPropertyChanged(nameof(Note)); } }

        string _OpeningAmount { get; set; }
        public string OpeningAmount { get { return _OpeningAmount; } set { _OpeningAmount = value; SetPropertyChanged(nameof(OpeningAmount)); } }

        RegisterclosureDto _Registerclosure { get; set; }
		public RegisterclosureDto Registerclosure { get { return _Registerclosure; } set { _Registerclosure = value; SetPropertyChanged(nameof(Registerclosure)); } }

        #endregion


        public AddCashInCashOutViewModel()
		{
            Title = LanguageExtension.Localize("AddCashInOutPageTitle");
            outletServices = new OutletServices(OutletApiService);
		}

        public override void OnAppearing()
        {
            base.OnAppearing();
            Note = string.Empty;
            OpeningAmount = string.Empty;
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
        }


        #region Command
        public ICommand CloseCommand => new Command(CloseTapped);
        public ICommand RemoveCashCommand => new Command(RemoveCashTapped);
        public ICommand AddCashCommand => new Command(AddCashTapped);
        #endregion

        #region Command Execution
        public async void CloseTapped()
        {
            await ClosePage();
        }

        public async Task ClosePage()
        {
            try
            {
                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                {
                    if (_navigationService.IsFlyoutPage)
                    {
                        var lastpage = _navigationService.NavigatedPage;
                        if (lastpage != null && lastpage is CashRegisterPage baseContentPage)
                        {
                            baseContentPage.ViewModel.IsClosePopup = true;
                        }
                    }
                    await NavigationService.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
                
        }

        async void RemoveCashTapped()
        {
            try
            {

                decimal decimalparseresult;
                if (!string.IsNullOrEmpty(OpeningAmount) && decimal.TryParse(OpeningAmount, out decimalparseresult))
                {
                    //Ticket #948 Start:Add/Remove cash double click issue. By Nikhil
                    //Ticket #948 EndAdd/Remove cash double click issue. By Nikhil
                    var Amount = decimalparseresult.ToPositive();
                    var result = await SaveCashInOut(Amount, Note, Enums.RegisterCashType.CashOut);
                    if (result)
                    {
                        CashInCashOutAdded?.Invoke(this, result);
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                    }
                    //Ticket #948 Start:Add/Remove cash double click issue. By Nikhil
                    //Ticket #948 End:By Nikhil
                }

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        async void AddCashTapped()
        {
            try
            {
                decimal decimalparseresult;
                if (!string.IsNullOrEmpty(OpeningAmount) && decimal.TryParse(OpeningAmount, out decimalparseresult))
                {   //Ticket #948 Start:Add/Remove cash double click issue. By Nikhil
                    //Ticket #948 End: By Nikhil
                    var Amount = decimalparseresult.ToPositive();
                    var result = await SaveCashInOut(Amount, Note, Enums.RegisterCashType.CashIn);
                    if (result)
                    {
                        CashInCashOutAdded?.Invoke(this, result);
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                    }
                    //Ticket #948 Start:Add/Remove cash double click issue. By Nikhil
                    //Ticket #948 End:By Nikhil
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        #endregion

        public async Task<bool> SaveCashInOut(decimal Amount, string Note, RegisterCashType RegisterCashType)
		{
            RegisterCashInOutDto result = null;
			using (new Busy(this, true))
			{
                try
                {
                    RegisterCashInOutDto registerCashInOutDto = new RegisterCashInOutDto();
                    registerCashInOutDto.RegisterClosureId = Registerclosure.Id;
                    if (RegisterCashType == RegisterCashType.CashIn)
                    {
                        registerCashInOutDto.Amount = Amount;
                    }
                    else
                    {
                        registerCashInOutDto.Amount = Amount * -1;
                    }
                    registerCashInOutDto.Note = Note;
                    registerCashInOutDto.PaymentOptionId = (int)PaymentOptionType.Cash;
                    registerCashInOutDto.UserId = Settings.CurrentUser.Id;
                    registerCashInOutDto.RegisterCashType = RegisterCashType;

                    result = await outletServices.AddCashInOutRegister(Fusillade.Priority.UserInitiated, registerCashInOutDto);
                    if (result != null)
                        await PrintCashInOutReceipt(RegisterCashType, registerCashInOutDto.Amount, Note);
                }
                catch(Exception ex)
                {
                    ex.Track();

                }
				
            };

			if (result != null)
			{
                return true;
			}
			return false;

		}

       async Task PrintCashInOutReceipt(Enums.RegisterCashType CashType, decimal Amount, string Note)
        {
            try
            {
                var print = DependencyService.Get<IPrint>();
                if (print != null)
                {
                    //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                    List<Printer> AvailablePrinter = Settings.GetCachePrinters.Where(x => x.PrimaryReceiptPrint == true).ToList();
                    if (AvailablePrinter != null && AvailablePrinter.Count > 0)
                    {
                        var mPOPStarBarcode = DependencyService.Get<IMPOPStarBarcode>();
                        //Ticket starts #70775:The client wants to connect  usb scanner to mc3 print in ipad.by rupesh
                        var mPOPPrinterConfigure = AvailablePrinter != null && AvailablePrinter.Any(x => (!string.IsNullOrEmpty(x.ModelName) && x.ModelName.Contains("POP")) || x.EnableUSBScanner);
                        //var mPOPPrinterConfigure = AvailablePrinter != null && AvailablePrinter.Any();
                        //Ticket ends #70775.by rupesh
                        if (mPOPPrinterConfigure)
                        {
                            mPOPStarBarcode.CloseService();
                        }

                        foreach (Printer objPrinter in AvailablePrinter)
                        {

                            //Ticket start:#62808 iPad:Print Receipt spacing issues. by pratik
                            CashInOutReceiptView.Content.WidthRequest = objPrinter.width;
                            CashInOutReceiptView.WidthRequest = objPrinter.width;
                            CashInOutReceiptView.UpdateCashInOut(CashType.ToString(), Amount, Note);
                            CashInOutReceiptView.ForceLayout();
                            //Ticket end:#62808 by pratik
                            await Task.Delay(50);
                            var ViewHeight = CashInOutReceiptView.Content.Height;
                            print.PrintViews2(CashInOutReceiptView, ViewHeight, true, objPrinter);
                            //Ticket end:#14410.by rupesh

                        }
                        if (mPOPPrinterConfigure)
                        {
                            mPOPStarBarcode.StartService();
                        }

                        //Ticket  end:#18093 by rupesh
                        //Ticket end:#14410.by rupesh

                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("PrinterValidationMessage"));
                    }

                    //print.PrintViews(CashInOutReceiptView, true);
                    //Ticket end: #62808 .by rupesh
                }
			}
            catch (Exception ex)
            {
                ex.Track();
            }
        }
    }
}
