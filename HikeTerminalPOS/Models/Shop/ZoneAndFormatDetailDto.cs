using Realms;
namespace HikePOS.Models
{
	public class ZoneAndFormatDetailDto
	{
		public string TimeZone { get; set; }
		public string Currency { get; set; }
		public int DefaultTax { get; set; }
		public string IanaTimeZone { get; set; }
		public string CurrencySymbol { get; set; }
		public string Culture { get; set; }
        public string SymbolForDecimalSeperatorForNonDot { get; set; }
        public string Language { get; set; }

        public ZoneAndFormatDetailDB ToModel()
        {
            ZoneAndFormatDetailDB zoneAndFormatDetail = new ZoneAndFormatDetailDB
            {
                TimeZone = TimeZone,
                Culture = Culture,
                Currency = Currency,
                CurrencySymbol = CurrencySymbol,
                DefaultTax = DefaultTax,
                IanaTimeZone = IanaTimeZone,
                SymbolForDecimalSeperatorForNonDot = SymbolForDecimalSeperatorForNonDot,
                Language = Language
            };
            return zoneAndFormatDetail;
        }
        public static ZoneAndFormatDetailDto FromModel(ZoneAndFormatDetailDB zoneAndFormatDetailDB)
        {
            if (zoneAndFormatDetailDB == null)
                return null;
            ZoneAndFormatDetailDto zoneAndFormatDetail = new ZoneAndFormatDetailDto
            {
                TimeZone = zoneAndFormatDetailDB.TimeZone,
                Culture = zoneAndFormatDetailDB.Culture,
                Currency = zoneAndFormatDetailDB.Currency,
                CurrencySymbol = zoneAndFormatDetailDB.CurrencySymbol,
                DefaultTax = zoneAndFormatDetailDB.DefaultTax,
                IanaTimeZone = zoneAndFormatDetailDB.IanaTimeZone,
                SymbolForDecimalSeperatorForNonDot = zoneAndFormatDetailDB.SymbolForDecimalSeperatorForNonDot,
                Language = zoneAndFormatDetailDB.Language
            };
            return zoneAndFormatDetail;

        }
    }

    public partial class ZoneAndFormatDetailDB : IRealmObject
    {
        public string TimeZone { get; set; }
        public string Currency { get; set; }
        public int DefaultTax { get; set; }
        public string IanaTimeZone { get; set; }
        public string CurrencySymbol { get; set; }
        public string Culture { get; set; }
        public string SymbolForDecimalSeperatorForNonDot { get; set; }
        public string Language { get; set; }
    }
}
