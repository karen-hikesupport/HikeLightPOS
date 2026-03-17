using System.Globalization;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using Font = Microsoft.Maui.Graphics.Font;

namespace HikePOS.UserControls
{
	public class CustomEntry : Entry
	{

		public CustomEntry()
		{
			this.TextChanged += bindable_TextChanged;
		}

		~CustomEntry() 
		{ 
			this.TextChanged -= bindable_TextChanged;
		}

		public static readonly BindableProperty BorderWidthProperty =
			BindableProperty.Create("BorderWidth", typeof(double), typeof(CustomEntry), double.MinValue);

		public double BorderWidth { 
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public static readonly BindableProperty FontProperty =
			BindableProperty.Create("Font", typeof(Font), typeof(CustomEntry), new Font());

		public Font Font {
			get { return (Font)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}

		public static readonly BindableProperty XAlignProperty =
			BindableProperty.Create("XAlign", typeof(TextAlignment), typeof(CustomEntry), TextAlignment.Start);

		public TextAlignment XAlign {
			get { return (TextAlignment)GetValue(XAlignProperty); }
			set { SetValue(XAlignProperty, value); }
		}

		public static readonly BindableProperty IsPhoneNumberEntryProperty =
			BindableProperty.Create("IsPhoneNumberEntry", typeof(bool), typeof(CustomEntry), false);

		public bool IsPhoneNumberEntry {
			get { return (bool)GetValue(IsPhoneNumberEntryProperty); }
			set { SetValue(IsPhoneNumberEntryProperty, value); }
		}

		public static readonly BindableProperty IsEmailProperty =
			BindableProperty.Create("IsEmail", typeof(bool), typeof(CustomEntry), false, defaultBindingMode: BindingMode.TwoWay);

		public bool IsEmail {
			get { return (bool)GetValue(IsEmailProperty); }
			set { SetValue(IsEmailProperty, value); }
		}

		public static readonly BindableProperty OnlyAllowDecimalValueProperty = 
			BindableProperty.Create("OnlyAllowDecimalValue", typeof(bool), typeof(CustomEntry), false);

		public bool OnlyAllowDecimalValue
		{
			get { return (bool)GetValue(OnlyAllowDecimalValueProperty); }
			set { SetValue(OnlyAllowDecimalValueProperty, value); }
		}


        public static readonly BindableProperty OnlyAllowNumericValueProperty =
            BindableProperty.Create("OnlyAllowNumericValue", typeof(bool), typeof(CustomEntry), false);

        public bool OnlyAllowNumericValue
        {
            get { return (bool)GetValue(OnlyAllowNumericValueProperty); }
            set { SetValue(OnlyAllowNumericValueProperty, value); }
        }

		void bindable_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (OnlyAllowDecimalValue)
			{
				Entry textBox = sender as Entry;
				if (textBox != null && textBox.Text != null)
				{
					string newText = string.Empty;
                    string TextBoxValue = string.Empty;
                    int count = 0;

                    var CrossMultilingual = DependencyService.Get<IMultilingual>();
                    var DecimalSeparator = CrossMultilingual.CurrentCultureInfo.NumberFormat.CurrencyDecimalSeparator[0];
                    TextBoxValue = textBox.Text.Replace(CrossMultilingual.CurrentCultureInfo.NumberFormat.CurrencySymbol, "");
                    foreach (char c in TextBoxValue.ToCharArray())
					{
                        if (char.IsDigit(c) || char.IsControl(c) || (c == DecimalSeparator && count == 0) || (c == '-' && string.IsNullOrEmpty(newText)))
						{
							newText += c;
                            if (c == DecimalSeparator)
								count += 1;
						}
					}
					((Entry)sender).Text = newText;
				}
			}
            if (OnlyAllowNumericValue)
            {
                Entry textBox = sender as Entry;
                if (textBox != null && textBox.Text != null)
                {
                    string newText = string.Empty;
                    foreach (char c in textBox.Text.ToCharArray())
                    {
                        if (char.IsDigit(c))
                        {
                            newText += c;
                        }
                    }
                    ((Entry)sender).Text = newText;
                }
            }
		}


        public static readonly BindableProperty ItemIdProperty =
            BindableProperty.Create("ItemId", typeof(int), typeof(CustomEntry), 0);

        public int ItemId
        {
            get { return (int)GetValue(ItemIdProperty); }
            set { SetValue(ItemIdProperty, value); }
        }
	}


    public class CustomEditor : Editor{

		public static readonly BindableProperty BorderWidthProperty =
			BindableProperty.Create("BorderWidth", typeof(double), typeof(CustomEditor), double.MinValue);

		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public static readonly BindableProperty FontProperty =
			BindableProperty.Create("Font", typeof(Font), typeof(CustomEditor), new Font());

		public Font Font
		{
			get { return (Font)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}

		

		public static readonly BindableProperty XAlignProperty =
			BindableProperty.Create("XAlign", typeof(TextAlignment), typeof(CustomEditor), TextAlignment.Start);

		public TextAlignment XAlign
		{
			get { return (TextAlignment)GetValue(XAlignProperty); }
			set { SetValue(XAlignProperty, value); }
		}
    }
}