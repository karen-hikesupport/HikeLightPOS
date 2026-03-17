using System;
using System.Collections;
using System.Globalization;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class ListCountValueConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value == null)
			{
				return 0;
			}
			try
			{
				return ((IList)value).Count;
			}
			catch (Exception ex)
			{
				ex.Track();
				return 0;
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
