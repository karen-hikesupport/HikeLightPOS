using System.Collections.Generic;
using System.Threading.Tasks;
using HikePOS.Models;

namespace HikePOS.Services
{
	public interface IPrint
	{
        //Start Ticket #74633 iOS: Print Delivery Docket and Invoice together (FR) / #69284 iPad - Issue while printing Gift receipt by Pratik
        //Task<bool> DoPrint(View InvoicePrint, View DocketPrint, View CustomerView, View VantivView, double InvoiceHeight, double DocketHeight, double CustomerHeight, double VantivHeight, bool IsOpenCashDrawer, Printer printer, List<string> AssemblyPaymentReceiptData, List<string> VantivCloudReceipt, List<string> ReceiptData);
        Task<bool> DoPrint(View InvoicePrint, View DocketPrint, View CustomerView, View VantivView, double InvoiceHeight, double DocketHeight, double CustomerHeight, double VantivHeight, bool IsOpenCashDrawer, Printer printer, List<string> AssemblyPaymentReceiptData, List<string> VantivCloudReceipt, List<string> ReceiptData, View giftToPrint = null, double giftHeight = 0);
        //End Ticket #69284 by Pratik

        void PrintViews(ScrollView viewToPrint, bool isOpenCashDrawer);
       
        void PrintViews2(View viewToPrint, double viewHeight, bool isOpenCashDrawer, Printer printer);

        void CloudPrint(View InvoiceView, double InvoiceHeight, Printer printer);

		string GenerateBarcode(string text, int width = 600, int height = 160);

		string GenerateQRCode(string text, int width = 300, int height = 300);

        void OpenDrawer(bool IsHavingCashPayment);

        void RegisterSummaryPrint(RegisterclosureDto Registerclosure, Printer printer);

        Task<bool> DoTextPrint(InvoiceDto invoice, ReceiptTemplateDto receiptTemplate, OutletDto_POS CurrentOutlet, ShopDto GeneralShopDto, Printer printer,List<string> ReceiptData, bool IsOpenCashDrawer, string type, InvoiceFulfillmentDto  invoiceFulfillment = null,bool isCustomerReceipt = false, bool isDocketReceipt = false);
        
	}

}
