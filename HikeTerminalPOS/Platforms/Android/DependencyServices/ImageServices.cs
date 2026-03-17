using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HikePOS.Droid.DependencyServices;
using HikePOS.Services;

[assembly: Dependency(typeof(ImageServices))]
namespace HikePOS.Droid.DependencyServices
{
    class ImageServices : IImageServices
    {
        public void ClearCaches()
        {
            var documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var cache = System.IO.Path.Combine(documents, ".config", ".isolated-storage", "ImageLoaderCache");
            if (!string.IsNullOrEmpty(cache) && System.IO.Directory.Exists(cache))
            {
                if (System.IO.Directory.GetFiles(cache) != null && System.IO.Directory.GetFiles(cache).Count() > 0)
                {
                    foreach (var file in System.IO.Directory.GetFiles(cache))
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
        }
        public void ClearWKWebsiteCaches()
        {
        }
    }
}