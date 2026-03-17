using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HikePOS.Enums;
using Newtonsoft.Json;

namespace HikePOS.Models
{
	public class BaseModel : BaseNotify
	{
	}

	public class MessageCenterSubscribeClass { }

    public class Message{

        public Message(MessageType type, string text)
        {
            Text = text;
            Type = type;
        }
        public string Text { get; set; }
        public MessageType Type { get; set; }
    }

	public class Message<T>
	{

		public Message(MessageType type, string text, T result)
		{
			Text = text;
			Type = type;
            Result = result;
		}

		public Message(MessageType type, string text)
		{
			Text = text;
			Type = type;
		}
		public string Text { get; set; }
		public MessageType Type { get; set; }
        public T Result { get; set; }
	}


    public class AutoPrintMessageCenter
    {
        public bool OpenCashDrawer { get; set; } = false;
        public List<string> AssemblyPaymentReceiptData { get; set; }
        public bool OnlyAssemblyPayment { get; set; } = false;
    }

    public class PaypalAutoPrintMessageCenter
    {
        public bool OpenCashDrawer { get; set; } = false;
       
    }

    public class UpdatedInvoiceLineItemMessageCenter
    {
        public InvoiceLineItemDto invoiceLineItemDto { get; set; }
        public BackorderResult result { get; set; }
    }

    // Maui :  From HikePOS.ViewModels.InvoiceCalculations to here By Pratik
    public class BackorderResult
    {
        public bool IsValid { get; set; }
        public decimal BackOrderQty { get; set; }
        public decimal Validatedstock { get; set; }
        public decimal Quantity { get; set; }
    }
    // Maui End Pratik

}
