using System.Threading.Tasks;
using Refit;
using HikePOS.Models;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface IProfileApi
	{
		[Post("/api/services/app/profile/UpdateProfilePictureForIpad")]
		Task<ResponseModel<string>> UpdateProfilePicture([Header("Authorization")] string accessToken, [Body] UpdateProfilePictureModel updateProfilePictureRequest);


		[Post("/api/services/app/profile/ChangePassword")]
		Task<AjaxResponse> UpdatePassword([Header("Authorization")] string accessToken, [Body] ChangePasswordModel ChangePasswordRequest);
	}
}
