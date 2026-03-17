using System;
using HikePOS.Enums;
using Newtonsoft.Json;

namespace HikePOS.Models
{
    public class EnterSaleItemDto : FullAuditedPassiveEntityDto
    {
        public EnterSaleItemDto()
        {
        }

        public InvoiceItemType ItemType { get; set; }
        public ProductDto_POS Product { get; set; }
        public OfferDto Offer { get; set; }
        public CategoryDto Category { get; set; }

        // Start #90945 iOS:FR Hot keys: POS Process Sale Window By Pratik
        [JsonIgnore]
        public bool ISLayout { get; set; }
        // End #90945 By Pratik

    }
}
