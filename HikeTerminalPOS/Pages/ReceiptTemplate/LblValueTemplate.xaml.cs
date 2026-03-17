using System;
using System.Collections.Generic;

namespace HikePOS
{
    public partial class LblValueTemplate : Grid
    {
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(
    nameof(Label), typeof(string), typeof(LblValueTemplate), "");

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly BindableProperty ValueProperty = BindableProperty.Create(
   nameof(Value), typeof(string), typeof(LblValueTemplate), "");

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly BindableProperty ViewHeightProperty = BindableProperty.Create(
 nameof(ViewHeight), typeof(int), typeof(LblValueTemplate), defHeight, BindingMode.TwoWay);

        public int ViewHeight
        {
            get { return (int)GetValue(ViewHeightProperty); }
            set { SetValue(ViewHeightProperty, value);}
        }

        static int defHeight = 40;
        public LblValueTemplate()
        {
            InitializeComponent();
            if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                value.IsVisible = false;
                value1.IsVisible = true;
            }
            else
            {
                value.IsVisible = true;
                value1.IsVisible = false;
            }
            Microsoft.Maui.Handlers.LabelHandler.Mapper.AppendToMapping("MyCustomizationLabel", (handler, view) =>
            {
                if (handler != null && view is HikePOS.UserControls.AutoFitLabel)
                {
#if IOS
                    handler.PlatformView.AdjustsFontSizeToFitWidth = true;
#endif

                }
            });
        }

        public void UpdateHeight()
        {


            int labelLength = Label == null ? 0 : Label.Length;
            int valueLength = Value == null ? 0 : Value.Length;
            //Ticket #9753 Start : Register Summary print amount cut issue. By Nikhil
            //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
            //  ViewHeight = (labelLength > 12 || valueLength > 12) ? (2 * defHeight) : defHeight;
            //Ticket end: #62808 .by rupesh
            //Ticket #9753 End. By Nikhil
        }
    }
}
