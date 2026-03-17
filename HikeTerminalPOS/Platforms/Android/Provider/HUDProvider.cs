using System;
using Android.App;
using AndroidHUD;
using Microsoft.Maui.Platform;
using HikePOS.Droid;
using Google.Android.Material.Snackbar;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;

[assembly: Dependency(typeof(HUDProvider))]
namespace HikePOS.Droid
{
	public class HUDProvider : IHUDProvider
	{
        Activity activity;
      //  private AndHUD andHud = new AndHUD();

        public HUDProvider()
        {
            activity = MainActivity.activity;
            

        }
		public void DisplayProgress(string message)
		{
            if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity == null)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    AndHUD.Shared.Show(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, null, -1, MaskType.Clear);
                }
                else
                {
                    AndHUD.Shared.Show(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, message, -1, MaskType.Clear);
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

		IDisposable currentToast;
		public IDisposable DisplayToast(string Message, Color BackgroundColor = null, Color MessageTextColor = null, TimeSpan? Duration = null)
		{
            this.currentToast?.Dispose();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    ToastDuration duration = ToastDuration.Short;
                    double fontSize = 14;
                    var toast = Toast.Make(Message, duration, fontSize);
                    if(toast != null)
                        await toast.Show(cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
                //var contentView = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.FindViewById(Android.Resource.Id.Content);
                //var snackbar = Snackbar.Make(contentView, Message, BaseTransientBottomBar.LengthShort);
                //var snackbarView = snackbar.View;
                //snackbarView.TextAlignment = Android.Views.TextAlignment.Center;
                //snackbar.Show();
                ////var toast = Toast.MakeText(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, Message, ToastLength.Short);
                ////toast.View?.SetBackgroundColor(Android.Graphics.Color.DarkGray);
                ////toast.Show();
                //AndHUD.Shared.ShowToast(activity, Message, MaskType.None, Duration != null ? Duration.Value : new TimeSpan(0, 0, 3), false);
            });

            return this.currentToast;
		}

        public void DisplaySuccess(string message)
		{
            AndHUD.Shared.ShowSuccess(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, message);
		}

		public void DisplayError(string message)
		{
            AndHUD.Shared.ShowError(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, message);
		}

		public void Dismiss()
		{
            AndHUD.Shared.Dismiss();
		}

        public double GetSafeareaHeight()
        {
            return 0;
        }
        
        public bool HasSafeArea()
		{
			return false;
		}

        public IDisposable DisplayActionToast(string Message, Color BackgroundColor = null, Color MessageTextColor = null, TimeSpan? Duration = null, Action action = null)
        {
            this.currentToast?.Dispose();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                //AndHUD.Shared.ShowToast(activity, Message, MaskType.None, Duration != null ? Duration.Value : new TimeSpan(0, 0, 3), false);
                //var contentView = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.FindViewById(Android.Resource.Id.Content);
                //var snackbar = Snackbar.Make(contentView, Message, BaseTransientBottomBar.LengthShort);
                //var snackbarView = snackbar.View;
                //snackbarView.TextAlignment = Android.Views.TextAlignment.Center;
                //snackbar.Show();
                try
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    ToastDuration duration = ToastDuration.Short;
                    double fontSize = 14;
                    var toast = Toast.Make(Message, duration, fontSize);
                    await toast.Show(cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    ex.Track();
                }

                //var toast = Toast.MakeText(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, Message, ToastLength.Short);
                //toast.View?.SetBackgroundColor(Android.Graphics.Color.DarkGray);
                //toast.Show();

            });

            return this.currentToast;
        }
    }
}
