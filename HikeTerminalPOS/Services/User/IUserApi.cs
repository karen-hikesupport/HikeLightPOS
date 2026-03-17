using System.Threading.Tasks;
using Refit;
using HikePOS.Models;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface IUserApi
	{
		[Post("/api/services/app/user/GetAll")]
		Task<ListResponseModel<UserListDto>> GetAll([Header("Authorization")] string accessToken);

		[Post("/api/services/app/role/GetRoles")]
		Task<ListResponseModel<RoleListDto>> GetRoles([Header("Authorization")] string accessToken, [Body] RoleRequestModel roleRequest);
	
		[Post("/api/services/app/user/GetByEmail")]
		Task<ResponseModel<UserListDto>> GetByEmail([Header("Authorization")] string accessToken, [AliasAs("Email")] string Email);


        [Post("/api/services/app/user/GetUserByUserNameOrEmail")]
		Task<ResponseModel<UserListDto>> GetUserByUserNameOrEmail([Header("Authorization")] string accessToken, [AliasAs("name")] string UserNameOrEmail);

        [Post("/api/services/app/userClockActivity/CreateOrUpdate")]
        Task<ResponseListModel<UserClockActivityDto>> UpdateUserClockActivity([Header("Authorization")] string accessToken, [Body] UserClockActivityInputDto userClockActivity);

        [Post("/api/services/app/userClockActivity/GetUserActivities")]
        Task<ResponseListModel<UserClockActivityDto>> GetClockUserActivities([Header("Authorization")] string accessToken);

	}
}
