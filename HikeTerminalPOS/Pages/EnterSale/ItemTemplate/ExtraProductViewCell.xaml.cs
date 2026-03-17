
namespace HikePOS
{
    public partial class ExtraProductViewCell : Grid
    {
        public ExtraProductViewCell()
        {
            InitializeComponent();

            if (App.FontIncrAmount == 0)
            {
                ProductImage.HeightRequest = 105;
                ItemName.HeightRequest = 35;
                this.HeightRequest = 140;
            }
            else
            {
                ProductImage.HeightRequest = 145;
                ItemName.HeightRequest = 60;
                this.HeightRequest = 205;
            }
        }
    }
}
