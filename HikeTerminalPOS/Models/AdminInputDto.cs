using System;
using HikePOS.Helpers;
namespace HikePOS
{
	public class AdminInputDto
	{
		public int targetTenantId { get; set; } 
		public int targetUserId { get; set; } 
		public AdminInputDto() { 
			
		}
	}

	public class VerifySecurityCodeDto
	{
		public string Provider { get; set; } 
		public string Code { get; set; } 
		public string ReturnUrl { get; set; }
		public bool RememberBrowser { get; set; }
		public bool RememberMe { get; set; }
		public bool IsRememberBrowserEnabled { get; set; }
	}
}
