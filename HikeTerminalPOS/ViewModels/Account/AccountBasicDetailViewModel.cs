using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HikePOS.Services;
using HikePOS.Enums;
using HikePOS.Models;
using Fusillade;
using System.Windows.Input;
using System.Linq;
using HikePOS.Resx;

namespace HikePOS.ViewModels
{
	public class AccountBasicDetailViewModel : BaseViewModel
	{

        ApiService<IShopApi> shopApiService = new ApiService<IShopApi>();
        public ShopServices shopService;

        public event EventHandler<bool> AccountBasicDetailAdded;

        #region Properties

        public ObservableCollection<Prediction> _locationPredictionList { get; set; }

		public ObservableCollection<Prediction> LocationPredictionList { get { return _locationPredictionList; } set { _locationPredictionList = value; SetPropertyChanged(nameof(LocationPredictionList)); } }

		ObservableCollection<Prediction> _selectedLocationPrediction;
        public ObservableCollection<Prediction> SelectedLocationPrediction { get { return _selectedLocationPrediction; } set { _selectedLocationPrediction = value; SetPropertyChanged(nameof(SelectedLocationPrediction)); } }


        BasicShopInfo _accountBasicInfo { get; set; }
		public BasicShopInfo AccountBasicInfo { get { return _accountBasicInfo; } set { _accountBasicInfo = value; SetPropertyChanged(nameof(AccountBasicInfo)); } }

	    string _placeId { get; set; }
		public string PlaceId { get { return _placeId; } set { _placeId = value; SetPropertyChanged(nameof(PlaceId)); } }

		string _city;
        public string City { get { return _city; } set { _city = value; SetPropertyChanged(nameof(City)); CityTextChangedCommand.Execute(null); } }

        bool _autoCompleteViewVisible;
        public bool AutoCompleteViewVisible { get { return _autoCompleteViewVisible; } set { _autoCompleteViewVisible = value; SetPropertyChanged(nameof(AutoCompleteViewVisible)); } }

        ObservableCollection<string> _industryTypeList { get; set; } = new ObservableCollection<string>(
			Enum.GetNames(typeof(IndustryType))
			.Select(x => { return LanguageExtension.Localize(x.ToString()); }));
		
		public ObservableCollection<string> IndustryTypeList { get { return _industryTypeList; } set { _industryTypeList = value; SetPropertyChanged(nameof(IndustryTypeList)); } }


		public string _selectedIndustryType { get; set; } = LanguageExtension.Localize(IndustryType.FashionClothing.ToString());
		public string SelectedIndustryType { get { return _selectedIndustryType; } set { _selectedIndustryType = value; SetPropertyChanged(nameof(SelectedIndustryType)); } }

		ObservableCollection<string> _describesYourBusinessList { get; set; } = new ObservableCollection<string>(
			Enum.GetNames(typeof(SellerBy))
			.Select(x => { return LanguageExtension.Localize(x.ToString()); }));

		//ObservableCollection<string> _describesYourBusinessList { get; set; } = new ObservableCollection<string>(Enum.GetNames(typeof(SellerBy)));
		public ObservableCollection<string> DescribesYourBusinessList { get { return _describesYourBusinessList; } set { _describesYourBusinessList = value; SetPropertyChanged(nameof(DescribesYourBusinessList)); } }


		public string _describesYourBusiness { get; set; } = LanguageExtension.Localize(SellerBy.StartingNewBusiness.ToString());
		public string DescribesYourBusiness { get { return _describesYourBusiness; } set { _describesYourBusiness = value; SetPropertyChanged(nameof(DescribesYourBusiness)); } }

        //string _SelectedCity { get; set; }
        //public string SelectedCity { get { return _SelectedCity; } set { _SelectedCity = value; SetPropertyChanged(nameof(SelectedCity)); } }
        #endregion

        #region Life Cycle
        public AccountBasicDetailViewModel()
		{
			AccountBasicInfo = new BasicShopInfo();
			SaveAccountBasicInfoCommand = new Command(SaveAccountbasicInfo);
			shopService = new ShopServices(shopApiService);
			//PropertyChanged += (sender, e) => {
			//	if (e.PropertyName == nameof(SelectedCity))
			//	{
			//		//method call
			//	}
			//};
		}
		#endregion

		#region Command
		public ICommand YesAddDemoProductCommand => new Command(YesAddDemoProduct);
        public ICommand NoAddDemoProductCommand => new Command(NoAddDemoProduct);
        public ICommand TermsAndConditionCommand => new Command(TermsAndCondition);
        public ICommand PrivacyPolicyCommand => new Command(PrivacyPolicy);
        public ICommand ItemSelectedCommand => new Command(ItemSelected);
        public ICommand CityTextChangedCommand => new Command(CityAutoCompleteHandleValueChanged);
        public ICommand SaveAccountBasicInfoCommand { get; }

        #endregion

        #region Command Exe / Methods

        void ItemSelected(object dto)
        {
			AutoCompleteViewVisible = false;
            LocationPredictionList = new System.Collections.ObjectModel.ObservableCollection<Prediction>();
            if (dto != null)
            {
               
				City = ((Prediction)dto).description;
                var prediction = (Prediction)dto;
                if (prediction != null && prediction.terms != null && prediction.terms.Count > 0)
                {
                    AccountBasicInfo.City = prediction.terms[0].value;
                    PlaceId = prediction.place_id;

                }
                else
                {
                    AccountBasicInfo.City = dto.ToString();
                }

            }
            else
            {
                AccountBasicInfo.City = "";
            }

        }

