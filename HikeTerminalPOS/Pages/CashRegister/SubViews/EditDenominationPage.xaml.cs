using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core.Platform;
using HikePOS.Models;
using HikePOS.UserControls;
using HikePOS.ViewModels;

namespace HikePOS
{

    public partial class EditDenominationPage : PopupBasePage<EditDenominationViewModel>
    {
        public EditDenominationPage()
        {
            InitializeComponent();
        }

        void DenominationHandle_Unfocused(object sender, FocusEventArgs e)
        {
            CustomEntry SenderItem = (CustomEntry)sender;
            if (SenderItem != null && (string.IsNullOrEmpty(SenderItem.Text) || SenderItem.Text == "0") && SenderItem.ItemId == 0)
            {
                ViewModel.lstDenomination.RemoveAt(0);
            }
        }

        void DenominationHandle_Focused(object sender, FocusEventArgs e)
        {
            try
            {
                CustomEntry item = (CustomEntry)sender;
                //Ticket #12945 Denominations Calculator freezing.By Nikhil	
                if (!string.IsNullOrEmpty(item.Text) && item.Text == "0")
                {
                    item.Text = string.Empty;
                }
                //Ticket #12945 End.By Nikhil
                ViewModel.lstDenomination.All(x =>
                {
                    x.IsEditable = (x.Id == item.ItemId);
                    return true;
                });

            }
            catch (Exception ex)
            {
                ex.Track();
            }

        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            DenominationDto param = button.CommandParameter as DenominationDto;
            try
            {
                if(DeviceInfo.Platform == DevicePlatform.Android)
                {
                    CustomEntry entry = (((Border)((StackLayout)button.Parent).Children[0]).Content as CustomEntry);
                    entry.IsEnabled=false;
                    entry.HideKeyboardAsync();
                    entry.IsEnabled=true;
                }
            }
            catch(Exception ex)
            { 
                ex.Track();
            }
            ViewModel.SaveDenominationCommand.Execute(param);
        }

    }
}
