using System;

namespace HikePOS.UserControls
{
	public class CustomDatePicker : DatePicker
	{
		public static readonly BindableProperty NullableDateProperty =
            BindableProperty.Create(nameof(NullableDate), typeof(DateTime?), typeof(CustomDatePicker), null,BindingMode.TwoWay,null,NullableDateChanged);

        public DateTime? NullableDate
		{
			get { return (DateTime?)GetValue(NullableDateProperty); }
			set { SetValue(NullableDateProperty, value); }
		}

		public static readonly BindableProperty AllowNullDateProperty =
			BindableProperty.Create(nameof(AllowNullDate), typeof(bool), typeof(CustomDatePicker), false);

		public bool AllowNullDate
		{
			get { return (bool)GetValue(AllowNullDateProperty); }
			set { SetValue(AllowNullDateProperty, value); }
		}

		public static readonly BindableProperty DisplayBorderProperty =
            BindableProperty.Create(nameof(DisplayBorder), typeof(bool), typeof(CustomDatePicker), true);

		public bool DisplayBorder
		{
			get { return (bool)GetValue(DisplayBorderProperty); }
			set { SetValue(DisplayBorderProperty, value); }
		}

       
		
        public CustomDatePicker()
		{
            try
            {
                this.DateSelected += CustomDatePicker_DateSelected;
            }
            catch(Exception ex)
            {
                ex.Track();
            }
		}

        ~CustomDatePicker()
        {
            try
            {
                this.DateSelected -= CustomDatePicker_DateSelected;
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }
		
        void CustomDatePicker_DateSelected(object sender, DateChangedEventArgs e)
		{
            try
            {
                if (e != null && e.NewDate != null)
                {
                    this.NullableDate = new DateTime(
                        e.NewDate.Year,
                        e.NewDate.Month,
                        e.NewDate.Day);

                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
		}

        static void NullableDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            try
            {
                if (bindable != null)
                {
                    var customDatePicker = bindable as CustomDatePicker;
                    if (customDatePicker != null)
                    {
                        if (newValue != null)
                        {
                            customDatePicker.Date = Convert.ToDateTime(newValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
    }
}
