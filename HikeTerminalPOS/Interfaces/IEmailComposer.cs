using System;
using System.Collections.Generic;

namespace HikePOS.Interfaces
{
    public interface IEmailComposer
    {
        void SendEmail(List<string> toAddresses, string subject, string body, string file_content);
    }
}
