using System;
using System.Threading.Tasks;
using HikePOS.Models;
using Refit;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface IRestaurantApi
	{
		[Post("/api/services/app/floor/Get")]
		Task<ResponseModel<FloorDto>> GetFloor([Header("Authorization")] string accessToken, [AliasAs("id")] int floorId);

		[Post("/api/services/app/floor/GetFloors")]
		Task<ListResponseModel<FloorDto>> GetFloors([Body] GetFloorInput filterRequest, [Header("Authorization")] string accessToken);


	}

}
