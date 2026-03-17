using System;
namespace HikePOS.Models.Common
{
    public class ExceptionDetail
    {
        public string FileName { get; set; }
        public string LineNumber { get; set; }
        //public string ColumnNumber { get; set; }
        public string Method { get; set; }
        public string Class { get; set; }
    }
}
