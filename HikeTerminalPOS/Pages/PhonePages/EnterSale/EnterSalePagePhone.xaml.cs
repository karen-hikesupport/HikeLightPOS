using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Resources;
using HikePOS.Services;
using HikePOS.UserControls;
using HikePOS.ViewModels;
using Microsoft.Maui.Graphics;

namespace HikePOS
{
    public partial class EnterSalePagePhone : BaseContentPage<EnterSaleViewModel>
    {

        public static bool DataUpdated = true;
        public static UserListDto ServedBy = null;
        public Label Customernamelbl { get; set; }
        public BorderLessEntry InvoiceNote { get; set; }
        public ContentView _PrintReceiptSummaryView { get; set; }

        public EnterSalePagePhone()
        {
            InitializeComponent();
            ViewModel.EnterSalePagePhone = this;
            _PrintReceiptSummaryView = PrintReceiptSummaryView;
            InvoiceNote = invoiceNote;
            Customernamelbl = customernamelbl;
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();

            if (BindingContext == null)
                return;

            ViewCell theViewCell = ((ViewCell)sender);
            var menu = theViewCell.ContextActions.FirstOrDefault();
            theViewCell.ContextActions.Clear();
            theViewCell.ContextActions.Add(menu);
        }
        //Start #90946 iOS:FR  Item Serial Number Tracking by Pratik
        private async void CustomEntry_Unfocused(object sender, FocusEventArgs e)
        {
            if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EnableSerialNumberTracking)
            {
                CustomEntry customEntry = ((CustomEntry)sender);
                if (!string.IsNullOrEmpty(customEntry.Text) && !string.IsNullOrWhiteSpace(customEntry.Text))
                {
                    var model = customEntry.BindingContext as InvoiceLineItemDto;
                    await ViewModel.CheckSerialNumber(model);
                }
            }
        }

        private async void CustomEntry_Completed(object sender, EventArgs e)
        {
            if (Settings.StoreGeneralRule != null && Settings.StoreGeneralRule.EnableSerialNumberTracking)
            {
                CustomEntry customEntry = ((CustomEntry)sender);
                if (!string.IsNullOrEmpty(customEntry.Text) && !string.IsNullOrWhiteSpace(customEntry.Text))
                {
                    var model = customEntry.BindingContext as InvoiceLineItemDto;
                    await ViewModel.CheckSerialNumber(model);
                }
            }
        }
        //End #90946 by Pratik

        private System.Timers.Timer _debounceTimer;
        public void ScrollInvoiceItemToStart(InvoiceLineItemDto itemDto)
        {
            try
            {
                if (ViewModel.invoicemodel?.Invoice?.InvoiceLineItems != null && ViewModel.invoicemodel.Invoice.InvoiceLineItems.Count > 0)
                {
#if ANDROID
                if (!Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart)
                    InvoiceItemList.ScrollTo(0, -1, ScrollToPosition.MakeVisible, false);
                else if(Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart && itemDto != null)
                {
                    if (!Settings.StoreGeneralRule.ShowGroupProductsByCategory)
                        InvoiceItemList.ScrollTo(itemDto, position: ScrollToPosition.End, animate: false);
                    else
                    {
                        _debounceTimer?.Stop();
                        _debounceTimer = new System.Timers.Timer(200);
                        _debounceTimer.Elapsed += (s, e) =>
                        {
                            _debounceTimer.Stop();
                            Task.Run(async () =>
                            {
                                Dispatcher.Dispatch(() =>
                                {
                                    InvoiceItemList.ScrollTo(itemDto, position: ScrollToPosition.End, animate: false);
                                });
                            });
                        };
                        _debounceTimer.Start();
                    }
                }
#elif IOS
                    if (Settings.StoreGeneralRule.DisplayProductsBasedOnTheOrderAddedInCart && itemDto != null)
                    {
                        if (!Settings.StoreGeneralRule.ShowGroupProductsByCategory)
                        {

                            InvoiceItemList1.ScrollTo(itemDto, ScrollToPosition.End, false);
                            _debounceTimer?.Stop();
                            _debounceTimer = new System.Timers.Timer(200);
                            _debounceTimer.Elapsed += (s, e) =>
                            {
                                _debounceTimer.Stop();
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        if (ViewModel.invoicemodel.Invoice.InvoiceLineItems.Count > 5)
                                        {
                                            InvoiceItemList1.ScrollTo(ViewModel.invoicemodel.Invoice.InvoiceLineItems[ViewModel.invoicemodel.Invoice.InvoiceLineItems.Count - 2], ScrollToPosition.End, false);
                                            InvoiceItemList1.ScrollTo(itemDto, ScrollToPosition.End, true);
                                        }
                                    }
                                    catch (System.Exception)
                                    {
                                    }

                                });
                            };
                            _debounceTimer.Start();

                        }
                    }
#endif
                }
            }
            catch (System.Exception)
            {
            }
        }

        public void ReSizeListView()
        {
#if IOS
            var list = InvoiceItemList1.ItemsSource;
            InvoiceItemList1.ItemsSource = null;
            InvoiceItemList1.ItemsSource = list;
#endif
        }
    }
}
