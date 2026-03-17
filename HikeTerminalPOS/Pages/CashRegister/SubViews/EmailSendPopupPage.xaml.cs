using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using HikePOS.Resources;

namespace HikePOS.Pages;
//Start #92768 Pratik
public partial class EmailSendPopupPage : PopupBasePage<BaseViewModel>
{
    public event EventHandler<List<object>> FillFormed;
    public List<string> EmailLists;

    public EmailSendPopupPage(string _txtSubject)
    {
        InitializeComponent();
        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            maingrid.WidthRequest = 320;
        else
            maingrid.WidthRequest = 500;
            
        EmailLists = new List<string>();
        txtSubject.Text = _txtSubject;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetButtonEnable();
        txtemail.Loaded += txtemail_Loaded;
        txtemail.Text = "";
    }

    private async void txtemail_Loaded(object sender, EventArgs e)
    {
       await Task.Delay(100);
       txtemail.Focus();
    }

    void btnContinue_Clicked(object sender, EventArgs e)
    {
        BorderLessEntry_Completed(txtemail,null);
        if (EmailLists.Count > 0)
        {
            List<object> list = new List<object>();
            list.Add(EmailLists);
            list.Add(txtSubject.Text ?? string.Empty);
            FillFormed?.Invoke(this, list);
        }
    }

    void btnClosed_Clicked(object sender, EventArgs e)
    {
        ViewModel.ClosePopupCommand.Execute(null);
    }

    void SetButtonEnable()
    {
        if (EmailLists.Count > 0)
        {
            emailSV.IsVisible = true;
            btnContinue.IsEnabled = true;
            btnContinue.BackgroundColor = AppColors.NavigationBarBackgroundColor;
        }
        else
        {
            btnContinue.IsEnabled = false;
            btnContinue.BackgroundColor = AppColors.ProductLightgrayColor;
            emailSV.IsVisible = false;
            if (!string.IsNullOrWhiteSpace(txtemail.Text))
            {
                if (txtemail.Text.IsValidEmail())
                {
                    btnContinue.IsEnabled = true;
                    btnContinue.BackgroundColor = AppColors.NavigationBarBackgroundColor;
                }
            }
        }
    }

    private void BorderLessEntry_Completed(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtemail.Text))
        {
            if (txtemail.Text.IsValidEmail())
            {
                if (!EmailLists.Any(a => a.ToLower() == txtemail.Text.ToLower()))
                {
                    BindableLayout.SetItemsSource(vstemaillist, null);
                    EmailLists.Add(txtemail.Text);
                    BindableLayout.SetItemsSource(vstemaillist, EmailLists);
                    SetButtonEnable();
                }
                txtemail.Text = string.Empty;
            }
            else
            { 
                App.Instance.Hud.DisplayToast(LanguageExtension.Localize("Email_InValidMessage"), Colors.Red, Colors.White);
            }
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        BindableLayout.SetItemsSource(vstemaillist, null);
        EmailLists.Remove(EmailLists.First(a => a.ToLower() == e.Parameter.ToString().ToLower()));
        BindableLayout.SetItemsSource(vstemaillist, EmailLists);
        SetButtonEnable();
    }

    private void txtemail_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtemail.Text))
        {
            if (txtemail.Text.IsValidEmail())
            {
                btnContinue.IsEnabled = true;
                btnContinue.BackgroundColor = AppColors.NavigationBarBackgroundColor;
            }
            else
            {
                SetButtonEnable();
            }
        }
    }
}
//End #92768 Pratik
