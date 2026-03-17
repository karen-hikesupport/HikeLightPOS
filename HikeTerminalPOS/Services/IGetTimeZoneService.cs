using System;

namespace HikePOS.Services
{
	public interface IGetTimeZoneService
	{
		TimeZoneInfo getTimeZoneInfo(string id);
	}
}
