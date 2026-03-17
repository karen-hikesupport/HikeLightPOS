using System;

namespace HikePOS
{
	public class AdminWebView : WebView
	{
		//public static BindableProperty EvaluateJavascriptProperty = BindableProperty.Create(nameof(EvaluateJavascript), typeof(Func<string, Task<string>>), typeof(PaymentWebView), null, BindingMode.OneWayToSource);

		//public Func<string, Task<string>> EvaluateJavascript
		//{
		//	get { return (Func<string, Task<string>>)GetValue(EvaluateJavascriptProperty); }
		//	set { SetValue(EvaluateJavascriptProperty, value); }
		//}

		public EventHandler<bool> WebNavigating;

	}
}
