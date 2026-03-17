using System;
using System.Collections;
using System.Collections.Generic;

namespace HikePOS
{
    public partial class VantivReceiptView : ScrollView
    {
        public VantivReceiptView()
        {
            InitializeComponent();

			DeclinedEMVListView.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if (e.PropertyName == "Renderer" || e.PropertyName == "ItemsSource")
				{
					UpdateReceiptSize();
				}
			};
        }
		public void UpdateReceiptSize()
		{
			try
			{
				var itemsSource = BindableLayout.GetItemsSource(DeclinedEMVListView);
				if (itemsSource != null)
				{
					var tmp = (IList)itemsSource;
					DeclinedEMVListView.HeightRequest = tmp.Count * 35;
				}
				else
					DeclinedEMVListView.HeightRequest = 0;
			}
			catch (Exception ex)
			{
				ex.Track();
			}
		}
    }
}
