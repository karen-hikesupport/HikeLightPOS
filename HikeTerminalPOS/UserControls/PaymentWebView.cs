using System;
using System.Threading.Tasks;

namespace HikePOS.UserControls
{
	public partial class PaymentWebView : WebView
	{
		public EventHandler<bool> WebNavigating;

		public static BindableProperty EvaluateJavascriptProperty = BindableProperty.Create(nameof(EvaluateJavascript), typeof(Func<string, Task<string>>), typeof(PaymentWebView), null, BindingMode.OneWayToSource);

		public Func<string, Task<string>> EvaluateJavascript
		{
			get { return (Func<string, Task<string>>)GetValue(EvaluateJavascriptProperty); }
			set { SetValue(EvaluateJavascriptProperty, value); }
		}
	}
}
