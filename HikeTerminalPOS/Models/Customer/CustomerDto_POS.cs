using System;
using System.Collections.ObjectModel;
using HikePOS.Enums;
using Newtonsoft.Json;
using Realms;

namespace HikePOS.Models
{
    public class CustomerDto_POS : FullAuditedPassiveEntityDto
    {
        public CustomerDto_POS()
        {
            Address = new AddressDto(); 
        }

        public string KeyId { get; set; }

        public bool IsSync { get; set; } = true;

        [JsonProperty("SyncReference")]
        public string TempId { get; set; }

        [JsonIgnore]
        string _FirstName { get; set; }
        public string FirstName
        {
            get { return _FirstName; }
            set
            {
                _FirstName = value;
                SetPropertyChanged(nameof(FirstName));
                SetPropertyChanged(nameof(FullName));
            }
        }

        [JsonIgnore]
        string _LastName { get; set; }

        public string LastName
        {
            get { return _LastName; }
            set
            {
                _LastName = value;
                SetPropertyChanged(nameof(LastName));
                SetPropertyChanged(nameof(LastName));
            }
        }

        [JsonIgnore]
        string _CompanyName { get; set; }

        public string CompanyName { get { return _CompanyName; } set { _CompanyName = value; SetPropertyChanged(nameof(CompanyName)); } }

        [JsonIgnore]
        public string _phone { get; set; }
        public string Phone { get { return _phone; } set { _phone = value; SetPropertyChanged(nameof(Phone)); } }

        public InvoiceFrom CustomerFrom { get; set; } =  InvoiceFrom.iPad;
        public string CustomerCode { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public bool _allowLoyalty { get; set; }
        public bool AllowLoyalty
        {

            get
            {
                return _allowLoyalty;
            }
            set
            {
                _allowLoyalty = value;
                SetPropertyChanged(nameof(AllowLoyalty));
            }
        }

        //Ticket #10921 Start : New Feature Customer Tax Exemption. By Nikhil
        bool _toAllowForTaxExempt { get; set; }
        public bool ToAllowForTaxExempt
        {

            get
            {
                return _toAllowForTaxExempt;
            }
            set
            {
                _toAllowForTaxExempt = value;
                SetPropertyChanged(nameof(ToAllowForTaxExempt));
            }
        }
        //Ticket #10921 End. By Nikhil


        [JsonIgnore]
        public string FullName
        {
            get
            {
                var str = "";
                if (!string.IsNullOrEmpty(FirstName))
                {
                    str = FirstName.ToUppercaseFirstCharacter();
                }
                if (!string.IsNullOrEmpty(LastName))
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        str = str + " ";
                    }
                    str = str + LastName.ToUppercaseFirstCharacter();
                }
                return str;
            }
        }

        [JsonIgnore]
        public string ContactDetail
        {
            get
            {
                if (!string.IsNullOrEmpty(Phone) && !string.IsNullOrEmpty(Email))
                    return Phone + " / " + Email;
                else if (!string.IsNullOrEmpty(Phone))
                    return Phone;
                else if (!string.IsNullOrEmpty(Email))
                    return Email;
                else
                    return "";
            }
        }

        DateTime? _birthDate { get; set; }
        public DateTime? BirthDate
        {
            get
            {
                //if (_birthDate == null)
                //	return DateTime.Now.Date;
                //else
                return _birthDate;
            }
            set
            {
                _birthDate = value;
                PickerDate = value;
                SetPropertyChanged(nameof(BirthDate));
            }
        }


        DateTime? _pickerDate { get; set; }
        public DateTime? PickerDate
        {
            get
            {
                if (_pickerDate == null)
                    return DateTime.Now.Date;
                else
                    return _pickerDate;
            }
            set
            {
                _pickerDate = value;
                SetPropertyChanged(nameof(PickerDate));
            }
        }

        //Ticket start:#23573 iOS - Gender Options.by rupesh
        public int _gender { get; set; } = 2;
        //Ticket end:#23573 .by rupesh
        public int Gender
        {
            get
            {
                return _gender;
            }
            set
            {
                _gender = value;
                SetPropertyChanged(nameof(Gender));
            }
        }

