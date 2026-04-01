using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
//using Com.Paypal.Paypalretailsdk;
//using static Com.Paypal.Paypalretailsdk.RetailSDK;
using HikePOS.Droid.DependencyServices;
using HikePOS.Services;
using Android.Views.InputMethods;
using AndroidX.Activity.Result;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS;

[Activity(Label = "Hike POS", Theme = "@style/Maui.SplashTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.FullSensor, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, AlwaysRetainTaskState = true, LaunchMode = LaunchMode.SingleTask)]

public class MainActivity : MauiAppCompatActivity
{
   // private static FrameLayout DecorView => Platform.CurrentActivity?.Window?.DecorView == null ? null : Platform.CurrentActivity?.Window?.DecorView as FrameLayout;
    public static MainActivity activity;
    public static Android.Webkit.IValueCallback UploadMessage;
    public static int FILECHOOSER_RESULTCODE = 2;
    public ActivityResultLauncher resultLauncher;
    protected override void OnCreate(Bundle bundle)
    {
        try
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Phone)
                this.RequestedOrientation = ScreenOrientation.SensorPortrait;
            else
                this.RequestedOrientation = ScreenOrientation.SensorLandscape;
            // TabLayoutResource = Resource.Layout.Tabbar;
            // ToolbarResource = Resource.Layout.Toolbar;

            new ImageCropper.Maui.Platform().Init(this);

            base.OnCreate(bundle);
            // DecorView?.SetBackgroundColor(Android.Graphics.Color.Transparent);
            activity = this;
            // activity.SetTheme(Resource.Style.MyTransparentTheme);

            ShowScreenResolution();


            //Intercom.Initialize(this.Application, "android_sdk-966b7f37816a8f19c756aa993ecd318f1e2a045c", "rjkebmxr");

            PreventLinkerFromStrippingCommonLocalizationReferences();



        }
        catch (Exception e)
        {
            e.Track();
            System.Diagnostics.Debug.WriteLine("Exception in OnCreate of MainActivity : " + e.Message + "" + e.StackTrace);
            Toast.MakeText(this, "Error: " + e.Message, ToastLength.Long).Show();

        }
    }

    void ShowScreenResolution()
    {
        var displayMetrics = new DisplayMetrics();
        activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);

        var width = displayMetrics.WidthPixels;
        var height = displayMetrics.HeightPixels;
        System.Diagnostics.Debug.WriteLine("Screen Resolution : " + width + " : " + height);

        System.Diagnostics.Debug.WriteLine("Screen Density : " + displayMetrics.Density + " : " + displayMetrics.DensityDpi + " : " + displayMetrics.ScaledDensity);
    }

    public override void OnLowMemory()
    {
        base.OnLowMemory();
        System.Diagnostics.Debug.WriteLine("In OnLowMemory of : " + this);
        GC.Collect();
    }


    protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
    {
        if (requestCode == FILECHOOSER_RESULTCODE)
        {
            if (null == UploadMessage)
                return;
            //Ticket start:#15324 Android - App Crash When Uploading Product Images.by rupesh
            UploadMessage.OnReceiveValue(Android.Webkit.WebChromeClient.FileChooserParams.ParseResult((int)resultCode, data));
            //Ticket end:#15324 .by rupesh
            UploadMessage = null;
        }
        else
            base.OnActivityResult(requestCode, resultCode, data);
    }
    //Ticket start:#12465 Android - Location Permission for App by rupesh
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
    {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    //Ticket end:#12465 by rupesh
    protected override void OnPause()
    {
        //App.EventNotFromNavigation = true;
        base.OnPause();
    }

    //Ticket start:#34015 iPad: hijari calendar is not working on iPad.by rupesh
    private static void PreventLinkerFromStrippingCommonLocalizationReferences()
    {
        _ = new System.Globalization.GregorianCalendar();
        _ = new System.Globalization.PersianCalendar();
        _ = new System.Globalization.UmAlQuraCalendar();
        _ = new System.Globalization.ThaiBuddhistCalendar();
    }
    //Ticket end:#34015 .by rupesh
    public override bool DispatchTouchEvent(MotionEvent? e)
    {
        if (e!.Action == MotionEventActions.Down)
        {
            var focusedElement = CurrentFocus;
            if (focusedElement is EditText editText)
            {
                var editTextLocation = new int[2];
                editText.GetLocationOnScreen(editTextLocation);
                var clearTextButtonWidth = 100; // syncfusion clear button at the end of the control
                var editTextRect = new Rect(editTextLocation[0], editTextLocation[1], editText.Width + clearTextButtonWidth, editText.Height);
                //var editTextRect = editText.GetGlobalVisibleRect(editTextRect);  //not working in MAUI, always returns 0,0,0,0
                var touchPosX = (int)e.RawX;
                var touchPosY = (int)e.RawY;
                if (!editTextRect.Contains(touchPosX, touchPosY))
                {
                    editText.ClearFocus();
                    var inputService = GetSystemService(Context.InputMethodService) as InputMethodManager;
                    inputService?.HideSoftInputFromWindow(editText.WindowToken, 0);
                }
            }
        }
        return base.DispatchTouchEvent(e);
    }
    private static System.Text.StringBuilder barcodeBuffer = new System.Text.StringBuilder();
    private static long lastScanTime = 0;
    private const int SCAN_TIMEOUT = 50; // milliseconds

    public override bool DispatchKeyEvent(KeyEvent e)
    {
        try
        {
            if (e.Action != KeyEventActions.Down)
                return base.DispatchKeyEvent(e);

            long currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();

            // If delay too long, clear buffer (human typing vs scanner)
            if (currentTime - lastScanTime > SCAN_TIMEOUT)
            {
                barcodeBuffer.Clear();
            }

            lastScanTime = currentTime;

            // Handle Enter key
            if (e.KeyCode == Keycode.Enter)
            {
                string barcode = barcodeBuffer.ToString().Trim();
                barcodeBuffer.Clear();

                if (!string.IsNullOrEmpty(barcode))
                {
                    SendBarcodeToApp(barcode);
                }

                return base.DispatchKeyEvent(e);
            }

            // Handle Backspace key
            if (e.KeyCode == Keycode.Del) // Backspace
            {
                if (barcodeBuffer.Length > 0)
                    barcodeBuffer.Remove(barcodeBuffer.Length - 1, 1);

                return base.DispatchKeyEvent(e);
            }

            // Append printable character to buffer
            char c = (char)e.UnicodeChar;
            if (c != 0)
            {
                barcodeBuffer.Append(c);
                return base.DispatchKeyEvent(e);
            }

            // For all other keys, let Android handle normally
            return base.DispatchKeyEvent(e);
        }
        catch (Exception ex)
        {
            ex.Track();
            return base.DispatchKeyEvent(e);

        }
    }
    private void SendBarcodeToApp(string barcode)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Toast.MakeText(this, barcode, ToastLength.Long).Show();
            WeakReferenceMessenger.Default.Send(new Messenger.BarcodeMessenger(barcode.Trim()));

        });
    }
    // public void GetDeviceName()
    // {
    //     try
    //     {
    //         var context = Android.App.Application.Context;

    //         string deviceName = Android.Provider.Settings.Global.GetString(
    //             context.ContentResolver,
    //             Android.Provider.Settings.Global.DeviceName
    //         );
    //         HikePOS.Helpers.Settings.TerminalId = deviceName;

    //     }
    //     catch (Exception ex)
    //     {
    //         SentrySdk.CaptureException(ex);

    //     }
    // }

}
