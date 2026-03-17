using HikePOS.Enums;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public class RegisterCashInOutDto : FullAuditedPassiveEntityDto
    {
        public int RegisterClosureId { get; set; }
        public int PaymentOptionId { get; set; }
        public int UserId { get; set; }
        public RegisterCashType RegisterCashType { get; set; }
        public string RegisterCashTypeName { get; set; }
        public string UserBy { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }

        public RegisterCashInOutDB ToModel()
        {
            RegisterCashInOutDB model = new RegisterCashInOutDB
            {
                Id = Id,
                IsActive = IsActive,
                RegisterClosureId = RegisterClosureId,
                PaymentOptionId = PaymentOptionId,
                UserId = UserId,
                RegisterCashType = (int)RegisterCashType,
                RegisterCashTypeName = RegisterCashTypeName,
                UserBy = UserBy,
                Amount = Amount,
                Note = Note,
            };

            return model;
        }

        public static RegisterCashInOutDto FromModel(RegisterCashInOutDB dbModel)
        {
            if (dbModel == null)
                return null;

            RegisterCashInOutDto model = new RegisterCashInOutDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                RegisterClosureId = dbModel.RegisterClosureId,
                PaymentOptionId = dbModel.PaymentOptionId,
                UserId = dbModel.UserId,
                RegisterCashType = (RegisterCashType)dbModel.RegisterCashType,
                RegisterCashTypeName = dbModel.RegisterCashTypeName,
                UserBy = dbModel.UserBy,
                Amount = dbModel.Amount,
                Note = dbModel.Note,

            };

            return model;
        }
    }

    public partial class RegisterCashInOutDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int RegisterClosureId { get; set; }
        public int PaymentOptionId { get; set; }
        public int UserId { get; set; }
        public int RegisterCashType { get; set; }
        public string RegisterCashTypeName { get; set; }
        public string UserBy { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }

    public class RegisterCashInOutParam
    {
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }


    /// <summary>
    /// Tax list.
    /// Note : Sprint task Jul-2019.
    /// </summary>
    public class TaxList : BaseModel
    {
        public int TaxId { get; set; }
        //public string TaxName { get; set; }

        //[JsonIgnore]
        //public string TaxFullName { get; set; }


        public string _TaxName { get; set; }
        public string TaxName
        {
            get
            {
                return _TaxName;
            }
            set
            {
                _TaxName = value;
                SetPropertyChanged(nameof(TaxFullName));
                // TaxFullName = "";
            }
        }


        public string _TaxFullName { get; set; }

        [JsonIgnore]
        public string TaxFullName
        {
            get
            {
                if (TaxName != null && TaxSaleAmount.ToString() != null)
                {
                    //Ticket #10462 Start : Tax rate getting cut on register summary print. By Nikhil
                    return TaxName + " (Sales " + TaxSaleAmount .ToString("C2") + ")";
                    //Ticket #10462 End : By Nikhil
                }
                else
                {
                    return "";
                }
            }
            set
            {
                TaxFullName = value;
                SetPropertyChanged(nameof(TaxFullName));
            }
        }



        public decimal TaxRate { get; set; }
        //public decimal TaxAmount { get; set; }
        //public decimal TaxSaleAmount { get; set; }
        public List<TaxList> SubTaxes { get; set; }

        decimal _TaxAmount { get; set; }
        public decimal TaxAmount
        {
            get { return _TaxAmount; }
            set
            {
                _TaxAmount = value;

            }
        }


        decimal _TaxSaleAmount { get; set; }
        public decimal TaxSaleAmount
        {
            get { return _TaxSaleAmount; }
            set
            {
                _TaxSaleAmount = value;

            }
        }

        public TaxListDB ToModel()
        {
            TaxListDB tax = new TaxListDB
            {
                TaxId = TaxId,
                TaxName = TaxName,
                TaxRate = TaxRate,
                TaxAmount = TaxAmount,
                TaxSaleAmount = TaxSaleAmount
            };
            SubTaxes?.ForEach(i => tax.SubTaxes.Add(i.ToModel()));
            return tax;
        }

        public static TaxList FromModel(TaxListDB taxDB)
        {
            if (taxDB == null)
                return null;

            TaxList tax = new TaxList
            {
                TaxId = taxDB.TaxId,
                TaxName = taxDB.TaxName,
                TaxRate = taxDB.TaxRate,
                TaxAmount = taxDB.TaxAmount,
                TaxSaleAmount = taxDB.TaxSaleAmount
            };
            tax.SubTaxes = new List<TaxList>(taxDB.SubTaxes.Select(a => TaxList.FromModel(a)));
            return tax;
        }

    }

    public partial class TaxListDB : IRealmObject
    {
        [PrimaryKey]
        public int TaxId { get; set; }
        public string TaxName { get; set; }
        public decimal TaxRate { get; set; }
        public IList<TaxListDB> SubTaxes { get; }
        public decimal TaxAmount { get; set; }
        public decimal TaxSaleAmount { get; set; }
    }

    /// <summary>
    /// Register closure transaction details dto.
    ///  Note : Sprint task Jul-2019.
    /// </summary>


    [JsonObject("RegisterClosureTransactionDetailsDto2")]
    public class RegisterClosureTransactionDetailsDto
    {
        [JsonProperty("totalNewCustomers")]
        public int TotalNewCustomers { get; set; }
        [JsonProperty("totalTransactions")]
        public int TotalTransactions { get; set; }
        [JsonProperty("avgSaleAmount")]
        public double AvgSaleAmount { get; set; }

        public RegisterClosureTransactionDetailsDB ToModel()
        {
            RegisterClosureTransactionDetailsDB model = new RegisterClosureTransactionDetailsDB
            {
                TotalNewCustomers = TotalNewCustomers,
                TotalTransactions = TotalTransactions,
                AvgSaleAmount = AvgSaleAmount
            };

            return model;
        }

        public static RegisterClosureTransactionDetailsDto FromModel(RegisterClosureTransactionDetailsDB dbModel)
        {
            if (dbModel == null)
                return null;

            RegisterClosureTransactionDetailsDto model = new RegisterClosureTransactionDetailsDto
            {
                TotalNewCustomers = dbModel.TotalNewCustomers,
                TotalTransactions = dbModel.TotalTransactions,
                AvgSaleAmount = dbModel.AvgSaleAmount
            };

            return model;
        }
    }

    public partial class RegisterClosureTransactionDetailsDB : IRealmObject
    {
        public int TotalNewCustomers { get; set; }
        public int TotalTransactions { get; set; }
        public double AvgSaleAmount { get; set; }
    }

}
