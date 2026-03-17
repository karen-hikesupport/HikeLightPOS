using System;
using System.Collections.Generic;
using System.Linq;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{

    public partial class OfferSelectPage : PopupBasePage<OfferSelectViewModel>
    {
        public event EventHandler<OfferDto> OfferSelected;

        public OfferSelectPage()
        {
            InitializeComponent();
        }
        void SaveHandle_Clicked(object sender, System.EventArgs e)
        {
            var offer = ViewModel.Offers.FirstOrDefault(x => x.IsSelected);
            OfferSelected.Invoke(sender, offer);
             Navigation.PopModalAsync();

        }
        void CloseHandle_Clicked(object sender, System.EventArgs e)
        {
            var offer = ViewModel.SelectedOffer;
            OfferSelected.Invoke(sender, offer);
            Navigation.PopModalAsync();
        }

        private void OfferSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e?.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                var offer = (OfferDto)e.CurrentSelection.Last();
                ViewModel.OfferSelected(offer);
                OffersCV.SelectedItem = null;
            }
        }
    }
}
