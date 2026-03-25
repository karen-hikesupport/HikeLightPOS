using System;
using System.Runtime.CompilerServices;
using HikePOS.Models;
using HikePOS.Models.Payment;

namespace HikePOS.Services
{
    public interface INadaPayTerminalAppService 
    {
        Task<HikePayTerminalResponse> CreateNadaPayTerminalSale(decimal amount, string currency, string lastReference, PaymentOptionDto paymentOption, string InvoiceSyncReference);
        Task<HikePayTerminalResponse> CreateNadaPayTerminalRefund(bool isPartial, decimal amount, string currency, string saleId, string merchantReference, string poiTransactionId, DateTime poiTransactionTimeStamp, decimal surcharge, decimal fixedCommission, decimal variableCommissionPercentage, decimal paymentFee, PaymentOptionDto paymentOption, NadaPayTransactionDto nadaPayTransactionDto);
        Task<HikePayTerminalResponse> CreateNadaPayTerminalCancel(decimal amount, string currency, string lastReference, PaymentOptionDto paymentOption, string serviceId);
        Task<HikePayTerminalResponse> VerifyNadaPayTerminalSale(decimal amount, string currency, string lastReference, PaymentOptionDto paymentOption, string serviceId);
        Task<HikePayTerminalResponse> DiagnosisRequestToTerminal(string terminalId);

    }
}
