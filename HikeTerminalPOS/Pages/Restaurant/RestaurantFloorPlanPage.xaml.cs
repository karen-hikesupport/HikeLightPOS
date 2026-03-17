using System.Collections.ObjectModel;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class RestaurantFloorPlanPage : BaseContentPage<RestaurantFloorPlanViewModel>
    {

         public EventHandler<Models.CanvanceTableLayout> ClosedPaged;

        public RestaurantFloorPlanPage(ObservableCollection<OccupideTableDto> occupiedTables ,InvoiceDto invoice)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ViewModel.OccupiedTables = occupiedTables;
            ViewModel.Invoice = invoice;
            ViewModel.RestaurantFloorPlanPage = this;
        }

    }
}
