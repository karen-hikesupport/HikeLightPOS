using System.Collections;
using System.Windows.Input;

namespace HikePOS.UserControls
{
	public class CustomListView : ListView
	{
		public CustomListView()
		{
			ItemAppearing += CustomListView_ItemAppearing;
		}

		public CustomListView(ListViewCachingStrategy strategy) : base(strategy)
		{

		}

		public static readonly BindableProperty LoadMoreCommandProperty =
			BindableProperty.Create(nameof(LoadMoreCommand),typeof(ICommand),typeof(CustomListView), default(ICommand));
		
		/// <summary>
		/// Gets or sets the command binding that is called whenever the listview is getting near the bottomn of the list, and therefore requiress more data to be loaded.
		/// </summary>
		public ICommand LoadMoreCommand
		{
			get { return (ICommand)GetValue(LoadMoreCommandProperty); }
			set { SetValue(LoadMoreCommandProperty, value); }
		}

		void CustomListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
		{
			var items = ItemsSource as IList;

			if (items != null && e.Item == items[items.Count - 1])
			{
				if (LoadMoreCommand != null && LoadMoreCommand.CanExecute(null))
					LoadMoreCommand.Execute(null);
			}
		}

	}
}
