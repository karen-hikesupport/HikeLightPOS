using System;
using System.Threading.Tasks;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class AssemblyPaymentTransactionPage : PopupBasePage<AssemblyPaymentTransactionViewModel>
    {
        
        public AssemblyPaymentTransactionPage()
        {
            InitializeComponent();
            Title = "Assembly payment";
        }

        async void CloseHandle_Clicked(object sender, System.EventArgs e)
        {
            //Ticket start:#49047 iPad: Westpac payment is accepted by payment device but the sale is not completed.by rupesh
            if (SPICommonViewModel._spi.CurrentFlow != SPIClient.SpiFlow.Transaction)
                await Close();
            //Ticket start:#49047.by rupesh

        }

        public async Task Close()
        {
            try
            {
				await ViewModel.Cancel();
				         
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
    }
}
