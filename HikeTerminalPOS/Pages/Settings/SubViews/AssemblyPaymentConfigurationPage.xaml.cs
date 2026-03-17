using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HikePOS.UserControls;
using System.Threading.Tasks;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class AssemblyPaymentConfigurationPage : PopupBasePage<AssemblyPaymentConfigurationViewModel>
    {
        public AssemblyPaymentConfigurationPage()
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
