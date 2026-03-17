using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class CreateNewOrAddToExistingPO : MainPopupBasePage
    {
        public CreateNewOrAddToExistingPO()
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
                    await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            PendingOrderPicker.SelectedIndex = -1;
        }
    }
}
