using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using HikePOS.Enums;
using HikePOS.Helpers;
using System.Linq;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public class ProductDto_POS : FullAuditedPassiveEntityDto
    {
        public ProductDto_POS()
        {
            ProductAttributes = new ObservableCollection<ProductAttributeDto>();
            // ProductCategoryDtos = new ObservableCollection<CategoryDto>();
            ProductVarients = new ObservableCollection<ProductVarientsDto>();
            ProductOutlet = new ProductOutletDto_POS();
            ProductOutlets = new ObservableCollection<ProductOutletDto_POS>();
            ProductImages = new ObservableCollection<ProductImageDto>();
            ProductSuppliers = new ObservableCollection<ProductSupplierDto>();
            ServiceUsers = new ObservableCollection<ServiceUserDto>();
        }

        public int? ParentId { get; set; }

        [JsonIgnore]
        string _Name { get; set; }
        public string Name { get { return _Name; } set { _Name = value; SetPropertyChanged(nameof(Name)); } }

        //Start #81159 Pratik
        public string ShortName
        {
            get
            {
                if (ItemImage.HasValue)
                    return string.Empty;

                return (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.DisplayFullProductNameOnPOS)
                    ? Name
                    : CommonMethods.GetProductName(Name);
            }
        }

       //End #81159 Pratik

        public string Description { get; set; }
        public string Sku { get; set; }
        public string BarCode { get; set; }
        public string Specification { get; set; }
        public decimal Depth { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public int? WeightUnit { get; set; }

        [JsonIgnore]
        Guid? _ItemImage { get; set; }
        public Guid? ItemImage { get { return _ItemImage; } set { _ItemImage = value; SetPropertyChanged(nameof(ItemImage)); } }

        //Ticket #11252 Start : Product images are not showing on POS. By Nikhil 
        [JsonIgnore]
        string _ItemImageUrl { get; set; }
        public string ItemImageUrl { get { return _ItemImageUrl; } set { _ItemImageUrl = value; SetPropertyChanged(nameof(ItemImageUrl)); } }

        [JsonIgnore]
        public string FullItemImageUrl =>  ItemImageUrl?.GetImageUrl("Product_Medium_Entersale")?.ToString() ?? string.Empty;
        //Ticket #11252 End. By Nikhil

        public string ColorTag { get; set; }

        [JsonIgnore]
        ColorType? _ColorType { get; set; }

        public ColorType? ColorType { get { return _ColorType; } set { _ColorType = value; SetPropertyChanged(nameof(ColorType)); } }

        public int? BrandId { get; set; }
        public string BranName { get; set; }
        public int? SeasonId { get; set; }
        public string SeasonName { get; set; }

        public int? TagId { get; set; }
        public string TagName { get; set; }

        //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
        [JsonIgnore]
        public bool ISLayout { get; set; }

        [JsonIgnore]
        public ColorType? LayoutColor { get; set; }
        [JsonIgnore]
        public string LayoutId { get; set; }

        [JsonIgnore]
        public int? ProductsCount { get; set; }

        [JsonIgnore]
        public bool ProductDeleted { get; set; }
        //End #90945 By Pratik

        public bool EnableAccountSyncRule { get; set; }
        public string SupplierCode { get; set; }
        public string SalesCode { get; set; }
        public string PurchaseCode { get; set; }
        public string InventoryAssertCode { get; set; }
        public string CustomField { get; set; }

        public bool IsPricingDifferentByOutlet { get; set; }

        public ProductType ProductType { get; set; }
        public bool HasVarients { get; set; }
        public bool TrackInventory { get; set; }
        public bool EnableSerialNumber { get; set; }

        public bool ShowStockCountOnEnterSale
        {
            get
            {
                if (Settings.StoreGeneralRule != null)
                {
                    return (Settings.StoreGeneralRule.ShowStockCountOnEnterSale && TrackInventory && !IsUnitOfMeasure);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool AllowOutOfStock { get; set; }
        public Decimal Loyalty { get; set; }
        public int ServiceDuration { get; set; }
        public string PrinterLocationID { get; set; }

        [JsonIgnore]
        bool _IsInAnyOffer { get; set; }

        public bool IsInAnyOffer { get { return _IsInAnyOffer; } set { _IsInAnyOffer = value; SetPropertyChanged(nameof(IsInAnyOffer)); } }


        public bool CanEditableInThirdParty { get; set; }
        public bool EnableSeo { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDesc { get; set; }
        public string MetaKeyword { get; set; }
        public string Slug { get; set; }

        [JsonIgnore]
        bool _isSelected { get; set; }

        public bool isSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                SetPropertyChanged(nameof(isSelected));
            }
        }

        public ObservableCollection<int> ProductCategories { get; set; }
        // public ObservableCollection<CategoryDto> ProductCategoryDtos { get; set; }

        public ObservableCollection<int> ProductTags { get; set; }

        public ObservableCollection<int> ProductExtras { get; set; }

        public ObservableCollection<int> SalesChannels { get; set; }


        public ObservableCollection<VariantAttributeValueDto> VariantAttributesValues { get; set; }

        public ObservableCollection<ProductVarientsDto> ProductVarients { get; set; }

        ObservableCollection<ProductAttributeDto> _ProductAttributes { get; set; }
        public ObservableCollection<ProductAttributeDto> ProductAttributes { get { return _ProductAttributes; } set { _ProductAttributes = value; SetPropertyChanged(nameof(ProductAttributes)); } }


        public ObservableCollection<ServiceUserDto> ServiceUsers { get; set; }
        public ObservableCollection<ProductImageDto> ProductImages { get; set; }
        public ObservableCollection<ProductSupplierDto> ProductSuppliers { get; set; }

        [JsonIgnore]
        ProductOutletDto_POS _ProductOutlet { get; set; }
        public ProductOutletDto_POS ProductOutlet { get { return _ProductOutlet; } set { _ProductOutlet = value; SetPropertyChanged(nameof(ProductOutlet)); SetPropertyChanged(nameof(Stock)); } }

        //For signalR call
        [JsonIgnore]
        ObservableCollection<ProductOutletDto_POS> _productOutlets { get; set; }
        public ObservableCollection<ProductOutletDto_POS> ProductOutlets { get { return _productOutlets; } set { _productOutlets = value; SetPropertyChanged(nameof(ProductOutlets)); } }


        [JsonIgnore]
        public string Stock
        {
            get
            {
                //if (HasVarients && ProductVarients != null)
                //{
                //    decimal stock = 0;
                //    foreach (var item in ProductVarients)
                //    {
                //        if (item.VariantOutlet != null)
                //        {
                //            stock += (item.VariantOutlet.Stock);
                //        }
                //    }
                //    return stock.ToKMFormat();
                //}
                //else
                //{
                var stockstr = ProductOutlet != null ? (ProductOutlet.Stock).ToKMFormat() : "0";
                return stockstr;
                //}
            }
        }
        //Ticket start:#22898 Composite sale not working properly.by rupesh
        public ObservableCollection<ProductCompositeItem_POS_Dto> ProductCompositeItems { get; set; }
        //Ticket end:#22898 .by rupesh

        //Ticket start#26322 iPad - Composite product issue.by rupesh
        public bool DisableSellIndividually { get; set; }
        //Ticket end#26322 .by rupesh

        //Start #84444 iOS: FR: remove item price at search bar that includes tax by Pratik
        [JsonIgnore]
        public decimal UOMSearchPrice { get; set; }
        //End #84444 by Pratik

        //Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
        [JsonIgnore]
        public bool IsUnitOfMeasure { get; set; }
        [JsonIgnore]
        public ProductUnitOfMeasureDto ProductUnitOfMeasureDto { get; set; }
        [JsonIgnore]
        //Ticket start:#26980 iPad: UOM product price should be showing in search option.by rupesh
        public decimal UOMSellingPrice { get; set; }
        //Ticket end:#26980 iPad: UOM product price should be showing in search option.by rupesh
        //Ticket end:#20064 .by rupesh
        //Ticket start:#45382 iPad: FR - Non-Discountable items.by rupesh
        public bool DisableDiscountIndividually { get; set; }
        //Ticket endd:#45382 .by rupesh
        //Start:#63489 IOS: Items Not sequence wise in print receipt.by rupesh
        [JsonIgnore]
        public DateTime SelectionDateTime { get; set; }
        //Start:#63489 .by rupesh

        //Ticket:start:#71296 iPad - Feature: Product Loyalty point exclusion.by rupesh
        public bool DisableAdditionalLoyalty { get; set; }
        //Ticket:end:#71296 .by rupesh

        //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
        public bool AgeVerificationToSellProduct { get; set; }

        public int AgeVerificationLimit { get; set; }
        //Ticket:end:#90938,#94423 .by rupesh

        public ProductDB ToModel()
        {
            ProductDB producDB = new ProductDB
            {
                Id = Id,
                IsActive = IsActive,
                ParentId = ParentId,
                Name = Name,
                Description = Description,
                Sku = Sku,
                BarCode = BarCode,
                Specification = Specification,
                Depth = Depth,
                Width = Width,
                Height = Height,
                Weight = Weight,
                WeightUnit = WeightUnit,
                ItemImage = ItemImage,
                ItemImageUrl = ItemImageUrl,
                ColorTag = ColorTag,
                ColorType = ColorType.HasValue ? (int)ColorType.Value : null,
                BrandId = BrandId,
                BranName = BranName,
                SeasonId = SeasonId,
                SeasonName = SeasonName,
                TagId = TagId,
                TagName = TagName,
                EnableAccountSyncRule = EnableAccountSyncRule,
                SupplierCode = SupplierCode,
                SalesCode = SalesCode,
                PurchaseCode = PurchaseCode,
                InventoryAssertCode = InventoryAssertCode,
                CustomField = CustomField,
                IsPricingDifferentByOutlet = IsPricingDifferentByOutlet,
                ProductType = (int)ProductType,
                HasVarients = HasVarients,
                TrackInventory = TrackInventory,
                EnableSerialNumber = EnableSerialNumber,
                EnableSeo = EnableSeo,
                AllowOutOfStock = AllowOutOfStock,
                Loyalty = Loyalty,
                ServiceDuration = ServiceDuration,
                PrinterLocationID = PrinterLocationID,
                IsInAnyOffer = IsInAnyOffer,
                CanEditableInThirdParty = CanEditableInThirdParty,
                MetaKeyword = MetaKeyword,
                MetaDesc = MetaDesc,
                MetaTitle = MetaTitle,
                Slug = Slug,
                isSelected = isSelected,
                DisableSellIndividually = DisableSellIndividually,
                DisableDiscountIndividually = DisableDiscountIndividually,
                ProductOutlet = ProductOutlet.ToModel(),
                DisableAdditionalLoyalty = DisableAdditionalLoyalty,
                AgeVerificationToSellProduct = AgeVerificationToSellProduct,
                AgeVerificationLimit = AgeVerificationLimit
            };

            ProductCategories?.ForEach(i => producDB.ProductCategories.Add(i));
            ProductTags?.ForEach(i => producDB.ProductTags.Add(i));
            ProductExtras?.ForEach(i => producDB.ProductExtras.Add(i));
            SalesChannels?.ForEach(i => producDB.SalesChannels.Add(i));
            // ProductCategoryDtos?.ForEach(i => producDB.ProductCategoryDtos.Add(i.ToModel()));
            VariantAttributesValues?.ForEach(i => producDB.VariantAttributesValues.Add(i));
            ProductVarients?.ForEach(i => producDB.ProductVarients.Add(i.ToModel()));
            ProductAttributes?.ForEach(i => producDB.ProductAttributes.Add(i.ToModel()));
            ServiceUsers?.ForEach(i => producDB.ServiceUsers.Add(i));
            ProductImages?.ForEach(i => producDB.ProductImages.Add(i));
            ProductSuppliers?.ForEach(i => producDB.ProductSuppliers.Add(i));
            ProductOutlets?.ForEach(i => producDB.ProductOutlets.Add(i.ToModel()));
            ProductCompositeItems?.ForEach(i => producDB.ProductCompositeItems.Add(i.ToModel()));
            if (ProductCategories != null && ProductCategories.Any())
                producDB.ProductCategoriesDesc = "," + string.Join(",", ProductCategories.ToArray()) + ",";
            else
                producDB.ProductCategoriesDesc = "";
            if (SalesChannels != null && SalesChannels.Any())
                producDB.SalesChannelsStr = "," + string.Join(",", SalesChannels.ToArray()) + ",";
            else
                producDB.SalesChannelsStr = "";
            return producDB;
        }

        public static ProductDto_POS FromModel(ProductDB producDB)
        {
            if (producDB == null)
                return null;

            ProductDto_POS productDto = new ProductDto_POS
            {
                Id = producDB.Id,
                IsActive = producDB.IsActive,
                ParentId = producDB.ParentId,
                Name = producDB.Name,
                Description = producDB.Description,
                Sku = producDB.Sku,
                BarCode = producDB.BarCode,
                Specification = producDB.Specification,
                Depth = producDB.Depth,
                Width = producDB.Width,
                Height = producDB.Height,
                Weight = producDB.Weight,
                WeightUnit = producDB.WeightUnit,
                ItemImage = producDB.ItemImage,
                ItemImageUrl = producDB.ItemImageUrl,
                ColorTag = producDB.ColorTag,
                ColorType = producDB.ColorType.HasValue ? (ColorType)producDB.ColorType : null,
                BrandId = producDB.BrandId,
                BranName = producDB.BranName,
                SeasonId = producDB.SeasonId,
                SeasonName = producDB.SeasonName,
                TagId = producDB.TagId,
                TagName = producDB.TagName,
                EnableAccountSyncRule = producDB.EnableAccountSyncRule,
                SupplierCode = producDB.SupplierCode,
                SalesCode = producDB.SalesCode,
                PurchaseCode = producDB.PurchaseCode,
                InventoryAssertCode = producDB.InventoryAssertCode,
                CustomField = producDB.CustomField,
                IsPricingDifferentByOutlet = producDB.IsPricingDifferentByOutlet,
                ProductType = (ProductType)producDB.ProductType,
                HasVarients = producDB.HasVarients,
                TrackInventory = producDB.TrackInventory,
                EnableSerialNumber = producDB.EnableSerialNumber,
                EnableSeo = producDB.EnableSeo,
                AllowOutOfStock = producDB.AllowOutOfStock,
                Loyalty = producDB.Loyalty,
                ServiceDuration = producDB.ServiceDuration,
                PrinterLocationID = producDB.PrinterLocationID,
                IsInAnyOffer = producDB.IsInAnyOffer,
                CanEditableInThirdParty = producDB.CanEditableInThirdParty,
                MetaKeyword = producDB.MetaKeyword,
                MetaDesc = producDB.MetaDesc,
                MetaTitle = producDB.MetaTitle,
                Slug = producDB.Slug,
                isSelected = producDB.isSelected,
                DisableSellIndividually = producDB.DisableSellIndividually,
                DisableDiscountIndividually = producDB.DisableDiscountIndividually,
                ProductOutlet = ProductOutletDto_POS.FromModel(producDB.ProductOutlet),
                DisableAdditionalLoyalty = producDB.DisableAdditionalLoyalty,
                AgeVerificationToSellProduct = producDB.AgeVerificationToSellProduct,
                AgeVerificationLimit = producDB.AgeVerificationLimit
            };

            productDto.ProductCategories = new ObservableCollection<int>(producDB.ProductCategories);
            productDto.ProductTags = new ObservableCollection<int>(producDB.ProductTags);
            productDto.ProductExtras = new ObservableCollection<int>(producDB.ProductExtras);
            productDto.SalesChannels = new ObservableCollection<int>(producDB.SalesChannels);
            productDto.VariantAttributesValues = new ObservableCollection<VariantAttributeValueDto>(producDB.VariantAttributesValues);
            productDto.ServiceUsers = new ObservableCollection<ServiceUserDto>(producDB.ServiceUsers);
            productDto.ProductImages = new ObservableCollection<ProductImageDto>(producDB.ProductImages);
            productDto.ProductSuppliers = new ObservableCollection<ProductSupplierDto>(producDB.ProductSuppliers);
            productDto.ProductOutlets = new ObservableCollection<ProductOutletDto_POS>(producDB.ProductOutlets.Select(a => ProductOutletDto_POS.FromModel(a)));
            productDto.ProductCompositeItems = new ObservableCollection<ProductCompositeItem_POS_Dto>(producDB.ProductCompositeItems.Select(a => ProductCompositeItem_POS_Dto.FromModel(a)));
            //  productDto.ProductCategoryDtos = new ObservableCollection<CategoryDto>(producDB.ProductCategoryDtos.ToList().Select(a => CategoryDto.FromModel(a)));
            productDto.ProductVarients = new ObservableCollection<ProductVarientsDto>(producDB.ProductVarients.Select(a => ProductVarientsDto.FromModel(a)));
            productDto.ProductAttributes = new ObservableCollection<ProductAttributeDto>(producDB.ProductAttributes.Select(a => ProductAttributeDto.FromModel(a)));

            return productDto;
        }

        public static ProductDto_POS FromBindModel(ProductDB producDB)
        {
            if (producDB == null)
                return null;

            ProductDto_POS productDto = new ProductDto_POS
            {
                Id = producDB.Id,
                Name = producDB.Name,
                Sku = producDB.Sku,
                BarCode = producDB.BarCode,
                ItemImage = producDB.ItemImage,
                ItemImageUrl = producDB.ItemImageUrl,
                ColorTag = producDB.ColorTag,
                ColorType = producDB.ColorType.HasValue ? (ColorType)producDB.ColorType : null,
                Specification = producDB.Specification
            };
            return productDto;
        }
    }
    public class ProductFilterModel : PagedSortedAndFilteredInputDto
    {
        public DateTime modifiedDateTime { get; set; }
        public int supplierId { get; set; }
        public int outletId { get; set; }
    }
    public class GetProductDetailInput
    {
        public int id { get; set; }
    }
    public class ProductPubnub
    {
        public int ProductId { get; set; }
        public bool IsAnyOffer { get; set; }
    }

    public partial class ProductCompositeItem_POS_Dto
    {
        public int CompositeProductId { get; set; }
        public decimal? Qty { get; set; }
        //Ticket start:#84291 iOS:FR: composite change. by rupesh
        public decimal? Discount { get; set; }
        public decimal? IndividualPrice { get; set; }
        //Ticket end:#84291 . by rupesh

        public ProductCompositeItemDB ToModel()
        {
            ProductCompositeItemDB model = new ProductCompositeItemDB
            {
                CompositeProductId = CompositeProductId,
                Qty = Qty,
                //Ticket start:#84291 iOS:FR: composite change. by rupesh
                Discount = Discount,
                IndividualPrice = IndividualPrice
                //Ticket end:#84291 . by rupesh

            };
            return model;
        }
        public static ProductCompositeItem_POS_Dto FromModel(ProductCompositeItemDB dbmodel)
        {
            if (dbmodel == null)
                return null;
            ProductCompositeItem_POS_Dto model = new ProductCompositeItem_POS_Dto
            {
                CompositeProductId = dbmodel.CompositeProductId,
                Qty = dbmodel.Qty,
                //Ticket start:#84291 iOS:FR: composite change. by rupesh
                Discount = dbmodel.Discount,
                IndividualPrice = dbmodel.IndividualPrice
                //Ticket end:#84291 . by rupesh
            };
            return model;
        }

    }

    public partial class ProductCompositeItemDB : IRealmObject
    {
        public int CompositeProductId { get; set; }
        public decimal? Qty { get; set; }
        //Ticket start:#84291 iOS:FR: composite change. by rupesh
        public decimal? Discount { get; set; }
        public decimal? IndividualPrice { get; set; }
        //Ticket end:#84291 . by rupesh

    }

    public partial class ProductDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Sku { get; set; }
        public string BarCode { get; set; }
        public string Specification { get; set; }
        public decimal Depth { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public int? WeightUnit { get; set; }
        public Guid? ItemImage { get; set; }
        public string ItemImageUrl { get; set; }
        public string ColorTag { get; set; }
        public int? ColorType { get; set; }
        public int? BrandId { get; set; }
        public string BranName { get; set; }
        public int? SeasonId { get; set; }
        public string SeasonName { get; set; }
        public int? TagId { get; set; }
        public string TagName { get; set; }
        public bool EnableAccountSyncRule { get; set; }
        public string SupplierCode { get; set; }
        public string SalesCode { get; set; }
        public string PurchaseCode { get; set; }
        public string InventoryAssertCode { get; set; }
        public string CustomField { get; set; }
        public bool IsPricingDifferentByOutlet { get; set; }
        public int ProductType { get; set; }
        public bool HasVarients { get; set; }
        public bool TrackInventory { get; set; }
        public bool EnableSerialNumber { get; set; }
        public bool AllowOutOfStock { get; set; }
        public Decimal Loyalty { get; set; }
        public int ServiceDuration { get; set; }
        public string PrinterLocationID { get; set; }

        public bool IsInAnyOffer { get; set; }

        public bool CanEditableInThirdParty { get; set; }
        public bool EnableSeo { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDesc { get; set; }
        public string MetaKeyword { get; set; }
        public string Slug { get; set; }

        public bool isSelected { get; set; }
        public string ProductCategoriesDesc { get; set; }
        public string SalesChannelsStr { get; set; }
        public IList<int> ProductCategories { get; }
        // public IList<CategoryDB> ProductCategoryDtos { get; }

        public IList<int> ProductTags { get; }

        public IList<int> ProductExtras { get; }

        public IList<int> SalesChannels { get; }


        public IList<VariantAttributeValueDto> VariantAttributesValues { get; }

        public IList<ProductVarientsDB> ProductVarients { get; }

        public IList<ProductAttributeDB> ProductAttributes { get; }

        public IList<ServiceUserDto> ServiceUsers { get; }
        public IList<ProductImageDto> ProductImages { get; }
        public IList<ProductSupplierDto> ProductSuppliers { get; }

        public ProductOutletDB_POS ProductOutlet { get; set; }

        public IList<ProductOutletDB_POS> ProductOutlets { get; }

        public IList<ProductCompositeItemDB> ProductCompositeItems { get; }
        public bool DisableSellIndividually { get; set; }
        public bool DisableDiscountIndividually { get; set; }
        public bool DisableAdditionalLoyalty { get; set; }
        //Ticket:start:#90938,#94423 IOS:FR Age varification.by rupesh
        public bool AgeVerificationToSellProduct { get; set; }
        public int AgeVerificationLimit { get; set; }
        //Ticket:end:#90938,#94423 .by rupesh

    }

    //Start #71295 iPad- Feature: Converting Quote to Sale - Insufficient Stock By Pratik
    public class ProductQuote
    {
        public String Name { get; set; }
        public String Sku { get; set; }
        public String Stoke { get; set; }
        public String Order { get; set; }
    }
    //End #71295
}