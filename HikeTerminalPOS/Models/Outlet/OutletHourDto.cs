using System;
using HikePOS.Enums;

namespace HikePOS.Models
{
	public class OutletHourDto : FullAuditedPassiveEntityDto
	{
		public int OutletId { get; set; }
		public string DayOfWeek { get; set; }
		public WeekDay WeekDay { get; set; }
		public TimeSpan OpenTime { get; set; }
		public TimeSpan CloseTime { get; set; }
		public bool IsOpen { get; set; }
	}
}
