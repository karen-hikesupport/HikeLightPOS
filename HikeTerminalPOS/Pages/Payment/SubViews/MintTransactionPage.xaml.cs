using System;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.Services;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS
{
	public partial class MintTransactionPage : PopupBasePage<MintTransactionViewModel>
    {
        public event EventHandler<MintPaymentSummary> PaymentSuccessed;
        IMintPayment mintPayment;

		public MintTransactionPage()
		{
			InitializeComponent();
			Title = "Mint payment";
		}




		protected override void OnAppearing()
		{
			base.OnAppearing();

            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.MintPaymentStatusMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.MintPaymentStatusMessenger>(this, (sender, arg) =>
                {
                    string status = arg.Value.Text;
                    if (arg.Value.Type == Enums.MessageType.Success)
                    {
                        PaymentSuccessed?.Invoke(this, arg.Value.Result);
                        status = "Successfuly done";
                        //App.Instance.Hud.Dismiss();
                        ViewModel.IsBusy = false;
                    }
                    else if (arg.Value.Type == Enums.MessageType.Failed)
                    {
                        //App.Instance.Hud.Dismiss();
                        ViewModel.IsBusy = false;
                    }

                    StatusMessage.Text = status;
                });
            }

            if (!WeakReferenceMessenger.Default.IsRegistered<Messenger.MintReaderStatusMessenger>(this))
            {
                WeakReferenceMessenger.Default.Register<Messenger.MintReaderStatusMessenger>(this, (sender, arg) =>
                {
                    string status = arg.Value.Text;
                    if (arg.Value.Type != Enums.MessageType.Info)
                    {
                        ViewModel.IsBusy = false;
                        //App.Instance.Hud.Dismiss();
                    }
                    StatusMessage.Text = status;
                });
            }

            StatusMessage.Text = "";

        }

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
            WeakReferenceMessenger.Default.Unregister<Messenger.MintReaderStatusMessenger>(this);
            WeakReferenceMessenger.Default.Unregister<Messenger.MintPaymentStatusMessenger>(this);
        }

        void UpdateHandle_Clicked(object sender, System.EventArgs e)
        {
            if (ViewModel.IsBusy)
			{
				return;
			}

            if (mintPayment == null)
            {
                mintPayment = DependencyService.Get<IMintPayment>();
            }
            //App.Instance.Hud.DisplayProgress("Please wait...");
			ViewModel.IsBusy = true;
            mintPayment.UpdateReader(ViewModel.AccessToken);
        }


        void PayHandle_Clicked(object sender, System.EventArgs e)
		{
            if (ViewModel.IsBusy)
			{
				return;
			}

			if (mintPayment == null)
			{
                mintPayment = DependencyService.Get<IMintPayment>();
			}
			if (ViewModel.Invoice.Status != Enums.InvoiceStatus.Refunded)
            {
				if (ViewModel.Invoice.TenderAmount > 0)
				{
					//App.Instance.Hud.DisplayProgress("Please wait...");
					ViewModel.IsBusy = true;
                    //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                    var amount = Math.Round((ViewModel.Invoice.TenderAmount + (ViewModel.PaymentOption.DisplaySurcharge ?? 0)) * 100);
                    //End ticket #73190  By Rupesh

                    mintPayment.PaymentRequest(amount.ToString(), "", ViewModel.AccessToken);
				}
				else
				{
					App.Instance.Hud.DisplayToast(string.Format("You can not pay less than {0}0 via mint", Settings.StoreCurrencySymbol));
				}            
            }
            else
            {

				if (ViewModel.Invoice.TenderAmount == 0)
				{
                    App.Instance.Hud.DisplayToast(string.Format("You can not pay {0} tender amount via mint", ViewModel.Invoice.TenderAmount.ToString("C")));
					return;
				}

				if (ViewModel.Invoice.InvoiceRefundPayments != null)
				{
                    var PreviousPayments = ViewModel.Invoice.InvoiceRefundPayments.Where(x => x.PaymentOptionType == Enums.PaymentOptionType.Mint);
					if (PreviousPayments != null && PreviousPayments.Any())
					{
						//foreach (var PreviousPayment in PreviousPayments)
						//{
						var PreviousPayment = PreviousPayments.First();
                        if (PreviousPayment.Amount >= (ViewModel.Invoice.TenderAmount * -1))
						{

                            //ViewModel.Invoice.TenderAmount = PreviousPayment.Amount * -1;
                            var remainingRefundAmount = PreviousPayment.Amount - ViewModel.Invoice.InvoicePayments.Where(x => !x.IsDeleted && x.PaymentOptionType == Enums.PaymentOptionType.Mint).Sum(x => x.Amount.ToPositive());
                            if(remainingRefundAmount <   ViewModel.Invoice.TenderAmount.ToPositive())
                            {
                                App.Instance.Hud.DisplayToast(string.Format("You can not refund more than {0}", remainingRefundAmount.ToString("C")));
                                return;
                            }

							if (PreviousPayment.InvoicePaymentDetails != null && PreviousPayment.InvoicePaymentDetails.Count > 0)
							{
								try
								{
									var paymentobj = PreviousPayment.InvoicePaymentDetails.FirstOrDefault();
									if (paymentobj != null && paymentobj.Value != null)
									{
                                        var transactionDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<MintPaymentSummary>(paymentobj.Value);
                                        if (transactionDetail != null)
                                        {
                                            var TransactionID = transactionDetail.TransactionRequestId;
                                            //Start ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By Rupesh
                                            var refundamount = Math.Round((ViewModel.Invoice.TenderAmount + (ViewModel.PaymentOption.DisplaySurcharge ?? 0)) * -100);
                                            //End ticket #73190 By Rupesh
                                            //App.Instance.Hud.DisplayProgress("Please wait...");
                                            ViewModel.IsBusy = true;
                                            mintPayment.MakeRefund(refundamount.ToString(), TransactionID, ViewModel.AccessToken);
                                        }
									}
									else
									{
										App.Instance.Hud.DisplayToast(string.Format("You can not refund without transaction id"));
										//break;
									}
								}
								catch (Exception ex)
								{
                                    ex.Track();
								}
							}
							else
							{
								App.Instance.Hud.DisplayToast(LanguageExtension.Localize("MintRefundMessage"));
							}
						}
						else
						{
                            App.Instance.Hud.DisplayToast(string.Format("You can not refund more than {0}", PreviousPayment.Amount.ToString("C")));
						}
					}
					else
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("MintRefundMessage"));
					}
				}
				else
				{
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("MintRefundMessage"));
				}
            }
        }

		async void CloseHandle_Clicked(object sender, System.EventArgs e)
		{
			await Close();
		}

		public async Task Close()
		{
			try
			{
				ViewModel.IsBusy = false;
                if (mintPayment == null)
                {
                    mintPayment = DependencyService.Get<IMintPayment>();
                }
                mintPayment.Cancel();
                if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
					await Navigation.PopModalAsync();
			}
			catch (Exception ex)
			{
                ex.Track();
			}
		}
    }
}