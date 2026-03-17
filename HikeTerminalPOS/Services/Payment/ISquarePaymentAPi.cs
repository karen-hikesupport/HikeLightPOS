using System;
using System.Threading.Tasks;
using HikePOS.Models;
using HikePOS.Models.Payment;
using Refit;

namespace HikePOS.Services.Payment
{
    [Headers("Accept: application/json")]
    public interface ISquarePaymentApi
    { 
        [Post("/api/services/app/squarePayment/CreateSquareTerminalCheckout")]
        Task<ResponseModel<SquareTerminalCheckoutResponse>> CreateSquareTerminalCheckout([Body]SquareTerminalCheckOutRequest input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/squarePayment/GetSquareTerminalCheckout")]
        Task<ResponseModel<SquareTerminalCheckoutResponse>> GetSquareTerminalPaymentStatus([Body] SquareTerminalPaymentStatusRequest input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/squarePayment/CancelSquareTerminalCheckout")]
        Task<ResponseModel<SquareTerminalCheckoutResponse>> CancelSquareTerminalCheckout([Body] SquareTerminalPaymentStatusRequest input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/squarePayment/CreateSquareTerminalRefund")]
        Task<ResponseModel<SquareTerminalRefundResponse>> CreateSquareTerminalRefund([Body] SquareTerminalRefundRequest input, [Header("Authorization")] string accessToken);

        [Post("/api/services/app/squarePayment/GetSquareRefund")]
        Task<ResponseModel<SquareTerminalRefundStatusRepsponse>> GetSquareTerminalRefundStatus([Body] SquareTerminalRefundStatusRequest input, [Header("Authorization")] string accessToken);


    }

}