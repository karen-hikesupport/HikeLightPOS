using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ZXing;

namespace HikePOS.Pages.PhonePages.Payment;

public partial class TapToPayEducationPagePhone : ContentPage
{
    public TapToPayEducationPagePhone()
    {
        InitializeComponent();
        Carousal.ItemsSource = new ObservableCollection<string> { "education_1", "education_2", "education_3", "education_4", "education_5"};
    }

    private void Close_Clicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync(true);
    }

    private void Position_changed(object sender, PositionChangedEventArgs e)
    {
           ProgressBar.Progress = (Carousal.Position + 1) / 5.0;
            if (Carousal.Position == 4)
            {
                ReadyButton.IsVisible = true;
            }

    }
     private void OnCarouselViewScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
    }

}
