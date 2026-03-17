using System;
using System.Collections.Generic;
using HikePOS.Helpers;

namespace HikePOS
{
	public partial class CashInOutReceipt : ScrollView
	{
		public CashInOutReceipt()
		{
			InitializeComponent();
        }

		public void UpdateCashInOut(string cashIn, decimal amount, string note)
		{
			try
			{
				CashType.Text = cashIn;
                CashAmount.Text = amount.ToString("C");

				Date.Text = DateTime.Now.ToStoreTime().ToString("dd MMM yyyy, h:mm tt");
				User.Text = "Process By : "+Settings.CurrentUser.FullName;




				if (!string.IsNullOrEmpty(note))
				{
					Note.Text = note;
					Note.IsVisible = true;
				}
				else
				{
					Note.IsVisible = false;
				}
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}
	}
}
