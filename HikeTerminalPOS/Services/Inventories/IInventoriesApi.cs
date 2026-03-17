using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.Models;
using Refit;

namespace HikePOS.Services
{
	[Headers("Accept: application/json")]
	public interface IInventoriesApi
	{
		[Post("/api/services/app/purchaseOrder/CreateOrUpdate")]
		Task<ResponseModel<PurchaseOrderDto>> CreateorUpdatePO([Body]PurchaseOrderDto input, [Header("Authorization")] string accessToken);

		[Post("/api/services/app/sale/ToCreatePOFromSale")]
		Task<ResponseListModel<PurchaseOrderDto>> GetPOFromSale([AliasAs("invoiceId")] int invoiceId, [Header("Authorization")] string accessToken);

        //Ticket Start:#11847 Feature Request - Cannot link backorders to existing PO by rupesh
        [Post("/api/services/app/purchaseOrder/GetPendingOrdersListBySupplier")]
        Task<ResponseModel<PurchaseOrderListResponseObjectResult>> GetPendingOrdersListBySupplier([AliasAs("outletId")] int outletId, [AliasAs("supplierId")] int supplierId, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/purchaseOrder/AssignBackorderToPO")]
        Task<ResponseModel<object>> AssignBackorderToPO([AliasAs("orderId")] int orderId, [AliasAs("saleInvoiceId")] int saleInvoiceId, [Body]PurchaseOrderDto input,[Header("Authorization")] string accessToken);
        //Ticket End

        [Post("/api/services/app/purchaseOrder/GetPOReferenceSettting")]
        Task<ResponseModel<POReferenceDto>> GetPOReferenceSettting([AliasAs("outletId")] int outletId,[Header("Authorization")] string accessToken);

    }
}
