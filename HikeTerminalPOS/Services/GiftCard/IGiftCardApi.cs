using System;
using System.Threading.Tasks;
using HikePOS.Models;
using Refit;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface IGiftCardApi
	{
		[Post("/api/services/app/giftCard/ValidateGiftCard")]
		Task<ResponseModel<GiftCardDto>> ValidateGiftCard([Header("Authorization")] string accessToken, [AliasAs("CardNumber")] string CardNumber);

		[Post("/api/services/app/giftCard/GetByNumber")]
		Task<ResponseModel<GiftCardDto>> GetByNumber([Header("Authorization")] string accessToken,[AliasAs("CardNumber")] string CardNumber);

		[Post("/api/services/app/giftCard/GetAll")]
		Task<ListResponseModel<GiftCardDto>> GetAll([Body] PagedSortedAndFilteredInputDto filter,[Header("Authorization")] string accessToken );

	}

}
