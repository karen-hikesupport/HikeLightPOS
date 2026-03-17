using System;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using System.Linq;
using System.Collections.ObjectModel;
using SPIClient;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HikePOS.Enums;
using HikePOS.Services.Payment;
using HikePOS.Models.Payment;
using Newtonsoft.Json;
using Fusillade;

namespace HikePOS.ViewModels
{
    public class CloseRegisterTallyViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService = ServiceLocator.Get<INavigationService>();
        public EventHandler<bool> Closed;

        ApiService<IOutletApi> OutletApiService = new ApiService<IOutletApi>();
		OutletServices outletServices;

		ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
		SaleServices saleService;

		ApiService<ICustomerApi> customerApiService = new ApiService<ICustomerApi>();
		CustomerServices customerService;

        ApiService<IEconduitPaymentApi> econduitApiService = new ApiService<IEconduitPaymentApi>();
        public EConduitPaymentService econduitPaymentService;

        ApiService<IClearantPaymentService> clearantApiService = new ApiService<IClearantPaymentService>();
        ClearantPaymentService clearantPaymentService;

        ApiService<IPaymentApi> paymentApiService = new ApiService<IPaymentApi>();
        PaymentServices paymentService;

        private string _note { get; set; }
        public string Note { get { return _note; } set { _note = value; SetPropertyChanged(nameof(Note)); } }

        RegisterclosureDto _Registerclosure { get; set; }
		public RegisterclosureDto Registerclosure { get { return _Registerclosure; } set { _Registerclosure = value; SetPropertyChanged(nameof(Registerclosure)); } }
        OpenCashCalculatorPage openCashCalculatorPage;
        

        public ICommand OpenCashCalculatorCommand { get; }

