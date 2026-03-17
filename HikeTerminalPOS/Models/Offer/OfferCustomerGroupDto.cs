using Realms;

namespace HikePOS.Models
{
	public partial class OfferCustomerGroupDto : FullAuditedPassiveEntityDto
    {
        public int OfferId { get; set; }
		public int CustomerGroupId { get; set; }
		public string CustomerGroupName { get; set; }

        public OfferCustomerGroupDB ToModel()
        {
            OfferCustomerGroupDB model = new OfferCustomerGroupDB
            {
                Id = Id,
                IsActive = IsActive,
                OfferId = OfferId,
                CustomerGroupId = CustomerGroupId,
                CustomerGroupName = CustomerGroupName
            };

            return model;
        }

        public static OfferCustomerGroupDto FromModel(OfferCustomerGroupDB dbModel)
        {
            if (dbModel == null)
                return null;

            OfferCustomerGroupDto model = new OfferCustomerGroupDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                OfferId = dbModel.OfferId,
                CustomerGroupId = dbModel.CustomerGroupId,
                CustomerGroupName = dbModel.CustomerGroupName
            };

            return model;
        }
    }

    public partial class OfferCustomerGroupDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int OfferId { get; set; }
        public int CustomerGroupId { get; set; }
        public string CustomerGroupName { get; set; }
    }
}
