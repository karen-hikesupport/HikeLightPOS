using System.Windows.Input;
namespace HikePOS.UserControls
{
	public class CustomSwitch : Switch
	{
		public static readonly BindableProperty ToggleCommandProperty =
			BindableProperty.Create(nameof(ToggleCommand),typeof(ICommand),typeof(CustomSwitch), default(Command), BindingMode.TwoWay, propertyChanged: HandleBindingPropertyChangedDelegate);


		public ICommand ToggleCommand
		{
			get { return (ICommand)GetValue(ToggleCommandProperty); }
			set { SetValue(ToggleCommandProperty, value); }
		}

		static void HandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = bindable as CustomSwitch;
			picker.ToggleCommand = (ICommand)newValue;
		}

		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CustomSwitch), default(object), BindingMode.TwoWay, propertyChanged: OnCommandParameterChanged);

		static void OnCommandParameterChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var picker = bindable as CustomSwitch;
			picker.CommandParameter = newvalue;
		}

		public object CommandParameter
		{
			get { return (object)GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		public CustomSwitch()
		{
			this.Toggled += (sender, e) =>
			{
				if (ToggleCommand != null && CommandParameter != null)
				{
					ToggleCommand.Execute(CommandParameter);
				}
			};
		}
	}
}
