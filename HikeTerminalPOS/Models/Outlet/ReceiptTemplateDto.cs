using System.Diagnostics;
using System.Runtime.Serialization;
using HikePOS.Enums;
using HikePOS.Helpers;
using Realms;

namespace HikePOS.Models
{
	public class ReceiptTemplateDto : FullAuditedPassiveEntityDto
	{
		public string ReceiptName { get; set; }
		public int ReceiptStyle { get; set; }
		public bool PrintReceiptBarcode { get; set; }

        bool _displaylogo { get; set; }
        public bool Displaylogo
        {
            get
            {
                return _displaylogo;
            }
            set
            {
                _displaylogo = value;
                SetPropertyChanged(nameof(Displaylogo));
            }
        }
		public string StoreNameLable { get; set; }
		public bool ShowOutletAddress { get; set; }
		public bool ShowCustomerAddress { get; set; }
		public string _headerText { get; set; }
		public string HeaderText 
		{ 
			get {
				return _headerText?.Trim();
			} 
			set{
				_headerText = value;
                SetPropertyChanged(nameof(HeaderText));
			} 
		}
		public string InvoiceNoPrefix { get; set; }
		public string InvoiceHeading { get; set; }
		public string ServedByLable { get; set; }
		public string DiscountLable { get; set; }
		public string tipsLable { get; set; }
		public string SubTotalLable { get; set; }
		public string TaxLable { get; set; }
		public string ToPayLable { get; set; }
		public string TotalLable { get; set; }
		public string ChangeLable { get; set; }
		public string _footerText { get; set; }
		public string FooterText 
		{ 
			get {
				return _footerText?.Trim();
			}
			set{
				_footerText = value;
                SetPropertyChanged(nameof(FooterText));
			} 
		}
        public bool ShowLoyaltyPointsOnReciept { get; set; }
        public bool ShowTotalNumberOfItemsOnReceipt { get; set; } 
        public bool ShowCustomerSignature { get; set; }
        public bool ShowCustomerNote { get; set; }
        public bool HideDiscountLineOnReceipt { get; set; }

		//Ticket #10303 Start : In the invoice, company name should be displayed first and customer name below it. By Nikhil 
		public bool ShowCompanyNameInBillingInvoice { get; set; }
		//Ticket #10303 End. By Nikhil

        //Ticket start: #71298 DATE ON PAYMENTS.by pratik
        public bool ShowPaymentDateOnReciept { get; set; }
        //#71298 end .by pratik

		bool _PrintSKU { get; set; }
        public bool PrintSKU
        {
            get
            {
                Settings.IsPrintSKU = _PrintSKU;
                return _PrintSKU;
            }
            set
            {
                _PrintSKU = value;
                SetPropertyChanged(nameof(PrintSKU));
            }
        }
        //Ticket start:#90943 iOS:FR Display Barcode or SKU instead of Product Name .by Pratik
        private bool _replaceProductNameWithSKU;
        public bool ReplaceProductNameWithSKU
        {
            get
            {
                return _replaceProductNameWithSKU;
            }
            set
            {
                _replaceProductNameWithSKU = value;
                SetPropertyChanged(nameof(ReplaceProductNameWithSKU));
            }
        }
        //Ticket end:#90943 .by Pratik

        //Ticket start:#24753 iOS - Product Barcode Missing in Receipts.by Pratik
        bool _ShowItemBarCode { get; set; }
        public bool ShowItemBarCode
        {
            get
            {
                Settings.IsShowItemBarCode = _ShowItemBarCode;
                return _ShowItemBarCode;
            }
            set
            {
                _ShowItemBarCode = value;
                SetPropertyChanged(nameof(ShowItemBarCode));
            }
        }
        //Ticket end:#24753 .by rupesh

        //Ticket Start: #26664 IOS - New feature :: Customer delivery address.by rupesh
        bool _ShowCustomerDeliverAddress { get; set; }
        public bool ShowCustomerDeliverAddress
        {
            get
            {
                return _ShowCustomerDeliverAddress;
            }
            set
            {
                _ShowCustomerDeliverAddress = value;
                SetPropertyChanged(nameof(ShowCustomerDeliverAddress));
            }
        }
        //Ticket end:#26664 .by rupesh

