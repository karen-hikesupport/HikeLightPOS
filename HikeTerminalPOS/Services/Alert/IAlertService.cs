using System;
using System.Threading.Tasks;
using HikePOS.Models;
using Refit;

namespace HikePOS.Services
{
	public interface IAlertService
	{
		Task ShowAlert(string title, string message, string cancel);
		Task<bool> ShowAlert(string title, string message, string accept, string cancel);
		Task<bool> ShowFulfillmentAlert(string title, string message, string accept, string cancel);

	}

}
