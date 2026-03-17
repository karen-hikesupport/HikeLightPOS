using System;
using System.Text.RegularExpressions;

namespace HikePOS.Behaviors
{
	public class EmailValidatorBehavior : Behavior<Entry>
	{
		const string emailRegex = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
			@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

		// Creating BindableProperties with Limited write access: http://iosapi.xamarin.com/index.aspx?link=M%3AXamarin.Forms.BindableObject.SetValue(Xamarin.Forms.BindablePropertyKey%2CSystem.Object) 

		static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(NumberValidatorBehavior), false);

		public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

		public bool IsValid
		{
			get { return (bool)base.GetValue(IsValidProperty); }
			set { base.SetValue(IsValidPropertyKey, value); }
		}

		protected override void OnAttachedTo(Entry bindable)
		{
			bindable.Unfocused += HandleTextUnfocus;
			bindable.TextChanged += HandleTextChanged;


			IsValid = true;
		}


		void HandleTextChanged(object sender, TextChangedEventArgs e)
		{
			IsValid = true;
			((Entry)sender).TextColor = Colors.Black;

		}

		void HandleTextUnfocus(object sender, FocusEventArgs e)
		{
			string textValue = ((Entry)sender).Text;
			if (!string.IsNullOrEmpty(textValue))
			{
				IsValid = (Regex.IsMatch(textValue, emailRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
				((Entry)sender).TextColor = IsValid ? Colors.Black : Colors.Red;
			}
			else
			{
				IsValid = true;
				((Entry)sender).TextColor = Colors.Black;
			}
		}


		protected override void OnDetachingFrom(Entry bindable)
		{
			bindable.Unfocused -= HandleTextUnfocus;
			bindable.TextChanged -= HandleTextChanged;

		}
	}
}