        //Ticket #32363 iPad :: Feature request :: Hide Customer Name or Company Name in Receipt
        public bool ShowCompanyName { get; set; }
        //Ticket #32363

        //Ticket start:#32371 iPad :: Feature request :: customer custom field not reflecting in print receipt.by rupesh
        public bool PrintCustomerTaxId { get; set; }
        public string CustomerTaxIdLabel { get; set; }
        //Ticket end:#32371 .by rupesh

        //Ticket start:#36540 iPad: Feature request : Larger log size.by rupesh
        public ReceipStyleFormat ReceipStyleFormat { get; set; }
        public string ReceiptFormat
        {
            set
            {
               ReceipStyleFormat = Newtonsoft.Json.JsonConvert.DeserializeObject<ReceipStyleFormat>(value ?? "");
            }
        }
        public bool ShowQRCode { get; set; }
        public string VATNumber { get; set; }


        //#40623 iPad: Feature request Don't display SoldBy
        public bool showServedByOnReciept { get; set; }

        //#45652 start iPad: FR - Show the title for the 'Customer name' field on the receipt.by rupesh
        public bool ShowCustomerTitle { get; set; }
        public string CustomerTitleLabel { get; set; }
        //#45652 end .by rupesh

        //Ticket start:#58412 Print Store Credit on Receipt.by rupesh  
        public bool ShowStoreCreditOnReciept { get; set; }
        //Ticket end:#58412.by rupesh

        //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
        public bool ShowOnAccountOutStadningOnReciept { get; set; }
        //End Ticket #63876 by Pratik

        //Start ticket #76208 IOS:FR:Terms of payments by Pratik
        public bool ShowInvoiceDueDateOnReciept { get; set; }
        //End ticket #76208 by Pratik
        //Start Ticket #84441 iOS: FR :add discount on receipt (Sale Invoice Receipt Upgradation) by Pratik
        public bool ShowTotalDiscountOnReciept { get; set; }
        //End Ticket #84441 by Pratik

        //Start #84295 iOS - Feature:- Receipt Template Change Invoice Header Titles by Pratik
        public string ItemTitleLabel { get; set; }
        public string PriceTitleLabel { get; set; }
        public string QuantityTitleLabel { get; set; }
        //End #84295 by Pratik

