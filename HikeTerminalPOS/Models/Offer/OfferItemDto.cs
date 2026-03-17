using HikePOS.Enums;
using Realms;

namespace HikePOS.Models
{
	public class OfferItemDto : FullAuditedPassiveEntityDto
	{
        public int OfferId { get; set; }
        public OfferOn OfferOn { get; set; }
        public int OfferOnId { get; set; }
        public string Name { get; set; }
        public decimal? CompositeQty { get; set; }
        public decimal? FixedPrice { get; set; }
        public int BuyAndGetType { get; set; }

        public OfferItemDB ToModel()
        {
            OfferItemDB model = new OfferItemDB
            {
                Id = Id,
                IsActive = IsActive,
                OfferId = OfferId,
                OfferOn = (int)OfferOn,
                OfferOnId = OfferOnId,
                Name = Name,
                CompositeQty = CompositeQty,
                FixedPrice = FixedPrice,
                BuyAndGetType = BuyAndGetType
            };

            return model;
        }

        public static OfferItemDto FromModel(OfferItemDB dbModel)
        {
            if (dbModel == null)
                return null;

            OfferItemDto model = new OfferItemDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                OfferId = dbModel.OfferId,
                OfferOn = (OfferOn)dbModel.OfferOn,
                OfferOnId = dbModel.OfferOnId,
                Name = dbModel.Name,
                CompositeQty = dbModel.CompositeQty,
                FixedPrice = dbModel.FixedPrice,
                BuyAndGetType = dbModel.BuyAndGetType
            };

            return model;
        }
    }

    public partial class OfferItemDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int OfferId { get; set; }
        public int OfferOn { get; set; }
        public int OfferOnId { get; set; }
        public string Name { get; set; }
        public decimal? CompositeQty { get; set; }
        public decimal? FixedPrice { get; set; }
        public int BuyAndGetType { get; set; }
    }
}
