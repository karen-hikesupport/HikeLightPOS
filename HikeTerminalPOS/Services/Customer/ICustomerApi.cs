using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Customer;
using Refit;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface ICustomerApi
	{

		[Post("/api/services/app/customer/GetAll_For_POS_ByPaging")]
		Task<ListResponseModel<CustomerDto_POS>> GetCustomers([Body]GetCustomerInput input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/customerGroup/GetAll_For_POS")]
		Task<ListResponseModel<CustomerGroupDto>> GetCustomerGroups([Header("Authorization")] string accessToken);

		[Post("/api/services/app/customer/CreateOrUpdate")]
		Task<ResponseModel<CustomerDto_POS>> AddOrUpadateCustomer([Body]CustomerDto_POS input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/customer/GetDetail")]
		Task<ResponseModel<CustomerDto_POS>> GetCustomerDetail([Body]GetCustomerDetailInput input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/customer/GetCustomerInvoices")]
		Task<ListResponseModel<InvoiceDto>> GetCustomerInvoices([Body]GetCustomerInvoicesInput input, [Header("Authorization")] string accessToken);

        [Post ("/api/services/app/customer/GetByEmail")]
        Task<ResponseModel<CustomerDto_POS>> GetCustomerDetailByEmail([AliasAs("Email")]string Email, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/customer/GetCustomerCreditBalance")]
        Task<ListResponseModel<CreditBalanceHistoryDto>> GetCustomerCreditBalance([Body]GetCustomerCreditBalanceInput input, [Header("Authorization")] string accessToken);

        [Post ("/api/services/app/customer/AddCustomerCreditIssued")]
        Task<ResponseModel<CreditBalanceHistoryDto>> AddCustomerCreditIssued([Body]CreditBalanceHistoryDto input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/customer/GetCustomFields")]
        Task<ResponseListModel<CustomField>> GetCustomFields([Header("Authorization")] string accessToken);

        [Post("/api/services/app/customer/CreateOrUpdateCustomField")]
        Task<ResponseListModel<CustomField>> AddOrUpadateCustomField([Body]ObservableCollection<CustomField> input, [Header("Authorization")] string accessToken);

		//Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
		[Post("/api/services/app/customer/IsExistCustomerAddresses")]
		Task<ResponseModel<bool>> IsExistCustomerAddress([AliasAs("customerId")] int CustomerId, [Header("Authorization")] string accessToken);


		[Post("/api/services/app/customer/GetAllDeliveryAddresses")]
		Task<ListResponseModel<CustomerAddressDto>> GetAllDeliveryAddresses([Body] CustomerAddressInputDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/customer/GetCustomerAddresses")]
		Task<ListResponseModel<CustomerAddressDto>> GetCustomerAddresses([Body] CustomerAddressInputDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/customer/CreateOrUpdateCustomerAddresse")]
		Task<ResponseModel<CustomerAddressDto>> CreateOrUpdateCustomerAddress([Body] CustomerAddressDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/customer/DeleteDeliveryAddress")]
		Task<ResponseModel<object>> DeleteDeliveryAddress([AliasAs("customerAddressId")] int customerAddressId, [Header("Authorization")] string accessToken);
		//Ticket end:#26664 .by rupesh


	}
}
