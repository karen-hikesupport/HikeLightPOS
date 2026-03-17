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

namespace HikePOS.Droid.Renderers
{

    public partial class CustomEntryHandler : EntryHandler
    {
        CustomEntry View = null;
        AppCompatEditText Control = null;


        public override void SetVirtualView(IView view)
        {
            base.SetVirtualView(view);
            View = (CustomEntry)view;
            if (View != null && Control != null)
            {
                Control.SetBackground(null);
                if (!View.IsPassword)
                    Control.InputType = InputTypes.TextFlagNoSuggestions;

                try
                {
                    if (View.HorizontalTextAlignment != Microsoft.Maui.TextAlignment.Center)
                    {
                        Control.SetPadding(14, 0, 5, 0);
                    }
                    Control.SetTextColor(Android.Graphics.Color.Black);
                    SetTextAlignment(View);

                }
                catch (Exception exc)
                {
                    exc.Track();
                }

            }

        }

        protected override void ConnectHandler(AppCompatEditText platformView)
        {
            base.ConnectHandler(platformView);
            Control = platformView;
            if (View != null && Control != null)
            {

                Control.SetBackground(null);
                if (!View.IsPassword)
                    Control.InputType = InputTypes.TextFlagNoSuggestions;

                try
                {
                    if (View.HorizontalTextAlignment != Microsoft.Maui.TextAlignment.Center)
                    {
                        Control.SetPadding(14, 0, 5, 0);
                    }
                    Control.SetTextColor(Android.Graphics.Color.Black);
                    SetTextAlignment(View);

                }
                catch (Exception exc)
                {
                    exc.Track();
                }
            }

        }

        void SetMaxLength(CustomEntry view)
        {
            IInputFilter[] FilterArray = new IInputFilter[1];
            FilterArray[0] = new InputFilterLengthFilter(view.MaxLength);
            Control.SetFilters(FilterArray);

        }

        void SetTextAlignment(CustomEntry view)
        {
            switch (view.XAlign)
            {
                case Microsoft.Maui.TextAlignment.Center:
                    Control.Gravity = GravityFlags.Center;
                    break;
                case Microsoft.Maui.TextAlignment.End:
                    Control.Gravity = GravityFlags.Center | GravityFlags.End;
                    break;
                case Microsoft.Maui.TextAlignment.Start:
                    Control.Gravity = GravityFlags.Center | GravityFlags.Start;
                    break;
            }

        }

        void SetPlaceholderColor(CustomEntry view)
        {
            Control.SetHintTextColor(Android.Graphics.Color.ParseColor("#044361"));
        }

        void SetPhoneNumberEntry(CustomEntry view)
        {
            if (Control != null)
            {
                if (view.IsPhoneNumberEntry == true)
                {
                    if (view.Text.Length == 10)
                    {
                        try
                        {
                            Control.Text = Convert.ToInt64(view.Text).ToString("(###)-###-####");
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }
                }
            }
        }
    }

    public partial class BorderLessEntryHandler : EntryHandler
    {
        protected override void ConnectHandler(AppCompatEditText platformView)
        {
            base.ConnectHandler(platformView);
            if (platformView != null)
            {
                try
                {
                    platformView.SetBackground(null);
                    platformView.SetPadding(14, 0, 5, 0);
                }
                catch (Exception exc)
                {
                    exc.Track();
                }
            }

        }

    }
}