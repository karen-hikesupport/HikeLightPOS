using System;
namespace HikePOS.Services
{
	public interface IMPOPStarBarcode
	{
		void StartService();
		void CloseService();
	}
}
