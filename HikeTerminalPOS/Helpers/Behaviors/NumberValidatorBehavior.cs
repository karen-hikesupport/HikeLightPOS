
namespace HikePOS.Behaviors
{
	public class NumberValidatorBehavior : Behavior<Entry>
	{
		// Creating BindableProperties with Limited write access: http://iosapi.xamarin.com/index.aspx?link=M%3AXamarin.Forms.BindableObject.SetValue(Xamarin.Forms.BindablePropertyKey%2CSystem.Object) 

		static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(NumberValidatorBehavior), false);

		public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

		public bool IsValid
		{
			get { return (bool)base.GetValue(IsValidProperty); }
			private set { base.SetValue(IsValidPropertyKey, value); }
		}

		protected override void OnAttachedTo(Entry bindable)
		{
			bindable.TextChanged += bindable_TextChanged;
		}

		void bindable_TextChanged(object sender, TextChangedEventArgs e)
		{
			double result;
			IsValid = double.TryParse(e.NewTextValue, out result);
			((Entry)sender).TextColor = IsValid ? Colors.Black: Colors.Red;

		}

		protected override void OnDetachingFrom(Entry bindable)
		{
			bindable.TextChanged -= bindable_TextChanged;
		}
	}
}