        void YesAddDemoProduct()
		{
            AccountBasicInfo.ToBuildDemoData = true;
        }

        void NoAddDemoProduct()
        {
            AccountBasicInfo.ToBuildDemoData = false;
        }

        async void TermsAndCondition()
        {
            Uri uri = new Uri(ServiceConfiguration.TermsAndConditionLink);
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }

        async void PrivacyPolicy()
        {
            Uri uri = new Uri(ServiceConfiguration.PrivacyPolicyLink);
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }

        public async Task<ObservableCollection<Prediction>> GetLocation(string Address)
		{
            using (new Busy(this, true))
            {
                try
                {
                    string strAutoCompleteQuery = ServiceConfiguration.strPlacesAutofillUrl + "autocomplete/json" + "?input=" + Address + "&key=" + ServiceConfiguration.strGooglePlaceAPILey;
                    var tmpLocationPredictionList = await GetLocationService.LocationAutoComplete(strAutoCompleteQuery);

                    if (tmpLocationPredictionList != null && tmpLocationPredictionList.predictions != null && tmpLocationPredictionList.predictions.Any(x => x.types.Any(y => y == "locality")))
                    {
                        LocationPredictionList = new ObservableCollection<Prediction>(tmpLocationPredictionList.predictions.Where(x => x.types.Any(y => y == "locality")));
                    }
                }
                catch(Exception ex)
                {
                    ex.Track();
                }

                if (LocationPredictionList == null)
                    LocationPredictionList = new ObservableCollection<Prediction>();
                
                return LocationPredictionList;
            };
		}


		public async Task<PlaceDetail> GetPlaceDetail(string Address)
		{
			string strQuery = ServiceConfiguration.strPlacesAutofillUrl + "details/json" +"?placeid=" + Address + "&key=" + ServiceConfiguration.strGooglePlaceAPILey;
			var placeDetail = await GetLocationService.PlaceDetail(strQuery);

			if (placeDetail != null)
				return placeDetail;
			else
				return new PlaceDetail();

		}


		public async void SaveAccountbasicInfo()
		{
			using (new Busy(this, true))
			{
				try
				{
					#region Validation
					if (string.IsNullOrEmpty(AccountBasicInfo.Name))
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("FirstNameRequiredValidationMessage"));
						return;
					}
					if (string.IsNullOrEmpty(AccountBasicInfo.LastName))
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("LastNameRequiredValidationMessage"));
						return;
					}
					if (string.IsNullOrEmpty(AccountBasicInfo.City) || string.IsNullOrEmpty(PlaceId))
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("CityRequiredValidationMessage"));
						return;
					}
					if (string.IsNullOrEmpty(AccountBasicInfo.PostCode))
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("ZipRequiredValidationMessage"));
						return;
					}
					if (string.IsNullOrEmpty(AccountBasicInfo.Phone))
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("PhoneRequiredValidationMessage"));
						return;
					}

					#endregion

					if (!string.IsNullOrEmpty(DescribesYourBusiness))
					{
						foreach (SellerBy enumValue in Enum.GetValues(typeof(SellerBy)))
						{
							if (DescribesYourBusiness == AppResources.ResourceManager.GetString(enumValue.ToString()))
							{
								AccountBasicInfo.SellerBy = enumValue;
								break;
							}
						}
					}


					if (!string.IsNullOrEmpty(SelectedIndustryType))
					{
						foreach (IndustryType enumValue in Enum.GetValues(typeof(IndustryType)))
						{
							if (SelectedIndustryType == AppResources.ResourceManager.GetString(enumValue.ToString()))
							{
								AccountBasicInfo.IndustryType = enumValue;
								break;
							}
						}
					}

					if (!string.IsNullOrEmpty(PlaceId))
					{
						var Place = await GetPlaceDetail(PlaceId);
						if (Place != null && !string.IsNullOrEmpty(Place.status))
						{
							AccountBasicInfo.lattitude = Place.result.geometry.location.lat.ToString();
							AccountBasicInfo.longitude = Place.result.geometry.location.lng.ToString();

							string CountryName = Place.result.address_components.FirstOrDefault(x => x.types.Any(c => c == "country")).short_name;

							AccountBasicInfo.Country = CountryName;

							string StateName = Place.result.address_components.FirstOrDefault(x => x.types.Any(c => c == "administrative_area_level_1")).long_name;

							AccountBasicInfo.State = StateName;
						}
					}

					AjaxResponse shopResponse = await shopService.UpdateAccountBasicInfo(Priority.UserInitiated, AccountBasicInfo);
					if (shopResponse != null && shopResponse.success)
					{
						AccountBasicDetailAdded?.Invoke(this, true);
					}
					else
					{
						App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"));
					}
				}
				catch (Exception ex)
				{
					App.Instance.Hud.DisplayToast(LanguageExtension.Localize("SomethingWrong"));
					ex.Track();
				}
			}
		}

        async void CityAutoCompleteHandleValueChanged()
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }
                AccountBasicInfo.City = City;
                await GetLocation(City);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        #endregion
    }
}
