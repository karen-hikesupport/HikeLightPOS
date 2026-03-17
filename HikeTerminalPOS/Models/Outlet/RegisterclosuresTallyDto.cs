using System;
using System.Collections.ObjectModel;
using HikePOS.Enums;
using Realms;
using Newtonsoft.Json;
using HikePOS.Helpers;

namespace HikePOS.Models
{
	public class RegisterclosuresTallyDto : FullAuditedPassiveEntityDto
	{
		public int RegisterCloserId { get; set; }
		public int PaymentOptionId { get; set; }
        public PaymentOptionType paymentOptionType { get; set; }

		public string PaymentOptionName { get; set; }

        //#29818 Net cash payment amount in Register summary

        [JsonIgnore]
        public string NetPaymentName { get; set; }
        public bool IsNetAmountDisplay { get; set; } = false;
        //#29818 Net cash payment amount in Register summary

        public decimal DifferenceTotal
        {
            get
            {
                // Start #91261 iOS:FR counted amount upon closing by Pratik
                if (Settings.GrantedPermissionNames != null && !Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.POS.CloseRegister.ShowExpectedColumn") && RegisteredTotal <= 0)
                    return 0;
                else
                    return RegisteredTotal - ActualTotal;
                //End #91261 by Pratik
            }  
		}

		decimal _RegisteredTotal { get; set; }
		public decimal RegisteredTotal { get { return _RegisteredTotal;} set {

				_RegisteredTotal = value;
                //StrRegisteredTotal = value.ToString();
				SetPropertyChanged(nameof(DifferenceTotal));
				SetPropertyChanged(nameof(RegisteredTotal));
			} }

        [JsonIgnore]
        string _strRegisteredTotal { get; set; } = "0";

        [JsonIgnore]
		public string StrRegisteredTotal
		{
            get { return _strRegisteredTotal; }
			set
			{
				//try
				//{
				//	if (!string.IsNullOrEmpty(value))
				//	{
				//		RegisteredTotal = decimal.Parse(value);
				//	}
				//	else
				//		RegisteredTotal = 0;
				//}
				//catch (Exception ex)
				//{
    //                ex.Track();
				//	RegisteredTotal = 0;
				//}
               
                _strRegisteredTotal = value;
                
                SetPropertyChanged(nameof(StrRegisteredTotal));
			}
		}

        [JsonIgnore]
        decimal _cashCalculatorTotal { get; set; } = 0;
        [JsonIgnore]
        public decimal CashCalculatorTotal
        {
            get
            {
                return _cashCalculatorTotal;
            }
            set
            {
                _cashCalculatorTotal = value;
                SetPropertyChanged(nameof(CashCalculatorTotal));
            }

        }


		decimal _ActualTotal { get; set; }
		public decimal ActualTotal
		{
			get { return _ActualTotal; }
			set
			{
				_ActualTotal = value;
				RegisteredTotal = value;
				SetPropertyChanged(nameof(ActualTotal));
				SetPropertyChanged(nameof(RegisteredTotal));
			}
		}
        ObservableCollection<RegisterClosureTallyDenominationDto> _registerClosureTallyDenominations { get; set; }
        public ObservableCollection<RegisterClosureTallyDenominationDto> RegisterClosureTallyDenominations 
        { 
            get
            {
                return _registerClosureTallyDenominations;
            }
            set
            {
                _registerClosureTallyDenominations = value;
                SetPropertyChanged(nameof(RegisterClosureTallyDenominations)); 
            }
        }

        public RegisterclosuresTallyDB ToModel()
        {
            RegisterclosuresTallyDB model = new RegisterclosuresTallyDB
            {
                Id = Id,
                IsActive = IsActive,
                RegisterCloserId = RegisterCloserId,
                PaymentOptionId = PaymentOptionId,
                paymentOptionType = (int)paymentOptionType,
                PaymentOptionName = PaymentOptionName,
                NetPaymentName = NetPaymentName,
                IsNetAmountDisplay = IsNetAmountDisplay,
                RegisteredTotal = RegisteredTotal,
                StrRegisteredTotal = StrRegisteredTotal,
                CashCalculatorTotal = CashCalculatorTotal,
                ActualTotal = ActualTotal,
            };
            RegisterClosureTallyDenominations?.ForEach(i => model.RegisterClosureTallyDenominations.Add(i.ToModel()));
            return model;
        }

        public static RegisterclosuresTallyDto FromModel(RegisterclosuresTallyDB dbModel)
        {
            if (dbModel == null)
                return null;

            RegisterclosuresTallyDto model = new RegisterclosuresTallyDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                RegisterCloserId = dbModel.RegisterCloserId,
                PaymentOptionId = dbModel.PaymentOptionId,
                paymentOptionType = (PaymentOptionType)dbModel.paymentOptionType,
                PaymentOptionName = dbModel.PaymentOptionName,
                NetPaymentName = dbModel.NetPaymentName,
                IsNetAmountDisplay = dbModel.IsNetAmountDisplay,
                RegisteredTotal = dbModel.RegisteredTotal,
                StrRegisteredTotal = dbModel.StrRegisteredTotal,
                CashCalculatorTotal = dbModel.CashCalculatorTotal,
                ActualTotal = dbModel.ActualTotal,
            };
            model.RegisterClosureTallyDenominations = new ObservableCollection<RegisterClosureTallyDenominationDto>(dbModel.RegisterClosureTallyDenominations.Select(a => RegisterClosureTallyDenominationDto.FromModel(a))?.ToList());
            return model;
        }
    }

