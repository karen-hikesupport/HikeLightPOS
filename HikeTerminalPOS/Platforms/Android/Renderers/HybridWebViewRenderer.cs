using System;
using System.Collections.Generic;
using System.Net.Http;
using Android.Content;
using Android.Webkit;
using HikePOS;
using HikePOS.Droid.Renderers;
using HikePOS.Helpers;
using Android.Graphics;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility;

namespace HikePOS.Droid.Renderers
{
    class HybridWebViewRenderer : ViewRenderer<HybridWebView, Android.Webkit.WebView>
    {
        Context context;

        public HybridWebViewRenderer(Context context) : base(context)
        {
            this.context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            try
            {
                base.OnElementChanged(e);
                if (Control == null)
                {
                    var webView = new Android.Webkit.WebView(context);
                    webView.Settings.JavaScriptEnabled = true;
                    webView.Settings.DomStorageEnabled = true;
                    webView.SetWebViewClient(new JavascriptWebViewClient(Element));
                    //Ticket start:12459  Android - Can't Upload Images .by rupesh
                    // chrome client to upload files from webview
                    var chrome = new FileChooserWebChromeClient((uploadMsg, acceptType, capture) => {
                        MainActivity.UploadMessage = uploadMsg;
                        var i = new Intent(Intent.ActionGetContent);
                        i.AddCategory(Intent.CategoryOpenable);
                        i.SetType("image/*");
                        MainActivity.activity.StartActivityForResult(Intent.CreateChooser(i, "File Chooser"), MainActivity.FILECHOOSER_RESULTCODE);
                    });
                    webView.SetWebChromeClient(chrome);
                    //Ticket end:12459 by rupesh
                    SetNativeControl(webView);
                }
                if (e.OldElement != null)
                {
                    var hybridWebView = e.OldElement as HybridWebView;
                    hybridWebView.UpdateWebUrl -= HybridWebView_UpdateWebUrl;
                }
                if (e.NewElement != null)
                {
                    var hybridWebView = e.NewElement as HybridWebView;
                    hybridWebView.UpdateWebUrl += HybridWebView_UpdateWebUrl;
                    /*loadUrl("https://ipadmay.Hikeup.com/account/SwitchToIPadUserAccount?TargetTenantId=13619&TargetUserId=1");*/
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                System.Diagnostics.Debug.WriteLine("Exception in OnElementChanged : " + ex.Message + " : " + ex.StackTrace);
            }
        }

        void HybridWebView_UpdateWebUrl(object sender, string e)
        {
            loadUrl(e);
        }

        public void loadUrl(string Uri)
        {
            try
            {
                if (!string.IsNullOrEmpty(Uri))
                {
                    Control.LoadUrl(Uri);

                    if (Control != null)
                    {
                        if (Element != null && Element is HybridWebView)
                        {
                            ((HybridWebView)Element).WebNavigating?.Invoke(this, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
                System.Diagnostics.Debug.WriteLine("Exception in loadUrl : " + ex.Message + " : " + ex.StackTrace);
            }
        }

        public class JavascriptWebViewClient : WebViewClient
        {
            Dictionary<string, string> Headers; 
            HybridWebView Element;

            public JavascriptWebViewClient(HybridWebView Element)
            { 
                this.Element = Element;
                Headers = new Dictionary<string, string>();
                Headers.Add("Authorization", HikePOS.Helpers.Settings.AccessToken);
            }

            public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, IWebResourceRequest request)
            {
                view.LoadUrl(request.Url.ToString(), Headers);
                return true;
            }

            public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
                if (Element != null && Element is HybridWebView)
                {
                    ((HybridWebView)Element).WebNavigating?.Invoke(this, true);
                }
            }

            public override void OnPageFinished(Android.Webkit.WebView view, string url)
            {
                try
                {
                    base.OnPageFinished(view, url); 
                        if (Element != null && Element is HybridWebView)
                        {
                            ((HybridWebView)Element).WebNavigating?.Invoke(this, false);
                        } 
                }
                catch (Exception ex)
                {
                    ex.Track();
                    System.Diagnostics.Debug.WriteLine("Exception in OnPageFinished : " + ex.Message + " : " + ex.StackTrace);
                }
            }
        }


        public class FileChooserWebChromeClient : WebChromeClient
        {
            private Action<IValueCallback, Java.Lang.String, Java.Lang.String> callback;

            public FileChooserWebChromeClient(Action<IValueCallback, Java.Lang.String, Java.Lang.String> callBack)
            {
                callback = callBack;
            }

            // For Android < 5.0
            [Java.Interop.Export]
            public void openFileChooser(IValueCallback uploadMsg, Java.Lang.String acceptType, Java.Lang.String capture)
            {
                callback(uploadMsg, acceptType, capture);
            }

            // For Android > 5.0
            public override Boolean OnShowFileChooser(Android.Webkit.WebView webView, IValueCallback uploadMsg, WebChromeClient.FileChooserParams fileChooserParams)
            {
                try
                {
                    callback(uploadMsg, null, null);
                }
                catch (Exception ex)
                {
                    var j = ex;
                }
                return true;
            }
        }
    }

}
