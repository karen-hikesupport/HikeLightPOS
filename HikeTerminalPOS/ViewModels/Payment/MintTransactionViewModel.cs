using System;
using HikePOS.Models;

namespace HikePOS.ViewModels
{
	public class MintTransactionViewModel : BaseViewModel
	{

		InvoiceDto _Invoice { get; set; }
		public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }

		string _AccessToken { get; set; }
		public string AccessToken { get { return _AccessToken; } set { _AccessToken = value; SetPropertyChanged(nameof(AccessToken)); } }
        public PaymentOptionDto PaymentOption;

    }
}
