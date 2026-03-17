using System;
using System.Collections;

namespace HikePOS
{
    public class CustomCollectionView : View
    {
        public CustomCollectionView()
        {
            SelectionEnabled = true;
        }

        public EventHandler<bool> LoadMore;


        // The items source property
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(CustomCollectionView), null, BindingMode.OneWay, null, null, null, null);

        // The item template property
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(CustomCollectionView), null, BindingMode.OneWay, null, null, null, null);

        // The item template property
        public static readonly BindableProperty ItemTemplateSelectorProperty = BindableProperty.Create("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(CustomCollectionView), null, BindingMode.OneWay, null, null, null, null);


        // The row spacing property
        public static readonly BindableProperty RowSpacingProperty = BindableProperty.Create("RowSpacing", typeof(double), typeof(CustomCollectionView), 0.0, BindingMode.OneWay, null, null, null, null);

        // The column spacing property
        public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create("ColumnSpacing", typeof(double), typeof(CustomCollectionView), 0.0, BindingMode.OneWay, null, null, null, null);

        // The number of column property
        public static readonly BindableProperty NoOfColumnProperty = BindableProperty.Create("NoOfColumn", typeof(double), typeof(CustomCollectionView), 0.0, BindingMode.OneWay, null, null, null, null);

        // The item width property
        public static readonly BindableProperty ItemWidthProperty = BindableProperty.Create("ItemWidth", typeof(double?), typeof(CustomCollectionView), null, BindingMode.OneWay, null, null, null, null);

        // The item height property
        public static readonly BindableProperty ItemHeightProperty = BindableProperty.Create("ItemHeight", typeof(double?), typeof(CustomCollectionView), null, BindingMode.OneWay, null, null, null, null);

        // The padding property
        public static readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(CustomCollectionView), new Thickness(0), BindingMode.OneWay);

        // The ScrollDirectionProperty property
        public static readonly BindableProperty ScrollDirectionProperty = BindableProperty.Create("ScrollDirection", typeof(CollectionScrollDirection), typeof(CustomCollectionView), CollectionScrollDirection.Vertical, BindingMode.OneWay, null, null, null, null);

        // The ScrollDirectionProperty property
        //public static readonly BindableProperty AutoWidthProperty = BindableProperty.Create("AutoWidth", typeof(CollectionScrollDirection), typeof(CustomCollectionView), false, BindingMode.OneWay, null, null, null, null);



        // Gets or sets the items source.
        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)base.GetValue(CustomCollectionView.ItemsSourceProperty);
            }
            set
            {
                base.SetValue(CustomCollectionView.ItemsSourceProperty, value);
            }
        }

        // Gets or sets the item template.
        public DataTemplate ItemTemplate
        {
            get
            {
                return (DataTemplate)base.GetValue(CustomCollectionView.ItemTemplateProperty);
            }
            set
            {
                base.SetValue(CustomCollectionView.ItemTemplateProperty, value);
            }
        }


        // Gets or sets the item template.
        public DataTemplateSelector ItemTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)base.GetValue(CustomCollectionView.ItemTemplateSelectorProperty);
            }
            set
            {
                base.SetValue(CustomCollectionView.ItemTemplateSelectorProperty, value);
            }
        }

        // Gets or sets the row spacing.
        public double RowSpacing
        {
            get
            {
                return (double)base.GetValue(CustomCollectionView.RowSpacingProperty);
            }
            set
            {
                base.SetValue(CustomCollectionView.RowSpacingProperty, value);
            }
        }

        // Gets or sets the column spacing.
        public double ColumnSpacing
        {
            get
            {
                return (double)base.GetValue(CustomCollectionView.ColumnSpacingProperty);
            }
            set
            {
                base.SetValue(CustomCollectionView.ColumnSpacingProperty, value);
            }
        }

        // Gets or sets the number of column.
        public double NoOfColumn
        {
            get
            {
                return (double)base.GetValue(CustomCollectionView.NoOfColumnProperty);
            }
            set
            {
                base.SetValue(CustomCollectionView.NoOfColumnProperty, value);
            }
        }

        // Gets or sets the width of the item.
        public double? ItemWidth
        {
            get
            {
                return (double?)base.GetValue(CustomCollectionView.ItemWidthProperty);
            }
            set
            {
                base.SetValue(CustomCollectionView.ItemWidthProperty, value);
            }
        }

        // Gets or sets the height of the item.
        public double? ItemHeight
        {
            get
            {
                return (double?)base.GetValue(CustomCollectionView.ItemHeightProperty);
            }
            set
            {
                base.SetValue(CustomCollectionView.ItemHeightProperty, value);
            }
        }

        // Gets or sets the padding.
        public Thickness Padding
        {
            get
            {
                return (Thickness)base.GetValue(PaddingProperty);
            }
            set
            {
                base.SetValue(PaddingProperty, value);
            }
        }

        // Occurs when item is selected.
        public event EventHandler<object> ItemSelected;

        // Invokes the item selected event.
        public void InvokeItemSelectedEvent(object sender, object item)
        {
            if (this.ItemSelected != null)
            {
                this.ItemSelected.Invoke(sender, item);
            }
        }

        // Gets or sets a value indicating whether [selection enabled].
        public bool SelectionEnabled
        {
            get;
            set;
        }





        // Gets or sets the Scroll direction.
        public CollectionScrollDirection ScrollDirection
        {
            get
            {
                return (CollectionScrollDirection)base.GetValue(CustomCollectionView.ScrollDirectionProperty);
            }
            set
            {
                base.SetValue(CustomCollectionView.ScrollDirectionProperty, value);
            }
        }


        // Gets or sets the Auto width.
        //public bool AutoWidth
        //{
        //	get
        //	{
        //		return (bool)base.GetValue(CustomCollectionView.AutoWidthProperty);
        //	}
        //	set
        //	{
        //		base.SetValue(CustomCollectionView.AutoWidthProperty, value);
        //	}
        //}


    }

    public enum CollectionScrollDirection
    {
        Vertical,
        Horizontal
    }
}