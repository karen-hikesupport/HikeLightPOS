using System;
using System.Threading.Tasks;
using Refit;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS.Services
{
	public interface INavigationService
	{
		Page GetCurrentPage { get; }

		AppShell MainPage { get; }

		bool IsFlyoutPage { get; }

		INavigation Navigation { get; }

		Page NavigatedPage  { get; }
		
		Page RootPage { get; }

		Page CurrentPage { get; }
		bool IsCurrentPage<T>() where T : BaseViewModel;

	}
}
