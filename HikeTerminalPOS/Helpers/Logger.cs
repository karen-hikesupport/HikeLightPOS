using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace HikePOS.Helpers
{
    //Start #61832  iPad:Create text file for invoice log .Pratik
    public static class Logger
	{
		public static void SaleLogger(string info)
		{

            try
            {

                string filepath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "salelog");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                DeleteLogFile(filepath);
                filepath = filepath + "/" + DateTime.Today.ToString("dd-MM-yyyy") + ".txt";
                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    string msg = info.Contains("----") ? Environment.NewLine : "";
                    msg = DateTime.Now.ToString() + ": " + info;
                    sw.WriteLine(msg);
                    sw.Flush();
                    sw.Close();
                }

            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        public static void DeleteLogFile(string filepath)
        {
            var filesList = Directory.GetFiles(filepath);
            if (filesList != null && filesList.Count() > 0)
            {
                foreach (var file in filesList)
                {
                    var filename = Path.GetFileName(file);
                    var strfile = filename.Split('.');
                    var datedata = strfile[0].Split('-');
                    if (datedata.Count() > 2)
                    {
                        var filedate = new DateTime(Convert.ToInt32(datedata[2]),
                            Convert.ToInt32(datedata[1]),
                            Convert.ToInt32(datedata[0]));
                            
                        if (filedate < DateTime.Now.AddDays(-30))
                        {
                            File.Delete(Path.GetFullPath(file));
                        }
                    }
                }
            }
        }

        public static async Task CompressAndExportFolder(string folderPath, Microsoft.Maui.Controls.Shapes.Rectangle bound, string folderstartwith)
        {
            // Get a temporary cache directory
            var exportZipTempDirectory = Path.Combine(FileSystem.CacheDirectory, "Export");

            // Delete folder incase anything from previous exports, it will be recreated later anyway
            try
            {
                if (Directory.Exists(exportZipTempDirectory))
                {
                    Directory.Delete(exportZipTempDirectory, true);
                }
            }
            catch (Exception ex)
            {
                // Log things and move on, don't want to fail just because of a left over lock or something
                Debug.WriteLine(ex.Message);
            }

            // Get a timestamped filename
            var exportZipFilename = folderstartwith;
            Directory.CreateDirectory(exportZipTempDirectory);

            var exportZipFilePath = Path.Combine(exportZipTempDirectory, exportZipFilename);
            if (File.Exists(exportZipFilePath))
            {
                File.Delete(exportZipFilePath);
            }

            // Zip everything up
            ZipFile.CreateFromDirectory(folderPath, exportZipFilePath, CompressionLevel.Fastest, true);
            
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = LanguageExtension.Localize("ExportLogLabelText"),
                File = new ShareFile(exportZipFilePath),
                PresentationSourceBounds = bound.ToSystemRectangle()
            });

        }

        public static Stream CompressAndgetSteam(string folderPath, string folderstartwith)
        {
            // Get a temporary cache directory
            var exportZipTempDirectory = Path.Combine(FileSystem.CacheDirectory, "Export");

            // Delete folder incase anything from previous exports, it will be recreated later anyway
            try
            {
                if (Directory.Exists(exportZipTempDirectory))
                {
                    Directory.Delete(exportZipTempDirectory, true);
                }
            }
            catch (Exception ex)
            {
                // Log things and move on, don't want to fail just because of a left over lock or something
                Debug.WriteLine(ex.Message);
            }

            // Get a timestamped filename
            var exportZipFilename = folderstartwith;
            Directory.CreateDirectory(exportZipTempDirectory);

            var exportZipFilePath = Path.Combine(exportZipTempDirectory, exportZipFilename);
            if (File.Exists(exportZipFilePath))
            {
                File.Delete(exportZipFilePath);
            }

            // Zip everything up
            ZipFile.CreateFromDirectory(folderPath, exportZipFilePath, CompressionLevel.Fastest, true);

             FileStream fs = new FileStream(exportZipFilePath, FileMode.Open, FileAccess.Read);

             StreamReader r = new StreamReader(fs);
            return r.BaseStream;
        }

        public static void SyncLogger(string info)
        {

            try
            {

                string filepath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "salelog");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                DeleteLogFile(filepath);
                filepath = filepath + "/" + DateTime.Today.ToString("dd-MM-yyyy") + "-Sync.txt";
                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    string msg = info.Contains("----") ? Environment.NewLine : "";
                    msg = DateTime.Now.ToString() + ": " + info;
                    sw.WriteLine(msg);
                    sw.Flush();
                    sw.Close();
                }

            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        public static void ExceptionLogger(Exception exception)
        {
            try
            {

                string filepath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "salelog");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                DeleteLogFile(filepath);
                filepath = filepath + "/" + DateTime.Today.ToString("dd-MM-yyyy") + "-exception.txt";
                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    string msg = Environment.NewLine + "----------------------";
                    msg += exception.StackTrace;
                    msg += Environment.NewLine + "----------------------";
                    sw.WriteLine(msg);
                    sw.Flush();
                    sw.Close();
                }

            }
            catch (Exception e)
            {
                e.ToString();
            }
        }
    }

    //END #61832 Pratik

}

