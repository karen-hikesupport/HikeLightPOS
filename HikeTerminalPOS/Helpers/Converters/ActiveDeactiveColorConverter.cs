using System;
using System.Globalization;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
    public class ActiveDeactiveColorConverter : IValueConverter, IMarkupExtension
    {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
			{
                if((bool)value)
						return Resources.AppColors.HikeColor;
                else
						return Resources.AppColors.DarkTextColor;
			}
			else
			{
				return Resources.AppColors.DarkTextColor;
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