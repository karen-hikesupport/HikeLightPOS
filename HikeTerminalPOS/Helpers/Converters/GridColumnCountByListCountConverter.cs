using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class GridColumnCountByListCountConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value == null)
			{
				return 0;
			}
			try
			{
				return ((IList)value).Count > 3 ? 3 : ((IList)value).Count;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
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
