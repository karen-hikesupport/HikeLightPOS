using System;
using Android.App;
using Android.Content;
using HikePOS;
using HikePOS.Droid.Renderers;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility;

namespace HikePOS.Droid.Renderers
{
    public class PopupPageRenderer : PageRenderer
    {
        Context context;

        public PopupPageRenderer(Context context) : base(context)
        {
            this.context = context;
            var activity = this.Context as Activity;
            

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

        }
    }

}