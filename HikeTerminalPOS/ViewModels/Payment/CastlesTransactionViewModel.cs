using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using Newtonsoft.Json;
namespace HikePOS.ViewModels
{
    public class CastlesTransactionViewModel : BaseViewModel
    {
        InvoiceDto _Invoice { get; set; }
        public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }
        public CastlesPaymentConfiguration configuration { get; set; }


        //Ticket #11319 Start : Add logs for payment types. By Nikhil
        public PaymentOptionDto paymentOption { get; set; }
        //Ticket #11319 End. By Nikhil

        ApiService<ISaleApi> saleApiService = new ApiService<ISaleApi>();
        public SaleServices saleService;

        public CastlesTransactionViewModel()
        {
            saleService = new SaleServices(saleApiService);
        }
        async public Task<CastlesPaymentResponse> PerformCastlesSalePayment(decimal amount)
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return await ExecuteTransaction(amount, false);

            }
            else
            {
                var response = await Task.Run(async () =>
                {
                    return await ExecuteTransaction(amount, false);
                });

                return response;
            }
        }
        async public Task<CastlesPaymentResponse> PerformCastlesRefundPayment(decimal amount)
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return await ExecuteTransaction(amount, true);

            }
            else
            {
                var response = await Task.Run(async () =>
                {
                    return await ExecuteTransaction(amount, true);
                });

                return response;


            }
        }
        public Task<CastlesPaymentResponse> PerformCastlesVoidPayment(decimal amount)
        {
            return null;

        }
        async Task<CastlesPaymentResponse> ExecuteTransaction(decimal amount,bool isRefund)
        {

            try
            {
                // Setting up the server IP address and port number
                IPAddress serverAddress = IPAddress.Parse(configuration.IPAddress);
                int serverPort = configuration.Port;
                // Creating our client socket
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connecting to the server
                await clientSocket.ConnectAsync(new IPEndPoint(serverAddress, serverPort));
                Console.WriteLine("Connected to server!");
                Random rnd = new Random();
                int transNumber = rnd.Next(000001, 999999);
                // Sending message to the server
                string clientMessage = "";
                if (isRefund)
                {
                   // clientMessage = @"{ 'txnPosTxnId':{transNumber}, 'txnType':'refund','txnAmtTrans':'10.00'}";
                    var refundRequest = new CastlesRefundRequest { txnPosTxnId = transNumber.ToString(), txnType = "refund", txnAmtTrans = amount.ToPositive().ToString() };
                    string tempPaymentobject = JsonConvert.SerializeObject(refundRequest);
                    SaveInLocalbeforePayment(tempPaymentobject, true);
                    clientMessage = JsonConvert.SerializeObject(refundRequest);
                }
                else
                {
                  //  clientMessage = @"{ 'txnPosTxnId':'157290', 'txnType':'sale','txnAmtBase':'11.00'}";
                    var saleRequest = new CastlesSaleRequest { txnPosTxnId = transNumber.ToString(), txnType = "sale", txnAmtBase = amount.ToString() };
                    string tempPaymentobject = JsonConvert.SerializeObject(saleRequest);
                    SaveInLocalbeforePayment(tempPaymentobject, false);
                    clientMessage = JsonConvert.SerializeObject(saleRequest);

                }
                //string clientMessage = @"{ 'txnPosTxnId':'1003141', 'txnType':'void','txnStan':'0023'}";
                Logger.SaleLogger("----Castles request---\n" + clientMessage);
                byte[] temp = Encoding.UTF8.GetBytes(clientMessage);
                clientSocket.Send(temp);
                CastlesPaymentResponse castlesPaymentResponse;
                while (true)
                {
                    // Receiving message from the server
                    byte[] temp2 = new byte[1024];
                    int serverBytes = clientSocket.Receive(temp2);
                    string serverMessage = Encoding.UTF8.GetString(temp2, 0, serverBytes);
                    Console.WriteLine("Received response from server: " + serverMessage);
                    Logger.SaleLogger("----Castles response---\n" + serverMessage);
                    castlesPaymentResponse = JsonConvert.DeserializeObject<CastlesPaymentResponse>(serverMessage);
                    if (castlesPaymentResponse?.TxnReturnCode != null)
                        break;
                }
                // Closing the connection
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                var suceess = false;
                switch(castlesPaymentResponse?.TxnReturnCode)
                {
                    case "00000000":
                        if (castlesPaymentResponse?.TxnType == "void")
                            App.Instance.Hud.DisplayToast("Transaction voided", Colors.Red, Colors.White);
                        else
                        {
                            suceess = true;
                            App.Instance.Hud.DisplayToast("Approved", Colors.Green, Colors.White);
                        }
                        break;
                    case "E0000000":
                        App.Instance.Hud.DisplayToast("TERMINAL ERROR OFFSET", Colors.Red, Colors.White);
                        break;
                    case "E0000001":
                        App.Instance.Hud.DisplayToast("Initialization fail (bad config file…etc )", Colors.Red, Colors.White);
                        break;
                    case "E0000002":
                        App.Instance.Hud.DisplayToast("Invalid parameter", Colors.Red, Colors.White);
                        break;
                    case "E0000003":
                        App.Instance.Hud.DisplayToast("Unsupported function", Colors.Red, Colors.White);
                        break;
                    case "E0000004":
                        App.Instance.Hud.DisplayToast("Device busy", Colors.Red, Colors.White);
                        break;
                    case "E0000005":
                        App.Instance.Hud.DisplayToast("Network error", Colors.Red, Colors.White);
                        break;
                    case "E0000006":
                        App.Instance.Hud.DisplayToast("Poll card timeout, no card detected", Colors.Red, Colors.White);
                        break;
                    case "E0000007":
                        App.Instance.Hud.DisplayToast("Host response timeout, no response from host", Colors.Red, Colors.White);
                        break;
                    case "E0000008":
                        App.Instance.Hud.DisplayToast("User cancel", Colors.Red, Colors.White);
                        break;
                    case "E0000009":
                        App.Instance.Hud.DisplayToast("Declinded by local (card or terminal), if declined by host", Colors.Red, Colors.White);
                        break;
                    case "E000000A":
                        App.Instance.Hud.DisplayToast("Read card fail", Colors.Red, Colors.White);
                        break;
                    case "E000000B":
                        App.Instance.Hud.DisplayToast("Contactless collision (multiple card)", Colors.Red, Colors.White);
                        break;
                    case "E000000C":
                        App.Instance.Hud.DisplayToast("Transaction not found", Colors.Red, Colors.White);
                        break;
                    case "E000000D":
                        App.Instance.Hud.DisplayToast("Settlement fail", Colors.Red, Colors.White);
                        break;
                    case "E000000E":
                        App.Instance.Hud.DisplayToast("Transaction not found", Colors.Red, Colors.White);
                        break;
                    case "E00000010":
                        App.Instance.Hud.DisplayToast("Printer error", Colors.Red, Colors.White);
                        break;
                    case "E00000011":
                        App.Instance.Hud.DisplayToast("Transaction already voided", Colors.Red, Colors.White);
                        break;
                    case "E00000012":
                        App.Instance.Hud.DisplayToast("Transaction declined by card", Colors.Red, Colors.White);
                        break;
                    case "E00000013":
                        App.Instance.Hud.DisplayToast("Trasnaction declined by signature failed", Colors.Red, Colors.White);
                        break;

                    default:
                        App.Instance.Hud.DisplayToast("Trasnaction declined", Colors.Red, Colors.White);
                        break;

                }
                return suceess ? castlesPaymentResponse : null;

            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                App.Instance.Hud.DisplayToast(e.Message,Colors.Red,Colors.White);
                Logger.SaleLogger("----Castles Exception---\n" + e.ToString());
                return null;
            }
        }
        #region Save invoice in local database before payment
        private async void SaveInLocalbeforePayment(string paymentObject, bool isRefund)
        {
            try
            {
                //Ticket start:#24764 iOS - Refund Completed by Mistaken for Integrated Payment.by rupesh
                if (Invoice.Status == InvoiceStatus.Refunded || Invoice.Status == InvoiceStatus.Exchange)
                {
                    return;
                }
                //Ticket end:#24764 .by rupesh

                string currentPaymentObject = string.Empty;
                if (isRefund)
                    currentPaymentObject = "Castles Refund : ";
                else
                    currentPaymentObject = "Castles Sale :  ";

                currentPaymentObject = currentPaymentObject + " : " + paymentObject;
                Invoice.CurrentPaymentObject = currentPaymentObject;
                Invoice.LocalInvoiceStatus = LocalInvoiceStatus.PaymentProcessing;
                await saleService.UpdateLocalInvoice(Invoice, LocalInvoiceStatus.PaymentProcessing);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while saving offline : " + ex.ToString());

            }

        }
        #endregion
    }
}
