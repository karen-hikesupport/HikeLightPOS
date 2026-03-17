
using Realms;

namespace HikePOS.Models
{
	public class ProductTagDto
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public decimal Quantity { get; set; }
		public decimal Amount { get; set; }
		public Color Color { get; set; }
        public string TagId { 
            set {
                if (!string.IsNullOrEmpty(value))
                    Id = value;
            }
        }

        public ProductTagDB ToModel()
        {
            ProductTagDB tagDB = new ProductTagDB
            {
                Name = Name,
                Id = Id,
                Quantity = Quantity,
                Amount = Amount,
            };
            return tagDB;
        }
        public static ProductTagDto FromModel(ProductTagDB tagDB)
        {
            if (tagDB == null)
                return null;
            ProductTagDto tagDto = new ProductTagDto
            {
                Id = tagDB.Id,
                Name = tagDB.Name,
                Quantity = tagDB.Quantity,
                Amount = tagDB.Amount,
            };
            return tagDto;
        }
    }

    public partial class ProductTagDB : IRealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
