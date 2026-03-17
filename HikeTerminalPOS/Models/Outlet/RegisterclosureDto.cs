using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HikePOS.Helpers;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public class RegisterclosureDto : FullAuditedPassiveEntityDto
    {
        public RegisterclosureDto()
        {
            RegisterclosuresTallys = new ObservableCollection<RegisterclosuresTallyDto>();
            RegisterCashInOuts = new ObservableCollection<RegisterCashInOutDto>();
        }

        public int RegisterId { get; set; }
        public DateTime? _startDateTime { get; set; }
        public DateTime? StartDateTime
        {
            get
            {
                return _startDateTime;
            }
            set
            {
                _startDateTime = value;
                SetPropertyChanged(nameof(StartDateTime));
                StartDateTimeString = "";
            }
        }

        public string _startDateTimeString { get; set; }

        [JsonIgnore]
        public string StartDateTimeString
        {
            get
            {
                if (StartDateTime != null)
                {
                    if (EndDateTime == null)
                    {
                        return string.Format("Opened at {0:dd MMM, yyyy hh.mmtt}", StartDateTime.Value.ToStoreTime());
                    }
                    else
                    {
                        //return "";//string.Format("Opened at {0:hh.mmtt} on {0:dddd, dd MMMM yyyy} and Closed at {1:hh.mmtt} on {1:dddd, dd MMMM yyyy}", StartDateTime.Value.ToStoreTime(), EndDateTime.Value.ToStoreTime());
                         return string.Format("Closed at {0:dd MMM, yyyy hh.mmtt}", EndDateTime.Value.ToStoreTime());

                    }
                }
                else
                {
                    return "";
                }
            }
            set
            {
                _startDateTimeString = value;
                SetPropertyChanged(nameof(StartDateTimeString));
            }
        }


        public DateTime? _endDateTime { get; set; }

        public DateTime? EndDateTime
        {
            get
            {
                return _endDateTime;
            }
            set
            {
                _endDateTime = value;
            }
        }

        public string RegisterName { get; set; }
        public string OutletName { get; set; }

        public int? StartBy { get; set; }
        public int? CloseBy { get; set; }
        public string StartByUser { get; set; }
        public string CloseByUser { get; set; }

        decimal? _TotalSales { get; set; }
        public decimal? TotalSales
        {
            get
            {
                return _TotalSales;
            }
            set
            {
                if (value == null)
                    _TotalSales = 0;
                else
                    _TotalSales = value;
            }
        }

        decimal? _TotalOnAccountSales { get; set; }
        public decimal? TotalOnAccountSales
        {
            get
            {
                return _TotalOnAccountSales;
            }
            set
            {
                if (value == null)
                    _TotalOnAccountSales = 0;
                else
                    _TotalOnAccountSales = value;
            }
        }

        public decimal? TotalCompletedSales { get; set; } //Start #92768 By Pratik
        public decimal? TotalParkedSales { get; set; } //Start #92768 By Pratik
        public decimal? TotalLayBySales { get; set; } //Start #92768 By Pratik
 
        public decimal? _Difference { get; set; }
        public decimal? Difference
        {
            get
            {
                return _Difference;
            }
            set
            {
                if (value == null)
                    _Difference = 0;
                else
                    _Difference = value;
            }
        }
        public decimal? _TotalDiscounts { get; set; }
        public decimal? TotalDiscounts
        {
            get
            {
                return _TotalDiscounts;
            }
            set
            {
                if (value == null)
                    _TotalDiscounts = 0;
                else
                    _TotalDiscounts = value;
            }
        }

        //Ticket start:#62955 IOS Cash register not showing discount amount .by rupesh
        public decimal? _ItemDiscounts { get; set; }
        public decimal? ItemDiscounts
        {
            get
            {
                return _ItemDiscounts;
            }
            set
            {
                if (value == null)
                    _ItemDiscounts = 0;
                else
                    _ItemDiscounts = value;
            }
        }
        [JsonIgnore]
        public decimal? Discounts
        {
            get
            {
                return TotalDiscounts + ItemDiscounts;
            }
        }

        //Ticket end:#62955 .by rupesh

        public decimal? _TotalTax { get; set; }
        public decimal? TotalTax
        {
            get
            {
                return _TotalTax;
            }
            set
            {
                if (value == null)
                    _TotalTax = 0;
                else
                    _TotalTax = value;
            }
        }

        public decimal? _TotalTip { get; set; }
        public decimal? TotalTip
        {
            get
            {
                return _TotalTip;
            }
            set
            {
                if (value == null)
                    _TotalTip = 0;
                else
                    _TotalTip = value;
            }
        }
        public decimal? _TotalPayments { get; set; }
        public decimal? TotalPayments
        {
            get
            {
                return _TotalPayments;
            }
            set
            {
                if (value == null)
                    _TotalPayments = 0;
                else
                    _TotalPayments = value;
            }
        }
        decimal? _TotalRefunds { get; set; }
        public decimal? TotalRefunds
        {
            get
            {
                return _TotalRefunds;
            }
            set
            {
                if (value == null)
                    _TotalRefunds = 0;
                else
                    _TotalRefunds = value;
            }
        }

        //Ticket #10984 Start : Register Summary add field Net Receipts. By Nikhil
        //Start #77336 POS ipad cash register crashing regularly by Pratik
        [JsonIgnore]
        public decimal NetReceipts
        {
            get
            {
                return TotalPayments.HasValue == true ? (TotalPayments.Value - (TotalRefunds.HasValue ? TotalRefunds.Value.ToPositive() : 0)) : 0;
            }
        }
        //End #77336 by Pratik
        //Ticket #10984 End : By Nikhil
        private string _Notes;
        public string Notes
         {
            get
            {
                return _Notes;
            }
            set
            {
                _Notes = value;
                 SetPropertyChanged(nameof(Notes));
            }
         }
        public ObservableCollection<RegisterclosuresTallyDto> RegisterclosuresTallys { get; set; }
        public ObservableCollection<RegisterCashInOutDto> RegisterCashInOuts { get; set; }

        [JsonIgnore]
        public bool ShowExpectedColumn => Settings.GrantedPermissionNames == null ? true : Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.ShowExpectedColumn");

        //start: #91266 iOS:FR register report by pratik
        [JsonIgnore]
        public decimal RegisterTotalCashOuts => (RegisterCashInOuts != null  && RegisterCashInOuts.Count >0) ? RegisterCashInOuts.Where(a=>a.RegisterCashType == Enums.RegisterCashType.CashOut).Sum(a=>a.Amount) : 0;

        [JsonIgnore]
        public decimal RegisterTotalCashIn=> (RegisterCashInOuts != null  && RegisterCashInOuts.Count >0) ? RegisterCashInOuts.Where(a=>a.RegisterCashType == Enums.RegisterCashType.CashIn).Sum(a=>a.Amount) : 0;
        //end: #91266  by pratik
        
        public ObservableCollection<TaxList> TaxList { get; set; }

       // [JsonProperty("registerClosureTransactionDetailsDto")]
        public RegisterClosureTransactionDetailsDto registerClosureTransactionDetailsDto { get; set; }

        public string merchant_receipt { get; set; }

        //#32365 iPad :: Feature request :: Shipping fee not being recorded in Hike
        decimal? _TotalShipping { get; set; }
        public decimal? TotalShipping
        {
            get
            {
                return _TotalShipping;
            }
            set
            {
                if (value == null)
                    _TotalShipping = 0;
                else
                    _TotalShipping = value;
            }
        }
        //#32365 iPad :: Feature request :: Shipping fee not being recorded in Hike

        //Start #90940 ios :FR Batch number on cash register By Pratik
        public string refNumber { get; set; }
        //End #90940 By Pratik

        public RegisterclosureDB ToModel()
        {
            RegisterclosureDB model = new RegisterclosureDB
            {
                Id = Id,
                IsActive = IsActive,
                RegisterId = RegisterId,
                StartDateTime = StartDateTime,
                EndDateTime = EndDateTime,
                RegisterName = RegisterName,
                OutletName = OutletName,
                StartBy = StartBy,
                CloseBy = CloseBy,
                StartByUser = StartByUser,
                CloseByUser = CloseByUser,
                TotalSales = TotalSales,
                Difference = Difference,
                TotalDiscounts = TotalDiscounts,
                ItemDiscounts = ItemDiscounts,
                TotalTax = TotalTax,
                TotalTip = TotalTip,
                TotalPayments = TotalPayments,
                TotalRefunds = TotalRefunds,
                Notes = Notes,
                RegisterClosureTransactionDetails = registerClosureTransactionDetailsDto?.ToModel(),
                merchant_receipt = merchant_receipt,
                TotalShipping = TotalShipping,
                //Start #90940 ios :FR Batch number on cash register By Pratik
                refNumber = refNumber,
                //End #90940 By Pratik
                //Start #92768 By Pratik
                TotalCompletedSales = TotalCompletedSales,
                TotalParkedSales = TotalCompletedSales,
                TotalLayBySales = TotalCompletedSales,
                //End #92768 By Pratik
            };

            RegisterclosuresTallys?.ForEach(a => model.RegisterclosuresTallys.Add(a.ToModel()));
            RegisterCashInOuts?.ForEach(a => model.RegisterCashInOuts.Add(a.ToModel()));
            TaxList?.ForEach(a => model.TaxList.Add(a.ToModel()));

            return model;
        }


        public static RegisterclosureDto FromModel(RegisterclosureDB dbModel)
        {
            if (dbModel == null)
                return null;

            RegisterclosureDto model = new RegisterclosureDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                RegisterId = dbModel.RegisterId,
                StartDateTime = dbModel.StartDateTime?.UtcDateTime,
                EndDateTime = dbModel.EndDateTime?.UtcDateTime,
                RegisterName = dbModel.RegisterName,
                OutletName = dbModel.OutletName,
                StartBy = dbModel.StartBy,
                CloseBy = dbModel.CloseBy,
                StartByUser = dbModel.StartByUser,
                CloseByUser = dbModel.CloseByUser,
                TotalSales = dbModel.TotalSales,
                Difference = dbModel.Difference,
                TotalDiscounts = dbModel.TotalDiscounts,
                ItemDiscounts = dbModel.ItemDiscounts,
                TotalTax = dbModel.TotalTax,
                TotalTip = dbModel.TotalTip,
                TotalPayments = dbModel.TotalPayments,
                TotalRefunds = dbModel.TotalRefunds,
                Notes = dbModel.Notes,
                registerClosureTransactionDetailsDto = RegisterClosureTransactionDetailsDto.FromModel(dbModel.RegisterClosureTransactionDetails),
                merchant_receipt = dbModel.merchant_receipt,
                TotalShipping = dbModel.TotalShipping,
                //Start #90940 ios :FR Batch number on cash register By Pratik
                refNumber = dbModel.refNumber,
                //End #90940 By Pratik

                //Start #92768 By Pratik
                TotalCompletedSales = dbModel.TotalCompletedSales,
                TotalParkedSales = dbModel.TotalCompletedSales,
                TotalLayBySales = dbModel.TotalCompletedSales,
                //End #92768 By Pratik
            };
            model.RegisterCashInOuts = new ObservableCollection<RegisterCashInOutDto>(dbModel.RegisterCashInOuts.Select(a=> RegisterCashInOutDto.FromModel(a)));
            model.RegisterclosuresTallys = new ObservableCollection<RegisterclosuresTallyDto>(dbModel.RegisterclosuresTallys.Select(a => RegisterclosuresTallyDto.FromModel(a)));
            model.TaxList = new ObservableCollection<TaxList>(dbModel.TaxList.Select(a => HikePOS.Models.TaxList.FromModel(a)));
            return model;
        }
    }

    public class RecieptDataRequest
    {
        public int id { get; set; }
        public string merchant_receipt { get; set; }

    }

    public partial class RegisterclosureDB : IRealmObject
    {
        //[PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int RegisterId { get; set; }
        public DateTimeOffset? StartDateTime { get; set; }
        public DateTimeOffset? EndDateTime { get; set; }
        public string RegisterName { get; set; }
        public string OutletName { get; set; }
        public int? StartBy { get; set; }
        public int? CloseBy { get; set; }
        public string StartByUser { get; set; }
        public string CloseByUser { get; set; }
        public decimal? TotalSales { get; set; }
        public decimal? Difference { get; set; }
        public decimal? TotalDiscounts { get; set; }
        public decimal? ItemDiscounts { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal? TotalTip { get; set; }
        public decimal? TotalPayments { get; set; }
        public decimal? TotalRefunds { get; set; }
        public string Notes { get; set; }
        public IList<RegisterclosuresTallyDB> RegisterclosuresTallys { get; }
        public IList<RegisterCashInOutDB> RegisterCashInOuts { get; }
        public IList<TaxListDB> TaxList { get; }
        public RegisterClosureTransactionDetailsDB RegisterClosureTransactionDetails { get; set; }
        public string merchant_receipt { get; set; }
        public decimal? TotalShipping { get; set; }

        //Start #90940 ios :FR Batch number on cash register By Pratik
        public string refNumber { get; set; }
        //End #90940 By Pratik

        public decimal? TotalCompletedSales { get; set; } //Start #92768 By Pratik
        public decimal? TotalParkedSales { get; set; } //Start #92768 By Pratik
        public decimal? TotalLayBySales { get; set; } //Start #92768 By Pratik
    }

}
