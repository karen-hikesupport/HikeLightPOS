using System;
using System.Globalization;
using HikePOS.Resources;

namespace HikePOS.Helpers
{
    [AcceptEmptyServiceProvider]
	public class CheckBoxImageConverter: IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is bool)
			{
				if ((bool)value)
				{
					return AppImages.TrueRadioIcon;
				}
				else
				{
					return AppImages.FalseRadioIcon;
				}
			}
			else
			{
				return AppImages.FalseRadioIcon;
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
