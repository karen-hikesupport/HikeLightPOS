using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HikePOS.Droid.Renderers;
using HikePOS.UserControls;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Polly;

namespace HikePOS.Droid.Renderers
{
    public partial class PickerWithFocusHandler : PickerHandler
    {

        protected override void ConnectHandler(MauiPicker platformView)
        {
            base.ConnectHandler(platformView);
            if (platformView == null)
                return;
            try
            {
                platformView.SetBackground(null);
                platformView.SetPadding(14, 0, 20, 0);

            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
    }
}