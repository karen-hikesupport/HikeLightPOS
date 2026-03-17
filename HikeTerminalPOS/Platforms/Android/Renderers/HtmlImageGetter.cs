using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using HikePOS.UserControls;
using Java.IO;
using Java.Net;
using Org.Apache.Http;
using Org.Apache.Http.Client.Methods;
using Org.Apache.Http.Impl.Client;

namespace HikePOS.Droid.Renderers
{
    public class HtmlImageGetter : Java.Lang.Object, Html.IImageGetter
    {
        Context context;
        TextView textView;
        CustomHtmlLabel htmlLabel;
        public Dictionary<string, double[]> imgSizes { get; set; }

        public HtmlImageGetter(Context context, TextView textView, CustomHtmlLabel htmlLabel)
        {
            this.context = context;
            this.textView = textView;
            this.htmlLabel = htmlLabel;
        }

        public Drawable GetDrawable(string source)
        {
            LevelListDrawable mDrawable = new LevelListDrawable();

            //htmlLabel.IsLoaded = false;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
                new ImageGetterAsyncTask(this, source, mDrawable).ExecuteOnExecutor(AsyncTask.ThreadPoolExecutor);
            else
                new ImageGetterAsyncTask(this, source, mDrawable).Execute();

            return mDrawable;
        }


        public class ImageGetterAsyncTask : AsyncTask<Java.Lang.Void, Java.Lang.Void, Bitmap>
        {
            LevelListDrawable mDrawable;
            string source;
            HtmlImageGetter htmlImageGetter;

            public ImageGetterAsyncTask(HtmlImageGetter htmlImageGetter, string source, LevelListDrawable mDrawable)
            {
                if (!source.StartsWith("http://") && !source.StartsWith("https://"))
                    source = "https://" + source;
                this.source = source;
                this.mDrawable = mDrawable;
                this.htmlImageGetter = htmlImageGetter;
            }

            protected override Bitmap RunInBackground(params Java.Lang.Void[] @params)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Url in RunInBackground of HtmlImageGetter : " + source);
                    return BitmapFactory.DecodeStream(new URL(source).OpenStream());
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception in RunInBackground of HtmlImageGetter : " + ex.Message + "\n" + ex.StackTrace);
                }
                return null;
            }

            protected override void OnPostExecute(Bitmap bitmap)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("bitmap In onPostExecute bitmap : " + bitmap);
                    if (bitmap != null)
                    {
                        int width = 100;
                        int height = 100;
                        var sizes = htmlImageGetter.imgSizes;
                        if (sizes != null && sizes.ContainsKey(source))
                        {
                            width = (int)sizes[source][0];
                            height = (int)sizes[source][1];
                        }

                        System.Diagnostics.Debug.WriteLine("In onPostExecute bitmap : " + source + " : " + width + " : " + height);
                        BitmapDrawable d = new BitmapDrawable(bitmap);
                        mDrawable.AddLevel(1, 1, d);
                        mDrawable.SetBounds(0, 0, width, height);
                        mDrawable.SetLevel(1);

                        // i don't know yet a better way to refresh TextView
                        // mTv.invalidate() doesn't work as expected
                        //var text = this.textView.Text;
                        htmlImageGetter.textView.Invalidate();
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception in OnPostExecute of HtmlImageGetter : " + ex.Message + "\n" + ex.StackTrace);
                }
                //htmlImageGetter.htmlLabel.IsLoaded = true;
            }
        }
    }
}

