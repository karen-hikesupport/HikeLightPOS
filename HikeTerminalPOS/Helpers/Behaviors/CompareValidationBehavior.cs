using System;
namespace HikePOS.Behaviors
{
	public class CompareValidationBehavior : Behavior<Entry>
	{
		public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text),typeof(string),typeof(CompareValidationBehavior), string.Empty, BindingMode.TwoWay);

		public string Text
		{
			get
			{
				return (string)GetValue(TextProperty);
			}
			set
			{
				SetValue(TextProperty, value);
			}
		}

		static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(EntryValidatorBehavior), false);

		public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

		public bool IsValid
		{
			get { return (bool)base.GetValue(IsValidProperty); }
			set { base.SetValue(IsValidPropertyKey, value); }
		}

		public static readonly BindableProperty ErrorMessageProperty =
			BindableProperty.Create("ErrorMessage", typeof(string), typeof(EntryValidatorBehavior), string.Empty);
		

		public string ErrorMessage
		{
			get
			{
				return (string)GetValue(ErrorMessageProperty);
			}
			set
			{
				SetValue(ErrorMessageProperty, value);
			}
		}


		public static readonly BindableProperty InValidMessageProperty =
			BindableProperty.Create("InValidMessage", typeof(string), typeof(EntryValidatorBehavior), string.Empty);


		public string InValidMessage
		{
			get
			{
				return (string)GetValue(InValidMessageProperty);
			}
			set
			{
				SetValue(InValidMessageProperty, value);
			}
		}


		protected override void OnAttachedTo(Entry bindable)
		{
			bindable.TextChanged += HandleTextChanged;
			bindable.Focused += HandleTextFocused;
			base.OnAttachedTo(bindable);
		}

		void HandleTextFocused(object sender, FocusEventArgs e)
		{
			//IsValid = false;
			ErrorMessage = string.Empty;
		}


		void HandleTextChanged(object sender, TextChangedEventArgs e)
		{
			IsValid = e.NewTextValue == Text;
			ErrorMessage = IsValid ? string.Empty : InValidMessage;

			//bool IsValid = false;
			//IsValid = e.NewTextValue == Text;

			//((Entry)sender).TextColor = IsValid ? Color.Default : Color.Red;
		}

		protected override void OnDetachingFrom(Entry bindable)
		{
			bindable.TextChanged -= HandleTextChanged;
			//bindable.Unfocused -= HandleTextUnfocus;
			bindable.Focused -= HandleTextFocused;
			base.OnDetachingFrom(bindable);
		}


	}
}
