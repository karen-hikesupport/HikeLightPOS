using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using HikePOS.Droid.Renderers;
using HikePOS.UserControls;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AndroidX.AppCompat.Widget;
using Android.Graphics;

namespace HikePOS.Droid.Renderers
{
    public partial class CustomEditorHandler : EditorHandler
    {
        protected override void ConnectHandler(AppCompatEditText platformView)
        {
            base.ConnectHandler(platformView);
            if (platformView != null)
            {
                try
                {
                    platformView.SetBackground(null);
                    platformView.SetPadding(5, 14, 0, 0);
                    platformView.SetTextColor(Android.Graphics.Color.Black);
                }
                catch (Exception exc)
                {
                    exc.Track();
                }

            }

        }

        //public static void UpdateMaxLength(CustomEditorHandler handler, CustomEditor view)
        //{
        //    if (handler?.PlatformView == null)
        //        return;

        //    IInputFilter[] FilterArray = new IInputFilter[1];
        //    FilterArray[0] = new InputFilterLengthFilter(view.MaxLength);
        //    handler.PlatformView.SetFilters(FilterArray);
        //}
    }

    //public partial class CustomEditorHandler
    //{
    //    private static readonly PropertyMapper<CustomEditor, CustomEditorHandler> propertyMapper = new PropertyMapper<CustomEditor, CustomEditorHandler>(EditorHandler.ViewMapper)
    //    {
    //        [nameof(CustomEditor.MaxLengthProperty)] = UpdateMaxLength,
    //    };

    //    public CustomEditorHandler() : base(propertyMapper)
    //    {
    //    }
    //}
}