        //Start #90941 iOS:FR: Custom filed on product detailpage By Pratik
        public bool ShowCustomField { get; set; }
        public string CustomFieldLabel { get; set; }
        //End #90941 By Pratik
        //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
        public bool ShowGiftCardOnReciept { get; set; }
        //Ticket end:#94420.by rupesh
        public ReceiptTemplateDB ToModel()
        {
            ReceiptTemplateDB receiptTemplate = new ReceiptTemplateDB
            {
                Id = Id,
                IsActive = IsActive,
                ReceiptName = ReceiptName,
                ReceiptStyle = ReceiptStyle,
                PrintReceiptBarcode = PrintReceiptBarcode,
                Displaylogo = Displaylogo,
                StoreNameLable = StoreNameLable,
                ShowOutletAddress = ShowOutletAddress,
                ShowCustomerAddress = ShowCustomerAddress,
                HeaderText = HeaderText,
                InvoiceNoPrefix = InvoiceNoPrefix,
                InvoiceHeading = InvoiceHeading,
                ServedByLable = ServedByLable,
                DiscountLable = DiscountLable,
                tipsLable = tipsLable,
                SubTotalLable = SubTotalLable,
                TaxLable = TaxLable,
                ToPayLable = ToPayLable,
                TotalLable = TotalLable,
                ChangeLable = ChangeLable,
                FooterText = FooterText,
                ShowLoyaltyPointsOnReciept = ShowLoyaltyPointsOnReciept,
                ShowTotalNumberOfItemsOnReceipt = ShowTotalNumberOfItemsOnReceipt,
                ShowCustomerSignature = ShowCustomerSignature,
                ShowCustomerNote = ShowCustomerNote,
                HideDiscountLineOnReceipt = HideDiscountLineOnReceipt,
                ShowCompanyNameInBillingInvoice = ShowCompanyNameInBillingInvoice,
                PrintSKU = PrintSKU,
                ShowItemBarCode = ShowItemBarCode,
                ShowCustomerDeliverAddress = ShowCustomerDeliverAddress,
                ShowCompanyName = ShowCompanyName,
                PrintCustomerTaxId = PrintCustomerTaxId,
                CustomerTaxIdLabel = CustomerTaxIdLabel,
                ReceipStyleFormat = ReceipStyleFormat == null ? null : (int)ReceipStyleFormat.LogoSize,
                ReceiptFormat = ReceipStyleFormat == null ? null : Newtonsoft.Json.JsonConvert.SerializeObject(ReceipStyleFormat),
                ShowQRCode = ShowQRCode,
                VATNumber = VATNumber,
                showServedByOnReciept = showServedByOnReciept,
                ShowCustomerTitle = ShowCustomerTitle,
                CustomerTitleLabel = CustomerTitleLabel,
                ShowStoreCreditOnReciept = ShowStoreCreditOnReciept,
                //Ticket start: #71298 DATE ON PAYMENTS.by pratik
                ShowPaymentDateOnReciept = ShowPaymentDateOnReciept,
                //#71298 end .by pratik
                //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                ShowOnAccountOutStadningOnReciept = ShowOnAccountOutStadningOnReciept,
                //End Ticket #63876 by Pratik
                //Start ticket #76208 IOS:FR:Terms of payments by Pratik
                ShowInvoiceDueDateOnReciept = ShowInvoiceDueDateOnReciept,
                //End ticket #76208 by Pratik
                //Start #84295 iOS - Feature:- Receipt Template Change Invoice Header Titles by Pratik
                ItemTitleLabel = ItemTitleLabel,
                PriceTitleLabel = PriceTitleLabel,
                QuantityTitleLabel = QuantityTitleLabel,
                //End #84295 by Pratik
                //Start Ticket #84441 iOS: FR :add discount on receipt (Sale Invoice Receipt Upgradation) by Pratik
                ShowTotalDiscountOnReciept = ShowTotalDiscountOnReciept,
                //End Ticket #84441 by Pratik
                //Ticket start:#90943 iOS:FR Display Barcode or SKU instead of Product Name .by Pratik
                ReplaceProductNameWithSKU = ReplaceProductNameWithSKU,
                //Ticket end:#90943 by Pratik
                //Start #90941 iOS:FR: Custom filed on product detailpage By Pratik
                ShowCustomField = ShowCustomField,
                CustomFieldLabel = CustomFieldLabel,
                //End #90941 By Pratik
                //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
                ShowGiftCardOnReciept = ShowGiftCardOnReciept,
                //Ticket end:#94420.by rupesh
            };

            return receiptTemplate;
        }

