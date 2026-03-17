using System;
using Realms;

namespace HikePOS.Models
{
	public partial class OfferOutletDto : FullAuditedPassiveEntityDto
    {
        public int OfferId { get; set; }
		public int OutletId { get; set; }
		public string Outletname { get; set; }

        public OfferOutletDB ToModel()
        {
            OfferOutletDB model = new OfferOutletDB
            {
                Id = Id,
                IsActive = IsActive,
                OfferId = OfferId,
                OutletId = OutletId,
                Outletname = Outletname
            };

            return model;
        }

        public static OfferOutletDto FromModel(OfferOutletDB dbModel)
        {
            if (dbModel == null)
                return null;

            OfferOutletDto model = new OfferOutletDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                OfferId = dbModel.OfferId,
                OutletId = dbModel.OutletId,
                Outletname = dbModel.Outletname
            };

            return model;
        }
    }

    public partial class OfferOutletDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int OfferId { get; set; }
        public int OutletId { get; set; }
        public string Outletname { get; set; }
    }
}
