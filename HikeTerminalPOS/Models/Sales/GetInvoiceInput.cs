using System;
using System.Collections.Generic;
using HikePOS.Enums;

namespace HikePOS.Models
{
	public class GetInvoiceInput : PagedSortedAndFilteredInputDto
	{
		public int? outletId { get; set; }
		public List<InvoiceStatus> status { get; set; }
        public InvoiceSearchStatus SearchStatus { get; set; }
		public DateTime? fromDate { get; set; }
		public DateTime? toDate { get; set; }

	}

    public class CashDrawerLogInput 
    {
        public DateTime openTime { get; set; }
        public int? outletId { get; set; }
        public int? registerId { get; set; }
        public int? registerClosureId { get; set; }
        public InvoiceFrom openedFrom { get; set; }
        public bool isSync { get; set; } 
    }
    public class GetInvoiceDetailInput
    {
        public int id { get; set; }
    }

    //Ticket start:#56178 Receipts are getting printed with the customer name of previous sale.by rupesh
    public class UpdateCustomerDetailInput
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    //Ticket end:#56178 .by rupesh

}
