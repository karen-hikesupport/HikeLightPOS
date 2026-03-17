using System;
using System.Globalization;
using HikePOS.Resources;
using Microsoft.Maui.Graphics;

namespace HikePOS.Helpers
{
    [AcceptEmptyServiceProvider]
	public class YesNoToColorConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is bool)
            {
                if ((string)parameter == "yes")
                {
                    if ((bool)value)
                    {
                        return Colors.White;
                    }
                    else
                    {
                        return Colors.Transparent;
                    }
                }
                else
                {
                    if ((bool)value)
                    {
                        return Colors.Transparent;
                    }
                    else
                    {
                        return Colors.White;
                    }
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

    [AcceptEmptyServiceProvider]
    public class SortToColorConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is string)
            {
                if ((string)parameter == (string)value)
                {
                    return AppColors.BorderButtonColor;
                }
                else
                {
                    return Colors.White;
                }
            }
            else
            {
                return Colors.White;
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

