using System;
using System.Threading.Tasks;
using Fusillade;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Refit;

namespace HikePOS.Services.Payment
{
    public interface IWindcavePaymentService
    {
		//http://localhost:6235/api/services/app/windcavePayment/WindcaveSaleTransaction
		[Post("/api/services/app/windcavePayment/WindcaveSaleTransaction")]
		Task<WindcaveRoot> CreateWindcaveSale([Body] WindcaveRequest input, [Header("Authorization")] string accessToken);

		//Task<ResponseModel<object>> CreateAfterpayPayment([Body] AfterPayRequestObject input, [Header("Authorization")] string accessToken);


		//http://localhost:6235/api/services/app/windcavePayment/WindcaveRefundTransaction
		[Post("/api/services/app/windcavePayment/WindcaveRefundTransaction")]
		Task<WindcaveRoot> CreateWindcaveRefund([Body] WindcaveRequest input, [Header("Authorization")] string accessToken);

		//http://localhost:6235/api/services/app/windcavePayment/WindcaveStatusCheck
		[Post("/api/services/app/windcavePayment/WindcaveStatusCheck")]
		Task<WindcaveRoot> CheckWindcaveStatusCheck([Body] WindcaveStatusCheckDTO input, [Header("Authorization")] string accessToken);


		//http://localhost:6235/api/services/app/windcavePayment/WindcaveButtonTransaction
		[Post("/api/services/app/windcavePayment/WindcaveButtonTransaction")]
		Task<WindcaveButtonTransactionRoot> WindcaveButtonTransaction([Body] WindcaveButtonTransactionRequest input, [Header("Authorization")] string accessToken);
	}
}
