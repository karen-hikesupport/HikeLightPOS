using System;
using System.Windows.Input;

namespace HikePOS.ViewModels
{
	public class MintTermsAndConditionViewModel : BaseViewModel
	{
        public event EventHandler<bool> Accepted;
        public ICommand AcceptCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public MintTermsAndConditionViewModel()
        {
            AcceptCommand = new Command(Accept);
            CloseCommand = new Command(CloseTapped);
        }

        private async void CloseTapped()
        {
            Accepted?.Invoke(this, false);
            await Close();
        }

        async void Accept()
        {
            try
            {
                await Close();
                Accepted?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
    }
}

