using System;
using System.Threading.Tasks;
using HikePOS.Models;
using Refit;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface IHearAboutApi
	{
		[Post("/api/services/app/hearAbout/GetHearAbouts")]
		Task<ListResponseModel<HearAboutDto>> GetAllHearAbout([Body]PagedSortedAndFilteredInputDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/hearAbout/UpdateTitle")]
		Task<ResponseModel<HearAboutDto>> CreateOrUpdateHearAbout([Body]HearAboutDto input, [Header("Authorization")] string accessToken);

		//[Post("/api/services/app/hearAbout/Delete")]
		//Task<AjaxResponse> CreateOrUpdateHearAbout([Body]HearAboutDeleteInput input, [Header("Authorization")] string accessToken);
	}

	//public class HearAboutDeleteInput
	//{
	//	public int Id { get; set; }
	//}

}
