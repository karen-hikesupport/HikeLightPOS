using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class MultiTriggerConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType,
	   object parameter, CultureInfo culture)
		{
			if ((int)value > 0) // length > 0 ?
				return true;            // some data has been entered
			else
				return false;           // input is empty
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
