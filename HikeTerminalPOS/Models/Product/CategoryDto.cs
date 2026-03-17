using System;
using System.Collections.ObjectModel;
using HikePOS.Enums;
using HikePOS.Helpers;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
    public class CategoryDto : FullAuditedPassiveEntityDto
    {
        public CategoryDto()
        {
            SubCategories = new ObservableCollection<CategoryDto>();
        }

        [JsonIgnore]
        string _Name { get; set; }

        public string Name { get { return _Name; } set { _Name = value; SetPropertyChanged(nameof(Name)); } }

        [JsonIgnore]
        public bool DisplayName => CategoryImageId.HasValue == false ? (Settings.StoreGeneralRule != null ? Settings.StoreGeneralRule.displayFullCategoryNameOnPOS : false) : false; //Start #93286 by Pratik
        
        [JsonIgnore]
        public string ShortName
        {
            get
            {
                if (CategoryImageId.HasValue)
                    return string.Empty;

                return (Settings.StoreGeneralRule != null && !Settings.StoreGeneralRule.displayFullCategoryNameOnPOS)
                    ? CommonMethods.GetProductName(Name)
                    : Name;
            }
        }
        
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public string Description { get; set; }
        public int sequence { get; set; }
        public Guid? CategoryImageId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public int SubCategoryCount { get; set; }

        // Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
        [JsonIgnore]
        public bool ISLayout { get; set; }

        [JsonIgnore]
        public string LayoutId { get; set; }

        [JsonIgnore]
        public ColorType? LayoutColor { get; set; }

        [JsonIgnore]
        public bool DisplayFolder => !string.IsNullOrEmpty(LayoutId) && ISLayout;

        // End #90945 By Pratik

        [JsonIgnore]
        int _ProductsCount { get; set; }
        public int ProductsCount { get { return _ProductsCount; } set { _ProductsCount = value; SetPropertyChanged(nameof(ProductsCount)); } }

        public ObservableCollection<CategoryDto> SubCategories { get; set; }

        [JsonIgnore]
        bool _IsSelected { get; set; } = false;

        //[JsonIgnore]
        public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; SetPropertyChanged(nameof(IsSelected)); } }

        [JsonIgnore]
        bool _isLoad { get; set; } = false;

        [JsonIgnore]
        public bool IsLoad { get { return _isLoad; } set { _isLoad = value; SetPropertyChanged(nameof(IsLoad)); } }


        public CategoryDB ToModel()
        {
            CategoryDB categoryDB = new CategoryDB
            {
                 Name = Name,
                 ParentId = ParentId,
                 ParentName = ParentName,
                 Description = Description,
                 sequence = sequence,
                 CategoryImageId = CategoryImageId,
                 CreationTime = CreationTime,
                 LastModificationTime = LastModificationTime,
                 SubCategoryCount = SubCategoryCount,
                 ProductsCount = ProductsCount,
                 IsSelected = IsSelected,
                 IsActive = IsActive,
                 Id = Id
            };

            SubCategories.ForEach(i => categoryDB.SubCategories.Add(i.ToModel()));

            return categoryDB;
        }

        public static CategoryDto FromModel(CategoryDB list)
        {
            if (list == null)
                return null;

            CategoryDto categoryDto = new CategoryDto
            {
                Name = list.Name,
                ParentId = list.ParentId,
                ParentName = list.ParentName,
                Description = list.Description,
                sequence = list.sequence,
                CategoryImageId = list.CategoryImageId,
                CreationTime = list.CreationTime.UtcDateTime,
                LastModificationTime = list.LastModificationTime?.UtcDateTime,
                SubCategoryCount = list.SubCategoryCount,
                ProductsCount = list.ProductsCount,
                IsSelected = list.IsSelected,
                IsActive = list.IsActive,
                Id = list.Id
            };

            if(list.SubCategories != null)
                categoryDto.SubCategories = new ObservableCollection<CategoryDto>(list.SubCategories.Select(a=> CategoryDto.FromModel(a)));

            return categoryDto;
        }
    }

    public class ProductCategoryDto
	{
		public int CategoryId { get; set; }
		public string Name { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public int ParentCategoryId { get; set; }
	}


    public partial class CategoryDB : IRealmObject
    {
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public string Description { get; set; }
        public int sequence { get; set; }
        public Guid? CategoryImageId { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset? LastModificationTime { get; set; }
        public int SubCategoryCount { get; set; }

        public int ProductsCount { get; set; }
        public IList<CategoryDB> SubCategories { get; }

        public bool IsSelected { get; set; }

        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }

    }
}
