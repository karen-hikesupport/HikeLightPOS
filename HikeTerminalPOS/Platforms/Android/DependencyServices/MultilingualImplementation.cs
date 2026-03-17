using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using HikePOS.Helpers;
using Android.Widget;
using HikePOS.Droid.DependencyServices;
using HikePOS.Interfaces;
using NodaTime;
using NodaTime.Calendars;

[assembly: Dependency(typeof(MultilingualImplementation))]
namespace HikePOS.Droid.DependencyServices
{
    class MultilingualImplementation : IMultilingual
    {
        CultureInfo _currentCultureInfo = CultureInfo.InstalledUICulture;
        public CultureInfo CurrentCultureInfo
        {
            get
            {
                return _currentCultureInfo;
            }
            set
            {
                _currentCultureInfo = value;
                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                CultureInfo.DefaultThreadCurrentCulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;
            }
        }

        public CultureInfo DeviceCultureInfo { get { return CultureInfo.InstalledUICulture; } }

        public CultureInfo[] CultureInfoList { get { return CultureInfo.GetCultures(CultureTypes.AllCultures); } }

        public CultureInfo[] NeutralCultureInfoList { get { return CultureInfo.GetCultures(CultureTypes.AllCultures); } }

        public CultureInfo GetCultureInfo(string name) { return CultureInfo.GetCultureInfo(name); }

        public DateTime ConvertIntoCultureDate(DateTime date, string cultureName)
        {
            try
            {
                //Ticket #9609 Start: Sales are not showing in Sale History issue. By Nikhil.
                bool shouldCovertDate = !Settings.StoreGeneralRule.UseEnglishCalendarWhenNoEnglishCalendar;
                if (cultureName == "ar-SY" && shouldCovertDate)
                {

                    var islamicCalendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base15, IslamicEpoch.Civil);
                    var localDate = new LocalDate(date.Year, date.Month, date.Day, islamicCalendar);
                    var cal = localDate.WithCalendar(CalendarSystem.Gregorian);
                    date = cal.ToDateTimeUnspecified();
                    //Ticket #9609 End:By Nikhil. 
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            return date;
        }
    }
}