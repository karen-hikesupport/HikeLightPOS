
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Sales;
using Refit;

namespace HikePOS.Services
{
    [Headers("Accept: application/json")]
    public interface ISaleApi
    {

        [Post("/api/services/app/saleSearch/GetSalesInvoices")]
        Task<ListResponseModel<InvoiceDto>> GetSalesInvoices([Body] GetInvoiceInput filterRequest, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/saleSearch/GetHistory")]
        Task<ListResponseModel<InvoiceHistoryDto>> GetHistories([AliasAs("invoiceId")] int invoiceId, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/sale/CreateorUpdateInvoice")]
        Task<ResponseModel<InvoiceDto>> CreateorUpdateInvoice([Body] InvoiceDto input, [Header("Authorization")] string accessToken, [Header("User-Agent")] string deviceInfo);

        [Post("/api/services/app/sale/AssociateCustomerToInvoice")]
        Task<ResponseModel<InvoiceDto>> AssociateCustomerToInvoice([Body] InvoiceDto input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/sale/SendInvoiceToCustomer")]
        Task<object> SendInvoiceEmail([AliasAs("invoiceNumber")] string invoiceNumber, [AliasAs("customerId")] int customerId, [AliasAs("email")] string email, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/sale/AddCashDrawerLog")]
        Task<ResponseModel<object>> AddCashDrawerLog([Body] CashDrawerLogInput filterRequest, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/sale/UpdateInvoiceNote")]
        Task<ResponseModel<InvoiceDto>> UpdateInvoiceNote([Body] InvoiceDto input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/sale/HasInStock")]
        Task<ResponseModel<bool>> HasInStock([AliasAs("invoiceId")] int invoiceId, [Header("Authorization")] string accessToken);


        [Post("/api/services/app/sale/SaveSerialNumber")]
        Task<ResponseModel<AjaxResponse>> AddUpdateSerialNumberFromSaleHistory([Body] SerialNumberDto input, [Header("Authorization")] string accessToken);
        //Ticket start:#45653 iPad: FR - create a new customer from the payment slider.by rupesh
        [Post("/api/services/app/sale/Get")]
        Task<ResponseModel<InvoiceDto>> GetInvoice([Body] GetInvoiceDetailInput input, [Header("Authorization")] string accessToken);
        //Ticket end:#45653 .by rupesh

        //Ticket start:#45648 iPad: Make the Customer field editable for the Walk In customers from the Sales History paged.by rupesh
        [Post("/api/services/app/sale/UpdateCustomerName")]
        Task<ResponseModel<AjaxResponse>> UpdateCustomerName([Body] UpdateCustomerDetailInput filterRequest, [Header("Authorization")] string accessToken);
        //Ticket end:#45648 .by rupesh

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        [Post("/api/services/app/sale/UpdateInvoiceDueDate")]
        Task<ResponseModel<string>> UpdateInvoiceDueDate([AliasAs("invoiceId")] int invoiceId, [AliasAs("invoiceDueDate")] System.DateTime invoiceDueDate, [Header("Authorization")] string accessToken);
        //End ticket #76208 by Pratik

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        [Post("/api/services/app/sale/CreateOrUpdateInvoicefulfillment")]
        Task<ResponseModel<InvoiceDto>> CreateOrUpdateInvoicefulfillment([Body] InvoiceDto input, [Header("Authorization")] string accessToken, [Header("User-Agent")] string deviceInfo);
        //End #84293 by Pratik


        //Start #90946 iOS:FR  Item Serial Number Tracking by Pratik
        [Post("/api/services/app/serialNumber/GetPOSSerialNumber")]
        Task<ResponseModel<POSSerialNumberDto>> GetPOSSerialNumber([Body] POSSerialNumberRequest input, [Header("Authorization")] string accessToken);
        //End #90946 by Pratik
        
        //Start #94565 by Pratik
        [Post("/api/services/app/sale/GetFloorSalesList")]
        Task<ResponseListModel<OccupideTableDto>> GetFloorSalesList([AliasAs("outletId")] int outletId, [Header("Authorization")] string accessToken);
        
        //End #94565 by Pratik

    }

}
