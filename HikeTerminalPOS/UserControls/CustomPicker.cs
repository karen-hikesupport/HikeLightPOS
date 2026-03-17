using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace HikePOS.UserControls
{
	public class CustomPicker : Picker
	{
		public CustomPicker()
		{
			SelectedIndexChanged += OnSelectedIndexChanged;
			this.BindingContextChanged += (sender, e) =>
			{
				this.SelectedIndex = -1;
			};
		}

		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create("SelectedItem", typeof(object), typeof(CustomPicker), null, BindingMode.TwoWay, null, OnSelectedItemChanged);

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set
			{
				try
				{
					SetValue(SelectedItemProperty, value);
				}
				catch (Exception ex)
				{
					ex.Track();
				}
			}
		}



		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(CustomPicker), null, BindingMode.TwoWay, null, OnItemsSourceChanged);

		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public static readonly BindableProperty DisplayPropertyProperty =
			BindableProperty.Create("DisplayProperty", typeof(string), typeof(CustomPicker), null, BindingMode.TwoWay, null, OnDisplayPropertyChanged);


		public string DisplayProperty
		{
			get { return (string)GetValue(DisplayPropertyProperty); }
			set { SetValue(DisplayPropertyProperty, value); }
		}
		private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = (CustomPicker)bindable;

			if(picker.SelectedItem != newValue)
				picker.SelectedItem = newValue;

			if (picker.ItemsSource != null && picker.SelectedItem != null)
			{
				var count = 0;
				foreach (var obj in picker.ItemsSource)
				{
					if (obj == picker.SelectedItem)
					{
						picker.SelectedIndex = count;
						break;
					}
					count++;
				}
			}
		}

		private static void OnDisplayPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = (CustomPicker)bindable;
			picker.DisplayProperty = (string)newValue;
			LoadItemsAndSetSelected(bindable);
		}

		private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = (CustomPicker)bindable;
			picker.ItemsSource = (IList)newValue;

			var oc = newValue as INotifyCollectionChanged;

			if (oc != null)
			{
				oc.CollectionChanged += (a, b) =>
				{
					LoadItemsAndSetSelected(bindable);
				};
			}

			LoadItemsAndSetSelected(bindable);
		}

		private static void LoadItemsAndSetSelected(BindableObject bindable)
		{
			var picker = (CustomPicker)bindable;

			if (picker.ItemsSource == null)
				return;

			var count = 0;
			//Ticket start:#36960 iPad: deleted tax showing under the product in cart.by rupesh
			picker.Items.Clear();
			//Ticket end:#36960 .by rupesh
			foreach (var obj in picker.ItemsSource)
			{
				var value = string.Empty;
				if (picker.DisplayProperty != null)
				{
					var prop = obj.GetType().GetRuntimeProperties().FirstOrDefault(p => string.Equals(p.Name, picker.DisplayProperty, StringComparison.OrdinalIgnoreCase));

					if (prop != null)
						value = prop.GetValue(obj).ToString();
				}
				else
				{
					value = obj.ToString();
				}

				if (!picker.Items.Contains(value))
				{
					picker.Items.Add(value);
				}

				if (picker.SelectedItem != null && picker.SelectedItem == obj)
					picker.SelectedIndex = count;

				count++;
			}

			if (picker.ItemsSource.Count == picker.Items.Count - 1)
				picker.SelectedIndex++;
		}
		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedIndex > -1)
			{
				SelectedItem = ItemsSource[SelectedIndex];
			}
		}
	}
}
