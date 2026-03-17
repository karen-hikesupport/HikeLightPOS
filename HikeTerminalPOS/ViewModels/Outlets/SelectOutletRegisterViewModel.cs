
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;

namespace HikePOS.ViewModels
{
	public class SelectOutletRegisterViewModel : BaseViewModel
	{
        public event EventHandler<bool> RegisterIsSelected;
        ObservableCollection<OutletDto_POS> _Stores { get; set; }
		public ObservableCollection<OutletDto_POS> Stores { get { return _Stores; } set { _Stores = value; SetPropertyChanged(nameof(Stores)); } }
		
        ObservableCollection<RegisterDto> _OutletRegisters { get; set; }
        public ObservableCollection<RegisterDto> OutletRegisters { get { return _OutletRegisters; } set { _OutletRegisters = value;  SetPropertyChanged(nameof(OutletRegisters));} }

        public bool IsClose { get; set; }

        public ICommand OutletTappedCommand => new Command<OutletDto_POS>(OutletTapped);
        public ICommand RegisterSelectionChangedCommand => new Command<RegisterDto>(RegisterTappedHandle_ItemTapped);


        public void SetStores(ObservableCollection<OutletDto_POS> outlets)
		{
			if (outlets != null)
			{
				Stores = outlets;
			}
		}

		public SelectOutletRegisterViewModel()
		{
            Title = "Select outlet register page";
            Stores = new ObservableCollection<OutletDto_POS>();
            OutletRegisters = new ObservableCollection<RegisterDto>();
		}

        public override void OnAppearing()
        {
            base.OnAppearing();
            SelectOutlet(Stores.FirstOrDefault());
        }

        void RegisterTappedHandle_ItemTapped(RegisterDto register)
        {
            if (IsClose && Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                App.Instance.MainPage = new NavigationPage(new LoginUserPage());
            }
            else
            {
                if (register != null)
                {
                    var outlet = Stores.FirstOrDefault(x => x.OutletRegisters.Contains(register));
                    if (outlet != null)
                    {
                        try
                        {
                            Settings.SelectedOutletId = outlet.Id;
                            Settings.SelectedOutletName = outlet.Title;
                            //Ticket start:#38783 iPad: Feature request - Register's Name in Process Sale.by rupesh
                            Settings.SelectedOutlet = outlet;
                            //Ticket end:#38783 .by rupesh
                            Settings.CurrentRegister = register;
                            RegisterIsSelected?.Invoke(this, true);
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                    }
                }
            }
        }


        public void SelectOutlet(OutletDto_POS outlet){
            if (outlet != null)
            {
				Stores.Where(x => x.Id != outlet.Id).All(x => { x.IsSelected = false; return true; });
				Stores.Where(x => x.Id == outlet.Id).All(x => { x.IsSelected = true; return true; });
                OutletRegisters = outlet.OutletRegisters;
            }
        }


        private void OutletTapped(OutletDto_POS dto)
        {
			SelectOutlet(dto);
        }
    }
}
