namespace HikePOS.Enums
{
	public enum InvoiceStatus : short
	{
		initial = -1,
		Pending = 0,
		Parked = 1,
		Completed = 2,
		Refunded = 3,
		Voided = 4,
		LayBy = 5,
		BackOrder = 6,
		OnAccount = 7,
		Exchange = 8,
		RefundedAndDiscard = 9,
		RefundedAndDiscardBO = 10,
		Quote = 11,
		//Start #84293 iOS - Feature:- Expand Pick & Pack & Order Full fill from multiple location by Pratik
		PartialFulfilled = 12,
		//End #84293 by Pratik
		Packed = 13,
		ReadyToShip = 14,
		EmailSent = 15,
		
		OnGoing = 16, //#94565

    }

    public enum CurrentInvoiceStatus : short
	{ 
		Uncomplete = 0,
		Refund = 1,
		Reopen = 2
	}

	public enum LocalInvoiceStatus : short
	{
		Pending = 0,
		Processing = 1,
		PaymentProcessing = 2,
		Completed = 3,
		OnGoing = 4
	}

	public enum FinancialStatus : short
	{
		Pending = 0,
		Partially_Paid = 1,
		Paid = 2,
		Partially_Refunded = 3,
		Refunded = 4,
		Voided = 5,
		Closed = 6,
		VoidedQuote = 7

	}
}
