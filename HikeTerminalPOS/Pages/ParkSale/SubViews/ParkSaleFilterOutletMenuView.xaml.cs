using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS
{
    public partial class ParkSaleFilterOutletMenuView : ContentView
    {
        public event EventHandler<OutletDto_POS> ItemChanged;
        public int SelectedOutletId { get; set; }
        private ObservableCollection<OutletDto_POS> outlets;

        public ParkSaleFilterOutletMenuView()
        {
            InitializeComponent();
            LoadOutlets();
        }

        public void LoadOutlets()
        {
            //Ticket #10041 by rupesh
            var realm = RealmService.GetRealm();
            var list = realm.All<OutletDB_POS>().ToList().Select(a=>OutletDto_POS.FromModel(a)).ToList();// await CommonQueries.GetAllLocals<OutletDto_POS>();
            outlets = new ObservableCollection<OutletDto_POS>(list.Where(x => Settings.CurrentUser.Outlets.Select(s => s.OutletId).Contains(x.Id)));
            outlets.Insert(0, new OutletDto_POS { Title = "All" });
            outlets.Where(x => x.Id == Settings.SelectedOutletId).ForEach(x => x.IsSelected = true);
            OutletListView.ItemsSource = outlets;
            OutletListView.HeightRequest = Math.Min(44 * 10, 44 * outlets.Count);
        }

        private void SelectedItem(object sender, SelectedItemChangedEventArgs e)
        {
            ItemChanged?.Invoke(this, (OutletDto_POS)sender);
        }
        void OutletSelected(object sender, System.EventArgs e)
        {
            var btn = sender as Button;
            outlets.ForEach(x => x.IsSelected = false);
            var selectedOutlet = (OutletDto_POS)btn.BindingContext;
            ItemChanged?.Invoke(this, selectedOutlet);
            selectedOutlet.IsSelected = true;
            OutletListView.ItemsSource = null;
            OutletListView.ItemsSource = outlets;
        }

    }
}
