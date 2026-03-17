using System;
using System.Globalization;

namespace HikePOS.Helpers
{
    [AcceptEmptyServiceProvider]
    public class IntegerToBoolConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                if (((int)value) == 1)
                    return true;
            }
            return false;
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

