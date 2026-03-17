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
	public class GroupInvoiceLineItemsConverter : IValueConverter, IMarkupExtension
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//Ticket start:#45367 iPad: FR - Group Products by Category on POS.by rupesh
			try
			{
				if ((String)parameter == "Discount")
					value = new InvoiceLineItemsDiscountConverter().Convert(value, targetType, parameter, culture);
				if (!Settings.StoreGeneralRule.ShowGroupProductsByCategory)
				{
					return value;
				}
				var groupLineiItemGroupList = new ObservableCollection<InvoiceLineiItemGroup>();
				if (value != null && value is IList)
				{
					var items = value as ObservableCollection<InvoiceLineItemDto>;
					var groupeditemsList = items.Where(x => !x.IsExtraproduct).GroupBy(u => u.categoryId).Select(grp => grp.ToList()).ToList();
					foreach (var groupedItem in groupeditemsList)
					{
						var invoiceLineiItemGroup = new InvoiceLineiItemGroup();
						var firstItem = groupedItem.FirstOrDefault();
						if (firstItem.CategoryDtos == null)
						{
							invoiceLineiItemGroup.Title = "NONE";

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
								invoiceLineiItemGroup = new InvoiceLineiItemGroup { Title = productCategories.FirstOrDefault().Name?.ToUpper() };
							}
						}
						foreach (var item in groupedItem)
						{
							invoiceLineiItemGroup.Add(item);
							if (item.HasExtraproduct || items.Any(x => x.InvoiceExtraItemValueParent == item.Sequence))
							{
								// Extra product not show in history by PR
								//var extraProductsItems = items.Where(x => x.InvoiceExtraItemValueParent == item.InvoiceItemValue);
								//End Extra product not show in history by PR
								var extraProductsItems = items.Where(x => x.InvoiceExtraItemValueParent == item.Sequence);
								foreach (var e in extraProductsItems)
								{
									invoiceLineiItemGroup.Add(e);

								}
							}
						}
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
	public class InvoiceLineiItemGroup : ObservableCollection<InvoiceLineItemDto>
	{
		public string Title { get; set; }

		public InvoiceLineiItemGroup(string title, IEnumerable<InvoiceLineItemDto> invoiceLineItems)
			: base(invoiceLineItems)
		{
			Title = title;
		}

		public InvoiceLineiItemGroup() { }
	}

}