        #region LifeCycle
        public CloseRegisterTallyViewModel()
        {
            outletServices = new OutletServices(OutletApiService);
			saleService = new SaleServices(saleApiService);
			customerService = new CustomerServices(customerApiService);
            econduitPaymentService = new EConduitPaymentService(econduitApiService);
            clearantPaymentService = new ClearantPaymentService(clearantApiService);
            paymentService = new PaymentServices(paymentApiService);

            OpenCashCalculatorCommand = new Command<RegisterclosuresTallyDto>(OpenCashCalculator);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            if (IsClosePopup == true)
            {
                IsClosePopup = false;
                return;
            }
            if (SPICommonViewModel._spi == null)
            {
                StartAssemblyPayment();
            }else{
                SPICommonViewModel._spi.Config.PromptForCustomerCopyOnEftpos = true;// ConfigurationModel.IsAllowPrintOnEFTPOS;
                SPICommonViewModel._spi.Config.SignatureFlowOnEftpos = true;// ConfigurationModel.IsAllowPrintOnEFTPOS;

                SPICommonViewModel._spi.StatusChanged -= OnStatusChanged1;// Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged -= OnSecretsChanged1; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged -= OnPairingFlowStateChanged1; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged -= OnTxFlowStateChanged1; // Called throughout to transaction process to update us with progress
            


                SPICommonViewModel._spi.StatusChanged += OnStatusChanged; // Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged += OnSecretsChanged; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged += OnPairingFlowStateChanged; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged += OnTxFlowStateChanged; // Called throughout to transaction process to update us with progress
            }
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsOpenPopup == true)
            {
                IsOpenPopup = false;
                return;
            }
            if (SPICommonViewModel._spi != null)
            {
                SPICommonViewModel._spi.StatusChanged -= OnStatusChanged; // Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged -= OnSecretsChanged; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged -= OnPairingFlowStateChanged; // Called throughout to pairing process to update us with progress

                //Ticket start:#17842,#18328 iOS - Cash register settlement report not printing on Presto terminal by rupesh
                // SPICommonViewModel._spi.TxFlowStateChanged -= OnTxFlowStateChanged; // Called throughout to transaction process to update us with progress

                SPICommonViewModel._spi.StatusChanged += OnStatusChanged1;// Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged += OnSecretsChanged1; // Called when Secrets are set or changed or voided. 
                SPICommonViewModel._spi.PairingFlowStateChanged += OnPairingFlowStateChanged1; // Called throughout to pairing process to update us with progress
                                                                                               //   SPICommonViewModel._spi.TxFlowStateChanged += OnTxFlowStateChanged1; // Called throughout to transaction process to update us with progress
                                                                                               //Ticket end:#17842,#18328  by rupesh
            }

        }

        #endregion


        #region Command
        public ICommand CloseCommand => new Command(CloseTapped);
        public ICommand CloseRegisterCommand => new Command(CloseRegisterTapped);
        public ICommand RegisteredTotalCommand => new Command<int>(RegisteredTotalTapped);

        #endregion

        #region Command Execution

        private void RegisteredTotalTapped(int PaymentOptionId)
        {
             UpadateRegisterclosuresTally(PaymentOptionId);
        }

        public void CloseTapped()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
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
                });
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        async void CloseRegisterTapped()
        {
            if (!string.IsNullOrWhiteSpace(Note))
            {
                Registerclosure.Notes = Note;
            }

            var result = await CloseRegister();
            if (result)
            {
                Closed?.Invoke(this, result);
                CloseTapped();
            }
        }

        #endregion

        public async Task<bool> CloseRegister(){
            bool result = false;
            using (new Busy(this, true))
			{
                try
                {
                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    {
						var tmpInvoices = saleService.GetOfflineInvoices();
						var tmpCustomer = customerService.GetUnSyncCustomer();

						if ((tmpInvoices != null && tmpInvoices.Count > 0) || (tmpCustomer != null && tmpCustomer.Count > 0))
						{
							var decline = await App.Alert.ShowAlert("Close Register", LanguageExtension.Localize("DataSyncContinueMessage"), "Yes", "No");
							if (decline)
							{
								OutletSync objOutletSync = new OutletSync();
								await objOutletSync.PushAllUnsyncDataOnRemote(inBackgroundMode: false, RequiredAllData: false, ResetAfterUppdate: false);
							}
						}

                        RegisterDto tempRegisterDto = Settings.CurrentRegister;
                        tempRegisterDto.Registerclosure = Registerclosure;
                        tempRegisterDto.LastCloseDateTime = DateTime.Now.ToUniversalTime();

                        RegisterDto registerDto =  await outletServices.CloseRegister(Fusillade.Priority.UserInitiated, tempRegisterDto);
                        if (registerDto != null)
                        {
                            Settings.CurrentRegister = registerDto;

                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CloseRegisterSuccessMessage"), Colors.Green, Colors.White);

                            if (tempRegisterDto.Registerclosure.RegisterclosuresTallys.Any(x => x.paymentOptionType == Enums.PaymentOptionType.AssemblyPayment))
                            {
                                var yes = await App.Alert.ShowAlert("Confimation", "Do you want to settle Simple Payments Integration?", "Yes", "Skip for now");
                                if (yes)
                                {
                                    if (SPICommonViewModel._spi != null && SPICommonViewModel._spi.CurrentStatus == SPIClient.SpiStatus.PairedConnected)
                                    {
                                        result = false;
                                        var data = SPICommonViewModel._spi.InitiateSettleTx("HIKEPOS" + Settings.CurrentRegister.Id);
                                    }
                                    else
                                    {
                                        result = true;
                                        App.Instance.Hud.DisplayToast("Sorry! We are not able to settlement in Simple Payments Integration Terminal. Please check connection.", Colors.Red, Colors.White);
                                    }
                                }
                                else
                                    result = true;
                            }
                            //Clearnt batch request start.
                            if (tempRegisterDto.Registerclosure.RegisterclosuresTallys.Any(x => x.paymentOptionType == Enums.PaymentOptionType.Clearent))
                            {
                                int regID = 0;
                                var register = Settings.CurrentRegister;
                                if (register != null)
                                {
                                    regID = register.Id;
                                }


                                var tmpAll_PaymentOptionList = paymentService.GetLocalPaymentOptionsByRegister(regID);
                                PaymentOptionDto payment = new PaymentOptionDto();
                                var clearantPayments = tmpAll_PaymentOptionList.Where(x => x.PaymentOptionType == PaymentOptionType.Clearent);


                                foreach (var item in clearantPayments)
                                {
                                    if (item.IsConfigered)
                                    {
                                        payment = item;
                                    }
                                }
                                var clearantConfigurationData = JsonConvert.DeserializeObject<ClearantConfiguration>(payment.ConfigurationDetails);
                                clearantConfigurationData.ToCloseRegister = true;
                                if (clearantConfigurationData.ToCloseRegister)
                                {
                                    var yes = await App.Alert.ShowAlert("Confimation", "Do you want to settle Clearnat Payment?", "Yes", "Skip for now");
                                    if (yes)
                                    {

                                        if (clearantConfigurationData != null)
                                        {


                                            ClearantBatchRequest clearantBatchRequest = new ClearantBatchRequest()
                                            {
                                                lastBatchClosedRequiredInResponse = true,
                                                SoftwareType = "hikepos"

                                            };

                                            ClearantBatchResponse clearantBatchResponse = await clearantPaymentService.SendClearantBatchRequest(Priority.UserInitiated, clearantConfigurationData.ApiKey, clearantBatchRequest, Settings.AccessToken);
                                            if (clearantBatchResponse != null)
                                            {

                                                if (clearantBatchResponse.payload != null && clearantBatchResponse.payload.batch != null)
                                                {
                                                    var data = ClearantBatchReceiptData(clearantBatchResponse);
                                                    //update settlement reciept to server
                                                    //Ticket  start:#18093 iOS - About Batch Settlement for Clearent.by rupesh
                                                    var recieptDataRequest = new RecieptDataRequest
                                                    {
                                                        id = Registerclosure.Id,
                                                        merchant_receipt = data
                                                    };

                                                    var response = await outletServices.UpdateRegisterClosureMerchantSettleReciept(Priority.UserInitiated, recieptDataRequest);
                                                    //Ticket  end:#18093 .by rupesh
                                                }

                                            }
                                            else
                                            {
                                                App.Instance.Hud.DisplayToast("Something went wrong!");
                                            }



                                        }
                                        else
                                        {
                                            App.Instance.Hud.DisplayToast("Sorry! We are not able to settlement in clearant Terminal. Please check connection.", Colors.Red, Colors.White);
                                        }

                                    }
                                }
                                result = true;
                            }
                            //Clearnt batch request end.


                            if (tempRegisterDto.Registerclosure.RegisterclosuresTallys.Any(x => x.paymentOptionType == PaymentOptionType.NorthAmericanBankcard
                            || x.paymentOptionType == PaymentOptionType.PayJunction
                            || x.paymentOptionType == PaymentOptionType.VerifonePaymark
                            || x.paymentOptionType == PaymentOptionType.eConduit
                            || x.paymentOptionType == PaymentOptionType.EVOPayment))
                            {
                                    
                                    int regID = 0;
                                    var register = Settings.CurrentRegister;
                                    if (register != null)
                                    {
                                        regID = register.Id;
                                    }

                                    //private ObservableCollection<PaymentOptionDto> tmpAll_PaymentOptionList;

                                    var tmpAll_PaymentOptionList = paymentService.GetLocalPaymentOptionsByRegister(regID);

                                  

                                    PaymentOptionDto payment = new PaymentOptionDto();
                                  

                                    var cEconduitPayments = tmpAll_PaymentOptionList.Where(x => x.PaymentOptionType == PaymentOptionType.VerifonePaymark
                                     || x.PaymentOptionType == PaymentOptionType.PayJunction
                                     || x.PaymentOptionType == PaymentOptionType.EVOPayment
                                     || x.PaymentOptionType == PaymentOptionType.eConduit
                                     || x.PaymentOptionType == PaymentOptionType.NorthAmericanBankcard);


                                    foreach (var item in cEconduitPayments)
                                    {
                                        if (item.IsConfigered)
                                         {
                                            payment = item;
                                         }
                                    }

                                    if (payment.PaymentOptionType != PaymentOptionType.VerifonePaymark)
                                    {
                                        var yes = await App.Alert.ShowAlert("Confirmation", "Do you want to settle eConduit Payment?", "Yes", "Skip for now");
                                        if (yes)
                                        {


                                            var econduitConfigurationData = JsonConvert.DeserializeObject<EconduitCofigurationDto>(payment.ConfigurationDetails);



                                            if (econduitConfigurationData != null)
                                            {

                                               string tempEconduitID = Guid.NewGuid().ToString();


                                              EconduitRequestObject econduitRequestObject = new EconduitRequestObject()
                                              {
                                                paymentOption = payment,
                                                refID = tempEconduitID,

                                              };

                                            EconduitResponse econduitResponse = await econduitPaymentService.CloseconduitBatchRequest(Priority.UserInitiated, econduitRequestObject, Settings.AccessToken);

                                            EconduitCloseRootObject econduitContent = JsonConvert.DeserializeObject<EconduitCloseRootObject>(econduitResponse.content);
                                            if (econduitContent.ResultCodeCredit == "Approved")
                                            {

                                            }
                                            else
                                            {

                                                if (!string.IsNullOrEmpty(econduitContent.MessageCredit))
                                                {
                                                    App.Instance.Hud.DisplayToast(econduitContent.MessageCredit);

                                                }
                                                else if (!string.IsNullOrEmpty(econduitContent.MessageGift))
                                                {
                                                    App.Instance.Hud.DisplayToast(econduitContent.MessageGift);

                                                }
                                                else
                                                {
                                                    App.Instance.Hud.DisplayToast("Something went wrong on econduit!");
                                                }

                                            }
                                        }
                                        else
                                        {
                                            App.Instance.Hud.DisplayToast("Sorry! We are not able to settlement in eConduit Terminal. Please check connection.", Colors.Red, Colors.White);
                                        }
                                    }
                                    }
                                    result = true;
                                        
                                   
                                
                                
                            }
                            else
                            {
                                result = true;

                            }
                       

                        }
                        else
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"), Colors.Red, Colors.White);
                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
                catch(Exception ex)
                {
                    ex.Track();
                }
            };
            return result;
        }
    
        async void OpenCashCalculator(RegisterclosuresTallyDto registerclosuresTally)
        {
            if (App.Instance.IsInternetConnected)
            {
                try
                {
                    if (openCashCalculatorPage == null)
                    {
                        openCashCalculatorPage = new OpenCashCalculatorPage();
                        openCashCalculatorPage.ViewModel.Saved += async (sender, e) =>
                        {
                            IsClosePopup = true;
                            if (e != null && Registerclosure != null && Registerclosure.RegisterclosuresTallys != null)
                            {
                                registerclosuresTally = (RegisterclosuresTallyDto)e;
                                registerclosuresTally.RegisteredTotal = registerclosuresTally.CashCalculatorTotal;
                                //Ticket start:#29444 10 Million dollar Sales register error.by rupesh
                                registerclosuresTally.StrRegisteredTotal = registerclosuresTally.CashCalculatorTotal.ToString("C");
                                //Ticket end:#29444 .by rupesh
                            }

                            Registerclosure.RegisterclosuresTallys.Where(x => x.Id == registerclosuresTally.Id).All(x =>
                              {

                                  x = registerclosuresTally;
                                  return true;
                              });
                            using (new Busy(this, true))
                            {
                                await outletServices.CreateOrUpdateRegisterClosureDenomination(Fusillade.Priority.UserInitiated, registerclosuresTally.RegisterClosureTallyDenominations);
                            }
                        };
                    }
                    //openCashCalculatorPage.ViewModel.RegisterclosuresTally = registerclosuresTally;
                    using (new Busy(this, true))
                    {
                        var lstRegisterClosureTallyDenomination = new ObservableCollection<RegisterClosureTallyDenominationDto>();
                        if (registerclosuresTally.RegisterClosureTallyDenominations != null)
                            lstRegisterClosureTallyDenomination = new ObservableCollection<RegisterClosureTallyDenominationDto>((registerclosuresTally.RegisterClosureTallyDenominations.OrderBy(x => x.DenominationValue).ToList()));

                        registerclosuresTally.RegisterClosureTallyDenominations = lstRegisterClosureTallyDenomination;
                        openCashCalculatorPage.ViewModel.RegisterclosuresTally = registerclosuresTally;
                    }
                    IsOpenPopup = true;
                    await NavigationService.PushModalAsync(openCashCalculatorPage);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
            else
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
            }
        }
    
        public void UpadateRegisterclosuresTally(int id)
        {
            try
            {

                RegisterclosuresTallyDto registerclosuresTally = Registerclosure?.RegisterclosuresTallys?.FirstOrDefault(x => x.PaymentOptionId == id);
                if (registerclosuresTally != null)
                {
                    if(!string.IsNullOrEmpty(registerclosuresTally.StrRegisteredTotal))
                        //Ticket start:#29444 10 Million dollar Sales register error.by rupesh
                        registerclosuresTally.RegisteredTotal = decimal.Parse(registerclosuresTally.StrRegisteredTotal,System.Globalization.NumberStyles.Currency);
                    //Ticket end:#29444 .by rupesh
                    else
                        registerclosuresTally.RegisteredTotal = 0;
                    
                    Registerclosure.RegisterclosuresTallys.Where(x => x.Id == registerclosuresTally.Id).All(x =>
                    {
                        x = registerclosuresTally;
                        return true;
                    });
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }

        private void StartAssemblyPayment()
        {

            // This is where you load your state - like the pos_id, eftpos address and secrets - from your file system or database
            #region Spi Setup
            try
            {

                if (!string.IsNullOrEmpty(Settings.APEncKey) && !string.IsNullOrEmpty(Settings.APHmacKey))
                {
                    SPICommonViewModel._spiSecrets = new Secrets(Settings.APEncKey, Settings.APHmacKey);
                }



                // This is how you instantiate Spi.
                SPICommonViewModel._spi = new Spi("HIKEPOS" + Settings.CurrentRegister.Id, Settings.SerialNumber, Settings.EFTPOSAddress, SPICommonViewModel._spiSecrets); // It is ok to not have the secrets yet to start with.
                SPICommonViewModel._spi.SetPosInfo("HIKEPOS" + Settings.CurrentRegister.Id, "1.0");
                SPICommonViewModel._spi.SetDeviceApiKey(ServiceConfiguration.DeviceApiKey);
                SPICommonViewModel._spi.SetAutoAddressResolution(true);
                var result = SPICommonViewModel._spi.SetTenantCode(Settings.AcquirerCode);
                SPICommonViewModel._spi.Config.PromptForCustomerCopyOnEftpos = true;// ConfigurationModel.IsAllowPrintOnEFTPOS;
                SPICommonViewModel._spi.Config.SignatureFlowOnEftpos = true;// ConfigurationModel.IsAllowPrintOnEFTPOS;

                SPICommonViewModel._spi.StatusChanged -= OnStatusChanged1;// Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged -= OnSecretsChanged1; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged -= OnPairingFlowStateChanged1; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged -= OnTxFlowStateChanged1; // Called throughout to transaction process to update us with progress

                SPICommonViewModel._spi.StatusChanged += OnStatusChanged; // Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged += OnSecretsChanged; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged += OnPairingFlowStateChanged; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged += OnTxFlowStateChanged; // Called throughout to transaction process to update us with progress
                SPICommonViewModel._spi.Start();
                #endregion
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }

        private void OnStatusChanged1(object sender, SpiStatusEventArgs spiStatus)
        {
        }

        private void OnPairingFlowStateChanged1(object sender, PairingFlowState pairingFlowState)
        {
        }

        private void OnTxFlowStateChanged1(object sender, TransactionFlowState txFlowState)
        {
        }

        private void OnSecretsChanged1(object sender, Secrets newSecrets)
        {
        }
        private void OnSecretsChanged(object sender, Secrets newSecrets)
        {
            SPICommonViewModel._spiSecrets = newSecrets;
            if (newSecrets != null)
            {
                Settings.APEncKey = newSecrets.EncKey;
                Settings.APHmacKey = newSecrets.HmacKey;
            }
            else
            {
                Settings.APEncKey = string.Empty;
                Settings.APHmacKey = string.Empty;
            }
        }

        private void OnStatusChanged(object sender, SpiStatusEventArgs spiStatus)
        {
            if (SPICommonViewModel._spi.CurrentFlow == SpiFlow.Idle)
            {
               // ProcessStatus = "";
            }
        }

        private void OnPairingFlowStateChanged(object sender, PairingFlowState pairingFlowState)
        {
            //ProcessStatus = "";
        }

        private async void OnTxFlowStateChanged(object sender, TransactionFlowState txFlowState)
        {
            //If the transaction is finished, we take some extra steps.
            /*try
            {
                if (txFlowState != null)
                {
                    var ProcessStatus = "";
                    ProcessStatus = " # Id: " + txFlowState.PosRefId;
                    ProcessStatus = ProcessStatus + "\n # Type: " + txFlowState.Type;
                    ProcessStatus = ProcessStatus + "\n # RequestSent: " + txFlowState.RequestSent;
                    ProcessStatus = ProcessStatus + "\n # WaitingForSignature: " + txFlowState.AwaitingSignatureCheck;
                    ProcessStatus = ProcessStatus + "\n # Attempting to Cancel : " + txFlowState.AttemptingToCancel;
                    ProcessStatus = ProcessStatus + "\n # Finished: " + txFlowState.Finished;
                    ProcessStatus = ProcessStatus + "\n # Outcome: " + txFlowState.Success;
                    ProcessStatus = ProcessStatus + "\n # Display Message: " + txFlowState.DisplayMessage;
                    if (txFlowState.Response != null)
                    {
                        ProcessStatus = "\n # Response : " + txFlowState.Response.ToString();
                    }
                    if (txFlowState.SignatureRequiredMessage != null)
                    {
                        ProcessStatus = "\n # SignatureRequiredMessage : " + txFlowState.SignatureRequiredMessage.ToString();
                    }

                    await Application.Current.MainPage.DisplayAlert("alert", ProcessStatus, "Ok");
                }
                else
                    App.Instance.Hud.DisplayToast("transcation update", Colors.Red, Colors.White);
            }
            catch (Exception e)
            {
                App.Instance.Hud.DisplayToast(e.Message, Colors.Red, Colors.White);

            }*/
            try
            {

             if (txFlowState.Finished)
            {    
                if (txFlowState.Response != null && txFlowState.Type == TransactionType.Settle)
                {
                    if (Settings.GetCachePrinters != null)
                    {
                        var settleResponse = new Settlement(txFlowState.Response);

                        //We need to print the receipt for the customer to sign.
                        var data = settleResponse.GetReceipt().TrimEnd();
                        //ProcessStatus = ProcessStatus + txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();

                        MainThread.BeginInvokeOnMainThread(async() => 
                        {
                            var AvailablePrinter = Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint));
                            var mPOPStarBarcode = DependencyService.Get<IMPOPStarBarcode>();
                            //Ticket starts #70775:The client wants to connect  usb scanner to mc3 print in ipad.by rupesh
                            var mPOPPrinterConfigure = AvailablePrinter != null && AvailablePrinter.Any(x => (!string.IsNullOrEmpty(x.ModelName) && x.ModelName.Contains("POP")) || x.EnableUSBScanner);
                            //var mPOPPrinterConfigure = AvailablePrinter != null && AvailablePrinter.Any();
                            //Ticket end #70775.by rupesh
                            if (mPOPPrinterConfigure)
                            {
                                mPOPStarBarcode.CloseService();
                            }
                            var print = DependencyService.Get<IPrint>();
                            foreach (Printer objPrinter in AvailablePrinter)
                            {
                                await print.DoPrint(null, null, null, null, 0, 0, 0, 0, false, objPrinter, new List<string>(){data}, null,null);
                            }
                            if (mPOPPrinterConfigure)
                            {
                                mPOPStarBarcode.StartService();
                            }
                        });
                        //update settlement reciept to server
                        //Ticket  start:#18093 iOS - About Batch Settlement for Clearent.by rupesh
                        var recieptDataRequest = new RecieptDataRequest
                        {
                            id = Registerclosure.Id,
                            merchant_receipt = data
                        };

                        var response = await outletServices.UpdateRegisterClosureMerchantSettleReciept(Priority.UserInitiated, recieptDataRequest);
                        //Ticket  end:#18093 .by rupesh

                    }
                    SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                    Closed?.Invoke(this, true);
                }
                else
                {
                    // We did not even get a response, like in the case of a time-out.
                }
            }

          }
            catch (Exception e)
            {
                App.Instance.Hud.DisplayToast(e.Message, Colors.Red, Colors.White);

            }


}

        string ClearantBatchReceiptData(ClearantBatchResponse clearantBatchResponse)
        {
            string receipt = string.Empty;

            receipt = ClearantBatchReceiptHeader();

            try
            {
                receipt = " " + Settings.CurrentUser.FullName
                                     + "\n " + receipt
                                     + "\n " + DateTime.Now.ToString("dd-MM-yyyy h:mm")
                                     + "\n "
                                     + "\n " + "MERCHANT ID " + clearantBatchResponse.payload.batch.MerchantId
                                     + "\n " + "TID " + clearantBatchResponse.payload.batch.TerminalId
                                     + "\n " + "NET AMOUNT " + clearantBatchResponse.payload.batch.NetAmount
                                     + "\n " + "REFUND COUNT " + clearantBatchResponse.payload.batch.RefundCount
                                     + "\n " + "REFUND TOTAL " + clearantBatchResponse.payload.batch.RefundTotal
                                     + "\n " + "SALES COUNT " + clearantBatchResponse.payload.batch.SalesCount
                                     + "\n " + "SALES TOTAL " + clearantBatchResponse.payload.batch.SalesTotal
                                     + "\n " + "STATUS " + clearantBatchResponse.payload.batch.status
                                     + "\n " + "TOTAL COUNT " + clearantBatchResponse.payload.batch.TotalCount;
            }
            catch (Exception e)
            {
                e.Track();
            //    Debug.WriteLine("Exception in EConduitReceiptData : " + e.Message);
            }
            return receipt;
        }

        string ClearantBatchReceiptHeader()
        {
            string receipt = string.Empty;
            try
            {
                 var currentOutlet = outletServices.GetLocalOutletById(Settings.SelectedOutletId);
                receipt = Settings.StoreName.ToUpper();
                if (currentOutlet.Address?.Address1 != null)
                {
                    receipt = receipt + "\n " + currentOutlet.Address?.Address1;

                }
                if (currentOutlet.Address?.Address2 != null)
                {
                    receipt = receipt + "\n " + currentOutlet.Address?.Address2;

                }
                if (currentOutlet.Address?.City != null)
                {
                    receipt = receipt + "\n " + currentOutlet.Address?.City;

                }
                if (currentOutlet.Address?.Country != null)
                {
                    receipt = receipt + "," + currentOutlet.Address?.Country.ToUpper();

                }
                if (currentOutlet.Address?.PostCode != null)
                {
                    receipt = receipt + "," + currentOutlet.Address?.PostCode;

                }
                if (currentOutlet.Phone != null)
                {
                    receipt = receipt + "\n " + currentOutlet.Phone;

                }
            }
            catch (Exception e)
            {
                e.Track();
              //  Debug.WriteLine("Exception in AddMonerisReceiptHeader : " + e.Message);
            }
            return receipt;
        }


    }
}