        [JsonIgnore]
        decimal _currentLoyaltyBalance;
        public decimal CurrentLoyaltyBalance
        {
            get
            {
                return _currentLoyaltyBalance;
            }
            set
            {
                _currentLoyaltyBalance = value;
                SetPropertyChanged(nameof(CurrentLoyaltyBalance));
            }
        }

        [JsonIgnore]
        decimal? _creditBalance;
        public decimal? CreditBalance
        {
            get
            {
                return _creditBalance;
            }
            set
            {
                _creditBalance = value;
                SetPropertyChanged(nameof(CreditBalance));
            }
        }

        [JsonIgnore]
        decimal? _outStandingBalance;
        public decimal? OutStandingBalance
        {
            get
            {
                return _outStandingBalance;
            }
            set
            {
                _outStandingBalance = value;
                SetPropertyChanged(nameof(OutStandingBalance));
            }
        }

        public decimal TotalLoyaltySpent { get; set; }

        [JsonIgnore]
        private decimal? _CreditLimit { get; set; }
        public decimal? CreditLimit
        {
            get
            {
                return _CreditLimit;
            }
            set
            {
                _CreditLimit = value;
                _strCreditLimit = CreditLimit?.ToString("0.##");
            }
        }

        [JsonIgnore]
        public string _strCreditLimit { get; set; }
        public string StrCreditLimit
        {
            get
            {
                return _strCreditLimit;
            }
            set
            {
                _strCreditLimit = value;
                SetPropertyChanged(nameof(StrCreditLimit));
            }
        }

        public int? HowDidYouHearAboutus { get; set; }

        [JsonIgnore]
        AddressDto _address { get; set; }
        public AddressDto Address { get { return _address; } set { _address = value; SetPropertyChanged(nameof(Address)); } }

        public int? CustomerGroupId { get; set; }
        //Ticket #589 CustomerGroupName added by rupesh
        public string CustomerGroupName { get; set; }

        [JsonIgnore]
        bool _isAbletoEdit { get; set; }
        public bool IsAbletoEdit { get { return _isAbletoEdit; } set { _isAbletoEdit = value; SetPropertyChanged(nameof(IsAbletoEdit)); } }

        public DateTime? CreationTime { get; set; }

        public decimal? VisitCurrentMonth { get; set; }
        public decimal? AvgMonthlyVisit { get; set; }

        public decimal? TotalPurchase { get; set; }
        public decimal? AveragePurchase { get; set; }

        public string CustomeField1 { get; set; }
        public string CustomeField2 { get; set; }
        public string CustomeField3 { get; set; }
        public string CustomeField4 { get; set; }


        public bool _toAllowForMarketing { get; set; }
        public bool ToAllowForMarketing
        {

            get
            {
                return _toAllowForMarketing;
            }
            set
            {
                _toAllowForMarketing = value;
                SetPropertyChanged(nameof(ToAllowForMarketing));
            }
        }
        public ObservableCollection<CustomerOutletDto> CustomerOutlets { get; set; }
        //Ticket start:#32389 iPad :: Feature request :: Allow to add multiple email address.by rupesh
        public string SecondaryEmail1 { get; set; }
        public string SecondaryEmail2 { get; set; }
        //Ticket end:#32389 .by rupesh
        //Ticket start:#32371 iPad :: Feature request :: customer custom field not reflecting in print receipt.by rupesh
        public string CustomerTaxId { get; set; }
        //Ticket end:#32371 .by rupesh

