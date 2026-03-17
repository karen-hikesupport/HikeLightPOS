
using System.Threading.Tasks;
using Refit;
using HikePOS.Models;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface ITaxApi
	{
		//[Post("/api/services/app/tax/GetAll")]
		//Task<TaxDto> CreateOrUpdate([Header("Authorization")] string accessToken, TaxDto input);

		//[Post("/api/services/app/shop/GetDetail")]
		//Task<TaxDto> Get([Header("Authorization")] string accessToken, EntityDto input);

		//[Post("/api/services/app/shop/GetDetail")]
		//Task<PagedResultDto<TaxDto>> GetAll([Header("Authorization")] string accessToken, GetTaxInput input);

		[Post("/api/services/app/tax/GetAll")]
		Task<ListResponseModel<TaxDto>> GetAll([Header("Authorization")] string accessToken);

		//[Post("/api/services/app/shop/GetDetail")]
		//Task<TaxDto> GetByName([Header("Authorization")] string accessToken, string name);

		//[Post("/api/services/app/shop/GetDetail")]
		//Task Delete([Header("Authorization")] string accessToken, EntityDto input);
	}
}
