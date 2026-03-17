using System;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
    public class GroupPriceTemplateDto 
    {
        public Int32 PriceBookId { get; set; }
        public String SKU { get; set; }
        public String ItemName { get; set; }
        public Decimal RetailPrice { get; set; }
        //Ticket #9784 Start: Customer group from server  issue. By Nikhil.
        public Decimal? LoyaltyValue { get; set; }
        //Ticket #9784 End:By Nikhil. 
        public Decimal? MinUnits { get; set; }
        public Decimal? MaxUnits { get; set; }

        public GroupPriceTemplateDB ToModel()
        {
            GroupPriceTemplateDB model = new GroupPriceTemplateDB
            {
                PriceBookId = PriceBookId,
                SKU = SKU,
                ItemName = ItemName,
                RetailPrice = RetailPrice,
                LoyaltyValue = LoyaltyValue,
                MinUnits = MinUnits,
                MaxUnits = MaxUnits
            };
            return model;
        }
        public static GroupPriceTemplateDto FromModel(GroupPriceTemplateDB model)
        {
            GroupPriceTemplateDto groupPriceDto = new GroupPriceTemplateDto
            {
                PriceBookId = model.PriceBookId,
                SKU = model.SKU,
                ItemName = model.ItemName,
                RetailPrice = model.RetailPrice,
                LoyaltyValue = model.LoyaltyValue,
                MinUnits = model.MinUnits,
                MaxUnits = model.MaxUnits
            };

            return groupPriceDto;
        }
    }

    public partial class GroupPriceTemplateDB : IRealmObject
    {
        public Int32 PriceBookId { get; set; }
        public String SKU { get; set; }
        public String ItemName { get; set; }
        public Decimal RetailPrice { get; set; }
        public Decimal? LoyaltyValue { get; set; }
        public Decimal? MinUnits { get; set; }
        public Decimal? MaxUnits { get; set; }
    }
}
