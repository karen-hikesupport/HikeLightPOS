using System;
using HikePOS.Services;
using HikePOS.ViewModels;
using System.Threading.Tasks;
using System.Collections;

namespace HikePOS
{

	public partial class AccountBasicDetailPage : PopupBasePage<AccountBasicDetailViewModel>
    {
		public AccountBasicDetailPage()
		{
			InitializeComponent();

			CityAutoComplete.Focused += (sender, e) =>
			{
				AutoCompleteView.TranslationY = CityAutoCompleteFrame.Bounds.Y + CityAutoCompleteFrame.Bounds.Height + 5;
                ViewModel.AutoCompleteViewVisible = true;
			};

			CityAutoComplete.Unfocused += (sender, e) =>
			{
				ViewModel.AutoCompleteViewVisible = false;
			};

			CityListView.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if (e.PropertyName == "ItemsSource")
				{
					try
					{
						if (CityListView.ItemsSource != null)
						{
							var tmp = (IList)CityListView.ItemsSource;
							if (tmp.Count < 5)
								CityListView.HeightRequest = tmp.Count * 50;
							else
								CityListView.HeightRequest = 250;
						}
					}
					catch (Exception ex)
					{
						
                        ex.Track();
					}
				}
			};
		}
		
	}

}
