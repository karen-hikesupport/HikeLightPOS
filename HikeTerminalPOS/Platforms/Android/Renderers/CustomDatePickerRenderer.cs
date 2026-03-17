using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HikePOS.Droid.Renderers;
using HikePOS.UserControls;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility;
using DatePicker = Microsoft.Maui.Controls.DatePicker;
using Microsoft.Maui.Controls;

namespace HikePOS.Droid.Renderers
{
    class CustomDatePickerRenderer : DatePickerRenderer
    {
        Context context;
        public CustomDatePickerRenderer(Context context) : base(context)
        {
            this.context = context;
        }
        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {

            try
            {
                base.OnElementChanged(e);

                if (this.Control == null || e == null) return;

                var hikeDatePicker = e.NewElement as CustomDatePicker;

                if (hikeDatePicker != null)
                {
                    SetBorder(hikeDatePicker);
                   // SetFont(hikeDatePicker);
                    Control.SetPadding(14, 0, 0, 0);
                    this.SetValue(hikeDatePicker);
                    SetTextColor(hikeDatePicker);

                }
            }
            catch (Exception ex)
            {
                ex.Track();
                System.Diagnostics.Debug.WriteLine("Exception in OnElementChanged : " + ex.Message + " : " + ex.StackTrace);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (this.Control == null || e == null || this.Element == null) return;

            var hikeDatePicker = this.Element as CustomDatePicker;

            if (hikeDatePicker != null)
            {
                switch (e.PropertyName)
                {
                    case nameof(CustomDatePicker.NullableDate):
                        this.SetValue(hikeDatePicker);
                        break;
                    case nameof(CustomDatePicker.TextColor):
                        this.SetTextColor(hikeDatePicker);
                        break;
                    case nameof(CustomDatePicker.FontSize):
                        this.SetFont(hikeDatePicker);
                        break;
                    case nameof(CustomDatePicker.FontFamily):
                        this.SetFont(hikeDatePicker);
                        break;
                    case nameof(CustomDatePicker.DisplayBorder):
                        this.SetBorder(hikeDatePicker);
                        break;
                }

            }
        }

        private void SetValue(CustomDatePicker hikeDatePicker)
        {
            if (hikeDatePicker != null && hikeDatePicker.AllowNullDate)
            {
                if (hikeDatePicker.NullableDate.HasValue)
                {
                    this.Control.Text = hikeDatePicker.NullableDate.Value.ToString(hikeDatePicker.Format);
                    hikeDatePicker.Date = hikeDatePicker.NullableDate.Value;
                }
                else
                {
                    this.Control.Text = string.Empty;
                }
            }
            else
            {
                this.Control.Text = hikeDatePicker.Date.ToString(hikeDatePicker.Format);
            }
        }

        void SetFont(CustomDatePicker view)
        {
            try
            {
                var fontFamily = view.FontFamily;
                if (fontFamily != null)
                {
                    System.Diagnostics.Debug.WriteLine("Before fontFamily in SetFont of CustomDatePickerRenderer : " + fontFamily);
                    fontFamily = fontFamily.Split("#")[0];
                    System.Diagnostics.Debug.WriteLine("After fontFamily in SetFont of CustomDatePickerRenderer : " + fontFamily);
                    Typeface font = Typeface.CreateFromAsset(context.Assets, fontFamily);
                    if (font != null)
                    {
                        Control.Typeface = font;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                System.Diagnostics.Debug.WriteLine("Exception in SetFont of CustomDatePickerRenderer : " + ex.Message + " : " + ex.StackTrace);
            }
        }

        void SetTextColor(CustomDatePicker view)
        {
            try
            {
                if (view != null && Control != null)
                {
                    Control.SetTextColor(view.TextColor.ToAndroid());
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                System.Diagnostics.Debug.WriteLine("Exception in SetTextColor : " + ex.Message + " : " + ex.StackTrace);
            }
        }

        void SetBorder(CustomDatePicker view)
        {
            try
            {
                if (view != null && Control != null)
                {
                    if (!view.DisplayBorder)
                    {
                        Control.SetBackground(null);
                    }
                    else
                    {
                        //Control.BorderStyle = UIKit.UITextBorderStyle.RoundedRect;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

       
    }
}