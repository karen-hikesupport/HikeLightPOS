using System;
using Newtonsoft.Json;

namespace HikePOS.Models.Payment
{
    public class VantivConfigurationDto
    {

        //Web properties
        public int PaymentOptionId { get; set; }
        public string DeveloperKey { get; set; }
        public string DeveloperSecret { get; set; }
        public string LaneID { get; set; }
        public string DeviceIP { get; set; }
        public string DevicePort { get; set; }


        //iPad properties
        public string TerminalId { get; set; }
        public string AccountId { get; set; }
        public string AcceptorId { get; set; }
        public string AccountToken { get; set; }

        public bool ArePartialApprovalsAllowed { get; set; }
        public bool AreDuplicateTransactionsAllowed { get; set; }
        public bool IsCashbackAllowed { get; set; }
        public double CashbackAmount { get; set; }
        public bool IsDebitAllowed { get; set; }
        public bool IsEmvAllowed { get; set; }
    }
}