    public partial class RegisterclosuresTallyDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int RegisterCloserId { get; set; }
        public int PaymentOptionId { get; set; }
        public int paymentOptionType { get; set; }
        public string PaymentOptionName { get; set; }
        public string NetPaymentName { get; set; }
        public bool IsNetAmountDisplay { get; set; } 
        public decimal RegisteredTotal { get; set; }
        public string StrRegisteredTotal { get; set; }
        public decimal CashCalculatorTotal { get; set; }
        public decimal ActualTotal { get; set; }
        public IList<RegisterClosureTallyDenominationDB> RegisterClosureTallyDenominations { get; }
    }

    public class RegisterClosureTallyDenominationDto : FullAuditedPassiveEntityDto
    {
        public int RegisterclosuresTallyId { get; set; }

        public int DenominationId { get; set; }

        public decimal DenominationValue { get; set; }

        int _quantity { get; set; }
        public int Quantity { 
            get
            {
                return _quantity;
            } 
            set
            {
                _quantity = value;
                Total = DenominationValue * value;
                SetPropertyChanged(nameof(Quantity));
            } 
        }

        decimal _total { get; set; }
        public decimal Total 
        { 
            get
            {
                return _total;
            } 
            set
            {
                _total = value;
                SetPropertyChanged(nameof(Total));
            }
        }

        public RegisterClosureTallyDenominationDB ToModel()
        {
            RegisterClosureTallyDenominationDB model = new RegisterClosureTallyDenominationDB
            {
                Id = Id,
                IsActive = IsActive,
                RegisterclosuresTallyId = RegisterclosuresTallyId,
                DenominationId = DenominationId,
                DenominationValue = DenominationValue,
                Quantity = Quantity,
                Total = Total
            };

            return model;
        }

        public static RegisterClosureTallyDenominationDto FromModel(RegisterClosureTallyDenominationDB dbModel)
        {
            if (dbModel == null)
                return null;

            RegisterClosureTallyDenominationDto model = new RegisterClosureTallyDenominationDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                RegisterclosuresTallyId = dbModel.RegisterclosuresTallyId,
                DenominationId = dbModel.DenominationId,
                DenominationValue = dbModel.DenominationValue,
                Quantity = dbModel.Quantity,
                Total = dbModel.Total

            };

            return model;
        }
    }


    public partial class RegisterClosureTallyDenominationDB : IRealmObject
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int RegisterclosuresTallyId { get; set; }
        public int DenominationId { get; set; }
        public decimal DenominationValue { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}
