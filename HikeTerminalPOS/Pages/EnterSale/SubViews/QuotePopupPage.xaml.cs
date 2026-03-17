using HikePOS.Models;
using HikePOS.ViewModels;


namespace HikePOS.Pages;

public partial class QuotePopupPage : PopupBasePage<BaseViewModel>
{
    List<ProductQuote> AllproductDtos;
    public QuotePopupPage(List<ProductQuote> productDto_s)
    {
        InitializeComponent();
        AllproductDtos = productDto_s;
        lstsotck.ItemsSource = AllproductDtos;
    }

    void SearchTextHandle_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (e != null && e.NewTextValue != null)
            {
                lstsotck.ItemsSource = AllproductDtos.Where(a => a.Name.ToLower().Contains(SearchEntry.Text.ToLower()) || (!string.IsNullOrEmpty(a.Sku) && a.Sku.ToLower().Contains(SearchEntry.Text.ToLower()))).ToList();
            }
            else
            {
                lstsotck.ItemsSource = AllproductDtos;
            }
        }
        catch (Exception ex)
        {
            ex.Track();
        }
    }
}
