using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace HikePOS.Models
{
	public class OutletListDto : FullAuditedPassiveEntityDto
	{

		public OutletListDto()
		{
			OutletHours = new ObservableCollection<OutletHourDto>();
			OutletRegisters = new ObservableCollection<RegisterDto>();
		}

		public string Title { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public AddressDto Address { get; set; }
		public int? TaxId { get; set; }
		public string TaxName { get; set; }
		public int? ReceiptTemplateId { get; set; }
		public string ReceiptTemplateName { get; set; }
		public ReceiptTemplateDto ReceiptTemplate { get; set; }
		public ObservableCollection<RegisterDto> OutletRegisters { get; set; }
		public ObservableCollection<OutletHourDto> OutletHours { get; set; }

        [JsonIgnore]
        bool _IsSelected { get; set; } = false;
        //[JsonIgnore]
        public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; SetPropertyChanged(nameof(IsSelected));} }

	}
}
