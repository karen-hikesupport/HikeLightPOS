using System;
using System.Globalization;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class NullOrZeroValueToBoolConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return false;
			}
            else if(value is int  && (int)value == 0)
            {
                return false;
            }
            else if (value is decimal && (decimal)value == 0)
            {
                return false;
            }
            else
			{
				return true;
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
