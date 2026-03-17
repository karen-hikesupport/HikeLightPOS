using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HikePOS.Services;

namespace HikePOS;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp()
    {
        
        return MauiProgram.CreateMauiApp();
    }

    public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
    {
        activity.RequestedOrientation = ScreenOrientation.SensorLandscape;
    }
    private string GetProcessName()
    {
        int pid = Android.OS.Process.MyPid();
        var am = (ActivityManager)GetSystemService(Context.ActivityService);
        foreach (var process in am.RunningAppProcesses)
        {
            if (process.Pid == pid)
                return process.ProcessName;
        }
        return null;
    }

}

