using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
	public class ProductAttributeDto : FullAuditedPassiveEntityDto
    {
		public ProductAttributeDto()
		{
			ProductAttributeValues = new ObservableCollection<ProductAttributeValueDto>();
		}
		public int ProductAttributeId { get; set; }
		public int AttributeId { get; set; }
		public string Name { get; set; }
		public int Sequence { get; set; }

        ObservableCollection<ProductAttributeValueDto> _ProductAttributeValues { get; set; }
        public ObservableCollection<ProductAttributeValueDto> ProductAttributeValues { get { return _ProductAttributeValues; } set { _ProductAttributeValues = value; SetPropertyChanged(nameof(ProductAttributeValues)); } }

        double _ProductAtrHeight;
        [JsonIgnore]
        public double ProductAtrHeight { get { return _ProductAtrHeight; } set { _ProductAtrHeight = value; SetPropertyChanged(nameof(ProductAtrHeight)); } }

        public ProductAttributeDB ToModel()
        {
            ProductAttributeDB productAttribute = new ProductAttributeDB
            {
                ProductAttributeId = ProductAttributeId,
                AttributeId = AttributeId,
                Name = Name,
                Sequence = Sequence,
                Id = Id,
                IsActive = IsActive
            };

            ProductAttributeValues?.ForEach(i => productAttribute.ProductAttributeValues.Add(i));

            return productAttribute;
        }

        public static ProductAttributeDto FromModel(ProductAttributeDB list)
        {
            if (list == null)
                return null;
            ProductAttributeDto productAttribute = new ProductAttributeDto
            {
                ProductAttributeId = list.ProductAttributeId,
                AttributeId = list.AttributeId,
                Name = list.Name,
                Sequence = list.Sequence,
                Id = list.Id,
                IsActive = list.IsActive
            };

            productAttribute.ProductAttributeValues = new ObservableCollection<ProductAttributeValueDto>(list.ProductAttributeValues);

            return productAttribute;
        }
    }

    public partial class ProductAttributeDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        [PrimaryKey]
        public int ProductAttributeId { get; set; }
        public int AttributeId { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
        public IList<ProductAttributeValueDto> ProductAttributeValues { get; }
    }
}
