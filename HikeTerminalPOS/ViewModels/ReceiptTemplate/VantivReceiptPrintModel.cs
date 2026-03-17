using System;
using System.Collections.ObjectModel;

namespace HikePOS.ViewModels
{
    public class VantivReceiptPrintModel : BaseViewModel
    {
        string _MerchantID { get; set; }
        public string MerchantID { get { return _MerchantID; } set { _MerchantID = value; SetPropertyChanged(nameof(MerchantID)); } }

        string _TerminalID { get; set; }
        public string TerminalID { get { return _TerminalID; } set { _TerminalID = value; SetPropertyChanged(nameof(TerminalID)); } }

        string _VantivReference { get; set; }
        public string VantivReference { get { return _VantivReference; } set { _VantivReference = value; SetPropertyChanged(nameof(VantivReference)); } }

        string _AccountNo { get; set; }
        public string AccountNo { get { return _AccountNo; } set { _AccountNo = value; SetPropertyChanged(nameof(AccountNo)); } }

        string _CardLogo { get; set; }
        public string CardLogo { get { return _CardLogo; } set { _CardLogo = value; SetPropertyChanged(nameof(CardLogo)); } }

        string _CardEntry { get; set; }
        public string CardEntry { get { return _CardEntry; } set { _CardEntry = value; SetPropertyChanged(nameof(CardEntry)); } }

        string _TransactionID { get; set; }
        public string TransactionID { get { return _TransactionID; } set { _TransactionID = value; SetPropertyChanged(nameof(TransactionID)); } }

        string _ApprovalCode { get; set; }
        public string ApprovalCode { get { return _ApprovalCode; } set { _ApprovalCode = value; SetPropertyChanged(nameof(ApprovalCode)); } }

        string _ResponseCode { get; set; }
        public string ResponseCode { get { return _ResponseCode; } set { _ResponseCode = value; SetPropertyChanged(nameof(ResponseCode)); } }

        decimal _VantivSale { get; set; }
        public decimal VantivSale { get { return _VantivSale; } set { _VantivSale = value; SetPropertyChanged(nameof(VantivSale)); } }

        decimal _VantivCashBack { get; set; }
        public decimal VantivCashBack { get { return _VantivCashBack; } set { _VantivCashBack = value; SetPropertyChanged(nameof(VantivCashBack)); } }

        decimal _VantivTotal { get; set; }
        public decimal VantivTotal { get { return _VantivTotal; } set { _VantivTotal = value; SetPropertyChanged(nameof(VantivTotal)); } }


        string _ApplicationIdentifier { get; set; }
        public string ApplicationIdentifier { get { return _ApplicationIdentifier; } set { _ApplicationIdentifier = value; SetPropertyChanged(nameof(ApplicationIdentifier)); } }

        string _ApplicationLabel { get; set; }
        public string ApplicationLabel { get { return _ApplicationLabel; } set { _ApplicationLabel = value; SetPropertyChanged(nameof(ApplicationLabel)); } }

        string _TransactionStatus { get; set; }
        public string TransactionStatus { get { return _TransactionStatus; } set { _TransactionStatus = value; SetPropertyChanged(nameof(TransactionStatus)); } }

        DateTime _TransactionDateTime { get; set; }
        public DateTime TransactionDateTime { get { return _TransactionDateTime; } set { _TransactionDateTime = value; SetPropertyChanged(nameof(TransactionDateTime)); } }

        bool _WasPinVerified { get; set; }
        public bool WasPinVerified { get { return _WasPinVerified; } set { _WasPinVerified = value; SetPropertyChanged(nameof(WasPinVerified)); } }

        string _Cryptogram { get; set; }
        public string Cryptogram { get { return _Cryptogram; } set { _Cryptogram = value; SetPropertyChanged(nameof(Cryptogram)); } }

        ObservableCollection<string> _EmvTags { get; set; }
        public ObservableCollection<string> EmvTags { get { return _EmvTags; } set { _EmvTags = value; SetPropertyChanged(nameof(EmvTags)); } }

        string _PaymentType { get; set; }
        public string PaymentType { get { return _PaymentType; } set { _PaymentType = value; SetPropertyChanged(nameof(PaymentType)); } }

    }
}