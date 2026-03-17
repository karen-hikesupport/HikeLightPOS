using System.ComponentModel;

namespace HikePOS.Models
{
    public class Printer : BaseNotify
    {
        public string ModelName { get; set; }
        public string Port { get; set; }
        public string Font { get; set; }

        public string DeviceId { get; set; }

        bool _activeCustomerPrint { get; set; } = false;
        public bool ActiveCustomerPrint
        {
            get { return _activeCustomerPrint; }
            set
            {
                _activeCustomerPrint = value;
                SetPropertyChanged(nameof(ActiveCustomerPrint));
            }
        }

        bool _cloudInfoCode { get; set; } = false;
        public bool CloudInfoCode{get { return _cloudInfoCode; }set{_cloudInfoCode = value;SetPropertyChanged(nameof(CloudInfoCode));}}

        bool _cloudQRCode { get; set; }
        public bool CloudQRCode
        {
            get { return _cloudQRCode; }
            set{_cloudQRCode = value;

                SetPropertyChanged(nameof(CloudQRCode));
            }
        }


        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            var other = obj as Printer;
            if (other == null)
                return false;
            if (this.DeviceId != other.DeviceId) // if pos printers have difference id => false
                return false;
            return this.ModelName == other.ModelName && this.Port == other.Port;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        int _numberOfPrintCopy { get; set; } = 1;
        public int NumberOfPrintCopy
        {
            get { return _numberOfPrintCopy; }
            set
            {
                _numberOfPrintCopy = value;

                SetPropertyChanged(nameof(NumberOfPrintCopy));
            }
        }

        bool _primaryReceiptPrint { get; set; }
        public bool PrimaryReceiptPrint
        {
            get { return _primaryReceiptPrint; }
            set
            {
                _primaryReceiptPrint = value;

                SetPropertyChanged(nameof(PrimaryReceiptPrint));
                SetPropertyChanged(nameof(Status));
            }
        }

        bool _activeDocketPrint { get; set; } = false;
        public bool ActiveDocketPrint
        {
            get { return _activeDocketPrint; }
            set
            {
                _activeDocketPrint = value;
                SetPropertyChanged(nameof(ActiveDocketPrint));
                SetPropertyChanged(nameof(Status));
            }
        }

        bool _isAutoPrintReceipt { get; set; } = false;
        public bool IsAutoPrintReceipt
        {
            get { return _isAutoPrintReceipt; }
            set
            {
                _isAutoPrintReceipt = value;

                SetPropertyChanged(nameof(IsAutoPrintReceipt));
            }
        }
        bool _enableCashDrawer { get; set; } = false;
        public bool EnableCashDrawer
        {
            get { return _enableCashDrawer; }
            set
            {
                _enableCashDrawer = value;

                SetPropertyChanged(nameof(EnableCashDrawer));
            }
        }
        bool _onlyCashPaymentCashDrawer { get; set; } = false;
        public bool OnlyCashPaymentCashDrawer
        {
            get { return _onlyCashPaymentCashDrawer; }
            set
            {
                _onlyCashPaymentCashDrawer = value;

                SetPropertyChanged(nameof(OnlyCashPaymentCashDrawer));
            }
        }

        public string Status { get { return (PrimaryReceiptPrint || ActiveDocketPrint) ? ((PrimaryReceiptPrint ? "Primary" : "") + ((PrimaryReceiptPrint && ActiveDocketPrint) ? ", " : "") + (ActiveDocketPrint ? "Docket" : "") + " printer") : LanguageExtension.Localize("UseInHikeLabelText"); } }
        public double width { get; set; }





        string _printerTyoe { get; set; } 
        public string PrinterType
        {
            get { return _printerTyoe; }
            set
            {
                _printerTyoe = value;

                SetPropertyChanged(nameof(PrinterType));
            }
        }
        //Ticket starts #70775:The client wants to connect  usb scanner to mc3 print in ipad.by rupesh
        public bool EnableUSBScanner { get; set; }
        public bool ShowEnableUSBScanner
        {

            get
            {
                if (ModelName.Contains("POP") || ModelName.Contains("TM-"))
                {
                    EnableUSBScanner = false;
                    return false;
                }
                else
                    return true;
            }

        }
        //Ticket end #70775:.by rupesh

    }
}

