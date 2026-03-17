using System;
using System.Globalization;
using HikePOS.Resources;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class CheckButtonBackgroundConverter: IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is bool)
			{
				if ((bool)value)
				{
					return ((Color)parameter);
				}
				else
				{
					return Colors.Transparent;
				}
			}
			else
			{
				return Colors.Transparent;
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
