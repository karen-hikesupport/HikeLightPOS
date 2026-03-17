using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HikePOS.Enums;
using HikePOS.Helpers;
using HikePOS.Models;
using HikePOS.Services;
using HikePOS.ViewModels;

namespace HikePOS
{

    public partial class PaymentPagePhone : BaseContentPage<PaymentViewModel>
    {
        public ScrollView _Rootview
        {
            get
            {
                return rootview;
            }
        }
      
        public SearchCustomerView _CustomerPopup
        {
            get
            {
                return null;
            }
        }

        public PaymentPagePhone()
        {
            try
            {
                InitializeComponent();
                NavigationPage.SetHasNavigationBar(this, false);
            }
            catch (Exception ex)
            {
                ex.Track();
            }
            Title = "Payment";

        }
    }
}