        //Ticket start:#54908 iPad: FR: Customer Tag Discount. by rupesh
        //START Ticket #74344 iOS and WEB :: Discount Issue By pratik
        //Start #78059 iOS: Customer Detail not opening in POS screen by Pratik
        //public ObservableCollection<int> APICustomerTags { get; set; }
        [JsonIgnore]
        ObservableCollection<object> _tempCustomerTags;
        public ObservableCollection<object> CustomerTags
        {
            get
            {
                return _tempCustomerTags;
            }
            set
            {
                _tempCustomerTags = value;
                SetPropertyChanged(nameof(CustomerTags));
                if (value != null && value.Count > 0)
                {
                    int val = 0;
                    if (Int32.TryParse(value[0].ToString(), out val))
                        APICustomerTags = new ObservableCollection<int>(value.Select(a => Convert.ToInt32(a)));
                    else
                        APICustomerTags = new ObservableCollection<int>(value.Select(a => JsonConvert.DeserializeObject<CustomerTagsDto>(a.ToJson()).tagId));
                }
                else
                    APICustomerTags = new ObservableCollection<int>();
            }
        }
        [JsonIgnore]
        public ObservableCollection<int> APICustomerTags { get; set; }
        //Start #End iOS: Customer Detail not opening in POS screen by Pratik
        //End Ticket #74344 By pratik
        //Ticket end:#54908 . by rupesh

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public int? InvoicesDueDays { get; set; }
        public InvoicesDueType? InvoicesDueType { get; set; }
        //End ticket #76208 by Pratik

        //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik 
        decimal? _creditLimitIssue;
        public decimal? CreditLimitIssue
        {

            get
            {
                return _creditLimitIssue;
            }
            set
            {
                _creditLimitIssue = value;
                SetPropertyChanged(nameof(CreditLimitIssue));
            }
        }
        decimal? _creditLimitRedeemed;
        public decimal? CreditLimitRedeemed
        {

            get
            {
                return _creditLimitRedeemed;
            }
            set
            {
                _creditLimitRedeemed = value;
                SetPropertyChanged(nameof(CreditLimitRedeemed));
            }
        }
        //End Ticket #74631 by pratik 

        public CustomerDB_POS ToModel()
        {
            CustomerDB_POS customerDB_POS = new CustomerDB_POS
            {
                KeyId = Id > 0 ? Id.ToString() : (KeyId ?? Guid.NewGuid().ToString()),
                Id = Id,
                IsActive = IsActive,
                IsSync = IsSync,
                TempId = TempId,
                FirstName = FirstName,
                LastName = LastName,
                CompanyName = CompanyName,
                Phone = Phone,
                CustomerFrom = (int)CustomerFrom,
                CustomerCode = CustomerCode,
                Email = Email,
                Note = Note,
                AllowLoyalty = AllowLoyalty,
                ToAllowForTaxExempt = ToAllowForTaxExempt,
                BirthDate = BirthDate,
                PickerDate = PickerDate,
                Gender = Gender,
                CurrentLoyaltyBalance = CurrentLoyaltyBalance,
                TotalLoyaltySpent = TotalLoyaltySpent,
                CreditLimit = CreditLimit,
                StrCreditLimit = StrCreditLimit,
                CreditBalance = CreditBalance,
                OutStandingBalance = OutStandingBalance,
                HowDidYouHearAboutus = HowDidYouHearAboutus,
                Address = Address?.ToModel(),
                CustomerGroupId = CustomerGroupId,
                CustomerGroupName = CustomerGroupName,
                IsAbletoEdit = IsAbletoEdit,
                CreationTime = CreationTime,
                VisitCurrentMonth = VisitCurrentMonth,
                AvgMonthlyVisit = AvgMonthlyVisit,
                TotalPurchase = TotalPurchase,
                AveragePurchase = AveragePurchase,
                CustomeField1 = CustomeField1,
                CustomeField2 = CustomeField2,
                CustomeField3 = CustomeField3,
                CustomeField4 = CustomeField4,
                ToAllowForMarketing = ToAllowForMarketing,
                SecondaryEmail1 = SecondaryEmail1,
                SecondaryEmail2 = SecondaryEmail2,
                CustomerTaxId = CustomerTaxId,
                //START ticket #76208 IOS:FR:Terms of payments by Pratik
                InvoicesDueDays = InvoicesDueDays,
                InvoicesDueType = InvoicesDueType.HasValue ? (int)InvoicesDueType : null,
                //End ticket #76208 by Pratik
                //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik 
                CreditLimitIssue = CreditLimitIssue,
                CreditLimitRedeemed = CreditLimitRedeemed
                //End Ticket #74631 by pratik 

    };
            CustomerOutlets?.ForEach(i => customerDB_POS.CustomerOutlets.Add(new CustomerOutletDB { OutletId = i.OutletId}));
            //START Ticket #74344 / #78059 iOS and WEB :: Discount Issue By pratik
            APICustomerTags?.ForEach(i => customerDB_POS.APICustomerTags.Add(i));
            //End Ticket #74344 / #78059  By pratik
            return customerDB_POS;
        }

