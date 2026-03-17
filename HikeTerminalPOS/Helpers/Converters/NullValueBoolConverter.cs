using System;
using System.Globalization;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class NullValueBoolConverter: IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is string)
			{
				if (string.IsNullOrEmpty(value as string))
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

				if (value == null)
				{
					return false;
				}
				else
				{
					return true;
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


	[AcceptEmptyServiceProvider]
	public class BoolToOpacityConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is bool val)
			{
				if (val)
				{
					return 1;
				}
				else
				{
					return 0.6;
				}
			}
			else
			{
				return 1;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return 1;
		}
	}

}
