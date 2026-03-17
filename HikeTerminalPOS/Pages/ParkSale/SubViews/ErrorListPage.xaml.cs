using System;
using System.Threading.Tasks;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class ErrorListPage : PopupBasePage<ErrorListViewModel>
    {
        public ErrorListPage()
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
    }
}