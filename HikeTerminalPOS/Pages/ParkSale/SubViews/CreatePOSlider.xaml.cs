using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.ViewModels;
using System.Linq;
using System.Collections.ObjectModel;
using HikePOS.Models;

namespace HikePOS
{

	public partial class CreatePOSlider : PopupBasePage<CreatePOViewModel>
    {
		public CreatePOSlider()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
            var ItemCount = 0;
            var SupplierCount = 0;
            //Ticket start:#84288 iOS: FR:Backorder Management.by rupesh
			if (ViewModel.PurchaseOrderes != null && ViewModel.PurchaseOrderes.Any())
			{
			    ObservableCollection<PurchaseOrderDto> tempPurchaseOrderes = new ObservableCollection<PurchaseOrderDto>();
				foreach (var purchaseOrder in ViewModel.PurchaseOrderes)
				{
					foreach(var purchaseOrderLineItem in purchaseOrder.PurchaseOrderLineItems)
					{  
						var tempPurchaseOrder = purchaseOrder.Copy();
						tempPurchaseOrder.PurchaseOrderLineItems.Clear();
						tempPurchaseOrder.PurchaseOrderLineItems.Add(purchaseOrderLineItem);
						tempPurchaseOrderes.Add(tempPurchaseOrder);
					}
				}
                 ViewModel.PurchaseOrderes = tempPurchaseOrderes;
				 ItemCount = ViewModel.PurchaseOrderes.Count();
				 SupplierCount = ViewModel.PurchaseOrderes.GroupBy(y => y.SupplierName).Count();
			}
            //Ticket end:#84288 .by rupesh

            POSummaryText.Text = string.Format(LanguageExtension.Localize("POSummaryText"), ItemCount, SupplierCount, SupplierCount);
        }
	}
}
