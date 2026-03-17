using System;
using System.Globalization;
using HikePOS.Resources;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class EnableTextColorConverter: IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is bool)
			{
				if ((bool)value)
				{
					return AppColors.DarkTextColor;
				}
				else
				{
					return AppColors.LightBordersColor;
				}
			}
			else
			{
				return AppColors.LightBordersColor;
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
