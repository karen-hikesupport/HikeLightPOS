//using System;
//using HikePOS.Droid.DependencyServices;
//using HikePOS.Models;
//using HikePOS.Models.Enum;
//using HikePOS.Services.Payment;
//using HikePOS.Services;
//using Android.App;
//using Com.Paypal.Paypalretailsdk;
//using static Com.Paypal.Paypalretailsdk.RetailSDK;
//using HikePOS.Helpers;
//using static Com.Paypal.Paypalretailsdk.DeviceManager;
//using Java.Math;
//using System.Diagnostics;
//using static Com.Paypal.Paypalretailsdk.TransactionManager;
//using static Com.Paypal.Paypalretailsdk.TransactionContext;
//using static Com.Paypal.Paypalretailsdk.DeviceUpdate;
//using Java.Lang;
//using Exception = Java.Lang.Exception;

//[assembly: Dependency(typeof(PaypalHere))]
//namespace HikePOS.Droid.DependencyServices
//{
//    public class PaypalHere : IPaypalHere
//    {
//        public TransactionContext transaction;
//        string InvoiceNumber;
//        public decimal Amount;
//        string Token;
//        string RefreshUrl;

//        bool IsInitializeSDK = false;
//        public Action<Message<PaypalPaymentResult>> PaymentResultAction;
//        public bool IsRefund;
//        string PaypalInvoiceId;
//        string TransactionNumber;
//        public PaypalRetailInvoicePaymentMethod InvoicePaymentMethod;


//        public async void InitializeSDK(string token, string refreshUrl, string invoiceNumber, decimal amount, Action<Message<PaypalPaymentResult>> paymentResultAction, bool isRefund, string paypalInvoiceId, string transactionNumber, PaypalRetailInvoicePaymentMethod invoicePaymentMethod)
//        {

//            InvoiceNumber = invoiceNumber;
//            Amount = amount;
//            PaymentResultAction = paymentResultAction;
//            Token = token;
//            RefreshUrl = refreshUrl;

//            IsRefund = isRefund;
//            PaypalInvoiceId = paypalInvoiceId;
//            TransactionNumber = transactionNumber;
//            InvoicePaymentMethod = invoicePaymentMethod;

//            //Ticket start:#12465 Android - Location Permission for App by rupesh
//            var status = await Xamarin.Essentials.Permissions.CheckStatusAsync<Xamarin.Essentials.Permissions.LocationWhenInUse>();
//            if (status != Xamarin.Essentials.PermissionStatus.Granted)
//            {
//                status = await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.LocationWhenInUse>();
//            }
//            //Ticket end:#12465 by rupesh

//            if (!IsInitializeSDK)
//            {

//                RetailSDK.Initialize(MainActivity.activity, new AppState(), new AppInfo("HikePos", "1.0", "1.0"));
//                //#if DEBUGSMU
//                //                                var error = PayPalRetailSDK.InitializeSDK;
//                //#else
//                //                var error = PayPalRetailSDK.InitializeSDK();
//                //#endif
//                //  RetailSDK sdk = null;

//                //var error = RetailSDK;
//                //if (error == null)
//                //{
//                //    IsInitializeSDK = true;
//                //}
//                    IsInitializeSDK = true;

//            }

//            if (IsInitializeSDK)
//            {
//                var IsConnected = RetailSDK.DeviceManager.IsConnectedToMiura();
//                 if (IsConnected.BooleanValue() == false)
//                 {
//                    //PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Info, "Searching PaypalHere reader device."));
//                    //RetailSDK.DeviceManager.SearchAndConnect(HandlePPRetailDeviceManagerConnectionHandler);
//                    var connectionCallback = new ConnectionCallback();
//                    connectionCallback.paypalHere = this;
//                    RetailSDK.DeviceManager.SearchAndConnect(connectionCallback);
//                 }
//                 else
//                 {
//                     InitializeMerchantAndCreateInvoice();
//                 }
//            }
//        }
//        public void InitializeMerchantAndCreateInvoice()
//        {
//            try
//            {
//                var env = "live";
//                if (Settings.AppEnvironment != (int)Models.Enum.AppEnvironment.Live)
//                {
//                    env = "sandbox";

