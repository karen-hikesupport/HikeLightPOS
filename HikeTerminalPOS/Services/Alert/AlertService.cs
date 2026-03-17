using System;
using HikePOS.Models;
using Refit;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using HikePOS.Helpers;
using Fusillade;
using Polly;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace HikePOS.Services
{
	public class AlertService : IAlertService
	{
	    private readonly INavigationService _navigationService;  
		public AlertService(INavigationService navigationService)
		{
			_navigationService = navigationService;
		}
		public async Task ShowAlert(string title, string message, string cancel)
		{
			// Use the current MainPage to show the alert
			await _navigationService.GetCurrentPage.DisplayAlert(title, message, cancel);
		}
		public async Task<bool> ShowAlert(string title, string message, string accept, string cancel)
		{
			return await _navigationService.GetCurrentPage.DisplayAlert(title, message,accept, cancel);
		}
		public async Task<bool> ShowFulfillmentAlert(string title, string message, string accept, string cancel)
		{
			#if IOS
				var window = UIKit.UIApplication.SharedApplication
					.ConnectedScenes
					.OfType<UIKit.UIWindowScene>()
					.SelectMany(s => s.Windows)
					.FirstOrDefault(w => w.IsKeyWindow);

				if (window == null)
					return false;
				var rootController = window.RootViewController;
				var tcs = new TaskCompletionSource<bool>();
				var alert = UIKit.UIAlertController.Create(title, message, UIKit.UIAlertControllerStyle.Alert);
				alert.AddAction(UIKit.UIAlertAction.Create(accept, UIKit.UIAlertActionStyle.Default, _ => tcs.TrySetResult(true)));
				alert.AddAction(UIKit.UIAlertAction.Create(cancel, UIKit.UIAlertActionStyle.Cancel, _ => tcs.TrySetResult(false)));
				rootController.PresentViewController(alert, true, null);
				return await tcs.Task;
			#else
				return await _navigationService.GetCurrentPage.DisplayAlert(title, message, accept, cancel);
			#endif
	}
	}

}