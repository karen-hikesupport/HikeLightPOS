using System.ComponentModel;
using HikePOS.Helpers;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
	public partial class ProductAttributeValueDto : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
		[PrimaryKey]
        public int ProductAttributeValueId { get; set; }
		public int ProductAttributeId { get; set; }
		public int AttributeId { get; set; }
		public int AttributeValueId { get; set; }
		public string Value { get; set; }

        [JsonIgnore]
        string _Stock { get; set; } = "";
        public string Stock { get { return _Stock; } set { _Stock = value; RaisePropertyChanged(nameof(Stock)); } }


        [JsonIgnore]
        bool _IsStockVisible { get; set; } = false;
        public bool IsStockVisible { get { return _IsStockVisible; } set { _IsStockVisible = value; RaisePropertyChanged(nameof(IsStockVisible)); } }
        
        [JsonIgnore]
		bool _IsSelected { get; set; } = false;
         
		public bool IsSelected
		{
			get { return _IsSelected; }
			set
			{
				_IsSelected = value;
                RaisePropertyChanged(nameof(IsSelected));
            }
		}

        [JsonIgnore]
		bool _IsEnable { get; set; } = true;
         
		public bool IsEnable
		{
			get { return _IsEnable; }
			set
			{
				_IsEnable = value;
                RaisePropertyChanged(nameof(IsEnable));
            }
		}

	}
}
