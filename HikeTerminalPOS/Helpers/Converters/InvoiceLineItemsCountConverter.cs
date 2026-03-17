using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using HikePOS.Models;

namespace HikePOS.Helpers
{
    [AcceptEmptyServiceProvider]
    public class InvoiceLineItemsCountConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            decimal count = 0;  //var count = 0;  //#33830 iOS - Issues about Display Item Total Option
            
            try
            {
                if (value != null && value is IList)
                {
                    var items = value as ObservableCollection<InvoiceLineItemDto>;
                    count = items.Where(item => item.Quantity > 0 && (item.InvoiceItemType == Enums.InvoiceItemType.Standard
                            || item.InvoiceItemType == Enums.InvoiceItemType.Custom
                            || item.InvoiceItemType == Enums.InvoiceItemType.Composite
                            || item.InvoiceItemType == Enums.InvoiceItemType.CompositeProduct
                            || item.InvoiceItemType == Enums.InvoiceItemType.UnityOfMeasure
                            || item.InvoiceItemType == Enums.InvoiceItemType.GiftCard)).Sum(a => a.Quantity);
                }
                return count.ToString("0.####");

            }
            catch (Exception ex)
            {
                ex.Track();
                return 0;
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
