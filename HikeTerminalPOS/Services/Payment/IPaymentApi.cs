using System;
using System.Threading.Tasks;
using HikePOS.Models;
using Refit;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface IPaymentApi
	{
		[Post("/api/services/app/paymentConfiguration/GetAll")]
		Task<ListResponseModel<PaymentOptionDto>> GetPaymentOptions([Header("Authorization")] string accessToken);

		[Post("/api/services/app/paymentConfiguration/CreateOrUpdate")]
		Task<ResponseModel<PaymentOptionDto>> CreateOrUpdate([Body]PaymentOptionDto input, [Header("Authorization")] string accessToken);
	
		[Post("/api/services/app/sale/SendInvoiceToCustomer")]
		Task<object> SendInvoiceEmail([AliasAs("invoiceNumber")] string invoiceNumber, [AliasAs("customerId")] int customerId, [AliasAs("email")] string email, [Header("Authorization")] string accessToken);

	}
}
