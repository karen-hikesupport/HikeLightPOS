using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using HikePOS.Models;

namespace HikePOS.Helpers
{
    [AcceptEmptyServiceProvider]
    public class InvoiceLineItemsDiscountConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                if (value != null && value is IList)
                {
                    var items = value as ObservableCollection<InvoiceLineItemDto>;
                    items = new ObservableCollection<InvoiceLineItemDto>(items.Where(x => x.InvoiceItemType != Enums.InvoiceItemType.Discount));
                    return items;
                }
                return null;
            }
            catch (Exception ex)
            {
                ex.Track();
                return null;
            }
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
}
