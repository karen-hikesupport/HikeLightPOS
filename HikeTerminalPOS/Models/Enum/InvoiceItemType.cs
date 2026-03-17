namespace HikePOS.Enums
{
	public enum InvoiceItemType
	{
		Standard = 0,
		GiftCard = 1,
		Custom = 2,
		Composite = 3,
		Appointment = 4,
		Discount = 5,
		//Ticket start:#20064 Unit of measurement feature for iPad app.by rupesh
		UnityOfMeasure = 6,
		//Ticket end:#20064 .by rupesh
		Category = 6,
		//Offer = 7,
		//Ticket start:#22898 Composite sale not working properly.by rupesh
		CompositeProduct = 7,
		//Ticket end:#22898 Composite sale not working properly.by rupesh
		Other = 100,
		Back = 777 //Start #92766 FR POS - BE ABLE TO GO BACK By Pratik
	}

	public enum InvoiceSearchStatus
	{
		AllSales = 0,

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

		PartialFulfilled = 12,

		Awaiting_fulfillment = 13,

		VoidedQuote = 100,

		ClosedQuote = 101,
	}

	//#94565
	public enum InvoiceLineItemStatus
	{

		ToCook = 0,

		Ready = 1,

		Completed = 2,

		Done = 3,
	}
	//#94565
}
