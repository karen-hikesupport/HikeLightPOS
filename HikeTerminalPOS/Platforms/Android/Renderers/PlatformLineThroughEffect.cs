using System;
using Android.Widget;
using HikePOS.Droid.Renderers;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility;

namespace HikePOS.Droid.Renderers
{
    public class PlatformLineThroughEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            SetUnderline(true); 
        }

        protected override void OnDetached()
        {
            SetUnderline(false);
        }

        protected override void OnElementPropertyChanged(System.ComponentModel.PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);

            if (args.PropertyName == Label.TextProperty.PropertyName ||
                args.PropertyName == Label.FormattedTextProperty.PropertyName)
            {
                SetUnderline(true);
            }
        }

        private void SetUnderline(bool underlined)
        {
            try
            {
                var label = (TextView)Control;
                if (label != null)
                {
                    var text = label.Text;
                    if (text != null)
                    {
                        var paintFlags = label.PaintFlags;
                        if (underlined)
                        {
                            paintFlags |= Android.Graphics.PaintFlags.StrikeThruText;
                        }
                        else
                        {
                            paintFlags -= Android.Graphics.PaintFlags.StrikeThruText;
                        }
                        label.PaintFlags = paintFlags;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                System.Diagnostics.Debug.WriteLine("Exception in SetUnderline of PlatformLineThroughEffect : " + ex.Message + " : " + ex.StackTrace);
            }
        }
    }
}
