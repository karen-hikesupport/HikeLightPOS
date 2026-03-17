using System;
using System.Threading.Tasks;
using Refit;
using HikePOS.Models;

namespace HikePOS.Services
{
	public static class ServiceLocator
	{
		public static IServiceProvider Provider { get; set; }

		public static T Get<T>() => Provider.GetRequiredService<T>();
	}
}
