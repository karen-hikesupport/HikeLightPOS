using System;
using System.Threading.Tasks;
using HikePOS.Models;

namespace HikePOS.UserControls
{
	public partial class TyroWebView : WebView
	{
		public static BindableProperty PerformTyroOperationAsyncProperty = BindableProperty.Create(nameof(PerformTyroOperationAsync), typeof(Func<TyroPaymentInput, Task<PaymentResult>>), typeof(TyroWebView), null, BindingMode.OneWayToSource);

		public Func<TyroPaymentInput, Task<PaymentResult>> PerformTyroOperationAsync
		{
			get { return (Func<TyroPaymentInput, Task<PaymentResult>>)GetValue(PerformTyroOperationAsyncProperty); }
			set { SetValue(PerformTyroOperationAsyncProperty, value); }
		}

		public static BindableProperty CancelPaymentProperty = BindableProperty.Create(nameof(CancelPayment), typeof(Func<bool, Task<string>>), typeof(TyroWebView), null, BindingMode.OneWayToSource);

		public Func<bool, Task<string>> CancelPayment
		{
			get { return (Func<bool, Task<string>>)GetValue(CancelPaymentProperty); }
			set { SetValue(CancelPaymentProperty, value); }
		}

       

		public static BindableProperty CheckPaymentProcessIsActiveProperty = BindableProperty.Create(nameof(CheckPaymentProcessIsActive), typeof(Func<bool, Task<string>>), typeof(TyroWebView), null, BindingMode.OneWayToSource);

		public Func<bool, Task<string>> CheckPaymentProcessIsActive
		{
			get { return (Func<bool, Task<string>>)GetValue(CheckPaymentProcessIsActiveProperty); }
			set { SetValue(CheckPaymentProcessIsActiveProperty, value); }
		}
	}
}
