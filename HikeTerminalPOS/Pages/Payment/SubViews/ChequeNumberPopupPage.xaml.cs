using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.Pages;

// Start #90942 iOS:FR Cheque number for sale by pratik
public partial class ChequeNumberPopupPage : PopupBasePage<BaseViewModel>
{
    //Start #91991 By Pratik
    public event EventHandler<string> AddReferenceNumber;
    public ChequeNumberPopupPage(string popupTitle = "")
    {
        InitializeComponent();

        if (!string.IsNullOrEmpty(popupTitle))
            PaymentReferenceID.Text = popupTitle;
        else
            PaymentReferenceID.Text = LanguageExtension.Localize("PaymentReferenceID");
        if (PaymentReferenceID.Text.ToLower() != LanguageExtension.Localize("PaymentReferenceID").ToLower())
            btnContinue.IsEnabled = true;
    }
    //End #91991 By Pratik

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
        //Start #91991 By Pratik
        if (PaymentReferenceID.Text.ToLower() == LanguageExtension.Localize("PaymentReferenceID").ToLower())
            WeakReferenceMessenger.Default.Send(new Messenger.ChequeNumberMessenger(txtReference.Text));
        else
            AddReferenceNumber?.Invoke(this, txtReference.Text);
        //End #91991 By Pratik
    }

    private void txtReference_TextChanged(object sender, TextChangedEventArgs e)
    {       
        SetButtonEnable();
    }

    void SetButtonEnable()
    {
        if (PaymentReferenceID.Text.ToLower() == LanguageExtension.Localize("PaymentReferenceID").ToLower())  //Start #91991 By Pratik
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
}
// End #90942  by pratik
