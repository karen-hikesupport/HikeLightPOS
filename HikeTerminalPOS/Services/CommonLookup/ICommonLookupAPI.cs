using System;
using System.Threading.Tasks;
using HikePOS.Models;
using Refit;
namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface ICommonLookupAPI
	{
		[Post("/api/services/app/commonLookup/GetCountriesForCombobox")]
		Task<ListResponseModel<CountriesDto>> GetAllCountries([Header("Authorization")] string accessToken);
	}
}
