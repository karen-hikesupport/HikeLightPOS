using System;
using System.Threading.Tasks;

namespace HikePOS
{
	public interface IHUDProvider
	{
		void DisplayProgress(string message);
		void DisplaySuccess(string message);
		void DisplayError(string message);
		void Dismiss();
		bool HasSafeArea();
        double GetSafeareaHeight();
        //Start #71295 iPad- Feature: Converting Quote to Sale - Insufficient Stock By Pratik
        IDisposable DisplayActionToast(string Message, Color BackgroundColor = null, Color MessageTextColor = null, TimeSpan? Duration = null, Action action = null);
        //End #71295

        //void DisplayToast(string message);
        IDisposable DisplayToast(string Message, Color BackgroundColor = null, Color MessageTextColor = null, TimeSpan? Duration = null);
	}

}
