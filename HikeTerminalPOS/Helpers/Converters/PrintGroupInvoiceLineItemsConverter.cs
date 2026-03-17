using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using HikePOS.Models;
using Newtonsoft.Json;

namespace HikePOS.Helpers
{
	[AcceptEmptyServiceProvider]
	public class PrintGroupInvoiceLineItemsConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//Ticket start:#45367 iPad: FR - Group Products by Category on POS.by rupesh
			try
			{
				if((String)parameter == "Discount")
				value = new InvoiceLineItemsDiscountConverter().Convert(value,targetType,parameter,culture);
                ObservableCollection<PrintInvoiceLineiItemGroup> groupLineiItemGroupList = null;
                if (!Settings.StoreGeneralRule.ShowGroupProductsByCategory)
                {
                    var items = value as ObservableCollection<InvoiceLineItemDto>;
                     groupLineiItemGroupList = new ObservableCollection<PrintInvoiceLineiItemGroup>(items.Select(a => new PrintInvoiceLineiItemGroup()
                    {
                        Title = "",
                        InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>() { a }
                    }));

                    return groupLineiItemGroupList;
                }
				if (value != null && value is IList)
				{
					groupLineiItemGroupList = new ObservableCollection<PrintInvoiceLineiItemGroup>();
                    var items = value as ObservableCollection<InvoiceLineItemDto>;
					var groupeditemsList = items.GroupBy(u => u.categoryId).Select(grp => grp.ToList()).ToList();
					foreach (var groupedItem in groupeditemsList)
					{
						var invoiceLineiItemGroup = new PrintInvoiceLineiItemGroup();
						var firstItem = groupedItem.FirstOrDefault();
						if (firstItem.CategoryDtos == null)
						{
							invoiceLineiItemGroup.Title = "NONE";
                            invoiceLineiItemGroup.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();

                        }
						else
						{
							var productCategories = JsonConvert.DeserializeObject<List<CategoryDto>>(firstItem.CategoryDtos);
							if (productCategories.Count == 0)
							{
								invoiceLineiItemGroup.Title = "NONE";
							}
							else
                            {
								invoiceLineiItemGroup = new PrintInvoiceLineiItemGroup { Title = productCategories.FirstOrDefault().Name?.ToUpper() };

							}
                            invoiceLineiItemGroup.InvoiceLineItems = new ObservableCollection<InvoiceLineItemDto>();

                        }
                        foreach (var item in groupedItem)
						invoiceLineiItemGroup.InvoiceLineItems.Add(item);
						groupLineiItemGroupList.Add(invoiceLineiItemGroup);
					}
		
				}

				return groupLineiItemGroupList;
			}
			catch (Exception ex)
			{
				ex.Track();
				return null;
			}
			//Ticket end:#45367 .by rupesh
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
    public class PrintInvoiceLineiItemGroup : ObservableCollection<InvoiceLineItemDto>
    {

        public string Title { get; set; }
        public ObservableCollection<InvoiceLineItemDto> InvoiceLineItems { get; set; }

    }

}
