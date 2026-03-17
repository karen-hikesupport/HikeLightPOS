using System;
using System.Collections.ObjectModel;
using HikePOS.Models;

namespace HikePOS.Services
{
	public interface IPrintService
	{
		void Start();
		//void DecoveryAsync(Action<ObservableCollection<Printer>> SearchCompleted);
		ObservableCollection<Printer> DecoveryAsync();
		ObservableCollection<Printer> GetCachePrinters();
		void stopDiscovery();
		//Printer Current { get; set; }
		//int Numberofcopy { get; set; }
		//long CustomerFrom { get; set; }
		//long CustomerTo { get; set; }
		//bool CustomerNoSwithc { get; set; }
		//bool PrintDocket
		//{
		//	get;
		//	set;
		//}
		//bool BarcodeEnabled
		//{
		//	get;
		//	set;
		//}

		//bool InfoEnabled
		//{
		//	get;
		//	set;
		//}
		//bool ReceiptEnabled
		//{
		//	get;
		//	set;
		//}
		//bool IsRegistered
		//{
		//	get;
		//	set;
		//}

	}
}
