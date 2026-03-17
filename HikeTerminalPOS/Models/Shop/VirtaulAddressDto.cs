using Realms;
namespace HikePOS.Models
{
	public partial class VirtaulAddressDto : IRealmObject
	{
		public string FacebookUrl { get; set; }
		public string LinkedInUrl { get; set; }
		public string TwitterUrl { get; set; }
		public string GoogleUrl { get; set; }
		public string Website { get; set; }
	}
}
