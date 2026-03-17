using System;
using HikePOS.Models;

namespace HikePOS
{
    public class EnterSaleSearchItemTemplateSelector : DataTemplateSelector
    {
       
        private readonly DataTemplate productDataTemplate;
        private readonly DataTemplate offerDataTemplate;

        public EnterSaleSearchItemTemplateSelector()
        {
            this.productDataTemplate = new DataTemplate(typeof(SearchProductViewCell));
            this.offerDataTemplate = new DataTemplate(typeof(SearchOfferViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var messageVm = item as EnterSaleItemDto;
            if (messageVm == null)
                return null;

            if (messageVm.ItemType == Enums.InvoiceItemType.Standard && messageVm.Product != null)
            {
                return this.productDataTemplate;
            }
            else if (messageVm.ItemType == Enums.InvoiceItemType.Composite && messageVm.Offer != null)
            {
                return this.offerDataTemplate;
            }
            else
            {
                return null;
            }

        }
    }
}
