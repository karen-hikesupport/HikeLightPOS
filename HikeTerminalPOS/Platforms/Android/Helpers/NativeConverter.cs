using System;
using Platform = Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform;


namespace HikePOS.Droid.Helpers
{
	public class NativeConverter
	{
        public static Android.Views.View ConvertFormsToNative(View view, Rect size)
        {
            try
            {
                //var vRenderer = Platform.GetRenderer(view);
                //if(vRenderer == null || vRenderer?.View == null)
                //   vRenderer = Platform.CreateRendererWithContext(view,MainActivity.activity);
                var viewGroup = view.Handler.PlatformView as Android.Views.ViewGroup;
                if(viewGroup.ChildCount > 0)
                   return viewGroup.GetChildAt(0) ;
               // var layoutParams = new Android.Views.ViewGroup.LayoutParams((int)size.Width, (int)size.Height); 
               // viewGroup.LayoutParameters = layoutParams;
              //  view.Layout(size);
              //  viewGroup.Layout(0, 0, (int)view.WidthRequest, (int)view.HeightRequest);
                //Ticket start:#21151 Android - Customer Queue Docket Not Printed Properly - Star TSP100 by rupesh
               // vRenderer.Tracker.UpdateLayout();
                //Ticket end:#21151 by rupesh
                return viewGroup;

            }
            catch (Exception ex)
            {
                //  ex.Track(); 
                return null;
            }
            
        }
    }
}
