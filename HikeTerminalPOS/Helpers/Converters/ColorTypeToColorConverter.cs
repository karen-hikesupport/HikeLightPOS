using System;
using System.Globalization;
using HikePOS.Enums;
using HikePOS.Resources;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class ColorTypeToColorConverter: IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Color result = AppColors.ItemBackgroundColor;
			try
			{
				return SetColor(value,parameter); // Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
            }
			catch (Exception ex)
			{
				ex.Track();
				return result;
			}
		}

        // Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
        public Color SetColor(object value ,object parameter)
		{
			Color result = AppColors.ItemBackgroundColor;

			if(value == null || !(value is ColorType))
			{
				if(parameter != null && parameter is string str1 && str1 == "name")
					return AppColors.ProductWhiteColor;
				else if(parameter != null && parameter is string str2 && str2 == "text")
					return AppColors.PrimaryTextColor;
				else if(parameter != null && parameter is string str3 && str3 == "bg")
					return Colors.Transparent;
				else
					return AppColors.ItemBackgroundColor;
			}

			if(parameter != null && parameter is string str4 && str4 == "bg")
			{
				result = Colors.Transparent;
				switch ((ColorType)value)
				{
					case ColorType.Blue:
						result = Color.FromRgb(242, 255, 249);
						break;
					case ColorType.coral:
						result = Color.FromRgb(255, 248, 242);
						break;
					case ColorType.Fiord:
						result = Color.FromRgb(239, 249, 255);
						break;
					case ColorType.Gray:
						result = Color.FromRgb(225, 225, 225);
						break;
					case ColorType.Green:
						result = Color.FromRgb(234, 255, 245);
						break;
					case ColorType.khakirose:
						result = Color.FromRgb(255, 245, 244);
						break;
					case ColorType.Pink:
						result = Color.FromRgb(255, 240, 248);
						break;
					case ColorType.Purple:
						result = Color.FromRgb(242, 239, 255);
						break;
					case ColorType.Red:
						result = Color.FromRgb(255, 242, 242);
						break;
					case ColorType.Salmon:
						result = Color.FromRgb(255, 239, 232);
						break;
					case ColorType.Seagull:
						result = Color.FromRgb(243, 254, 255);
						break;
					case ColorType.Seaping:
						result = Color.FromRgb(255, 242, 242);
						break;
					case ColorType.White:
						result = Color.FromRgb(255, 255, 255);
						break;
					case ColorType.Yellow:
						result = Color.FromRgb(255, 252, 244);
						break;
					case ColorType.Beige:
						result = Color.FromRgb(255, 241, 232);
						break;
					case ColorType.Bj:
						result = Color.FromArgb("#d1c76026");
						break;
					default:
						result = Colors.Transparent;
						break;
				}
			}
			else if(parameter != null && parameter is string str && str == "text")
			{
				result = AppColors.PrimaryTextColor;
				switch ((ColorType)value)
				{
					case ColorType.Fiord:
					case ColorType.Pink:
					case ColorType.Gray:
					case ColorType.Blue:
					case ColorType.Purple:
					case ColorType.Seaping:
					case ColorType.Seagull:
					case ColorType.khakirose:
					case ColorType.Red:
					case ColorType.Bj:
						result = AppColors.ProductWhiteColor;
						break;
					default:
						result = AppColors.PrimaryTextColor;
						break;
				}
			}
			else
			{
				switch ((ColorType)value)
				{
					case ColorType.Blue:
						result = AppColors.ProductBlueColor;
						break;
					case ColorType.coral:
						result = AppColors.ProductCoralColor;
						break;
					case ColorType.Fiord:
						result = AppColors.ProductFiordColor;
						break;
					case ColorType.Gray:
						result = AppColors.ProductGrayColor;
						break;
					case ColorType.Green:
						result = AppColors.ProductGreenColor;
						break;
					case ColorType.khakirose:
						result =AppColors.ProductKhakiColor;
						break;
					case ColorType.Pink:
						result = AppColors.ProductPinkColor;
						break;
					case ColorType.Purple:
						result =AppColors.ProductPurpleColor;
						break;
					case ColorType.Red:
						result = AppColors.ProductRedColor;
						break;
					case ColorType.Salmon:
						result = AppColors.ProductSalmonColor;
						break;
					case ColorType.Seagull:
						result = AppColors.ProductSeagullColor;
						break;
					case ColorType.Seaping:
						result = AppColors.ProductSeapingColor;
						break;
					case ColorType.White:
						result = AppColors.ProductWhiteColor;
						break;
					case ColorType.Yellow:
						result = AppColors.ProductYellowColor;
						break;
					case ColorType.Beige:
						result = AppColors.ProductBeigeColor;
						break;
					case ColorType.Bj:
						result = AppColors.ProductBjColor;
						break;
					default:
						if(parameter != null && parameter is string str1 && str1 == "name")
						{
							result = AppColors.ProductWhiteColor;
						}
						else
						result = AppColors.ItemBackgroundColor;
						break;
				}
			}
			return result;
		}
        // End #90945 Pratik

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
