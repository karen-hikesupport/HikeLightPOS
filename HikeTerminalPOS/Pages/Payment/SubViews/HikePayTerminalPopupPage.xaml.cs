using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace HikePOS.Pages;

public partial class HikePayTerminalPopupPage : PopupBasePage<BaseViewModel>
{
    public Func<Task>? OnCancel { get; set; }

    public HikePayTerminalPopupPage()
    {
        InitializeComponent();

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
    async void btnCancel_Clicked(System.Object sender, System.EventArgs e)
    {
        if (OnCancel != null)
            await OnCancel();

    }

}
