using System;

namespace HikePOS
{
	public partial class ParkSaleFilterOptionMenuView : ContentView
	{

        public event EventHandler<string> ItemChanged;

        public ParkSaleFilterOptionMenuView()
		{
			InitializeComponent();
		}

        void AllHandle_Clicked(object sender, System.EventArgs e)
        {
          ItemChanged?.Invoke(this, "All");
        }

		void LayByHandle_Clicked(object sender, System.EventArgs e)
		{
            ItemChanged?.Invoke(this, "LayBy");
		}

		void ParkedHandle_Clicked(object sender, System.EventArgs e)
		{
            ItemChanged?.Invoke(this, "Parked");
		}

            void OnGoingHandle_Clicked(object sender, System.EventArgs e)
		{
                  ItemChanged?.Invoke(this, "OnGoing");
		}

		void VoidedHandle_Clicked(object sender, System.EventArgs e)
		{
            ItemChanged?.Invoke(this, "Voided");
		}

		void BackOrderHandle_Clicked(object sender, System.EventArgs e)
		{
            ItemChanged?.Invoke(this, "BackOrder");
		}

		void OnAccountHandle_Clicked(object sender, System.EventArgs e)
		{
            ItemChanged?.Invoke(this, "OnAccount");
		}

		void RefundedHandle_Clicked(object sender, System.EventArgs e)
		{
            ItemChanged?.Invoke(this, "Refunded");
		}

		void CompletedHandle_Clicked(object sender, System.EventArgs e)
		{
            ItemChanged?.Invoke(this, "Completed");
		}

        void UnsyncHandle_Clicked(object sender, System.EventArgs e)
        {
            ItemChanged?.Invoke(this, "Unsync");
        }

        //Ticket start:#22406 Quote sale.by rupesh
        void IssueAndOpenQuotesHandle_Clicked(object sender, System.EventArgs e)
        {
            ItemChanged?.Invoke(this, "OpenQuotes");
        }
        void ClosedQuotesHandle_Clicked(object sender, System.EventArgs e)
        {
            ItemChanged?.Invoke(this, "ClosedQuotes");
        }
        void VoidedQuotesHandle_Clicked(object sender, System.EventArgs e)
        {
            ItemChanged?.Invoke(this, "VoidedQuotes");
        }
        //Ticket end:#22406 .by rupesh

        void ExchangedHandle_Clicked(System.Object sender, System.EventArgs e)
        {
			ItemChanged?.Invoke(this, "Exchange");
		}


    }
}