//                }

//                var credentials = new SdkCredential(env,Token);
//                credentials.SetTokenRefreshCredentials(RefreshUrl);
//                if (credentials != null)
//                {
//                    var merchantInitializedCallback =  new MerchantInitializedCallback();
//                    merchantInitializedCallback.paypalHere = this;
//                    RetailSDK.InitializeMerchant(credentials, merchantInitializedCallback);
//                }


//            }
//            catch (Exception ex)
//            {

//                ex.Track();
//            }
//        }
//        public void CreateInvoice(string Currency)
//        {
//            // var tokenDefault = NSUserDefaults.StandardUserDefaults;
//            //var merchCurrency = tokenDefault.StringForKey("MERCH_CURRENCY");

//            RetailInvoice mInvoice = new RetailInvoice(Currency);
//            var price = new BigDecimal(Amount.ToString());
//            var quantity = new BigDecimal(1);
//            mInvoice.AddItem("Invoice #" + InvoiceNumber + " from Hike POS Register ", price, quantity, (Java.Lang.Integer)123, null);
//            // mInvoice.Number = Guid.NewGuid().ToString();


//            var rnd = new Random();
//            mInvoice.Number = "sdk2test" + rnd.Next(1, 99999);

//            if (mInvoice.ItemCount.IntValue() > 0 && mInvoice.Total.IntValue() > 0)
//            {
//                Debug.WriteLine("");
//            }
//            else
//            {
//                return;
//            }


//            Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//            {

//                if (mInvoice.ItemCount.IntValue() > 0 && mInvoice.Total.IntValue() > 0)
//                {
//                    PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Info, "Paypal creating invoice..."));
//                    var transactionCallBack = new TransactionCallBack();
//                    transactionCallBack.paypalHere = this;
//                    RetailSDK.TransactionManager.CreateTransaction(mInvoice, transactionCallBack);
//                }
//                else
//                {
//                    PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Failed, "Error! You can not perform less than $1 sale using Paypal."));
//                    Console.WriteLine("less than $1 or error creating invoice");
//                }
//            });
//        }
//       public void ProvideRefund()
//        {
//            if (IsInitializeSDK)
//            {
//                var refundTransactionCallBack = new RefundTransactionCallBack();
//                refundTransactionCallBack.paypalHere = this;
//                RetailSDK.TransactionManager.CreateRefundTransaction(PaypalInvoiceId, TransactionNumber, Com.Paypal.Paypalretailsdk.InvoicePaymentMethod.Paypal, refundTransactionCallBack);
//            }
//        }


//        public void ChargeTransaction()
//        {

//            if (transaction == null)
//                return;
//            var cardPresentedCallback =new CardPresentedCallback();
//            cardPresentedCallback.paypalHere = this;
//            transaction.SetCardPresentedHandler(cardPresentedCallback);

//            var transactionCompletedCallback = new TransactionCompletedCallback();
//            transactionCompletedCallback.paypalHere = this;
//            transaction.SetCompletedHandler(transactionCompletedCallback);

//            // Setting up the options for the transaction
//            var options = new TransactionBeginOptions();
//            options.ShowPromptInCardReader = new Java.Lang.Boolean(true);
//            options.ShowPromptInApp = new Java.Lang.Boolean(true);
//            options.TippingOnReaderEnabled = new Java.Lang.Boolean(false);
//            options.AmountBasedTipping = new Java.Lang.Boolean(false);
//            options.IsAuthCapture = new Java.Lang.Boolean(false);

//            try
//            {
//                Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//                {
//                    PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Info, "Begin Payment"));
//                if (transaction.PaymentState != PaymentState.InProgress)
//                {
//                    transaction.BeginPayment(options);
//                }
//                                });

//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//            }
//        }
//        public void CheckForReaderUpdate(PaymentDevice reader)
//        {
//            if (reader != null && reader.PendingUpdate != null)
//            {
//                Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//                {

//                    PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Info, "Updating PaypalHere device."));
//                    var completedCallback = new CompletedCallback();
//                    completedCallback.paypalHere = this;
//                    reader.PendingUpdate.Offer(completedCallback);
//                });
//            }
//        }

