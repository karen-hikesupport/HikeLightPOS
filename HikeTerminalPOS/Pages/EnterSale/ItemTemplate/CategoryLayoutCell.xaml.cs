using HikePOS.Helpers;

namespace HikePOS
{
    public partial class CategoryLayoutCell : Grid
    {
        public CategoryLayoutCell()
        {
            InitializeComponent();
            if (App.FontIncrAmount != 0)
            {               
                this.HeightRequest = 205;
            }
        }
        void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            ((ViewModels.EnterSaleViewModel)this.Parent.BindingContext).ProductSelectCommand.Execute(this.BindingContext);
        }
    }
}
