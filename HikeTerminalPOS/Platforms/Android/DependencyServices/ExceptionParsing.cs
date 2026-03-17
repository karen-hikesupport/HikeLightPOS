using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HikePOS.Droid.DependencyServices;
using HikePOS.Interfaces;
using HikePOS.Models.Common;

[assembly: Dependency(typeof(ExceptionParsing))]
namespace HikePOS.Droid.DependencyServices
{
    class ExceptionParsing : IExceptionParsing
    {
        public ExceptionParsing()
        {
        }


        public ExceptionDetail GetDetail(Exception ex)
        {
            try
            {
                if (ex != null)
                {
                    ExceptionDetail exceptionDetail = new ExceptionDetail();
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }

                    var st = new StackTrace(ex, true);
                    // Get the top stack frame
                    var frame = st.GetFrame(0);

                    exceptionDetail.FileName = frame.GetFileName();
                    exceptionDetail.LineNumber = frame.GetFileLineNumber().ToString();
                    //exceptionDetail.ColumnNumber = frame.GetFileColumnNumber();
                    exceptionDetail.Method = frame.GetMethod().ToString();
                    exceptionDetail.Class = frame.GetMethod().DeclaringType.ToString();

                    return exceptionDetail;

                    //var st = new System.Diagnostics.StackTrace(ex, true); // create the stack trace
                    //var query = st.GetFrames()         // get the frames
                    //.Select(frame => new
                    //{                   // get the info
                    //    FileName = frame.GetFileName(),
                    //    LineNumber = frame.GetFileLineNumber(),
                    //    ColumnNumber = frame.GetFileColumnNumber(),
                    //    Method = frame.GetMethod(),
                    //    Class = frame.GetMethod().DeclaringType,
                    //});
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}