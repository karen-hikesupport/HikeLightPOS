using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Payment;

namespace HikePOS
{
	public interface IVantiv
	{
        void MakeSale(string amount, string registerId, VantivConfigurationDto ConfigurationModel);
        void MakeRefund(string amount, string registerId, VantivConfigurationDto ConfigurationModel);
        Task<bool> DeviceConfigure(VantivConfigurationDto ConfigurationModel);
		void Cancel();
	}
}
