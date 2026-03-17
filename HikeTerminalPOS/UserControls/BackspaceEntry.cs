using System.Globalization;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using Font = Microsoft.Maui.Graphics.Font;

namespace HikePOS.UserControls
{
	public class BackspaceEntry : Entry
	{
		public event EventHandler? OnBackspacePressed;

		public void RaiseBackspacePressed()
		{
			OnBackspacePressed?.Invoke(this, EventArgs.Empty);
		}
	}
}