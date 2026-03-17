using Microsoft.Maui.Handlers;
using HikePOS.UserControls;
#if IOS
using UIKit;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
#endif
namespace HikePOS.Handlers;

public partial class CustomHtmlLabelHandler : LabelHandler
{
    // doc: https://alexdunn.org/2017/02/28/xamarin-controls-CustomHtmlLabel/

    static IPropertyMapper<CustomHtmlLabel, CustomHtmlLabelHandler> PropertyMapper = new PropertyMapper<CustomHtmlLabel, CustomHtmlLabelHandler>(Mapper)
    {
        [nameof(CustomHtmlLabel.Text)] = (handler, textView) => MapTextProperty(handler, textView)
    };

    static partial void MapTextProperty(CustomHtmlLabelHandler handler, CustomHtmlLabel textView);

    public CustomHtmlLabelHandler() : base(PropertyMapper)
    {
    }
}

public static class FormHandler
{
    public static void RemoveBorders()
    {
#if IOS
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
        {
            if (handler != null && view is BorderLessEntry)
            {
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
                handler.PlatformView.SpellCheckingType = UITextSpellCheckingType.No;
                if (view.HorizontalTextAlignment != TextAlignment.Center)
                {
                    var paddingView = new UIView(new CoreGraphics.CGRect(0, 0, 14, 0));
                    handler.PlatformView.LeftView = paddingView;
                    handler.PlatformView.LeftViewMode = UITextFieldViewMode.Always;
                }
            }
        });
#endif
    }

    public static void CommanHandlers()
    {
        Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("MyCustomizationButton", (handler, view) =>
        {

#if IOS
            handler.PlatformView.TitleLabel.TextAlignment = UIKit.UITextAlignment.Center;
            handler.PlatformView.TitleLabel.LineBreakMode = UIKit.UILineBreakMode.WordWrap;
#endif
        });
    }
}
