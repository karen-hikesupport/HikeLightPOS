using System;
using System.Collections.Generic;
using HikePOS.Enums;

namespace HikePOS.Models
{
	public class CustomerInputDto
	{
	}

	public class GetCustomerInput : PagedSortedAndFilteredInputDto
	{
		public int? OutletId { get; set; }
		public DateTime modifiedDateTime { get; set; }
	}

	public class GetCustomerDetailInput
	{
		public int id { get; set; }
	}

	public class GetCustomerInvoicesInput : PagedSortedAndFilteredInputDto
	{
		public int customerId { get; set; }
		public List<InvoiceStatus> status { get; set; }
	}

    public class GetCustomerCreditBalanceInput : PagedSortedAndFilteredInputDto
    {
        public int customerId { get; set; }
    }
	//Ticket start:#26664 IOS - New feature :: Customer delivery address.by rupesh
	public class CustomerAddressInputDto : PagedSortedAndFilteredInputDto
	{
		public int? customerId { get; set; }
		public DateTime? modifiedDateTime { get; set; }

	}
	//Ticket end:#26664 .by rupesh

}