        public static ReceiptTemplateDto FromModel(ReceiptTemplateDB receiptTemplateDB)
        {
            if (receiptTemplateDB == null)
                return null;

            ReceiptTemplateDto receiptTemplate = new ReceiptTemplateDto
            {
                Id = receiptTemplateDB.Id,
                IsActive = receiptTemplateDB.IsActive,
                ReceiptName = receiptTemplateDB.ReceiptName,
                ReceiptStyle = receiptTemplateDB.ReceiptStyle,
                PrintReceiptBarcode = receiptTemplateDB.PrintReceiptBarcode,
                Displaylogo = receiptTemplateDB.Displaylogo,
                StoreNameLable = receiptTemplateDB.StoreNameLable,
                ShowOutletAddress = receiptTemplateDB.ShowOutletAddress,
                ShowCustomerAddress = receiptTemplateDB.ShowCustomerAddress,
                HeaderText = receiptTemplateDB.HeaderText,
                InvoiceNoPrefix = receiptTemplateDB.InvoiceNoPrefix,
                InvoiceHeading = receiptTemplateDB.InvoiceHeading,
                ServedByLable = receiptTemplateDB.ServedByLable,
                DiscountLable = receiptTemplateDB.DiscountLable,
                tipsLable = receiptTemplateDB.tipsLable,
                SubTotalLable = receiptTemplateDB.SubTotalLable,
                TaxLable = receiptTemplateDB.TaxLable,
                ToPayLable = receiptTemplateDB.ToPayLable,
                TotalLable = receiptTemplateDB.TotalLable,
                ChangeLable = receiptTemplateDB.ChangeLable,
                FooterText = receiptTemplateDB.FooterText,
                ShowLoyaltyPointsOnReciept = receiptTemplateDB.ShowLoyaltyPointsOnReciept,
                ShowTotalNumberOfItemsOnReceipt = receiptTemplateDB.ShowTotalNumberOfItemsOnReceipt,
                ShowCustomerSignature = receiptTemplateDB.ShowCustomerSignature,
                ShowCustomerNote = receiptTemplateDB.ShowCustomerNote,
                HideDiscountLineOnReceipt = receiptTemplateDB.HideDiscountLineOnReceipt,
                ShowCompanyNameInBillingInvoice = receiptTemplateDB.ShowCompanyNameInBillingInvoice,
                PrintSKU = receiptTemplateDB.PrintSKU,
                ShowItemBarCode = receiptTemplateDB.ShowItemBarCode,
                ShowCustomerDeliverAddress = receiptTemplateDB.ShowCustomerDeliverAddress,
                ShowCompanyName = receiptTemplateDB.ShowCompanyName,
                PrintCustomerTaxId = receiptTemplateDB.PrintCustomerTaxId,
                CustomerTaxIdLabel = receiptTemplateDB.CustomerTaxIdLabel,
                ReceipStyleFormat = receiptTemplateDB.ReceipStyleFormat.HasValue ? new ReceipStyleFormat() { LogoSize = (LogoSize)receiptTemplateDB.ReceipStyleFormat } : null,
                ReceiptFormat = receiptTemplateDB.ReceiptFormat,
                ShowQRCode = receiptTemplateDB.ShowQRCode,
                VATNumber = receiptTemplateDB.VATNumber,
                showServedByOnReciept = receiptTemplateDB.showServedByOnReciept,
                ShowCustomerTitle = receiptTemplateDB.ShowCustomerTitle,
                CustomerTitleLabel = receiptTemplateDB.CustomerTitleLabel,
                ShowStoreCreditOnReciept = receiptTemplateDB.ShowStoreCreditOnReciept,
                //Ticket start: #71298 DATE ON PAYMENTS.by pratik
                ShowPaymentDateOnReciept = receiptTemplateDB.ShowPaymentDateOnReciept,
                //#71298 end .by pratik
                //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
                ShowOnAccountOutStadningOnReciept = receiptTemplateDB.ShowOnAccountOutStadningOnReciept,
                //End Ticket #63876 by Pratik
                //Start ticket #76208 IOS:FR:Terms of payments by Pratik
                ShowInvoiceDueDateOnReciept = receiptTemplateDB.ShowInvoiceDueDateOnReciept,
                //End ticket #76208 by Pratik
                //Start #84295 iOS - Feature:- Receipt Template Change Invoice Header Titles by Pratik
                ItemTitleLabel = receiptTemplateDB.ItemTitleLabel,
                PriceTitleLabel = receiptTemplateDB.PriceTitleLabel,
                QuantityTitleLabel = receiptTemplateDB.QuantityTitleLabel,
                //End #84295 by Pratik
                //Start Ticket #84441 iOS: FR :add discount on receipt (Sale Invoice Receipt Upgradation) by Pratik
                ShowTotalDiscountOnReciept = receiptTemplateDB.ShowTotalDiscountOnReciept,
                //End Ticket #84441 by Pratik
                //Ticket start:#90943 iOS:FR Display Barcode or SKU instead of Product Name .by Pratik
                ReplaceProductNameWithSKU = receiptTemplateDB.ReplaceProductNameWithSKU,
                //Ticket end:#90943 by Pratik
                //Start #90941 iOS:FR: Custom filed on product detailpage By Pratik
                ShowCustomField = receiptTemplateDB.ShowCustomField,
                CustomFieldLabel = receiptTemplateDB.CustomFieldLabel,
                //End #90941 By Pratik
                //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
                ShowGiftCardOnReciept = receiptTemplateDB.ShowGiftCardOnReciept,
                //Ticket end:#94420.by rupesh
            };

            return receiptTemplate;
        }
    }

