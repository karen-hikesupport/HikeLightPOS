using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using System.Linq;
using System.Reactive.Linq;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
	public class PrinterConfigurationViewModel : BaseViewModel
	{
		public PrinterConfigurationViewModel()
		{
		}
		Printer _selectedPrinter { get; set; }
		public Printer SelectedPrinter { get { return _selectedPrinter; } set { _selectedPrinter = value; SetPropertyChanged(nameof(SelectedPrinter));  } }

		bool _isAllReceiptRegistered { get; set; }
		public bool IsAllReceiptRegistered { get { return _isAllReceiptRegistered; } set { _isAllReceiptRegistered = value; SetPropertyChanged(nameof(IsAllReceiptRegistered)); } }

		public void DoSave()
		{
			if (Settings.GetCachePrinters != null)
			{
				var AllPrinter = Settings.GetCachePrinters;
				var tmpAllPrinter = new ObservableCollection<Printer>(AllPrinter.Where(x => x.ModelName != SelectedPrinter.ModelName).ToList());
				tmpAllPrinter.Add(SelectedPrinter);
				Settings.GetCachePrinters = tmpAllPrinter;

			}
			//GetAllPrinter = new ObservableCollection<Printer>(GetAllPrinter.Where(x => x.ModelName == SelectedPrinter.ModelName)
			//                                                  .Select(s => { s = SelectedPrinter; return s; }));
		}
	}
}
