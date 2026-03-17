using System;
using System.Collections.ObjectModel;
using System.Linq;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS.ViewModels
{
    public class OfferSelectViewModel : BaseViewModel
    {
        ObservableCollection<OfferDto> _Offers { get; set; }
        public ObservableCollection<OfferDto> Offers { get { return _Offers; } set { _Offers = value; SetPropertyChanged(nameof(Offers)); } }
        public OfferDto SelectedOffer { get; set; }
        public OfferSelectViewModel()
        {
        }
        public void OfferSelected(OfferDto offer)
        {
            Offers.ForEach(x => x.IsSelected = false);
            offer.IsSelected = !offer.IsSelected;

        }
    }
}
