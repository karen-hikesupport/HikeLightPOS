using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using HikePOS.Models;
using HikePOS.UserControls;
using NodaTime;

namespace HikePOS
{
    public partial class SearchCustomerView : Border
    {
        public static SearchCustomerView Instant;
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set
            {
                SetValue(IsActiveProperty, value);

            }
        }

        public static readonly BindableProperty IsActiveProperty =
            BindableProperty.Create("IsActive", typeof(bool), typeof(ExpandCollapseView), false, propertyChanged: OnIsActiveChanged);
        static void OnIsActiveChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue == null)
                return;

            if ((bool)newValue == true)
            {
                Instant?.FocusOnEntry();
            }
            else
            {
                Instant?.UnFocusOnEntry();
            }
        }

        CancellationTokenSource cancellation;
        public SearchCustomerView()
        {
            InitializeComponent();
            Instant = this;
            SearchCustomerEntry.TextChanged += async (sender, e) =>
            {
                try
                {


                    if (e != null && e.NewTextValue != null)
                    {
                        Stop();

                        Start(e.NewTextValue);
                    }
                    else
                    {
                        ((CustomerViewModel)BindingContext).SearchCustomer(e.NewTextValue);
                    }


                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            };
        }

        public void ClearSearchCustomerEntry()
        {
            SearchCustomerEntry.Text = string.Empty;
        }

        private void CustomerSelected(object sender, TappedEventArgs e)
        {
            if (sender != null)
            {
                try
                {
                    CustomerDto_POS myselecteditem = ((Grid)sender).BindingContext as CustomerDto_POS;
                    _ = ((CustomerViewModel)BindingContext).SelectedCustomerChanged(myselecteditem);
                    SearchCustomerEntry.Unfocus();
                    SearchCustomerEntry.Text = "";
                }
                catch (Exception ex)
                {
                    ex.Track();
                }
            }
        }


        public void Start(string textvalue)
        {
            try
            {
                var timespan = TimeSpan.FromSeconds(0.5);
                CancellationTokenSource cts = this.cancellation; // safe copy
                Dispatcher.StartTimer(timespan,
                    () =>
                    {
                        try
                        {
                            if (cts.IsCancellationRequested)
                                return false;
                            try
                            {
                                ((CustomerViewModel)BindingContext).SearchCustomer(textvalue);
                                Stop();
                            }
                            catch (Exception ex)
                            {
                                ex.Track();
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Track();
                        }
                        return false; // or true for periodic behavior
                    });
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }


        public void Stop()
        {
            try
            {
                var temp = Interlocked.Exchange(ref cancellation, new CancellationTokenSource());
                if (temp != null)
                    temp.Cancel();
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        public void FocusOnEntry()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SearchCustomerEntry.IsEnabled = true;
                    SearchCustomerEntry.Focus();
                });
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

        //Added by rupesh to hidekeyboard 
        public void UnFocusOnEntry()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SearchCustomerEntry.Text = "";
                    SearchCustomerEntry.Unfocus();
                    SearchCustomerEntry.IsEnabled = false;


                });
            }
            catch (Exception ex)
            {
                ex.Track();
            }
        }

    }
}
