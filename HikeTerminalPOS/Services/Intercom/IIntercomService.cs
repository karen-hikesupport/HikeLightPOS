using System;
namespace HikePOS.Services
{
    public interface IIntercomService
    {
        void LoginUser();
        void OpenMessenger();
        void CloseMessenger();
    }
}
