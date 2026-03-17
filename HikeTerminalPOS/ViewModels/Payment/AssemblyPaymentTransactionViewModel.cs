using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Serilog;
using SPIClient;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.ViewModels
{
    public class AssemblyPaymentTransactionViewModel : BaseViewModel
    {
        //private Spi _spi;
        //private Secrets _spiSecrets;

        public event EventHandler<AssemblyPaymentResponse> PaymentSuccessed;

        //Ticket #11319 Start : Add logs for payment types. By Nikhil 
        public SubmitLogServices logService { get; set; }
        public Dictionary<string, string> logRequestDetails { get; set; }
        //Ticket #11319 End. By Nikhil
        public PaymentOptionDto paymentOptionDto { get; set; }

        InvoiceDto _Invoice { get; set; }
        public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }

        public AssemblyPaymentConfigurationDto ConfigurationModel { get; set; }

        public ICommand ChargeCommand { get; }
        public ICommand PairUnpairCommand { get; }
        public ICommand Action1Command { get; }
        public ICommand Action2Command { get; }
        public ICommand Action3Command { get; }
        public ICommand Action4Command { get; }

        string _action1Text { get; set; }
        public string Action1Text { get { return _action1Text; } set { _action1Text = value; SetPropertyChanged(nameof(Action1Text)); } }

        string _action2Text { get; set; }
        public string Action2Text { get { return _action2Text; } set { _action2Text = value; SetPropertyChanged(nameof(Action2Text)); } }

        string _action3Text { get; set; }
        public string Action3Text { get { return _action3Text; } set { _action3Text = value; SetPropertyChanged(nameof(Action3Text)); } }


        //Added this varible for ticket #23534 iOS - Westpac Retry Button Doesn't Work
        public bool IsRetry { get; set; } = false;
        



        bool _action1Visible { get; set; }
        public bool Action1Visible { get { return _action1Visible; } set { _action1Visible = value; SetPropertyChanged(nameof(Action1Visible)); } }

        bool _action2Visible { get; set; }
        public bool Action2Visible { get { return _action2Visible; } set { _action2Visible = value; SetPropertyChanged(nameof(Action2Visible)); } }

        bool _action3Visible { get; set; }
        public bool Action3Visible { get { return _action3Visible; } set { _action3Visible = value; SetPropertyChanged(nameof(Action3Visible)); } }


        bool _action4Visible { get; set; }
        public bool Action4Visible { get { return _action4Visible; } set { _action4Visible = value; SetPropertyChanged(nameof(Action4Visible)); } }


        bool _chargeVisible { get; set; }
        public bool ChargeVisible { get { return _chargeVisible; } set { _chargeVisible = value; SetPropertyChanged(nameof(ChargeVisible)); } }

        bool _pairVisible { get; set; } = true;
        public bool PairVisible { get { return _pairVisible; } set { _pairVisible = value; SetPropertyChanged(nameof(PairVisible)); } }



        string _pairUnpairText { get; set; } = "Try to pair again";
        public string PairUnpairText { get { return _pairUnpairText; } set { _pairUnpairText = value; SetPropertyChanged(nameof(PairUnpairText)); } }


        string _status { get; set; }
        public string Status { get { return _status; } set { _status = value; SetPropertyChanged(nameof(Status)); } }

        string _processtatus { get; set; }
        public string ProcessStatus { get { return _processtatus; } set { _processtatus = value; SetPropertyChanged(nameof(ProcessStatus)); } }

        public SaleServices SaleService { get; internal set; }

        public AssemblyPaymentTransactionViewModel()
        {
            ChargeCommand = new Command(Charge);
            PairUnpairCommand = new Command(Pair);
            Action1Command = new Command(Action1);
            Action2Command = new Command(Action2);
            Action3Command = new Command(Action3);
            Action4Command = new Command(Action4);
        } 

        public override void OnAppearing()
        {
            base.OnAppearing();
            

            if (SPICommonViewModel._spi != null && ConfigurationModel != null)
            {
                //if (!string.IsNullOrEmpty(ConfigurationModel.PosId))
                //{
                //	_spi.SetPosId(ConfigurationModel.PosId);
                //}

                if (!string.IsNullOrEmpty(ConfigurationModel.EftposAddress))
                {
                    SPICommonViewModel._spi.SetEftposAddress(ConfigurationModel.EftposAddress);
                }

                SPICommonViewModel._spi.Config.PromptForCustomerCopyOnEftpos = ConfigurationModel.ReceiptFromEFTPOS;
                SPICommonViewModel._spi.Config.SignatureFlowOnEftpos = ConfigurationModel.ReceiptFromEFTPOS;

                SPICommonViewModel._spi.StatusChanged -= OnStatusChanged1;// Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged -= OnSecretsChanged1; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged -= OnPairingFlowStateChanged1; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged -= OnTxFlowStateChanged1; // Called throughout to transaction process to update us with progress


                SPICommonViewModel._spi.StatusChanged += OnStatusChanged; // Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged += OnSecretsChanged; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged += OnPairingFlowStateChanged; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged += OnTxFlowStateChanged; // Called throughout to transaction process to update us with progress

                SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
            }
            ProcessStatus = "";
            Status = "";

            if (SPICommonViewModel._spi == null)
            {
                Start();
            }
            else
            {
                PrintStatusAndActions();
            }
        }

        public override void OnDisappearing()
        {
            if (SPICommonViewModel._spi != null)
            {
                SPICommonViewModel._spi.StatusChanged -= OnStatusChanged; // Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged -= OnSecretsChanged; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged -= OnPairingFlowStateChanged; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged -= OnTxFlowStateChanged; // Called throughout to transaction process to update us with progress

                SPICommonViewModel._spi.StatusChanged += OnStatusChanged1;// Called when Status changes between Unpaired, PairedConnected and PairedConnecting
                SPICommonViewModel._spi.SecretsChanged += OnSecretsChanged1; // Called when Secrets are set or changed or voided.
                SPICommonViewModel._spi.PairingFlowStateChanged += OnPairingFlowStateChanged1; // Called throughout to pairing process to update us with progress
                SPICommonViewModel._spi.TxFlowStateChanged += OnTxFlowStateChanged1; // Called throughout to transaction process to update us with progress
            }
        }


        private void Start()
        {

            // This is where you load your state - like the pos_id, eftpos address and secrets - from your file system or database
            #region Spi Setup


            if (!string.IsNullOrEmpty(Settings.APEncKey) && !string.IsNullOrEmpty(Settings.APHmacKey))
            {
                SPICommonViewModel._spiSecrets = new Secrets(Settings.APEncKey, Settings.APHmacKey);
            }


            // This is how you instantiate Spi.
            SPICommonViewModel._spi = new Spi("HIKEPOS" + Settings.CurrentRegister.Id, Settings.SerialNumber, Settings.EFTPOSAddress, SPICommonViewModel._spiSecrets); // It is ok to not have the secrets yet to start with.
            SPICommonViewModel._spi.SetPosInfo("HIKEPOS", "1.0");
            SPICommonViewModel._spi.SetDeviceApiKey(ServiceConfiguration.DeviceApiKey);
            SPICommonViewModel._spi.SetAutoAddressResolution(true);
            var result = SPICommonViewModel._spi.SetTenantCode(Settings.AcquirerCode);
            SPICommonViewModel._spi.Config.PromptForCustomerCopyOnEftpos = ConfigurationModel.ReceiptFromEFTPOS;
            SPICommonViewModel._spi.Config.SignatureFlowOnEftpos = ConfigurationModel.ReceiptFromEFTPOS;

            SPICommonViewModel._spi.StatusChanged -= OnStatusChanged1;// Called when Status changes between Unpaired, PairedConnected and PairedConnecting
            SPICommonViewModel._spi.SecretsChanged -= OnSecretsChanged1; // Called when Secrets are set or changed or voided.
            SPICommonViewModel._spi.PairingFlowStateChanged -= OnPairingFlowStateChanged1; // Called throughout to pairing process to update us with progress
            SPICommonViewModel._spi.TxFlowStateChanged -= OnTxFlowStateChanged1; // Called throughout to transaction process to update us with progress


            SPICommonViewModel._spi.StatusChanged += OnStatusChanged; // Called when Status changes between Unpaired, PairedConnected and PairedConnecting
            SPICommonViewModel._spi.SecretsChanged += OnSecretsChanged; // Called when Secrets are set or changed or voided.
            SPICommonViewModel._spi.PairingFlowStateChanged += OnPairingFlowStateChanged; // Called throughout to pairing process to update us with progress
            SPICommonViewModel._spi.TxFlowStateChanged += OnTxFlowStateChanged; // Called throughout to transaction process to update us with progress
            SPICommonViewModel._spi.Start();
            var docFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            docFolder = System.IO.Path.Combine(docFolder, "SpiLog");
            if (!Directory.Exists(docFolder))
            {
                Directory.CreateDirectory(docFolder);

            }
            var path = System.IO.Path.Combine(docFolder, "spiLog.log");
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
            .WriteTo.Logger(config => config
                    .WriteTo.File(path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5))
                .CreateLogger();

            #endregion

            PrintStatusAndActions();
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

        private void OnStatusChanged(object sender, SpiStatusEventArgs spiStatus)
        {

            if (SPICommonViewModel._spi.CurrentFlow == SpiFlow.Idle)
            {
                ProcessStatus = "";
            }
            PrintStatusAndActions();
        }

        private void OnPairingFlowStateChanged(object sender, PairingFlowState pairingFlowState)
        {
            ProcessStatus = "";
            Status = pairingFlowState.Message;

            if (pairingFlowState.ConfirmationCode != "")
            {
                ProcessStatus = "MATCH THE FOLLOWING CODE WITH THEEFTPOS. " + pairingFlowState.ConfirmationCode + " DOES CODE MATCH?";
            }
            PrintStatusAndActions();
        }

        bool SignaturePrinted = false;

        private void OnTxFlowStateChanged(object sender, TransactionFlowState txFlowState)
        {
            ProcessStatus = "";
            Status = txFlowState.DisplayMessage;
            //ProcessStatus = " # Id: " + txFlowState.Id;
            //ProcessStatus = ProcessStatus + "\n # Type: " + txFlowState.Type;
            //ProcessStatus = ProcessStatus + "\n # RequestSent: " + txFlowState.RequestSent;
            //ProcessStatus = ProcessStatus + "\n # WaitingForSignature: " + txFlowState.AwaitingSignatureCheck;
            //ProcessStatus = ProcessStatus + "\n # Attempting to Cancel : " + txFlowState.AttemptingToCancel;
            //ProcessStatus = ProcessStatus + "\n # Finished: " + txFlowState.Finished;
            //ProcessStatus = ProcessStatus + "\n # Outcome: " + txFlowState.Success;
            //ProcessStatus = ProcessStatus + "\n # Display Message: " + txFlowState.DisplayMessage;

            if (txFlowState.AwaitingSignatureCheck && !ConfigurationModel.ReceiptFromEFTPOS)
            {
                //We need to print the receipt for the customer to sign.
                var data = txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                if (!string.IsNullOrEmpty(data))
                {
                    data = data + "\r\n\r\n  *MERCHANT COPY*";
                    //ProcessStatus = ProcessStatus + txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                    var AvailablePrinter = Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint));
                    if (AvailablePrinter != null && AvailablePrinter.Count() > 0)
                    {
                        WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter() { AssemblyPaymentReceiptData = new List<string>() { data }, OnlyAssemblyPayment = true }));
                    }

                    SignaturePrinted = true;
                }
            }

            //If the transaction is finished, we take some extra steps.
            if (txFlowState.Finished)
            {
                if (txFlowState.Success == SPIClient.Message.SuccessState.Unknown)
                {

                    //TH-4T, TH-4N, TH-2T - This is the dge case when we can't be sure what happened to the transaction.
                    //Invite the merchant to look at the last transaction on the EFTPOS using the dicumented shortcuts.
                    //Now offer your merchant user the options to:
                    //A. Retry the transaction from scratch or pay using a different method - If Merchant is confident that tx didn't go through.
                    //B. Override Order as Paid in you POS - If Merchant is confident that payment went through.
                    //C. Cancel out of the order all together - If the customer has left / given up without paying
                    //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                    ProcessStatus = ProcessStatus + "\n Not sure if we got paid or not. Check last transaction manually on EFTPOS!";
                    //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                }
                else
                {
                    //We have a result...
                    switch (txFlowState.Type)
                    {
                        //Depending on what type of transaction it was, we might act diffeently or use different data.
                        case TransactionType.Purchase:
                            Purchase(txFlowState);
                            break;
                        case TransactionType.MOTO:

                            Purchase(txFlowState);
                            break;
                        case TransactionType.Refund:
                            if (txFlowState.Response != null)
                            {


                                var refundResponse = new RefundResponse(txFlowState.Response);
                                if (refundResponse != null)
                                {
                                    //ProcessStatus = ProcessStatus + "\n # Scheme: " + refundResponse.SchemeName;
                                    //ProcessStatus = ProcessStatus + "\n # Response: " + refundResponse.GetResponseText();
                                    //ProcessStatus = ProcessStatus + "\n # RRN: " + refundResponse.GetRRN();
                                    //ProcessStatus = ProcessStatus + "\n # Customer Receipt:";

                                    //richtextReceipt.Text = richtextReceipt.Text + System.Environment.NewLine + refundResponse.GetCustomerReceipt().TrimEnd();

                                    var CustomerReceiptData = refundResponse.GetCustomerReceipt();
                                    if (!string.IsNullOrEmpty(CustomerReceiptData))
                                    {
                                        CustomerReceiptData = CustomerReceiptData.TrimEnd();
                                    }

                                    var MerchantReceiptData = refundResponse.GetMerchantReceipt();
                                    if (!string.IsNullOrEmpty(MerchantReceiptData))
                                    {
                                        MerchantReceiptData = MerchantReceiptData.TrimEnd() + "\r\n\r\n  *MERCHANT COPY*";
                                    }

                                    if (refundResponse.Success)
                                    {

                                        SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                                        PaymentSuccessed?.Invoke(this, new AssemblyPaymentResponse()
                                        {
                                            Success = refundResponse.Success,
                                            RequestId = refundResponse.RequestId,
                                            PosRefId = refundResponse.PosRefId,
                                            SchemeName = refundResponse.SchemeName,
                                            SchemeAppName = refundResponse.SchemeAppName,
                                            RRN = refundResponse.GetRRN(),
                                            PurchaseAmount = refundResponse.GetRefundAmount(),
                                            TipAmount = 0,
                                            CashoutAmount = 0,
                                            BankNonCashAmount = 0,
                                            BankCashAmount = 0,
                                            CustomerReceipt = CustomerReceiptData,
                                            MerchantReceipt = MerchantReceiptData,
                                            ResponseText = refundResponse.GetResponseText(),
                                            ResponseCode = refundResponse.GetResponseCode(),
                                            TerminalReferenceId = refundResponse.GetTerminalId(),
                                            CardEntry = refundResponse.GetCardEntry(),
                                            AccountType = refundResponse.GetAccountType(),
                                            AuthCode = refundResponse.GetAuthCode(),
                                            BankDate = refundResponse.GetBankDate(),
                                            BankTime = refundResponse.GetBankTime(),
                                            MaskedPan = refundResponse.GetMaskedPan(),
                                            TerminalId = refundResponse.GetTerminalId(),
                                            MerchantReceiptPrinted = refundResponse.WasMerchantReceiptPrinted(),
                                            CustomerReceiptPrinted = refundResponse.WasCustomerReceiptPrinted(),
                                            SettlementDate = refundResponse.GetSettlementDate(),
                                            IsAllowPrintOnEFTPOS = ConfigurationModel.ReceiptFromEFTPOS,
                                            IsRequiredMerchantCopyToPrint = !SignaturePrinted
                                        });
                                        SignaturePrinted = false;
                                    }
                                    else
                                    {
                                        if (!ConfigurationModel.ReceiptFromEFTPOS)
                                        {
                                            //ProcessStatus = ProcessStatus + txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                                            var AvailablePrinter1 = Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint));
                                            if (AvailablePrinter1 != null && AvailablePrinter1.Count() > 0)
                                            {
                                                var datas = new List<string>();

                                                if (!string.IsNullOrEmpty(MerchantReceiptData) && !SignaturePrinted)
                                                {
                                                    datas.Add(MerchantReceiptData);
                                                }
                                                else
                                                {
                                                    SignaturePrinted = false;
                                                }

                                                if (!string.IsNullOrEmpty(CustomerReceiptData))
                                                {
                                                    datas.Add(CustomerReceiptData);
                                                }
                                                if (datas != null && datas.Count > 0)
                                                {
                                                    WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter() { AssemblyPaymentReceiptData = datas, OnlyAssemblyPayment = true }));
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(txFlowState.Response.GetError()))
                                {
                                    ProcessStatus = ProcessStatus + "\n # Error: " + txFlowState.Response.GetError();
                                }
                                else
                                {
                                    ProcessStatus = ProcessStatus;
                                }
                            }
                            else
                            {
                                // We did not even get a response, like in the case of a time-out.
                            }
                            break;

                        case TransactionType.Settle:
                            if (txFlowState.Response != null)
                            {
                                var settleResponse = new Settlement(txFlowState.Response);
                                //ProcessStatus = ProcessStatus + "\n # Response: " + settleResponse.GetResponseText();
                                ProcessStatus = ProcessStatus + "\n # Error: " + txFlowState.Response.GetError();
                                //ProcessStatus = ProcessStatus + "\n # Merchant Receipt:";
                                //richtextReceipt.Text = richtextReceipt.Text + System.Environment.NewLine + settleResponse.GetReceipt().TrimEnd();

                                if (settleResponse != null)
                                {
                                    //richtextReceipt.Text = richtextReceipt.Text + System.Environment.NewLine + refundResponse.GetCustomerReceipt().TrimEnd();
                                    if (!ConfigurationModel.ReceiptFromEFTPOS)
                                    {
                                        //ProcessStatus = ProcessStatus + txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                                        var AvailablePrinter1 = Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint));
                                        if (AvailablePrinter1 != null && AvailablePrinter1.Count() > 0)
                                        {
                                            var settledata = settleResponse.GetReceipt().TrimEnd();

                                            if (!string.IsNullOrEmpty(settledata))
                                            {
                                                WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter() { AssemblyPaymentReceiptData = new List<string>() { settledata }, OnlyAssemblyPayment = true }));
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                // We did not even get a response, like in the case of a time-out.
                            }
                            break;
                        case TransactionType.GetLastTransaction:
                            if (txFlowState.Response != null)
                            {
                                //Ticket start:#23024 iOS - Westpac Acquirer.by rupesh
                                //var gltResponse = new GetLastTransactionResponse(txFlowState.Response);
                                var gltResponse = new GetTransactionResponse(txFlowState.Response);
                                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                                var amount = Convert.ToInt32((Invoice.TenderAmount + (paymentOptionDto.DisplaySurcharge ?? 0)) * 100);
                                //End ticket #73190  By Rupesh

                                ProcessStatus = ProcessStatus + "\n # Checking to see if it matches the " + Invoice.TenderAmount.ToString("C") + " purchase we did 2 minute ago :)";
                                //  var success = SPICommonViewModel._spi.GltMatch(gltResponse, PurchaseReferenceId);
                                //var success = SPICommonViewModel._spi.GltMatch(gltResponse: gltResponse, expectedType: TransactionType.Purchase, expectedAmount: amount, requestTime: DateTime.Now.Subtract(TimeSpan.FromMinutes(1)), posRefId: ConfigurationModel.PosId);
                                 //if (success == SPIClient.Message.SuccessState.Unknown)
                                if (!gltResponse.Success)
                                {
                                    ProcessStatus = ProcessStatus + "\n # Did not retrieve Expected Transaction.";
                                }
                                //Ticket end:#23024 .by rupesh
                                else
                                {
                                    ProcessStatus = ProcessStatus + "\n # Tx Matched Expected Purchase Request.";
                                    var purchaseResponse = new PurchaseResponse(txFlowState.Response);
                                    if (purchaseResponse != null)
                                    {
                                        var CustomerReceiptData = purchaseResponse.GetCustomerReceipt();
                                        if (!string.IsNullOrEmpty(CustomerReceiptData))
                                        {
                                            CustomerReceiptData = CustomerReceiptData.TrimEnd() + "\r\n\r\n  *CUSTOMER COPY*";
                                        }

                                        var MerchantReceiptData = purchaseResponse.GetMerchantReceipt();
                                        if (!string.IsNullOrEmpty(MerchantReceiptData))
                                        {
                                            MerchantReceiptData = MerchantReceiptData.TrimEnd() + "\r\n\r\n  *MERCHANT COPY*";
                                        }

                                        if (purchaseResponse.Success)
                                        {
                                            SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                                            PaymentSuccessed?.Invoke(this, new AssemblyPaymentResponse()
                                            {
                                                Success = purchaseResponse.Success,
                                                RequestId = purchaseResponse.RequestId,
                                                PosRefId = purchaseResponse.PosRefId,
                                                SchemeName = purchaseResponse.SchemeName,
                                                SchemeAppName = purchaseResponse.SchemeAppName,
                                                RRN = purchaseResponse.GetRRN(),
                                                PurchaseAmount = purchaseResponse.GetPurchaseAmount(),
                                                TipAmount = purchaseResponse.GetTipAmount(),
                                                CashoutAmount = purchaseResponse.GetCashoutAmount(),
                                                BankNonCashAmount = purchaseResponse.GetBankNonCashAmount(),
                                                BankCashAmount = purchaseResponse.GetBankCashAmount(),
                                                CustomerReceipt = CustomerReceiptData,
                                                MerchantReceipt = MerchantReceiptData,
                                                ResponseText = purchaseResponse.GetResponseText(),
                                                ResponseCode = purchaseResponse.GetResponseCode(),
                                                TerminalReferenceId = purchaseResponse.GetTerminalId(),
                                                CardEntry = purchaseResponse.GetCardEntry(),
                                                AccountType = purchaseResponse.GetAccountType(),
                                                AuthCode = purchaseResponse.GetAuthCode(),
                                                BankDate = purchaseResponse.GetBankDate(),
                                                BankTime = purchaseResponse.GetBankTime(),
                                                MaskedPan = purchaseResponse.GetMaskedPan(),
                                                TerminalId = purchaseResponse.GetTerminalId(),
                                                MerchantReceiptPrinted = purchaseResponse.WasMerchantReceiptPrinted(),
                                                CustomerReceiptPrinted = purchaseResponse.WasCustomerReceiptPrinted(),
                                                SettlementDate = purchaseResponse.GetSettlementDate(),
                                                IsAllowPrintOnEFTPOS = ConfigurationModel.ReceiptFromEFTPOS,
                                                IsRequiredMerchantCopyToPrint = !SignaturePrinted
                                            });
                                        }
                                        else
                                        {
                                            if (!ConfigurationModel.ReceiptFromEFTPOS)
                                            {
                                                //ProcessStatus = ProcessStatus + txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                                                var AvailablePrinter1 = Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint));
                                                if (AvailablePrinter1 != null && AvailablePrinter1.Count() > 0)
                                                {
                                                    var datas = new List<string>();

                                                    if (!string.IsNullOrEmpty(MerchantReceiptData) && !SignaturePrinted)
                                                    {
                                                        datas.Add(MerchantReceiptData);
                                                    }
                                                    else
                                                    {
                                                        SignaturePrinted = false;
                                                    }

                                                    if (!string.IsNullOrEmpty(CustomerReceiptData))
                                                    {
                                                        datas.Add(CustomerReceiptData);
                                                    }
                                                    if (datas != null && datas.Count > 0)
                                                    {
                                                        WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter() { AssemblyPaymentReceiptData = datas, OnlyAssemblyPayment = true }));
                                                    }


                                                }
                                            }
                                        }
                                    }

                                    //richtextReceipt.Text = richtextReceipt.Text + System.Environment.NewLine + purchaseResponse.GetMerchantReceipt().TrimEnd();
                                }
                            }
                            else
                            {
                                // We did not even get a response, like in the case of a time-out.
                            }
                            break;
                    }
                }
            }

            PrintStatusAndActions();
            //Task.Delay(TimeSpan.FromMinutes(1));
        }


        public void PrintStatusAndActions()
        {
            //lblStatus.Text = _spi.CurrentStatus + ":" + _spi.CurrentFlow;
            switch (SPICommonViewModel._spi.CurrentStatus)
            {
                case SpiStatus.Unpaired:
                    switch (SPICommonViewModel._spi.CurrentFlow)
                    {
                        case SpiFlow.Idle:
                            Status = "Your payment device seems to be disconnected from your POS.";
                            //ProcessStatus = "An integrated EFTPOS transaction is not allowed";

                            PairUnpairText = "Try to pair again";
                            //PairVisible = true;
                            PairVisible = false;
                            ChargeVisible = false;
                            //Action1Visible = false;
                            //Action2Visible = false;
                            //Action3Visible = false;
                            //PaymentSuccessed?.Invoke(this, null);
                            App.Instance.Hud.DisplayToast("An integrated EFTPOS transaction is not allowed");
                            break;

                        case SpiFlow.Pairing:
                            if (SPICommonViewModel._spi.CurrentPairingFlowState.AwaitingCheckFromPos)
                            {
                                Action1Visible = true;
                                Action1Text = "YES IT MATCHES";
                                Action2Visible = true;
                                Action2Text = "NO, IT DOES NOT";
                                Action3Visible = false;
                                ChargeVisible = false;
                            }
                            else if (!SPICommonViewModel._spi.CurrentPairingFlowState.Finished)
                            {
                                Action1Visible = true;
                                Action1Text = "Cancel";
                                Action2Visible = false;
                                Action3Visible = false;
                                ChargeVisible = false;
                            }
                            else
                            {
                                Action1Visible = true;
                                Action1Text = "OK";
                                Action2Visible = false;
                                Action3Visible = false;
                                ChargeVisible = false;
                            }

                            break;

                        case SpiFlow.Transaction:
                            break;

                        default:
                            Action1Visible = true;
                            Action1Text = "OK";
                            Action2Visible = false;
                            Action3Visible = false;
                            ChargeVisible = false;
                            ProcessStatus = "# .. Unexpected Flow .. " + SPICommonViewModel._spi.CurrentFlow;
                            break;
                    }
                    break;

                case SpiStatus.PairedConnecting:
                    PairUnpairText = "UnPair";
                    PairVisible = false;
                    Action1Visible = false;
                    Action2Visible = false;
                    Action3Visible = false;
                    Action4Visible = false;
                    ChargeVisible = false;
                    Status = "Paired But still trying to connect";
                    ProcessStatus = "(Check network and EFTPOS IP Address)";
                    break;

                case SpiStatus.PairedConnected:
                    switch (SPICommonViewModel._spi.CurrentFlow)
                    {
                        case SpiFlow.Idle:
                            PairUnpairText = "UnPair";
                            PairVisible = false;
                            Action1Visible = false;
                            Action2Visible = false;
                            Action3Visible = false;
                            Action4Visible = false;
                            Status = "Paired and connected";
                            //pnlActions.Visible = true;
                            //lblStatus.BackColor = Color.Green
                            ChargeVisible = true;
                            break;

                        case SpiFlow.Transaction:
                            if (SPICommonViewModel._spi.CurrentTxFlowState.AwaitingSignatureCheck)
                            {
                                Action1Visible = true;
                                Action1Text = "Accept Signature";
                                Action2Visible = true;
                                Action2Text = "Decline Signature";
                                Action3Visible = true;
                                Action3Text = "Cancel";
                                ChargeVisible = false;
                                Action4Visible = false;
                            }
                            else if (!SPICommonViewModel._spi.CurrentTxFlowState.Finished)
                            {
                                Action1Visible = true;
                                Action1Text = "Cancel";
                                Action2Visible = false;
                                Action3Visible = false;
                                ChargeVisible = false;
                                Action4Visible = false;
                            }
                            else if (SPICommonViewModel._spi.CurrentTxFlowState.Finished && SPICommonViewModel._spi.CurrentTxFlowState.Success == SPIClient.Message.SuccessState.Unknown)
                            {
                                Action1Visible = true;
                                Action1Text = "Retry";
                                Action2Visible = true;
                                Action2Text = "Cancel";
                                Action3Visible = false;
                                ChargeVisible = false;
                                Action4Visible = true;

                            }
                            else
                            {
                                switch (SPICommonViewModel._spi.CurrentTxFlowState.Success)
                                {
                                    case SPIClient.Message.SuccessState.Success:
                                        Action1Visible = true;
                                        Action1Text = "OK";
                                        Action2Visible = false;
                                        Action3Visible = false;
                                        ChargeVisible = false;
                                        Action4Visible = false;
                                        break;

                                    case SPIClient.Message.SuccessState.Failed:
                                        Action1Visible = true;
                                        Action1Text = "Retry";
                                        Action2Visible = true;
                                        Action2Text = "Cancel";
                                        Action3Visible = false;
                                        ChargeVisible = false;
                                        Action4Visible = false;
                                        break;

                                    default:
                                        Action1Visible = true;
                                        Action1Text = "OK";
                                        Action2Visible = false;
                                        Action3Visible = false;
                                        ChargeVisible = false;
                                        Action4Visible = false;
                                        break;
                                }
                            }
                            break;

                        case SpiFlow.Pairing:
                            Action1Visible = true;
                            Action1Text = "OK";
                            Action2Visible = false;
                            Action3Visible = false;
                            ChargeVisible = false;
                            Action4Visible = false;
                            break;

                        default:
                            Action1Visible = true;
                            Action1Text = "OK";
                            Action2Visible = false;
                            Action3Visible = false;
                            ChargeVisible = false;
                            Action4Visible = false;
                            ProcessStatus = "# .. Unexpected Flow .. " + SPICommonViewModel._spi.CurrentFlow;
                            break;
                    }
                    break;

                default:
                    Action1Visible = true;
                    Action1Text = "OK";
                    Action2Visible = false;
                    Action3Visible = false;
                    ChargeVisible = false;
                    Action4Visible = false;
                    ProcessStatus = "# .. Unexpected Flow .. " + SPICommonViewModel._spi.CurrentFlow;
                    break;
            }

        }

        void Action1()
        {
            if (Action1Text == "YES IT MATCHES")
            {
                SPICommonViewModel._spi.PairingConfirmCode();
                ProcessStatus = "";
                Status = "";
            }

            else if (Action1Text == "NO, IT DOES NOT")
            {
                SPICommonViewModel._spi.PairingCancel();
                // _frmMain.lblStatus.BackColor = Color.Red;
            }
            else if (Action1Text == "Cancel Pairing")
            {
                SPICommonViewModel._spi.PairingCancel();
                ProcessStatus = "";
                Status = "";
                Action4Visible = false;
                // _frmMain.lblStatus.BackColor = Color.Red;
            }
            else if (Action1Text == "Cancel")
            {
                SPICommonViewModel._spi.CancelTransaction();
                RemoveCancelSaleFromDB();
            }
            else if (Action1Text == "OK")
            {
                SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                

                ProcessStatus = "";

                PrintStatusAndActions();
                if (PairUnpairText == "UnPair")
                {
                    PairVisible = false;
                }
                else
                {
                    // PairVisible = true;
                    PairVisible = false;
                }
                //_frmMain.Enabled = true;
                //_frmMain.btnPair.Enabled = true;
                //_frmMain.textPosId.Enabled = true;
                //_frmMain.textEftposAddress.Enabled = true;
            }
            else if (Action1Text == "Accept Signature")
            {
                SPICommonViewModel._spi.AcceptSignature(true);
            }
            else if (Action1Text == "Retry")
            {
                SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                ProcessStatus = "";
                if (SPICommonViewModel._spi.CurrentTxFlowState.Type == TransactionType.Purchase)
                {
                    IsRetry = true;
                    Charge();
                }
                else
                {
                    Status = "Retry by selecting from the options";
                    PrintStatusAndActions();
                }
            }
        }

        public void Action2()
        {
            if (Action2Text == "Cancel Pairing")
            {
                SPICommonViewModel._spi.PairingCancel();
                //_frmMain.lblStatus.BackColor = Color.Red;
            }
            else if (Action2Text == "NO, IT DOES NOT")
            {
                SPICommonViewModel._spi.PairingCancel();
                // _frmMain.lblStatus.BackColor = Color.Red;
            }
            else if (Action2Text == "Decline Signature")
            {
                SPICommonViewModel._spi.AcceptSignature(false);
            }
            else if (Action2Text == "Cancel")
            {
                SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                ProcessStatus = "";
                PrintStatusAndActions();
            }
        }

        public void Action3()
        {
            if (Action3Text == "Cancel")
            {
                SPICommonViewModel._spi.PairingCancel();
                PrintStatusAndActions();
            }
        }

        //Override Order as Paid
        public void Action4()
        {
            SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
            PaymentSuccessed?.Invoke(this, new AssemblyPaymentResponse()
            {
                Success = true,
                RequestId = "override_sale",
                PosRefId = PurchaseReferenceId,
                SchemeName = "override_schemename",
                SchemeAppName = "override_SchemeAppName",
                RRN = "override_rrn",
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                PurchaseAmount = (int)((Invoice.TenderAmount + (paymentOptionDto.DisplaySurcharge ?? 0)) * 100),
                //End ticket #73190  By Rupesh
                TipAmount = 0,
                CashoutAmount = 0,
                BankNonCashAmount = 0,
                BankCashAmount = 0,
                CustomerReceipt = null,
                MerchantReceipt = null,
                ResponseText = "override_ResponseText",
                ResponseCode = "override_ResponseCode",
                TerminalReferenceId = "override_TerminalId",
                CardEntry = "override_CardEntry",
                AccountType = "override_AccountType",
                AuthCode = "override_AuthCode",
                BankDate = "override_BankDate",
                BankTime = "override_BankTime",
                MaskedPan = "override_MaskedPan",
                TerminalId = "override_TerminalId",
                MerchantReceiptPrinted = true,
                CustomerReceiptPrinted = true,
                SettlementDate = null,
                IsAllowPrintOnEFTPOS = true,
                IsRequiredMerchantCopyToPrint = !SignaturePrinted
            });

            SignaturePrinted = false;
        }

        public async Task<bool> Cancel()
        {
            RemoveCancelSaleFromDB();

            Action4Visible = false;
            //if ((Action1Text == "Cancel Pairing" && Action1Visible) ||( Action2Text == "Cancel Pairing"  &&  Action2Visible))
            //         {
            //             SPICommonViewModel._spi.PairingCancel();
            //	await Task.Delay(100);
            //         }

            //if(Action2Text == "Decline Signature" && Action2Visible)
            //{
            //             SPICommonViewModel._spi.AcceptSignature(false);
            //             await Task.Delay(100);
            //}

            if (Action2Text == "Cancel" && Action2Visible)
            {
                SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                await Task.Delay(100);
            }

            if ((Action3Text == "Cancel" && Action3Visible) || (Action1Text == "Cancel" && Action1Visible))
            {
                SPICommonViewModel._spi.CancelTransaction();
                await Task.Delay(100);
            }
            return true;
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


        void Pair()
        {
            if (PairUnpairText == "Try to pair again")
            {
                //_spi.SetPosId(ConfigurationModel.PosId);
                SPICommonViewModel._spi.SetEftposAddress(ConfigurationModel.EftposAddress);
                SPICommonViewModel._spi.Pair();
                PairVisible = false;
            }
            else if (PairUnpairText == "UnPair")
            {
                SPICommonViewModel._spi.Unpair();
                PairUnpairText = "Try to pair again";
                //PairVisible = true;
                PairVisible = false;
                Action1Visible = false;
                Action2Visible = false;
                Action3Visible = false;
            }
        }

        string PurchaseReferenceId = "";

        void Charge()
        {
            if (ConfigurationModel == null || string.IsNullOrEmpty(ConfigurationModel.PosId))
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("AssemblyPaymentConfigurationMessage"));
                return;
            }

            PurchaseReferenceId = Guid.NewGuid().ToString();


            if (!IsRetry)
            {
                IsRetry = false;
                //Ticket #11319 Start : Add logs for payment types. By Nikhil
                //Ticket start:#31703 IOS: the refund button stuck.by rupesh
                var isRefund = Invoice.Status == Enums.InvoiceStatus.Refunded || Invoice.Status == Enums.InvoiceStatus.RefundedAndDiscard || (Invoice.Status == Enums.InvoiceStatus.Exchange && Invoice.TenderAmount < 0);
                logRequestDetails["IsRefund"] = isRefund.ToString();
                logRequestDetails["PurchaseReferenceId"] = PurchaseReferenceId;
                logRequestDetails["AmountInCents"] = (Invoice.TenderAmount * 100).ToString();
                //Ticket start:#21625 Add Invoice number and Integrated payment response in API log.by rupesh
                logRequestDetails["InvoiceNumber"] = Invoice.Number;
                //Ticket end:#21625 .by rupesh
                //Ticket end:#31703 .by rupesh
                Extensions.SendLogsToServer(logService, paymentOptionDto, logRequestDetails);
                //Ticket #11319 End : By Nikhil
            }
          

            if ((Invoice.Status == Enums.InvoiceStatus.Refunded) || (Invoice.Status == Enums.InvoiceStatus.RefundedAndDiscard)
            || (Invoice.Status == Enums.InvoiceStatus.RefundedAndDiscardBO))
            {
                //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                var amount = Convert.ToInt32((Invoice.TenderAmount + (paymentOptionDto.DisplaySurcharge ?? 0)) * -100);
                //End ticket #73190  By Rupesh


                //if ((Invoice.Status == Enums.InvoiceStatus.RefundedAndDiscard)
                //|| (Invoice.Status == Enums.InvoiceStatus.RefundedAndDiscardBO))
                //{
                //    foreach (var item in Invoice.InvoicePayments)
                //    {
                //        amount = Convert.ToInt32(item.TenderedAmount * -100); ;
                //    }
                //}


                SaveInLocalbeforePayment(amount);

                var refund = SPICommonViewModel._spi.InitiateRefundTx(PurchaseReferenceId, amount);

                if (refund.Initiated)
                {
                    ProcessStatus = ProcessStatus + "\n Refund Initiated. Will be updated with Progress.";
                }
                else
                {
                    ProcessStatus = ProcessStatus + "\n Could not initiate refund: " + refund.Message + ". Please Retry.";
                }
            }
            else

            //if ((Invoice.Status != Enums.InvoiceStatus.Refunded) || (Invoice.Status != Enums.InvoiceStatus.RefundedAndDiscard) 
            //|| (Invoice.Status != Enums.InvoiceStatus.RefundedAndDiscardBO))
            {

                if (Invoice.Status == Enums.InvoiceStatus.Exchange && Invoice.TenderAmount < 0)
                {
                    //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                    var amount = Convert.ToInt32((Invoice.TenderAmount + (paymentOptionDto.DisplaySurcharge ?? 0)) * -100);
                    //End ticket #73190 By Rupesh


                    SaveInLocalbeforePayment(amount);

                    var refund = SPICommonViewModel._spi.InitiateRefundTx(PurchaseReferenceId, amount);

                    if (refund.Initiated)
                    {
                        ProcessStatus = ProcessStatus + "\n Refund Initiated. Will be updated with Progress.";
                    }
                    else
                    {
                        ProcessStatus = ProcessStatus + "\n Could not initiate refund: " + refund.Message + ". Please Retry.";
                    }
                }
                else
                {


                    //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                    var amount = Convert.ToInt32((Invoice.TenderAmount + (paymentOptionDto.DisplaySurcharge ?? 0)) * 100);
                    //End ticket #73190 By Rupesh

                    InitiateTxResult purchase = null;



                    SaveInLocalbeforePayment(amount);
                   
                    if (!paymentOptionDto.IsMoto)
                        purchase = SPICommonViewModel._spi.InitiatePurchaseTx(PurchaseReferenceId, amount);
                    else
                        purchase = SPICommonViewModel._spi.InitiateMotoPurchaseTx(PurchaseReferenceId, amount);


                    if (purchase.Initiated)
                    {
                        ProcessStatus = ProcessStatus + "\n Purchase Initiated. Will be updated with Progress.";
                    }
                    else
                    {
                        ProcessStatus = ProcessStatus + "\n Could not initiate purchase: " + purchase.Message + ". Please Retry.";
                    }
                }
            }
            PrintStatusAndActions();
        }

        // Create common method because same code is using incase of moto.
        private void Purchase(TransactionFlowState txFlowState)
        {
            #region purchase code
            if (txFlowState.Success == SPIClient.Message.SuccessState.Success)
            {
                //TH-6A
                //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                ProcessStatus = ProcessStatus + "\n Hooray! We got paid. Close the order!";
                //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
            }
            else
            {
                //TH-6E
                //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
                ProcessStatus = ProcessStatus + "\n We didn't get paid. Retry payment or give up!";
                //ProcessStatus = ProcessStatus + "\n # ##########################################################################";
            }

            if (txFlowState.Response != null)
            {
                

                var purchaseResponse = new PurchaseResponse(txFlowState.Response);

                if (purchaseResponse != null)
                {
                    var manualType = purchaseResponse.GetCardEntry();
                    Debug.WriteLine(manualType.ToString());


                    //ProcessStatus = ProcessStatus + "\n # Scheme: " + purchaseResponse.SchemeName;
                    //ProcessStatus = ProcessStatus + "\n # Response: " + purchaseResponse.GetResponseText();
                    //ProcessStatus = ProcessStatus + "\n # RRN: " + purchaseResponse.GetRRN();
                    //ProcessStatus = ProcessStatus + "\n # Customer Receipt:";

                    var CustomerReceiptData = purchaseResponse.GetCustomerReceipt();
                    if (!string.IsNullOrEmpty(CustomerReceiptData))
                    {

                        //
                        //You can insert a string right after another string.
                        //

                        int index2 = CustomerReceiptData.IndexOf("APPROVED\r\n\r\n ");
                        string tempstring = CustomerReceiptData.Insert(index2 + "APPROVED\r\n\r\n ".Length, manualType + "\r\n\r\n ");
                        Console.WriteLine(tempstring);

                        CustomerReceiptData = tempstring.TrimEnd();
                        //  CustomerReceiptData = CustomerReceiptData.TrimEnd() + "\r\n\r\n " + manualType;
                    }

                    var MerchantReceiptData = purchaseResponse.GetMerchantReceipt();
                    if (!string.IsNullOrEmpty(MerchantReceiptData))
                    {
                        MerchantReceiptData = MerchantReceiptData.TrimEnd() + "\r\n\r\n " + manualType;
                        MerchantReceiptData = MerchantReceiptData.TrimEnd() + "\r\n\r\n  *MERCHANT COPY*";

                    }



                    if (purchaseResponse.Success)
                    {


                        SPICommonViewModel._spi.AckFlowEndedAndBackToIdle();
                        PaymentSuccessed?.Invoke(this, new AssemblyPaymentResponse()
                        {
                            Success = purchaseResponse.Success,
                            RequestId = purchaseResponse.RequestId,
                            PosRefId = purchaseResponse.PosRefId,
                            SchemeName = purchaseResponse.SchemeName,
                            SchemeAppName = purchaseResponse.SchemeAppName,
                            RRN = purchaseResponse.GetRRN(),
                            PurchaseAmount = purchaseResponse.GetPurchaseAmount(),
                            TipAmount = purchaseResponse.GetTipAmount(),
                            CashoutAmount = purchaseResponse.GetCashoutAmount(),
                            BankNonCashAmount = purchaseResponse.GetBankNonCashAmount(),
                            BankCashAmount = purchaseResponse.GetBankCashAmount(),
                            CustomerReceipt = CustomerReceiptData,
                            MerchantReceipt = MerchantReceiptData,
                            ResponseText = purchaseResponse.GetResponseText(),
                            ResponseCode = purchaseResponse.GetResponseCode(),
                            TerminalReferenceId = purchaseResponse.GetTerminalId(),
                            CardEntry = purchaseResponse.GetCardEntry(),
                            AccountType = purchaseResponse.GetAccountType(),
                            AuthCode = purchaseResponse.GetAuthCode(),
                            BankDate = purchaseResponse.GetBankDate(),
                            BankTime = purchaseResponse.GetBankTime(),
                            MaskedPan = purchaseResponse.GetMaskedPan(),
                            TerminalId = purchaseResponse.GetTerminalId(),
                            MerchantReceiptPrinted = purchaseResponse.WasMerchantReceiptPrinted(),
                            CustomerReceiptPrinted = purchaseResponse.WasCustomerReceiptPrinted(),
                            SettlementDate = purchaseResponse.GetSettlementDate(),
                            IsAllowPrintOnEFTPOS = ConfigurationModel.ReceiptFromEFTPOS,
                            IsRequiredMerchantCopyToPrint = !SignaturePrinted
                        });

                        SignaturePrinted = false;
                    }
                    else
                    {
                        //if (!ConfigurationModel.ReceiptFromEFTPOS)
                        {
                            //ProcessStatus = ProcessStatus + txFlowState.SignatureRequiredMessage.GetMerchantReceipt().TrimEnd();
                            var AvailablePrinter1 = Settings.GetCachePrinters.Where(x => (x.PrimaryReceiptPrint || x.ActiveDocketPrint));
                            if (AvailablePrinter1 != null && AvailablePrinter1.Count() > 0)
                            {
                                var datas = new List<string>();

                                //DECLINED
                                if (!MerchantReceiptData.Contains("DECLINED"))
                                {
                                    if (!string.IsNullOrEmpty(MerchantReceiptData) && !SignaturePrinted)
                                    {
                                        datas.Add(MerchantReceiptData);
                                    }
                                    else
                                    {
                                        SignaturePrinted = false;
                                    }
                                }

                                if (!string.IsNullOrEmpty(CustomerReceiptData))
                                {
                                    datas.Add(CustomerReceiptData);
                                }
                                if (datas != null && datas.Count > 0)
                                {
                                    WeakReferenceMessenger.Default.Send(new Messenger.AutoPrintMessenger(new AutoPrintMessageCenter() { AssemblyPaymentReceiptData = datas, OnlyAssemblyPayment = true }));
                                }
                            }
                        }
                    }
                }


                if (!string.IsNullOrEmpty(txFlowState.Response.GetError()))
                {
                    ProcessStatus = ProcessStatus + "\n # Error: " + txFlowState.Response.GetError();
                }
                else
                {
                    ProcessStatus = ProcessStatus;
                }

                //richtextReceipt.Text = richtextReceipt.Text + System.Environment.NewLine + purchaseResponse.GetCustomerReceipt().TrimEnd();
            }
            else
            {
                // We did not even get a response, like in the case of a time-out.
            }

            #endregion purchase
        }


        #region Save invoice in local database before payment
        private async void SaveInLocalbeforePayment(decimal amount)
        {

            //Ticket start:#24764 iOS - Refund Completed by Mistaken for Integrated Payment.by rupesh
            if (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
            {
                return;
            }
            //Ticket end:#24764 .by rupesh

            string tempWespacData = "PurchaseReferenceId : " + PurchaseReferenceId + " amount : " + amount.ToString();
                Invoice.CurrentPaymentObject = paymentOptionDto.PaymentOptionName + " : " + tempWespacData;
                Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;


                await SaleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.PaymentProcessing);

            


        }

        //#22745 iOS - Cancelled Refund Recorded as Completed
        private void RemoveCancelSaleFromDB()
        {
            try
            {
                SaleService.RemoveLocalPendingInvoice(Invoice, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ex : " + ex.ToString());
            }
        }
        //#22745 iOS - Cancelled Refund Recorded as Completed
        #endregion
    }
}