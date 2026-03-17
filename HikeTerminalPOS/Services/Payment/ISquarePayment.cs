using System;
using HikePOS.Models.Payment;

namespace HikePOS.Services
{
    public interface ISquarePayment
    {
        bool PerformRequest(SquarePaymentConfigurationDto configurationDto, string locationId,string invoiceNumber, decimal amount, string notes);
    }
}
