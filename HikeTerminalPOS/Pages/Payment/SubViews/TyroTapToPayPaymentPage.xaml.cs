using HikePOS.ViewModels;
﻿using HikePOS.Pages.PhonePages.Payment;

namespace HikePOS;

public partial class TyroTapToPayPaymentPage : PopupBasePage<TyroTapToPayPaymentViewModel>
{

    public TyroTapToPayPaymentPage()
    {
        InitializeComponent();
    }

    async void CloseHandle_Clicked(object sender, System.EventArgs e)
    {
            await Close();
    }

    public async Task Close()
    {
        try
        {

            if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
            }
        }
        catch (Exception ex)
        {
            ex.Track();
        }
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new TapToPayEducationPagePhone());
    }
}
