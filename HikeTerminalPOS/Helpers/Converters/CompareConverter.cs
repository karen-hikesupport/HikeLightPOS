using System;
using System.Globalization;
using HikePOS.Resources;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class CompareConverter: IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is decimal && parameter is decimal)
			{
				if ((decimal)value == (decimal)parameter)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
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
