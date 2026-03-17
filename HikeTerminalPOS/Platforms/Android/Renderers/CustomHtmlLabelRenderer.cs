using System;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility;
using HikePOS.UserControls;
using System.ComponentModel;
using HikePOS.Helpers;
using System.Drawing;
using HikePOS.Droid.Renderers;
using Android.Content;
using Android.Text;
using Android.Widget;
using Android.Graphics.Drawables;
using System.Collections.Generic;
//using HtmlAgilityPack;
using System.Linq;


namespace HikePOS.Droid.Renderers
{
    public class CustomHtmlLabelRenderer : LabelRenderer
    {
        Context context; 
        public CustomHtmlLabelRenderer(Context context) : base(context)
        {
            this.context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            SetText(false); 
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Label.TextProperty.PropertyName)
            {
                SetText(true);
            }
        }

        HtmlImageGetter htmlImageGetter = null;
        void SetText(bool isTextChanged)
        {
            if (Control != null && Element != null && Element.Text != null)
            {
                var html = Html.FromHtml(GetHtmlData(Element), FromHtmlOptions.ModeLegacy, htmlImageGetter, null);
                Control.SetText(html, TextView.BufferType.Spannable);
            }
        }

        static string GetHtmlData(Label lbl)
        {
            //Ticket start:#30306 iOS: Receipt printed for ipad does not print the Footer text correctly.by rupesh
            string htmlData = "";
            if (lbl.HorizontalOptions.Alignment == LayoutAlignment.Start && !lbl.HorizontalOptions.Expands)
                htmlData = "<span style=\"font-size:" + lbl.FontSize + "\">" + lbl.Text + "</span>";
            else
                htmlData = "<span style=\"font-size:" + lbl.FontSize + ";text-align:center\">" + lbl.Text + "</span>";
            //Ticket end:#30306.by rupesh
            return htmlData;
        }

    }
}
