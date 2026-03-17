using HikePOS.Interfaces;
using Microsoft.Maui.Controls.PlatformConfiguration;

#if ANDROID
using static Android.Provider.Settings;
#elif IOS
using UIKit;
#endif

namespace HikePOS.Services
{
    public static class GetDeviceInfo
    {
        public static string GetDeviceID()
        {
#if ANDROID
            return Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Secure.AndroidId);
#elif IOS
        return UIDevice.CurrentDevice.IdentifierForVendor?.ToString();
#else
            return null;
#endif
        }
    }
}

