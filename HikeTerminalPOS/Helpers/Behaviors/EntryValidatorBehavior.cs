using System;
using System.Text.RegularExpressions;
using HikePOS.Enums;
using HikePOS.Helpers;

namespace HikePOS.Behaviors
{
	public class EntryValidatorBehavior : Behavior<Entry>
	{
		public string RegX
		{
			get
			{
				switch (HikeEntryType)
				{
					case EntryType.Default:
						return RegxValues.DefaultRegX;
					case EntryType.Email:
						return RegxValues.EmailRegx;
					case EntryType.Password:
						return RegxValues.PasswordRegx;
					case EntryType.UserPassword:
						return RegxValues.UserPasswordRegx;
					case EntryType.StoreName:
						//return RegxValues.StoreNameRegx;
                        return RegxValues.DefaultRegX;
					default:
						return RegxValues.DefaultRegX;
				}
			}
		}

		static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool?), typeof(EntryValidatorBehavior), null);

		public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

		public bool IsValid
		{
			get
			{
				if (base.GetValue(IsValidProperty) == null)
				{
					return false;
				}
				else
				{
					return (bool)base.GetValue(IsValidProperty);
				}
			}
			set { base.SetValue(IsValidPropertyKey, value); }
		}


		public static readonly BindableProperty HikeEntryTypeProperty =
			BindableProperty.Create("HikeEntryType", typeof(EntryType), typeof(EntryValidatorBehavior), EntryType.Default);


		public EntryType HikeEntryType
		{
			get
			{
				return (EntryType)GetValue(HikeEntryTypeProperty);
			}
			set
			{
				SetValue(HikeEntryTypeProperty, value);
			}
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


		public static readonly BindableProperty EmptyMessageProperty =
			BindableProperty.Create("EmptyMessage", typeof(string), typeof(EntryValidatorBehavior), string.Empty);


		public string EmptyMessage
		{
			get
			{
				return (string)GetValue(EmptyMessageProperty);
			}
			set
			{
				SetValue(EmptyMessageProperty, value);
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
			bindable.Unfocused += HandleTextUnfocus;
			bindable.TextChanged += HandleTextChanged;
			bindable.Focused += HandleTextFocused;

			//IsValid = false;
			//bindable.TextColor = Color.Default;
			//ErrorMessage = string.Empty;
			IsValid = true;

		}


		//void HandleTextChanged(object sender, TextChangedEventArgs e)
		//{
		//	IsValid = true;
		//	((Entry)sender).TextColor = Color.Default;
		//	ErrorMessage = string.Empty;
		//}

		void HandleTextFocused(object sender, FocusEventArgs e)
		{
			//IsValid = false;
			//((Entry)sender).TextColor = Color.Default;
			ErrorMessage = string.Empty;
			IsValid = true;

		}

		void HandleTextUnfocus(object sender, FocusEventArgs e)
		{
			string textVlaue = ((Entry)sender).Text;
			if (!string.IsNullOrEmpty(textVlaue))
			{
                 IsValid = (Regex.IsMatch(textVlaue, RegX, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
                    //((Entry)sender).TextColor = IsValid ? Color.Default : Color.Red;
                    ErrorMessage = IsValid ? string.Empty : InValidMessage;
			}
			else
			{
				IsValid = false;
				//((Entry)sender).TextColor = Color.Red;
				ErrorMessage = EmptyMessage;
			}
		}

		protected override void OnDetachingFrom(Entry bindable)
		{
			bindable.Unfocused -= HandleTextUnfocus;
			bindable.TextChanged -= HandleTextChanged;
			bindable.Focused -= HandleTextFocused;
		}

		void HandleTextChanged(object sender, TextChangedEventArgs e)
		{
			string textVlaue = ((Entry)sender).Text;
			if (!string.IsNullOrEmpty(textVlaue))
			{
				IsValid = (Regex.IsMatch(textVlaue, RegX, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
				//((Entry)sender).TextColor = IsValid ? Color.Default : Color.Red;
				ErrorMessage = IsValid ? string.Empty : InValidMessage;
			}
			else
			{
				IsValid = false;
				//((Entry)sender).TextColor = Color.Red;
				ErrorMessage = EmptyMessage;
			}
		}
	}
}
