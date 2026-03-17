using HikePOS.Helpers;
using HikePOS.Resources;

namespace HikePOS
{
    public partial class CategoryViewCell : Grid
    {
        public CategoryViewCell()
        {
            InitializeComponent();
            if (App.FontIncrAmount != 0)
            {               
                CategoryImage.HeightRequest = 135;
                ItemName.HeightRequest = 60;
                this.HeightRequest = 205;
            }

            if(Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.displayFullCategoryNameOnPOS)
            {
                lblname.FontSize = 14;
                lblname.TextColor = Colors.Black;
            }
            else
            {
                lblname.TextColor = AppColors.HikeColor;
            }
        }
        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            ((ViewModels.EnterSaleViewModel)this.Parent.BindingContext).ProductSelectCommand.Execute(this.BindingContext);
        }
    }
}
