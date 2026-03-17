using System;
using HikePOS.Models;
using Refit;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using HikePOS.Helpers;
using Fusillade;
using Polly;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using HikePOS.ViewModels;

namespace HikePOS.Services
{
	public class NavigationService : INavigationService
	{
		public Page GetCurrentPage => App.Instance.MainPage;

		public AppShell MainPage => App.Instance.MainPage as AppShell;

		public INavigation Navigation => Shell.Current.Navigation;

		public Page NavigatedPage => (Navigation.NavigationStack == null || Navigation.NavigationStack.Count == 0) ? null : (Navigation.NavigationStack.Count == 1 ? RootPage : Navigation.NavigationStack.Last());

		public Page CurrentPage => Shell.Current?.CurrentPage;

		public Page RootPage => (Shell.Current?.CurrentItem?.CurrentItem?.CurrentItem as IShellContentController)?.Page;

		public bool IsFlyoutPage => App.Instance.MainPage is AppShell;

		public bool IsCurrentPage<T>() where T : BaseViewModel
		{
			return IsFlyoutPage && CurrentPage != null && CurrentPage.BindingContext?.GetType() == typeof(T);
		}

	}
}
