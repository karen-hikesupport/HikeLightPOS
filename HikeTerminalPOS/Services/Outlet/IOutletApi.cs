using System;
using System.Threading.Tasks;
using Refit;
using HikePOS.Models;
using System.Collections.ObjectModel;

namespace HikePOS.Services
{


	[Headers("Accept: application/json")]
	public interface IOutletApi
	{
		[Post("/api/services/app/outlet/GetAll_POS")]
		Task<ListResponseModel<OutletDto_POS>> GetAll([Header("Authorization")] string accessToken);

		[Post("/api/services/app/outlet/GetDetail")]
		Task<ResponseModel<OutletDto_POS>> GetDetailById([Body] FullAuditedPassiveEntityDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/register/OpenRegister")]
		Task<ResponseModel<RegisterDto>> OpenRegister([Body] OpenRegisterInput input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/register/GetRegister")]
		Task<ResponseModel<RegisterDto>> GetRegisterById([Body] FullAuditedPassiveEntityDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/receiptTemplate/GetAll")]
		Task<ListResponseModel<ReceiptTemplateDto>> GetAllTemplatesReceipt([Header("Authorization")] string accessToken);

		//#36109 iPad: Tax collector was updated wrong in cash register.

		//https://hikeasytest0h2.asy.io:82/api/services/app/register/GetLastRegisterClosureById?registerClosureId=2
		[Post("/api/services/app/register/GetLastRegisterClosureById")]
		Task<ResponseModel<RegisterclosureDto>> GetLastRegisterClosureById([AliasAs("registerClosureId")] int registerId, [Header("Authorization")] string accessToken);
		//#36109 iPad: Tax collector was updated wrong in cash register.

		[Post("/api/services/app/register/CloseRegister")]
		Task<ResponseModel<RegisterDto>> CloseRegister([Body] RegisterDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/register/GetRegisterClosure")]
		Task<ResponseModel<RegisterclosureDto>> GetRegisterclosure([Header("Authorization")] string accessToken, [AliasAs("RegisterId")] int RegisterId);

		[Post("/api/services/app/register/UpdateRegisterClosureByClosureId")]
		Task<ResponseModel<AjaxResponse>> UpdateRegisterClosure([Header("Authorization")] string accessToken, [AliasAs("RegisterClosureId")] int RegisterClosureId);

		[Post("/api/services/app/register/CreateOrUpdateRegisterCashInOut")]
		Task<ResponseModel<RegisterCashInOutDto>> CreateOrUpdateRegisterCashInOut([Body] RegisterCashInOutDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/register/CreateOrUpdateRegisterClosureDenomination")]
		Task<ResponseModel<RegisterDto>> CreateOrUpdateRegisterClosureDenomination([Body] ObservableCollection<RegisterClosureTallyDenominationDto> input, [Header("Authorization")] string accessToken);


		[Post("/api/services/app/denomination/GetAll")]
		Task<ListResponseModel<DenominationDto>> GetAllDenomination([Header("Authorization")] string accessToken);

		[Post("/api/services/app/denomination/CreateOrUpdate")]
		Task<ResponseModel<DenominationDto>> CreateOrUpdateDenomination([Body] DenominationDto input, [Header("Authorization")] string accessToken);


		[Post("/api/services/app/denomination/Delete")]
		Task<ResponseModel<DenominationDto>> DeleteDenomination([Body] DenominationDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/register/UpdateRegisterClosureMerchantSettleReciept")]
		Task<ResponseModel<AjaxResponse>> UpdateRegisterClosureMerchantSettleReciept([Body] RecieptDataRequest input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/register/CheckIfRegisterClosed")]
		Task<ResponseModel<object>> CheckIfRegisterClosed([Header("Authorization")] string accessToken, [AliasAs("ClosureId")] int ClosureId);

		//Start #92768 Pratik
		[Post("/api/services/app/registerReport/SendRegisterClosureEmail")]
		Task<ResponseModel<object>> SendRegisterClosureEmail([Body]SendRegisterClosureDto input, [Header("Authorization")] string accessToken);
		//End #92768 Pratik
	}
}
