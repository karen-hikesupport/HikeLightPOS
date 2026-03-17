using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class StringToAvtarNameConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//Start #81159 Pratik
			if(value == null)
				return string.Empty;

			if (value is string str)
			{
				try
				{
					if (!string.IsNullOrEmpty(str))
					{
						return CommonMethods.GetProductName(str);
                    }
				}
				catch (Exception ex)
				{
					ex.Track();
				}
			}
			
			return string.Empty;
			//End #81159 Pratik
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
