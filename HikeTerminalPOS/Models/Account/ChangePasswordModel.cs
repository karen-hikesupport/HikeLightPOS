using System;
namespace HikePOS.Models
{
	public class ChangePasswordModel
	{
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
		public string RepeatNewPassword { get; set; }

	}
}

