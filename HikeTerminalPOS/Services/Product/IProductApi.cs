using System;
using System.Threading.Tasks;
using Refit;
using HikePOS.Models;
using System.Collections.ObjectModel;

namespace HikePOS.Services
{
    [Headers("Accept: application/json")]
    public interface IProductApi
    {
        [Post("/api/services/app/category/GetAll")]
        Task<ListResponseModel<CategoryDto>> GetAllCategories([Header("Authorization")] string accessToken);

        [Post("/api/services/app/productSearch/GetOutletProductsWithMinimumPayload")]
        Task<ListResponseModel<ProductDto_POS>> GetAllProducts([Body] ProductFilterModel filterRequest, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/productSearch/GetDetail")]
        Task<ResponseModel<ProductDto_POS>> GetProductDetailById([Body] GetProductDetailInput filterRequest, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/productSearch/GetProductOutletWithMinimumPayload")]
        Task<ListResponseModel<ProductOutletDto_POS>> GetOutletProductStock([AliasAs("outlet")]int outletId, [AliasAs("lastSyncDate")]string lastSyncDate, [Header("Authorization")] string accessToken);

        [Get("/api/services/app/stock/GetProductStocks")]
        Task<ResponseModel<InventoryDto>> GetAllOutletProductStocks([AliasAs("ProductId")]int ProductId, [Header("Authorization")] string accessToken);

        [Get("/api/services/app/stock/GetProductStocks")]
        Task<ResponseModel<Stock>> GetAllOutletProductStocks1([AliasAs("ProductId")]int ProductId, [Header("Authorization")] string accessToken);

        [Get("/api/services/app/productSearch/GetUnityOfMeasuresByOutlet_POS")]
        Task<ListResponseModel<ProductUnitOfMeasureDto>> GetUnityOfMeasuresByOutlet([AliasAs("outlet")] int outletId, [AliasAs("lastSyncDate")] string lastSyncDate, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/tag/GetAll_POS")]
        Task<ListResponseModel<ProductTagDto>> GetAllProductTags([AliasAs("lastSyncDate")] string lastSyncDate, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/category/GetAll_POS")]
        Task<ListResponseModel<CategoryDto>> GetAllCategoriesByOutlet([AliasAs("outletId")] int outletId,[Header("Authorization")] string accessToken);

        [Post("/api/services/app/product/GetProductDescription")]
        Task<ResponseModel<string>> GetProductDescription([Body] GetProductDetailInput filterRequest, [Header("Authorization")] string accessToken);

        //Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
        [Post("/api/services/app/stock/GetProductAvailableStocks")]
        Task<ResponseModel<List<ProductAvailableStocks>>> GetProductAvailableStocks([AliasAs("outletId")] int outletId, [AliasAs("invoiceId")] int invoiceId, [Header("Authorization")] string accessToken);
        //End #84293 by Pratik

        //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
        [Post("/api/services/app/layoutSell/GetCurrentLayout")]
        Task<ResponseModel<InvoiceLayoutSellDto>> GetCurrentLayout([AliasAs("registerId")]int RegisterId, [Header("Authorization")] string accessToken);
        //End #90945 Pratik

    }
}
