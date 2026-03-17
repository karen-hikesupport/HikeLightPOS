using System;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;

namespace HikePOS.Enums
{

	public class LocalizedDescriptionAttribute : Attribute
	{
		public string Name { get; private set; }

		public LocalizedDescriptionAttribute(string name)
		{
			Name = LanguageExtension.Localize(name);
		}
	}

	public enum InvoiceFrom : int
	{
		[LocalizedDescriptionAttribute("Migration")]
		Migration = -2,

		[LocalizedDescriptionAttribute("Import")]
		Import = -1,

		[LocalizedDescriptionAttribute("HikeEcommers")]
		HikeEcommers = 0,

		[LocalizedDescriptionAttribute("iPad")]
		iPad = 1,

		[LocalizedDescriptionAttribute("QB")]
		QB = 2,

		[LocalizedDescriptionAttribute("Web")]
		Web = 3,

		[LocalizedDescriptionAttribute("Woo")]
		Woo = 4,

		[LocalizedDescriptionAttribute("BigCommerce")]
		BigCommerce = 5,

		[LocalizedDescriptionAttribute("Shopify")]
		Shopify = 6,

		[LocalizedDescriptionAttribute("Xero")]
		Xero = 7,

		[LocalizedDescriptionAttribute("Magento")]
		Magento = 8,

		[LocalizedDescriptionAttribute("ePages")]
		EPages = 9,

        [LocalizedDescriptionAttribute("Amazon")]
        Amazon = 11,

        [LocalizedDescriptionAttribute("StoreCredit")]
        StoreCredit = 19,

        [LocalizedDescriptionAttribute("Android")] //#34960 iPad: source name is not showing when perform the sale from android.
		Android = 20

	}
}
