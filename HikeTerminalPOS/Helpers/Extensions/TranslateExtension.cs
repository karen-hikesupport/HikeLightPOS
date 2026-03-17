﻿using System;
using System.Diagnostics;
using System.Resources;
using System.Globalization;
using System.Reflection;
using HikePOS.Services;
using HikePOS.Helpers;
using HikePOS.Resx;
using HikePOS.Interfaces;

namespace HikePOS
{
	[ContentProperty("Text")]
	[AcceptEmptyServiceProvider]
	public class TranslateExtension : IMarkupExtension
	{
		public string Text { get; set; }
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (Text == null)
				return null;
			//Debug.WriteLine("Provide: " + Text);
			var translated = LanguageExtension.Localize(Text);
			return translated;
		}
	}

	public class LanguageExtension
	{
        const string ResourceId = "HikePOS.Resx.AppResources";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

		public static string Localize(string key)
		{
            if (key == null)
                return "";

            var CrossMultilingual = DependencyService.Get<IMultilingual>();
            var ci = CrossMultilingual.CurrentCultureInfo;
            var translation = resmgr.Value.GetString(key, ci);
            if (translation == null)
                translation = key; // returns the key, which GETS DISPLAYED TO THE USER

           
            return translation;
		}
	}
}