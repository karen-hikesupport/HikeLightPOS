using System;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public class ProductVarientsDto
    {
        public ProductVarientsDto()
        {
            VariantOutlet = new ProductOutletDto_POS();
            VarientOutlets = new ObservableCollection<ProductOutletDto_POS>();
            VariantAttributesValues = new ObservableCollection<VariantAttributeValueDto>();
        }

        public int ProductVarientId { get; set; }

        public string Name { get; set; }
        public string sku { get; set; }
        public string BarCode { get; set; }

        public ObservableCollection<VariantAttributeValueDto> VariantAttributesValues { get; set; }

        public ObservableCollection<ProductOutletDto_POS> VarientOutlets { get; set; }
        public ProductOutletDto_POS VariantOutlet { get; set; }

        [JsonIgnore]
        public string Stock
        {
            get
            {
                if (VariantOutlet != null)
                {
                    return (VariantOutlet.Stock).ToKMFormat();
                }
                return "0";
            }
        }

        public ProductVarientsDB ToModel()
        {
            ProductVarientsDB productVarients = new ProductVarientsDB
            {
                BarCode = BarCode,
                sku = sku,
                Name = Name,
                ProductVarientId = ProductVarientId,
                VariantOutlet = VariantOutlet?.ToModel(),
            };

            VarientOutlets?.ForEach(i => productVarients.VarientOutlets.Add(i.ToModel()));
            VariantAttributesValues?.ForEach(i => productVarients.VariantAttributesValues.Add(i));

            return productVarients;
        }

        public static ProductVarientsDto FromModel(ProductVarientsDB list)
        {
            if (list == null)
                return null;

            ProductVarientsDto productVarients = new ProductVarientsDto
            {
                BarCode = list.BarCode,
                sku = list.sku,
                Name = list.Name,
                ProductVarientId = list.ProductVarientId,
                VariantOutlet = ProductOutletDto_POS.FromModel(list.VariantOutlet),
            };

            productVarients.VarientOutlets = new ObservableCollection<ProductOutletDto_POS>(list.VarientOutlets.Select(a => ProductOutletDto_POS.FromModel(a)));
            productVarients.VariantAttributesValues = new ObservableCollection<VariantAttributeValueDto>(list.VariantAttributesValues);

            return productVarients;
        }
    }

    public partial class ProductVarientsDB : IRealmObject
    {
        public ProductVarientsDB()
        {
            VariantOutlet = new ProductOutletDB_POS();
        }

        [PrimaryKey]
        public int ProductVarientId { get; set; }

        public string Name { get; set; }
        public string sku { get; set; }
        public string BarCode { get; set; }

        public IList<VariantAttributeValueDto> VariantAttributesValues { get; }
        public IList<ProductOutletDB_POS> VarientOutlets { get; }
        public ProductOutletDB_POS VariantOutlet { get; set; }
    }
}
