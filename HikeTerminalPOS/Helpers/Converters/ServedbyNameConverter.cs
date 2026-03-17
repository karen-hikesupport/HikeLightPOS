using System;
using System.Globalization;

namespace HikePOS.Helpers
{
    [AcceptEmptyServiceProvider]
    public class ServedbyNameConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                if (string.IsNullOrEmpty(value as string))
                {
                    return "";
                }
                else
                {
                    string data = value as string;
                    string[] words = data.Split(' ');
                    if (words.Length > 1)
                    {
                        return words[0] + " " + words[1].Substring(0, 1);
                    }
                    else 
                    {
                        return value;
                    }
                }
            }
            else
            {
                return value;
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
