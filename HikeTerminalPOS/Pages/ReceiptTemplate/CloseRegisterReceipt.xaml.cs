using System;
using System.Collections;
using HikePOS.ViewModels;
using System.Linq;
using HikePOS.Models;
using HikePOS.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace HikePOS
{
    public partial class CloseRegisterReceipt : ScrollView
    {
        //Ticket #9632 Start: App crashes in register page after removing payment type.By Nikhil.
        List<RegisterclosuresTallyDto> tmpList;
        //Ticket #9632 End:By Nikhil. 

        public CloseRegisterReceipt()
        {
            InitializeComponent();
            Microsoft.Maui.Handlers.LabelHandler.Mapper.AppendToMapping("MyCustomizationLabel", (handler, view) =>
            {
                if (handler != null && view is HikePOS.UserControls.AutoFitLabel)
                {
#if IOS
                    handler.PlatformView.AdjustsFontSizeToFitWidth = true;
#endif
                }
            });
        }

        //Ticket #9753 Start :Register Summary print amount cut issue. By Nikhil
        void updateViewHeight()
        {
            //if (mPOPPrinterConfigure)
            //{
            totalSales.UpdateHeight();
            totalTax.UpdateHeight();

            totalDiscounts.UpdateHeight();
            tipSurcharge.UpdateHeight();
            totalPayments.UpdateHeight();
            totalRefunds.UpdateHeight();
            netReceipts.UpdateHeight();

            cashInTillExpected.UpdateHeight();
            cashInTillActual.UpdateHeight();
            cashSale.UpdateHeight();
            cashDifference.UpdateHeight();

            totalNewCustomers.UpdateHeight();
            totalTransactions.UpdateHeight();
            avgSaleValue.UpdateHeight();
            //} 
            //Ticket #11743 End. By Nikhil
        }
        //Ticket #9753 End : By Nikhil

        public void UpdateCloseRegister(bool IsBig)
        {
            try
            {
                var Registerclosure = ((CashRegisterViewModel)BindingContext).Registerclosure;
                if (Registerclosure != null)
                {
                    if (string.IsNullOrEmpty(Registerclosure.Notes))
                    {
                        note.IsVisible = false;
                        noteLine.IsVisible = false;
                    }
                    else
                    { 
                        note.IsVisible = true;
                        noteLine.IsVisible = true;
                    }
                    //Ticket #9632 Start: App crashes in register page after removing payment type.By Nikhil.
                        //Ticket start: #62808 iPad:Print Receipt spacing issues.by rupesh
                        //PaymentList.ItemsSource = Registerclosure.RegisterclosuresTallys.Where(x => x.PaymentOptionName != null && x.PaymentOptionName.ToLower() != "cash").ToList();
                        var itemSource = Registerclosure.RegisterclosuresTallys.Where(x => x.PaymentOptionName != null && x.PaymentOptionName.ToLower() != "cash").ToList();
                    BindableLayout.SetItemsSource(PaymentList, itemSource);
                    //Ticket end: #62808 .by rupesh
                    //Ticket #9632 End:By Nikhil.

                    //Ticket Start #62713 In print receipt Closed By not show after closing cash register  by: rupesh
                    ClosedByUser.Text = "";
                    if (Settings.CurrentRegister != null && !Settings.CurrentRegister.IsOpened)
                    {
                        ClosedByUser.Text = "Closed by: " + Settings.CurrentRegister.LastRegisterclosure?.CloseByUser;
                    }
                    //Ticket End #62713 by: rupesh
                    if (Registerclosure.StartDateTime != null)
                    {
                        if (Registerclosure.EndDateTime == null)
                        {
                             StartDateTime.Text =  string.Format("Opened at {0:dd MMM, yyyy hh.mmtt}", Registerclosure.StartDateTime.Value.ToStoreTime());
                        }
                        else
                        {
                             StartDateTime.Text =  string.Format("Opened at {0:hh.mmtt} on {0:dddd, dd MMMM yyyy} and Closed at {1:hh.mmtt} on {1:dddd, dd MMMM yyyy}", Registerclosure.StartDateTime.Value.ToStoreTime(), Registerclosure.EndDateTime.Value.ToStoreTime());
                        }
                    }

                    //Ticket #9632 Start: App crashes in register page after removing payment type.By Nikhil.
                    RegisterclosuresTallyDto registerclosuresTallyDto = Registerclosure.RegisterclosuresTallys.FirstOrDefault(x => x.PaymentOptionName != null && x.PaymentOptionName.ToLower() == "cash");
                    //Ticket #9632 End:By Nikhil.

                    if (registerclosuresTallyDto != null && Registerclosure.RegisterCashInOuts != null)
                    {
                        cashInTillExpected.Value = (registerclosuresTallyDto.ActualTotal).ToString("C");
                        cashInTillActual.Value = (registerclosuresTallyDto.RegisteredTotal).ToString("C");
                        var TotalCashSale = registerclosuresTallyDto.ActualTotal - Registerclosure.RegisterCashInOuts.Sum(x => x.Amount);
                        cashSale.Value = TotalCashSale.ToString("C");
                        cashDifference.Value = (registerclosuresTallyDto.DifferenceTotal).ToString("C");
                    }

                    //Ticket #9753 Start :Register Summary print amount cut issue. By Nikhil
                    updateViewHeight();
                    //Ticket #9753 End : By Nikhil
                }
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }
    }
}
