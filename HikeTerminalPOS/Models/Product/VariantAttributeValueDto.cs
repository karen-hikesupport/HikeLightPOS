using System;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public partial class VariantAttributeValueDto : IRealmObject
    { 
        public int ProductAttributeId { get; set; }
        [PrimaryKey]
        public int ProductAttributeValueId { get; set; }
        public int AttributeId { get; set; }
        public int AttributeValueId { get; set; }
        public int Sequence { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
