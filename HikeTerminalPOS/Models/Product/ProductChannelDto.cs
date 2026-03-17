
namespace HikePOS.Models
{
	public class ProductChannelDto
	{
		public SellsChannel SellsChannel { get; set; }
	}

	public enum SellsChannel
	{
		PointOfSale = 1,
		eCommerce = 2
	}


}
