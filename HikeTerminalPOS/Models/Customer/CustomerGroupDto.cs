using System;
using System.Collections.ObjectModel;
using Realms;
namespace HikePOS.Models
{
    public class CustomerGroupDto : FullAuditedPassiveEntityDto
    {
        public string Name { get; set; }
        public bool EnableDiscount { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime CreationTime { get; set; }
        public int CustomerGroupDiscountType { get; set; }
        public int CustomerCount { get; set; }

        public DateTime LastModificationTime
        {
            get;
            set;
        }
        public string UploadFileName { get; set; }

        //  public ICollection<GroupPriceListOutletDto> SelectedOutlets { get; set; }
        public ObservableCollection<GroupPriceDto> PriceBookLists { get; set; }

        public CustomerGroupDB ToModel()
        {
            CustomerGroupDB customerGroupDB = new CustomerGroupDB
            {
                Id = Id,
                IsActive = IsActive,
                Name = Name,
                EnableDiscount = EnableDiscount,
                DiscountPercent = DiscountPercent,
                CreationTime = CreationTime,
                CustomerGroupDiscountType = CustomerGroupDiscountType,
                CustomerCount = CustomerCount,
                LastModificationTime = LastModificationTime,
                UploadFileName = UploadFileName

            };
            if (PriceBookLists != null)
            {
                PriceBookLists.ForEach(i => customerGroupDB.PriceBookLists.Add(i.ToModel()));
            }

            return customerGroupDB;
        }
        public static CustomerGroupDto FromModel(CustomerGroupDB customerGroupDB)
        {
            if (customerGroupDB == null)
                return null;

            CustomerGroupDto customerGroupDto = new CustomerGroupDto
            {
                Id = customerGroupDB.Id,
                IsActive = customerGroupDB.IsActive,
                Name = customerGroupDB.Name,
                EnableDiscount = customerGroupDB.EnableDiscount,
                DiscountPercent = customerGroupDB.DiscountPercent,
                CreationTime = customerGroupDB.CreationTime.UtcDateTime,
                CustomerGroupDiscountType = customerGroupDB.CustomerGroupDiscountType,
                CustomerCount = customerGroupDB.CustomerCount,
                LastModificationTime = customerGroupDB.LastModificationTime.UtcDateTime,
                UploadFileName = customerGroupDB.UploadFileName

            };
            if (customerGroupDB.PriceBookLists != null)
                customerGroupDto.PriceBookLists = new ObservableCollection<GroupPriceDto>(customerGroupDB.PriceBookLists.Select(a => GroupPriceDto.FromModel(a))?.ToList());

            return customerGroupDto;
        }
    }

    public partial class CustomerGroupDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public bool EnableDiscount { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public int CustomerGroupDiscountType { get; set; }
        public int CustomerCount { get; set; }
        public DateTimeOffset LastModificationTime { get; set; }
        public string UploadFileName { get; set; }
        public IList<GroupPriceDB> PriceBookLists { get;}
    }

}
