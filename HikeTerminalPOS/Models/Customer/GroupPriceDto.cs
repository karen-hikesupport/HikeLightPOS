using System;
using System.Collections.ObjectModel;
using Realms;
namespace HikePOS.Models
{
    public class GroupPriceDto : FullAuditedPassiveEntityDto
    {
        public string PriceBookName { get; set; }
        public bool IsInStore { get; set; }
        public bool IsInECommerce { get; set; }
        public bool IsForAllOutlets { get; set; }
        public Int32 CustomerGroupId { get; set; }
        public string CustomerGroupName { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string UploadFileName { get; set; }
        public int CustomerDiscoutType { get; set; }
        public Int32 DiscountPercent { get; set; }
        public string AllPriceBookOutletsTitle { get; set; }
        public ObservableCollection<GroupPriceListOutletDto> PriceBookOutlets { get; set; }
        public ObservableCollection<GroupPriceTemplateDto> PricebookTemplates { get; set; }
        public GroupPriceDB ToModel()
        {
            GroupPriceDB groupPriceDB = new GroupPriceDB
            {
                Id = Id,
                IsActive = IsActive,
                PriceBookName = PriceBookName,
                IsInStore = IsInStore,
                IsInECommerce = IsInECommerce,
                IsForAllOutlets = IsForAllOutlets,
                CustomerGroupId = CustomerGroupId,
                CustomerGroupName = CustomerGroupName,
                ValidFrom = ValidFrom,
                ValidTo = ValidTo,
                UploadFileName = UploadFileName,
                CustomerDiscoutType = CustomerDiscoutType,
                DiscountPercent = DiscountPercent,
                AllPriceBookOutletsTitle = AllPriceBookOutletsTitle
            };
           
            PriceBookOutlets?.ForEach(i => groupPriceDB.PriceBookOutlets.Add(i.ToModel()));
            PricebookTemplates?.ForEach(i => groupPriceDB.PricebookTemplates.Add(i.ToModel()));

            return groupPriceDB;
        }
        public static GroupPriceDto FromModel(GroupPriceDB groupPriceDB)
        {
            GroupPriceDto groupPriceDto = new GroupPriceDto
            {
                Id = groupPriceDB.Id,
                IsActive = groupPriceDB.IsActive,
                PriceBookName = groupPriceDB.PriceBookName,
                IsInStore = groupPriceDB.IsInStore,
                IsInECommerce = groupPriceDB.IsInECommerce,
                IsForAllOutlets = groupPriceDB.IsForAllOutlets,
                CustomerGroupId = groupPriceDB.CustomerGroupId,
                CustomerGroupName = groupPriceDB.CustomerGroupName,
                ValidFrom = groupPriceDB.ValidFrom?.UtcDateTime,
                ValidTo = groupPriceDB.ValidTo?.UtcDateTime,
                UploadFileName = groupPriceDB.UploadFileName,
                CustomerDiscoutType = groupPriceDB.CustomerDiscoutType,
                DiscountPercent = groupPriceDB.DiscountPercent,
                AllPriceBookOutletsTitle = groupPriceDB.AllPriceBookOutletsTitle

            };
            
            if (groupPriceDB.PriceBookOutlets != null)
                groupPriceDto.PriceBookOutlets = new ObservableCollection<GroupPriceListOutletDto>(groupPriceDB.PriceBookOutlets.Select(a => GroupPriceListOutletDto.FromModel(a)));

            if (groupPriceDB.PricebookTemplates != null)
                groupPriceDto.PricebookTemplates = new ObservableCollection<GroupPriceTemplateDto>(groupPriceDB.PricebookTemplates.Select(a => GroupPriceTemplateDto.FromModel(a)));

            return groupPriceDto;
        }

    }
    public partial class GroupPriceDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string PriceBookName { get; set; }
        public bool IsInStore { get; set; }
        public bool IsInECommerce { get; set; }
        public bool IsForAllOutlets { get; set; }
        public Int32 CustomerGroupId { get; set; }
        public string CustomerGroupName { get; set; }
        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }
        public string UploadFileName { get; set; }
        public int CustomerDiscoutType { get; set; }
        public Int32 DiscountPercent { get; set; }
        public string AllPriceBookOutletsTitle { get; set; }
        public IList<GroupPriceListOutletDB> PriceBookOutlets { get;}
        public IList<GroupPriceTemplateDB> PricebookTemplates { get;}
    }

}