//        public void FindAndConnectDevice()
//        {
//            throw new NotImplementedException();
//        }

//    }
//    public class AppState : Java.Lang.Object, IAppState
//    {
//        public Android.App.Activity CurrentActivity
//        {
//            get
//            {
//                return MainActivity.activity;
//            }
//        }

//        public bool IsTabletMode
//        {


//            get
//            {
//                return false;

//            }


//        }
//    }
//    public class MerchantInitializedCallback : Java.Lang.Object, IMerchantInitializedCallback
//    {
//       public PaypalHere paypalHere;
//        public void MerchantInitialized(RetailSDKException error, Merchant merchant)
//        {
//            try
//            {
//                //Ticket start.#15055 Android - App Crash When Paying with PayPal.by rupesh
//                Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//                {

//                    if (error == null)
//                    {
//                        Console.WriteLine("Merchant Success!");
//                        paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Info, "Payment merchant is initialized"));
//                        if (!paypalHere.IsRefund)
//                        {
//                            paypalHere.CreateInvoice(merchant.Currency);
//                        }
//                        else
//                        {
//                            paypalHere.ProvideRefund();
//                        }
//                    }
//                    else
//                    {
//                        paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Failed, "Error! Getting error at initializing Paypal merchant."));
//                    }
//                });
//                //Ticket end.#15055 Android - App Crash When Paying with PayPal.by rupesh

//            }
//            catch (Exception ex)
//            {

//                ex.Track();
//            }
//        }
//    }
//    public class ConnectionCallback : Java.Lang.Object, IConnectionCallback
//    {
//       public PaypalHere paypalHere;
//        public void Connection(RetailSDKException error, PaymentDevice reader)
//        {
//            Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//            {

//                if (error == null)
//                {
//                    if (reader != null && reader.IsConnected().BooleanValue())
//                    {
//                        paypalHere.CheckForReaderUpdate(reader);
//                        Console.WriteLine("Device is connected");
//                        paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Info, "PaypalHere device is connected."));
//                        paypalHere.InitializeMerchantAndCreateInvoice();
//                    }
//                    else
//                    {
//                        paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Failed, "Error! Getting error at searching PaypalHere device."));
//                    }
//                }
//                else
//                {

//                    //Console.WriteLine("Search Device Error: {0}", error.DebugId);
//                    //Console.WriteLine("Search Device Error: {0}", error.Code);
//                    //Console.WriteLine("Search Device Error: {0}", error.Message);
//                    paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Failed, "Error! Getting error at searching PaypalHere device."));
//                }
//            });
//        }
//    }
//    public class TransactionCallBack : Java.Lang.Object, ITransactionCallback
//    {
//        public PaypalHere paypalHere;

//        public void Transaction(RetailSDKException error, TransactionContext tc)
//        {
//            Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//            {

//                if (error == null)
//                {
//                    paypalHere.transaction = tc;
//                    Console.WriteLine(tc);
//                    //FindAndConnectDevice();
//                    paypalHere.ChargeTransaction();
//                }
//                else
//                {
//                    paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Failed, "Error! Getting error at creating Paypal invoice."));
//                }
//            });
//        }
//    }
//    public class CardPresentedCallback : Java.Lang.Object, ICardPresentedCallback
//    {
//        public PaypalHere paypalHere;
//        public void CardPresented(Card p0)
//        {
//            paypalHere.transaction.ContinueWithCard(p0);

//        }
//    }
//    public class TransactionCompletedCallback : Java.Lang.Object,ITransactionCompletedCallback
//    {
//        public PaypalHere paypalHere;


//        public void TransactionCompleted(RetailSDKException error, TransactionRecord txnRecord)
//        {
//            Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//            {


//            if (error != null)
//            {
//                Console.WriteLine("Error Code: {0}", error.Code);
//                Console.WriteLine("Error Message: {0}", error.Message);
//                Console.WriteLine("Debug Id: {0}", error.DebugId);
//                paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Info, "Error! " + error.Message));
//                return;
//            }

