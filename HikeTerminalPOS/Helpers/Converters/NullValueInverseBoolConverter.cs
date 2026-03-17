using System;
using System.Globalization;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class NullValueInverseBoolConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is string)
			{
				if (string.IsNullOrEmpty(value as string))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{

				if (value == null)
				{
					return true;
				}
				else
				{
					return false;
				}
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
