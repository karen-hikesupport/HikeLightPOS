using System;
using HikePOS.Enums;
using System.Collections.ObjectModel;
using System.Linq;

namespace HikePOS.Models
{
    public class SubmitLogDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
    }
}
