namespace HikePOS
{
    //Start #92766 FR POS - BE ABLE TO GO BACK By Pratik
    public partial class BackItemViewCell : Grid
    {
        public BackItemViewCell()
        {
            InitializeComponent();
            if (App.FontIncrAmount == 0)
            {
                ItemName.HeightRequest = 35;
                this.HeightRequest = 140;
            }
            else
            {
                ItemName.HeightRequest = 60;
                this.HeightRequest = 205;
            }
        }
        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            ((ViewModels.EnterSaleViewModel)this.Parent.BindingContext).ProductSelectCommand.Execute(this.BindingContext);
        }
    }
    //End #92766 By Pratik
}
