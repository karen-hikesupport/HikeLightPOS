using CommunityToolkit.Mvvm.Messaging.Messages;
using HikePOS.Models;
using HikePOS.Models.Payment;
using HikePOS.ViewModels;

namespace HikePOS.Messenger
{
    public class MenuDataUpdatedMessenger : ValueChangedMessage<string>
    {
        public MenuDataUpdatedMessenger(string value) : base(value) { }
    }

    public class SignalRInvoiceMessenger : ValueChangedMessage<InvoiceDto>
    {
        public SignalRInvoiceMessenger(InvoiceDto value) : base(value) { }
    }

    public class BackgroundInvoiceUpdatedMessenger : ValueChangedMessage<InvoiceDto>
    {
        public BackgroundInvoiceUpdatedMessenger(InvoiceDto value) : base(value) { }
    }

    public class ProgressStatusMessenger : ValueChangedMessage<string>
    {
        public ProgressStatusMessenger(string value) : base(value) { }
    }

    public class DataStreamNetworkChangeMessenger : ValueChangedMessage<bool>
    {
        public DataStreamNetworkChangeMessenger(bool value) : base(value) { }
    }

    public class BarcodeMessenger : ValueChangedMessage<string>
    {
        public BarcodeMessenger(string value) : base(value) { }
    }

    public class ProductProgressMessenger : ValueChangedMessage<int>
    {
        public ProductProgressMessenger(int value) : base(value) { }
    }

    public class ResetDataMessenger : ValueChangedMessage<bool>
    {
        public ResetDataMessenger(bool value) : base(value) { }
    }

    public class ProductStockChangeMessenger : ValueChangedMessage<ProductDto_POS>
    {
        public ProductStockChangeMessenger(ProductDto_POS value) : base(value) { }
    }

    public class UpdateShopDataMessenger : ValueChangedMessage<bool>
    {
        public UpdateShopDataMessenger(bool value) : base(value) { }
    }

    public class SignalRPaymentTypesMessenger : ValueChangedMessage<PaymentOptionDto>
    {
        public SignalRPaymentTypesMessenger(PaymentOptionDto value) : base(value) { }
    }

    // public class SignalRReceiptTemplateMessenger : ValueChangedMessage<ReceiptTemplateDto>
    // {
    //     public SignalRReceiptTemplateMessenger(ReceiptTemplateDto value) : base(value) { }
    // }

    public class SignalRGiftReceiptTemplateMessenger : ValueChangedMessage<ReceiptTemplateDto>
    {
        public SignalRGiftReceiptTemplateMessenger(ReceiptTemplateDto value) : base(value) { }
    }

    public class SignalRDocketReceiptTemplateMessenger : ValueChangedMessage<ReceiptTemplateDto>
    {
        public SignalRDocketReceiptTemplateMessenger(ReceiptTemplateDto value) : base(value) { }
    }

    public class AllReceiptRegisteredMessenger : ValueChangedMessage<bool>
    {
        public AllReceiptRegisteredMessenger(bool value) : base(value) { }
    }

    public class EpsonPrinterFindMessenger : ValueChangedMessage<Printer>
    {
        public EpsonPrinterFindMessenger(Printer value) : base(value) { }
    }

    public class PosDeviceStatusMessenger : ValueChangedMessage<string>
    {
        public PosDeviceStatusMessenger(string value) : base(value) { }
    }

    public class PaymentOptionConfiguredMessenger : ValueChangedMessage<string>
    {
        public PaymentOptionConfiguredMessenger(string value) : base(value) { }
    }

    public class ManualPrintMessenger : ValueChangedMessage<bool>
    {
        public ManualPrintMessenger(bool value) : base(value) { }
    }

    public class OnlyVantivPrintMessenger : ValueChangedMessage<bool>
    {
        public OnlyVantivPrintMessenger(bool value) : base(value) { }
    }

    public class IncludeVantivPrintMessenger : ValueChangedMessage<bool>
    {
        public IncludeVantivPrintMessenger(bool value) : base(value) { }
    }

    public class AutoPrintMessenger : ValueChangedMessage<AutoPrintMessageCenter>
    {
        public AutoPrintMessenger(AutoPrintMessageCenter value) : base(value) { }
    }

    public class iZettleTransactionCompleteCallbackMessenger : ValueChangedMessage<iZettlePaymentResult>
    {
        public iZettleTransactionCompleteCallbackMessenger(iZettlePaymentResult value) : base(value) { }
    }

    public class SurchargeApplyMessenger : ValueChangedMessage<bool>
    {
        public SurchargeApplyMessenger(bool value) : base(value) { }
    }

    // Start #90942 iOS:FR Cheque number for sale by pratik
    public class ChequeNumberMessenger : ValueChangedMessage<string>
    {
        public ChequeNumberMessenger(string value) : base(value) { }
    }
    // End #90942 by pratik

    public class PaypalAutoPrintMessenger : ValueChangedMessage<bool>
    {
        public PaypalAutoPrintMessenger(bool value) : base(value) { }
    }

    public class SquarePaymentResultMessenger : ValueChangedMessage<SquarePaymentResult>
    {
        public SquarePaymentResultMessenger(SquarePaymentResult value) : base(value) { }
    }

    public class VantivTransactionCompleteCallbackMessenger : ValueChangedMessage<VantivReceiptPrintModel>
    {
        public VantivTransactionCompleteCallbackMessenger(VantivReceiptPrintModel value) : base(value) { }
    }

    public class TyroTransactionCompleteMessenger : ValueChangedMessage<PaymentResult>
    {
        public TyroTransactionCompleteMessenger(PaymentResult value) : base(value) { }
    }

    public class TyroTransactionCancelMessenger : ValueChangedMessage<bool>
    {
        public TyroTransactionCancelMessenger(bool value) : base(value) { }
    }

    public class MintPaymentStatusMessenger : ValueChangedMessage<Message<MintPaymentSummary>>
    {
        public MintPaymentStatusMessenger(Message<MintPaymentSummary> value) : base(value) { }
    }

    public class MintReaderStatusMessenger : ValueChangedMessage<Message<PaymentConfigurationModel>>
    {
        public MintReaderStatusMessenger(Message<PaymentConfigurationModel> value) : base(value) { }
    }

    public class CustomPaymentTransactionCompleteMessenger : ValueChangedMessage<CustomPaymentResult>
    {
        public CustomPaymentTransactionCompleteMessenger(CustomPaymentResult value) : base(value) { }
    }

    public class CustomPaymentTransactionCancelMessenger : ValueChangedMessage<bool>
    {
        public CustomPaymentTransactionCancelMessenger(bool value) : base(value) { }
    }

    public class TouchEventFiredMessenger : ValueChangedMessage<bool>
    {
        public TouchEventFiredMessenger(bool value) : base(value) { }
    }
}

