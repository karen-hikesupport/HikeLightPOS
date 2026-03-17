using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using HikePOS.Droid.DependencyServices;
using HikePOS.Interfaces;

[assembly: Dependency(typeof(iOSKeyboardHelper))]
namespace HikePOS.Droid.DependencyServices
{
    class iOSKeyboardHelper : IKeyboardHelper
    {
        public void HideKeyboard()
        {
            Activity activity = MainActivity.activity;
            if (activity != null&& activity.CurrentFocus!=null) { 
            InputMethodManager inputManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);

            inputManager.HideSoftInputFromWindow(activity.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
            }
        }
    }
}