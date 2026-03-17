using System;
using System.Globalization;
using HikePOS.Resources;
using HikePOS.Enums;
using System.Reflection;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class EnumToStringConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            try
            {
                if (value == null || !(value is Enum))
                    return null;

                var senum = value as Enum;
                var description = senum.ToString();

                var attrib = this.GetAttribute<LocalizedDescriptionAttribute>(senum);
                if (attrib != null)
                    description = attrib.Name;

                return description;
            }
            catch(Exception ex)
            {
                ex.Track();
            }
            return "";
		}

		private T GetAttribute<T>(Enum enumValue) where T : Attribute
		{
			return enumValue.GetType().GetTypeInfo()
				.GetDeclaredField(enumValue.ToString())
				.GetCustomAttribute<T>();
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
		