        public static CustomerDto_POS FromModel(CustomerDB_POS customerDB_POS)
        {
            if (customerDB_POS == null)
                return null;

            CustomerDto_POS customerDto_POS = new CustomerDto_POS
            {
                KeyId = customerDB_POS.KeyId,
                Id = customerDB_POS.Id,
                IsActive = customerDB_POS.IsActive,
                IsSync = customerDB_POS.IsSync,
                TempId = customerDB_POS.TempId,
                FirstName = customerDB_POS.FirstName,
                LastName = customerDB_POS.LastName,
                CompanyName = customerDB_POS.CompanyName,
                Phone = customerDB_POS.Phone,
                CustomerFrom = (InvoiceFrom) customerDB_POS.CustomerFrom,
                CustomerCode = customerDB_POS.CustomerCode,
                Email = customerDB_POS.Email,
                Note = customerDB_POS.Note,
                AllowLoyalty = customerDB_POS.AllowLoyalty,
                ToAllowForTaxExempt = customerDB_POS.ToAllowForTaxExempt,
                BirthDate = customerDB_POS.BirthDate?.UtcDateTime,
                PickerDate = customerDB_POS.PickerDate?.UtcDateTime,
                Gender = customerDB_POS.Gender,
                CurrentLoyaltyBalance = customerDB_POS.CurrentLoyaltyBalance,
                TotalLoyaltySpent = customerDB_POS.TotalLoyaltySpent,
                CreditLimit = customerDB_POS.CreditLimit,
                StrCreditLimit = customerDB_POS.StrCreditLimit,
                CreditBalance = customerDB_POS.CreditBalance,
                OutStandingBalance = customerDB_POS.OutStandingBalance,
                HowDidYouHearAboutus = customerDB_POS.HowDidYouHearAboutus,
                Address = AddressDto.FromModel(customerDB_POS.Address),
                CustomerGroupId = customerDB_POS.CustomerGroupId,
                CustomerGroupName = customerDB_POS.CustomerGroupName,
                IsAbletoEdit = customerDB_POS.IsAbletoEdit,
                CreationTime = customerDB_POS.CreationTime?.UtcDateTime,
                VisitCurrentMonth = customerDB_POS.VisitCurrentMonth,
                AvgMonthlyVisit = customerDB_POS.AvgMonthlyVisit,
                TotalPurchase = customerDB_POS.TotalPurchase,
                AveragePurchase = customerDB_POS.AveragePurchase,
                CustomeField1 = customerDB_POS.CustomeField1,
                CustomeField2 = customerDB_POS.CustomeField2,
                CustomeField3 = customerDB_POS.CustomeField3,
                CustomeField4 = customerDB_POS.CustomeField4,
                ToAllowForMarketing = customerDB_POS.ToAllowForMarketing,
                SecondaryEmail1 = customerDB_POS.SecondaryEmail1,
                SecondaryEmail2 = customerDB_POS.SecondaryEmail2,
                CustomerTaxId = customerDB_POS.CustomerTaxId,
                //START ticket #76208 IOS:FR:Terms of payments by Pratik
                InvoicesDueDays = customerDB_POS.InvoicesDueDays,
                InvoicesDueType = customerDB_POS.InvoicesDueType.HasValue ? (InvoicesDueType)customerDB_POS.InvoicesDueType : null,
                //End ticket #76208 by Pratik
                //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik 
                CreditLimitIssue = customerDB_POS.CreditLimitIssue,
                CreditLimitRedeemed = customerDB_POS.CreditLimitRedeemed
                //End Ticket #74631 by pratik 

            };
            if (customerDB_POS.CustomerOutlets != null)
                customerDto_POS.CustomerOutlets = new ObservableCollection<CustomerOutletDto>(customerDB_POS.CustomerOutlets.Select(a => new CustomerOutletDto { OutletId = a.OutletId }));
            //START Ticket #74344 / #78059 iOS and WEB :: Discount Issue By pratik
            if (customerDB_POS.APICustomerTags != null)
            {
                customerDto_POS.APICustomerTags = new ObservableCollection<int>(customerDB_POS.APICustomerTags);
                customerDto_POS.CustomerTags = new ObservableCollection<object>(customerDB_POS.APICustomerTags.Select(a=>(object)a));
            }
            //End Ticket #74344 / #78059 pratik
            return customerDto_POS;

        }
    }

    public class CustomerOutletDto
    {
        public int OutletId { get; set; }
    }

    public partial class CustomerDB_POS : IRealmObject
    {
        [PrimaryKey]
        public string KeyId { get; set; }

        public int Id { get; set; }
        public bool IsActive { get; set; }

        public bool IsSync { get; set; } = true;

        public string TempId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CompanyName { get; set; }

        public string Phone { get; set; }

        public int CustomerFrom { get; set; } = 1;
        public string CustomerCode { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public bool AllowLoyalty { get; set; }
        public bool ToAllowForTaxExempt { get; set; }
        public DateTimeOffset? BirthDate { get; set; }

        public DateTimeOffset? PickerDate { get; set; }
        
        public int Gender { get; set; }

        public decimal CurrentLoyaltyBalance { get; set; }
        public decimal TotalLoyaltySpent { get; set; }

        public decimal? CreditLimit { get; set; }

        public string StrCreditLimit { get; set; }


        public decimal? CreditBalance { get; set; }
        public decimal? OutStandingBalance { get; set; }

        public int? HowDidYouHearAboutus { get; set; }

        public AddressDB Address { get; set; }
        public int? CustomerGroupId { get; set; }
        public string CustomerGroupName { get; set; }

        public bool IsAbletoEdit { get; set; }

        public DateTimeOffset? CreationTime { get; set; }

        public decimal? VisitCurrentMonth { get; set; }
        public decimal? AvgMonthlyVisit { get; set; }

        public decimal? TotalPurchase { get; set; }
        public decimal? AveragePurchase { get; set; }

        public string CustomeField1 { get; set; }
        public string CustomeField2 { get; set; }
        public string CustomeField3 { get; set; }
        public string CustomeField4 { get; set; }


        public bool ToAllowForMarketing { get; set; }
        public IList<CustomerOutletDB> CustomerOutlets {get;}
        public string SecondaryEmail1 { get; set; }
        public string SecondaryEmail2 { get; set; }
        public string CustomerTaxId { get; set; }

        //START ticket #76208 IOS:FR:Terms of payments by Pratik
        public int? InvoicesDueDays { get; set; }
        public int? InvoicesDueType { get; set; }
        //End ticket #76208 by Pratik

        //Start Ticket #74631 iOS: Credit Note Receipt (FR) by pratik 
        public decimal? CreditLimitIssue { get; set; }
        public decimal? CreditLimitRedeemed { get; set; }
        //End Ticket #74631 by pratik

        //START Ticket #74344 / #78059 iOS and WEB :: Discount Issue By pratik
        public IList<int> APICustomerTags { get;}
        //End Ticket #74344/ #78059  By pratik

    }

    public partial class CustomerOutletDB : IRealmObject
    {
        public int OutletId { get; set; }
    }

    //Start #78059 iOS: Customer Detail not opening in POS screen by Pratik
    public class CustomerTagsDto
    {
        public int customerId { get; set; }
        public int tagId { get; set; }
        public string name { get; set; }
        public string customerName { get; set; }
        public object customerCode { get; set; }
        public object email { get; set; }
    }
    //End #78059 by Pratik


}
