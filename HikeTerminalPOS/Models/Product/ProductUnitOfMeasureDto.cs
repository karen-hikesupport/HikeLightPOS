
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public partial class ProductUnitOfMeasureDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public int ProductId { get; set; }

        public int MeasureProductId { get; set; }

        public string Name { get; set; }

        public string Sku { get; set; }

        public string BarCode { get; set; }

        public decimal Qty { get; set; }

        public bool DiscountIsAsPercentage { get; set; }

        public decimal DiscountValue { get; set; }

        public IList<int> OutletIds { get;}

        public IList<int> VariantIds { get; }
    }

    public class ProductUnitOfMeasureDto : FullAuditedPassiveEntityDto
    {

        public int ProductId { get; set; }

        public int MeasureProductId { get; set; }

        public string Name { get; set; }

        public string Sku { get; set; }

        public string BarCode { get; set; }

        public decimal Qty { get; set; }

        public bool DiscountIsAsPercentage { get; set; }

        public decimal DiscountValue { get; set; }

        public ObservableCollection<int> OutletIds { get; set; }

        public ObservableCollection<int> VariantIds { get; set; }

        public ProductUnitOfMeasureDB ToModel()
        {
            ProductUnitOfMeasureDB productVarients = new ProductUnitOfMeasureDB
            {
                Id = Id,
                IsActive = IsActive,
                ProductId = ProductId,
                Qty = Qty,
                DiscountIsAsPercentage = DiscountIsAsPercentage,
                DiscountValue = DiscountValue,
                BarCode = BarCode,
                Sku = Sku,
                Name = Name,
                MeasureProductId = MeasureProductId
            };

            OutletIds?.ForEach(i => productVarients.OutletIds.Add(i));
            VariantIds?.ForEach(i => productVarients.VariantIds.Add(i));

            return productVarients;
        }

        public static ProductUnitOfMeasureDto FromModel(ProductUnitOfMeasureDB list)
        {
            if (list == null)
                return null;

            ProductUnitOfMeasureDto productVarients = new ProductUnitOfMeasureDto
            {
                Id = list.Id,
                IsActive = list.IsActive,
                ProductId = list.ProductId,
                Qty = list.Qty,
                DiscountIsAsPercentage = list.DiscountIsAsPercentage,
                DiscountValue = list.DiscountValue,
                BarCode = list.BarCode,
                Sku = list.Sku,
                Name = list.Name,
                MeasureProductId = list.MeasureProductId
            };

            productVarients.OutletIds = new ObservableCollection<int>(list.OutletIds.ToList());
            productVarients.VariantIds = new ObservableCollection<int>(list.VariantIds.ToList());

            return productVarients;
        }

    }
}
