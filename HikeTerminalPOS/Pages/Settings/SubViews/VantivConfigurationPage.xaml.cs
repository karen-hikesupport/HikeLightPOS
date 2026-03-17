using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.UserControls;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class VantivConfigurationPage : PopupBasePage<VantivConfigurationViewModel>
    {
        public VantivConfigurationPage()
        {
            InitializeComponent();
            Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping("MyCustomizationSwitch", (handler, view) =>
            {
                if (handler != null && view is CustomSwitch)
                {
#if IOS
                handler.PlatformView.OnTintColor =  UIKit.UIColor.FromRGB(50, 189, 185);
#endif
                }
            });
        }
    }
}
