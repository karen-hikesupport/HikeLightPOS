using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.Pages;

// Start #94565 by pratik
public partial class OrderNamePopupPage : PopupBasePage<BaseViewModel>
{
    public event EventHandler<string> AddOrderName;
    public OrderNamePopupPage(string popupTitle = "")
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetButtonEnable();
    }

    async void btnContinue_Clicked(System.Object sender, System.EventArgs e)
    {
        if ((string.IsNullOrWhiteSpace(txtReference.Text) || string.IsNullOrEmpty(txtReference.Text)) && PaymentReferenceID.Text.ToLower() == LanguageExtension.Localize("PaymentReferenceID").ToLower())
            return;

        if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
            await Navigation.PopModalAsync();
        AddOrderName?.Invoke(this, txtReference.Text);
    }

    async void btnCancel_Clicked(System.Object sender, System.EventArgs e)
    {
        if (Navigation.ModalStack != null && Navigation.ModalStack.Count > 0)
            await Navigation.PopModalAsync();
    }

    private void txtReference_TextChanged(object sender, TextChangedEventArgs e)
    {
        SetButtonEnable();
    }

    void SetButtonEnable()
    {
        if (string.IsNullOrWhiteSpace(txtReference.Text) || string.IsNullOrEmpty(txtReference.Text))
        {
            btnContinue.IsEnabled = false;
            btnContinue.BackgroundColor = HikePOS.Resources.AppColors.ProductLightgrayColor;
        }
        else
        {
            btnContinue.IsEnabled = true;
            btnContinue.BackgroundColor = HikePOS.Resources.AppColors.NavigationBarBackgroundColor;
        }
    }
}
