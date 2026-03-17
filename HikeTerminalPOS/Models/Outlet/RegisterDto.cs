using System;
using System.Collections.ObjectModel;
using HikePOS.Models.Enum;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public class RegisterDto : FullAuditedPassiveEntityDto
    {
        public RegisterDto()
        {
            Registerclosure = new RegisterclosureDto();
            LastRegisterclosure = new RegisterclosureDto(); 
        }


        public CustomerDisplayConfigureType CustomerDisplayConfigureType { get; set; }
        public string CustomerDisplayConfigurePin { get; set; }

        public string Name { get; set; }
        public int OutletID { get; set; }
        public string OutletName { get; set; }
        public int ReceiptNumber { get; set; }
        public DateTime? LastCloseDateTime { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public bool EmailReceipt { get; set; }
        public bool PrintReceipt { get; set; }
        public string StripeSubscriptionID { get; set; }

        public bool IsOpened { get; set; } = false;

        //Ticket #10280 Start : Cash register showing wrongly open issue. By Nikhil
        public bool IsRegisterOpen { get { return IsOpened && (Registerclosure != null && Registerclosure.StartDateTime != null); } }
        //Ticket #10280 End : By Nikhil

        public int? DefaultTax { get; set; }
        public string DefaultTaxName { get; set; }
        public decimal? DefaultTaxRate { get; set; }

        public RegisterclosureDto Registerclosure { get; set; }
        public RegisterclosureDto LastRegisterclosure { get; set; }
        public int? ReceiptTemplateId { get; set; }

        [JsonIgnore]
        public ReceiptTemplateDto ReceiptTemplate
        {
            get
            {
                return ReceiptTemplateId.GetReceiptTemplate();
            }
        }

        public int? GiftReceiptTemplateId { get; set; }

        [JsonIgnore]
        public ReceiptTemplateDto GiftReceiptTemplate
        {
            get
            {
                //return _giftReceiptTemplate;
                return  GiftReceiptTemplateId.HasValue ? GiftReceiptTemplateId.GetReceiptTemplate() : ReceiptTemplateId.GetReceiptTemplate();
            }
        }

        public int? DocketReceiptTemplateId { get; set; }
       
        [JsonIgnore]
        public ReceiptTemplateDto DocketReceiptTemplate
        {
            get
            {
                return  DocketReceiptTemplateId.HasValue ? DocketReceiptTemplateId.GetReceiptTemplate() : ReceiptTemplateId.GetReceiptTemplate();
            }
        }

        public int? ParkOrderReceiptTemplateId { get; set; }

        [JsonIgnore]
        public ReceiptTemplateDto ParkOrderReceiptTemplate
        {
            get
            {
                  return  ParkOrderReceiptTemplateId.HasValue ? ParkOrderReceiptTemplateId.GetReceiptTemplate() : ReceiptTemplateId.GetReceiptTemplate();
            }
        }

        public int? LayByReceiptTemplateId { get; set; }

        [JsonIgnore]
        public ReceiptTemplateDto LayByReceiptTemplate
        {
            get
            {
                return  LayByReceiptTemplateId.HasValue ? LayByReceiptTemplateId.GetReceiptTemplate() : ReceiptTemplateId.GetReceiptTemplate();
            }
        }

        public int QuoteReceiptNumber { get; set; }
        public string QuotePrefix { get; set; }
        public string QuoteSuffix { get; set; }
        public int? QuoteReceiptTemplateId { get; set; }

        [JsonIgnore]
        public ReceiptTemplateDto QuoteReceiptTemplate
        {
            get
            {
                 return  QuoteReceiptTemplateId.HasValue ? QuoteReceiptTemplateId.GetReceiptTemplate() : ReceiptTemplateId.GetReceiptTemplate();
            }
        }
        //Ticket start:#32377 iPad :: Feature request :: Add Receipt Template for On-account Sales.by rupesh
        public int? OnAccountReceiptTemplateId { get; set; }

        [JsonIgnore]
        public ReceiptTemplateDto OnAccountReceiptTemplate
        {
            get
            {
                return  OnAccountReceiptTemplateId.HasValue ? OnAccountReceiptTemplateId.GetReceiptTemplate() : ReceiptTemplateId.GetReceiptTemplate();
            }
        }
        //Ticket end:#32377 .by rupesh

        //Ticket start:#92763 iOS:FR Backorder Receipts.by rupesh
        public int? BackorderReceiptTemplateId { get; set; }

        [JsonIgnore]
        public ReceiptTemplateDto BackOrderReceiptTemplate
        {
            get
            {
                 return  BackorderReceiptTemplateId.HasValue ? BackorderReceiptTemplateId.GetReceiptTemplate() : ReceiptTemplateId.GetReceiptTemplate();
            }
        }
        //Ticket end:#92763.by rupesh
        
        public bool IsActiveCashPayment { get; set; } //Start #94427 Disable cash option on a single register By Pratik

        public RegisterDB ToModel()
        {
            RegisterDB model = new RegisterDB
            {
                Id = Id,
                IsActive = IsActive,
                CustomerDisplayConfigureType = (int)CustomerDisplayConfigureType,
                CustomerDisplayConfigurePin = CustomerDisplayConfigurePin,
                Name = Name,
                OutletID = OutletID,
                OutletName = OutletName,
                ReceiptNumber = ReceiptNumber,
                LastCloseDateTime = LastCloseDateTime,
                Prefix = Prefix,
                Suffix = Suffix,
                EmailReceipt = EmailReceipt,
                PrintReceipt = PrintReceipt,
                StripeSubscriptionID = StripeSubscriptionID,
                IsOpened = IsOpened,
                DefaultTax = DefaultTax,
                DefaultTaxName = DefaultTaxName,
                DefaultTaxRate = DefaultTaxRate,
                Registerclosure = Registerclosure?.ToModel(),
                LastRegisterclosure = LastRegisterclosure?.ToModel(),
                ReceiptTemplateId = ReceiptTemplateId,
                //ReceiptTemplate = ReceiptTemplate?.ToModel(),
                GiftReceiptTemplateId = GiftReceiptTemplateId,
                //GiftReceiptTemplate = GiftReceiptTemplate?.ToModel(),
                DocketReceiptTemplateId = DocketReceiptTemplateId,
                //DocketReceiptTemplate = DocketReceiptTemplate?.ToModel(),
                ParkOrderReceiptTemplateId = ParkOrderReceiptTemplateId,
                //ParkOrderReceiptTemplate = ParkOrderReceiptTemplate?.ToModel(),
                LayByReceiptTemplateId = LayByReceiptTemplateId,
                //LayByReceiptTemplate = LayByReceiptTemplate?.ToModel(),
                QuoteReceiptNumber = QuoteReceiptNumber,
                QuotePrefix = QuotePrefix,
                QuoteSuffix = QuoteSuffix,
                QuoteReceiptTemplateId = QuoteReceiptTemplateId,
                //QuoteReceiptTemplate = QuoteReceiptTemplate?.ToModel(),
                OnAccountReceiptTemplateId = OnAccountReceiptTemplateId,
                //OnAccountReceiptTemplate = OnAccountReceiptTemplate?.ToModel()
                BackorderReceiptTemplateId = BackorderReceiptTemplateId,
                IsActiveCashPayment = IsActiveCashPayment //Start #94427 Disable cash option on a single register By Pratik
            };

            return model;
        }

        public static RegisterDto FromModel(RegisterDB dbModel)
        {
            if (dbModel == null)
                return null;

            RegisterDto model = new RegisterDto
            {
                Id = dbModel.Id,
                IsActive = dbModel.IsActive,
                CustomerDisplayConfigureType = (CustomerDisplayConfigureType)dbModel.CustomerDisplayConfigureType,
                CustomerDisplayConfigurePin = dbModel.CustomerDisplayConfigurePin,
                Name = dbModel.Name,
                OutletID = dbModel.OutletID,
                OutletName = dbModel.OutletName,
                ReceiptNumber = dbModel.ReceiptNumber,
                LastCloseDateTime = dbModel.LastCloseDateTime?.UtcDateTime,
                Prefix = dbModel.Prefix,
                Suffix = dbModel.Suffix,
                EmailReceipt = dbModel.EmailReceipt,
                PrintReceipt = dbModel.PrintReceipt,
                StripeSubscriptionID = dbModel.StripeSubscriptionID,
                IsOpened = dbModel.IsOpened,
                DefaultTax = dbModel.DefaultTax,
                DefaultTaxName = dbModel.DefaultTaxName,
                DefaultTaxRate = dbModel.DefaultTaxRate,
                Registerclosure = RegisterclosureDto.FromModel(dbModel.Registerclosure),
                LastRegisterclosure = RegisterclosureDto.FromModel(dbModel.LastRegisterclosure),
                ReceiptTemplateId = dbModel.ReceiptTemplateId,
                //ReceiptTemplate = ReceiptTemplateDto.FromModel(dbModel.ReceiptTemplate),
                GiftReceiptTemplateId = dbModel.GiftReceiptTemplateId,
                //GiftReceiptTemplate = ReceiptTemplateDto.FromModel(dbModel.GiftReceiptTemplate),
                DocketReceiptTemplateId = dbModel.DocketReceiptTemplateId,
                //DocketReceiptTemplate = ReceiptTemplateDto.FromModel(dbModel.DocketReceiptTemplate),
                ParkOrderReceiptTemplateId = dbModel.ParkOrderReceiptTemplateId,
                //ParkOrderReceiptTemplate = ReceiptTemplateDto.FromModel(dbModel.ParkOrderReceiptTemplate),
                LayByReceiptTemplateId = dbModel.LayByReceiptTemplateId,
                //LayByReceiptTemplate = ReceiptTemplateDto.FromModel(dbModel.LayByReceiptTemplate),
                QuoteReceiptNumber = dbModel.QuoteReceiptNumber,
                QuotePrefix = dbModel.QuotePrefix,
                QuoteSuffix = dbModel.QuoteSuffix,
                QuoteReceiptTemplateId = dbModel.QuoteReceiptTemplateId,
                //QuoteReceiptTemplate = ReceiptTemplateDto.FromModel(dbModel.QuoteReceiptTemplate),
                OnAccountReceiptTemplateId = dbModel.OnAccountReceiptTemplateId,
                //OnAccountReceiptTemplate = ReceiptTemplateDto.FromModel(dbModel.OnAccountReceiptTemplate)
                BackorderReceiptTemplateId = dbModel.BackorderReceiptTemplateId,
                IsActiveCashPayment = dbModel.IsActiveCashPayment //Start #94427 Disable cash option on a single register By Pratik

            };

            return model;
        }

    }

    public partial class RegisterDB : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int CustomerDisplayConfigureType { get; set; }
        public string CustomerDisplayConfigurePin { get; set; }

        public string Name { get; set; }
        public int OutletID { get; set; }
        public string OutletName { get; set; }
        public int ReceiptNumber { get; set; }
        public DateTimeOffset? LastCloseDateTime { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public bool EmailReceipt { get; set; }
        public bool PrintReceipt { get; set; }
        public string StripeSubscriptionID { get; set; }
        public bool IsOpened { get; set; }
        public int? DefaultTax { get; set; }
        public string DefaultTaxName { get; set; }
        public decimal? DefaultTaxRate { get; set; }
        public RegisterclosureDB Registerclosure { get; set; }
        public RegisterclosureDB LastRegisterclosure { get; set; }
        public int? ReceiptTemplateId { get; set; }
        //public ReceiptTemplateDB ReceiptTemplate { get; set; }
        public int? GiftReceiptTemplateId { get; set; }
        //public ReceiptTemplateDB GiftReceiptTemplate { get; set; }
        public int? DocketReceiptTemplateId { get; set; }
        //public ReceiptTemplateDB DocketReceiptTemplate { get; set; }
        public int? ParkOrderReceiptTemplateId { get; set; }
        //public ReceiptTemplateDB ParkOrderReceiptTemplate { get; set; }
        public int? LayByReceiptTemplateId { get; set; }
        //public ReceiptTemplateDB LayByReceiptTemplate { get; set; }

        public int QuoteReceiptNumber { get; set; }
        public string QuotePrefix { get; set; }
        public string QuoteSuffix { get; set; }
        public int? QuoteReceiptTemplateId { get; set; }
        //public ReceiptTemplateDB QuoteReceiptTemplate { get; set; }
        public int? OnAccountReceiptTemplateId { get; set; }
        //public ReceiptTemplateDB OnAccountReceiptTemplate { get; set; }
        public int? BackorderReceiptTemplateId { get; set; }

         public bool IsActiveCashPayment { get; set; } //Start #94427 Disable cash option on a single register By Pratik

    }


    public class OpenRegisterInput
    {
        public int registerId { get; set; }
        public double amount { get; set; }
        public string note { get; set; }
        public ObservableCollection<RegisterClosureTallyDenominationDto> registerClosureTallyDenominations { get; set; }
    }

    public class SignalRRegisterResponseModel
    {
        public int RegisterId { get; set; }
        public int OutletId { get; set; }
        public bool IsOpen { get; set; }
        public bool IsClose { get; set; }
        public RegisterDto RegisterResult { get; set; }
    }
}
