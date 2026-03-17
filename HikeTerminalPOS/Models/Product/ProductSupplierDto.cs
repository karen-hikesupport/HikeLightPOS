using Realms;

namespace HikePOS.Models
{
	public partial class ProductSupplierDto : IRealmObject
    {
        [PrimaryKey]
        public int SupplierId { get; set; }
		public string SupplierCode { get; set; }
		public bool IsDefault { get; set; }
	}
}
