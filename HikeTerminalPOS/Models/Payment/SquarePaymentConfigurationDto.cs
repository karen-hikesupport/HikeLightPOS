using System;
using System.Collections.ObjectModel;
using HikePOS.Enums;

namespace HikePOS.Models.Payment
{
    public class SquarePaymentConfigurationDto
    {
        public static string TERMINAL_STATUS_COMPLETED = "COMPLETED";
        public static string TERMINAL_STATUS_PENDING = "PENDING";
        public static string TERMINAL_STATUS_CANCELED = "CANCELED";
        public static string TERMINAL_STATUS_IN_PROGRESS = "IN_PROGRESS";
        public static string Def_Currency = "AUD"; 

        public string Currency { get; set; } = Def_Currency;

        public string access_token;
        public string refresh_token;
        public string merchant_id;

        public bool isTerminal;

        public ObservableCollection<RegisterLocation> registerLocation;

        public class RegisterLocation
        {
            public int outletId;
            public int registerId;
            public string registerName;
            public string locationId;

            public DeviceDetails deviceDetails;
        }

        public class DeviceDetails
        {
            public string id;
            public string deviceId;
            public string deviceName;
            public string deviceCode;
        }


        public SquareRequestTenderTypes SquareRequestTenderType { get; set; } = SquarePaymentConfigurationDto.SquareRequestTenderTypes.All;

        public enum SquareRequestTenderTypes : ulong
        {
            All = (9223372036854775807UL * 2 + 1),
            Card = 1 << 0,
            Cash = 1 << 1,
            Other = 1 << 2,
            SquareGiftCard = 1 << 3,
            CardOnFile = 1 << 4
        }
    }

    public class SquareTerminalCheckOutRequest
    {
        public int paymentId;
        public string squareConfiguratonDetail;
        public SquareCheckOutRequest squareCheckOutRequest;
    }

    public class SquareCheckOutRequest
    {
        public string idempotency_key;
        public Checkout checkout;
    }

    public class Checkout
    {
        public string id;
        public string reference_id;
        public string note;
        public string status;
        public string app_id;
        public string location_id;
        public string deadline_duration;
        public AmountMoney amount_money;
        public DeviceOptions device_options;
        public string[] payment_ids;
    }

    public class AmountMoney
    {
        public int amount;
        public string currency;
    }

    public class DeviceOptions
    {
        public string device_id;
        public bool skip_receipt_screen;
        public TipSettings tip_settings;
    }

    public class TipSettings
    {
        public bool allow_tipping;
    }

    public class SquareTerminalCheckoutResponse
    {
        public Checkout checkout;
        public SquareTerminalError[] errors;
    }

    public class SquareTerminalPaymentStatusRequest
    {
        public int paymentId;
        public string checkoutId;
        public string squareConfiguratonDetail;
    }

    public class SquareTerminalRefundRequest
    {
        public int paymentId;
        public string squareConfiguratonDetail;
        public SquareRefundRequestTerminal squareRefundRequestTerminal;
    }

    public class SquareRefundRequestTerminal
    {
        public string payment_id;
        public string reason;
        public string idempotency_key;
        public AmountMoney amount_money;
    }

    public class SquareTerminalRefundResponse
    { 
        public string payment_id;
        public Refund refund;
        public SquareTerminalError[] errors;
    }

    public class SquareTerminalRefundStatusRequest
    {
        public int paymentId;
        public string refundId;
        public string squareConfiguratonDetail;
    }

    public class SquareTerminalRefundStatusRepsponse
    {
        public Refund refund;
        public SquareTerminalError[] errors;
    }

    public class Refund
    {
        public string id;
        public string status;
        public string payment_id;
        public string order_id;
        public string location_id;
        public AmountMoney amount_money;
        public ProcessingFee[] processing_fee;
    }

    public class ProcessingFee
    {
        public string effective_at;
        public string type;
        public AmountMoney amount_money;
    }

    public class SquareTerminalError
    {
        public string category;
        public string code;
        public string detail;
    }
}
