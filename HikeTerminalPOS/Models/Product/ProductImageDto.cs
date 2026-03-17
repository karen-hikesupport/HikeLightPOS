using Realms;
namespace HikePOS.Models
{
	public partial class ProductImageDto : IRealmObject
    {
		public string ImageName { get; set; }
		public string OriginalName { get; set; }
		public bool IsPrimary { get; set; }
	}
}
