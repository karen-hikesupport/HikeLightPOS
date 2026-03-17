using System;
using System.Globalization;

namespace HikePOS.Interfaces
{
    public interface IMultilingual
    {
        CultureInfo CurrentCultureInfo { get; set; }
        CultureInfo DeviceCultureInfo { get; }
        CultureInfo[] CultureInfoList { get; }
        CultureInfo[] NeutralCultureInfoList { get; }
        CultureInfo GetCultureInfo(string name);
        DateTime ConvertIntoCultureDate(DateTime date, string cultureName);
    }
}
