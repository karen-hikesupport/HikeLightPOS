using System;
using HikePOS.Resources;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class ParkSaleFilterDateRangeMenuView : ContentView
    {
		public event EventHandler<string> ItemChanged;

		public ParkSaleFilterDateRangeMenuView()
        {
            InitializeComponent();
            FromDate.Unfocused += FromDate_Unfocused;
            ToDate.Unfocused += ToDate_Unfocused;

            FromDate.Focused += FromDate_Focused;
            ToDate.Unfocused += ToDate_Focused;
        }

        ~ParkSaleFilterDateRangeMenuView()
        {
            //FromDate.DateSelected+= FromDate_DateSelected;
            FromDate.Unfocused -= FromDate_Unfocused;
            ToDate.Unfocused -= ToDate_Unfocused;

            FromDate.Focused -= FromDate_Focused;
            ToDate.Unfocused -= ToDate_Focused;

            //ToDate.DateSelected += ToDate_DateSelected;
        }

        void FromDate_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                if (e != null && sender != null)
                {
                    var Picker = (DatePicker)sender;
                    ToDate.MinimumDate = Picker.Date;
                    frmFromDate.Stroke = AppColors.LightBordersColor;
                    ToDate.Focus();
                    frmToDate.Stroke = AppColors.DarkButtonBorderColor;
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }

        void FromDate_Focused(object sender, FocusEventArgs e)
        {
            try
            {
                if (e != null && sender != null)
                {
                    frmFromDate.Stroke = AppColors.DarkButtonBorderColor;
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }

        }

        void ToDate_Focused(object sender, FocusEventArgs e)
        {
            try
            {
                if (e != null && sender != null)
                {
                    //frmToDate.OutlineColor = AppColors.DarkButtonBorderColor;
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }

        }

        void ToDate_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1);
                    if (e != null && sender != null && !FromDate.IsFocused)
                    {
                        frmToDate.Stroke = AppColors.LightBordersColor;
                        var Picker = (DatePicker)sender;
                        ((ParkSaleViewModel)this.BindingContext).SelectFilterDateRange("CustomRange");
                    }
                });
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }



        void DateRangeHandle_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                ItemChanged?.Invoke(this, ((Button)sender).Text.ToString());
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }

    }
}
