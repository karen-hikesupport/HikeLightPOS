namespace HikePOS
{
    public partial class OfferViewCell : Grid
    {
        public OfferViewCell()
        {
            InitializeComponent();

			if (App.FontIncrAmount == 0)
			{
                OfferIcon.HeightRequest = 95;
                ItemName.HeightRequest = 35;
                this.HeightRequest = 140;
            }
			else
			{
                OfferIcon.HeightRequest = 135;
                ItemName.HeightRequest = 60;
                this.HeightRequest = 205;
            }
        }
        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            ((ViewModels.EnterSaleViewModel)this.Parent.BindingContext).ProductSelectCommand.Execute(this.BindingContext);
        }
    }
}
