using System;
using System.Collections.ObjectModel;
using HikePOS.Enums;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
	public class OfferDto : FullAuditedPassiveEntityDto
	{
		public OfferDto()
		{
			OfferItems = new ObservableCollection<OfferItemDto>();
			OfferOutlets = new ObservableCollection<OfferOutletDto>();
			OfferCustomerGroups = new ObservableCollection<OfferCustomerGroupDto>();
            OfferDays = new List<int>();
		}

		public string Name { get; set; }
		public int? Priority { get; set; }
		public string Description { get; set; }
		public Guid? OfferImage { get; set; }
		public OfferType OfferType { get; set; }

        [JsonIgnore]
        public decimal? BuyX { get; set; }
        [JsonProperty("BuyX")]
        public string RoundedBuyX
        {
            get
            {
                return BuyX?.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                BuyX = result;
            }
        }

        [JsonIgnore]
        public decimal? GetX { get; set; }
        [JsonProperty("GetX")]
        public string RoundedGetX
        {
            get
            {
                return GetX?.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                GetX = result;
            }
        }

        public bool IsPercentage { get; set; }
		public decimal OfferValue { get; set; }
		public DateTime? ValidFrom { get; set; }
		public DateTime? ValidTo { get; set; }
		public int? MinimumQuantity { get; set; }
		public int? MaximumQuantity { get; set; }
		public decimal OfferAmount { get; set; }
		public bool IsOfferOnAllCustomer { get; set; }
		public bool IsOfferOnAllOutlet { get; set; }
		public string CouponCode { get; set; }
		public string Sku { get; set; }
		public string BarCode { get; set; }

		public int? TaxID { get; set; }
		public string TaxName { get; set; }
		public decimal? TaxRate { get; set; }

        public bool Include { get; set; } //Start #91264 By Pratik

		public ObservableCollection<OfferItemDto> OfferItems { get; set; }
		public ObservableCollection<OfferOutletDto> OfferOutlets { get; set; }
		public ObservableCollection<OfferCustomerGroupDto> OfferCustomerGroups { get; set; }
        public int? OfferId { get; set; }

        //Ticket start:#30959 iPad - New feature request :: Rule for discount offers when there are more than one offers applicable.by rupesh
        [JsonIgnore]
        public decimal? TotalDiscount { get; set; }
        [JsonIgnore]
        private bool _IsSelected { get; set; }
        [JsonIgnore]
        public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; SetPropertyChanged(nameof(IsSelected)); } }
        //Ticket end:#30959 .by rupesh

        //Ticket start:#73188 iPad - Automatic Discounts every Weekday. by rupesh
        public List<int> OfferDays { get; set; } //#100117
        //Ticket end:#73188 . by rupesh
        
        public string FromTime { get; set; } //#94419
		public string ToTime { get; set; } //#94419


        public OfferDB ToModel()
        {
            OfferDB model = new OfferDB
            {
                Id = Id,
                IsActive = IsActive,
                Name = Name,
                Priority = Priority,
                Description = Description,
                OfferImage = OfferImage,
                OfferType = (int)OfferType,
                BuyX = BuyX,
                RoundedBuyX = RoundedBuyX,
                GetX = GetX,
                RoundedGetX = RoundedGetX,
                IsPercentage = IsPercentage,
                OfferValue = OfferValue,
                ValidFrom = ValidFrom,
                ValidTo = ValidTo,
                MinimumQuantity = MinimumQuantity,
                MaximumQuantity = MaximumQuantity,
                OfferAmount = OfferAmount,
                IsOfferOnAllCustomer = IsOfferOnAllCustomer,
                IsOfferOnAllOutlet = IsOfferOnAllOutlet,
                CouponCode = CouponCode,
                Sku = Sku,
                BarCode = BarCode,
                TaxID = TaxID,
                TaxName = TaxName,
                TaxRate = TaxRate,
                OfferId = OfferId,
                TotalDiscount = TotalDiscount,
                IsSelected = IsSelected,
                Include = Include, //Start #91264 By Pratik
                FromTime = FromTime,
                ToTime = ToTime
            };

            OfferOutlets?.ForEach(a => model.OfferOutlets.Add(a.ToModel()));
            OfferItems?.ForEach(a => model.OfferItems.Add(a.ToModel()));
            OfferCustomerGroups?.ForEach(a => model.OfferCustomerGroups.Add(a.ToModel()));
            OfferDays?.ForEach(a => model.OfferDays.Add(a));
            return model;
        }

        public static OfferDto FromModel(OfferDB dbModel)
        {
            if (dbModel == null)
                return null;

            OfferDto model = new OfferDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                Name = dbModel.Name,
                Priority = dbModel.Priority,
                Description = dbModel.Description,
                OfferImage = dbModel.OfferImage,
                OfferType = (OfferType)dbModel.OfferType,
                BuyX = dbModel.BuyX,
                RoundedBuyX = dbModel.RoundedBuyX,
                GetX = dbModel.GetX,
                RoundedGetX = dbModel.RoundedGetX,
                IsPercentage = dbModel.IsPercentage,
                OfferValue = dbModel.OfferValue,
                ValidFrom = dbModel.ValidFrom?.UtcDateTime,
                ValidTo = dbModel.ValidTo?.UtcDateTime,
                MinimumQuantity = dbModel.MinimumQuantity,
                MaximumQuantity = dbModel.MaximumQuantity,
                OfferAmount = dbModel.OfferAmount,
                IsOfferOnAllCustomer = dbModel.IsOfferOnAllCustomer,
                IsOfferOnAllOutlet = dbModel.IsOfferOnAllOutlet,
                CouponCode = dbModel.CouponCode,
                Sku = dbModel.Sku,
                BarCode = dbModel.BarCode,
                TaxID = dbModel.TaxID,
                TaxName = dbModel.TaxName,
                TaxRate = dbModel.TaxRate,
                OfferId = dbModel.OfferId,
                TotalDiscount = dbModel.TotalDiscount,
                IsSelected = dbModel.IsSelected,
                Include = dbModel.Include, //Start #91264 By Pratik
                FromTime = dbModel.FromTime,
                ToTime = dbModel.ToTime
            
            };

            if(dbModel.OfferCustomerGroups != null)
                model.OfferCustomerGroups = new ObservableCollection<OfferCustomerGroupDto>(dbModel.OfferCustomerGroups.ToList().Select(a => OfferCustomerGroupDto.FromModel(a)));
            if(dbModel.OfferOutlets != null)
                model.OfferOutlets = new ObservableCollection<OfferOutletDto>(dbModel.OfferOutlets.ToList().Select(a => OfferOutletDto.FromModel(a)));
            if(dbModel.OfferItems != null)
                model.OfferItems = new ObservableCollection<OfferItemDto>(dbModel.OfferItems.ToList().Select(a=> OfferItemDto.FromModel(a)));
            if (dbModel.OfferDays != null && dbModel.OfferDays.Count > 0)
                model.OfferDays = dbModel.OfferDays.Select(a => a).ToList();

            return model;
        }

    }

    public partial class OfferDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public int? Priority { get; set; }
        public string Description { get; set; }
        public Guid? OfferImage { get; set; }
        public int OfferType { get; set; }
        public decimal? BuyX { get; set; }
        public string RoundedBuyX
        {
            get
            {
                return BuyX?.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                BuyX = result;
            }
        }
        public decimal? GetX { get; set; }
        public string RoundedGetX
        {
            get
            {
                return GetX?.ToString("0.####");// Decimal.ToInt32(Quantity); 
            }
            set
            {
                decimal result = 0;
                decimal.TryParse(value, out result);
                GetX = result;
            }
        }
        public bool IsPercentage { get; set; }
        public decimal OfferValue { get; set; }
        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }
        public int? MinimumQuantity { get; set; }
        public int? MaximumQuantity { get; set; }
        public decimal OfferAmount { get; set; }
        public bool IsOfferOnAllCustomer { get; set; }
        public bool IsOfferOnAllOutlet { get; set; }
        public string CouponCode { get; set; }
        public string Sku { get; set; }
        public string BarCode { get; set; }

        public int? TaxID { get; set; }
        public string TaxName { get; set; }
        public decimal? TaxRate { get; set; }

        public IList<OfferItemDB> OfferItems { get; }
        public IList<OfferOutletDB> OfferOutlets { get; }
        public IList<OfferCustomerGroupDB> OfferCustomerGroups { get; }
        public int? OfferId { get; set; }
        public decimal? TotalDiscount { get; set; }
        public bool IsSelected { get; set; }
        public IList<int> OfferDays { get; }
        public bool Include { get; set; } //Start #91264 By Pratik
        
        public string FromTime { get; set; }
		public string ToTime { get; set; }
    }
}
