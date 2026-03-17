using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HikePOS.Droid.DependencyServices;
using HikePOS.Services;

[assembly: Dependency(typeof(GetTimeZoneInfo))]
namespace HikePOS.Droid.DependencyServices
{
    class GetTimeZoneInfo : IGetTimeZoneService
    {
        public TimeZoneInfo getTimeZoneInfo(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id))
                    return TimeZoneInfo.Utc;
                TimeZoneInfo StoreTimeZone = TimeZoneInfo.FindSystemTimeZoneById(id);
                //var dlo = NSTimeZone.LocalTimeZone.DaylightSavingTimeOffset(NSDate.Now);
                return StoreTimeZone;
            }
            catch (Exception ex)
            {
                ex.Track();
                System.Diagnostics.Debug.WriteLine("Exception in getTimeZoneInfo : " + ex.Message + " : " + ex.StackTrace);
            }
            return TimeZoneInfo.Utc;

        }
    }
}