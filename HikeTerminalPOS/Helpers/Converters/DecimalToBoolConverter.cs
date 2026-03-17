using System;
using System.Globalization;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class DecimalToBoolConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is decimal)
			{
				if ((decimal)value==0)
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
                //Start Ticket #66674 iOS: Discount option not available in custom sale edit & custom sale cost price always take 0 (without price) by Pratik
                //return true;
                if (parameter != null && (string)parameter == "null")
                    return false;
                else
                    return true;
                //End Ticket #66674 by Pratik
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
