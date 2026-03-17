using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.Pages;

public partial class SurchargePopupPage : PopupBasePage<BaseViewModel>
{
    public EventHandler<bool> CancelEvent;  // Start #90942 by pratik 

    public SurchargePopupPage(decimal payment, decimal surcharge)
    {
        InitializeComponent();
        lblpayment.Text = Math.Round(payment, Helpers.Settings.StoreDecimalDigit).ToString("C");
        lblsurcharge.Text = Math.Round(surcharge, Helpers.Settings.StoreDecimalDigit).ToString("C");
        btntopay.Text = "To Pay (" + Math.Round((payment + surcharge), Helpers.Settings.StoreDecimalDigit).ToString("C") + ")";
    }

    async void OkHandle_Clicked(object sender, System.EventArgs e)
    {
        try
        {
            CancelEvent?.Invoke(this,false); // Start #90942 by pratik 
            if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
           
        }
        catch (Exception ex)
        {
            ex.Track();
        }
    }

    async void btntopay_Clicked(System.Object sender, System.EventArgs e)
    {
        if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
           await Navigation.PopModalAsync();
        WeakReferenceMessenger.Default.Send(new Messenger.SurchargeApplyMessenger(true));
    }
}