    public class ReceipStyleFormat : BaseModel
    {
        public LogoSize LogoSize { get; set; }
    }

    public enum LogoSize : int
    {
        [EnumMember(Value = "default-logo")]
        Default = 0,

        [EnumMember(Value = "large-logo")]
        Large = 1,

        [EnumMember(Value = "small-logo")]
        Small = 2,
    }
  

    public partial class ReceiptTemplateDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string ReceiptName { get; set; }
        public int ReceiptStyle { get; set; }
        public bool PrintReceiptBarcode { get; set; }
        public bool Displaylogo { get; set; }
        public string StoreNameLable { get; set; }
        public bool ShowOutletAddress { get; set; }
        public bool ShowCustomerAddress { get; set; }
        public string HeaderText { get; set; }
        public string InvoiceNoPrefix { get; set; }
        public string InvoiceHeading { get; set; }
        public string ServedByLable { get; set; }
        public string DiscountLable { get; set; }
        public string tipsLable { get; set; }
        public string SubTotalLable { get; set; }
        public string TaxLable { get; set; }
        public string ToPayLable { get; set; }
        public string TotalLable { get; set; }
        public string ChangeLable { get; set; }
        public string FooterText { get; set; }
        public bool ShowLoyaltyPointsOnReciept { get; set; }
        public bool ShowTotalNumberOfItemsOnReceipt { get; set; }
        public bool ShowCustomerSignature { get; set; }
        public bool ShowCustomerNote { get; set; }
        public bool HideDiscountLineOnReceipt { get; set; }
        public bool ShowCompanyNameInBillingInvoice { get; set; }
        public bool PrintSKU { get; set; }
        public bool ShowItemBarCode { get; set; }
        public bool ShowCustomerDeliverAddress { get; set; }
        public bool ShowCompanyName { get; set; }
        public bool PrintCustomerTaxId { get; set; }
        public string CustomerTaxIdLabel { get; set; }
        public int? ReceipStyleFormat { get; set; }
        public string ReceiptFormat { get; set; }
        public bool ShowQRCode { get; set; }
        public string VATNumber { get; set; }
        public bool showServedByOnReciept { get; set; }
        public bool ShowCustomerTitle { get; set; }
        public string CustomerTitleLabel { get; set; }
        public bool ShowStoreCreditOnReciept { get; set; }
        //Ticket start: #71298 DATE ON PAYMENTS.by pratik
        public bool ShowPaymentDateOnReciept { get; set; }
        //#71298 end .by pratik
        //Start Ticket #63876 iOS: FR : On Account calculation on print receipt by Pratik
        public bool ShowOnAccountOutStadningOnReciept { get; set; }
        //End Ticket #63876 by Pratik
        //Start ticket #76208 IOS:FR:Terms of payments by Pratik
        public bool ShowInvoiceDueDateOnReciept { get; set; }
        //End ticket #76208 by Pratik

        //Start #84295 iOS - Feature:- Receipt Template Change Invoice Header Titles by Pratik
        public string ItemTitleLabel { get; set; }
        public string PriceTitleLabel { get; set; }
        public string QuantityTitleLabel { get; set; }
        //End #84295 by Pratik
        //Start Ticket #84441 iOS: FR :add discount on receipt (Sale Invoice Receipt Upgradation) by Pratik
        public bool ShowTotalDiscountOnReciept { get; set; }
        //End Ticket #84441 by Pratik

        //Ticket start:#90943 iOS:FR Display Barcode or SKU instead of Product Name .by Pratik
        public bool ReplaceProductNameWithSKU { get; set; }
        //Ticket end:#90943 by Pratik

        //Start #90941 iOS:FR: Custom filed on product detailpage By Pratik
        public bool ShowCustomField { get; set; }
        public string CustomFieldLabel { get; set; }
        //End #90941 By Pratik
        //Ticket start:#94420 iOS:FR Gift Voucher.by rupesh
        public bool ShowGiftCardOnReciept { get; set; }
        //Ticket end:#94420.by rupesh

    }

}
