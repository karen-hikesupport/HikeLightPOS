﻿using System;

namespace HikePOS
{
	public class HybridWebView : View
	{
        public EventHandler<bool> WebNavigating;
        public EventHandler<string> UpdateWebUrl;

		Action<string> action;

		public static readonly BindableProperty UriProperty = BindableProperty.Create(
			propertyName: "Uri",
			returnType: typeof(string),
			declaringType: typeof(HybridWebView),
			defaultValue: default(string));

		public string Uri
		{
			get { return (string)GetValue(UriProperty); }
			set { SetValue(UriProperty, value); }
		}
	}
}