//            Console.WriteLine("Txn ID: {0}", txnRecord.TransactionNumber);
//            paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Success, txnRecord.TransactionNumber, new PaypalPaymentResult()
//            {
//                TransactionNumber = txnRecord.TransactionNumber,
//                InvoiceId = txnRecord.InvoiceId,
//                PaymentMethod = (PaypalRetailInvoicePaymentMethod)txnRecord.PaymentMethod.Value,
//                AuthCode = txnRecord.AuthCode.ToJson(),
//                TransactionHandle = txnRecord.TransactionHandle.ToJson(),
//                ResponseCode = txnRecord.ResponseCode,
//                CorrelationId = txnRecord.CorrelationId
//            }));
//            });

//        }
//    }
//    public class RefundTransactionCallBack : Java.Lang.Object, ITransactionCallback
//    {
//        public PaypalHere paypalHere;

//        public void Transaction(RetailSDKException error, TransactionContext tc)
//        {
//            try
//            {
//                Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//                {

//                    var refundCardPresentedCallback = new RefundCardPresentedCallback();
//                    refundCardPresentedCallback.tc = tc;
//                    tc.SetCardPresentedHandler(refundCardPresentedCallback);

//                    var refundTransactionCompletedCallback = new RefundTransactionCompletedCallback();
//                    refundTransactionCompletedCallback.paypalHere = paypalHere;
//                    tc.SetCompletedHandler(refundTransactionCompletedCallback);
//                    paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Info, "Begin Refund"));
//                    tc.BeginRefund(new Java.Lang.Boolean(true), new BigDecimal((paypalHere.Amount * -1).ToString()));
//                });
//            }
//            catch (Exception ex)
//            {

//            }
//        }
//    }
//    public class RefundTransactionCompletedCallback : Java.Lang.Object, ITransactionCompletedCallback
//    {
//        public PaypalHere paypalHere;


//        public void TransactionCompleted(RetailSDKException error, TransactionRecord txnRecord)
//        {
//            if (error != null)
//            {
//                Console.WriteLine("Error Code: {0}", error.Code);
//                Console.WriteLine("Error Message: {0}", error.Message);
//                Console.WriteLine("Debug Id: {0}", error.DebugId);

//                return;
//            }
//            Console.WriteLine("Refund ID: {0}", txnRecord.TransactionNumber);
//            Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//            {

//                paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Success, txnRecord.TransactionNumber, new PaypalPaymentResult()
//                {
//                    TransactionNumber = txnRecord.TransactionNumber,
//                    InvoiceId = txnRecord.InvoiceId,
//                    PaymentMethod = (PaypalRetailInvoicePaymentMethod)txnRecord.PaymentMethod.Value,
//                    AuthCode = txnRecord.AuthCode.ToJson(),
//                    TransactionHandle = txnRecord.TransactionHandle.ToJson(),
//                    ResponseCode = txnRecord.ResponseCode,
//                    CorrelationId = txnRecord.CorrelationId
//                }));
//            });

//        }
//    }

//    public class RefundCardPresentedCallback : Java.Lang.Object, ICardPresentedCallback
//    {
//        public TransactionContext tc;
//        public void CardPresented(Card p0)
//        {
//            tc.ContinueWithCard(p0);

//        }
//    }
//    public class CompletedCallback : Java.Lang.Object, ICompletedCallback
//    {
//        public PaypalHere paypalHere;
//        public void Completed(RetailSDKException error, Java.Lang.Boolean updateComplete)
//        {
//            Xamarin.Forms.MainThread.BeginInvokeOnMainThread(() =>
//            {

//                if (updateComplete.BooleanValue())
//                {
//                    Console.WriteLine("Reader update complete.");
//                    paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Info, "PaypalHere device is updated."));
//                }
//                else
//                {
//                    Console.WriteLine("Error in offer step: {0}", error.DebugId);
//                    Console.WriteLine("Error in offer step: {0}", error.Code);
//                    Console.WriteLine("Error in offer step: {0}", error.Message);
//                    paypalHere.PaymentResultAction?.Invoke(new Message<PaypalPaymentResult>(Enums.MessageType.Failed, "Error! Getting error at updating PaypalHere device."));
//                }
//            });
//        }
//    }


//}
