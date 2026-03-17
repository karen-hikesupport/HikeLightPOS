using System;
namespace HikePOS.Models
{
	public class CreateTenantInputModel
	{
		public string TenancyName { get; set; }

		public string StoreName { get; set; }

		public string AdminEmailAddress { get; set; }

		public string AdminPassword { get; set; }

	}


}
