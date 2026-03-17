using System;
using HikePOS.Models;

namespace HikePOS
{
    public class EnterSaleItemTemplateSelector : DataTemplateSelector
    {
       
        private readonly DataTemplate productDataTemplate;
		private readonly DataTemplate categoryDataTemplate;
		private readonly DataTemplate categoryLayoutCell;
        private readonly DataTemplate offerDataTemplate;
		private readonly DataTemplate backItemTemplate; 

		// private readonly DataTemplate productNoBorderCell;

		// private readonly DataTemplate productOnlyStockCell;

		// private readonly DataTemplate productOfferStockCell;

		private readonly DataTemplate productUOMCell;

		public EnterSaleItemTemplateSelector()
		{
			this.productDataTemplate = new DataTemplate(typeof(ProductViewCell));
			this.categoryDataTemplate = new DataTemplate(typeof(CategoryViewCell));
			this.offerDataTemplate = new DataTemplate(typeof(OfferViewCell));
			this.backItemTemplate = new DataTemplate(typeof(BackItemViewCell)); 
			// this.productNoBorderCell = new DataTemplate(typeof(ProductNoBorderCell));
			// this.productOnlyStockCell = new DataTemplate(typeof(ProductOnlyStockCell));
			// this.productOfferStockCell = new DataTemplate(typeof(ProductOfferStockCell));
			this.categoryLayoutCell = new DataTemplate(typeof(CategoryLayoutCell));
			this.productUOMCell = new DataTemplate(typeof(ProductUOMCell));

		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var messageVm = item as EnterSaleItemDto;
			if (messageVm == null)
				return null;

            // if (messageVm.ItemType == Enums.InvoiceItemType.Standard && messageVm.Product != null && !messageVm.Product.IsInAnyOffer 
			// &&  !messageVm.Product.IsUnitOfMeasure && !messageVm.Product.ShowStockCountOnEnterSale)
            // {
            //     return this.productNoBorderCell;
            // }
			// else if (messageVm.ItemType == Enums.InvoiceItemType.Standard && messageVm.Product != null && !messageVm.Product.IsInAnyOffer 
			// &&  !messageVm.Product.IsUnitOfMeasure && messageVm.Product.ShowStockCountOnEnterSale)
            // {
            //     return this.productOnlyStockCell;
            // }
			// else if (messageVm.ItemType == Enums.InvoiceItemType.Standard && messageVm.Product != null && messageVm.Product.IsInAnyOffer 
			// &&  !messageVm.Product.IsUnitOfMeasure && messageVm.Product.ShowStockCountOnEnterSale)
            // {
            //     return this.productOfferStockCell;
            // }
			if (messageVm.ItemType == Enums.InvoiceItemType.Standard && messageVm.Product != null && messageVm.Product.IsUnitOfMeasure)
            {
                return this.productUOMCell;
            }
			else if (messageVm.ItemType == Enums.InvoiceItemType.Standard && messageVm.Product != null)
            {
                return this.productDataTemplate;
            }
			else if (messageVm.ItemType == Enums.InvoiceItemType.Category && messageVm.Category != null && messageVm.Category.ISLayout)
			{
				return this.categoryLayoutCell;
			}
            else if (messageVm.ItemType == Enums.InvoiceItemType.Category && messageVm.Category != null)
			{
				return this.categoryDataTemplate;
			}
            else if (messageVm.ItemType == Enums.InvoiceItemType.Composite && messageVm.Offer != null)
			{
				return this.offerDataTemplate;
			}
			else if (messageVm.ItemType == Enums.InvoiceItemType.Back && messageVm.Category != null) 
			{
				return this.backItemTemplate;
			}
            else
			{
				return null;
			}

		}
    }
}
