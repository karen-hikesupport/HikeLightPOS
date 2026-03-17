using System;
using HikePOS.Models.Common;

namespace HikePOS.Interfaces
{
    public interface IExceptionParsing
    {
        ExceptionDetail GetDetail(Exception ex);
    }
}
