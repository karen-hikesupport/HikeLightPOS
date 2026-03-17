using System;
namespace HikePOS.Services
{
    public interface IImageServices
    {
        void ClearCaches();

        void ClearWKWebsiteCaches();
    }
}
