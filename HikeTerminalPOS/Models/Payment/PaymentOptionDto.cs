using System.Collections.ObjectModel;
using System.ComponentModel;
using HikePOS.Enums;
using HikePOS.Resources;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public class PaymentOptionDto : FullAuditedPassiveEntityDto
    {
        public PaymentOptionDto()
        {
            RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>();
        }

        [JsonIgnore]
        string _Name { get; set; }
        public string Name { get { return _Name; } set { _Name = value; SetPropertyChanged(nameof(Name)); } }

        [JsonIgnore]
        string _DisplayName { get; set; }
        public string DisplayName { get { return _DisplayName; } set { _DisplayName = value; SetPropertyChanged(nameof(DisplayName)); } }

        [JsonIgnore]
        bool isEnable { get; set; } = true;
        public bool IsEnable { get { return isEnable; } set { isEnable = value; SetPropertyChanged(nameof(IsEnable)); } }



        public PaymentOptionType PaymentOptionType { get; set; }

        [JsonIgnore]
        string _PaymentOptionName { get; set; }
        public string PaymentOptionName { get { return _PaymentOptionName; } set { _PaymentOptionName = value; SetPropertyChanged(nameof(PaymentOptionName)); } }

        public bool IsDefault { get; set; }
        public bool CanBeConfigered { get; set; }

        [JsonIgnore]
        bool _IsConfigured { get; set; }
        public bool IsConfigered
        {
            get { return _IsConfigured; }
            set
            {
                _IsConfigured = value;
                SetPropertyChanged(nameof(IsConfigered));
            }
        }

        public bool IsCustomerAccount { get; set; }

        public bool IsDeleted { get; set; }

        //Use for only design purpose
        [JsonIgnore]
        public bool IsNotLast { get; set; } = true;

        public ObservableCollection<RegisterPaymentOptionDto> RegisterPaymentOptions { get; set; }

        //Start Ticket #72507 iPad:- Ability to Change Sequence of POS Screen Payment Types By: Pratik
        public int? Sequence { get; set; }
        //End Ticket #72507 By: PratikBy: Pratik
        public string ConfigurationDetails { get; set; }

        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        public decimal? Surcharge { get; set; }
        public bool IsDisplaySurchargeWarningOnSale { get; set; }
        public bool CanApplySurchageOnRefund { get; set; }

        [JsonIgnore]
        string _DisplaySubName { get; set; }
        [JsonIgnore]
        public string DisplaySubName { get { return _DisplaySubName; } set { _DisplaySubName = value; SetPropertyChanged(nameof(DisplaySubName)); } }

        [JsonIgnore]
        decimal? _displaySurcharge { get; set; }
        [JsonIgnore]
        public decimal? DisplaySurcharge { get { return _displaySurcharge; } set { _displaySurcharge = value; SetPropertyChanged(nameof(DisplaySurcharge)); } }
        //End Ticket #73190 by Pratik

        //Start Ticket #90942 iOS:FR Cheque number for sale By: Pratik
        public bool IsRequireCheckIDAtCheckout { get; set; }
        //End Ticket #90942 by Pratik

        public bool IsMoto { get; set; } = false;

        //only for application purpose
        //public string AccessToken { get; set; }
        //public string RefreshUrl { get; set; }

        //public string TerminalId { get; set; }
        //public string AccountId { get; set; }
        //public string AcceptorId { get; set; }

        //public bool ArePartialApprovalsAllowed { get; set; }
        //public bool AreDuplicateTransactionsAllowed { get; set; }
        //public bool IsCashbackAllowed { get; set; }
        //public double CashbackAmount { get; set; }
        //public bool IsDebitAllowed { get; set; }
        //public bool IsEmvAllowed { get; set; }
        //Ticket start:#94673 iOS-FR-Colour on pay screen.by rupesh
        public int? ColorTag { get; set; }
        [JsonIgnore]
        public Color BackgroundColor
        {
            get
            {
                return (ColorTag == null || ColorTag == 0) ? AppColors.NavigationBarBackgroundColor : AppColors.PaymentTagColors[ColorTag.Value - 1];
            }
        }

        public Color EffectiveBackgroundColor =>
            DisplayName == "[MOTO]" ? Colors.Transparent : BackgroundColor;

        [JsonIgnore]
        public Color TextColor
        {
            get
            {
                return (ColorTag == null || ColorTag == 0) ? Colors.White : AppColors.PaymentTextColors[ColorTag.Value - 1];
            }
        }
        [JsonIgnore]
        public Color BorderColor
        {
            get
            {
                if (BackgroundColor == Colors.White)
                    return Colors.Black;
                else
                    return Colors.Transparent;
            }

        }
        //Ticket end:#94673 .by rupesh

        public PaymentOptionDB ToModel()
        {
            PaymentOptionDB model = new PaymentOptionDB
            {
                Id = Id,
                IsActive = IsActive,
                IsNotLast = IsNotLast,
                IsMoto = IsMoto,
                IsConfigered = IsConfigered,
                IsCustomerAccount = IsCustomerAccount,
                IsDefault = IsDefault,
                IsDeleted = IsDeleted,
                IsEnable = IsEnable,
                DisplayName = DisplayName,
                ConfigurationDetails = ConfigurationDetails,
                Name = Name,
                PaymentOptionName = PaymentOptionName,
                CanBeConfigered = CanBeConfigered,
                //Start Ticket #72507 iPad:- Ability to Change Sequence of POS Screen Payment Types By: Pratik
                Sequence = Sequence,
                //End Ticket #72507 By: PratikBy: Pratik
                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                Surcharge = Surcharge,
                IsDisplaySurchargeWarningOnSale = IsDisplaySurchargeWarningOnSale,
                CanApplySurchageOnRefund = CanApplySurchageOnRefund,
                //END Ticket #73190 
                PaymentOptionType = (int)PaymentOptionType,
                //Start Ticket #90942 iOS:FR Cheque number for sale By: Pratik
                IsRequireCheckIDAtCheckout = IsRequireCheckIDAtCheckout,
                //End Ticket #90942 by Pratik
                //Ticket start:#94673 iOS-FR-Colour on pay screen.by rupesh
                ColorTag = ColorTag
                //Ticket end:#94673 .by rupesh


            };

            RegisterPaymentOptions?.ForEach(a => model.RegisterPaymentOptions.Add(a.ToModel()));

            return model;
        }

        public static PaymentOptionDto FromModel(PaymentOptionDB dbModel)
        {
            if (dbModel == null)
                return null;

            PaymentOptionDto model = new PaymentOptionDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                IsNotLast = dbModel.IsNotLast,
                IsMoto = dbModel.IsMoto,
                IsConfigered = dbModel.IsConfigered,
                IsCustomerAccount = dbModel.IsCustomerAccount,
                IsDefault = dbModel.IsDefault,
                IsDeleted = dbModel.IsDeleted,
                IsEnable = dbModel.IsEnable,
                DisplayName = dbModel.DisplayName,
                ConfigurationDetails = dbModel.ConfigurationDetails,
                Name = dbModel.Name,
                CanBeConfigered = dbModel.CanBeConfigered,
                //Start Ticket #72507 iPad:- Ability to Change Sequence of POS Screen Payment Types By: Pratik
                Sequence = dbModel.Sequence,
                //End Ticket #72507 By: PratikBy: Pratik
                //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
                Surcharge = dbModel.Surcharge,
                IsDisplaySurchargeWarningOnSale = dbModel.IsDisplaySurchargeWarningOnSale,
                CanApplySurchageOnRefund = dbModel.CanApplySurchageOnRefund,
                //END Ticket #73190 
                PaymentOptionName = dbModel.PaymentOptionName,
                PaymentOptionType = (PaymentOptionType)dbModel.PaymentOptionType,
                //Start Ticket #90942 iOS:FR Cheque number for sale By: Pratik
                IsRequireCheckIDAtCheckout = dbModel.IsRequireCheckIDAtCheckout,
                //End Ticket #90942 by Pratik
                //Ticket start:#94673 iOS-FR-Colour on pay screen.by rupesh
                ColorTag = dbModel.ColorTag
                //Ticket end:#94673 .by rupesh

            };

            model.RegisterPaymentOptions = new ObservableCollection<RegisterPaymentOptionDto>(dbModel.RegisterPaymentOptions.Select(a => RegisterPaymentOptionDto.FromModel(a)));

            return model;
        }

    }

    public partial class PaymentOptionDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsEnable { get; set; }
        public int PaymentOptionType { get; set; }
        public string PaymentOptionName { get; set; }
        public bool IsDefault { get; set; }
        public bool CanBeConfigered { get; set; }
        public bool IsConfigered { get; set; }
        public bool IsCustomerAccount { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsNotLast { get; set; }
        public IList<RegisterPaymentOptionDB> RegisterPaymentOptions { get; }
        public string ConfigurationDetails { get; set; }
        //Start Ticket #72507 iPad:- Ability to Change Sequence of POS Screen Payment Types By: Pratik
        public int? Sequence { get; set; }
        //End Ticket #72507 By: PratikBy: Pratik
        //Start Ticket #73190 iOS FR :Automatic Surcharge For Card Payment ONLY By: Pratik
        public decimal? Surcharge { get; set; }
        public bool IsDisplaySurchargeWarningOnSale { get; set; }
        public bool CanApplySurchageOnRefund { get; set; }
        //END Ticket #73190 
        public bool IsMoto { get; set; } = false;

        //Start Ticket #90942 iOS:FR Cheque number for sale By: Pratik
        public bool IsRequireCheckIDAtCheckout { get; set; }
        //End Ticket #90942 by Pratik
        //Ticket start:#94673 iOS-FR-Colour on pay screen.by rupesh
        public int? ColorTag { get; set; }
        //Ticket end:#94673 .by rupesh

    }
}
