using System;
using Realms;
namespace HikePOS.Models
{
    public class GroupPriceListOutletDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public Int32 PriceBookId { get; set; }
        public Int32 OutletId { get; set; }
        public string OutletName { get; set; }

        public GroupPriceListOutletDB ToModel()
        {
            GroupPriceListOutletDB model = new GroupPriceListOutletDB
            {
                Id = Id,
                IsActive = IsActive,
                PriceBookId = PriceBookId,
                OutletId = OutletId,
                OutletName = OutletName
            };
            return model;
        }
        public static GroupPriceListOutletDto FromModel(GroupPriceListOutletDB model)
        {
            GroupPriceListOutletDto groupPriceDto = new GroupPriceListOutletDto
            {
                Id = model.Id,
                IsActive = model.IsActive,
                PriceBookId = model.PriceBookId,
                OutletId = model.OutletId,
                OutletName = model.OutletName
            };
            
            return groupPriceDto;
        }
    }

    public partial class GroupPriceListOutletDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public Int32 PriceBookId { get; set; }
        public Int32 OutletId { get; set; }
        public string OutletName { get; set; }
    }
}
