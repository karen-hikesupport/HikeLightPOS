using System;
using System.Collections.ObjectModel;
using System.Linq;
using HikePOS.Enums;
using Newtonsoft.Json;
using Realms;
namespace HikePOS.Models
{
    public class InvoicePaymentDto : FullAuditedPassiveEntityDto
    {
        public InvoicePaymentDto()
        {
            InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>();
            //Ticket #8290 Start: Partial refund slider. By Nikhil.
            IsRefundSucceed = false;
            //Ticket #8290 End:By Nikhil. 
        }

        [JsonProperty("syncPaymentReference")]
        public string SyncPaymentReference { get; set; }

        public int InvoiceId { get; set; }
        public int PaymentOptionId { get; set; }
        public string PaymentOptionName { get; set; }

        // Start #90942 iOS:FR Cheque number for sale by pratik
        [JsonIgnore]
        public string PaymentOptionDisplayName => PaymentOptionName + ((InvoicePaymentDetails!=null && InvoicePaymentDetails.Any(a=>a.Key == InvoicePaymentKey.CheckIDAtCheckout)) ? " \n(" + InvoicePaymentDetails.First(a=>a.Key == InvoicePaymentKey.CheckIDAtCheckout).Value + ")": "" );

        [JsonIgnore]
        public string PrintPaymentOptionDisplayName => (string.IsNullOrEmpty(PrintPaymentOptionName) ? PaymentOptionName : PrintPaymentOptionName) + ((InvoicePaymentDetails!=null && InvoicePaymentDetails.Any(a=>a.Key == InvoicePaymentKey.CheckIDAtCheckout)) ? " (" + InvoicePaymentDetails.First(a=>a.Key == InvoicePaymentKey.CheckIDAtCheckout).Value + ")": "" );
        // End #90942 by pratik

        [JsonIgnore]
        string _PrintPaymentOptionName;
        public string PrintPaymentOptionName
        {
            get { return _PrintPaymentOptionName; }
            set
            {
                _PrintPaymentOptionName = value;
                SetPropertyChanged(nameof(PrintPaymentOptionName));
            }
        }

