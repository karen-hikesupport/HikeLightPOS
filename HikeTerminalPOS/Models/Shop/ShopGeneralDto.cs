using Realms;
namespace HikePOS.Models
{
	public class ShopGeneralDto
	{
		public ShopDto Shop { get; set; }
		public AddressDto Address { get; set; }
		public GeneralRuleDto GeneralRule { get; set; }
		public ZoneAndFormatDetailDto ZoneAndFormat { get; set; }
		public VirtaulAddressDto VirtualAddress { get; set; }
        public ShopGeneralDB ToModel()
        {
            ShopGeneralDB shopGeneralDB = new ShopGeneralDB
            {
                Shop = Shop.ToModel(),
                Address = Address.ToModel(),
                GeneralRule = GeneralRule.ToModel(),
                ZoneAndFormat = ZoneAndFormat.ToModel(),
                VirtualAddress = VirtualAddress
            };
            return shopGeneralDB;
        }
        public static ShopGeneralDto FromModel(ShopGeneralDB shopGeneralDB)
        {
            if (shopGeneralDB == null)
                return null;

            ShopGeneralDto shopGeneralDto = new ShopGeneralDto
            {
                Shop = ShopDto.FromModel(shopGeneralDB.Shop),
                Address = AddressDto.FromModel(shopGeneralDB.Address),
                GeneralRule = GeneralRuleDto.FromModel(shopGeneralDB.GeneralRule),
                ZoneAndFormat = ZoneAndFormatDetailDto.FromModel(shopGeneralDB.ZoneAndFormat),
                VirtualAddress = shopGeneralDB.VirtualAddress
            };
            return shopGeneralDto;

        }
    }
    public partial class ShopGeneralDB : IRealmObject
    {
        public ShopDB Shop { get; set; }
        public AddressDB Address { get; set; }
        public GeneralRuleDB GeneralRule { get; set; }
        public ZoneAndFormatDetailDB ZoneAndFormat { get; set; }
        public VirtaulAddressDto VirtualAddress { get; set; }
    }

}
