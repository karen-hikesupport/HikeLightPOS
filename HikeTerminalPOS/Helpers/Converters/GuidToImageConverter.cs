using System;
using System.Diagnostics;
using System.Globalization;
using HikePOS.Services;

namespace HikePOS.Helpers
{
    [AcceptEmptyServiceProvider]
    public class GuidToImageConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                {
                    return string.Empty;
                }
                var imgUrl = value.ToString().GetImageUrl((string)parameter);
                //Debug.WriteLine("imgUrl : " + imgUrl);
                return imgUrl;

            }
            catch (Exception ex)
            {
                ex.Track();
                return string.Empty;
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
