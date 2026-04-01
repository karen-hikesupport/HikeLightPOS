using HikePOS.ViewModels;

namespace HikePOS
{

       public partial class LoginUserPage : BaseContentPage<LoginUserViewModel>
    {
        public LoginUserPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ViewModel.DigitOne = Digit1;
         
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            if (entry == null)
                return;

            var newText = e.NewTextValue;
            var oldText = e.OldTextValue;

            // Focus forward	
            if (newText.Length == 1 && newText == " ")
            {
                entry.Text = "";
            }
            else if (!string.IsNullOrEmpty(newText) && newText.Length > 1)
            {
                entry.Text = newText.Last().ToString();
            }
            else if (!string.IsNullOrEmpty(newText) && newText.Length == 1)
            {
                if (entry == Digit1) Digit2.Focus();
                else if (entry == Digit2) Digit3.Focus();
                else if (entry == Digit3) Digit4.Focus();
                // else if (entry == Digit4) Digit5.Focus();
                // else if (entry == Digit5) Digit6.Focus();

                if (string.IsNullOrEmpty(Digit1.Text))
                {
                    Digit2.Text = string.Empty;
                    Digit3.Text = string.Empty;
                    Digit4.Text = string.Empty;
                    // Digit5.Text = string.Empty;
                    // Digit6.Text = string.Empty;
                }
            }



            // Backspace handling (only when going from 1 -> 0 character)
            if (string.IsNullOrEmpty(newText) && oldText?.Length == 1)
            {
                if (entry == Digit4) Digit3.Focus();
                // else if (entry == Digit5) Digit4.Focus();
                // else if (entry == Digit4) Digit3.Focus();
                else if (entry == Digit3) Digit2.Focus();
                else if (entry == Digit2) Digit1.Focus();
            }

            if (!string.IsNullOrEmpty(Digit1.Text) && !string.IsNullOrEmpty(Digit2.Text) && !string.IsNullOrEmpty(Digit3.Text) && !string.IsNullOrEmpty(Digit4.Text))
            {
                ViewModel.OTPEntry = Digit1.Text + Digit2.Text + Digit3.Text + Digit4.Text;
                Digit4.Unfocus();
                ViewModel.OTPCommand.Execute(null);
            }

        }

        private void Digit1_Focused(object sender, FocusEventArgs e)
        {
            if (!string.IsNullOrEmpty(Digit2.Text))
            {
                Digit2.Focus();
            }
        }

        private void Digit2_Focused(object sender, FocusEventArgs e)
        {
            if (string.IsNullOrEmpty(Digit1.Text))
            {
                Digit1.Focus();
            }
            else if (!string.IsNullOrEmpty(Digit3.Text))
            {
                Digit3.Focus();
            }
        }
        private void Digit3_Focused(object sender, FocusEventArgs e)
        {
            if (string.IsNullOrEmpty(Digit2.Text))
            {
                Digit2.Focus();
            }
            else if (!string.IsNullOrEmpty(Digit4.Text))
            {
                Digit4.Focus();
            }
        }
        private void Digit4_Focused(object sender, FocusEventArgs e)
        {
            if (string.IsNullOrEmpty(Digit3.Text))
            {
                Digit3.Focus();
            }
            // else if (!string.IsNullOrEmpty(Digit5.Text))
            // {
            //     Digit5.Focus();
            // }
        }
        // private void Digit5_Focused(object sender, FocusEventArgs e)
        // {
        //     if (string.IsNullOrEmpty(Digit4.Text))
        //     {
        //         Digit4.Focus();
        //     }
        //     else if (!string.IsNullOrEmpty(Digit6.Text))
        //     {
        //         Digit6.Focus();
        //     }
        // }
        // private void Digit6_Focused(object sender, FocusEventArgs e)
        // {
        //     if (string.IsNullOrEmpty(Digit5.Text))
        //     {
        //         Digit5.Focus();
        //     }
        // }

        private void Digit_OnBackspacePressed(object sender, EventArgs e)
        {
            var entry = sender as Entry;
            if (entry == null)
                return;

            string oldText = string.Empty;
            string newText = entry.Text;
            // if (entry == Digit6) oldText = Digit5.Text;
            // else if (entry == Digit5) oldText = Digit4.Text;
            if (entry == Digit4) oldText = Digit3.Text;
            else if (entry == Digit3) oldText = Digit2.Text;
            else if (entry == Digit2) oldText = Digit1.Text;

            if (string.IsNullOrEmpty(newText) && oldText?.Length == 1)
            {
                // if (entry == Digit6) Digit5.Focus();
                // else if (entry == Digit5) Digit4.Focus();
                if (entry == Digit4) Digit3.Focus();
                else if (entry == Digit3) Digit2.Focus();
                else if (entry == Digit2) Digit1.Focus();
            }

            if (!string.IsNullOrEmpty(Digit1.Text) && !string.IsNullOrEmpty(Digit2.Text) && !string.IsNullOrEmpty(Digit3.Text) && !string.IsNullOrEmpty(Digit4.Text))
            {
                ViewModel.OTPEntry = Digit1.Text + Digit2.Text + Digit3.Text + Digit4.Text;
                Digit4.Unfocus();
                ViewModel.OTPCommand.Execute(null);
            }
        }

        private void Digit4_Completed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Digit1.Text) && !string.IsNullOrEmpty(Digit2.Text) && !string.IsNullOrEmpty(Digit3.Text) && !string.IsNullOrEmpty(Digit4.Text))
            {
                ViewModel.OTPEntry = Digit1.Text + Digit2.Text + Digit3.Text + Digit4.Text;
                Digit4.Unfocus();
                ViewModel.OTPCommand.Execute(null);
            }


        }
    }


}
