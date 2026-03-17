using System;
using System.Threading.Tasks;
using HikePOS.Models;
using Refit;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface ITagApi
	{
		[Post("/api/services/app/tag/GetAll")]
		Task<ListResponseModel<TagDto>> GetAll([Header("Authorization")] string accessToken);
	}
}
