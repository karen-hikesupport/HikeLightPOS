using System;
using HikePOS.Enums;
namespace HikePOS.Models
{
	public class BasicShopInfo : FullAuditedPassiveEntityDto
	{
		public string Name { get; set; }

		public string LastName { get; set; }

		public string Phone { get; set; }

		public string City { get; set; }

		public string State { get; set; }

		public string Country { get; set; }

		public string PostCode { get; set; }

		public string lattitude { get; set; }

		public string longitude { get; set; }

		public IndustryType? IndustryType { get; set; }

		public SellerBy? SellerBy { get; set; }

		public string regiontaxRate { get; set; }

		public string regiontaxName { get; set; }

		public bool IsExternalLogin { get; set; }

		bool _toBuildDemoData { get; set; }

		public bool ToBuildDemoData { get { return _toBuildDemoData;} set { _toBuildDemoData = value; SetPropertyChanged(nameof(ToBuildDemoData));} }

	}
}
