using System.Collections.ObjectModel;
using HikePOS.Enums;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    //Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
    public class InvoiceLayoutSellDto : FullAuditedPassiveEntityDto
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool SetAsCurrentLayout { get; set; }
        public bool IsDefault { get; set; }
        public ICollection<RegisterLayoutOptionDto> RegisterLayoutOptions { get; set; }
        public bool IsAllowToDelete { get; set; }
        public string NameToAllocateRegister { get; set; }

        public InvoiceLayoutSellDB ToModel()
        {
            InvoiceLayoutSellDB dB = new InvoiceLayoutSellDB
            {
                 Name = Name,
                 Value = Value,
                 SetAsCurrentLayout = SetAsCurrentLayout,
                 IsDefault = IsDefault,
                 IsAllowToDelete = IsAllowToDelete,
                 NameToAllocateRegister = NameToAllocateRegister,
                 IsActive = IsActive,
                 Id = Id
            };

            RegisterLayoutOptions.ForEach(i => dB.RegisterLayoutOptions.Add(i.ToModel()));

            return dB;
        }

        public static InvoiceLayoutSellDto FromModel(InvoiceLayoutSellDB list)
        {
            if (list == null)
                return null;

            InvoiceLayoutSellDto dto = new InvoiceLayoutSellDto
            {
                Name = list.Name, 
                Value = list.Value,
                SetAsCurrentLayout = list.SetAsCurrentLayout,
                IsDefault = list.IsDefault,
                IsAllowToDelete = list.IsAllowToDelete,
                NameToAllocateRegister = list.NameToAllocateRegister,
                IsActive = list.IsActive,
                Id = list.Id
            };

            if(list.RegisterLayoutOptions != null)
                dto.RegisterLayoutOptions = new ObservableCollection<RegisterLayoutOptionDto>(list.RegisterLayoutOptions.Select(a=> RegisterLayoutOptionDto.FromModel(a))?.ToList());

            return dto;
        }
  
    }

    public partial class InvoiceLayoutSellDB : IRealmObject
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool SetAsCurrentLayout { get; set; }
        public bool IsDefault { get; set; }
        public IList<RegisterLayoutOptionDB> RegisterLayoutOptions { get; }
        public bool IsAllowToDelete { get; set; }
        public string NameToAllocateRegister { get; set; }

        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class RegisterLayoutOptionDto
    {
        public string RegisterName { get; set; }
        public int RegisterId { get; set; }
        public string InvoiceLayoutSellName { get; set; }
        public int InvoiceLayoutSellId { get; set; }

        public RegisterLayoutOptionDB ToModel()
        {
            RegisterLayoutOptionDB dB = new RegisterLayoutOptionDB
            {
                 RegisterName = RegisterName,
                 RegisterId = RegisterId,
                 InvoiceLayoutSellName = InvoiceLayoutSellName,
                 InvoiceLayoutSellId = InvoiceLayoutSellId,
            };
            return dB;
        }

        public static RegisterLayoutOptionDto FromModel(RegisterLayoutOptionDB list)
        {
            if (list == null)
                return null;

            RegisterLayoutOptionDto dto = new RegisterLayoutOptionDto
            {
                RegisterName = list.RegisterName,
                RegisterId = list.RegisterId,
                InvoiceLayoutSellName = list.InvoiceLayoutSellName,
                InvoiceLayoutSellId = list.InvoiceLayoutSellId,
            };

            return dto;
        }

    }

    public partial class RegisterLayoutOptionDB : IRealmObject
    {
        public string RegisterName { get; set; }
        public int RegisterId { get; set; }
        public string InvoiceLayoutSellName { get; set; }

        [PrimaryKey]
        public int InvoiceLayoutSellId { get; set; }

    }

    public class LayoutOptionResponse
    {
        public string type { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string colorTypeProduct { get; set; }
        public List<List<LayoutOptionResponse>> columns { get; set; }
        public int? countProduct { get; set; }
        public string itemImage { get; set; }
        public bool? dragging { get; set; }
        public string itemImageUrl { get; set; }
        public ColorType? colorType { get; set; }
        public int? itemType { get; set; }
        public int? measureProductId { get; set; }
        public string sku { get; set; }
        public bool? isActive { get; set; }
        public int? parentId { get; set; }
        public bool? trackInventory { get; set; }
        public bool? hasVarients { get; set; }
        public string layoutProductName { get; set; }
        public int? productId { get; set; }

        public static ProductDto_POS ConvertToProductDto(LayoutOptionResponse optionResponse,ProductDto_POS productDtos)
        {
            ProductDto_POS productDto = new ProductDto_POS();
            int id = 0;
            if(optionResponse!= null && int.TryParse(optionResponse.id , out id) && id > 0 && productDtos != null)
            {
                if(optionResponse.itemType == 4)
                {
                    productDto.Name = string.IsNullOrEmpty(optionResponse.layoutProductName) ? optionResponse.name : optionResponse.layoutProductName ;
                    productDto.ISLayout = false;
                    productDto.LayoutId = optionResponse.id;
                    productDto.ProductsCount = optionResponse.countProduct;
                    if(!string.IsNullOrEmpty(optionResponse.colorTypeProduct) && System.Enum.TryParse(optionResponse.colorTypeProduct, out ColorType myStatus))
                        productDto.LayoutColor = myStatus;
                    else
                        productDto.LayoutColor = ColorType.Yellow;
                    productDto.ColorType = null;   
                    productDto.ProductDeleted = false;         
                    productDto.Id = optionResponse.measureProductId ?? 0;  
                    productDto.ItemImageUrl = optionResponse.itemImageUrl; 
                    productDto.IsUnitOfMeasure = true;    
                    if(!string.IsNullOrEmpty(optionResponse.itemImage))
                        productDto.ItemImage =  new Guid(optionResponse.itemImage);        
                }
                else
                {
                    productDto =  productDtos;  
                    productDto.Name = string.IsNullOrEmpty(optionResponse.layoutProductName) ? optionResponse.name : optionResponse.layoutProductName ;
                    if(!string.IsNullOrEmpty(optionResponse.colorTypeProduct) && System.Enum.TryParse(optionResponse.colorTypeProduct, out ColorType myStatus))
                        productDto.LayoutColor = myStatus;
                    else
                        productDto.LayoutColor = ColorType.Yellow;
                    productDto.ColorType = null;
                    productDto.ProductDeleted = false;
                } 

            }
            else if(optionResponse?.type != null && int.TryParse(optionResponse.id , out id) && id > 0 && optionResponse.type.ToLower() == "item" && productDtos == null)
            {
                productDto.Name =  string.IsNullOrEmpty(optionResponse.layoutProductName) ? optionResponse.name : optionResponse.layoutProductName ;
                productDto.ISLayout = false;
                productDto.LayoutId = optionResponse.id;
                productDto.ProductsCount = optionResponse.countProduct;
                if(!string.IsNullOrEmpty(optionResponse.colorTypeProduct) && System.Enum.TryParse(optionResponse.colorTypeProduct, out ColorType myStatus))
                    productDto.LayoutColor = myStatus;
                else
                    productDto.LayoutColor = ColorType.Yellow;
                productDto.ColorType = null;   
                productDto.ProductDeleted = true;         
                productDto.Id = string.IsNullOrEmpty(optionResponse.id) ? 0 : Convert.ToInt32(optionResponse.id);   
                productDto.ItemImageUrl = optionResponse.itemImageUrl;     
                if(!string.IsNullOrEmpty(optionResponse.itemImage))
                    productDto.ItemImage =  new Guid(optionResponse.itemImage);
            }
            else if(optionResponse?.type != null && optionResponse.type.ToLower() == "container")
            {
                productDto.Name =  optionResponse.name;
                productDto.ISLayout = true;
                productDto.LayoutId = optionResponse.id;
                productDto.ProductsCount = optionResponse.countProduct;
                 if(!string.IsNullOrEmpty(optionResponse.colorTypeProduct) && System.Enum.TryParse(optionResponse.colorTypeProduct, out ColorType myStatus))
                    productDto.ColorType = myStatus;
                else
                    productDto.ColorType = ColorType.Yellow;
                productDto.LayoutColor = productDto.ColorType;
                productDto.ProductDeleted = false; 
            }
            return productDto;
        }
    }

    //End #90945 By Pratik
}