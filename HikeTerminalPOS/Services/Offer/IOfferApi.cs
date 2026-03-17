using System;
using System.Threading.Tasks;
using Refit;
using HikePOS.Models;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface IOfferApi
	{

		[Post("/api/services/app/offer/GetAll")]
		Task<ListResponseModel<OfferDto>> GetAll([Header("Authorization")] string accessToken);
	}
}
