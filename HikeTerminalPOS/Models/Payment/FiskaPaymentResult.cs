using System;
namespace HikePOS.Models.Payment
{
    public class FiskaPaymentResult
    {


        public string ReferenceCode { get; set; }
        public string TerminalId { get; set; }

        public string ResultTitle { get; set; }
        public string ResultText { get; set; }
        public string RegisterId { get; set; }

        public OperationStatus operationStatus { get; set; }

        public ResultCode ResultCode { get; set; }
        public int ApprovedAmount { get; set; }
        public string TerminalMessage { get; set; }
        public string OperationId { get; set; }
        public string TerminalCode { get; set; }

        public string CustomErrormessage { get; set; }
    }




    public enum ResultCode
    {
        FSKResultCodeSuccess = 0,
        FSKResultCodeUserCancelled = 1,
        FSKResultCodeBatchEmpty = 2,
        FSKResultCodeReceiptMessage = 3,
        FSKResultCodeSuccessWithBalance = 4,

        FSKResultCodeAdminMode = 1201,
        FSKResultCodeFailedCouldNotBeCompleted = 1204,
        FSKResultCodeDeclinedGeneral = 2200,
        FSKResultCodeDeclinedByMerchant = 2203,
        FSKResultCodeDeclinedTimedOut = 2204,
        FSKResultCodeUninitializedMerchant = 3001,
        FSKResultCodeReferenceCodeRejectedByTerminal = 3002,
        FSKResultCodeFailedCommunicationError = 3005,
        FSKResultCodeFailedBatteryLow = 3006
    }


    public enum OperationStatus
    {
        FSKOperationStatusInProcess = 0,
        FSKOperationStatusCompleted = 1
    }


    public enum ErrorCode
    {
        FSKPaymentSdkOperationErrorConnectivity = 1,  //generated if the connectivity is broken between the device and the terminal
        FSKPaymentSdkOperationErrorMerchantNotActivated = 2, ////generated if the merchant is not activated
        FSKPaymentSdkOperationErrorInternal = 3 //generated on internal errors
    }
}
