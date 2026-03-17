using System.Threading.Tasks;
using HikePOS.Models;
using Refit;

namespace HikePOS.Services
{
    [Headers("Accept: application/json")]
    public interface ISubmitLogApi
    {
        [Post("/api/services/app/webLog/AddFileLog")]
        Task<AjaxResponse> SubmitLogs([Body] SubmitLogDto input, [Header("Authorization")] string accessToken);
    }
}
