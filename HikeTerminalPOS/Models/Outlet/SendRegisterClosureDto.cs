using System;
using HikePOS.Enums;
using HikePOS.Model;
using HikePOS.Models.Shop;

namespace HikePOS.Models
{
	//Start #92768 Pratik
	public class SendRegisterClosureDto
	{
		public RegisterClosureDetail registerClosureDetail { get; set; }
		public EmailTemplate emailTemplate { get; set; }
		public List<EmailList> emailList { get; set; }
		public string subject { get; set; }
		public string body { get; set; }
	}
	public class EmailList
	{
		public string emailId { get; set; }
		public bool cc { get; set; }
	}

	public class EmailTemplate
	{
		public object outletId { get; set; }
		public EmailTemplateType templateTypeId { get; set; }
		public string name { get; set; }
		public string subject { get; set; }
		public string body { get; set; }
		public bool loyaltyDetailsAdded { get; set; }
		public object placeHolderList { get; set; }
		public bool isActive { get; set; }
		public int id { get; set; }
	}

	public class RegisterClosureDetail
	{
		public string refNumber { get; set; }
		public int registerId { get; set; }
		public DateTime? startDateTime { get; set; }
		public object endDateTime { get; set; }
		public string registerName { get; set; }
		public string outletRegisterName { get; set; }
		public string outletName { get; set; }
		public int? startBy { get; set; }
		public object closeBy { get; set; }
		public string startByUser { get; set; }
		public object closeByUser { get; set; }
		public decimal totalSales { get; set; }
		public decimal totalCompletedSales { get; set; }
		public decimal totalOnAccountSales { get; set; }
		public decimal totalParkedSales { get; set; }
		public decimal totalLayBySales { get; set; }
		public decimal difference { get; set; }
		public decimal totalDiscounts { get; set; }
		public decimal totalTax { get; set; }
		public decimal totalTip { get; set; }
		public decimal totalPayments { get; set; }
		public decimal totalRefunds { get; set; }
		public object notes { get; set; }
		public object merchant_receipt { get; set; }
		public object transactionDetail { get; set; }
		public int thirdPartySyncStatus { get; set; }
		public List<RegisterclosuresTallyDto> registerclosuresTallys { get; set; }
		public List<RegisterCashInOutDto> registerCashInOuts { get; set; }
		public List<TaxList> taxList { get; set; }
		public object registerClosureTallyDenominations { get; set; }
		public RegisterClosureTransactionDetailsDto registerClosureTransactionDetailsDto { get; set; }
		public bool isActive { get; set; }
		public int id { get; set; }
	}

	public static class EmailTemplateHTML
	{
		public static string EmailBody => "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"color:#646464; font-family:Arial,Helvetica,sans-serif; font-size:14px; text-align:left; width:100%\">"
		 + "\n\t<tbody>\n\t\t<tr>\n\t\t\t<td>\n\t\t\t<div><strong>Dear,</strong></div>"
		 + "\n\t\t\t</td>\n\t\t</tr>\n\t\t<tr>\n\t\t\t<td>&nbsp;</td>\n\t\t</tr>\n\t\t<tr>\n\t\t\t"
		 + "<td>This is the &nbsp;register closure summary for "
		 + "<strong> Store {0} </strong> from <strong>{1} to {2}</strong>."
		 + "</td>\n\t\t</tr>\n\t\t<tr>\n\t\t\t<td>&nbsp;</td>\n\t\t</tr>\n\t\t<tr>\n\t\t\t"
		 + "<td>Please find the attached file for detailed information regarding this closure.</td>\n\t\t</tr>\n\t</tbody>\n</table>\n";
	}
	//End #92768
}
