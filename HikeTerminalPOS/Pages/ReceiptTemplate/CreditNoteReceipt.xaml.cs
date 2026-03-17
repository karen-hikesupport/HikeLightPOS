using System.Threading.Tasks;
using HikePOS.Helpers;
using HikePOS.Models;

namespace HikePOS;

public partial class CreditNoteReceipt : ScrollView
{
    public CreditNoteReceipt()
    {
        InitializeComponent();
    }

    public async Task<bool> UpdateData(ReceiptTemplateDto CurrentReceiptTemplate, CustomerDto_POS customerDto)
    {
        decimal TotalLoyaltySpent = 0;
        if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.LoyaltyPointsValue > 0 && customerDto.CurrentLoyaltyBalance > 0)
            TotalLoyaltySpent = customerDto.CurrentLoyaltyBalance / Settings.StoreGeneralRule.LoyaltyPointsValue;

        lblCurrentLoyaltyBalance.Text = customerDto.CurrentLoyaltyBalance.ToString("C") + "(" + TotalLoyaltySpent.ToString("C") + ")";
        if (Settings.StoreShopDto.LogoImagePath != null && CurrentReceiptTemplate.Displaylogo)
        {
            Displaylogo.IsVisible = CurrentReceiptTemplate.Displaylogo;
            Displaylogo.Source = ImageSource.FromUri(Settings.StoreShopDto.LogoImagePath.GetImageUrl("Store"));
            StoreNameLable.FontSize = 30;
        }
        else
        {
            Displaylogo.IsVisible = false;
            StoreNameLable.FontSize = 50;
        }

        var GenderList = new string[] { "Male", "Female", "Prefer not to say" };
        if (customerDto.Gender < 3 && customerDto.Gender >= 0)
            lblGender.Text = GenderList[customerDto.Gender];
        else
            lblGender.Text = "Prefer not to say";

        if (customerDto.BirthDate.HasValue)
        {
            if (Settings.StoreGeneralRule.DoNotAskForTheYearInTheBirthDateOfTheCustomers)
            {
                lbldob.Text = customerDto.BirthDate.Value.ToString("dd MMM");
            }
            else
            {
                lbldob.Text = customerDto.BirthDate.Value.ToString("dd MMM yyyy");
            }
        }

        if (!customerDto.OutStandingBalance.HasValue)
        {
            customerDto.OutStandingBalance = 0;
            lblOSB.Text = customerDto.OutStandingBalance.Value.ToString("C");
        }
        else
            lblOSB.Text = customerDto.OutStandingBalance.Value.ToString("C");

        if (!customerDto.CreditBalance.HasValue)
            customerDto.CreditBalance = 0;
        if (!customerDto.CreditLimitIssue.HasValue)
            customerDto.CreditLimitIssue = 0;
        if (!customerDto.CreditLimitRedeemed.HasValue)
            customerDto.CreditLimitRedeemed = 0;

        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        await Task.Delay(1000);
        //if (Displaylogo.Source != null && !Displaylogo.Source.IsEmpty && Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        //{
        //    var img = await Displaylogo.GetImageAsPngAsync();
        //    if (img == null)
        //    {
        //        Displaylogo.Finish += async (sender, e) =>
        //        {

        //            img = await Displaylogo.GetImageAsPngAsync();
        //            if ((e.ScheduledWork.IsCompleted || e.ScheduledWork.IsCancelled) && !tcs.Task.IsCompleted)
        //                tcs?.SetResult(true);
        //        };
        //    }
        //    else
        //    {
        //        tcs.SetResult(true);

        //    }

        //}
        //else
        //{
        //    tcs.SetResult(true);

        //}
        UpdateLogoSize(CurrentReceiptTemplate);
        return await tcs.Task;
    }

    private void UpdateLogoSize(ReceiptTemplateDto CurrentReceiptTemplate)
    {

        if (CurrentReceiptTemplate.ReceipStyleFormat?.LogoSize == LogoSize.Large)
        {
            Displaylogo.WidthRequest = Displaylogo.HeightRequest = 380;
        }
        else if (CurrentReceiptTemplate.ReceipStyleFormat?.LogoSize == LogoSize.Small)
        {
            Displaylogo.WidthRequest = Displaylogo.HeightRequest = 180;
        }
        else
        {
            Displaylogo.WidthRequest = Displaylogo.HeightRequest = 280;
        }

    }
}
//End Ticket #74631 by pratik - add page
