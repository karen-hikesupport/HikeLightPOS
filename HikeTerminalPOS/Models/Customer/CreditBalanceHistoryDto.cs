using System;
using HikePOS.Models.Enum;
using Realms;
namespace HikePOS.Models.Customer
{
    public class CreditBalanceHistoryDto : FullAuditedPassiveEntityDto
    {
        public int CustomerId { get; set; }
        public String CustomerName { get; set; }
        public int? InvoiceId { get; set; }
        public String InvoiceNo { get; set; }
        public AccountTransactionType AccountTransactionType { get; set; }
        //Ticket start:#72472 Couldn't remove store credit payment from sales.by rupesh
        public string AccountTransactionTypeName
        {
            get { return AccountTransactionType == AccountTransactionType.CreditNote ? "Refund" : AccountTransactionType.ToString(); }
        }
        public int? TransactionId { get; set; }
        public Decimal NetSaleAmount { get; set; }
        public Decimal CreditAmount
        {
            get { return Credit > 0 ? Credit : (-1) * Debit; }
        }
        //Ticket end:#72472.by rupesh
        public Decimal OpeningBalance { get; set; }
        public Decimal Debit { get; set; }
        public Decimal Credit { get; set; }
        public Decimal ClosingBalance { get; set; }
        public String Note { get; set; }
        public String ServerdBy { get; set; }
        public DateTime CreationTime { get; set; }

        public DateTime TransactionDate { get; set; }

        public bool IsSync { get; set; } = false;
        public string TempId { get; set; }
        public CreditBalanceHistoryDB ToModel()
        {
            CreditBalanceHistoryDB creditBalanceHistoryDB = new CreditBalanceHistoryDB
            {
                Id = Id,
                IsActive = IsActive,
                CustomerId = CustomerId,
                CustomerName = CustomerName,
                InvoiceId = InvoiceId,
                InvoiceNo = InvoiceNo,
                AccountTransactionType = (int)AccountTransactionType,
                TransactionId = TransactionId,
                NetSaleAmount = NetSaleAmount,
                OpeningBalance = OpeningBalance,
                Debit = Debit,
                Credit = Credit,
                ClosingBalance = ClosingBalance,
                Note = Note,
                ServerdBy = ServerdBy,
                CreationTime = CreationTime,
                TransactionDate = TransactionDate,
                IsSync = IsSync,
                TempId = TempId

            };
            return creditBalanceHistoryDB;
        }
        public static CreditBalanceHistoryDto FromModel(CreditBalanceHistoryDB creditBalanceHistoryDB)
        {
            CreditBalanceHistoryDto creditBalanceHistoryDto = new CreditBalanceHistoryDto
            {
                Id = creditBalanceHistoryDB.Id,
                IsActive = creditBalanceHistoryDB.IsActive,
                CustomerId = creditBalanceHistoryDB.CustomerId,
                CustomerName = creditBalanceHistoryDB.CustomerName,
                InvoiceId = creditBalanceHistoryDB.InvoiceId,
                InvoiceNo = creditBalanceHistoryDB.InvoiceNo,
                AccountTransactionType = (AccountTransactionType)creditBalanceHistoryDB.AccountTransactionType,
                TransactionId = creditBalanceHistoryDB.TransactionId,
                NetSaleAmount = creditBalanceHistoryDB.NetSaleAmount,
                OpeningBalance = creditBalanceHistoryDB.OpeningBalance,
                Debit = creditBalanceHistoryDB.Debit,
                Credit = creditBalanceHistoryDB.Credit,
                ClosingBalance = creditBalanceHistoryDB.ClosingBalance,
                Note = creditBalanceHistoryDB.Note,
                ServerdBy = creditBalanceHistoryDB.ServerdBy,
                CreationTime = creditBalanceHistoryDB.CreationTime.UtcDateTime,
                TransactionDate = creditBalanceHistoryDB.TransactionDate.UtcDateTime,
                IsSync = creditBalanceHistoryDB.IsSync,
                TempId = creditBalanceHistoryDB.TempId

            };
            return creditBalanceHistoryDto;
        }
    }
    public partial class CreditBalanceHistoryDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public int AccountTransactionType { get; set; }
        public int? TransactionId { get; set; }
        public decimal NetSaleAmount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal ClosingBalance { get; set; }
        public string Note { get; set; }
        public string ServerdBy { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        public bool IsSync { get; set; } = false;
        public string TempId { get; set; }
    }

}
