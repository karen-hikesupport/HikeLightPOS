using System;
using HikePOS.Models;

namespace HikePOS.ViewModels
{
	public class iZettleTransactionViewModel : BaseViewModel
	{

		InvoiceDto _Invoice { get; set; }
		public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }

	}
}
