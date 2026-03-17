using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Models;
using HikePOS.ViewModels;

namespace HikePOS
{
    public partial class RefundAndVoidPage : PopupBasePage<RefundAndVoidViewModel>
    {

        public RefundAndVoidPage()
        {
            InitializeComponent();
            //Added to call refund and void transaction for other payments.
            ViewModel.RefundAndVoidPage = this;

        }
        // Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        void TenderAmount_Unfocused(System.Object sender, Microsoft.Maui.Controls.FocusEventArgs e)
        {

            ViewModel.Invoice.TenderAmount = Math.Max((-1) * ViewModel.Invoice.TotalTender, (-1) * ViewModel.TenderAmount.ToPositive());
            ViewModel.TenderAmount = ViewModel.Invoice.TenderAmount;

        }
        //End Ticket #73190  By: Pratik

    }
}
