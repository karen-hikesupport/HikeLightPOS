using Microsoft.Extensions.Logging;
using HikePOS.Interfaces;
using HikePOS.Services;
using ZXing.Net.Maui.Controls;
using CommunityToolkit.Maui;
using HikePOS.UserControls;
using HikePOS.Helpers;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Microsoft.Maui.Handlers;
using FFImageLoading.Maui;
using BarcodeScanning;
using HikePOS.Droid.DependencyServices;


#if ANDROID
using HikePOS.Droid.Renderers;
#elif IOS
using HikePOS.iOS.Renderers;
#endif

namespace HikePOS;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseFFImageLoading()
            .UseMauiCommunityToolkit()
            .UseMauiCompatibility()
            .UseBarcodeReader()
            .UseBarcodeScanning()
            .UseSentry(options =>
            {
               // The DSN is the only required setting.
                #if ANDROID
                    //options.Dsn = "https://7b96a54350b6a1a379e64e490e0da7bd@o4509037939589120.ingest.us.sentry.io/4509053891313664";       
                    options.Dsn = "https://07f50b86b8b24dcf9c5244b93812451b@o4508567046389760.ingest.us.sentry.io/4509982709841920";
                // #elif IOS
                //     options.Dsn = "https://8e86c95a13e2ba4c416d7c1f7335c4a8@o4509037939589120.ingest.us.sentry.io/4509037994442752";
                #endif

               // Use debug mode if you want to see what the SDK is doing.
               // Debug messages are written to stdout with Console.Writeline,
               // and are viewable in your IDE's debug console or with 'adb logcat', etc.
               // This option is not recommended when deploying your application.
            //    options.Debug = true;
            //     // Attach screenshots on errors
            //    options.AttachScreenshot = true;
            })
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Foco-Bold.ttf", "HikeBoldFont");
                fonts.AddFont("Foco-Light.ttf", "HikeLightFont");
				fonts.AddFont("Foco-Regular.ttf", "HikeDefaultFont");
                fonts.AddFont("fontawesome-webfont.ttf", "HikeAwesomeFont");
#if ANDROID
                fonts.AddFont("arial_bold_mt.ttf", "Arial-BoldMT");
				fonts.AddFont("arial_mt.ttf", "ArialMT");
#endif
            })
            .ConfigureMauiHandlers((handlers) =>
            {
#if ANDROID
                handlers.AddCompatibilityRenderer(typeof(CustomDatePicker), typeof(CustomDatePickerRenderer));
                handlers.AddHandler(typeof(CustomEditor), typeof(CustomEditorHandler));
                handlers.AddHandler(typeof(BorderLessEntry), typeof(BorderLessEntryHandler));
                handlers.AddHandler(typeof(CustomEntry), typeof(CustomEntryHandler));
                handlers.AddCompatibilityRenderer(typeof(CustomHtmlLabel), typeof(CustomHtmlLabelRenderer));
                handlers.AddHandler(typeof(CustomPicker), typeof(PickerWithFocusHandler));
                handlers.AddCompatibilityRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer));
                handlers.AddHandler(typeof(BackspaceEntry), typeof(BackspaceEntryHandler));
#elif IOS
                handlers.AddCompatibilityRenderer(typeof(AdminWebView), typeof(AdminWebViewRender));
                handlers.AddCompatibilityRenderer(typeof(CustomDatePicker), typeof(CustomDatePickerRenderer));
                handlers.AddHandler(typeof(CustomEditor), typeof(CustomEditorHandler));
                handlers.AddHandler(typeof(CustomEntry), typeof(CustomEntryHandler));
                handlers.AddHandler(typeof(CustomPaymentWebView), typeof(CustomPaymentWebViewHandler));
                handlers.AddHandler(typeof(CustomPicker), typeof(PickerWithFocusHandler));
                handlers.AddCompatibilityRenderer(typeof(CustomWebView), typeof(CustomWebviewRenderer));
                handlers.AddCompatibilityRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer));
                handlers.AddCompatibilityRenderer(typeof(PaymentWebView), typeof(PaymentWebViewRender));
                if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
                {
                   handlers.AddHandler(typeof(MainPopupBasePage), typeof(PopupPageHandler));
                }
                handlers.AddCompatibilityRenderer(typeof(TyroWebView), typeof(TyroWebViewRenderer));

                PageHandler.Mapper.ReplaceMapping<ContentPage, IPageHandler>(nameof(ContentPage.HideSoftInputOnTapped), HideSoftInputOnTapHandlerMappings.MapHideSoftInputOnTapped);
                PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(VisualElement.IsFocused), HideSoftInputOnTapHandlerMappings.MapInputIsFocused);

                HikePOS.Handlers.FormHandler.RemoveBorders();
                HikePOS.Handlers.FormHandler.CommanHandlers();
#endif

            }).ConfigureEffects(effects =>
            {
#if ANDROID
                //effects.Add<UnderlineEffect, PlatformUnderlineEffect>();

#elif IOS
                effects.Add<UnderlineEffect, PlatformUnderlineEffect>();
#endif
            });
        ;


        // builder.UseBarcodeReader();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.RegisterAppServices();
        var app = builder.Build();
        ServiceLocator.Provider = app.Services;

        return app;
	}

    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<INavigationService, NavigationService>();
        mauiAppBuilder.Services.AddSingleton<IAlertService, AlertService>();
        return mauiAppBuilder;
    }

}

