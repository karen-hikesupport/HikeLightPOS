using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;

namespace HikePOS.ViewModels
{
    public class EditDenominationViewModel : BaseViewModel
    {
        public EventHandler<bool> Closed;
        ApiService<IOutletApi> OutletApiService = new ApiService<IOutletApi>();
        OutletServices outletServices;
        public bool IsUpdated = false;

        ObservableCollection<DenominationDto> _lstDenomination { get; set; }

        public int RegisterClosureTallyId { get; set; } 

        public ObservableCollection<DenominationDto> lstDenomination
        {
            get
            {
                return _lstDenomination;
            }
            set
            {
                _lstDenomination = value;
                SetPropertyChanged(nameof(lstDenomination));
            }
        }


        public EditDenominationViewModel()
        {
            outletServices = new OutletServices(OutletApiService);
            RemoveDenominationItemCommand = new Command<DenominationDto>(RemoveDenominationItem);
            SaveDenominationCommand = new Command<DenominationDto>(SaveDenomination);
        }



        #region Command
        public ICommand RemoveDenominationItemCommand { get; }
        public ICommand SaveDenominationCommand { get; }
        public ICommand CloseCommand => new Command(CloseTapped);
        public ICommand AddNewCommand => new Command(AddNewTapped);

        #endregion

        #region Command Execution

        public async void CloseTapped()
        {
            try
            {
                Closed?.Invoke(this, IsUpdated);
                if (NavigationService.ModalStack != null && NavigationService.ModalStack.Count > 0)
                    await NavigationService.PopModalAsync();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        void AddNewTapped()
        {
            if (lstDenomination.Any(x => x.Id == 0))
            {
                return;
            }
            lstDenomination.Select(x => x.IsEditable = false);
            lstDenomination.Insert(0, new DenominationDto() { IsEditable = true });
        }

        public async void RemoveDenominationItem(DenominationDto item)
        {
            try
            {
                using (new Busy(this, true))
                {
                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    {
                        var result = await outletServices.RemoveDenomination(Fusillade.Priority.UserInitiated, item);

                        if (result)
                        {
                            LoadDenomination();
                            await outletServices.GetRemoteRegisterById(Fusillade.Priority.UserInitiated, true, Settings.CurrentRegister.Id);
                            IsUpdated = true;
                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        async void SaveDenomination(DenominationDto item)
        {
            try
            {
                using (new Busy(this, true))
                {
                    if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    {

                        var tmplstDenomination = outletServices.GetLocalDenomination();
                        if (!tmplstDenomination.Any(x => x.Value == item.Value && x.Id == item.Id))
                        {

                            var result = await outletServices.CreateOrUpdateRemoteDenomination(Fusillade.Priority.UserInitiated, item);

                            if (result != null)
                            {
                                LoadDenomination();
                                await outletServices.GetRemoteRegisterById(Fusillade.Priority.UserInitiated, true, Settings.CurrentRegister.Id);
                                IsUpdated = true;
                            }
                        }
                        else if (lstDenomination.Any(x => x.Value == item.Value && x.Id != item.Id))
                        {
                            App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                        }
                        else
                        {
                            lstDenomination.All(x =>
                            {
                                x.IsEditable = false;
                                return true;
                            });
                        }
                    }
                    else
                    {
                        App.Instance.Hud.DisplayToast(LanguageExtension.Localize("NoInternetMessage"), Colors.Red, Colors.White);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        #endregion


        public override void OnAppearing()
        {
            base.OnAppearing();
            IsUpdated = false;
            LoadDenomination();
        }

        public void LoadDenomination()
        {
            using (new Busy(this, true))
            {
                lstDenomination = outletServices.GetLocalDenomination();
            }
        }
    }
}
