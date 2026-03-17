using System;
using System.Globalization;
using HikePOS.Enums;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class StatusNameToStatusColorConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            if (value is InvoiceStatus)
			{
				switch (value.ToString().ToUpper())
				{
					case "PARKED":
						return Resources.AppColors.ParkedLabelColor;

					case "ORDER RECEIVED":
						return Resources.AppColors.OrderReceivedLabelColor;

					case "PROCESSING":
						return Resources.AppColors.ProcessingLabelColor;

					case "DISPATCHED":
						return Resources.AppColors.DispatchedLabelColor;
					case "COMPLETED":
						return Resources.AppColors.HikeColor;
					default:
						return Resources.AppColors.DarkTextColor;
				}
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
