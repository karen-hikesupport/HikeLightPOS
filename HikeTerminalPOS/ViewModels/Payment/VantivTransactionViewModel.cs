using System;
using HikePOS.Models;
using HikePOS.Models.Payment;

namespace HikePOS.ViewModels
{
    public class VantivTransactionViewModel : BaseViewModel
    {
        InvoiceDto _Invoice { get; set; }
        public InvoiceDto Invoice { get { return _Invoice; } set { _Invoice = value; SetPropertyChanged(nameof(Invoice)); } }

        public VantivConfigurationDto ConfigurationModel { get; set; }

    }
}
