using System;
namespace HikePOS.Behaviors
{
	public class AnimateSizeBehavior : Behavior<Entry>
	{
		public static readonly BindableProperty EasingFunctionProperty = BindableProperty.Create(nameof(EasingFunctionName),
			typeof(string),
			typeof(AnimateSizeBehavior),
			"SinIn",
			propertyChanged: OnEasingFunctionChanged);

		public static readonly BindableProperty ScaleProperty = BindableProperty.Create(
            nameof(Scale),
            typeof(double),
            typeof(AnimateSizeBehavior),
			1.25);

		Easing _easingFunction;

		public string EasingFunctionName
		{
			get { return (string)GetValue(EasingFunctionProperty); }
			set { SetValue(EasingFunctionProperty, value); }
		}

		public double Scale
		{
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		protected override void OnAttachedTo(Entry bindable)
		{
			bindable.Focused += OnItemFocused;
			bindable.SizeChanged += OnItemSizeChanged;
			base.OnAttachedTo(bindable);

		}

		protected override void OnDetachingFrom(Entry bindable)
		{
			bindable.Focused -= OnItemFocused;
			bindable.SizeChanged -= OnItemSizeChanged;
			base.OnDetachingFrom(bindable);
		}





		async void OnItemSizeChanged(object sender, EventArgs e)
		{
			await ((Entry)sender).ScaleTo(Scale, 250, _easingFunction);
			await ((Entry)sender).ScaleTo(1.00, 250, _easingFunction);
		}


		static Easing GetEasing(string easingName)
		{
			switch (easingName)
			{
				case "BounceIn": return Easing.BounceIn;
				case "BounceOut": return Easing.BounceOut;
				case "CubicInOut": return Easing.CubicInOut;
				case "CubicOut": return Easing.CubicOut;
				case "Linear": return Easing.Linear;
				case "SinIn": return Easing.SinIn;
				case "SinInOut": return Easing.SinInOut;
				case "SinOut": return Easing.SinOut;
				case "SpringIn": return Easing.SpringIn;
				case "SpringOut": return Easing.SpringOut;
				default: throw new ArgumentException(easingName + " is not valid");
			}
		}

		static void OnEasingFunctionChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			(bindable as AnimateSizeBehavior).EasingFunctionName = newvalue?.ToString();
			(bindable as AnimateSizeBehavior)._easingFunction = GetEasing(newvalue?.ToString());
		}

		async void OnItemFocused(object sender, FocusEventArgs e)
		{
			await ((Entry)sender).ScaleTo(Scale, 250, _easingFunction);
			await ((Entry)sender).ScaleTo(1.00, 250, _easingFunction);
		}




	}

}
