using System;

namespace HikePOS.UserControls
{
    public class CustomPaymentWebView : WebView
    {
		public EventHandler<string> UpdateWebUrl;
		public static readonly BindableProperty UriProperty = BindableProperty.Create(
			propertyName: "Uri",
			returnType: typeof(string),
			declaringType: typeof(CustomPaymentWebView),
			defaultValue: default(string));

		public string Uri
		{
			get { return (string)GetValue(UriProperty); }
			set { SetValue(UriProperty, value); }
		}

	}
}