        public PaymentOptionType PaymentOptionType { get; set; }
        public decimal TenderedAmount { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsDeleted { get; set; }
        public int? RegisterId { get; set; }
        public string RegisterName { get; set; }
        public ActionType ActionType { get; set; }
        public DateTime? PaymentDate { get; set; }

        [JsonIgnore]
        public DateTime? PaymentStoreDate
        {
            get
            {
                if (PaymentDate != null)
                {
                    return PaymentDate.Value.ToStoreTime();
                }
                else
                {
                    return null;
                }
            }
        }

        public InvoiceFrom PaymentFrom { get; set; }
        public string ServedBy { get; set; }
        public int OutletId { get; set; }
        public String OutletName { get; set; }
        public ObservableCollection<InvoicePaymentDetailDto> InvoicePaymentDetails { get; set; }

        //[JsonIgnore]
        //public string PaymentDetails
        //{
        //    get
        //    {
        //        if (InvoicePaymentDetails != null && InvoicePaymentDetails.Count > 0)
        //        {
        //            return string.Join(",", InvoicePaymentDetails.Select(x =>
        //             {
        //                 return x.Key.ToString() + ":" + x.Value.ToString();
        //             }));
        //        }
        //        else
        //        {
        //            return "";
        //        }
        //    }
        //}

        public int? RegisterClosureId { get; set; }

        public decimal? BackorderPayment { get; set; }

        public decimal? LoyaltyPoints { get; set; }

        public decimal RoundingAmount { get; set; }

        //Ticket #8290 Start: Partial refund slider. By Nikhil.
        [JsonIgnore]
        bool isRefundSuceed;
        public bool IsRefundSucceed
        {
            get { return isRefundSuceed; }
            set
            {
                isRefundSuceed = value;
                SetPropertyChanged(nameof(IsRefundSucceed));
            }
        }
        //Ticket #8290 End:By Nikhil.
   
        bool _isDeletePaymentActive;
        public bool IsDeletePaymentActive
        {
            get { return _isDeletePaymentActive; }
            set
            {
                _isDeletePaymentActive = value;
                SetPropertyChanged(nameof(IsDeletePaymentActive));
            }
        }
        public int? CreatorUserId { get; set; }

        //Ticket start:#91263 iOS:FR Redeeming Points.by rupesh
        [JsonIgnore]
        public string ApprovedBy
        {
            get { 
                 if(PaymentOptionType == PaymentOptionType.Loyalty && InvoicePaymentDetails?.LastOrDefault() != null && InvoicePaymentDetails.LastOrDefault().Key == InvoicePaymentKey.ApprovedByUser)
                 {
                   return InvoicePaymentDetails.LastOrDefault().Value;
                 }
                 else
                   return null;
             }
        }
        //Ticket end:#91263.by rupesh

        public InvoicePaymentDB ToModel()
        {
            InvoicePaymentDB invoicePaymentDB = new InvoicePaymentDB
            {
                Id = Id,
                IsActive = IsActive,
                SyncPaymentReference = SyncPaymentReference,
                InvoiceId = InvoiceId,
                PaymentOptionId = PaymentOptionId,
                PaymentOptionName = PaymentOptionName,
                PrintPaymentOptionName = PrintPaymentOptionName,
                PaymentOptionType = (int)PaymentOptionType,
                TenderedAmount = TenderedAmount,
                Amount = Amount,
                IsPaid = IsPaid,
                IsDeleted = IsDeleted,
                RegisterId = RegisterId,
                RegisterName = RegisterName,
                ActionType = (int)ActionType,
                PaymentDate = PaymentDate,
                PaymentFrom = (int)PaymentFrom,
                ServedBy = ServedBy,
                OutletId = OutletId,
                OutletName = OutletName,
                RegisterClosureId = RegisterClosureId,
                BackorderPayment = BackorderPayment,
                RoundingAmount = RoundingAmount,
                IsRefundSucceed = IsRefundSucceed,
                IsDeletePaymentActive = IsDeletePaymentActive,
                LoyaltyPoints = LoyaltyPoints,
                CreatorUserId = CreatorUserId

            };
            InvoicePaymentDetails?.ForEach(i => invoicePaymentDB.InvoicePaymentDetails.Add(i.ToModel()));
            return invoicePaymentDB;
        }
        public static InvoicePaymentDto FromModel(InvoicePaymentDB invoicePaymentDB)
        {
            if (invoicePaymentDB == null)
                return null;

            InvoicePaymentDto invoicePaymentDto = new InvoicePaymentDto
            {
                Id = invoicePaymentDB.Id,
                IsActive = invoicePaymentDB.IsActive,
                SyncPaymentReference = invoicePaymentDB.SyncPaymentReference,
                InvoiceId = invoicePaymentDB.InvoiceId,
                PaymentOptionId = invoicePaymentDB.PaymentOptionId,
                PaymentOptionName = invoicePaymentDB.PaymentOptionName,
                PrintPaymentOptionName = invoicePaymentDB.PrintPaymentOptionName,
                PaymentOptionType = (PaymentOptionType)invoicePaymentDB.PaymentOptionType,
                TenderedAmount = invoicePaymentDB.TenderedAmount,
                Amount = invoicePaymentDB.Amount,
                IsPaid = invoicePaymentDB.IsPaid,
                IsDeleted = invoicePaymentDB.IsDeleted,
                RegisterId = invoicePaymentDB.RegisterId,
                RegisterName = invoicePaymentDB.RegisterName,
                ActionType = (ActionType)invoicePaymentDB.ActionType,
                PaymentDate = invoicePaymentDB.PaymentDate?.UtcDateTime,
                PaymentFrom = (InvoiceFrom)invoicePaymentDB.PaymentFrom,
                ServedBy = invoicePaymentDB.ServedBy,
                OutletId = invoicePaymentDB.OutletId,
                OutletName = invoicePaymentDB.OutletName,
                RegisterClosureId = invoicePaymentDB.RegisterClosureId,
                BackorderPayment = invoicePaymentDB.BackorderPayment,
                RoundingAmount = invoicePaymentDB.RoundingAmount,
                IsRefundSucceed = invoicePaymentDB.IsRefundSucceed,
                IsDeletePaymentActive = invoicePaymentDB.IsDeletePaymentActive,
                LoyaltyPoints = invoicePaymentDB.LoyaltyPoints,
                CreatorUserId = invoicePaymentDB.CreatorUserId
            };
            if (invoicePaymentDB.InvoicePaymentDetails != null)
                invoicePaymentDto.InvoicePaymentDetails = new ObservableCollection<InvoicePaymentDetailDto>(invoicePaymentDB.InvoicePaymentDetails.Select(a => InvoicePaymentDetailDto.FromModel(a)));

            return invoicePaymentDto;

        }

    }
    public partial class InvoicePaymentDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string SyncPaymentReference { get; set; }

        public int InvoiceId { get; set; }
        public int PaymentOptionId { get; set; }
        public string PaymentOptionName { get; set; }
        public string PrintPaymentOptionName { get; set; }
        public int PaymentOptionType { get; set; }
        public decimal TenderedAmount { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsDeleted { get; set; }
        public int? RegisterId { get; set; }
        public string RegisterName { get; set; }
        public int ActionType { get; set; }
        public DateTimeOffset? PaymentDate { get; set; }

        public int PaymentFrom { get; set; }
        public string ServedBy { get; set; }
        public int OutletId { get; set; }
        public String OutletName { get; set; }
        public IList<InvoicePaymentDetailDB> InvoicePaymentDetails { get; }

        public int? RegisterClosureId { get; set; }

        public decimal? BackorderPayment { get; set; }

        public decimal RoundingAmount { get; set; }

        public bool IsRefundSucceed { get; set; }
        public bool IsDeletePaymentActive { get; set; }
        public decimal? LoyaltyPoints { get; set; }
        public int? CreatorUserId { get; set; }
    }

    public class RefundPaymentDto : BaseNotify
    {
         [JsonIgnore]
        string _PrintPaymentOptionName;
        public string PrintPaymentOptionName
        {
            get { return _PrintPaymentOptionName; }
            set
            {
                _PrintPaymentOptionName = value;
                SetPropertyChanged(nameof(PrintPaymentOptionName));
            }
        }

        private decimal _returnAmount;
        public decimal ReturnAmount
        {
            get { return _returnAmount; }
            set
            {
                _returnAmount = value;
                SetPropertyChanged(nameof(ReturnAmount));
            }
        }
   
        private int _id;
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                SetPropertyChanged(nameof(Id));
            }
        }
   

        private string _tenderedAmount;
        public string TenderedAmount
        {
            get { return _tenderedAmount; }
            set
            {
                _tenderedAmount = value;
                SetPropertyChanged(nameof(TenderedAmount));
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                SetPropertyChanged(nameof(Amount));
            }
        }
    }
}
