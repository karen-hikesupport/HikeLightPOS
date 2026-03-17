using HikePOS.Helpers;
using System.Linq;

namespace HikePOS
{
    public partial class ProductOfferStockCell : Grid
    {
        public ProductOfferStockCell()
        {
            InitializeComponent();
          
            if (App.FontIncrAmount != 0)
            {
                ProductImage.HeightRequest = 135;
                ItemName.HeightRequest = 60;
                this.HeightRequest = 205;
            }
            if(Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.DisplayFullProductNameOnPOS)
            {
                lblname.FontSize = 14;
            }
        }

        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            ((ViewModels.EnterSaleViewModel)this.Parent.BindingContext).ProductSelectCommand.Execute(this.BindingContext);
        }
    }
}
